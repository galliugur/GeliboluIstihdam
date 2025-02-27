namespace GeliboluIstihdam.Areas.Employer.Models
{
    public class JobPostingViewModel
    {
        public int ID { get; set; }
        public int SektorID { get; set; }
        public string IlanBaslik { get; set; }
        public string Pozisyon { get; set; }
        public int CalismaSekli { get; set; }
        public int TecrubeYili { get; set; }
        public int EgitimSeviyesi { get; set; }
        public string Aciklama { get; set; }
        public string Nitelikler { get; set; }
        public string IsTanimi { get; set; }
        public int SehirID { get; set; }
        public int IlceID { get; set; }
        public string MaasAralik { get; set; }
    }
} 