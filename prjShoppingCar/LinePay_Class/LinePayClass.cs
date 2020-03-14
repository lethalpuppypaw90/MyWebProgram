using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace prjShoppingCar.LinePay_Class
{    
    public class LienPayRequest
    {
        public int amount { get; set; }
        public string productImageUrl { get; set; }
        public string confirmUrl { get; set; }
        public string productName { get; set; }
        public string orderId { get; set; }
        public string currency { get; set; }
    }

    public class PaymentUrl
    {
        public string web { get; set; }
        public string app { get; set; }
    }

    public class Info
    {
        public PaymentUrl paymentUrl { get; set; }
        public long transactionId { get; set; }
        public string paymentAccessToken { get; set; }
    }

    public class RootObject
    {
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
        public Info info { get; set; }
    }

    public class ConfirmPayInfo
    {
        public string method { get; set; }
        public int amount { get; set; }
        public string maskedCreditCardNumber { get; set; }
    }

    public class ConfirmInfo
    {
        public long transactionId { get; set; }
        public string orderId { get; set; }
        public List<ConfirmPayInfo> payInfo { get; set; }
    }

    public class ConfirmObject
    {
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
        public ConfirmInfo info { get; set; }
    }

    public class ShoppingCarDetail
    {
        public string fReceiver { get; set; }
        public string fEmail { get; set; }
        public string fAddress { get; set; }
        public string orderId { get; set; }
    }
}