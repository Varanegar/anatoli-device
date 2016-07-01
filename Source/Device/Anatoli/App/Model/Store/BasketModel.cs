using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class BasketModel : BaseModel
    {
        public static Guid CheckOutBasketTypeId = Guid.Parse("F6CE03E2-8A2A-4996-8739-DA9C21EAD787");
        public static Guid FavoriteBasketTypeId = Guid.Parse("AE5DE00D-3391-49FE-985B-9DA7045CDB13");
        public static Guid IncompleteBasketTypeId = Guid.Parse("194CA845-2E34-4A06-9A89-DCAFF956FE4D");
        public Guid CustomerId { get; set; }
        public Guid BasketTypeValueId { get; set; }
        public string BasketName { get; set; }
        public List<BasketItemModel> BasketItems { get; set; }
    }
}
