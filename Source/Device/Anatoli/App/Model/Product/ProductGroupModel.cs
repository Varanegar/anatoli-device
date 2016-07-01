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
        public string UniqueIdString { get; set; }
        public int ParentId { get; set; }
        public string ParentUniqueIdString { get; set; }
        public string GroupName { get; set; }
        public int NLeft { get; set; }
        public int NRight { get; set; }
        public int NLevel { get; set; }
        public string CharGroupIdString { get; set; }
    }
}
