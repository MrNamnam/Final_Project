using System.Net;
using System.IO;
using System.Text;

namespace smokeDetector.Helper
{
    class Utilities
    {
        public static WebResponseTuple ExecuteWebRequest(string requestUrl, string content = "", string contentType = "application/json", string method = "POST")
        {
            var webRequest = WebRequest.Create(requestUrl);
            webRequest.Method = method;



            if (content.Length > 0)
            {
                byte[] dataStream = Encoding.UTF8.GetBytes(content);
                webRequest.ContentType = contentType;
                //     webRequest.Headers.Add(HttpRequestHeader.ContentEncoding.ToString(), Encoding.UTF8.WebName.ToString());

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
