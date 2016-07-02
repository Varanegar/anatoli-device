using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Product
{
    public class ProductGroupModel : BaseModel
    {
        public static Guid NullGroupId = Guid.Parse("5c2782e7-57e6-45b8-b438-7f05f48703a7");
        public string UniqueIdString { get; set; }
        public int ParentId { get; set; }
        public string ParentUniqueIdString { get; set; }
        public string GroupName { get; set; }
        public int NLeft { get; set; }
        public int NRight { get; set; }
        public int NLevel { get; set; }
        public string CharGroupIdString { get; set; }
        public string Image { get; set; }
    }
}
