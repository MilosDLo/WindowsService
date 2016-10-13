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

namespace TestWinService
{
    public partial class Service1 : ServiceBase
    {

        string connString = @"Data Source=c:\mydb.sqlite;Version=3";
        JsonClass jc;


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
        string jsonString = @"{
                'Name' : 'Milos',
                'Adress' : 'Bg'                        
            }";
            


        protected override void OnStart(string[] args)
        {
            
            eventLog1.WriteEntry("Servis pokrenut");

     
            Thread t1 = new Thread(WaitingForJson);
            t1.Name = "ThreadWaitingForJson";
            t1.IsBackground = true;
            t1.Start();               

            eventLog1.WriteEntry("Kraj main thread-a");
        }


        private void WaitingForJson()
        {
            while (true)
            {
                #region ako se dobije url sa json-om
                var jsonUrl = "http://echo.jsontest.com/Config/b-12-4/Desc/Iron/idjson/1212";
                string jsonStringUrl = "";
                using (WebClient wc = new WebClient())
                {
                    jsonStringUrl = wc.DownloadString(jsonUrl);
                }
                /*
                 //ili asihrono ako je .NET 4.5 ili veci
                 using (var httpClient = new HttpClient()){
                  var json = await httpClient.GetStringAsync("url");
                  }
                  */
                #endregion


                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();


                    #region kreiranje baze i tabele
                    //     using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [JsonTable] (id INTEGER PRIMARY KEY AUTOINCREMENT,idjson INTEGER,desc TEXT,config TEXT)", conn))
                    //     {
                    //         int i = command.ExecuteNonQuery();
                    //         eventLog1.WriteEntry(i.ToString());  
                    //     }
                    //
                    //     using (SQLiteCommand command = new SQLiteCommand("INSERT INTO JsonTable (idjson,desc,config) values (112,'proba','nestonesto') ", conn))
                    //     {
                    //         int i = command.ExecuteNonQuery();
                    //         eventLog1.WriteEntry(i.ToString());
                    //     }
                    #endregion

                    jc = JsonConvert.DeserializeObject<JsonClass>(jsonStringUrl);


                    using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM JsonTable ORDER BY id DESC LIMIT 1", conn))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (jc.IDJson == Int32.Parse(reader[1].ToString()))
                                {
                                    eventLog1.WriteEntry("Isti jsoni");
                                }
                                else
                                {
                                    string commandText = String.Format("INSERT INTO JsonTable (idjson,desc,config) values ({0},'{1}','{2}')", jc.IDJson, jc.Desc, jc.Config);
                     
                                    using (SQLiteCommand commandJsonWrite = new SQLiteCommand(commandText, conn))
                                    {
                                        commandJsonWrite.ExecuteNonQuery();
                                        eventLog1.WriteEntry("Ubacio u bazu");
                                    }
                                }
                            }
                        }
                    }
                    // PrintThread();
                }
                eventLog1.WriteEntry("Proveravamo opet za 60sec.");
                Thread.Sleep(60000);
            }

            //  Thread t1 = new Thread(PrintThread);
            //  t1.Name = "1.";
            //  t1.Start();
            //  //t1.Join();
            //  
            //  Thread t2 = new Thread(PrintThread);
            //  t2.Name = "2.";
            //  t2.Start();


            //fora pokupiti body sa urla
            //   var task = MakeRequest();
            //   task.Wait();
            //
            //   var response = task.Result;
            //   var body = response.Content.ReadAsStringAsync().Result;
            //   eventLog1.WriteEntry(body);
        }


        // private static async Task<HttpResponseMessage> MakeRequest()
        // {
        //     var httpClient = new HttpClient();
        //     return await httpClient.GetAsync(new Uri("http://requestb.in/1f19ydi1"));
        // }

        private void PrintThread()
        {
            var message = "Ispisujem iz "  +Thread.CurrentThread.Name+ " thread-a: " + jc.IDJson +", " +jc.Desc+", "+jc.Config;
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
