using Microsoft.AspNetCore.Mvc;

namespace GeliboluIstihdam.Areas.Employer.Models
{
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

    public class RegisterViewModel
    {
        public List<ProvinceModel> Provinces { get; set; }
        public RegisterViewModel()
        {
            Provinces = new List<ProvinceModel>();
        }
    }
}
