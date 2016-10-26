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
        private List<ListOneOrder> listOfOrders;
        private MSMQ queue;
        
        public Service1()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
            listOfOrders = null;
        }

        ///
        protected override void OnStart(string[] args)
        {
            //eventLog1.WriteEntry("Servis pokrenut");

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

            //  Thread t2 = new Thread(SendingMsgFromQueueToServer);
            //  t2.Name = "ThreadSendingMsgFromQueueToServer";
            //  t2.IsBackground = true;
            //  t2.Start();

            // za testiranje
             //DropTable();
            eventLog1.WriteEntry("Kraj main thread-a");
        }

        private void DropTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Ne moze da otvori konekciju ka bazi.");
                }

                using (SQLiteCommand command = new SQLiteCommand("DROP TABLE IF EXISTS OrderTable", conn))
                {
                    int i = command.ExecuteNonQuery();
                    eventLog1.WriteEntry("Izbrisao:" + i.ToString());
                }
            }         
        }

        private void SendingMsgFromQueueToServer()
        {
            string errorMsg = null;

            while (true)
            {
                if (errorMsg == null)
                {
                    //metoda sinhrono ceka da se vrati message,ne ide dalje dok ne dobije
                    errorMsg = queue.ReceiveErrorMessageFromQueue();
                    try
                    {
                        SendingMsgToSever(errorMsg);
                        eventLog1.WriteEntry("Poslata poruka iz queue-a serveru:" + errorMsg);
                        errorMsg = null;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(60000);
                    }
                }
                else
                {
                    try
                    {
                        SendingMsgToSever(errorMsg);
                        errorMsg = null;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(60000);
                    }
                }
            }

        }

        private void SendingMsgToSever(string msgToServer)
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
            var jsonUrl = "https://mobile.esteh.net/EMExport/orderList";
            bool isCatch;
            while (true)
            {
                isCatch = false;
                string jsonStringFromUrl = @"{""data"":[
                                                        {""id"": ""42877307"",
                                                         ""items"": [
                                                                    {""id"": ""165288485"",""status"": ""1""},
                                                                    {""id"": ""165288488"",""status"": ""1""},
                                                                    {""id"": ""165288491"",""status"": ""1""}],
                                                         ""status"": ""3""},
                                                        {""id"": ""42920957"",
                                                         ""items"": [
                                                                    {""id"": ""165459845"",""status"": ""1""},
                                                                    {""id"": ""165459848"",""status"": ""1""} ],
                                                         ""status"": ""3""}],
                                             ""status"": ""1""} ";

                //skidanje jsona sa servera/web-a,sada ne treba .. ;)
                try
                {
              //      using (WebClient wc = new WebClient())
              //      {
              //          jsonStringFromUrl = wc.DownloadString(jsonUrl);   
              //      }
              
                     listOfOrders = JsonDeserializer.GetListOfOrders(jsonStringFromUrl);
              
                    /*
                     //ili asihrono ako je .NET 4.5 ili veci
                     using (var httpClient = new HttpClient()){
                      var jsonStringUrlx = await httpClient.GetStringAsync("url");
                      }
                      */
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Ne moze da pristupi serveru na datoj adresi,ili je json drugog formata");
                    isCatch = true;
                    // ili continue,ali onda ovde mora thread.sleep(60000)
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
                            queue.SendErrorMessageToQueue("Ne moze da otvori konekciju ka bazi.");
                            Thread.Sleep(60000);
                            continue;    //proverava ponovo json,jer mozda je u medjuvremenu dobio novu verziju
                        }

                        #region kreiranje tabele,ako nema
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [OrderTable] (id INTEGER PRIMARY KEY AUTOINCREMENT,idorderitem TEXT,status TEXT)", conn))
                        {
                            int i = command.ExecuteNonQuery();
                            eventLog1.WriteEntry("Pravljenje tabele:" + i.ToString());
                        }
                        #endregion

                        //     samo ubacivanje dump podataka
                        //     using (SQLiteCommand command = new SQLiteCommand("INSERT INTO OrderTable (idjson,desc,config) values (112,'proba','nestonesto') ", conn))
                        //     {
                        //         int i = command.ExecuteNonQuery();
                        //         eventLog1.WriteEntry(i.ToString());
                        //     }

                        //provera da li je zadnje upisan json kao ovaj novi,ako ne onda upisi u bazu
                        CheckOrderIfExist(conn, listOfOrders);

                    }
                }
                eventLog1.WriteEntry("Proveravamo opet za 60sec.");      
                Thread.Sleep(60000);       //ili vec na koji interval treba 
            }
        }


        private void CheckOrderIfExist(SQLiteConnection conn, List<ListOneOrder> listOfOrders)
        {
            foreach (ListOneOrder order in listOfOrders)
            {
                string commandText = String.Format("SELECT * FROM OrderTable WHERE idorderitem='{0}'", order.ID);
                using (SQLiteCommand command = new SQLiteCommand(commandText, conn))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            try
                            {
                                SendingMsgToSever("Same order exists in table.");
                            }
                            catch (Exception)
                            {
                                queue.SendErrorMessageToQueue("Error sending msg to server");
                            }
                        }
                        else
                        {
                            InsertOrderIntoTable(conn, order);
                        }
                    }
                }
            }           
        }
                   
        private void InsertOrderIntoTable(SQLiteConnection conn, ListOneOrder order)
        {
            string commandText = String.Format("INSERT INTO OrderTable (idorderitem,status) values ('{0}','{1}')", order.ID, order.Status);

            using (SQLiteCommand commandJsonWrite = new SQLiteCommand(commandText, conn))
            {
                commandJsonWrite.ExecuteNonQuery();
                eventLog1.WriteEntry("Ubacio jedan order u bazu sa ID:"+order.ID);
            }
        }

        // private static async Task<HttpResponseMessage> MakeRequest()
        // {
        //     var httpClient = new HttpClient();
        //     return await httpClient.GetAsync(new Uri("http://requestb.in/1f19ydi1"));
        // }

        protected override void OnStop()
        {
            eventLog1.WriteEntry(" Servis zaustavljen");
        }


    }
}
