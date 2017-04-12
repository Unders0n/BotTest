using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

namespace MarketRetailBot.JsonHelpers
{
    public static class SerializationHelper
    {
        /// <summary>
        ///     Deserialising json string and unwrapping only part with needed node
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="nameOfNode">Name of node to start from, "Data" by default</param>
        /// <returns></returns>
        public static T DeserializeAndUnwrap<T>(string json, string nameOfNode = "Data") where T : class 
        {
            var jo = JObject.Parse(json);
            if (!jo.Properties().First(property => property.Name.ToLower() == nameOfNode.ToLower()).Value.Any())
                return null;
            return  (jo.Properties().First(property => property.Name.ToLower() == nameOfNode.ToLower()).Value.ToObject<T>());
        }
    }
}