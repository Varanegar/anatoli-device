﻿using Anatoli.Framework.Model;
using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Product
{
    public class ProductModel : BaseDataModel
    {
        public int order_count { get; set; }
        public int cat_id { get; set; }
        public int brand_id { get; set; }
        public string product_name { get; set; }
        public int product_id { get; set; }
        public double price { get; set; }
        public int favorit { get; set; }
        public bool IsFavorit
        {
            get { return favorit == 1 ? true : false; }
        }
    }
}