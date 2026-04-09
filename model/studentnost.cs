namespace posechenie.model
{
    public class studentnost
    {
        public string Name { get; set; }
        public spisokpar spisokpar { get; set; } = new spisokpar();
    }
}