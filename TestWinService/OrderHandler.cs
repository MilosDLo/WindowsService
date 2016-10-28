using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestWinService.Sqllite;

namespace TestWinService
{
    public class OrderHandler
    {
        private List<ListOneOrder> listOfOrders;
        private MSMQ queue;
        SqliteEClass sqliteEInstance;

        public OrderHandler()
        {
            queue = new MSMQ();
            sqliteEInstance = SqliteEClass.GetInstance();
        }

        public void WaitingForJson(string url)
        {
            bool isCatch;

            while (true)
            {
                isCatch = false;
                string jsonStringFromUrl = "";

                //skidanje jsona sa servera/web-a
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        jsonStringFromUrl = wc.DownloadString(url);
                    }
                    //ili asinhrono,ali treba novija verzija .Net-a
                    // private static async Task<HttpResponseMessage> MakeRequest()
                    // {
                    //     var httpClient = new HttpClient();
                    //     return await httpClient.GetAsync(new Uri("http://requestb.in/1f19ydi1"));
                    // }

                    listOfOrders = JsonDeserializer.GetListOfOrders(jsonStringFromUrl);
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Ne moze da pristupi serveru na datoj adresi,ili je json drugog formata");
                    isCatch = true;
                    // ili continue,ali onda ovde mora thread.sleep(60000)
                }

                if (!isCatch)
                {
                    #region dump podaci
                    //     samo ubacivanje dump podataka
                    //     using (SQLiteCommand command = new SQLiteCommand("INSERT INTO OrderTable (idjson,desc,config) values (112,'proba','nestonesto') ", conn))
                    //     {
                    //         int i = command.ExecuteNonQuery();
                    //         eventLog1.WriteEntry(i.ToString());
                    //     }
                    #endregion

                    //provera da li je zadnje upisan json kao ovaj novi,ako ne onda upisi u bazu
                    sqliteEInstance.CheckOrderIfExist(listOfOrders);
                }
                Thread.Sleep(60000);       //ili vec na koji interval treba 
            }
        }


    }
}
