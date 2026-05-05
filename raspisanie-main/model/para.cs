namespace posechenie.model
{
    public class Para
    {
    public int Id { get; set; }

    public string Subject { get; set; }
    public DateTime Date { get; set; }
    public string Time { get; set; }

    public string Teacher { get; set; }
    public string Room { get; set; }

    public bool? IsAttended { get; set; }
    }
}