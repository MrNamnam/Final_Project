using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SmokingDetectorFunctions
{
    class signalRFunctions
    {

        // Creating an AzureSignalR object - see AzureSignalR class in this project
        private static readonly AzureSignalR SignalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));

        // This function Deserialize an http requset
        private static async Task<T> ExtractContent<T>(HttpRequestMessage request)
        {
            string connectionRequestJson = await request.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(connectionRequestJson);
        }


        // Initielize the negotiation on signalR
        [FunctionName("negotiate")]
        public static async Task<SignalRConnectionInfo> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage request,
            ILogger log)
        {
            try
            {
                ConnectionRequest connectionRequest = await ExtractContent<ConnectionRequest>(request);
                log.LogInformation($"Negotiating connection for user: <{connectionRequest.UserId}>.");

                string clientHubUrl = SignalR.GetClientHubUrl("AlertsHub");
                string accessToken = SignalR.GenerateAccessToken(clientHubUrl, connectionRequest.UserId);
                return new SignalRConnectionInfo { AccessToken = accessToken, Url = clientHubUrl };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to negotiate connection.");
                throw;
            }
        }


        // Internal class SignalRMessage
        public class WebResponseTuple
        {
            // Gets or sets the string contents of the http response
            public string ResponseContents { get; set; }

            // Gets or sets the status code of the http response
            public HttpStatusCode StatusCode { get; set; }

            // Converts the string contents of the http response to a JObject
            // return: JObject of string contents
            public JObject ResponseAsJObject()
            {
                return JObject.Parse(this.ResponseContents);
            }

            // Converts the string contents of the http response to a JArray
            // return: JArray of string contents
            public JArray ResponseAsJArray()
            {
                return JArray.Parse(this.ResponseContents);
            }
        }

        // Utilities for accessing APIs
        public class Utilities
        {
            // Execute the web request for given URL and content and returns response
            // <param name="requestUrl">URL for which to make web request</param>
            // <param name="headers">Authorization headers for this request</param>
            // <param name="content">Content of request</param>
            // <param name="contentType">Content type</param>
            // <param name="method">Method of request</param>
            // <returns>Response from web request</returns>
            public static WebResponseTuple ExecuteWebRequest(string requestUrl, string content = "", string contentType = "application/json", string method = "POST")
            {
                var webRequest = WebRequest.Create(requestUrl);
                webRequest.Method = method;

                if (content.Length > 0)
                {
                    byte[] dataStream = Encoding.UTF8.GetBytes(content);
                    webRequest.ContentType = contentType;
                    // webRequest.Headers.Add(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());
                    // Write request content
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(dataStream, 0, dataStream.Length);
                        requestStream.Flush();
                    }
                }

                try
                {
                    // Execute request and get response
                    using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                    {
                        // TODO: Is there a way to get the return code here?
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                var result = streamReader.ReadToEnd();
                                WebResponseTuple tuple = new WebResponseTuple()
                                {
                                    ResponseContents = result,
                                    StatusCode = response.StatusCode
                                };

                                return tuple;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    var response = (HttpWebResponse)e.Response;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            var result = streamReader.ReadToEnd();
                            WebResponseTuple tuple = new WebResponseTuple()
                            {
                                ResponseContents = result,
                                StatusCode = response.StatusCode
                            };

                            return tuple;
                        }
                    }
                }
            }
        }
    }
}
