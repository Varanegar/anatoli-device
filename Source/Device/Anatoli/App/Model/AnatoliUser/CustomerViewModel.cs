using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model
{
    public class CustomerViewModel : BaseModel
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string BirthDay { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string NationalCode { get; set; }
        string _firstName;
        public string FirstName { get { return _firstName == null ? "" : _firstName.Trim(); } set { _firstName = value; } }
        string _lastName;
        public string LastName { get { return _lastName == null ? "" : _lastName.Trim(); } set { _lastName = value; } }
        public string MainStreet { get; set; }
        public string OtherStreet { get; set; }
        public string RegionInfoId { get; set; }
        public Guid? RegionLevel1Id { get; set; }
        public Guid? RegionLevel2Id { get; set; }
        public Guid? RegionLevel3Id { get; set; }
        public Guid? RegionLevel4Id { get; set; }
        public Guid? DefauleStoreId { get; set; }

    }
}
