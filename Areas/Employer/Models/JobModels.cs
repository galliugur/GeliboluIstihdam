using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GeliboluIstihdam.Areas.Employer.Models
{
    public class SectorModel
    {
        public int ID { get; set; }
        public string Sektor { get; set; }
        public int Durum { get; set; }
    }

    public class ProvinceModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class DistrictModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ProvinceID { get; set; }
    }

    public class JobPostingModel
    {
        public int SektorID { get; set; }
        public string Pozisyon { get; set; }
        public string Aciklama { get; set; }
        public int SehirID { get; set; }
        public int IlceID { get; set; }
        public int CalismaSekli { get; set; }
        public int TecrubeSeviyesi { get; set; }
        public int EgitimSeviyesi { get; set; }
    }

    public class EditJobPostingModel : JobPostingModel
    {
        public int ID { get; set; }
    }

    public class EditJobPostingViewModel : EditJobPostingModel
    {
        public List<SectorModel> Sectors { get; set; }
        public List<ProvinceModel> Provinces { get; set; }
        public List<DistrictModel> Districts { get; set; }
    }

    public class JobPosting
    {
        public int ID { get; set; }
        public string IlanBaslik { get; set; }
        public string SektorAdi { get; set; }
        public string SehirAdi { get; set; }
        public string IlceAdi { get; set; }
        public int CalismaSekli { get; set; }
        public DateTime KayitZamani { get; set; }
        public int BasvuruSayisi { get; set; }
        public int Durum { get; set; }

        public string CalismaSekliText => CalismaSekli switch
        {
            1 => "Tam Zamanlı",
            2 => "Yarı Zamanlı",
            3 => "Uzaktan",
            4 => "Stajyer",
            _ => "Belirsiz"
        };

        public string DurumText => Durum == 1 ? "Aktif" : "Pasif";
    }

    public class JobPostingsViewModel
    {
        public List<JobPosting> JobPostings { get; set; }
    }

    public class JobApplication
    {
        public int ID { get; set; }
        public int IlanID { get; set; }
        public string IlanBaslik { get; set; }
        public string AdaySoyad { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Cinsiyet { get; set; }
        public int Yas { get; set; }
        public string EgitimDurumu { get; set; }
        public string Deneyim { get; set; }
        public DateTime BasvuruZamani { get; set; }
        public int Durum { get; set; }
        public string DurumText => Durum switch
        {
            1 => "Yeni Başvuru",
            2 => "İnceleniyor",
            3 => "Mülakat",
            4 => "Kabul Edildi",
            5 => "Reddedildi",
            _ => "Belirsiz"
        };
    }

    public class ApplicationsViewModel
    {
        public List<JobApplication> Applications { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Firma unvanı zorunludur")]
        public string FirmaUnvan { get; set; }

        [Required(ErrorMessage = "Vergi numarası zorunludur")]
        public string VergiNo { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Sifre { get; set; }

        [Required(ErrorMessage = "Şehir seçimi zorunludur")]
        public int SehirID { get; set; }

        [Required(ErrorMessage = "İlçe seçimi zorunludur")]
        public int IlceID { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        public string Adres { get; set; }

        public List<ProvinceModel> Provinces { get; set; }
        public List<DistrictModel> Districts { get; set; }
    }

    public class CreateJobPostingViewModel
    {
        public List<SectorModel> Sectors { get; set; }
        public List<ProvinceModel> Provinces { get; set; }
        public List<DistrictModel> Districts { get; set; }
    }

    public class ProfileViewModel
    {
        public int ID { get; set; }
        public string FirmaUnvan { get; set; }
        public string VergiNo { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        public int SehirID { get; set; }
        public int IlceID { get; set; }
        public string Adres { get; set; }
        public DateTime KayitZamani { get; set; }
        public int Durum { get; set; }

        public List<ProvinceModel> Provinces { get; set; }
        public List<DistrictModel> Districts { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur")]
        public string MevcutSifre { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string YeniSifre { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string YeniSifreTekrar { get; set; }
    }
} 