using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Models.License;
using SefimV2.ViewModels.Result;
using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using vrlibwin.Model.License;

namespace SefimV2.Controllers
{
    public class AuthenticationController : BaseController
    {
        private static string uygulamaTipi = WebConfigurationManager.AppSettings["UygulamaTipi"];

        #region (POST) USER LOGIN AND LISANS CONTROL 

        public ActionResult Login()
        {
            var user = new AdminViewModel()
            {
                ID = 0,
                Password = "",
                UserName = "",
                ResultBOOL = true,
                ResultSTRING = "",
                UygulamaTipi = uygulamaTipi,
            };

            return View(user);
        }

        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult LoginAction(AdminViewModel viewModel)
        {
            //Session["sessionName"] = "sessionValue";
            var result = new JsonResultModel()
            {
                IsSuccess = false,
                UserMessage = "İşlem yapılırken bir hata oluştu. Lütfen bilgilerinizi kontrol ediniz."
            };

            #region ***LISANS CONTROL (NEW)***

            var resultLicense = LicenseControl(viewModel.UygulamaTipi);
            if (!resultLicense.IsSuccess)
            {
                return SendErrorAfterRedirect(resultLicense.UserMessage, "LisansCreate", "Authentication");
            }

            #endregion ***LISANS CONTROL (NEW)*** 

            #region USER LOGIN CONTROL           
            if (viewModel != null || (!string.IsNullOrEmpty(viewModel.UserName) && !string.IsNullOrEmpty(viewModel.Password)))
            {
                if (!string.IsNullOrEmpty(viewModel.UserName) && !string.IsNullOrEmpty(viewModel.Password))
                {
                    var userLogin = Ayarlar.GetUserInfo(viewModel.UserName, viewModel.Password, resultLicense.Param1);
                    //var userLogin = Ayarlar.GetUserInfo(viewModel.UserName, viewModel.Password,139);
                    if (viewModel.UserName == userLogin.UserName && viewModel.Password == userLogin.Password)
                    {
                        try
                        {
                            result.IsSuccess = true;
                            result.UserMessage = "Başarılı";
                            viewModel.ResultBOOL = true;

                            //VegaMaster = 1,
                            //SefimPanel = 2,
                            //VegaMasterSefimPanel = 3,
                            if (userLogin.UygulamaTipi == "1")
                            {
                                return RedirectToAction("Index", "Dashboard");
                            }
                            else if (userLogin.UygulamaTipi == "2")
                            {
                                return RedirectToAction("Main", "SefimPanelHome");
                            }
                            else if (userLogin.UygulamaTipi == "3")
                            {
                                return RedirectToAction("Index", "Dashboard");
                            }
                        }
                        catch (Exception ex)
                        {
                            Singleton.WritingLogFile("LoginAction", ex.Message.ToString());
                            result.IsSuccess = false;
                            result.UserMessage = ex.Message;
                            viewModel.ResultBOOL = false;
                        }
                    }
                    else
                    {
                        result.IsSuccess = false;
                        viewModel.ResultBOOL = false;
                        result.UserMessage = "Lütfen Kullanıcı Adı veya Şifre bilgilerinizi kontrol ediniz.";
                        ModelState.AddModelError("UserName", "Lütfen Kullanıcı Adı veya Şifre bilgilerinizi kontrol ediniz.");
                        return SendErrorAfterRedirect(result.UserMessage, "Login", "Authentication");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(viewModel.UserName))
                    {
                        result.UserMessage += "Kullanıcı Adı bilginizi giriniz";
                    }
                    if (string.IsNullOrEmpty(viewModel.Password))
                    {
                        result.UserMessage += "<br/> Şİfre bilginizi giriniz";
                    }
                    return SendErrorAfterRedirect(result.UserMessage, "Login", "Authentication");
                }
            }
            else
            {
                Singleton.WritingLog("LoginAction", "User Name:" + viewModel.UserName + " ViewModel NULL");
                return SendWarningAfterRedirect("Kullanıcı bilgilerini kontrol ediniz.", "Login", "Authentication");
            }

            #endregion USER LOGIN CONTROL

            return SendWarningAfterRedirect(result.UserMessage, "Login", "Authentication");
        }

        #endregion (POST) USER LOGIN AND LISAN CONTROL

        #region LİSANS POST 

