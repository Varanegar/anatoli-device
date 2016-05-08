using Anatoli.Framework.AnatoliBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnatoliIOS.Clients
{
    class IosWebClient : AnatoliWebClient
    {
        public override bool IsOnline()
        {
            if (Reachability.InternetConnectionStatus() == NetworkStatus.NotReachable)
                return false;
            else
                return true;
        }
    }
}
