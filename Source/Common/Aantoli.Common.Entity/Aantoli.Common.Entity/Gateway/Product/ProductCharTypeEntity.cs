﻿using Aantoli.Common.Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aantoli.Common.Entity.Gateway.Product
{
    public class ProductCharTypeEntity : BaseEntity
    {
        public string CharTypeDesc { get; set; }
        public List<ProductCharValueEntity> CharValueList { get; set; }
    }
}
