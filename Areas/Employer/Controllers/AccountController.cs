using Microsoft.AspNetCore.Mvc;
using Netia.Katharsys.Core;
using System.Data;
using GeliboluIstihdam.Areas.Employer.Models;
using System.Linq;

namespace GeliboluIstihdam.Areas.Employer.Controllers
{

    [Area("Employer")]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Create()
        {
            if (!(Request.ContentLength > 0 && Request.Form != null && Request.Form.Count > 0))
                return Json(new { success = false, message = "Geçersiz istek." });

            // Form verilerini al ve temizle
            string firmaAdi = Request.Form["companyTitle"].TryString().ToUpper().Trim();
            int firmaTuruID = Request.Form["companyType"].TryInteger();
            string vergiNo = Request.Form["taxNumber"].TryString().Trim();
            string telefonNo = Request.Form["phoneNumber"].TryString()
                .Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("_", "");
            string ePosta = Request.Form["e-mail"].TryString().ToLower().Trim();
            string firmaAdres = Request.Form["companyAddress"].TryString().Trim();
            int firmaSehirID = Request.Form["city"].TryInteger();
            int firmaIlceID = Request.Form["district"].TryInteger();
            string postaKodu = Request.Form["postalCode"].TryString().Trim();
            string sifre = Request.Form["password"].TryString();
            string rSifre = Request.Form["r-password"].TryString();

            // Veri doğrulama kontrolleri
            if (string.IsNullOrWhiteSpace(firmaAdi) || string.IsNullOrWhiteSpace(vergiNo) ||
                string.IsNullOrWhiteSpace(telefonNo) || string.IsNullOrWhiteSpace(ePosta))
                return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurun." });

            // E-posta formatı kontrolü
            if (!System.Text.RegularExpressions.Regex.IsMatch(ePosta,
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return Json(new { success = false, message = "Geçersiz e-posta formatı." });

            // Vergi no formatı kontrolü (11 haneli sayısal değer)
            if (!System.Text.RegularExpressions.Regex.IsMatch(vergiNo, @"^\d{11}$"))
                return Json(new { success = false, message = "Vergi numarası 11 haneli sayısal değer olmalıdır." });

            // Uzunluk kontrolleri
            if (firmaAdi.Length > 150)
                return Json(new { success = false, message = "Firma adı 150 karakterden uzun olamaz." });

            if (ePosta.Length > 100)
                return Json(new { success = false, message = "E-posta adresi 100 karakterden uzun olamaz." });

            if (firmaAdres.Length > 250)
                return Json(new { success = false, message = "Firma adresi 250 karakterden uzun olamaz." });

            if (postaKodu.Length > 5)
                return Json(new { success = false, message = "Posta kodu 5 karakterden uzun olamaz." });

            if (sifre.Length > 50)
                return Json(new { success = false, message = "Şifre 50 karakterden uzun olamaz." });

            // Mükerrer kayıt kontrolü
            int kayitSayisi = SqlService.ExecuteScalar(
                Alias: "Default",
                Query: "SELECT COUNT(*) FROM IsverenFirma WHERE VKN = @VKN OR GSM = @GSM OR Eposta = @Eposta",
                new()
                {
            { "VKN", new("VKN", vergiNo) },
            { "GSM", new("GSM", telefonNo) },
            { "Eposta", new("Eposta", ePosta) }
                }
            ).TryInteger();

            bool kayitVarMi = kayitSayisi > 0;

            if (kayitVarMi)
            {
                return Json(new { success = false, message = "Girilen vergi numarası, telefon numarası veya e-posta adresi zaten kayıtlı." });
            }

            int eklenenKayitID = SqlService.InsertWithScopeIdentity(
                Alias: "Default",
                TableName: "IsverenFirma",
                new Dictionary<string, object>()
                {
            { "FirmaUnvan", firmaAdi },
            { "FirmaTuruID", firmaTuruID },
            { "VKN", vergiNo },
            { "GSM", telefonNo },
            { "Eposta", ePosta },
            { "Adres", firmaAdres },
            { "SehirID", firmaSehirID },
            { "IlceID", firmaIlceID },
            { "PostaKodu", postaKodu },
                }
            );

            // MD5 hash oluştur
            string sifreHash = "";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sifre);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                sifreHash = Convert.ToHexString(hashBytes).ToLower();
            }

            SqlService.Insert(
                Alias: "Default",
                TableName: "FirmaPersonel",
                new Dictionary<string, object>()
                {
            { "FirmaID", eklenenKayitID },
            { "AdminDurum", 1 },
            { "Ad", "Admin" },
            { "GSM", telefonNo },
            { "mail", ePosta },
            { "SifreHash", sifreHash },
                }
            );

            return Json(new { success = true, message = "Kayıt işlemi başarıyla tamamlandı.", redirectUrl = "Employer/Main/Index" });
        }
        public IActionResult Register()
        {
            var model = new RegisterViewModel();

            // İlleri getir ve modele doldur
            var illerData = SqlService.ExecuteDataTable(
                Alias: "Default",
                Query: @"SELECT ID, Name FROM [GalliKOM-Core].dbo.Province where CountryID = 231 ORDER BY Name"
            );

            model.Provinces = illerData.AsEnumerable()
                .Select(row => new ProvinceModel
                {
                    ID = row.Field<int>("ID"),
                    Name = row.Field<string>("Name")
                }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult GetIlceler(int ilId)
        {
            var ilcelerData = SqlService.ExecuteDataTable(
                Alias: "Default",
                Query: @"SELECT ID, Name FROM [GalliKOM-Core].dbo.District 
                        WHERE ProvinceID = @ProvinceID ORDER BY Name",
                new() { { "ProvinceID", new("ProvinceID", ilId) } }
            );

            var ilceler = ilcelerData.AsEnumerable()
                .Select(row => new DistrictModel
                {
                    ID = row.Field<int>("ID"),
                    Name = row.Field<string>("Name"),
                    ProvinceID = ilId
                }).ToList();

            return Json(ilceler);
        }

        public IActionResult Logon()
        {
            return View(); // Kayıt formunu içeren bir View oluşturun
        }
    }
}
