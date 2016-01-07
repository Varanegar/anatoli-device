using System;
using System.Linq;
using Anatoli.DataAccess.Models;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using Anatoli.DataAccess.Models.Identity;

namespace Anatoli.DataAccess.Configs
{
    public class StockConfig : EntityTypeConfiguration<Stock>
    {
        public StockConfig()
        {
            this.HasMany<StockProduct>(pp => pp.StockProducts)
                .WithRequired(p => p.Stock)
                .WillCascadeOnDelete(false);
            this.HasMany<StockOnHandSync>(pp => pp.StockOnHandSyncs)
                .WithRequired(p => p.Stock)
                .WillCascadeOnDelete(false);
            this.HasMany<StockProductRequest>(pp => pp.StockProductRequests)
                .WithRequired(p => p.Stock)
                .WillCascadeOnDelete(false);

            this.HasOptional<Principal>(p => p.Accept1By);
            this.HasOptional<Principal>(p => p.Accept2By);
            this.HasOptional<Principal>(p => p.Accept3By);
        }
    }
}