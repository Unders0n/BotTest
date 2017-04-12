using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MarketRetailBot.JsonHelpers;
using MarketRetailBot.Model;
using RestSharp;

namespace MarketRetailBot.ApiHelpers
{
    //todo: refactor to using DI container when in production
    public static class ApiClients
    {
        //todo: move to configs
        private const string ProductUri = "https://kixify-util-services.azurewebsites.net/api/product/getbytitle";
        private const string ProductAvailabilityUri =   "http://www.kixify.com/app/category/bystylecode";
        private const string AuthorisationKey = "Basic dXRpbHNlcnZpY2U6dXRpbGF1dGg3Nzc =";
        public static async Task<List<ProductAvailability>> GetProductAvailabilities(string sku)
        {
            //low level HttpClient instead of Restsharp 'cos cant find any way to make same in RS
            var httpClient = new HttpClient();
            var form = new MultipartFormDataContent();

            form.Add(new StringContent(sku), "style_code");
            var response = await httpClient.PostAsync(ProductAvailabilityUri, form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            var sd = response.Content.ReadAsStringAsync().Result;
            var productAvailabilities = SerializationHelper.DeserializeAndUnwrap<List<ProductAvailability>>(sd);
            return productAvailabilities;
        }

        public static string GetProducts(string searchString)
        {
            var client = new RestClient(ProductUri);
            var request = new RestRequest("", Method.POST);
            request.AddHeader("Authorization", AuthorisationKey);
            request.AddObject(new { Title = searchString });
            var resStr = client.Execute(request).Content;
            SerializationHelper.DeserializeAndUnwrap<List<Product>>(resStr);
            return resStr;
        }
    }
}