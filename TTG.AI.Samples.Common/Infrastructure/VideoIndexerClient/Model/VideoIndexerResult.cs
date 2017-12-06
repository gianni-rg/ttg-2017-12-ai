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

namespace TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient.Model
{
    using System;

    public class VideoIndexerResult
    {
        public string accountId { get; set; }
        public string id { get; set; }
        public object partition { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string userName { get; set; }
        public DateTime createTime { get; set; }
        public string organization { get; set; }
        public string privacyMode { get; set; }
        public string state { get; set; }
        public bool isOwned { get; set; }
        public bool isEditable { get; set; }
        public bool isBase { get; set; }
        public int durationInSeconds { get; set; }
        public SummarizedInsights summarizedInsights { get; set; }
        public Breakdown[] breakdowns { get; set; }
        public Social social { get; set; }
    }

    public class SummarizedInsights
    {
        public string name { get; set; }
        public string shortId { get; set; }
        public int privacyMode { get; set; }
        public Duration duration { get; set; }
        public string thumbnailUrl { get; set; }
        public Face[] faces { get; set; }
        public Topic[] topics { get; set; }
        public Sentiment[] sentiments { get; set; }
        public AudioEffect[] audioEffects { get; set; }
        public Annotation[] annotations { get; set; }
        public Brand[] brands { get; set; }
    }

    public class Duration
    {
        public string time { get; set; }
        public float seconds { get; set; }
    }

    public class Face
    {
        public int id { get; set; }
        public string shortId { get; set; }
        public object bingId { get; set; }
        public float confidence { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string thumbnailUrl { get; set; }
        public string thumbnailFullUrl { get; set; }
        public Appearance[] appearances { get; set; }
        public float seenDuration { get; set; }
        public float seenDurationRatio { get; set; }
    }

    public class Topic
    {
        public string name { get; set; }
        public Appearance[] appearances { get; set; }
        public bool isTranscript { get; set; }
        public int id { get; set; }
    }

    public class Appearance
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        public float startSeconds { get; set; }
        public float endSeconds { get; set; }
    }

    public class Sentiment
    {
        public string sentimentKey { get; set; }
        public Appearance[] appearances { get; set; }
        public float seenDurationRatio { get; set; }
    }


    public class AudioEffect
    {
        public string audioEffectKey { get; set; }
        public Appearance[] appearances { get; set; }
        public float seenDurationRatio { get; set; }
        public float seenDuration { get; set; }
    }

    public class Annotation
    {
        public string name { get; set; }
        public Appearance[] appearances { get; set; }
        public TimeRange[] timeRanges { get; set; }
        public AdjustedTimeRange[] adjustedTimeRanges { get; set; }
    }

    public class Brand
    {
        public string name { get; set; }
        public string wikiId { get; set; }
        public string wikiUrl { get; set; }
        public string description { get; set; }
        public Appearance[] appearances { get; set; }
    }

    public class Social
    {
        public bool likedByUser { get; set; }
        public int likes { get; set; }
        public int views { get; set; }
    }

    public class Breakdown
    {
        public string accountId { get; set; }
        public string id { get; set; }
        public string state { get; set; }
        public string processingProgress { get; set; }
        public string failureCode { get; set; }
        public string failureMessage { get; set; }
        public object externalId { get; set; }
        public object externalUrl { get; set; }
        public object metadata { get; set; }
        public Insights insights { get; set; }
        public string thumbnailUrl { get; set; }
        public string publishedUrl { get; set; }
        public string viewToken { get; set; }
        public string sourceLanguage { get; set; }
        public string language { get; set; }
    }

    public class Insights
    {
        public TranscriptBlock[] transcriptBlocks { get; set; }
        public object[] topics { get; set; }
        public Brand[] brands { get; set; }
        public Face[] faces { get; set; }
        public Participant[] participants { get; set; }
        public ContentModeration contentModeration { get; set; }
        public AudioEffectsCategory[] audioEffectsCategories { get; set; }
    }

    public class ContentModeration
    {
        public float adultClassifierValue { get; set; }
        public float racyClassifierValue { get; set; }
        public int bannedWordsCount { get; set; }
        public float bannedWordsRatio { get; set; }
        public bool reviewRecommended { get; set; }
        public bool isAdult { get; set; }
    }

    public class TranscriptBlock
    {
        public int id { get; set; }
        public Line[] lines { get; set; }
        public object[] sentimentIds { get; set; }
        public object[] thumbnailsIds { get; set; }
        public float sentiment { get; set; }
        public Face[] faces { get; set; }
        public Ocr[] ocrs { get; set; }
        public AudioEffectInstance[] audioEffectInstances { get; set; }
        public Scene[] scenes { get; set; }
        public Annotation[] annotations { get; set; }
    }

    public class Line
    {
        public int id { get; set; }
        public TimeRange timeRange { get; set; }
        public AdjustedTimeRange adjustedTimeRange { get; set; }
        public int participantId { get; set; }
        public string text { get; set; }
        public bool isIncluded { get; set; }
        public float confidence { get; set; }
    }

    public class OcrLine
    {
        public int id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string language { get; set; }
        public string textData { get; set; }
        public float confidence { get; set; }
    }

    public class TimeRange
    {
        public string start { get; set; }
        public string end { get; set; }
    }

    public class AdjustedTimeRange
    {
        public string start { get; set; }
        public string end { get; set; }
    }


    public class Range
    {
        public TimeRange timeRange { get; set; }
        public AdjustedTimeRange adjustedTimeRange { get; set; }
    }

    public class Ocr
    {
        public TimeRange timeRange { get; set; }
        public AdjustedTimeRange adjustedTimeRange { get; set; }
        public OcrLine[] lines { get; set; }
    }

    public class AudioEffectInstance
    {
        public int type { get; set; }
        public Range[] ranges { get; set; }
    }

    public class Scene
    {
        public int id { get; set; }
        public TimeRange timeRange { get; set; }
        public string keyFrame { get; set; }
        public Shot[] shots { get; set; }
    }

    public class Shot
    {
        public int id { get; set; }
        public TimeRange timeRange { get; set; }
        public string keyFrame { get; set; }
    }

    public class OcrTimeRange
    {
        public string start { get; set; }
        public string end { get; set; }
    }

    public class OcrAdjustedTimeRange
    {
        public string start { get; set; }
        public string end { get; set; }
    }

    public class Participant
    {
        public int id { get; set; }
        public string name { get; set; }
        public object pictureUrl { get; set; }
    }

    public class AudioEffectsCategory
    {
        public int type { get; set; }
        public string key { get; set; }
    }
}
