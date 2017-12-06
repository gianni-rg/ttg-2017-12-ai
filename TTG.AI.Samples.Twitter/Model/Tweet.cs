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

namespace TTG.AI.Samples.Twitter.Model
{
    using System;
    using System.Collections.Generic;

    public class Tweet
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public string Url { get; set; }
        public DateTime Created { get; set; }
        public Dictionary<TweetEntityTextualType, IList<string>> TextualEntities { get; }
        public Dictionary<TweetEntityMediaType, IList<string>> MediaEntities{ get; }

        public Tweet()
        {
            TextualEntities = new Dictionary<TweetEntityTextualType, IList<string>>
            {
                { TweetEntityTextualType.Hashtags, new List<string>() },
                { TweetEntityTextualType.Symbols, new List<string>() },
                { TweetEntityTextualType.Urls, new List<string>() },
                { TweetEntityTextualType.UserMentions, new List<string>() }
            };

            MediaEntities = new Dictionary<TweetEntityMediaType, IList<string>>
            {
                { TweetEntityMediaType.Image, new List<string>() },
                { TweetEntityMediaType.Video, new List<string>() },
            };
        }

        public override int GetHashCode()
        {
            int hash = 269;
            hash = (hash * 47) + Content.GetHashCode();
            return hash;
        }
    }
}
