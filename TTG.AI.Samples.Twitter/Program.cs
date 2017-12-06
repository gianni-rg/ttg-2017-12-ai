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

namespace TTG.AI.Samples.Twitter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using System.IO;
    using System.Reflection;
    using TTG.AI.Samples.Common;
    using TTG.AI.Samples.Twitter.Services;
    using System.Collections.Generic;
    using TTG.AI.Samples.Twitter.Model;
    using TTG.AI.Samples.Common.Model;
    using TTG.AI.Samples.Common.Infrastructure;

    class Program
    {
        private static TwitterMonitor m_TwitterMonitor;
        private static WebScraper m_WebScraper;
        private static ContentAnalyzer m_ContentAnalyzer;

        public static async Task Main(string[] args)
        {
            WriteHeader();

            // Get /bin folder path
            // See: https://github.com/dotnet/project-system/issues/2239
            var executableFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // Load configurable parameters from a json file
            // !! REMEMBER TO CONFIGURE YOUR CognitiveServices API keys and Twitter credentials !!
            var configuration = LoadConfigurationFromJsonFile(Path.Combine(executableFolder, "appsettings.json"));

            // Setup a folder where to store analysis results
            string resultsPath = Path.Combine(executableFolder, "Results");
            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
            }

            // Initialize services and components. For simplicity, we do not use IoC/DI.
            SetupServices(configuration);

            // Retrieve a list of tweet to analyze, either from online stream or from previous saved files
            IList<Tweet> tweets = await GetTweets(args, resultsPath);

            // Analyze tweets
            foreach (var tweet in tweets)
            {
                var textAnalysisCollection = new List<TextAnalysisResult>();
                var imageAnalysisCollection = new List<ImageAnalysisResult>();
                var videoAnalysisCollection = new List<VideoAnalysisResult>();

                string filePrefix = $"{tweet.Created.ToString("yyyyMMdd_HHmmss")}_{tweet.Id}";

                // Analyze tweet content
                Console.WriteLine();
                Console.WriteLine("Analyzing tweet content:");
                Console.WriteLine(tweet.Content);
                textAnalysisCollection.Add(await PerformTweetContentAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}_content_results.txt", tweet));

                // Download & analyze the links pointed by the tweet (if any)
                int entityId = 0;
                int count = tweet.TextualEntities[Model.TweetEntityTextualType.Urls].Count;
                if (count > 0)
                {
                    Console.WriteLine($"\tAnalyzing links pointed by the tweet (found: {count})");

                    foreach (var link in tweet.TextualEntities[Model.TweetEntityTextualType.Urls])
                    {
                        entityId++;
                        textAnalysisCollection.Add(await PerformTweetLinkContentAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}_link_{entityId}", link));
                    }
                }

                // Download & analyze images pointed by the tweet (if any)
                count = tweet.MediaEntities[Model.TweetEntityMediaType.Image].Count;
                entityId = 0;
                if (count > 0)
                {
                    Console.WriteLine($"\tAnalyzing images pointed by the tweet (found: {count})");
                    foreach (var link in tweet.MediaEntities[Model.TweetEntityMediaType.Image])
                    {
                        entityId++;

                        var resImg = await PerformTweetImageAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}", link);

                        Console.WriteLine($"\t\t\t    Adult: {resImg.AdultContent.IsAdultContent} ({resImg.AdultContent.AdultScore:0.00})");
                        Console.WriteLine($"\t\t\t     Racy: {resImg.AdultContent.IsRacyContent} ({resImg.AdultContent.RacyScore:0.00})");
                        Console.WriteLine($"\t\t\t   Colors");
                        Console.WriteLine($"\t\t\t         Accent: {resImg.Colors.AccentColor}");
                        Console.WriteLine($"\t\t\t     Background: {resImg.Colors.DominantColorBackground}");
                        Console.WriteLine($"\t\t\t     Foreground: {resImg.Colors.DominantColorForeground}");
                        Console.WriteLine($"\t\t\t            B/W: {resImg.Colors.IsBWImg}");
                        Console.WriteLine($"\t\t\tDescriptions");
                        foreach (var description in resImg.Descriptions)
                        {
                            Console.WriteLine($"\t\t\t            {description.Text}");
                        }
                        Console.WriteLine($"\t\t\t Categories");
                        foreach (var cat in resImg.Categories)
                        {
                            Console.WriteLine($"\t\t\t            {cat.Text} ({cat.Score:0.00})");
                        }
                        Console.WriteLine($"\t\t\t       Tags");
                        foreach (var tag in resImg.Tags)
                        {
                            Console.WriteLine($"\t\t\t            {tag.Text} ({tag.Score:0.00})");
                        }
                        Console.WriteLine($"\t\t\t   OCR Text");
                        foreach (var text in resImg.Text)
                        {
                            Console.WriteLine($"\t\t\t            {text.Text}");
                        }
                        Console.WriteLine($"\t\t\t      Faces ({resImg.Faces.Count})");
                        foreach (var face in resImg.Faces)
                        {
                            Console.WriteLine($"\t\t\t       Age: {face.Age}");
                            Console.WriteLine($"\t\t\t    Gender: {face.Gender}");
                        }
						
						Console.WriteLine();

                        imageAnalysisCollection.Add(resImg);
                    }
                }

                // Download & analyze videos pointed by the tweet (if any)
                count = tweet.MediaEntities[Model.TweetEntityMediaType.Video].Count;
                entityId = 0;
                if (count > 0)
                {
                    Console.WriteLine($"\tAnalyzing videos pointed by the tweet (found {count})");
                    foreach (var link in tweet.MediaEntities[Model.TweetEntityMediaType.Video])
                    {
                        entityId++;
                        videoAnalysisCollection.Add(await PerformTweetVideoAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}_video_{entityId}", link));
                    }
                }
            }

            m_WebScraper.Dispose();

            Console.WriteLine();
            Console.WriteLine("Analysis completed. Press any key to terminate...");
            Console.ReadLine();
        }

        private static async Task<IList<Tweet>> GetTweets(string[] args, string resultsPath)
        {
            // Try to load feed items from local file (if specified)
            string sourceFile = string.Empty;
            IList<Model.Tweet> tweets = null;
            if (args.Length == 1)
            {
                var tweetList = new List<Model.Tweet>();
                var feedFiles = Directory.GetFiles(args[0], "*_tweets.txt");
                foreach (var file in feedFiles)
                {
                    tweetList.AddRange(await FileSerializer.LoadFromFileAsync<IList<Model.Tweet>>(file));
                }
                tweets = tweetList;
            }

            // Retrieve recent tweets from the configured account
            if (tweets == null)
            {
                tweets = await m_TwitterMonitor.RetrieveTweetsAsync();

                // Store tweets for later user (i.e. repeat analysis on the same content)
                await FileSerializer.StoreInFileAsync(tweets, $"{resultsPath}\\{DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")}_tweets.txt");
            }

            return tweets;
        }

        private static async Task<VideoAnalysisResult> PerformTweetVideoAnalysisIfNotAlreadyDoneAsync(string partialFileName, string link)
        {
            var fileExt = link.Substring(link.Length - 3);
            string destinationFile = $"{partialFileName}.{fileExt}";
            if (!File.Exists(destinationFile))
            {
                await m_WebScraper.DownloadResourceAsync(link, destinationFile);
            }

            // Analyze video content pointed by tweet (if not already done)

            Console.WriteLine($"\t\t{link}");
            destinationFile = $"{partialFileName}_results.txt";
            if (!File.Exists(destinationFile))
            {
                // VIDEO ANALYSIS DISABLED, TOO MUCH TIME FOR DEMO
                return null;
                //var linkContentAnalysisResult = await m_ContentAnalyzer.AnalyzeVideoAsync(destinationFile, link);

                //// Store results for later user
                //await FileSerializer.StoreInFileAsync(linkContentAnalysisResult, destinationFile);

                //return linkContentAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<VideoAnalysisResult>(destinationFile);
            }
        }

        private static async Task<ImageAnalysisResult> PerformTweetImageAnalysisIfNotAlreadyDoneAsync(string partialFileName, string link)
        {
            // Download video locally (for later use)
            string fileExt = link.Substring(link.Length - 3);
            string destinationFile = $"{partialFileName}.{fileExt}";

            if (!File.Exists(destinationFile))
            {
                await m_WebScraper.DownloadResourceAsync(link, destinationFile);
            }

            // Analyze the content pointed by link in the tweet (if not already done)
            Console.WriteLine($"\t\t{link}");
            destinationFile = $"{partialFileName}_results.txt";
            if (!File.Exists(destinationFile))
            {
                var linkContentAnalysisResult = await m_ContentAnalyzer.AnalyzeImageFromUrlAsync(link);

                // Store results for later user
                await FileSerializer.StoreInFileAsync(linkContentAnalysisResult, destinationFile);

                return linkContentAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<ImageAnalysisResult>(destinationFile);
            }
        }

        private static async Task<TextAnalysisResult> PerformTweetLinkContentAnalysisIfNotAlreadyDoneAsync(string fileName, string link)
        {
            string destinationFile = $"{fileName}.txt";

            WebPage webPage = await FileSerializer.LoadFromFileAsync<WebPage>(destinationFile);
            if (webPage == null)
            {
                // Potentially, each web page may need a dedicated text extractor for optimal results
                // Here, for example, we have a customized extractor for TechCrunch/Mashable/Twitter pages
                webPage = await m_WebScraper.DownloadWebPageAsync(link);

                if (webPage == null)
                {
                    return null;
                }

                // Store webpage for later user
                await FileSerializer.StoreInFileAsync(webPage, destinationFile);
            }

            // Analyze the content pointed by link in the tweet
            Console.WriteLine($"\t\t{link}");
            destinationFile = $"{fileName}_results.txt";
            if (!File.Exists(destinationFile))
            {
                var linkContentAnalysisResult = await m_ContentAnalyzer.AnalyzeTextAsync(webPage.Text);

                // Store results for later user
                await FileSerializer.StoreInFileAsync(linkContentAnalysisResult, destinationFile);

                return linkContentAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<TextAnalysisResult>(destinationFile);
            }
        }

        private static async Task<TextAnalysisResult> PerformTweetContentAnalysisIfNotAlreadyDoneAsync(string destinationFile, Tweet tweet)
        {
            if (!File.Exists(destinationFile))
            {
                var contentAnalysisResult = await m_ContentAnalyzer.AnalyzeTextAsync(tweet.Content);

                // Store results for later user
                await FileSerializer.StoreInFileAsync(contentAnalysisResult, destinationFile);

                return contentAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<TextAnalysisResult>(destinationFile);
            }
        }
        
        private static void SetupServices(IConfigurationRoot configuration)
        {
            m_TwitterMonitor = new TwitterMonitor(configuration["Twitter:ConsumerKey"], configuration["Twitter:ConsumerSecret"], configuration["Twitter:AccessToken"], configuration["Twitter:AccessTokenSecret"]);
            m_WebScraper = new WebScraper();
            m_ContentAnalyzer = new ContentAnalyzer(configuration["CognitiveServicesKeys:TextAnalyticsAPI"], configuration["CognitiveServicesKeys:VisionAPI"], configuration["CognitiveServicesKeys:VideoIndexerAPI"], configuration["CognitiveServicesKeys:FaceAPI"]);
        }

        private static IConfigurationRoot LoadConfigurationFromJsonFile(string file)
        {
            // See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration?tabs=basicconfiguration
            var builder = new ConfigurationBuilder().AddJsonFile(file);
            return builder.Build();
        }

        private static void WriteHeader()
        {
            Console.WriteLine("TTG - December 2017 - Microsoft Cognitive Services demo - Twitter Analyzer");
            Console.WriteLine("Copyright (C) 2017 Gianni Rosa Gallina. Released under MIT license.");
            Console.WriteLine("See LICENSE file for details.");
            Console.WriteLine("==========================================================================");
        }
    }
}
