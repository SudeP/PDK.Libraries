using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeDPayment
{
    public class Payment
    {
        public IPaymentProvider Provider { get; private set; }
        public Payment(BankNames bankNames)
        {
            switch (bankNames)
            {
                case BankNames.AkBank:
                case BankNames.IsBankasi:
                case BankNames.HalkBank:
                case BankNames.ZiraatBankasi:
                case BankNames.TurkEkonomiBankasi:
                case BankNames.IngBank:
                case BankNames.TurkiyeFinans:
                case BankNames.AnadoluBank:
                case BankNames.HSBC:
                case BankNames.SekerBank:
                    Provider = new NestPayPaymentProvider();
                    break;
                case BankNames.DenizBank:
                    Provider = new DenizbankPaymentProvider();
                    break;
                case BankNames.FinansBank:
                    Provider = new FinansbankPaymentProvider();
                    break;
                case BankNames.Garanti:
                    Provider = new GarantiPaymentProvider();
                    break;
                case BankNames.KuveytTurk:
                    Provider = new KuveytTurkPaymentProvider();
                    break;
                case BankNames.VakifBank:
                    Provider = new VakifbankPaymentProvider();
                    break;
                case BankNames.Yapikredi:
                case BankNames.Albaraka:
                    Provider = new PosnetPaymentProvider();
                    break;
            }
        }
        public string CreatePaymentFormHtml(Dictionary<string, object> parameters, Uri actionUrl, bool appendSubmitScript = true)
        {
            if (parameters == null || !parameters.Any())
                throw new ArgumentNullException(nameof(parameters));

            if (actionUrl == null)
                throw new ArgumentNullException(nameof(actionUrl));

            string formId = "PaymentForm";
            StringBuilder formBuilder = new StringBuilder();
            formBuilder.Append($"<form id=\"{formId}\" name=\"{formId}\" action=\"{actionUrl}\" role=\"form\" method=\"POST\">");

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                formBuilder.Append($"<input type=\"hidden\" name=\"{parameter.Key}\" value=\"{parameter.Value}\">");
            }

            formBuilder.Append("</form>");

            if (appendSubmitScript)
            {
                StringBuilder scriptBuilder = new StringBuilder();
                scriptBuilder.Append("<script>");
                scriptBuilder.Append($"document.{formId}.submit();");
                scriptBuilder.Append("</script>");
                formBuilder.Append(scriptBuilder.ToString());
            }

            return formBuilder.ToString();
        }
    }
}