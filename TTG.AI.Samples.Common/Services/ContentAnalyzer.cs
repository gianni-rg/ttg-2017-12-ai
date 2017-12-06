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
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
    using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Vision;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TTG.AI.Samples.Common.Infrastructure;
    using TTG.AI.Samples.Common.Infrastructure.FaceApiClient.Model.Mappers;
    using TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient;
    using TTG.AI.Samples.Common.Infrastructure.VideoIndexerClient.Model.Mappers;
    using TTG.AI.Samples.Common.Interfaces;
    using TTG.AI.Samples.Common.Model;

    public class ContentAnalyzer : IContentAnalyzer
    {
        #region Private fields
        private readonly string m_TextAnalyticsAPISubscriptionKey;
        private readonly string m_VisionAPISubscriptionKey;
        private readonly string m_VideoIndexerAPISubscriptionKey;
        private readonly string m_FaceAPISubscriptionKey;
        #endregion

        #region Constructor
        public ContentAnalyzer(string textAnalyticsAPIkey, string visionAPIkey, string videoIndexerApiKey, string faceApiKey)
        {
            m_TextAnalyticsAPISubscriptionKey = textAnalyticsAPIkey;
            m_VisionAPISubscriptionKey = visionAPIkey;
            m_VideoIndexerAPISubscriptionKey = videoIndexerApiKey;
            m_FaceAPISubscriptionKey = faceApiKey;
        }
        #endregion

        #region Methods
        public async Task<TextAnalysisResult> AnalyzeTextAsync(string text)
        {
            // See: https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/quickstarts/csharp

            var analysisResult = new TextAnalysisResult();

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): no text to analyze");
                return analysisResult;
            }

            string textToAnalyze = text;
            if (text.Length > 5000)
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): text longer than supported length. Trimming it...");
                textToAnalyze = text.Substring(0, 5000);
            }

            Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): initializing TextAnalyticsAPI");

            ITextAnalyticsAPI m_TextAnalyticsClient = new TextAnalyticsAPI
            {
                AzureRegion = AzureRegions.Westeurope,
                SubscriptionKey = m_TextAnalyticsAPISubscriptionKey
            };

            Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): detecting content language");

            var batchLanguageResult = await m_TextAnalyticsClient.DetectLanguageAsync(new BatchInput(new List<Input>() { new Input("1", textToAnalyze) })).ConfigureAwait(false);
            if (batchLanguageResult.Errors.Count > 0)
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): error while detecting language");
                foreach (var errors in batchLanguageResult.Errors)
                {
                    Console.WriteLine($"\t{errors.Message}");
                }
                return analysisResult;
            }

            analysisResult.DetectedLanguage = batchLanguageResult.Documents[0].DetectedLanguages[0].Name;
            analysisResult.DetectedLanguageScore = batchLanguageResult.Documents[0].DetectedLanguages[0].Score.GetValueOrDefault();

            Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeTextAsync(): detected language is '{analysisResult.DetectedLanguage}' ({(analysisResult.DetectedLanguageScore * 100):0.00}%)");

            Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): performing key-phrase extraction");

            var multiLanguageInput = new MultiLanguageBatchInput(new List<MultiLanguageInput>() { new MultiLanguageInput(batchLanguageResult.Documents[0].DetectedLanguages[0].Iso6391Name, "1", textToAnalyze) });
            var batchKeyphraseResult = await m_TextAnalyticsClient.KeyPhrasesAsync(multiLanguageInput).ConfigureAwait(false);

            if (batchKeyphraseResult.Errors.Count > 0)
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): error while extracting key-phrases");
                foreach (var errors in batchKeyphraseResult.Errors)
                {
                    Console.WriteLine($"\t\t\t\t{errors.Message}");
                }
                return analysisResult;
            }

            Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeTextAsync(): retrieved {batchKeyphraseResult.Documents[0].KeyPhrases.Count} key-phrases:");
            foreach (var keyphrase in batchKeyphraseResult.Documents[0].KeyPhrases)
            {
                analysisResult.KeyPhrases.Add(keyphrase);
                Console.WriteLine($"\t\t\t\t{keyphrase}");
            }

            Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): performing sentiment analysis");

            var batchSentimentResult = await m_TextAnalyticsClient.SentimentAsync(multiLanguageInput).ConfigureAwait(false);
            if (batchSentimentResult.Errors.Count > 0)
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeTextAsync(): error while detecting sentiment");
                foreach (var errors in batchSentimentResult.Errors)
                {
                    Console.WriteLine($"\t\t\t\t{errors.Message}");
                }
                return analysisResult;
            }

            analysisResult.SentimentScore = batchSentimentResult.Documents[0].Score.GetValueOrDefault();
            analysisResult.Sentiment = GetSentiment(analysisResult.SentimentScore);

            Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeTextAsync(): sentiment is '{analysisResult.Sentiment}' ({(analysisResult.SentimentScore * 100):0.00}%)");

            return analysisResult;
        }

        public async Task<ImageAnalysisResult> AnalyzeImageFromUrlAsync(string url)
        {
            var analysisResult = new ImageAnalysisResult();

            try
            {
                // USING Microsoft provided VisionClientLibrary seems not working in NET Core as-is, a fix is required for ExpandoObject
                // see: https://github.com/Microsoft/Cognitive-Vision-DotNetCore/pull/1/commits/9c4647edb400aecd4def330537d5bcd74f126111

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): initializing VisionAPI client");

                var visionApiClient = new VisionServiceClient(m_VisionAPISubscriptionKey, "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");

                var visualFeatures = new List<VisualFeature> { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType/*, VisualFeature.Tags */};
                var details = new List<string> { "Celebrities", "Landmarks" };

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): started image analysis");

                var visionApiResult = await visionApiClient.AnalyzeImageAsync(url, visualFeatures, details).ConfigureAwait(false);

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): executing OCR");

                var ocrResult = await visionApiClient.RecognizeTextAsync(url).ConfigureAwait(false);

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): performing tag identification");

                var tagsResult = await visionApiClient.GetTagsAsync(url).ConfigureAwait(false);

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): analysis completed");

                // Mapping VisionAPI Client entity to domain entity
                analysisResult.AdultContent = new ImageAnalysisAdultContentResult { AdultScore = visionApiResult.Adult.AdultScore, IsAdultContent = visionApiResult.Adult.IsAdultContent, IsRacyContent = visionApiResult.Adult.IsRacyContent, RacyScore = visionApiResult.Adult.RacyScore };
                analysisResult.Colors = new ImageAnalysisColorResult { AccentColor = visionApiResult.Color.AccentColor, DominantColorBackground = visionApiResult.Color.DominantColorBackground, DominantColorForeground = visionApiResult.Color.DominantColorForeground, IsBWImg = visionApiResult.Color.IsBWImg };
                analysisResult.Categories = visionApiResult.Categories.Select(c => new ImageAnalysisCategoryResult { Text = c.Name, Score = c.Score }).OrderByDescending(c => c.Score).ToList();
                analysisResult.Descriptions = visionApiResult.Description.Captions.Select(c => new ImageAnalysisDescriptionResult { Text = c.Text, Score = c.Confidence }).OrderByDescending(c => c.Score).ToList();

                // Merge detected tags from image analysis and image tags
                analysisResult.Tags = tagsResult.Tags.Select(t => new ImageAnalysisTagResult { Text = t.Name, Score = t.Confidence, Hint = t.Hint }).ToList();
                foreach (var t in visionApiResult.Description.Tags)
                {
                    analysisResult.Tags.Add(new ImageAnalysisTagResult { Text = t, Score = 0.0, Hint = string.Empty });
                }

                analysisResult.Faces = visionApiResult.Faces.Select(f => new ImageAnalysisFaceResult { Age = f.Age, Gender = f.Gender == "Male" ? Gender.Male : f.Gender == "female" ? Gender.Female : Gender.Unknown }).ToList();
                analysisResult.Text = ocrResult.Regions.Select(r => new ImageAnalysisTextResult() { Language = ocrResult.Language, Orientation = ocrResult.Orientation, TextAngle = ocrResult.TextAngle.GetValueOrDefault(), Text = string.Join(" ", r.Lines.Select(l => string.Join(" ", l.Words.Select(w => w.Text)))) }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeImageAsync(): an error occured while analyzing image - {ex.Message}");
            }

            return analysisResult;
        }

        public async Task<ImageAnalysisResult> AnalyzeImageFromLocalFileAsync(string localFilePath)
        {
            var analysisResult = new ImageAnalysisResult();

            try
            {
                // USING Microsoft provided VisionClientLibrary seems not working in NET Core as-is, a fix is required for ExpandoObject
                // see: https://github.com/Microsoft/Cognitive-Vision-DotNetCore/pull/1/commits/9c4647edb400aecd4def330537d5bcd74f126111

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): initializing VisionAPI client");

                var visionApiClient = new VisionServiceClient(m_VisionAPISubscriptionKey, "https://westeurope.api.cognitive.microsoft.com/vision/v1.0");

                var visualFeatures = new List<VisualFeature> { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType/*, VisualFeature.Tags */};
                var details = new List<string> { "Celebrities", "Landmarks" };

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): started image analysis");

                using (Stream imageFileStream = File.OpenRead(localFilePath))
                {
                    var visionApiResult = await visionApiClient.AnalyzeImageAsync(imageFileStream, visualFeatures, details).ConfigureAwait(false);

                    Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): executing OCR");

                    // Reset stream to re-upload it
                    imageFileStream.Seek(0, SeekOrigin.Begin);

                    var ocrResult = await visionApiClient.RecognizeTextAsync(imageFileStream).ConfigureAwait(false);

                    Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): performing tag identification");

                    // Reset stream to re-upload it
                    imageFileStream.Seek(0, SeekOrigin.Begin);

                    var tagsResult = await visionApiClient.GetTagsAsync(imageFileStream).ConfigureAwait(false);

                    Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeImageAsync(): analysis completed");

                    // Mapping VisionAPI Client entity to domain entity
                    analysisResult.AdultContent = new ImageAnalysisAdultContentResult { AdultScore = visionApiResult.Adult.AdultScore, IsAdultContent = visionApiResult.Adult.IsAdultContent, IsRacyContent = visionApiResult.Adult.IsRacyContent, RacyScore = visionApiResult.Adult.RacyScore };
                    analysisResult.Colors = new ImageAnalysisColorResult { AccentColor = visionApiResult.Color.AccentColor, DominantColorBackground = visionApiResult.Color.DominantColorBackground, DominantColorForeground = visionApiResult.Color.DominantColorForeground, IsBWImg = visionApiResult.Color.IsBWImg };
                    analysisResult.Categories = visionApiResult.Categories.Select(c => new ImageAnalysisCategoryResult { Text = c.Name, Score = c.Score }).OrderByDescending(c => c.Score).ToList();
                    analysisResult.Descriptions = visionApiResult.Description.Captions.Select(c => new ImageAnalysisDescriptionResult { Text = c.Text, Score = c.Confidence }).OrderByDescending(c => c.Score).ToList();

                    // Merge detected tags from image analysis and image tags
                    analysisResult.Tags = tagsResult.Tags.Select(t => new ImageAnalysisTagResult { Text = t.Name, Score = t.Confidence, Hint = t.Hint }).ToList();
                    foreach (var t in visionApiResult.Description.Tags)
                    {
                        analysisResult.Tags.Add(new ImageAnalysisTagResult { Text = t, Score = 0.0, Hint = string.Empty });
                    }
                    
                    analysisResult.Faces = visionApiResult.Faces.Select(f => new ImageAnalysisFaceResult { Age = f.Age, Gender = f.Gender == "Male" ? Gender.Male : f.Gender == "female" ? Gender.Female : Gender.Unknown }).ToList();
                    analysisResult.Text = ocrResult.Regions.Select(r => new ImageAnalysisTextResult() { Language = ocrResult.Language, Orientation = ocrResult.Orientation, TextAngle = ocrResult.TextAngle.GetValueOrDefault(), Text = string.Join(" ", r.Lines.Select(l => string.Join(" ", l.Words.Select(w => w.Text)))) }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeImageAsync(): an error occured while analyzing image - {ex.Message}");
            }

            return analysisResult;
        }

        public async Task<FaceAnalysisResult> AnalyzeFacesFromLocalFileAsync(string localFilePath)
        {

            var analysisResult = new FaceAnalysisResult();

            // Call the Face API.
            try
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeFacesFromLocalFileAsync(): initializing FaceAPI client");

                IFaceServiceClient faceServiceClient = new FaceServiceClient(m_FaceAPISubscriptionKey, "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeFacesFromLocalFileAsync(): starting analysis");

                using (Stream imageFileStream = File.OpenRead(localFilePath))
                {
                    // The list of Face attributes to return.
                    IEnumerable<FaceAttributeType> faceAttributes = new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.FacialHair, FaceAttributeType.HeadPose };

                    var faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);

                    Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeFacesFromLocalFileAsync(): analysis completed. Detected {faces.Length} face(s)");

                    // Mapping FaceAPI Client entity to domain entity
                    analysisResult = faces.MapToDomain();
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException fex)
            {
                Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeFacesFromLocalFileAsync(): an error occured while analyzing picture - {fex.Message}");
            }
            // Catch and display all other errors.
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeFacesFromLocalFileAsync(): a generic error occured while analyzing picture - {ex.Message}");
            }


            return analysisResult;
        }


        public async Task<VideoAnalysisResult> AnalyzeVideoFromUrlAsync(string videoId, string url)
        {
            // See: https://docs.microsoft.com/en-us/azure/cognitive-services/video-indexer/video-indexer-use-apis

            VideoAnalysisResult analysisResult = null;

            try
            {
                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeVideoAsync(): initializing VideoIndexerAPI client");

                var videoIndexerApiClient = new VideoIndexerApiClient(m_VideoIndexerAPISubscriptionKey);

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeVideoAsync(): starting analysis");

                var videoIndexerAnalysisResult = await videoIndexerApiClient.AnalyzeVideoAsync(videoId, url).ConfigureAwait(false);

                Console.WriteLine("\t\t\tContentAnalyzer.AnalyzeVideoAsync(): analysis completed");
                
                // Mapping VideoIndexerAPI Client entity to domain entity
                analysisResult = videoIndexerAnalysisResult.MapToDomain();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t\tContentAnalyzer.AnalyzeVideoAsync(): an error occured while analyzing video - {ex.Message}");
            }
            return analysisResult;
        }
        #endregion

        #region Private methods
        private SentimentValue GetSentiment(double sentimentScore)
        {
            if (sentimentScore <= 0.35)
            {
                return SentimentValue.Negative;
            }
            else if (sentimentScore > 0.35 && sentimentScore < 0.65)
            {
                return SentimentValue.Neutral;
            }
            else
            {
                return SentimentValue.Positive;
            }
        }
        #endregion
    }
}
