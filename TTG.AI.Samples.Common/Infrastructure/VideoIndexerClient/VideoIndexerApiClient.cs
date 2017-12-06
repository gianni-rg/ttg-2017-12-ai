//
// Copyright (c) Gianni Rosa Gallina. All rights reserved.
// Licensed under the MIT license.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient
{
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient.Model;

    // See: https://docs.microsoft.com/en-us/azure/cognitive-services/video-indexer/video-indexer-use-apis
    // See: https://videobreakdown.portal.azure-api.net/docs/services/582074fb0dc56116504aed75/operations/582074fb0dc5610e14c75ec7

    public class VideoIndexerApiClient
    {
        #region Constants
        private const string ApiEndpoint = "https://videobreakdown.azure-api.net/Breakdowns/Api/Partner/Breakdowns";
        #endregion

        #region Private fields
        private string m_SubscriptionKey;
        private readonly HttpClient m_HttpClient;
        #endregion

        #region Constructor
        public VideoIndexerApiClient(string subscriptionKey)
        {
            m_SubscriptionKey = subscriptionKey;

            m_HttpClient = new HttpClient();
            m_HttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
        }
        #endregion

        #region Methods
        public async Task<VideoIndexerResult> AnalyzeVideoAsync(string videoId, string videoUrl)
        {
            Console.WriteLine($"VideoIndexerApiClient.AnalyzeVideoAsync({videoId}) Starting processing ({videoUrl})");

            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["name"] = videoId;
            queryString["privacy"] = "Private";
            queryString["videoUrl"] = videoUrl;
            //queryString["language"] = "{string}";
            //queryString["externalId"] = "{string}";
            //queryString["metadata"] = "{string}";
            //queryString["description"] = "{string}";
            //queryString["partition"] = "{string}";
            //queryString["callbackUrl"] = "{string}";
            //queryString["indexingPreset"] = "{string}";
            //queryString["streamingPreset"] = "{string}";

            string requestUri = $"{ApiEndpoint}?{queryString}";

            var content = new MultipartFormDataContent();
            var result = await m_HttpClient.PostAsync(requestUri, content).ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var id = JsonConvert.DeserializeObject<string>(json);

            Console.WriteLine($"VideoIndexerApiClient.AnalyzeVideoAsync({videoId}) Processing started ({id})");

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);

                    result = await m_HttpClient.GetAsync($"{ApiEndpoint}/{id}/State").ConfigureAwait(false);
                    json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

                    Console.WriteLine($"VideoIndexerApiClient.AnalyzeVideoAsync({videoId}) Processing state: {json}");

                    dynamic processingState = JsonConvert.DeserializeObject(json);
                    if (processingState.state != "Uploaded" && processingState.state != "Processing")
                    {
                        Console.WriteLine($"VideoIndexerApiClient.AnalyzeVideoAsync({videoId}) Processing finished");
                        break;
                    }
                }
            });

            Console.WriteLine($"VideoIndexerApiClient.AnalyzeVideoAsync({videoId}) Retrieving analysis results");

            result = await m_HttpClient.GetAsync($"{ApiEndpoint}/{id}").ConfigureAwait(false);

            json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<VideoIndexerResult>(json);
        }

        public async Task<VideoIndexerResult> GetAnalyzedVideoAsync(string id)
        {
            var result = await m_HttpClient.GetAsync($"{ApiEndpoint}/{id}").ConfigureAwait(false);
            var json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<VideoIndexerResult>(json);
        }
        #endregion
    }
}
