public class Reservation(string huurder, Categorie categorie, string kenteken, string autoOmschrijving, Brandstof brandstof)
{
    public string Huurder { get; set; } = huurder;
    public Categorie Categorie { get; set; } = categorie;
    public string AutoOmschrijving { get; set; } = autoOmschrijving;
    public Brandstof Brandstof { get; set; } = brandstof;
    public string Nummer { get; set; } = null!;
    public string Kenteken { get; set; } = kenteken;
    public string Soort { get; set; } = null!;
    public string Eenheid { get; set; } = null!;

}