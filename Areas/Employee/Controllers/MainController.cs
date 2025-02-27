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
    [Area("Employee")]
    public class MainController : Controller
    {
        // Profil sayfasını görüntüle
        public IActionResult Index()
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employee" });
            }

            try
            {
                // Kullanıcı bilgilerini getir
                var kullaniciSorgu = @"SELECT 
                                        ID, Ad, Soyad, Mail, GSM, TCKN, KayitZamani
                                    FROM IsArayan 
                                    WHERE ID = @ID";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("ID", kullaniciId)
                );

                var kullanici = SqlService.FetchSingleDataRow("Default", kullaniciSorgu, parametreler);

                if (kullanici == null)
                {
                    return RedirectToAction("Logon", "Account", new { area = "Employee" });
                }

                // İstatistikleri getir
                var istatistikSorgu = @"
                    SELECT 
                        COUNT(*) as ToplamBasvuru,
                        SUM(CASE WHEN Durum = 1 THEN 1 ELSE 0 END) as AktifBasvuru,
                        SUM(CASE WHEN Durum = 2 THEN 1 ELSE 0 END) as MulakatSayisi
                    FROM IlanBasvurulari 
                    WHERE IsArayanID = @IsArayanID";

                var istatistikParametreler = new QueryVariableCollection(
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var istatistikler = SqlService.FetchSingleDataRow("Default", istatistikSorgu, istatistikParametreler);

                // İstatistikleri ViewBag'e ekle
                ViewBag.ToplamBasvuru = istatistikler != null ? istatistikler["ToplamBasvuru"] : 0;
                ViewBag.AktifBasvuru = istatistikler != null ? istatistikler["AktifBasvuru"] : 0;
                ViewBag.MulakatSayisi = istatistikler != null ? istatistikler["MulakatSayisi"] : 0;

                return View(kullanici);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Bilgiler yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        // Profil güncelleme action'ı
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile()
        {
            try
            {
                var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
                if (!kullaniciId.HasValue)
                    return Json(new { success = false, message = "Oturum süresi dolmuş!" });

                // Form verilerini al
                var fields = new Dictionary<string, object>
                {
                    { "Ad", Request.Form["ad"].ToString().Trim().ToUpper() },
                    { "Soyad", Request.Form["soyad"].ToString().Trim().ToUpper() },
                    { "TCKN", Request.Form["tckn"].ToString().Trim() },
                    { "Mail", Request.Form["mail"].ToString().Trim().ToLower() },
                    { "GSM", Request.Form["gsm"].ToString().Trim() },
                    { "GuncellemeZamani", DateTime.Now }
                };

                // Şifre değişikliği kontrolü
                string currentPassword = Request.Form["currentPassword"].ToString();
                string newPassword = Request.Form["newPassword"].ToString();

                if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
                {
                    // Mevcut şifreyi kontrol et
                    string currentPasswordHash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(currentPassword);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);
                        currentPasswordHash = Convert.ToHexString(hashBytes).ToLower();
                    }

                    // Veritabanındaki şifreyi kontrol et
                    var sifreKontrol = SqlService.FetchSingleDataRow(
                        "Default",
                        "SELECT SifreHash FROM IsArayan WHERE ID = @ID",
                        new QueryVariableCollection(new QueryVariable("ID", kullaniciId))
                    );

                    if (sifreKontrol == null || !string.Equals(currentPasswordHash, sifreKontrol["SifreHash"].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return Json(new { success = false, message = "Mevcut şifreniz hatalı!" });
                    }

                    // Yeni şifreyi hashle
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(newPassword);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);
                        fields.Add("SifreHash", Convert.ToHexString(hashBytes).ToLower());
                    }
                }

                var result = SqlService.Update(
                    Alias: "Default",
                    TableName: "IsArayan",
                    IdentifierValue: kullaniciId.Value,
                    Fields: fields
                );

                // Session'ları güncelle
                HttpContext.Session.SetString("IsArayanAd", fields["Ad"].ToString());
                HttpContext.Session.SetString("IsArayanSoyad", fields["Soyad"].ToString());
                HttpContext.Session.SetString("IsArayanMail", fields["Mail"].ToString());
                HttpContext.Session.SetString("IsArayanGSM", fields["GSM"].ToString());

                return Json(new { 
                    success = true, 
                    message = "Profiliniz başarıyla güncellendi." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Güncelleme sırasında hata: {ex.Message}" 
                });
            }
        }

        // Çıkış yapma action'ı
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home", new { area = "" });
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
        public IActionResult ApplyJob(int ilanId)
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
            }

            try
            {
                // Daha önce başvuru yapılmış mı kontrol et
                var kontrolSorgu = @"SELECT COUNT(*) FROM IlanBasvurulari 
                                    WHERE IlanID = @IlanID AND IsArayanID = @IsArayanID";

                var kontrolParametreler = new QueryVariableCollection(
                    new QueryVariable("IlanID", ilanId),
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var basvuruSayisi = SqlService.ExecuteScalar("Default", kontrolSorgu, kontrolParametreler).TryInteger();

                if (basvuruSayisi > 0)
                {
                    return Json(new { success = false, message = "Bu ilana daha önce başvuru yapmışsınız!" });
                }

                // Yeni başvuru ekle
                var fields = new Dictionary<string, object>
                {
                    { "IlanID", ilanId },
                    { "IsArayanID", kullaniciId },
                    { "BasvuruZamani", DateTime.Now },
                    { "Durum", 1 } // 1: Aktif başvuru
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

        public IActionResult Jobs()
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employee" });
            }

            try
            {
                // Aktif iş ilanlarını getir
                var ilanlarSorgu = @"
                    SELECT 
                        i.ID,
                        i.IlanBaslik,
                        i.Pozisyon,
                        i.CalismaSekli,
                        i.Aciklama as IlanDetay,
                        i.KayitZamani as YayinTarihi,
                        i.GuncellemeZamani as SonBasvuruTarihi,
                        s.Sektor as SektorAdi,
                        f.FirmaUnvan as FirmaAdi,
                        f.GSM as FirmaTelefon,
                        f.Eposta as FirmaMail,
                        (SELECT COUNT(*) FROM IlanBasvurulari WHERE IlanID = i.ID AND IsArayanID = @IsArayanID) as BasvuruYapilmis
                    FROM Ilanlar i
                    INNER JOIN dbo.Sektor s ON i.SektorID = s.ID
                    INNER JOIN dbo.IsverenFirma f ON i.FirmaID = f.ID
                    WHERE i.Durum = 1 -- Aktif ilanlar
                    ORDER BY i.KayitZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var ilanlarData = SqlService.ExecuteDataTable("Default", ilanlarSorgu, parametreler);

                var model = ilanlarData.Rows.Cast<DataRow>()
                    .Select(row => new JobViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        IlanBaslik = row["IlanBaslik"].ToString(),
                        Pozisyon = row["Pozisyon"].ToString(),
                        CalismaSekli = Convert.ToInt32(row["CalismaSekli"]),
                        IlanDetay = row["IlanDetay"].ToString(),
                        YayinTarihi = Convert.ToDateTime(row["YayinTarihi"]),
                        SonBasvuruTarihi = Convert.ToDateTime(row["SonBasvuruTarihi"]),
                        SektorAdi = row["SektorAdi"].ToString(),
                        FirmaAdi = row["FirmaAdi"].ToString(),
                        FirmaTelefon = row["FirmaTelefon"].ToString(),
                        FirmaMail = row["FirmaMail"].ToString(),
                        BasvuruYapilmis = Convert.ToInt32(row["BasvuruYapilmis"]) > 0
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "İş ilanları yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<JobViewModel>());
            }
        }

        public IActionResult Applications()
        {
            var kullaniciId = HttpContext.Session.GetInt32("IsArayanID");
            if (!kullaniciId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employee" });
            }

            try
            {
                var basvuruSorgu = @"
                    SELECT 
                        b.ID,
                        i.IlanBaslik,
                        i.Pozisyon,
                        i.CalismaSekli,
                        s.Sektor as SektorAdi,
                        f.FirmaUnvan as FirmaAdi,
                        b.BasvuruZamani,
                        b.Durum,
                        b.DurumAciklama
                    FROM IlanBasvurulari b
                    INNER JOIN Ilanlar i ON b.IlanID = i.ID
                    INNER JOIN Sektor s ON i.SektorID = s.ID
                    INNER JOIN IsverenFirma f ON i.FirmaID = f.ID
                    WHERE b.IsArayanID = @IsArayanID
                    ORDER BY b.BasvuruZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("IsArayanID", kullaniciId)
                );

                var basvuruData = SqlService.ExecuteDataTable("Default", basvuruSorgu, parametreler);

                var model = basvuruData.Rows.Cast<DataRow>()
                    .Select(row => new JobApplicationViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        IlanBaslik = row["IlanBaslik"].ToString(),
                        Pozisyon = row["Pozisyon"].ToString(),
                        CalismaSekli = Convert.ToInt32(row["CalismaSekli"]),
                        SektorAdi = row["SektorAdi"].ToString(),
                        FirmaAdi = row["FirmaAdi"].ToString(),
                        BasvuruZamani = Convert.ToDateTime(row["BasvuruZamani"]),
                        Durum = Convert.ToInt32(row["Durum"]),
                        DurumAciklama = row["DurumAciklama"].ToString()
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Başvurularınız yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<JobApplicationViewModel>());
            }
        }
    }
}
