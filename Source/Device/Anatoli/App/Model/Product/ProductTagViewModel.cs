using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Product
{
    public class ProductTagViewModel : BaseModel
    {
        public static readonly Guid Special = Guid.Parse("17F36FE2-BFB4-4E7E-BC10-6AE7512750B3");
        public string ProductTagName { get; set; }
    }
}