        public JsonResultModel LicenseControl(string AppType)
        {
            var result = new JsonResultModel() { IsSuccess = true, UserMessage = "Lisansı vardır." };
            try
            {
                if (AppType == "1")
                {
                    /*Master*/
                    #region Şefim Panel lisans kontrol

                    var licenseStatusResultSefimPanel = SefimUtil.LicenseManager.LicenseCotnrol("139");
                    if (licenseStatusResultSefimPanel.Status == false)
                    {
                        #region cookies
                        if (HttpContext.Request.Cookies["PRAUT"] != null)
                        {
                            HttpCookie currentUserCookie = HttpContext.Request.Cookies["PRAUT"];
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                            currentUserCookie.Expires = DateTime.Now.AddYears(-1);
                            currentUserCookie.Value = null;
                            System.Web.HttpContext.Current.Response.Cookies["PRAUT"].Value = null;
                            System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                        }
                        #endregion cookies

                        result.Param1 = 5;
                        result.IsSuccess = false;
                        if (licenseStatusResultSefimPanel.Message != "")
                        {
                            result.UserMessage = licenseStatusResultSefimPanel.Message;
                            return result;
                        }
                        else
                        {
                            result.UserMessage = "Vega Master Lisansınız Bulunmamaktadır.";
                            return result;
                        }
                    }

                    result.Param1 = 139;

                    #endregion Şefim Panel lisans kontrol
                }
                else if (AppType == "3")
                {
                    #region Şefim Panel lisans kontrol

                    var licenseStatusResultSefimPanel = SefimUtil.LicenseManager.LicenseCotnrol("180");
                    if (licenseStatusResultSefimPanel.Status == false)
                    {
                        #region cookies
                        if (HttpContext.Request.Cookies["PRAUT"] != null)
                        {
                            HttpCookie currentUserCookie = HttpContext.Request.Cookies["PRAUT"];
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                            currentUserCookie.Expires = DateTime.Now.AddYears(-1);
                            currentUserCookie.Value = null;
                            System.Web.HttpContext.Current.Response.Cookies["PRAUT"].Value = null;
                            System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                        }
                        #endregion cookies

                        result.Param1 = 5;
                        result.IsSuccess = false;
                        if (licenseStatusResultSefimPanel.Message != "")
                        {
                            result.UserMessage = licenseStatusResultSefimPanel.Message;
                            return result;
                        }
                        else
                        {
                            result.UserMessage = "Şefim Panel Lisansınız Bulunmamaktadır";
                            return result;
                        }
                    }

                    result.Param1 = 180;

                    #endregion Şefim Panel lisans kontrol
                }
                else
                {
                    /*Master*/
                    #region Şefim Panel lisans kontrol

                    var licenseStatusResultSefimPanel = SefimUtil.LicenseManager.LicenseCotnrol("139");
                    if (licenseStatusResultSefimPanel.Status == false)
                    {
                        #region cookies
                        if (HttpContext.Request.Cookies["PRAUT"] != null)
                        {
                            HttpCookie currentUserCookie = HttpContext.Request.Cookies["PRAUT"];
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                            currentUserCookie.Expires = DateTime.Now.AddYears(-1);
                            currentUserCookie.Value = null;
                            System.Web.HttpContext.Current.Response.Cookies["PRAUT"].Value = null;
                            System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                            System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                        }
                        #endregion cookies

                        //Singleton.WritingLogFile("LicenseControlMessage:", licenseStatusResultSefimPanel.Message.ToString());
                        result.Param1 = 5;
                        result.IsSuccess = false;
                        if (licenseStatusResultSefimPanel.Message != "")
                        {
                            result.UserMessage = licenseStatusResultSefimPanel.Message;
                            return result;
                        }
                        else
                        {
                            result.UserMessage = "Vega Master Lisansınız Bulunmamaktadır.";
                            return result;
                        }
                    }

                    result.Param1 = 139;

                    #endregion Şefim Panel lisans kontrol
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("LicenseControlException:", ex.Message.ToString());

                result.IsSuccess = false;
                result.UserMessage = "Lisans Kontrol Edilemedi";
                return result;
            }


            return result;
        }

        public ActionResult LisansCreate(string Id)
        {
            LisansViewModel viewModel = new LisansViewModel
            {
                IsSuccess = true,
                productID = Id
            };
            return View(viewModel);
        }

        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult LisansCreate(LisansViewModel course)
        {
            ActionResultMessages result = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(course.username) || string.IsNullOrEmpty(course.password) || string.IsNullOrEmpty(course.companyCode) || string.IsNullOrEmpty(course.licenceKey))
            {
                result.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                result.IsSuccess = false;
                course.IsSuccess = false;
                course.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";
                //return View(course);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            #endregion

            if (uygulamaTipi == "3")
            {//SefimPanel
                course.productID = "180";
            }
            else if (uygulamaTipi == "139")
            {//VegaMaster
                course.productID = "139";
            }
            else
            {
                //VegaMaster
                course.productID = "139";
            }

            //course.productID = "139";
            var resultData = SefimUtil.LicenseManager.LicenseRegister(course.username, course.password, course.companyCode, course.licenceKey, course.productID);
            LicenseStatusResult licenseStatusResult;
            licenseStatusResult = new LicenseStatusResult();
            result.IsSuccess = Convert.ToBoolean(resultData.Status);
            result.UserMessage = resultData.Message;
            course.IsSuccess = Convert.ToBoolean(resultData.Status);
            course.UserMessage = resultData.Message;

            if (resultData.Status)
            {
                licenseStatusResult = SefimUtil.LicenseManager.LicenseCotnrol(course.productID);
                Dispose();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion LİSANS POST

        #region LOGOUT  

        [HttpPost]
        public JsonResult Logout()
        {
            var result = new JsonResultModel();

            if (HttpContext.Request.Cookies["PRAUT"] != null)
            {
                HttpCookie currentUserCookie = HttpContext.Request.Cookies["PRAUT"];
                System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
                currentUserCookie.Expires = DateTime.Now.AddYears(-1);
                currentUserCookie.Value = null;
                System.Web.HttpContext.Current.Response.Cookies["PRAUT"].Value = null;
                System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                System.Web.HttpContext.Current.Response.Cookies.Remove("PRAUT");
            }

            //Kişiye Atanmış Uygulama Tipini Tutar.
            if (HttpContext.Request.Cookies["ATYPE"] != null)
            {
                HttpCookie currentUserCookie = HttpContext.Request.Cookies["ATYPE"];
                System.Web.HttpContext.Current.Response.Cookies.Remove("ATYPE");
                currentUserCookie.Expires = DateTime.Now.AddYears(-3);
                currentUserCookie.Value = null;
                System.Web.HttpContext.Current.Response.Cookies["ATYPE"].Value = null;
                System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                System.Web.HttpContext.Current.Response.Cookies.Remove("ATYPE");
                if (System.Web.HttpContext.Current.Response.Cookies["ATYPE"] != null)
                {
                    System.Web.HttpContext.Current.Response.Cookies["ATYPE"].Value = null;
                    System.Web.HttpContext.Current.Response.Cookies["ATYPE"].Expires = DateTime.Now.AddMonths(-1);
                }
                result.IsSuccess = true;
            }

            //ÜrünTipi
            if (HttpContext.Request.Cookies["PRTYPE"] != null)
            {
                HttpCookie currentUserCookie = HttpContext.Request.Cookies["PRTYPE"];
                System.Web.HttpContext.Current.Response.Cookies.Remove("PRTYPE");
                currentUserCookie.Expires = DateTime.Now.AddYears(-3);
                currentUserCookie.Value = null;
                System.Web.HttpContext.Current.Response.Cookies["PRTYPE"].Value = null;
                System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                System.Web.HttpContext.Current.Response.Cookies.Remove("PRTYPE");
                if (System.Web.HttpContext.Current.Response.Cookies["PRTYPE"] != null)
                {
                    System.Web.HttpContext.Current.Response.Cookies["PRTYPE"].Value = null;
                    System.Web.HttpContext.Current.Response.Cookies["PRTYPE"].Expires = DateTime.Now.AddMonths(-1);
                }
                result.IsSuccess = true;
            }

            //Session.Abandon();
            //Session.Clear();
            if (HttpContext.Request.Cookies["VGMSTRS"] != null)
            {
                HttpCookie currentUserCookie = HttpContext.Request.Cookies["VGMSTRS"];
                //HttpContext.Response.Cookies.Remove("VGMSTR");
                System.Web.HttpContext.Current.Response.Cookies.Remove("VGMSTRS");
                currentUserCookie.Expires = DateTime.Now.AddYears(-3);
                currentUserCookie.Value = null;
                System.Web.HttpContext.Current.Response.Cookies["VGMSTRS"].Value = null;
                //HttpContext.Response.SetCookie(currentUserCookie);
                System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);
                System.Web.HttpContext.Current.Response.Cookies.Remove("VGMSTRS");

                if (System.Web.HttpContext.Current.Response.Cookies["VGMSTRS"] != null)
                {
                    System.Web.HttpContext.Current.Response.Cookies["VGMSTRS"].Value = null;
                    System.Web.HttpContext.Current.Response.Cookies["VGMSTRS"].Expires = DateTime.Now.AddMonths(-1);
                }
                result.IsSuccess = true;
            }


            if (result.IsSuccess == true)
            {
                Redirect("/Authentication/Login");
                result.IsSuccess = true;
            }
            //return this.RedirectToAction("Login", "Authentication");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion LOGOUT
    }
}