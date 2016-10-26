namespace TestWinService
{
    public class ListOneOrder
    {
        public ListOneOrder(string iD, string status)
        {
            ID = iD;
            Status = status;
        }

        public ListOneOrder()
        {

        }
        public string ID { get; set; }
        public string Status { get; set; }

    }
}