﻿using Aantoli.Framework.Entity.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aantoli.Common.Entity.Gateway.BaseValue
{
    public class BaseValueTypeEntity : BaseEntity
    {
        public string BaseTypeDescription { get; set; }
        public string BaseTypeName { get; set; }
        public BaseValueInfoListEntity BaseValueList { get; set; }
    }
}