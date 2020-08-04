using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace oath_tests.Biz.Helpers
{
    public class WebMethodsHelper
    {
        public async Task<string> Post(string endpoint,Dictionary<string,string> values) {
            string result = null;
            HttpClient client = new HttpClient();

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(endpoint, content);

            result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}