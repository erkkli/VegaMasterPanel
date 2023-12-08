using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.Models.License
{
    public class LisansViewModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string productID { get; set; }
        public string companyCode { get; set; }
        public string licenceKey { get; set; }

        public bool IsSuccess { get; set; }
        public string UserMessage { get; set; }

    }
}