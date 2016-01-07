using System;
using System.Linq;
using Anatoli.DataAccess.Models;
using System.Collections.Generic;
using Anatoli.Business.Proxy.Interfaces;
using Anatoli.DataAccess.Models.Identity;
using Anatoli.ViewModels.BaseModels;
using Anatoli.ViewModels.StockModels;

namespace Anatoli.Business.Proxy.Concretes.StockActiveOnHandConcretes
{
    public class StockActiveOnHandProxy : AnatoliProxy<StockActiveOnHand, StockActiveOnHandViewModel>, IAnatoliProxy<StockActiveOnHand, StockActiveOnHandViewModel>
    {
        public override StockActiveOnHandViewModel Convert(StockActiveOnHand data)
        {
            return new StockActiveOnHandViewModel
            {
                ID = data.Number_ID,
                UniqueId = data.Id,
                PrivateOwnerId = data.PrivateLabelOwner.Id,

            };
        }

        public override StockActiveOnHand ReverseConvert(StockActiveOnHandViewModel data)
        {
            return new StockActiveOnHand
            {
                Number_ID = data.ID,
                Id = data.UniqueId,
                PrivateLabelOwner = new Principal { Id = data.PrivateOwnerId },

            
            };
        }
    }
}