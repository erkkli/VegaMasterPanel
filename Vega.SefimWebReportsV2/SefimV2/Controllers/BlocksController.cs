using SefimV2.ViewModels.YetkiNesneViewModel;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class BlocksController : Controller
    {
        public PartialViewResult _LeftBlock(YetkiUserVewModel maindata)
        {
            return PartialView(maindata);
        }

        //public PartialViewResult _HeaderBlock(UserMainData  maindata)
        //{
        //    return PartialView(maindata);
        //}

        //public PartialViewResult _RightBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}
        //public PartialViewResult _ThemeBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}


        //public PartialViewResult _NotificationBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}

        //public PartialViewResult _FooterBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}
        //public PartialViewResult _QuickBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}
        //public PartialViewResult _QuickNavBlock(UserMainData maindata)
        //{
        //    return PartialView(maindata);
        //}
    }
}