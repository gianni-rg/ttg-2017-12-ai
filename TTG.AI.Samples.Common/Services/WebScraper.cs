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

namespace TTG.AI.Samples.Common
{
    using HtmlAgilityPack;
    using TTG.AI.Samples.Common.Model;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TTG.AI.Samples.Common.Services.Helpers;
    using TTG.AI.Samples.Common.Interfaces;
    using System.Net.Http;
    using System.IO;
    using System.Collections.Generic;

    // See: http://html-agility-pack.net/

    public class WebScraper : IDisposable, IWebScraper
    {
        #region Constants
        private const string MSEdgeFCUUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299";
        #endregion

        #region Private fields
        private readonly HTMLStripper m_HtmlStripper;
        private readonly HttpClient m_Client;
        private readonly Dictionary<string, ITextExtractor> m_TextExtractors;
        private readonly HashSet<string> m_BlackList;
        #endregion

        #region Constructor
        public WebScraper()
        {
            m_BlackList = new HashSet<string>
            {
                "www.siliconarmada.com"
            };

            // Configure a generic Text Extractor, which just remove HTML tags from retrieved webpages
            // and customized extractor based on tag properties for specific domains
            m_TextExtractors = new Dictionary<string, ITextExtractor>
            {
                { "default", new BaseTextExtractor() },
                { "techcrunch.com", new TagSelectorTextExtractor("//div[@class='article-entry text']") },
                { "mashable.com", new TagSelectorTextExtractor("//section[@class='article-content blueprint']") },
                { "twitter.com", new TagSelectorTextExtractor("//div[@class='js-tweet-text-container']") }
            };

            m_HtmlStripper = new HTMLStripper();
            m_Client = new HttpClient();
            m_Client.DefaultRequestHeaders.Add("User-Agent", MSEdgeFCUUserAgent);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (m_Client != null)
                    {
                        m_Client.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebScraper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        #endregion

        #region Methods
        public async Task<WebPage> DownloadWebPageAsync(string url)
        {
            Console.WriteLine($"\t\t\tWebScraper.DownloadWebPageAsync(): started retrieving web resource '{url}'");

            HtmlDocument doc;
            var web = new HtmlWeb
            {
                // MS-Edge FCU Update
                UserAgent = MSEdgeFCUUserAgent
            };

            string expandedUrl = null;
            try
            {
                expandedUrl = await GetExpandedUrlAsync(url).ConfigureAwait(false);

                // Ignore websites in "black-list"
                if (IsInBlackList(new Uri(expandedUrl)))
                {
                    Console.WriteLine($"\t\t\tWebScraper.DownloadWebPageAsync(): black-listed. Ignore.");
                    return null;
                }

                doc = await web.LoadFromWebAsync(expandedUrl).ConfigureAwait(false);

                Console.WriteLine($"\t\t\tWebScraper.DownloadWebPageAsync(): completed retrieving web resource '{expandedUrl}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tWebScraper.DownloadWebPageAsync(): error while retrieving web resource, '{ex.Message}'");
                return null;
            }

            if (string.IsNullOrEmpty(expandedUrl))
            {
                return null;
            }

            var webPage = new WebPage(expandedUrl)
            {
                RawHtml = doc.ParsedText
            };

            ExtractMetadata(doc, webPage);

            var textExtractor = GetTextExtractorFromDomain(new Uri(webPage.SourceUrl));
            webPage.Text = textExtractor.ExtractText(doc);

            return webPage;
        }

        private ITextExtractor GetTextExtractorFromDomain(Uri uri)
        {
            string domain = uri.Host;
            if (m_TextExtractors.ContainsKey(domain))
            {
                return m_TextExtractors[domain];
            }
            else
            {
                if (uri.Segments.Length > 2)
                {
                    var key = $"{uri.Host}/{uri.Segments[2]}";
                    if (m_TextExtractors.ContainsKey(key))
                    {
                        return m_TextExtractors[key];
                    }
                }
                return m_TextExtractors["default"];
            }
        }

        private bool IsInBlackList(Uri uri)
        {
            string domain = uri.Host;
            return m_BlackList.Contains(domain);
        }

        public async Task DownloadResourceAsync(string sourceUrl, string destinationFilePath)
        {
            Console.WriteLine($"\t\t\tWebScraper.DownloadResourceAsync(): starting retrieving web resource '{sourceUrl}'");

            try
            {
                var downloadStream = await m_Client.GetStreamAsync(sourceUrl).ConfigureAwait(false);
                using (var destinationFileStream = File.OpenWrite(destinationFilePath))
                {
                    await downloadStream.CopyToAsync(destinationFileStream).ConfigureAwait(false);
                }

                Console.WriteLine($"\t\t\tWebScraper.DownloadResourceAsync(): completed retrieving web resource");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tWebScraper.DownloadResourceAsync(): failed retrieving web resource: {ex.Message}");
            }
        }
        #endregion

        #region Private methods
        private async Task<string> GetExpandedUrlAsync(string url)
        {
            // See: https://bit.do/list-of-url-shorteners.php
            if (IsShortenedUrl(url))
            {
                Console.WriteLine($"\t\t\tWebScraper.GetExpandedUrlAsync(): trying to retrieve expanded url for '{url}'");
                var response = await m_Client.GetAsync(url).ConfigureAwait(false);

                string expandedUrl;
                if (response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
                {
                    expandedUrl = response.Headers.Location.AbsoluteUri;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Moved)
                {
                    expandedUrl = response.Headers.Location.AbsoluteUri;
                }
                else
                {
                    expandedUrl = response.RequestMessage.RequestUri.AbsoluteUri;
                }
                Console.WriteLine($"\t\t\tWebScraper.GetExpandedUrlAsync(): retrieved expanded url '{expandedUrl}'");

                // It may happen that a shortened URL is pointing to another shortened URL
                if (IsShortenedUrl(expandedUrl))
                {
                    Console.WriteLine($"\t\t\tWebScraper.GetExpandedUrlAsync(): retrieved another shortened url '{expandedUrl}', try to expand it");
                    return await GetExpandedUrlAsync(expandedUrl);
                }

                return expandedUrl;
            }

            return url;
        }

        private bool IsShortenedUrl(string url)
        {
            bool isShortened =
               url.Contains("/bit.ly/") ||
               url.Contains("/virg.in/") ||
               url.Contains("/hubs.ly/") ||
               url.Contains("/bit.do/") ||
               url.Contains("/t.co/") ||
               url.Contains("/lnkd.in/") ||
               url.Contains("/db.tt/") ||
               url.Contains("/qr.ae/") ||
               url.Contains("/adf.ly/") ||
               url.Contains("/goo.gl/") ||
               url.Contains("/ow.ly/") ||
               url.Contains("/avana.de/") ||
               url.Contains("/msft.social/") ||
               url.Contains("/read.bi/") ||
               url.Contains("/buff.ly/") ||
               url.Contains("/dlvr.it/") ||
               url.Contains("/fb.me/") ||
               url.Contains("/tnw.me/") ||
               url.Contains("/on.mash.to/") ||
               url.Contains("/t.me/") ||
               url.Contains("/tcrn.ch/") ||
               url.Contains("/shawnw.me/") ||
               url.Contains("/ift.tt/") ||
               url.Contains("/tmi.me/") ||
               url.Contains("/gizmo.do/") ||
               url.Contains("/trib.al/") ||
               url.Contains("/lifehac.kr/") ||
               url.Contains("/ubm.io/") ||
               url.Contains("/youtu.be/") ||
               url.Contains("/mcrump.me/") ||
               url.Contains("/mspu.co/") ||
               url.Contains("/engt.co/") ||
               url.Contains("/fal.cn/") ||
               url.Contains("/ht.ly/") ||
               url.Contains("/clkon.us/") ||
               url.Contains("/plrsig.ht/") ||
               url.Contains("/cnet.co/");

            Console.WriteLine($"\t\t\tWebScraper.IsShortenedUrl(): {isShortened}");

            return isShortened;
        }

        private void ExtractMetadata(HtmlDocument doc, WebPage webPage)
        {
            var nodes = doc.DocumentNode.Descendants("meta").ToList();

            foreach (var metaNode in nodes)
            {
                if (metaNode.Attributes["http-equiv"] != null)
                {
                    string metaName = metaNode.Attributes["http-equiv"].Value;
                    if (!webPage.Metadata.ContainsKey(metaName))
                    {
                        if (metaNode.Attributes["content"] != null)
                        {
                            webPage.Metadata.Add(metaName, metaNode.Attributes["content"].Value);
                        }
                        else
                        {
                            // Ignore
                            Console.WriteLine($"\t\t\tMeta tag 'http-equiv' does not contain 'content'");
                        }
                    }
                }
                else if (metaNode.Attributes["name"] != null)
                {
                    string metaName = metaNode.Attributes["name"].Value;
                    if (!webPage.Metadata.ContainsKey(metaName))
                    {
                        if (metaNode.Attributes["content"] != null)
                        {
                            webPage.Metadata.Add(metaName, metaNode.Attributes["content"].Value);
                        }
                        else
                        {
                            // Ignore
                            Console.WriteLine($"\t\t\tMeta tag 'name' does not contain 'content'");
                        }
                    }
                }
            }

            var titleNode = doc.DocumentNode.Descendants("title").FirstOrDefault();
            if (titleNode != null)
            {
                if (!webPage.Metadata.ContainsKey("title"))
                {
                    webPage.Metadata.Add("title", m_HtmlStripper.UnHtml(titleNode.InnerHtml));
                }
            }
        }
        #endregion
    }
}
