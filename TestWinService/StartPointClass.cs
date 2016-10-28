using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestWinService
{
    public class StartPointClass
    {
        private static string url = "http://127.0.0.1:8080/EMExport/orderList";
        MSMQ queue = new MSMQ();


        public void Start()
        {
            Thread t1 = new Thread(ThreadWaitingForJson);
            t1.Name = "ThreadWaitingForJson";
            t1.IsBackground = true;
            t1.Start();

            //queue.SendErrorMessageToQueue("test");

            Thread t2 = new Thread(ThreadSendingMsgFromQueueToServer);
            t2.Name = "ThreadSendingMsgFromQueueToServer";
            t2.IsBackground = true;
            t2.Start();
        }


        private void ThreadSendingMsgFromQueueToServer()
        {
            queue.SendMsgFromQueueToServer();
        }

        private void ThreadWaitingForJson()
        {
            OrderHandler oHandler = new OrderHandler();
            oHandler.WaitingForJson(url);
        }



    }
}
