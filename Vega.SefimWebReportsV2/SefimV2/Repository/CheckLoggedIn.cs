using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SefimV2.Repository
{
    public class CheckLoggedIn : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;

            if (HttpContext.Current.Request.Cookies["VGMSTRS"] == null)
            {
                HttpContext.Current.Response.Redirect("~/Authentication/Login");
            }
            else if (context.Request.Cookies["VGMSTRS"] != null)
            {
                HttpCookie cookie = new HttpCookie("VGMSTRS", HttpContext.Current.Request.Cookies["VGMSTRS"].Value)
                {
                    Expires = DateTime.Now.AddHours(24)
                };
                HttpContext.Current.Response.Cookies.Add(cookie);

                /*Kullanıcıya tanımlanan uygulama tip*/
                if (context.Request.Cookies["ATYPE"] != null)
                {
                    HttpCookie cookieAppType = new HttpCookie("ATYPE", HttpContext.Current.Request.Cookies["ATYPE"].Value)
                    {
                        Expires = DateTime.Now.AddHours(6)
                    };
                    HttpContext.Current.Response.Cookies.Add(cookieAppType);
                }

                /*Ürün Id*/
                if (context.Request.Cookies["PRTYPE"] != null)
                {
                    HttpCookie cookiePrType = new HttpCookie("PRTYPE", HttpContext.Current.Request.Cookies["PRTYPE"].Value)
                    {
                        Expires = DateTime.Now.AddHours(6)
                    };
                    HttpContext.Current.Response.Cookies.Add(cookiePrType);
                }
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "controller", "Authentication/Login" },
                    { "action", "" }
                });
            }
            base.OnActionExecuting(filterContext);
        }
    }
}