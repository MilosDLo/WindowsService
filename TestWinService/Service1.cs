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

namespace TestWinService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource("MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }


        PersonJson person = null;

        string jsonString = @"{
                'Name' : 'Milos',
                'Adress' : 'Bg'                        
            }";
            


        protected override void OnStart(string[] args)
        {
            // Library.WriteToLog("\n\n" + DateTime.Now.ToString()+": Servis pokrenut");
            eventLog1.WriteEntry("Servis pokrenut");

            #region ako se dobije url sa json-om
             var jsonUrl = "http://echo.jsontest.com/Adress/Bg/Name/Milos";
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

            //sad parsiramo sa JSON.NET
            //ako je nekog atributa nema bice null
            //person = JsonConvert.DeserializeObject<PersonJson>(jsonString);
            person = JsonConvert.DeserializeObject<PersonJson>(jsonStringUrl);

            Thread.CurrentThread.Name = "main";
            PrintThread();
            

            Thread t1 = new Thread(PrintThread);
            t1.Name = "1.";
            t1.Start();
            //t1.Join();
            
            Thread t2 = new Thread(PrintThread);
            t2.Name = "2.";
            t2.Start();
            
            Thread t3 = new Thread(PrintThread);
            t3.Name = "3.";
            t3.Start();




            eventLog1.WriteEntry("Kraj main thread-a");
        }

        private void PrintThread()
        {
            var message = "Ispisujem iz "  +Thread.CurrentThread.Name+ " thread-a: " +person.Name+", "+person.Adress;
            for (int i = 0; i < 3; i++)
            {
                // Library.WriteToLog(message);
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
