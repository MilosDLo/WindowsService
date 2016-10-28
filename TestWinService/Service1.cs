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
using TestWinService.Sqllite;

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


        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Servis pokrenut");

            StartPointClass start = new StartPointClass();
            start.Start();
 
            //SqliteEClass.GetInstance().DropTable();

            eventLog1.WriteEntry("Kraj main thread-a");
        }


        protected override void OnStop()
        {
            eventLog1.WriteEntry(" Servis zaustavljen");
        }

    }
}
