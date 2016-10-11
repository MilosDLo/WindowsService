using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWinService
{
    public static class Library
    {
        public static void WriteToLog(string message)
        {
            StreamWriter sw = null;
            using (sw = new StreamWriter(@"C:\Users\milos\Desktop\Log.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString()+", "+message);

            }

        }
    }
}
