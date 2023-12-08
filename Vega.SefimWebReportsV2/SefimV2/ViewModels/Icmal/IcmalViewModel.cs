using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static SefimV2.Enums.General;

namespace SefimV2.ViewModels.Icmal
{
    public class IcmalViewModel
    {
        public int Id { get; set; }
        public string SubeId { get; set; } = string.Empty;
        public string UserId { get; set; }
        public string PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
        public string PaymentDate { get; set; }

        [DisplayName("Şube Seç")]
        //[Required(ErrorMessage = "{0} seçiniz.")]
        public string SubeName { get; set; } = string.Empty;

        [DisplayName("Nakit Tutarı")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        //[RegularExpression(@"^[0-9]{1,8}([,][0-9]{1,2})?$", ErrorMessage = "{0} Geçersiz değer")]
        public decimal CashAmount { get; set; }

        [DisplayName("Kredi Kart")]
        public decimal KreditAmount { get; set; }

        [DisplayName("Yemek Kartı")]
        public decimal TicketAmount { get; set; }

        [DisplayName("Online Tutarı")]
        public decimal OnlineAmount { get; set; }

        [DisplayName("Açık Hesab Tutarı")]
        public decimal DebitAmount { get; set; }

        [DisplayName("Onay Durumu")]
        public OnayDurumu OnayDurumu { get; set; }

        public string Islem { get; set; }
        public int? Index { get; set; }
        public List<IcmalSalesTypes> IcmalSalesTypesTemp { get; set; }
        public List<IcmalSalesTypes> IcmalSalesTypesBranch { get; set; }
        public KreditPaymentBank KreditPayment { get; set; }
        public TicketPaymentTicketTypeList TicketPayment { get; set; }
        public List<KreditPaymentBank> KreditPaymentBankTypeList { get; set; }
        public List<TicketPaymentTicketTypeList> TicketPaymentTicketTypeList { get; set; }
    }

    public class KreditPaymentBankTypeList
    {
        public int IcmalKreditPaymentId { get; set; }
        public int IcmalPaymentId { get; set; }
        public int BankBkmId { get; set; }

        [DisplayName("Banka Adı")]
        public string BankName { get; set; }

        [DisplayName("Banka Seç")]
        public BankaTipi? BankaTipi { get; set; }

        [DisplayName("Tutar")]
        public decimal? Amount { get; set; }
    }
    public class TicketPaymentTicketTypeList
    {
        public int IcmalTicketPaymentId { get; set; }
        public int IcmalPaymentId { get; set; }
        public int TicketId { get; set; }

        [DisplayName("Yemek Kartı")]
        public string TicketName { get; set; }

        [DisplayName("Yemek Kartı Seç")]
        public YemekKartiTipi? YemekKartiTipi { get; set; }

        [DisplayName("Tutar")]
        public decimal? Amount { get; set; }
    }

    public class KreditPaymentBank
    {
        public int IcmalKreditPaymentId { get; set; }
        public int IcmalPaymentId { get; set; }
        public int BankBkmId { get; set; }

        [DisplayName("Banka Adı")]
        public string BankName { get; set; }

        [DisplayName("Banka Seç")]
        public BankaTipi? BankaTipi { get; set; }

        [DisplayName("Tutar")]
        public decimal? Amount { get; set; }
    }

    public class IcmalSalesTypes
    {
        public string UserId { get; set; }
        public int IcmalPaymentId { get; set; }
        public string SubeId { get; set; } = string.Empty;
        public string SubeName { get; set; } = string.Empty;
        public string PaymentDate { get; set; }

        [DisplayName("Nakit Tutarı")]
        public decimal CashAmount { get; set; }

        [DisplayName("Kredi Kart")]
        public decimal KreditAmount { get; set; }

        [DisplayName("Yemek Kartı")]
        public decimal TicketAmount { get; set; }

        [DisplayName("Online Tutarı")]
        public decimal OnlineAmount { get; set; }

        [DisplayName("Açık Hesab Tutarı")]
        public decimal DebitAmount { get; set; }

        [DisplayName("Onay Durumu")]
        public OnayDurumu OnayDurumu { get; set; }
        public bool KayitVarMi { get; set; }
    }
}