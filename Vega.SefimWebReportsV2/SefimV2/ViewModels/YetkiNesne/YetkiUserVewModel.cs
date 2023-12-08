using System.Collections.Generic;

namespace SefimV2.ViewModels.YetkiNesneViewModel
{
    public class YetkiUserVewModel
    {
        public List<YetkiUserDetailModel> YetkiUserDetailModelList { get; set; }
        public List<YetkiMenuUser> YetkiMenuUserList { get; set; }
      
        public class YetkiUserDetailModel
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int MenuId { get; set; }
            public int IslemTipiId { get; set; }
            public int YetkiNesneId { get; set; }
            public string MenuYetkiDegeri { get; set; }
        }
        public class YetkiMenuUser
        {
            public int Id { get; set; }
            public int UstMenuId { get; set; }
            public int YetkiNesneDetayId { get; set; }
            public string Adi { get; set; }
            public string YetkiDegeri { get; set; }
            public bool CheckedYetkiMenu { get; set; }

            public List<YetkiIslemTipUser> YetkiIslemTipUserList { get; set; }
        }
        public class YetkiIslemTipUser
        {
            public int Id { get; set; }
            public int IslemTipi { get; set; }
            public string IslemTipiAdi { get; set; }
            public bool CheckedYetkiIslemTip { get; set; }
            public string MenuYetkiDegeri { get; set; }
        }
    }
}