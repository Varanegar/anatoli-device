using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Product
{
    public class ProductUpdateModel : BaseViewModel
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

        //public List<SupplierViewModel> Suppliers { get; set; }
        public string MainProductGroupIdString { get; set; }
        string _productGroupId;
        public string ProductGroupId { get {return _productGroupId;} set { _productGroupId = (value == null ) ? null : value.ToUpper();} }
        public string ManufactureIdString { get; set; }

        //public List<CharValueViewModel> CharValues { get; set; }
        //public List<ProductPictureViewModel> ProductPictures { get; set; }
    }
}
