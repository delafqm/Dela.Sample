using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Dela.Sample.WebMvc
{
    public class ApiHelper
    {
        public static async Task<string> GetApi()
        {
            // discover endpoints from metadata
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("http://47.99.36.29:8086");
            if (disco.IsError)
            {
                //验证基地址错误
                Console.WriteLine(disco.Error);
                return "";
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client.api.service",
                ClientSecret = "clientsecret",
                Scope = "clientservice"
            });

            if (tokenResponse.IsError)
            {
                //获取token错误
                Console.WriteLine(tokenResponse.Error);
                return "";
            }

            Console.WriteLine(tokenResponse.Json);

            // call api
            var client1 = new HttpClient();
            client1.SetBearerToken(tokenResponse.AccessToken);

            var response = await client1.GetAsync("http://47.99.36.29:8088/api/Authorize/AuthValues");
            if (!response.IsSuccessStatusCode)
            {
                //获取API错误
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            return "";
        }

        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        //获取Token

        public static async Task<string> GetToken(Dictionary<string, string> dic)
        {
            string token = null;
            if (cache.TryGetValue<string>("Token", out token))
            {
                return token;
            }
            TokenResult items = null;
            using (HttpClient http = new HttpClient())
            using (var content = new FormUrlEncodedContent(dic))
            {
                var msg = await http.PostAsync("http://47.99.36.29:8086/connect/token", content);
                if (!msg.IsSuccessStatusCode)
                {
                    //return StatusCode(Convert.ToInt32(msg.StatusCode));

                    throw new Exception(msg.StatusCode.ToString());
                }

                string result = await msg.Content.ReadAsStringAsync();


                //accessToken = result;

                items = JsonConvert.DeserializeObject<TokenResult>(result);

                token = items.access_token;

                cache.Set<string>("Token", token, TimeSpan.FromSeconds(items.expires_in));
            }
            //try
            //{
            //    //DiscoveryClient类：IdentityModel提供给我们通过基础地址（如：http://localhost:5000）就可以访问令牌服务端；
            //    //当然可以根据上面的restful api里面的url自行构建；上面就是通过基础地址，获取一个TokenClient;（对应restful的url：token_endpoint   "http://localhost:5000/connect/token"）
            //    //RequestClientCredentialsAsync方法：请求令牌；
            //    //获取令牌后，就可以通过构建http请求访问API接口；这里使用HttpClient构建请求，获取内容；
            //    //var dico = await DiscoveryClient.GetAsync("http://192.168.0.3:8085");
            //    //var tokenClient = new TokenClient(dico.TokenEndpoint, "client.api.service", "clientsecret");
            //    //var tokenResponse = await tokenClient.RequestClientCredentialsAsync("clientservice");



            //    if (tokenResponse.IsError)
            //    {
            //        throw new Exception(tokenResponse.Error);
            //    }
            //    token = tokenResponse.AccessToken;

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);
            //}
            return token;
        }


        public static async Task<T> GetAsync<T>(string url)
        {
            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //添加Token
                //var token = await GetToken();
                //http.SetBearerToken(token);

                var response = await http.GetAsync(url);

                response.EnsureSuccessStatusCode();

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                string Result = await response.Content.ReadAsStringAsync();

                var Item = JsonConvert.DeserializeObject<T>(Result);

                return Item;
            }
            //return "";
        }


        public static async Task<T> PostAsync<T>(string url, Dictionary<string, string> dic)
        {

            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //添加Token
                var token = await GetToken(dic);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //http.SetBearerToken(token);
                //使用FormUrlEncodedContent做HttpContent
                //var content = new FormUrlEncodedContent(dic);
                //await异步等待回应
                //var response = await http.PostAsync(url, content);
                var response = await http.GetAsync(url);

                //确保HTTP成功状态值
                //response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString());
                }

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                string Result = await response.Content.ReadAsStringAsync();

                var Item = JsonConvert.DeserializeObject<T>(Result);

                return Item;
            }
        }

        public static async Task<string> PostAsync(string url, Dictionary<string, string> dic)
        {

            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //添加Token
                var token = await GetToken(dic);
                //http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                http.SetBearerToken(token);
                //使用FormUrlEncodedContent做HttpContent
                //var content = new FormUrlEncodedContent(dic);
                //await异步等待回应
                //var response = await http.PostAsync(url, content);
                var response = await http.GetAsync(url);

                //确保HTTP成功状态值
                //response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(response.StatusCode.ToString());
                }

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                string Result = await response.Content.ReadAsStringAsync();

                //var Item = JsonConvert.DeserializeObject<T>(Result);

                return Result;
            }
        }
    }
}
