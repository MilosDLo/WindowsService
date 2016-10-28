using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Threading;

namespace TestWinService
{
    class MSMQ
    {
        private MessageQueue estehQueue;

        public MSMQ()
        {
            if (MessageQueue.Exists(@".\Private$\QueueEsteh"))
            {
                estehQueue = new MessageQueue(@".\Private$\QueueEsteh");
            }
            else
            {
                estehQueue = MessageQueue.Create(@".\Private$\QueueEsteh");
            }
        }


        public string ReceiveErrorMessageFromQueue()
        {
            estehQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });

            //da li samo da ceka,ili i ovo sa sekundama?
            //Message errorMessage = estehQueue.Receive(new TimeSpan(0,0,2));
            Message errorMessage = estehQueue.Receive();
            string er = errorMessage.Body as string;
            return er;
        }


        public bool SendErrorMessageToQueue(string er)
        {
            try
            {
                Message msg = new Message(er);
                estehQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
                estehQueue.Send(msg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SendMsgToSever(string msgToServer)
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
            //eventLog1.WriteEntry(msgToServer + " //Poruka poslata serveru.");
        }

        public void SendMsgFromQueueToServer()
        {
            string errorMsg = null;

            while (true)
            {
                if (errorMsg == null)
                {
                    //metoda sinhrono ceka da se vrati message,ne ide dalje dok ne dobije
                    errorMsg = ReceiveErrorMessageFromQueue();
                    try
                    {
                        SendMsgToSever(errorMsg);
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
                        SendMsgToSever(errorMsg);
                        errorMsg = null;
                    }
                    catch (Exception)
                    {

                        Thread.Sleep(60000);
                    }
                }
            }

        }



    }
}
