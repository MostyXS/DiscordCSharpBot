using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LOSCKeeper.VKTEST
{
    class LongPollingTest
    {

        private async Task<string> LongPollTest()
        {
            var url = "http://your.url";
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                using (var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead))
                {
                    using (var body = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(body))
                    {
                        string result;
                        result = await reader.ReadToEndAsync();
                        
                        return result;

                    }
                }
            }
            

        }


    }
}
