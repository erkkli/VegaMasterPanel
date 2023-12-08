using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SefimV2
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";

            //TL ve % işaretlerinin sağda görünmesi için. 
            //https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0
            culture.NumberFormat.CurrencyNegativePattern = 1;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 1;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";

            //TL ve % işaretlerinin sağda görünmesi için.
            //https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0
            culture.NumberFormat.CurrencyNegativePattern = 1;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 1;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

        }
    }
}
