using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SefimV2.ViewModels.YetkiNesneViewModel
{
    public class YetkiNesneViewModel
    {
        public int Id { get; set; }       
        [DisplayName("Yetki Adı")]
        [StringLength(250, ErrorMessage = "URL alanı 250 karakterden uzun olamaz")]
        [Required(ErrorMessage = "Yetki Adı alanı boş bırakılamaz")]
        public string YetkiAdi { get; set; }

        [DisplayName("Açıklama")]
        [StringLength(250)]
        public string Aciklama { get; set; }

        [DisplayName("Aktif Mi")]
        public bool IsActive { get; set; }

        public List<YetkiMenu> YetkiMenuList { get; set; }
      
        public class YetkiMenu
        {
            public int Id { get; set; }
            public int UstMenuId { get; set; }
            public int YetkiNesneDetayId { get; set; }

            [DisplayName("Menü Adı")]
            public string Adi { get; set; }

            [DisplayName("Yetki Adı")]
            public string YetkiDegeri { get; set; }

            public bool CheckedYetkiMenu { get; set; }

            public List<YetkiIslemTip> YetkiIslemTipList { get; set; }
            public class YetkiIslemTip
            {
                public int Id { get; set; }
                public int IslemTipi { get; set; }

                [DisplayName("İşlem Adı")]
                public string IslemTipiAdi { get; set; }

                public bool CheckedYetkiIslemTip { get; set; }
            }
        }
    }
}