﻿using Anatoli.Cloud.Gateway.Business.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Anatoli.Cloud.WebApi.Controllers
{
    [RoutePrefix("api/gateway/store")]
    public class StoreController : ApiController
    {
        [Authorize(Roles = "AuthorizedApp")]
        [Route("storeonhand")]
        public IHttpActionResult GetStoreOnHand()
        {
            return Ok(StoreOnHandCloudHandler.GetInstance().GetSampleData());
        }

        [Authorize(Roles = "AuthorizedApp")]
        [Route("storepricelist")]
        public IHttpActionResult GetStorePriceLists()
        {
            return Ok(StoreProductPriceListsCloudHandler.GetInstance().GetSampleData());
        }
    }
}
