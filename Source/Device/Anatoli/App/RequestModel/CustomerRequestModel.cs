using Anatoli.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.RequestModel
{
    public class CustomerRequestModel : BaseRequestModel
    {
        public Guid regionId { get; set; }
        public Guid customerId { get; set; }
        public Guid basketId { get; set; }
        //public CustomerShipAddressViewModel customerShipAddressData { get; set; }
        public CustomerViewModel customerData { get; set; }
        //public List<BasketItemViewModel> basketItemData { get; set; }
        //public List<BasketViewModel> basketData { get; set; }
        //public List<CustomerViewModel> customerListData { get; set; }
    }
}
