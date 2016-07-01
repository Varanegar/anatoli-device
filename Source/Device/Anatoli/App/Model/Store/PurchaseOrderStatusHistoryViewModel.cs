using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class PurchaseOrderStatusHistoryViewModel : BaseModel
    {
        private string actionTimeString = "";
        public  const string PreInvoice = "5C0D43FC-6822-4D39-AB40-363B885BE464";
        public  const string WaitForDelivery = "EA5961AB-792A-4D20-8A52-5501F01F034A";
        public  const string Cleared = "D12DEEE1-DA5B-44F6-937A-7B282789908F";
        public  const string Delivered = "3AA22CED-A45E-4E58-B992-A4B1F838B19B";
        public  const string Request = "A591658A-E46B-440D-9ADB-E3E5B01B7489";
        public  const string InQueue = "A0418ABD-941C-4826-8063-E82B4A5D48FE";
        public Guid StatusId { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionPDate { get; set; }
        public TimeSpan ActionTime { get; set; }
        public string ActionTimeString
        {
            get { return actionTimeString; }
            set
            {
                actionTimeString = value;
                ActionTime = TimeSpan.Parse(value);
            }
        }
        public string Comment { get; set; }
        public Guid PurchaseOrderId { get; set; }

        public static string GetStatusName(string statusId)
        {
            string status = "نا مشخص";
            switch (statusId)
            {
                case PreInvoice:
                    status = "صدور پیش فاکتور";
                    break;
                case WaitForDelivery:
                    status = "ارسال شده";
                    break;
                case Cleared:
                    status = "تسویه شده";
                    break;
                case Delivered:
                    status = "تحویل شده";
                    break;
                case Request:
                    status = "درخواست";
                    break;
                case InQueue:
                    status = "در صف ارسال";
                    break;
                default:
                    break;

            }
            return status;
        }
    }

}
