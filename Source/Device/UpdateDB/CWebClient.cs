using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDB
{
    class CWebClient : AnatoliWebClient
    {
        public override bool IsOnline()
        {
            return true;
        }
    }
}
