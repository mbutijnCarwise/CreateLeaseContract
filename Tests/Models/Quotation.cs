public class Quotation(string huurder, string datumOpvolging, string datumTijdVertrek, string datumTijdRetour, Categorie categorie, string kenteken, int teRijdenKilometers, decimal aanbetaling, decimal waarborg, string opmerking, string soort, string eenheid)
{
    public string Huurder { get; set; } = huurder;
    public string DatumOpvolging { get; set; } = datumOpvolging;
    public string DatumTijdVertrek { get; set; } = datumTijdVertrek;
    public string DatumTijdRetour { get; set; } = datumTijdRetour;
    public Categorie Categorie { get; set; } = categorie;
    public string Kenteken { get; set; } = kenteken;
    public int TeRijdenKilometers { get; set; } = teRijdenKilometers;
    public decimal Aanbetaling { get; set; } = aanbetaling;
    public decimal Waarborg { get; set; } = waarborg;
    public string Opmerking { get; set; } = opmerking;
    public string Soort { get; set; } = soort;
    public string Eenheid { get; set; } = eenheid;

    /// <summary>The number the application gives the quotation on its first save.</summary>
    public int? Nummer { get; set; }
}