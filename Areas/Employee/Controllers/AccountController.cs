using Microsoft.AspNetCore.Mvc;
using Netia.Katharsys.Core;
using Microsoft.AspNetCore.Http;
using QueryVariableCollection = Netia.Katharsys.Core.QueryVariableCollection;
using QueryVariable = Netia.Katharsys.Core.QueryVariable;
using System.Data;
using GeliboluIstihdam.Areas.Employee.Models;
using System.Linq;

namespace GeliboluIstihdam.Areas.Employee.Controllers
{
    // Employee (İş Arayan) alanı için hesap işlemlerini yöneten controller
    [Area("Employee")]
    public class AccountController : Controller
    {
        // Ana sayfa görünümünü döndüren action
        public IActionResult Index()
        {
            return View();
        }

        // Kayıt işlemini gerçekleştiren POST action
        [HttpPost]
        public IActionResult Register(string ad, string soyad, string tckn, string mail, string gsm, string password)
        {
            try
            {
                // Mail adresi kontrolü
                var mailKontrol = SqlService.GetRowCount(
                    "Default",
                    "SELECT COUNT(*) FROM IsArayan WHERE Mail = @Mail",
                    new QueryVariableCollection(new QueryVariable("Mail", mail))
                );

                if (mailKontrol > 0)
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten kayıtlı!" });
                }

                // Şifreyi hashle
                string passwordHash;
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    passwordHash = Convert.ToHexString(hashBytes).ToLower();
                }

                // Yeni kullanıcı ekle
                var fields = new Dictionary<string, object>
                {
                    { "Ad", ad.ToUpper() },
                    { "Soyad", soyad.ToUpper() },
                    { "TCKN", tckn },
                    { "Mail", mail.ToLower() },
                    { "GSM", gsm },
                    { "SifreHash", passwordHash },
                    { "KayitZamani", DateTime.Now },
                    { "Durum", 1 }
                };

                var result = SqlService.Insert("Default", "IsArayan", fields);

                return Json(new { 
                    success = result > 0, 
                    message = result > 0 ? "Kayıt başarılı! Giriş yapabilirsiniz." : "Kayıt yapılırken bir hata oluştu."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kayıt yapılırken bir hata oluştu: " + ex.Message });
            }
        }

        // GET: /Employee/Account/Logon
        // Login formunu görüntülemek için kullanılan action
        public IActionResult Logon()
        {
            return View();
        }

        // POST: /Employee/Account/LogonPost
        // Login formundan gelen verileri işleyen action
        [HttpPost]
        public IActionResult LogonPost(string email, string password)
        {
            try
            {
                // Şifreyi hashle
                string passwordHash;
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    passwordHash = Convert.ToHexString(hashBytes).ToLower();
                }

                // Kullanıcıyı kontrol et
                var kullaniciSorgu = @"SELECT 
                                        ID, Ad, Soyad, Mail, GSM
                                    FROM IsArayan 
                                    WHERE Mail = @Mail 
                                        AND SifreHash = @SifreHash 
                                        AND Durum = 1";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("Mail", email),
                    new QueryVariable("SifreHash", passwordHash)
                );

                var kullanici = SqlService.FetchSingleDataRow("Default", kullaniciSorgu, parametreler);

                if (kullanici != null)
                {
                    // Session'ları ayarla
                    HttpContext.Session.SetInt32("IsArayanID", Convert.ToInt32(kullanici["ID"]));
                    HttpContext.Session.SetString("IsArayanAd", kullanici["Ad"].ToString());
                    HttpContext.Session.SetString("IsArayanSoyad", kullanici["Soyad"].ToString());
                    HttpContext.Session.SetString("IsArayanMail", kullanici["Mail"].ToString());
                    HttpContext.Session.SetString("IsArayanGSM", kullanici["GSM"].ToString());

                    // Doğrudan yönlendir
                    return RedirectToAction("Index", "Main", new { area = "Employee" });
                }
                else
                {
                    TempData["ErrorMessage"] = "E-posta veya şifre hatalı!";
                    return RedirectToAction("Logon");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Giriş yapılırken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Logon");
            }
        }

