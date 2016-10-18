using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Net.Http;
using System.IO;

namespace TestWinService
{
    public partial class Service1 : ServiceBase
    {

        static string connString = @"Data Source=c:\mydb.sqlite;Version=3";
        JsonClass jc;
        MSMQ queue;

        
        public Service1()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
            jc = null;


        }
        ///
        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Servis pokrenut");

            queue = new MSMQ();

            //test
           // for (int i = 0; i < 3; i++)
           // {
           //    bool a = queue.sendErrorMessageToQueue("Esteh es is es");
           // }
          //  string b = queue.receiveErrorMessageFromQueue();
          //  eventLog1.WriteEntry(b);


            Thread t1 = new Thread(WaitingForJson);
            t1.Name = "ThreadWaitingForJson";
            t1.IsBackground = true;
            t1.Start();
           
            Thread t2 = new Thread(SendingMsgFromQueueToServer);
            t2.Name = "ThreadSendingMsgFromQueueToServer";
            t2.IsBackground = true;
            t2.Start();





            eventLog1.WriteEntry("Kraj main thread-a");
        }

        private void SendingMsgFromQueueToServer()
        {
            string errorMsg = null;

            while (true)
            {
                if (errorMsg == null)
                {
                    //meoda sinhrono ceka da se vrati message,ne ide dalje dok ne dobije
                    errorMsg = queue.receiveErrorMessageFromQueue();
                    try
                    {
                        sendingMsgToSever(errorMsg);
                        eventLog1.WriteEntry("Poslata poruka iz queue-a serveru:" + errorMsg);
                        errorMsg = null;
                    }
                    catch (Exception)
                    {

                        Thread.Sleep(60000);
                        continue;    //mislim da je visak?
                    }
                }
                else
                {
                    try
                    {
                        sendingMsgToSever(errorMsg);
                        errorMsg = null;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(60000);
                        continue;   //mislim da je visak?
                    }


                }
            }

        }

        private void sendingMsgToSever(string msgToServer)
        {
            //TO-DO: slanje serveru
           // string urlServer = "";
           // try
           // {
           //
           //     WebRequest wrequest = HttpWebRequest.Create(urlServer);
           //     wrequest.Method = "POST";
           //
           //     //encodovanje poruke u byte
           //     byte[] sendBytes = Encoding.UTF8.GetBytes(msgToServer);
           //
           //     Stream sendingStream = wrequest.GetRequestStream();
           //
           //     sendingStream.Write(sendBytes, 0, sendBytes.Length);
           //     sendingStream.Close();
           //
           // }
           // catch (Exception)
           // {
           //     queue.sendErrorMessageToQueue("Error connecting to server.");
           // }
           //


            eventLog1.WriteEntry(msgToServer+" //Poruka poslata serveru.");



        }


        private void WaitingForJson()
        {
            var jsonUrl = "http://echo.jsontest.com/Config/b-12-4/Desc/Iron/idjson/1212";
            bool isCatch;
            while (true)
            {
                isCatch = false;
                string jsonStringUrl = "";

                //skidanje jsona sa servera/web-a
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        jsonStringUrl = wc.DownloadString(jsonUrl);

                        jc = JsonConvert.DeserializeObject<JsonClass>(jsonStringUrl);
                    }
                    /*
                     //ili asihrono ako je .NET 4.5 ili veci
                     using (var httpClient = new HttpClient()){
                      var jsonStringUrlx = await httpClient.GetStringAsync("url");
                      }
                      */

                }
                catch (Exception)
                {
                    queue.sendErrorMessageToQueue("Ne moze da pristupi serveru na datoj adresi,ili je json drugog formata");
                    isCatch = true;
                }

                if (!isCatch)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connString))
                    {
                        try
                        {
                            conn.Open();
                        }
                        catch (Exception)
                        {
                            queue.sendErrorMessageToQueue("Ne moze da otvori konekciju ka bazi.");
                            Thread.Sleep(60000);
                            continue;
                        }

                        #region kreiranje tabele,ako nema
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [JsonTable] (id INTEGER PRIMARY KEY AUTOINCREMENT,idjson INTEGER,desc TEXT,config TEXT)", conn))
                        {
                            int i = command.ExecuteNonQuery();
                            eventLog1.WriteEntry("Pravljenje tabele:"+i.ToString());
                        }
                        //     samo ubacivanje dump podataka
                        //     using (SQLiteCommand command = new SQLiteCommand("INSERT INTO JsonTable (idjson,desc,config) values (112,'proba','nestonesto') ", conn))
                        //     {
                        //         int i = command.ExecuteNonQuery();
                        //         eventLog1.WriteEntry(i.ToString());
                        //     }
                        #endregion

                        using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM JsonTable ORDER BY id DESC LIMIT 1", conn))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if (jc.IDJson == Int32.Parse(reader[1].ToString()))
                                        {
                                            try
                                            {
                                                sendingMsgToSever("Isti jsoni");
                                            }
                                            catch (Exception)
                                            {
                                                queue.sendErrorMessageToQueue("Error sending msg to server");
                                            }
                                        }
                                        else
                                        {
                                            insertIntoTable(conn, jc);
                                        }
                                    }
                                }
                                else
                                {
                                    insertIntoTable(conn, jc);
                                }

                            }
                        }
                    }
                }

                eventLog1.WriteEntry("Proveravamo opet za 60sec.");      
                Thread.Sleep(60000);       //ili vec na koji interval treba 
            }

            #region fora pokupiti body sa urla         
            //   var task = MakeRequest();
            //   task.Wait();
            //
            //   var response = task.Result;
            //   var body = response.Content.ReadAsStringAsync().Result;
            //   eventLog1.WriteEntry(body);
            #endregion
        }

        private void insertIntoTable(SQLiteConnection conn, JsonClass jclass)
        {
            string commandText = String.Format("INSERT INTO JsonTable (idjson,desc,config) values ({0},'{1}','{2}')", jclass.IDJson, jclass.Desc, jclass.Config);

            using (SQLiteCommand commandJsonWrite = new SQLiteCommand(commandText, conn))
            {
                commandJsonWrite.ExecuteNonQuery();
                eventLog1.WriteEntry("Ubacio u bazu");
            }
        }


        // private static async Task<HttpResponseMessage> MakeRequest()
        // {
        //     var httpClient = new HttpClient();
        //     return await httpClient.GetAsync(new Uri("http://requestb.in/1f19ydi1"));
        // }


        private void PrintThread()
        {
            var message = "Ispisujem iz " + Thread.CurrentThread.Name + " thread-a: " + jc.IDJson + ", " + jc.Desc + ", " + jc.Config;
            for (int i = 0; i < 3; i++)
            {
                eventLog1.WriteEntry(message);
                Thread.Sleep(1000);
            }

        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry(" Servis zaustavljen");
        }


    }
}
