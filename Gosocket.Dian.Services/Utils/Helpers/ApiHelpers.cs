using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Utils.Helpers
{
    public class ApiHelpers
    {
        private static HttpClient _client = new HttpClient();
        private static ConcurrentDictionary<Guid, HttpClient> _httpClients = new ConcurrentDictionary<Guid, HttpClient>();

        private static async Task<HttpResponseMessage> ConsumeApiAsync<T>(string url, T requestObj)
        {
            if (!_httpClients.ContainsKey(ToGuid(url)))
            {
                _client.DefaultRequestHeaders.Connection.Add("Keep-Alive");

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                if (!_httpClients.ContainsKey(ToGuid(url)))
                    _httpClients[ToGuid(url)] = _client;


                return await _client.PostAsync(url, byteContent);
            }
            else
            {
                _client = _httpClients[ToGuid(url)];

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return await _client.PostAsync(url, byteContent);
            }
        }

        private static HttpResponseMessage ConsumeApi<T>(string url, T requestObj)
        {
            if (!_httpClients.ContainsKey(ToGuid(url)))
            {
                _client.DefaultRequestHeaders.Connection.Add("Keep-Alive");

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                if (!_httpClients.ContainsKey(ToGuid(url)))
                    _httpClients[ToGuid(url)] = _client;

                return _client.PostAsync(url, byteContent).Result;
            }
            else
            {
                _client = _httpClients[ToGuid(url)];

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return _client.PostAsync(url, byteContent).Result;
            }

        }

        private static async Task<HttpResponseMessage> ConsumeApiWithHeaderAsync<T>(string url, T requestObj, Dictionary<string, string> headers)
        {
            if (!_httpClients.ContainsKey(ToGuid(url)))
            {
                _client.DefaultRequestHeaders.Connection.Add("Keep-Alive");

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                if (headers != null)
                    foreach (var header in headers)
                        _client.DefaultRequestHeaders.Add(header.Key, header.Value);

                if (!_httpClients.ContainsKey(ToGuid(url)))
                    _httpClients[ToGuid(url)] = _client;

                return await _client.PostAsync(url, byteContent);
            }
            else
            {
                _client = _httpClients[ToGuid(url)];

                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return await _client.PostAsync(url, byteContent);
            }
        }

        private static HttpResponseMessage ConsumeApiWithHeader<T>(string url, T requestObj, Dictionary<string, string> headers)
        {
            using (_client)
            {
                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestObj));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                if (headers != null)
                    foreach (var header in headers)
                        _client.DefaultRequestHeaders.Add(header.Key, header.Value);

                return _client.PostAsync(url, byteContent).Result;
            }
        }

        public static async Task<T> ExecuteRequestAsync<T>(string url, dynamic requestObj)
        {
            string result = "";
            try
            {
                var response = await ConsumeApiAsync(url, requestObj);
                result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"result={result}____________request={JsonConvert.SerializeObject(requestObj)}____________{ex.Message} {ex.InnerException?.Message}");
            }
        }

        public static T ExecuteRequest<T>(string url, dynamic requestObj)
        {
            string result = "";
            try
            {
                var response = ConsumeApi(url, requestObj);
                result = response.Content.ReadAsStringAsync()?.Result;
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"result={result}____________request={JsonConvert.SerializeObject(requestObj)}____________{ex.Message} {ex.InnerException?.Message}");
            }
        }

        public static async Task<T> ExecuteRequestWithHeaderAsync<T>(string url, dynamic requestObj, Dictionary<string, string> headers)
        {
            var response = await ConsumeApiWithHeaderAsync(url, requestObj, headers);
            var result = response.Content.ReadAsStringAsync().Result;
            return (T)JsonConvert.DeserializeObject<T>(result);
        }

        public static T ExecuteRequestWithHeader<T>(string url, dynamic requestObj, Dictionary<string, string> headers)
        {
            var response = ConsumeApiWithHeader(url, requestObj, headers);
            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(result);
        }

        private static Guid ToGuid(string code)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(code);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return new Guid(hashBytes);
            }
        }
    }
}