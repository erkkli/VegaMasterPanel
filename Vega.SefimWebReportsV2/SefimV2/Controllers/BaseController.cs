using SefimV2.Models;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public ActionResult Index()
        {
            return View();
        }
       
        /// <summary>
        /// Mesaj gönderilen ekran üzerinden yönlendirme yapılacak ise kullanılır
        /// Önce mesaj gösterir sonra yönlendirme yapar
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="redirectInSeconds"></param>
        /// <returns></returns>
        protected dynamic SendNotification(string message)
        {
            TempData["SuccessMessage"] = new SuccessErrorViewModel { Message = message }; ;
            return TempData["SuccessMessage"];
        }

        /// <summary>
        /// Geçiş yapılan ekran üzerinde mesaj göstermek için kullanılır
        /// Önce yönlendirme yapar sonra mesaj gösterir
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected ActionResult SendNotificationAfterRedirect(string message, string actionName, string controllerName, object routeValues = null)
        {
            string url;

            if (routeValues == null)
            {
                url = Url.Action(actionName, controllerName);
            }
            else
            {
                url = Url.Action(actionName, controllerName, routeValues);
            }

            return SendNotificationAfterRedirect(message, url);
        }

        protected ActionResult SendNotificationAfterRedirect(string message, string url)
        {
            TempData["SuccessMessage"] = new SuccessErrorViewModel { Message = message };
            return new RedirectResult(url);
        }

        /// <summary>
        /// ajax get veya post ile çağırılan methodlardan sonra yönlendirme ve mesaj gösterme için kullanılır
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected string SendNotificationAfterAjaxRedirect(string message, string actionName, string controllerName, object routeValues = null)
        {
            TempData["SuccessMessage"] = new SuccessErrorViewModel { Message = message };

            if (routeValues == null)
                return Url.Action(actionName, controllerName);
            else
                return Url.Action(actionName, controllerName, routeValues);
        }

        /// <summary>
        /// Mesaj gönderilen ekran üzerinden yönlendirme yapılacak ise kullanılır
        /// Önce mesaj gösterir sonra yönlendirme yapar
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <param name="redirectInSeconds"></param>
        /// <returns></returns>
        protected dynamic SendError(string message)
        {
            TempData["ErrorMessage"] = new SuccessErrorViewModel { Message = message };
            return TempData["ErrorMessage"];
        }

        protected dynamic SendWarning(string message)
        {
            TempData["WarningMessage"] = new SuccessErrorViewModel { Message = message };
            return TempData["WarningMessage"];
        }

        /// <summary>
        /// Geçiş yapılan ekran üzerinde mesaj göstermek için kullanılır
        /// Önce yönlendirme yapar sonra mesaj gösterir
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected ActionResult SendErrorAfterRedirect(string message, string actionName, string controllerName, object routeValues = null)
        {
            string url;

            if (routeValues == null)
                url = Url.Action(actionName, controllerName);
            else
                url = Url.Action(actionName, controllerName, routeValues);

            return SendErrorAfterRedirect(message, url);
        }

        protected ActionResult SendErrorAfterRedirect(string message, string url)
        {
            TempData["ErrorMessage"] = new SuccessErrorViewModel { Message = message };
            return new RedirectResult(url);
        }

        /// <summary>
        /// Geçiş yapılan ekran üzerinde mesaj göstermek için kullanılır
        /// Önce yönlendirme yapar sonra mesaj gösterir
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected ActionResult SendWarningAfterRedirect(string message, string actionName, string controllerName, object routeValues = null)
        {
            TempData["WarningMessage"] = new SuccessErrorViewModel { Message = message };

            if (routeValues == null)
                return new RedirectResult(Url.Action(actionName, controllerName));
            else
                return new RedirectResult(Url.Action(actionName, controllerName, routeValues));
        }

        /// <summary>
        /// ajax get veya post ile çağırılan methodlardan sonra yönlendirme ve mesaj gösterme için kullanılır
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        protected string SendErrorAfterAjaxRedirect(string message, string actionName, string controllerName, object routeValues = null)
        {
            TempData["ErrorMessage"] = new SuccessErrorViewModel { Message = message };

            if (routeValues == null)
                return Url.Action(actionName, controllerName);
            else
                return Url.Action(actionName, controllerName, routeValues);
        }

        //protected PreviousPage GetPreviousPage()
        //{
        //    var paths = Request.UrlReferrer.AbsolutePath.ToString().Split('/');
        //    var controllerName = "";
        //    var actionName = "";

        //    if (paths.Length >= 3)
        //    {
        //        controllerName = paths[1];
        //        actionName = paths[2];
        //    }

        //    var collection = HttpUtility.ParseQueryString(Request.UrlReferrer.Query);

        //    return new PreviousPage()
        //    {
        //        ActionName = actionName,
        //        ControllerName = controllerName,
        //        Parameters = collection
        //    };
        //}
        //public NameValueCollection GetParametersFromEncryptedUrl()
        //{
        //    string q;
        //    if (Request.Headers["CurrentUrl"] != null)
        //        q = HttpUtility.ParseQueryString(new Uri(Request.Headers["CurrentUrl"]).Query)["q"];
        //    else
        //        q = HttpUtility.ParseQueryString(Request.UrlReferrer.Query)["q"];

        //    string desQueryString = Decrypt(q);

        //    return HttpUtility.ParseQueryString(desQueryString);
        //}
        ///// <summary>
        ///// Sadece query şifrelemesi için kullanılır. Veritabanına atılacak verilerin şifrelenmesinde asla kullanılmaz
        ///// </summary>
        ///// <param name="text"></param>
        ///// <returns></returns>
        //public string Encrypt(string text)
        //{
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        IEncryptString _encrypter = new ConfigurationBasedStringEncrypter();
        //        var encryptedValue = _encrypter.Encrypt(text);
        //        var encodedValue = HttpUtility.HtmlEncode(HttpUtility.UrlEncode(encryptedValue));
        //        return encodedValue;
        //    }
        //    else
        //    {
        //        return text;
        //    }
        //}
        //public string Decrypt(string text)
        //{
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        IEncryptString _decrypter = new ConfigurationBasedStringEncrypter();
        //        return _decrypter.Decrypt(HttpUtility.HtmlDecode(HttpUtility.UrlDecode(text)));
        //    }
        //    else
        //    {
        //        return text;
        //    }
        //}

        //public ActionResult Error(string errorMessage = ConstantMessages.SayfaGoruntulemeYetkinizYok, string header = "Hata", bool isPartial = false)
        //{
        //    ViewBag.ErrorMessage = errorMessage;
        //    ViewBag.Header = header;
        //    ViewBag.IsPartial = isPartial;
        //    if (isPartial)
        //    {
        //        return PartialView("_Error");
        //    }
        //    else
        //    {
        //        return View("_Error"); ;
        //    }
        //}


    }
}