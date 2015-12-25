﻿using Anatoli.ViewModels.BaseModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ClientApp
{
    public static class BasketManagement
    {
        public static List<BasketViewModel> GetCustomerBaskets(HttpClient client, string servserURI)
        {
            List<BasketViewModel> basketList = new List<BasketViewModel>();

            var result8 = client.GetAsync(servserURI + "/api/gateway/basket/customerbaskets?id=F125EDC7-473D-4C59-B966-3EF9E6E6A7D9&privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C").Result;
            var json8 = result8.Content.ReadAsStringAsync().Result;
            var obj = new List<BasketViewModel>();
            var x = JsonConvert.DeserializeAnonymousType(json8, basketList);
            return x;
            
        }

        public static void UpdateCustomerBasketFromServer(HttpClient client, string servserURI)
        {
            var obj = GetCustomerBaskets(client, servserURI);
            
            //obj.Baskets.RemoveAt(1);
            string data = new JavaScriptSerializer().Serialize(obj);
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
            var result8 = client.PostAsync(servserURI + "/api/gateway/customer/save?privateOwnerId=3EEE33CE-E2FD-4A5D-A71C-103CC5046D0C", content).Result;
            var json8 = result8.Content.ReadAsStringAsync().Result;
            var obj2 = new { message = "", ModelState = new Dictionary<string, string[]>() };
            var x = JsonConvert.DeserializeAnonymousType(json8, obj2);
        }

    }
}
