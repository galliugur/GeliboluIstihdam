using Microsoft.AspNetCore.Mvc;
using Netia.Katharsys.Core;

namespace GeliboluIstihdam.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register()
        {
            if (!(Request.ContentLength > 0 && Request.Form != null && Request.Form.Count > 0))
                return Json(new { success = false, message = "Geçersiz istek." });

            // Boşlukları temizleme yardımcı fonksiyonu
            string CleanString(string input) =>
                System.Text.RegularExpressions.Regex.Replace(
                    input?.Trim() ?? string.Empty,
                    @"\s+", // Birden fazla boşluğu yakala
                    " "     // Tek boşlukla değiştir
                ).Trim();

            // Form verilerini al, temizle ve formatla
            string ad = CleanString(Request.Form["firstName"].TryString()).ToUpper();
            string soyad = CleanString(Request.Form["lastName"].TryString()).ToUpper();
            string ePosta = CleanString(Request.Form["email"].TryString()).ToLower();

            string telefonNo = Request.Form["phone"].TryString()
                ?.Trim()
                .Replace(" ", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "")
                .Replace("_", "") ?? string.Empty;

            string sifre = Request.Form["passwordEmployeeRegister"].TryString() ?? string.Empty;

            // Şifre kontrolü
            if (sifre.Length < 10 ||
                !System.Text.RegularExpressions.Regex.IsMatch(sifre, @"[A-Z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(sifre, @"[a-z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(sifre, @"\d") ||
                !System.Text.RegularExpressions.Regex.IsMatch(sifre, @"[^A-Za-z0-9]"))
            {
                return Json(new { success = false, message = "Şifre gerekli kriterleri karşılamıyor." });
            }

            // Boş string kontrolü
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(telefonNo) || string.IsNullOrWhiteSpace(ePosta))
                return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurun." });

            // E-posta formatı kontrolü
            if (!System.Text.RegularExpressions.Regex.IsMatch(ePosta,
                @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return Json(new { success = false, message = "Geçersiz e-posta formatı." });

            // Mükerrer kayıt kontrolü
            int kayitSayisi = SqlService.ExecuteScalar(
                Alias: "Default",
                Query: "SELECT COUNT(*) FROM IsArayan WHERE GSM = @GSM OR Mail = @Mail",
                new()
                {
                    { "GSM", new("GSM", telefonNo) },
                    { "Mail", new("Mail", ePosta) }
                }
            ).TryInteger();

            if (kayitSayisi > 0)
            {
                return Json(new { success = false, message = "Bu telefon numarası veya e-posta adresi zaten kayıtlı." });
            }

            // MD5 hash oluştur
            string sifreHash = "";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sifre);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                sifreHash = Convert.ToHexString(hashBytes).ToLower();
            }

            // Veritabanına kaydet
            var result = SqlService.Insert(
                Alias: "Default",
                TableName: "IsArayan",
                new Dictionary<string, object>()
                {
                    { "Ad", ad },
                    { "Soyad", soyad },
                    { "Mail", ePosta },
                    { "GSM", telefonNo },
                    { "SifreHash", sifreHash }
                }
            );

            return Json(new { success = true, message = "Kayıt işlemi başarıyla tamamlandı.", redirectUrl = "/Employee/Main/Index" });
        }
        public IActionResult Logon()
        {
            return View();
        }
    }
}
