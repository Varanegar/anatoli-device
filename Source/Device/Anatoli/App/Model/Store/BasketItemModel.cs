using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class BasketItemModel : BaseModel
    {
        public int? Qty { get; set; }
        public string Comment { get; set; }
        public Guid ProductId { get; set; }
        public Guid BasketId { get; set; }
    }
}
