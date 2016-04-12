﻿using System;
using System.Web;
using System.Collections.Generic;
using Anatoli.ViewModels.StockModels;
using Anatoli.Cloud.WebApi.Controllers;
using Anatoli.ViewModels.ProductModels;

namespace Anatoli.Cloud.WebApi.Classes
{
    public abstract class AnatoliApiController : BaseApiController
    {


        public bool GetRemovedData
        {
            get
            {
                string data = HttpContext.Current.Request.Headers["GetRemovedData"];
                return (data == null) ? true : bool.Parse(data);
            }
        }

        public Guid OwnerKey
        {
            get
            {
                return Guid.Parse(HttpContext.Current.Request.Headers["OwnerKey"]);
            }
        }
        public Guid DataOwnerKey
        {
            get
            {
                if (HttpContext.Current.Request.Headers["DataOwnerKey"] == null)
                    return OwnerKey;
                else
                    return Guid.Parse(HttpContext.Current.Request.Headers["DataOwnerKey"]);
            }
        }
        public Guid DataOwnerCenterKey
        {
            get
            {
                if (HttpContext.Current.Request.Headers["DataOwnerCenterKey"] == null)
                    return DataOwnerKey;
                else
                    return Guid.Parse(HttpContext.Current.Request.Headers["DataOwnerCenterKey"]);
            }
        }
    }
}