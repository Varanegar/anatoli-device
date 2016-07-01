using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model
{
    public class ItemImageViewModel : BaseModel
    {
        public static string ProductImageType = "635126C3-D648-4575-A27C-F96C595CDAC5";
        public static string CenterImageType = "9CED6F7E-D08E-40D7-94BF-A6950EE23915";
        public static string ProductSiteGroupImageType = "149E61EF-C4DC-437D-8BC9-F6037C0A1ED1";
        public static string CustomerImageType = "73C20167-9B30-4385-95AE-1A0BA89CC415";
        public byte[] image { get; set; }
        public string ImageName { get; set; }
        public string ImageType { get; set; }

        public string BaseDataId { get; set; }
    }
}
