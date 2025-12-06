namespace FrasesRandomAPI.Models;

public class Quote
{
    public int Id { get; set; }
    public required string Autor { get; set; }
    public required string Texto { get; set; }
    public DateTime Fecha { get; set; }
}
