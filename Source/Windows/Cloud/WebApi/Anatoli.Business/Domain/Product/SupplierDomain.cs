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

namespace Anatoli.Business.Domain
{
    public class SupplierDomain : BusinessDomain<SupplierViewModel>, IBusinessDomain<Supplier, SupplierViewModel>
    {
        #region Properties
        public IAnatoliProxy<Supplier, SupplierViewModel> Proxy { get; set; }
        public IRepository<Supplier> Repository { get; set; }
        public IPrincipalRepository PrincipalRepository { get; set; }
        public Guid PrivateLabelOwnerId { get; private set; }

        #endregion

        #region Ctors
        SupplierDomain() { }
        public SupplierDomain(Guid privateLabelOwnerId) : this(privateLabelOwnerId, new AnatoliDbContext()) { }
        public SupplierDomain(Guid privateLabelOwnerId, AnatoliDbContext dbc)
            : this(new SupplierRepository(dbc), new PrincipalRepository(dbc), AnatoliProxy<Supplier, SupplierViewModel>.Create())
        {
            PrivateLabelOwnerId = privateLabelOwnerId;
        }

        public SupplierDomain(ISupplierRepository supplierRepository, IPrincipalRepository principalRepository, IAnatoliProxy<Supplier, SupplierViewModel> proxy)
        {
            Proxy = proxy;
            Repository = supplierRepository;
            PrincipalRepository = principalRepository;
        }
        #endregion

        #region Methods
        public async Task<List<SupplierViewModel>> FilterSuppliersAsync(string searchTerm)
        {
            var suppliers = await Repository.GetFromCachedAsync(p => p.SupplierName.Contains(searchTerm));

            return Proxy.Convert(suppliers.ToList()); ;
        }

        public async Task<List<SupplierViewModel>> GetAll()
        {
            var suppliers = await Repository.FindAllAsync(p => p.PrivateLabelOwner.Id == PrivateLabelOwnerId);

            return Proxy.Convert(suppliers.ToList()); ;
        }

        public async Task<List<SupplierViewModel>> GetAllChangedAfter(DateTime selectedDate)
        {
            var suppliers = await Repository.FindAllAsync(p => p.PrivateLabelOwner.Id == PrivateLabelOwnerId && p.LastUpdate >= selectedDate);

            return Proxy.Convert(suppliers.ToList()); ;
        }

        public async Task<List<SupplierViewModel>> PublishAsync(List<SupplierViewModel> dataViewModels)
        {
            try
            {
                var suppliers = Proxy.ReverseConvert(dataViewModels);
                var privateLabelOwner = PrincipalRepository.GetQuery().Where(p => p.Id == PrivateLabelOwnerId).FirstOrDefault();

                suppliers.ForEach(item =>
                {
                    item.PrivateLabelOwner = privateLabelOwner ?? item.PrivateLabelOwner;
                    var currentSupplier = Repository.GetQuery().Where(p => p.Id == item.Id).FirstOrDefault();
                    if (currentSupplier != null)
                    {
                        if (currentSupplier.SupplierName != item.SupplierName)
                        {
                            currentSupplier.SupplierName = item.SupplierName;
                            currentSupplier.LastUpdate = DateTime.Now;
                            Repository.UpdateAsync(currentSupplier);
                        }
                    }
                    else
                    {
                        item.CreatedDate = item.LastUpdate = DateTime.Now;
                        Repository.AddAsync(item);
                    }
                });

                await Repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error("PublishAsync", ex);
                throw ex;
            }

            return dataViewModels;
        }

        public async Task<List<SupplierViewModel>> CheckDeletedAsync(List<SupplierViewModel> dataViewModels)
        {
            try
            {
                var privateLabelOwner = PrincipalRepository.GetQuery().Where(p => p.Id == PrivateLabelOwnerId).FirstOrDefault();
                var currentDataList = Repository.GetQuery().Where(p => p.PrivateLabelOwner.Id == PrivateLabelOwnerId).ToList();

                currentDataList.ForEach(item =>
                {
                    if (dataViewModels.Find(p => p.UniqueId == item.Id) == null)
                    {
                        item.IsRemoved = true;
                        Repository.UpdateAsync(item);
                    }
                });

                await Repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error("CheckForDeletedAsync", ex);
                throw ex;
            }

            return dataViewModels;
        }

        public async Task<List<SupplierViewModel>> Delete(List<SupplierViewModel> dataViewModels)
        {
            await Task.Factory.StartNew(() =>
            {
                var suppliers = Proxy.ReverseConvert(dataViewModels);

                suppliers.ForEach(item =>
                {
                    var product = Repository.GetQuery().Where(p => p.Id == item.Id).FirstOrDefault();

                    Repository.DeleteAsync(product);
                });

                Repository.SaveChangesAsync();
            });

            return dataViewModels;
        }
        #endregion
    }
}
