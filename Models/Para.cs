namespace raspisanie.Models;

public class Para // пара в виде класса (далее в коде я это меняю чтобы проще было просто в бд закидывать)
{
    public int Id { get; set; }

    public string Subject { get; set; } = string.Empty; 
    public DateTime Date { get; set; }                  
    public string Time { get; set; } = string.Empty;    
    public string Teacher { get; set; } = string.Empty; 
    public string Room { get; set; } = string.Empty;    

    public bool? IsAttended { get; set; }               // null = не отмечено, true = был, false = не был 123sa321
}
