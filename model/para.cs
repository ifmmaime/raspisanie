namespace posechenie.model
{
    public class para
    {
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public bool? IsAttended { get; set; } # bool чтобы 3 состояние хранить
    }
}