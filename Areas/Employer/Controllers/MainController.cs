using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using GeliboluIstihdam.Areas.Employer.Models;
using Netia.Katharsys.Core;
using QueryVariableCollection = Netia.Katharsys.Core.QueryVariableCollection;
using QueryVariable = Netia.Katharsys.Core.QueryVariable;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GeliboluIstihdam.Areas.Employer.Controllers
{
    [Area("Employer")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employer" });
            }

            // ViewData değerlerini set et
            ViewData["PageTitle"] = "Dashboard";
            ViewData["Page"] = "Ana Sayfa";

            // İstatistikleri getir
            var istatistikSorgu = @"
                SELECT 
                    (SELECT COUNT(*) FROM Ilanlar WHERE FirmaID = @FirmaID AND Durum = 1) as AktifIlanSayisi,
                    (SELECT COUNT(*) FROM IlanBasvurulari ib 
                     INNER JOIN Ilanlar i ON ib.IlanID = i.ID 
                     WHERE i.FirmaID = @FirmaID) as ToplamBasvuruSayisi,
                    (SELECT COUNT(*) FROM IlanBasvurulari ib 
                     INNER JOIN Ilanlar i ON ib.IlanID = i.ID 
                     WHERE i.FirmaID = @FirmaID 
                     AND CAST(ib.BasvuruZamani AS DATE) = CAST(GETDATE() AS DATE)) as BugunYapilanBasvuru";

            var parametreler = new QueryVariableCollection(
                new QueryVariable("FirmaID", firmaId)
            );

            var istatistikler = SqlService.FetchSingleDataRow("Default", istatistikSorgu, parametreler);

            // Son ilanları getir
            var ilanlarSorgu = @"
                SELECT TOP 10 
                    i.ID, 
                    i.IlanBaslik, 
                    i.Pozisyon,
                    i.CalismaSekli,
                    i.Durum,
                    i.KayitZamani,
                    s.Sektor as SektorAdi,
                    p.Name as SehirAdi,
                    (SELECT COUNT(*) FROM IlanBasvurulari WHERE IlanID = i.ID) as BasvuruSayisi
                FROM Ilanlar i
                INNER JOIN Sektor s ON i.SektorID = s.ID
                INNER JOIN [GalliKOM-Core].dbo.Province p ON i.SehirID = p.ID
                WHERE i.FirmaID = @FirmaID 
                ORDER BY i.KayitZamani DESC";

            var ilanlar = SqlService.ExecuteDataTable("Default", ilanlarSorgu, parametreler);

            ViewBag.Istatistikler = istatistikler;
            ViewBag.Ilanlar = ilanlar;

            return View();
        }
        public IActionResult Profile()
        {
            ViewData["PageTitle"] = "Profil Detayları";
            ViewData["Page"] = "Profil";
            ViewData["SubPage"] = "Profil Detayları";
            return View();
        }

        public IActionResult JobPostings()
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employer" });
            }

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
                        (SELECT COUNT(*) FROM IlanBasvurulari WHERE IlanID = i.ID) as BasvuruSayisi
                    FROM Ilanlar i
                    INNER JOIN Sektor s ON i.SektorID = s.ID
                    WHERE i.FirmaID = @FirmaID
                    ORDER BY i.KayitZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("FirmaID", firmaId)
                );

                var ilanlarData = SqlService.ExecuteDataTable("Default", ilanlarSorgu, parametreler);

                var model = ilanlarData.Rows.Cast<DataRow>()
                    .Select(row => new JobListingViewModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        IlanBaslik = row["IlanBaslik"].ToString(),
                        Pozisyon = row["Pozisyon"].ToString(),
                        CalismaSekli = Convert.ToInt32(row["CalismaSekli"]),
                        Aciklama = row["Aciklama"].ToString(),
                        SektorAdi = row["SektorAdi"].ToString(),
                        KayitZamani = Convert.ToDateTime(row["KayitZamani"]),
                        BasvuruSayisi = Convert.ToInt32(row["BasvuruSayisi"])
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
        public IActionResult UpdateJobStatus(int ilanId, int durum)
        {
            try
            {
                var firmaId = HttpContext.Session.GetInt32("IsverenID");
                if (!firmaId.HasValue)
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı!" });

                var result = SqlService.Update(
                    Alias: "Default",
                    TableName: "Ilanlar",
                    WhereClause: "ID = @IlanID AND FirmaID = @FirmaID",
                    Fields: new Dictionary<string, object> { { "Durum", durum } },
                    Variables: new QueryVariableCollection(
                        new QueryVariable("IlanID", ilanId),
                        new QueryVariable("FirmaID", firmaId)
                    )
                );

                return Json(new { success = true, message = "İlan durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İlan durumu güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult CreateJobPosting()
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employer" });
            }

            try
            {
                // Sektörleri getir
                var sektorSorgu = "SELECT ID, Sektor FROM Sektor WHERE Durum = 1";
                var sektorler = SqlService.ExecuteDataTable("Default", sektorSorgu);

                ViewBag.Sektorler = sektorler.Rows.Cast<DataRow>()
                    .Select(row => new SelectListItem
                    {
                        Value = row["ID"].ToString(),
                        Text = row["Sektor"].ToString()
                    }).ToList();

                // Çalışma şekilleri
                ViewBag.CalismaSekilleri = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "Tam Zamanlı" },
                    new SelectListItem { Value = "2", Text = "Yarı Zamanlı" }
                };

                // Eğitim seviyeleri
                ViewBag.EgitimSeviyeleri = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "İlköğretim" },
                    new SelectListItem { Value = "2", Text = "Lise" },
                    new SelectListItem { Value = "3", Text = "Üniversite" },
                    new SelectListItem { Value = "4", Text = "Yüksek Lisans" },
                    new SelectListItem { Value = "5", Text = "Doktora" }
                };

                return View(new JobPostingViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Veriler yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult CreateJobPosting(JobPostingViewModel model)
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return Json(new { success = false, message = "Oturum süreniz dolmuş!" });
            }

            try
            {
                var fields = new Dictionary<string, object>
                {
                    { "Durum", 1 },
                    { "FirmaID", firmaId },
                    { "SektorID", model.SektorID },
                    { "IlanBaslik", model.IlanBaslik },
                    { "Pozisyon", model.Pozisyon },
                    { "CalismaSekli", model.CalismaSekli },
                    { "TecrubeYili", model.TecrubeYili },
                    { "EgitimSeviyesi", model.EgitimSeviyesi },
                    { "Aciklama", model.Aciklama },
                    { "Nitelikler", model.Nitelikler },
                    { "IsTanimi", model.IsTanimi },
                    { "SehirID", model.SehirID },
                    { "IlceID", model.IlceID },
                    { "MaasAralik", model.MaasAralik },
                    { "KayitZamani", DateTime.Now }
                };

                var result = SqlService.Insert("Default", "Ilanlar", fields);

                return Json(new { 
                    success = result > 0, 
                    message = result > 0 ? "İlan başarıyla oluşturuldu." : "İlan oluşturulurken bir hata oluştu." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İlan oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult Applications()
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employer" });
            }

            ViewData["PageTitle"] = "Başvurular";
            ViewData["Page"] = "Başvurular";

            try
            {
                var basvuruSorgu = @"
                    SELECT 
                        b.ID,
                        b.IlanID,
                        i.IlanBaslik,
                        ia.Ad + ' ' + ia.Soyad as AdaySoyad,
                        ia.Mail as Email,
                        ia.GSM as Telefon,
                        'Belirtilmemiş' as Cinsiyet,
                        YEAR(GETDATE()) - 2000 as Yas,
                        'Belirtilmemiş' as EgitimDurumu,
                        'Belirtilmemiş' as Deneyim,
                        b.BasvuruZamani,
                        b.Durum
                    FROM IlanBasvurulari b
                    INNER JOIN Ilanlar i ON b.IlanID = i.ID
                    INNER JOIN IsArayan ia ON b.IsArayanID = ia.ID
                    WHERE i.FirmaID = @FirmaID
                    ORDER BY b.BasvuruZamani DESC";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("FirmaID", firmaId)
                );

                var basvurularData = SqlService.ExecuteDataTable("Default", basvuruSorgu, parametreler);

                var model = new ApplicationsViewModel
                {
                    Applications = basvurularData.Rows.Cast<DataRow>()
                        .Select(row => new JobApplication
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            IlanID = Convert.ToInt32(row["IlanID"]),
                            IlanBaslik = row["IlanBaslik"].ToString(),
                            AdaySoyad = row["AdaySoyad"].ToString(),
                            Email = row["Email"].ToString(),
                            Telefon = row["Telefon"].ToString(),
                            Cinsiyet = row["Cinsiyet"].ToString(),
                            Yas = Convert.ToInt32(row["Yas"]),
                            EgitimDurumu = row["EgitimDurumu"].ToString(),
                            Deneyim = row["Deneyim"].ToString(),
                            BasvuruZamani = Convert.ToDateTime(row["BasvuruZamani"]),
                            Durum = Convert.ToInt32(row["Durum"])
                        }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Başvurular yüklenirken bir hata oluştu: " + ex.Message;
                return View(new ApplicationsViewModel { Applications = new List<JobApplication>() });
            }
        }

        [HttpPost]
        public IActionResult UpdateApplicationStatus(int basvuruId, int durum)
        {
            try
            {
                var firmaId = HttpContext.Session.GetInt32("IsverenID");
                if (!firmaId.HasValue)
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı!" });

                var result = SqlService.Update(
                    Alias: "Default",
                    TableName: "IlanBasvurulari",
                    WhereClause: "ID = @BasvuruID AND EXISTS (SELECT 1 FROM Ilanlar WHERE ID = IlanBasvurulari.IlanID AND FirmaID = @FirmaID)",
                    Fields: new Dictionary<string, object>
                    {
                        { "Durum", durum },
                        { "GuncellemeZamani", DateTime.Now }
                    },
                    Variables: new QueryVariableCollection(
                        new QueryVariable("BasvuruID", basvuruId),
                        new QueryVariable("FirmaID", firmaId)
                    )
                );

                if (result > 0)
                {
                    return Json(new { success = true, message = "Başvuru durumu güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Başvuru bulunamadı veya güncellenemedi." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Başvuru durumu güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult CompanyUsers()
        {
            ViewData["PageTitle"] = "Şirket Kullanıcıları";
            ViewData["Page"] = "Firma Yönetimi";
            ViewData["SubPage"] = "Kullanıcılar";
            return View();
        }

        public IActionResult CompanySettings()
        {
            ViewData["PageTitle"] = "Firma Ayarları";
            ViewData["Page"] = "Firma Yönetimi";
            ViewData["SubPage"] = "Ayarlar";
            return View();
        }

        [HttpPost]
        public IActionResult DeleteJobPosting(int ilanId)
        {
            try
            {
                var firmaId = HttpContext.Session.GetInt32("IsverenID");
                if (!firmaId.HasValue)
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı!" });

                // İlanın firmaya ait olduğunu kontrol et
                var result = SqlService.Delete(
                    Alias: "Default",
                    TableName: "Ilanlar",
                    WhereClause: "ID = @IlanID AND FirmaID = @FirmaID",
                    Variables: new QueryVariableCollection(
                        new QueryVariable("IlanID", ilanId),
                        new QueryVariable("FirmaID", firmaId)
                    )
                );

                if (result > 0)
                {
                    return Json(new { success = true, message = "İlan başarıyla silindi." });
                }
                else
                {
                    return Json(new { success = false, message = "İlan bulunamadı veya silinirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İlan silinirken bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult EditJobPosting(int id)
        {
            var firmaId = HttpContext.Session.GetInt32("IsverenID");
            if (!firmaId.HasValue)
            {
                return RedirectToAction("Logon", "Account", new { area = "Employer" });
            }

            ViewData["PageTitle"] = "İlan Düzenle";
            ViewData["Page"] = "İş İlanları";
            ViewData["SubPage"] = "İlan Düzenle";

            try
            {
                // İlanı getir
                var ilanSorgu = @"
                    SELECT * FROM Ilanlar 
                    WHERE ID = @IlanID AND FirmaID = @FirmaID";

                var parametreler = new QueryVariableCollection(
                    new QueryVariable("IlanID", id),
                    new QueryVariable("FirmaID", firmaId)
                );

                var ilanData = SqlService.FetchSingleDataRow("Default", ilanSorgu, parametreler);

                if (ilanData == null)
                {
                    return RedirectToAction("JobPostings");
                }

                var model = new EditJobPostingViewModel
                {
                    ID = id,
                    SektorID = Convert.ToInt32(ilanData["SektorID"]),
                    Pozisyon = ilanData["IlanBaslik"].ToString(),
                    Aciklama = ilanData["Aciklama"].ToString(),
                    SehirID = Convert.ToInt32(ilanData["SehirID"]),
                    IlceID = Convert.ToInt32(ilanData["IlceID"]),
                    CalismaSekli = Convert.ToInt32(ilanData["CalismaSekli"]),
                    TecrubeSeviyesi = Convert.ToInt32(ilanData["TecrubeYili"]),
                    EgitimSeviyesi = Convert.ToInt32(ilanData["EgitimSeviyesi"])
                };

                // Sektörleri getir
                var sektorlerData = SqlService.ExecuteDataTable(
                    Alias: "Default",
                    Query: "SELECT ID, Sektor, Durum FROM dbo.Sektor WHERE Durum = 1 ORDER BY Sektor"
                );

                model.Sectors = sektorlerData.Rows.Cast<DataRow>()
                    .Select(row => new SectorModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Sektor = row["Sektor"].ToString(),
                        Durum = Convert.ToInt32(row["Durum"])
                    }).ToList();

                // İlleri getir
                var illerData = SqlService.ExecuteDataTable(
                    Alias: "Default",
                    Query: @"SELECT ID, Name FROM Province WHERE CountryID = 231 ORDER BY Name"
                );

                model.Provinces = illerData.Rows.Cast<DataRow>()
                    .Select(row => new ProvinceModel
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        Name = row["Name"].ToString()
                    }).ToList();

                // İlçeleri getir
                if (model.SehirID > 0)
                {
                    var ilcelerData = SqlService.ExecuteDataTable(
                        Alias: "Default",
                        Query: @"SELECT ID, Name FROM District WHERE ProvinceID = @ProvinceID ORDER BY Name",
                        Variables: new QueryVariableCollection(
                            new QueryVariable("ProvinceID", model.SehirID)
                        )
                    );

                    model.Districts = ilcelerData.Rows.Cast<DataRow>()
                        .Select(row => new DistrictModel
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            Name = row["Name"].ToString()
                        }).ToList();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "İlan bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("JobPostings");
            }
        }

        [HttpPost]
        public IActionResult EditJobPosting([FromForm] EditJobPostingModel model)
        {
            try
            {
                var firmaId = HttpContext.Session.GetInt32("IsverenID");
                if (!firmaId.HasValue)
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı!" });

                var result = SqlService.Update(
                    Alias: "Default",
                    TableName: "Ilanlar",
                    WhereClause: "ID = @IlanID AND FirmaID = @FirmaID",
                    Fields: new Dictionary<string, object>
                    {
                        { "SektorID", model.SektorID },
                        { "IlanBaslik", model.Pozisyon },
                        { "Pozisyon", model.Pozisyon },
                        { "CalismaSekli", model.CalismaSekli },
                        { "TecrubeYili", model.TecrubeSeviyesi },
                        { "EgitimSeviyesi", model.EgitimSeviyesi },
                        { "Aciklama", model.Aciklama },
                        { "IsTanimi", model.Aciklama },
                        { "SehirID", model.SehirID },
                        { "IlceID", model.IlceID },
                        { "GuncellemeZamani", DateTime.Now }
                    },
                    Variables: new QueryVariableCollection(
                        new QueryVariable("IlanID", model.ID),
                        new QueryVariable("FirmaID", firmaId)
                    )
                );

                return Json(new { success = true, message = "İlan başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İlan güncellenirken bir hata oluştu: " + ex.Message });
            }
        }
    }
}
