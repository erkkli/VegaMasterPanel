using System.Web.Optimization;

namespace App.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/content/customLayout").Include(

                "~/content/css/font-awesome.css",
                "~/content/css/bootstrap.css",
                "~/content/css/smartadmin-production.css",
                "~/content/css/smartadmin-production-plugins.css",
                "~/content/css/araad/bootstrap-datepicker.css",

                "~/content/css/araad/Gridmvc.css",
                "~/content/css/fieldset/jquery.coolfieldset.css",

                "~/content/css/dashboard/svg-turkiye-haritasi.css",
                "~/content/css/araad/araad.css"
            ));

            bundles.Add(new StyleBundle("~/content/login").Include(
                "~/content/css/bootstrap.css",
                "~/content/css/smartadmin-production.css",
                "~/content/css/araad/araad.css"
            ));

            bundles.Add(new StyleBundle("~/content/araadharita").Include(
                "~/content/css/gis/ol.css",
                "~/content/css/gis/ol3-contextmenu.css",
                "~/content/css/gis/layerpopupcontrol.css",
                "~/content/css/gis/layerswitchercontrol.css",
                "~/content/css/gis/gis.css",
                "~/content/css/font-awesome.css",
                "~/content/css/bootstrap.css",
                "~/content/css/araad/araad.css"
                ));

            bundles.Add(new ScriptBundle("~/scripts/araadCustom").Include(
                "~/scripts/libs/jquery-2.1.1.min.js",
                "~/scripts/libs/jquery-ui-1.10.3.min.js",
                "~/scripts/app.config.js",
                "~/scripts/bootstrap/bootstrap.js",
                "~/scripts/notification/SmartNotification.min.js",
                "~/scripts/smartwidgets/jarvis.widget.min.js",
                "~/scripts/jquery.validate.js",
                "~/scripts/plugin/jquery-validate/jquery.validate.messages_tr.js",
                "~/scripts/jquery.validate.unobtrusive.js",
                "~/scripts/plugin/masked-input/jquery.maskedinput.min.js",
                "~/scripts/plugin/msie-fix/jquery.mb.browser.min.js",
                "~/scripts/plugin/fastclick/fastclick.js",
                "~/scripts/app.min.js",
                "~/scripts/gridmvc.customwidgets.js",
                "~/scripts/bootstrap/bootstrap-datepicker.js",

                ///scripts/datatables
                "~/scripts/plugin/datatables/jquery.dataTables.min.js",
                "~/scripts/plugin/datatables/dataTables.colVis.min.js",
                "~/scripts/plugin/datatables/dataTables.tableTools.min.js",
                "~/scripts/plugin/datatables/dataTables.bootstrap.min.js",
                "~/scripts/plugin/datatable-responsive/datatables.responsive.min.js",

                //scripts/forms
                "~/scripts/plugin/jquery-form/jquery-form.js",
                
                //scripts/full-calendar
                "~/scripts/plugin/moment/moment.js",
                "~/scripts/plugin/fullcalendar/jquery.fullcalendar.js",

                //scripts/araadCustom
                "~/scripts/export2excel/jquery.table2excel.js",
                "~/scripts/FileUploader/js/vendor/jquery.ui.widget.js",
                "~/scripts/FileUploader/js/jquery.iframe-transport.js",
                "~/scripts/FileUploader/js/jquery.fileupload.js",
                "~/scripts/FileUploader/js/jquery.fileupload-process.js",
                "~/scripts/jquery.tristate.js",
                "~/scripts/fieldset/jquery.coolfieldset.js",

                "~/scripts/gridmvc.js",

                "~/scripts/custom/araad/araad.js"
                ));

            bundles.Add(new ScriptBundle("~/scripts/araadharita").Include(
                "~/scripts/libs/jquery-2.1.1.min.js",
                "~/scripts/gis/ol/ol.js",
                "~/scripts/gis/proj4j.js",
                "~/scripts/gis/ol-contextmenu/ol3-contextmenu.js",
                "~/scripts/gis/ol-ext/layerswitchercontrol.js",
                "~/scripts/gis/ol-ext/layerpopupcontrol.js",
                "~/scripts/gis/FileSaver.js",
                "~/scripts/gis/AraadGisApi.js"
                ));

            bundles.Add(new ScriptBundle("~/scripts/login").Include(
                "~/scripts/libs/jquery-2.1.1.min.js",
                "~/scripts/app.config.js",
                "~/scripts/bootstrap/bootstrap.js",
                "~/scripts/notification/SmartNotification.min.js",
                "~/scripts/jquery.validate.js",
                "~/scripts/jquery.validate.unobtrusive.js",
                "~/scripts/plugin/masked-input/jquery.maskedinput.min.js",
                "~/scripts/plugin/msie-fix/jquery.mb.browser.min.js",
                "~/scripts/plugin/fastclick/fastclick.js",
                "~/scripts/app.min.js"));

            bundles.Add(new ScriptBundle("~/scripts/jquery").Include(
                "~/scripts/libs/jquery-2.1.1.min.js"
            ));

            //BundleTable.EnableOptimizations = false;
        }
    }
}