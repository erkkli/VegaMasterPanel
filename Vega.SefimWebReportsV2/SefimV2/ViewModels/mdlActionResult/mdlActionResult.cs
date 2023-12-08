using System.Collections.Generic;

namespace SefimV2.Models
{
    public class ActionResultMessages
    {
        public string result_STR { get; set; }
        public int result_INT { get; set; }
        public bool result_BOOL { get; set; }
        public object result_OBJECT { get; set; }


        public bool IsSuccess { get; set; }
        public string UserMessage { get; set; }
        public int Param1 { get; set; }
        public string Path { get; set; }
        public bool isMailSent { get; set; }
        public bool MailResult { get; set; }
        public string ParamStr { get; set; }
        public List<string> ErrorList { get; set; }
        public List<string> SuccessAlertList { get; set; }
    }
}