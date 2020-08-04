using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace oath_tests.Biz.Helpers
{
    public class GoogleEndpointsHelper
    {
        public GoogleEndpointsHelper()
        {
            using (var client = new WebClient())
            {
                var response = client.DownloadString("https://accounts.google.com/.well-known/openid-configuration");
                var jobject = JObject.Parse(response);
                AuthorizationEndpoint = jobject.SelectToken("authorization_endpoint").ToString();
                TokenEndpoint = jobject.SelectToken("token_endpoint").ToString();
            }
        }

        public string AuthorizationEndpoint { get; private set; }

        public string TokenEndpoint { get; private set; }
    }
}