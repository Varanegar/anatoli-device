﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Anatoli.Cloud.WebApi.Controllers
{
    [RoutePrefix("api/gateway/customer/shipaddress")]
    public class CustomerShipAddressController : ApiController
    {
        [Authorize(Roles = "AuthorizedApp")]
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok();
        }
    }
}
