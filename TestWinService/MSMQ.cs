using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
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





    }
}
