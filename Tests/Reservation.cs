public class Reservation(Categorie categorie, string kenteken, string autoOmschrijving, Brandstof brandstof)
{
    public Categorie Categorie { get; set; } = categorie;
    public string AutoOmschrijving { get; set; } = autoOmschrijving;
    public Brandstof brandstof { get; set; } = brandstof;
    public string Nummer { get; set; } = null!;
    public string Kenteken { get; set; } = kenteken;
    public string Soort { get; set; } = null!;
    public string Eenheid { get; set; } = null!;

}