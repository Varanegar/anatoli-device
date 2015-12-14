﻿using System;
using System.Linq;
using Anatoli.DataAccess.Models;
using System.Collections.Generic;
using Anatoli.DataAccess.Models.Identity;

namespace Anatoli.DataAccess.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
    }
}
