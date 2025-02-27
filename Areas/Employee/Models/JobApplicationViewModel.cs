using System;

namespace GeliboluIstihdam.Areas.Employee.Models
{
    public class JobApplicationViewModel
    {
        public int ID { get; set; }
        public string IlanBaslik { get; set; }
        public string Pozisyon { get; set; }
        public int CalismaSekli { get; set; }
        public string SektorAdi { get; set; }
        public string FirmaAdi { get; set; }
        public DateTime BasvuruZamani { get; set; }
        public int Durum { get; set; }
        public string DurumAciklama { get; set; }

        public string CalismaSekliText => CalismaSekli == 1 ? "Tam Zamanlı" : "Yarı Zamanlı";
        public string DurumText => Durum switch
        {
            1 => "Başvuru Alındı",
            2 => "Değerlendiriliyor",
            3 => "Mülakat",
            4 => "Kabul Edildi",
            5 => "Reddedildi",
            _ => "Belirsiz"
        };

        public string DurumClass => Durum switch
        {
            1 => "info",
            2 => "warning",
            3 => "primary",
            4 => "success",
            5 => "danger",
            _ => "secondary"
        };
    }
} 