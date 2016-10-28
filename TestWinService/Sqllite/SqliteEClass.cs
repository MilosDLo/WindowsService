using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestWinService.Sqllite
{
    //singleton class
    public sealed class SqliteEClass
    {

        private static SqliteEClass instance;
        private static readonly object _lock = new object();

        private string connString = @"Data Source=c:\mydb.sqlite;Version=3";
        private MSMQ queue = new MSMQ();


        private SqliteEClass()
        {
        }


        public static SqliteEClass GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new SqliteEClass();
                    }
                }
            }
            return instance;
        }

        public void InsertOrderIntoTable(ListOneOrder order)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Error connecting to database");
                    return;
                }

                string commandText = String.Format("INSERT INTO OrderTable (idorderitem,status) values ('{0}','{1}')", order.ID, order.Status);

                using (SQLiteCommand commandJsonWrite = new SQLiteCommand(commandText, conn))
                {
                    commandJsonWrite.ExecuteNonQuery();
                }
            }
        }

        public void CheckOrderIfExist(List<ListOneOrder> listOfOrders)
        {
            
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Error connecting to database");
                    return;
                }

                #region kreiranje tabele,ako nema
                using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [OrderTable] (id INTEGER PRIMARY KEY AUTOINCREMENT,idorderitem TEXT,status TEXT)", conn))
                {
                    int i = command.ExecuteNonQuery();
                }
                #endregion


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
                                    queue.SendMsgToSever("Same order exists in table.");
                                }
                                catch (Exception)
                                {
                                    queue.SendErrorMessageToQueue("Error sending msg to server");
                                }
                            }
                            else
                            {
                                InsertOrderIntoTable(order);
                            }
                        }
                    }
                }
            }
        }

        public void DropTable()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception)
                {
                    queue.SendErrorMessageToQueue("Error connecting to database");
                    return;
                }

                using (SQLiteCommand command = new SQLiteCommand("DROP TABLE IF EXISTS OrderTable", conn))
                {
                    int i = command.ExecuteNonQuery();
                }
            }
        }

}
}
