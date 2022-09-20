namespace SheepQQBot3.Model
{
    public class ClientData
    {
        public bool Group { get; set; }
        public long Group_Id { get; set; }
        public string Message { get; set; }
        public int Message_Id { get; set; }
        public int Message_Seq { get; set; }
        public string Message_Type { get; set; }
        public string Raw_Message { get; set; }
        public int Read_Id { get; set; }
        public int Time { get; set; }
        public Sender Sender { get; set; }
    }
}