using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model
{
    public class BaseTypeViewModel : BaseModel
    {
        public static Guid DeliveryType = Guid.Parse("f5ffad55-6e39-40bd-a95d-12a34ba4d005");
        public static Guid PayType = Guid.Parse("f17b8898-d39f-4955-9757-a6b31767f5c7");
        public string BaseTypeDesc { get; set; }
        public List<BaseValueViewModel> BaseValues { get; set; }
    }
}
