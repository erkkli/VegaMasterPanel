using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.Result
{
    public class JsonResultModel
    {
        public bool IsSuccess { get; set; }
        public string UserMessage { get; set; }
        public int Param1 { get; set; }
        public string Path { get; set; }
        public bool isMailSent { get; set; }
        public bool MailResult { get; set; }
        public string ParamStr { get; set; }

        public DateTime GelenGun { get; set; }
        public DateTime GelenGunSonu { get; set; }
    }
}