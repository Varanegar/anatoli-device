﻿using Anatoli.Business.Domain;
using Anatoli.ViewModels.ProductModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Anatoli.Cloud.WebApi.Controllers
{
    [RoutePrefix("api/gateway/base/manufacture")]
    public class ManufactureController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Products
        [Authorize(Roles = "AuthorizedApp, User")]
        [Route("manufactures")]
        public async Task<IHttpActionResult> GetManufactures(string privateOwnerId)
        {
            var owner = Guid.Parse(privateOwnerId);
            var manufactureDomain = new ManufactureDomain(owner);
            var result = await manufactureDomain.GetAll();

            return Ok(result);
        }

        [Authorize(Roles = "AuthorizedApp, User")]
        [Route("manufactures/after")]
        public async Task<IHttpActionResult> GetManufactures(string privateOwnerId, string dateAfter)
        {
            var owner = Guid.Parse(privateOwnerId);
            var manufactureDomain = new ManufactureDomain(owner);
            var validDate = DateTime.Parse(dateAfter);
            var result = await manufactureDomain.GetAllChangedAfter(validDate);

            return Ok(result);
        }

        [Authorize(Roles = "AuthorizedApp")]
        [Route("save")]
        public async Task<IHttpActionResult> SaveManufactures(string privateOwnerId, List<ManufactureViewModel> data)
        {
            if (data != null) log.Info("save manufacture count : " + data.Count);
            var owner = Guid.Parse(privateOwnerId);
            var manufactureDomain = new ManufactureDomain(owner);
            await manufactureDomain.PublishAsync(data);
            return Ok();
        }
        #endregion
    }
}
