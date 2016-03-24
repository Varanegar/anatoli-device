﻿using System;
using System.Linq;
using Anatoli.Business.Proxy;
using System.Threading.Tasks;
using Anatoli.DataAccess.Models;
using System.Collections.Generic;
using Anatoli.DataAccess.Interfaces;
using Anatoli.DataAccess.Repositories;
using Anatoli.Business.Proxy.Interfaces;
using Anatoli.DataAccess;
using Anatoli.ViewModels.ProductModels;
using Anatoli.ViewModels.StockModels;

namespace Anatoli.Business.Domain
{
    public class StockHistoryOnHandDomain : BusinessDomainV2<StockHistoryOnHand, StockHistoryOnHandViewModel, StockHistoryOnHandRepository, IStockHistoryOnHandRepository>, IBusinessDomainV2<StockHistoryOnHand, StockHistoryOnHandViewModel>
    {
        #region Properties
        #endregion

        #region Ctors
        public StockHistoryOnHandDomain(Guid applicationOwnerKey, Guid dataOwnerKey, Guid dataOwnerCenterKey)
            : this(applicationOwnerKey, dataOwnerKey, dataOwnerCenterKey, new AnatoliDbContext())
        {

        }
        public StockHistoryOnHandDomain(Guid applicationOwnerKey, Guid dataOwnerKey, Guid dataOwnerCenterKey, AnatoliDbContext dbc)
            : base(applicationOwnerKey, dataOwnerKey, dataOwnerCenterKey, dbc)
        {
        }
        #endregion

        #region Methods
        public override async Task PublishAsync(List<StockHistoryOnHand> dataViewModels)
        {
            try
            {
                throw new NotImplementedException();

            }
            catch(Exception ex)
            {
                Logger.Error("PublishAsync", ex);
                throw ex;
            }
        }
        #endregion
    }
}
