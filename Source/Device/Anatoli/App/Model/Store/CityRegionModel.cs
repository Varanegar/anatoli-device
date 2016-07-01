using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class CityRegionModel : BaseModel
    {
        private string uniqueIdString;
        public string UniqueIdString
        {
            get
            {
                return this.uniqueIdString;
            }
            set
            {
                this.uniqueIdString = value;
                if (value != null)
                    this.UniqueId = Guid.Parse(value);
            }
        }

        string _parentUniqueIdString;
        public string ParentUniqueIdString
        {
            get { return (_parentUniqueIdString != null) ? _parentUniqueIdString.ToUpper() : ""; }
            set { _parentUniqueIdString = (value != null) ? value.ToUpper() : ""; }
        }
        public int ParentId { get; set; }
        public string GroupName { get; set; }
        public int NLeft { get; set; }
        public int NRight { get; set; }
        public int NLevel { get; set; }
        public int? Priority { get; set; }

        public override string ToString()
        {
            return GroupName;
        }
    }
}
