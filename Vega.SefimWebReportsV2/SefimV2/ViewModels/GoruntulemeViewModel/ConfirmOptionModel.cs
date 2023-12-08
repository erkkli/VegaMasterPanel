using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SefimV2.ViewModels.GoruntulemeViewModel
{
    public enum ModalKritiklik
    {
        [EnumMember(Value = "primary")]
        Info,
        [EnumMember(Value = "warning")]
        Warning,
        [EnumMember(Value = "danger")]
        Danger,
        [EnumMember(Value = "success")]
        Success
    };
    public class ConfirmOptionModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ModalKritiklik ModalKritiklik { get; set; }

        public string Baslik { get; set; }
        public string Mesaj { get; set; }
        public string OnayButonMetni { get; set; }
        public string RedirectURL { get; set; }
        public bool UseAjax { get; set; }
        public bool VazgecButonuGosterme { get; set; }

        public string GetJSON()
        {
            string result = JsonConvert.SerializeObject(this);
            return result;
        }
    }
}