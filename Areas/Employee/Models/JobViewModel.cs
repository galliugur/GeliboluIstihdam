public class JobViewModel
{
    public int ID { get; set; }
    public string IlanBaslik { get; set; }
    public string Pozisyon { get; set; }
    public int CalismaSekli { get; set; }
    public string IlanDetay { get; set; }
    public DateTime YayinTarihi { get; set; }
    public DateTime SonBasvuruTarihi { get; set; }
    public string SektorAdi { get; set; }
    public string FirmaAdi { get; set; }
    public string FirmaTelefon { get; set; }
    public string FirmaMail { get; set; }
    public bool BasvuruYapilmis { get; set; }
} 