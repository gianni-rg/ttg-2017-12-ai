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

namespace TTG.AI.Samples.Twitter.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tweetinvi;
    using Tweetinvi.Models;
    using Tweetinvi.Parameters;
    using TTG.AI.Samples.Common.Services.Helpers;

    public class TwitterMonitor
    {
        // See: https://github.com/linvi/tweetinvi
        //      https://github.com/linvi/tweetinvi/wiki/Timelines

        private HTMLStripper m_HtmlStripper;

        public TwitterMonitor(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            // Set up your credentials (https://apps.twitter.com)
            Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

            m_HtmlStripper = new HTMLStripper();
        }

        public Task<IList<Model.Tweet>> RetrieveTweetsAsync()
        {
            return Task.Run(() =>
            {
                IList<Model.Tweet> retrievedTweets = new List<Model.Tweet>();

                // Get Home Timeline

                // Get more control over the request with a HomeTimelineParameters
                var homeTimelineParameters = new HomeTimelineParameters
                {
                    MaximumNumberOfTweetsToRetrieve = 100,
                    // ... setup additional parameters
                };

                var tweets = Timeline.GetHomeTimeline(homeTimelineParameters);
                foreach (var tweet in tweets)
                {
                    retrievedTweets.Add(MapTweetinviTweetToTweet(tweet));
                }

                return retrievedTweets;
            });
        }

        private Model.Tweet MapTweetinviTweetToTweet(ITweet tweet)
        {
            bool valid = true;

            if (tweet == null)
            {
                Console.WriteLine("TwitterMonitor.MapTweetinviTweetToTweet(): tweet is null");

                valid = false;
            }

            if (valid && string.IsNullOrEmpty(tweet.FullText))
            {
                Console.WriteLine("TwitterMonitor.MapTweetinviTweetToTweet(): tweet is not valid (No text)");
                valid = false;
            }

            if (!valid)
            {
                Console.WriteLine("TwitterMonitor.MapTweetinviTweetToTweet(): found invalid tweet. Ignore.");
                return null;
            }

            var domainTweet = new Model.Tweet
            {
                Id = tweet.IdStr,
                Content = m_HtmlStripper.UnHtml(tweet.FullText),
                Created = tweet.CreatedAt.ToUniversalTime(),
                Url = tweet.Url,
                AuthorId = tweet.CreatedBy.UserIdentifier.IdStr,
            };

            foreach (var entity in tweet.Entities.Hashtags)
            {
                domainTweet.TextualEntities[Model.TweetEntityTextualType.Hashtags].Add(entity.Text);
            }
            foreach (var entity in tweet.Entities.Medias)
            {
                if (entity.MediaType == "photo")
                {
                    domainTweet.MediaEntities[Model.TweetEntityMediaType.Image].Add(entity.MediaURL);
                }
                else if (entity.MediaType == "video")
                {
                    // Get the highest quality video
                    int mediaIndex = 0;
                    int maxBitrate = 0;
                    for (int i = 0; i < entity.VideoDetails.Variants.Length; i++)
                    {
                        if (entity.VideoDetails.Variants[i].Bitrate > maxBitrate)
                        {
                            mediaIndex = i;
                            maxBitrate = entity.VideoDetails.Variants[i].Bitrate;
                        }
                    }

                    domainTweet.MediaEntities[Model.TweetEntityMediaType.Video].Add(entity.VideoDetails.Variants[mediaIndex].URL);
                }
                else
                {
                    // Ignore
                }
            }
            foreach (var entity in tweet.Entities.Symbols)
            {
                domainTweet.TextualEntities[Model.TweetEntityTextualType.Symbols].Add(entity.Text);
            }
            foreach (var entity in tweet.Entities.Urls)
            {
                domainTweet.TextualEntities[Model.TweetEntityTextualType.Urls].Add(entity.ExpandedURL);
            }
            foreach (var entity in tweet.Entities.UserMentions)
            {
                domainTweet.TextualEntities[Model.TweetEntityTextualType.UserMentions].Add(entity.IdStr);
            }
            return domainTweet;
        }
    }
}