        [HttpGet]
        public IActionResult JobListings()
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employee" });
            }

            ViewData["Title"] = "İş İlanları";
            ViewData["Page"] = "İş İlanları";

            try
            {
                var ilanlarSorgu = @"
                    SELECT 
                        i.ID, 
                        i.IlanBaslik,
                        i.Pozisyon,
                        i.CalismaSekli,
                        i.Aciklama,
                        i.KayitZamani,
                        s.Sektor as SektorAdi,
                        f.FirmaUnvan as FirmaAdi,
                        f.GSM as FirmaTelefon,
                        f.Mail as FirmaMail,
                        (SELECT COUNT(*) FROM IlanBasvurulari WHERE IlanID = i.ID) as BasvuruSayisi,
                        (SELECT COUNT(*) FROM IlanBasvurulari WHERE IlanID = i.ID AND IsArayanID = @IsArayanID) as BasvurulduMu
                    FROM Ilanlar i
                    INNER JOIN dbo.Sektor s ON i.SektorID = s.ID
                    INNER JOIN dbo.IsverenFirma f ON i.FirmaID = f.ID
                    WHERE i.Durum = 1
                    ORDER BY i.KayitZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var ilanlarData = SqlService.ExecuteDataTable("Default", ilanlarSorgu, parametreler);

                var model = ilanlarData.Rows.Cast<DataRow>()
                    .Select(row => new JobListingViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        IlanBaslik = row["IlanBaslik"].ToString(),
                        Pozisyon = row["Pozisyon"].ToString(),
                        Aciklama = row["Aciklama"].ToString(),
                        CalismaSekli = Convert.ToInt32(row["CalismaSekli"]),
                        SektorAdi = row["SektorAdi"].ToString(),
                        FirmaAdi = row["FirmaAdi"].ToString(),
                        FirmaTelefon = row["FirmaTelefon"].ToString(),
                        FirmaMail = row["FirmaMail"].ToString(),
                        KayitZamani = Convert.ToDateTime(row["KayitZamani"]),
                        BasvuruSayisi = Convert.ToInt32(row["BasvuruSayisi"]),
                        BasvurulduMu = Convert.ToInt32(row["BasvurulduMu"]) > 0
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "İlanlar yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<JobListingViewModel>());
            }
        }

        [HttpPost]
        public IActionResult ApplyForJob(int ilanId)
        {
            try
            {
                var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
                if (!kullaniciId.HasValue)
                    return Json(new { success = false, message = "Oturum süresi dolmuş!" });

                // Daha önce başvuru yapılmış mı kontrol et
                var kontrolSorgu = @"
                    SELECT COUNT(*) FROM IlanBasvurulari 
                    WHERE IlanID = @IlanID AND IsArayanID = @IsArayanID";

                var kontrolParametreler = new QueryVariableCollection(
                    new QueryVariable("IlanID", ilanId),
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var basvuruSayisi = SqlService.ExecuteScalar("Default", kontrolSorgu, kontrolParametreler);

                if (Convert.ToInt32(basvuruSayisi) > 0)
                {
                    return Json(new { success = false, message = "Bu ilana daha önce başvuru yaptınız!" });
                }

                // Yeni başvuru ekle
                var fields = new Dictionary<string, object>
                {
                    { "IlanID", ilanId },
                    { "IsArayanID", kullaniciId },
                    { "BasvuruZamani", DateTime.Now },
                    { "Durum", 1 } // 1: Yeni Başvuru
                };

                var result = SqlService.Insert("Default", "IlanBasvurulari", fields);

                return Json(new { 
                    success = result > 0, 
                    message = result > 0 ? "Başvurunuz başarıyla alındı." : "Başvuru yapılırken bir hata oluştu."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Başvuru yapılırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult MyApplications()
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employee" });
            }

            ViewData["Title"] = "Başvurularım";
            ViewData["Page"] = "Başvurularım";

            try
            {
                var basvurularSorgu = @"
                    SELECT 
                        b.ID,
                        b.IlanID,
                        i.IlanBaslik,
                        i.Pozisyon,
                        i.CalismaSekli,
                        s.Sektor as SektorAdi,
                        f.FirmaUnvan as FirmaAdi,
                        f.GSM as FirmaTelefon,
                        f.Mail as FirmaMail,
                        b.BasvuruZamani,
                        b.Durum
                    FROM IlanBasvurulari b
                    INNER JOIN Ilanlar i ON b.IlanID = i.ID
                    INNER JOIN dbo.Sektor s ON i.SektorID = s.ID
                    INNER JOIN dbo.IsverenFirma f ON i.FirmaID = f.ID
                    WHERE b.IsArayanID = @IsArayanID
                    ORDER BY b.BasvuruZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var basvurularData = SqlService.ExecuteDataTable("Default", basvurularSorgu, parametreler);

                var model = basvurularData.Rows.Cast<DataRow>()
                    .Select(row => new ApplicationViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        IlanID = Convert.ToInt32(row["IlanID"]),
                        IlanBaslik = row["IlanBaslik"].ToString(),
                        Pozisyon = row["Pozisyon"].ToString(),
                        CalismaSekli = Convert.ToInt32(row["CalismaSekli"]),
                        SektorAdi = row["SektorAdi"].ToString(),
                        FirmaAdi = row["FirmaAdi"].ToString(),
                        FirmaTelefon = row["FirmaTelefon"].ToString(),
                        FirmaMail = row["FirmaMail"].ToString(),
                        BasvuruZamani = Convert.ToDateTime(row["BasvuruZamani"]),
                        Durum = Convert.ToInt32(row["Durum"])
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Başvurularınız yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<ApplicationViewModel>());
            }
        }
    }
}
