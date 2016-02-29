﻿using Anatoli.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.App.Model.Store
{
    public class DeliveryTimeModel : BaseViewModel
    {
        public TimeSpan timespan;
        public override string ToString()
        {
            if (timespan.Minutes == 0)
                return "ساعت " + timespan.Hours;
            else
                return "ساعت " + timespan.Hours + ":" + timespan.Minutes;
        }
    }
}
