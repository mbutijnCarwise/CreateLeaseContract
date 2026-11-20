public class Quotation(string huurder, string datumOpvolging, string datumTijdVertrek, string datumTijdRetour, string categorie, string kenteken, int teRijdenKilometers, decimal aanbetaling, decimal waarborg, string opmerking)
{
    public string Huurder { get; set; } = huurder;
    public string DatumOpvolging { get; set; } = datumOpvolging;
    public string DatumTijdVertrek { get; set; } = datumTijdVertrek;
    public string DatumTijdRetour { get; set; } = datumTijdRetour;
    public string Categorie { get; set; } = categorie;
    public string Kenteken { get; set; } = kenteken;
    public int TeRijdenKilometers { get; set; } = teRijdenKilometers;
    public decimal Aanbetaling { get; set; } = aanbetaling;
    public decimal Waarborg { get; set; } = waarborg;
    public string Opmerking { get; set; } = opmerking;
}