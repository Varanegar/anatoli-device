using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class MessageModel : BaseModel
    {
        public string store_id { get; set; }
        public string content { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int new_flag { get; set; }
        public string store_name { get; set; }
        public int msg_id { get; set; }
        public bool IsNewMsg { get { return (new_flag == 1) ? true : false; } }
    }
}
