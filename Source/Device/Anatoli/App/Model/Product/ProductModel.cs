using Anatoli.Framework.Model;
using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Anatoli.App.Model.Product
{
    public class ProductModel : BaseModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string StoreProductName { get; set; }
        public decimal? PackVolume { get; set; }
        public decimal? PackWeight { get; set; }
        public decimal QtyPerPack { get; set; }
        public decimal RateValue { get; set; }
        public string SmallPicURL { get; set; }
        public string LargePicURL { get; set; }
        public string Desctription { get; set; }
        public string PackUnitId { get; set; }
        public string ProductTypeId { get; set; }
        public string TaxCategoryId { get; set; }
        public string MainProductGroupIdString { get; set; }
        Guid? _ProductGroupId;
        public Guid? ProductGroupId { get { return _ProductGroupId == null ? ProductGroupModel.NullGroupId : _ProductGroupId; } set { _ProductGroupId = value; } }
        public string ManufactureIdString { get; set; }
        public double Price { get; set; }
        public int ShoppingBasketCount { get; set; }
        public int FavoritBasketCount { get; set; }
        public string ImageAddress { get; set; }
        public decimal Qty { get; set; }
        public string GroupName { get; set; }
        public bool IsFavorit
        {
            get { return FavoritBasketCount > 0 ? true : false; }
        }
        public bool IsAvailable
        {
            get { return Qty > 0 ? true : false; }
        }
    }

}
