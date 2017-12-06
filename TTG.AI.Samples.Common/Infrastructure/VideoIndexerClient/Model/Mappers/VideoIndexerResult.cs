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

namespace TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient.Model.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TTG.AI.Samples.Common.Model;

    public static partial class Mappers
    {
        public static VideoAnalysisResult MapToDomain(this VideoIndexerResult videoIndexerAnalysisResult)
        {
            var domainEntity = new VideoAnalysisResult()
            {
                Name = videoIndexerAnalysisResult.name,
                Description = videoIndexerAnalysisResult.description,
                Language = GetLanguageFromBreakdown(videoIndexerAnalysisResult),
                Topics = videoIndexerAnalysisResult.summarizedInsights.topics?.Select(MapToDomain).ToList(),
                Annotations = videoIndexerAnalysisResult.summarizedInsights.annotations?.Select(MapToDomain).ToList(),
                Brands = videoIndexerAnalysisResult.summarizedInsights.brands?.Select(MapToDomain).ToList(),
                Faces = videoIndexerAnalysisResult.summarizedInsights.faces?.Select(MapToDomain).ToList(),
                Sentiments = videoIndexerAnalysisResult.summarizedInsights.sentiments?.Select(MapToDomain).ToList(),
                ThumbnailUrl = videoIndexerAnalysisResult.summarizedInsights.thumbnailUrl,
                LengthInMinutes = (double)videoIndexerAnalysisResult.durationInSeconds / 60,
                ContentModeration = GetContentModerationFromBreakdown(videoIndexerAnalysisResult),
                Transcript = GetTranscriptFromBreakdown(videoIndexerAnalysisResult),
                Text = GetTextFromBreakdown(videoIndexerAnalysisResult),
            };

            return domainEntity;
        }

        private static string GetTranscriptFromBreakdown(VideoIndexerResult videoIndexerAnalysisResult)
        {
            if (videoIndexerAnalysisResult.breakdowns == null || videoIndexerAnalysisResult.breakdowns.Length == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            foreach (var block in videoIndexerAnalysisResult.breakdowns[0].insights.transcriptBlocks)
            {
                foreach (var line in block.lines)
                {
                    if(string.IsNullOrEmpty(line.text))
                    {
                        continue;
                    }
                    stringBuilder.AppendLine(line.text);
                }
            }

            return stringBuilder.ToString();
        }

        private static IList<string> GetTextFromBreakdown(VideoIndexerResult videoIndexerAnalysisResult)
        {
            var list = new List<string>();
            if (videoIndexerAnalysisResult.breakdowns == null || videoIndexerAnalysisResult.breakdowns.Length == 0)
            {
                return list;
            }

            foreach (var block in videoIndexerAnalysisResult.breakdowns[0].insights.transcriptBlocks)
            {
                foreach (var ocr in block.ocrs)
                {
                    foreach (var line in ocr.lines)
                    {
                        if (string.IsNullOrEmpty(line.textData))
                        {
                            continue;
                        }
                        list.Add(line.textData);
                    }
                }
            }

            return list;
        }

        private static string GetLanguageFromBreakdown(VideoIndexerResult videoIndexerAnalysisResult)
        {
            if (videoIndexerAnalysisResult.breakdowns == null || videoIndexerAnalysisResult.breakdowns.Length == 0)
            {
                return "unknown";
            }

            return videoIndexerAnalysisResult.breakdowns[0].language;
        }

        private static ContentModeration GetContentModerationFromBreakdown(VideoIndexerResult videoIndexerAnalysisResult)
        {
            if (videoIndexerAnalysisResult.breakdowns == null || videoIndexerAnalysisResult.breakdowns.Length == 0)
            {
                return new ContentModeration();
            }

            return new ContentModeration
            {
                AdultClassifierValue = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.adultClassifierValue,
                BannedWordsCount = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.bannedWordsCount,
                BannedWordsRatio = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.bannedWordsRatio,
                IsAdult = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.isAdult,
                RacyClassifierValue = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.racyClassifierValue,
                ReviewRecommended = videoIndexerAnalysisResult.breakdowns[0].insights.contentModeration.reviewRecommended,
            };
        }

        private static Annotation MapToDomain(this Model.Annotation a)
        {
            return new Annotation
            {
                Name = a.name,
                TimeRanges = a.timeRanges?.Select(MapToDomain).ToList(),
                Appearances = a.appearances?.Select(MapToDomain).ToList(),
            };
        }

        private static Topic MapToDomain(this Model.Topic t)
        {
            return new Topic
            {
                Name = t.name,
                Id = t.id,
                IsTranscript = t.isTranscript,
                Appearances = t.appearances?.Select(MapToDomain).ToList(),
            };
        }

        private static Brand MapToDomain(this Model.Brand b)
        {
            return new Brand
            {
                Appearances = b.appearances?.Select(MapToDomain).ToList(),
                Description = b.description,
                Name = b.name,
                WikiId = b.wikiId,
                WikiUrl = b.wikiUrl,
            };
        }
        private static Sentiment MapToDomain(this Model.Sentiment s)
        {
            return new Sentiment
            {
                SentimentKey = s.sentimentKey,
                Appearances = s.appearances?.Select(MapToDomain).ToList(),
                SeenDurationRatio = s.seenDurationRatio,
            };
        }

        private static Face MapToDomain(this Model.Face f)
        {
            return new Face
            {
                Id = f.id,
                Name = f.name,
                Confidence = f.confidence,
                Description = f.description,
                Title = f.title,
                ThumbnailUrl = f.thumbnailFullUrl,
                SeenDuration = f.seenDuration,
                SeenDurationRatio = f.seenDurationRatio,
                Appearances = f.appearances?.Select(MapToDomain).ToList(),
            };
        }

        private static Appearance MapToDomain(this Model.Appearance a)
        {
            return new Appearance { Start = TimeSpan.Parse(a.startTime), End = TimeSpan.Parse(a.endTime), StartSeconds = a.startSeconds, EndSeconds = a.endSeconds };
        }

        private static TimeRange MapToDomain(this Model.TimeRange tr)
        {
            return new TimeRange { Start = TimeSpan.Parse(tr.start), End = TimeSpan.Parse(tr.end) };
        }
    }
}
