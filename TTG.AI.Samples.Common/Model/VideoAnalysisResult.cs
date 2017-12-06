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

namespace TTG.AI.Samples.Common.Model
{
    using System;
    using System.Collections.Generic;

    public class VideoAnalysisResult
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public double LengthInMinutes { get; set; }
        public string Language { get; set; }
        public IList<Face> Faces { get; set; }
        public IList<Topic> Topics { get; set; }
        public IList<Sentiment> Sentiments { get; set; }
        public IList<Annotation> Annotations { get; set; }
        public IList<Brand> Brands { get; set; }
        public string Transcript { get; set; }
        public ContentModeration ContentModeration { get; set; }
        public IList<string> Text { get; set; }
    }

    public class Face
    {
        public int Id { get; set; }
        public float Confidence { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public IList<Appearance> Appearances { get; set; }
        public float SeenDuration { get; set; }
        public float SeenDurationRatio { get; set; }
    }

    public class Topic
    {
        public string Name { get; set; }
        public IList<Appearance> Appearances { get; set; }
        public bool IsTranscript { get; set; }
        public int Id { get; set; }
    }

    public class Appearance
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public float StartSeconds { get; set; }
        public float EndSeconds { get; set; }
    }

    public class Sentiment
    {
        public string SentimentKey { get; set; }
        public IList<Appearance> Appearances { get; set; }
        public float SeenDurationRatio { get; set; }
    }

    public class Annotation
    {
        public string Name { get; set; }
        public IList<Appearance> Appearances { get; set; }
        public IList<TimeRange> TimeRanges { get; set; }
    }

    public class Brand
    {
        public string Name { get; set; }
        public string WikiId { get; set; }
        public string WikiUrl { get; set; }
        public string Description { get; set; }
        public IList<Appearance> Appearances { get; set; }
    }

    public class ContentModeration
    {
        public float AdultClassifierValue { get; set; }
        public float RacyClassifierValue { get; set; }
        public int BannedWordsCount { get; set; }
        public float BannedWordsRatio { get; set; }
        public bool ReviewRecommended { get; set; }
        public bool IsAdult { get; set; }
    }

    public class TimeRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
