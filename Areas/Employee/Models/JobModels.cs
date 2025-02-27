using System;
using System.Collections.Generic;

namespace GeliboluIstihdam.Areas.Employee.Models
{
    public class JobListingViewModel
    {
        public int ID { get; set; }
        public string IlanBaslik { get; set; }
        public string Pozisyon { get; set; }
        public string Aciklama { get; set; }
        public int CalismaSekli { get; set; }
        public string SektorAdi { get; set; }
        public string FirmaAdi { get; set; }
        public string FirmaTelefon { get; set; }
        public string FirmaMail { get; set; }
        public DateTime KayitZamani { get; set; }
        public int BasvuruSayisi { get; set; }
        public bool BasvurulduMu { get; set; }
    }

    public class ApplicationViewModel
    {
        public int ID { get; set; }
        public int IlanID { get; set; }
        public string IlanBaslik { get; set; }
        public string Pozisyon { get; set; }
        public int CalismaSekli { get; set; }
        public string SektorAdi { get; set; }
        public string FirmaAdi { get; set; }
        public string FirmaTelefon { get; set; }
        public string FirmaMail { get; set; }
        public DateTime BasvuruZamani { get; set; }
        public int Durum { get; set; }
    }
} 