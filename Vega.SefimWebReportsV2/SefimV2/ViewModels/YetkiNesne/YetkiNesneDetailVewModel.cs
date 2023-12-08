using System.Collections.Generic;

namespace SefimV2.ViewModels.YetkiNesneViewModel
{
    public class YetkiNesneDetailVewModel
    {            
        public List<YetkiNesneDetail> YetkiNesneDetailList { get; set; }
    
        public class YetkiNesneDetail
        {
            public int Id { get; set; }
            public int YetkiNesneId { get; set; }
            public int MenuId { get; set; }
            public int IslemTipiId { get; set; }
        }
    }
}