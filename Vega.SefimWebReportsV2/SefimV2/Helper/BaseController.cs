using SefimV2.Models;
using System.Web.Mvc;
using System.Web.Routing;

namespace SefimV2.Helper
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            if (Session["mdlAdmin"] == null)
            {
                ctx.Result = new RedirectToRouteResult(
                                 new RouteValueDictionary(new { controller = "Authentication", action = "Login" })
                             );
                return;
            }          
            AdminViewModel admin = (AdminViewModel)Session["mdlAdmin"];
            if (admin.ResultBOOL==false)
            {
                ctx.Result = new RedirectToRouteResult(
                                 new RouteValueDictionary(new { controller = "Authentication", action = "Login" })
                             );
                return;
            }            
        }
    }
}