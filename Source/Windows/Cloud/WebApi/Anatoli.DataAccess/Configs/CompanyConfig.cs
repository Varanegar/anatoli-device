using System;
using System.Linq;
using Anatoli.DataAccess.Models;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;

namespace Anatoli.DataAccess.Configs
{
    public class CompanyConfig : EntityTypeConfiguration<Company>
    {
        public CompanyConfig()
        {
            this.HasMany<Stock>(csa => csa.Stocks)
                .WithOptional(c => c.Company)
                .WillCascadeOnDelete(false);

            this.HasMany<Store>(csa => csa.Stores)
                .WithOptional(c => c.Company)
                .WillCascadeOnDelete(false);

            this.HasMany<DistCompanyCenter>(csa => csa.DistCompanyCenters)
                .WithRequired(c => c.Company)
                .WillCascadeOnDelete(false);

            this.HasMany<CustomerNotVerified>(csa => csa.CustomerNotVerifieds)
                .WithRequired(c => c.Company)
                .WillCascadeOnDelete(false);

        }
    }
}