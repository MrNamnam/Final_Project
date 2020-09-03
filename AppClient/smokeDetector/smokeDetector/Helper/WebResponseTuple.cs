using System.Net;
using Newtonsoft.Json.Linq;

namespace smokeDetector.Helper
{
    class WebResponseTuple
    {

        /// <summary>
        /// Gets or sets the string contents of the http response
        /// </summary>
        public string ResponseContents { get; set; }

        /// <summary>
        /// Gets or sets the status code of the http response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Converts the string contents of the http response to a JObject
        /// </summary>
        /// <returns>JObject of string contents</returns>
        public JObject ResponseAsJObject()
        {
            return JObject.Parse(this.ResponseContents);
        }

        /// <summary>
        /// Converts the string contents of the http response to a JArray
        /// </summary>
        /// <returns>JArray of string contents</returns>
        public JArray ResponseAsJArray()
        {
            return JArray.Parse(this.ResponseContents);
        }
    }
}
