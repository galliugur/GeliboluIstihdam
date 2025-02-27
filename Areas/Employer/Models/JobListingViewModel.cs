namespace GeliboluIstihdam.Areas.Employer.Models
{
    public class JobListingViewModel
    {
        public int ID { get; set; }
        public string IlanBaslik { get; set; }
        public string Pozisyon { get; set; }
        public int CalismaSekli { get; set; }
        public string Aciklama { get; set; }
        public string SektorAdi { get; set; }
        public DateTime KayitZamani { get; set; }
        public int BasvuruSayisi { get; set; }

        public string CalismaSekliText => CalismaSekli == 1 ? "Tam Zamanlı" : "Yarı Zamanlı";
    }
} 