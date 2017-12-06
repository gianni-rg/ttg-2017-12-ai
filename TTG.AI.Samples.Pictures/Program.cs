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

namespace TTG.AI.Samples.Pictures
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using System.IO;
    using System.Reflection;
    using TTG.AI.Samples.Common;
    using System.Collections.Generic;
    using TTG.AI.Samples.Common.Model;
    using TTG.AI.Samples.Common.Infrastructure;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.Primitives;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp.Drawing;


    class Program
    {
        private static ContentAnalyzer m_ContentAnalyzer;

        public static async Task Main(string[] args)
        {
            WriteHeader();

            // Get /bin folder path
            // See: https://github.com/dotnet/project-system/issues/2239
            var executableFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // Load configurable parameters from a json file
            // !! REMEMBER TO CONFIGURE YOUR CognitiveServices API keys !!
            var configuration = LoadConfigurationFromJsonFile(Path.Combine(executableFolder, "appsettings.json"));

            // Setup a folder where to store analysis results
            string resultsPath = Path.Combine(executableFolder, "Results");
            if (!Directory.Exists(resultsPath))
            {
                Directory.CreateDirectory(resultsPath);
            }

            // Initialize services and components. For simplicity, we do not use IoC/DI.
            SetupServices(configuration);

            // Retrieve a list of picture to analyze
            IList<string> pictures = GetPictures();

            var faceDetailsList = new List<FaceDetails>();

            // Analyze pictures
            foreach (var picPath in pictures)
            {
                string filePrefix = $"{Path.GetFileName(picPath)}";

                var destinationCopyFileName = $"{resultsPath}\\{filePrefix}";
                if (!File.Exists(destinationCopyFileName))
                {
                    File.Copy(picPath, destinationCopyFileName);
                }

                var resImg = await PerformImageAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}", picPath);

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

                var resFace = await PerformFaceAnalysisIfNotAlreadyDoneAsync($"{resultsPath}\\{filePrefix}", picPath);
                int count = 1;
                foreach (var f in resFace.Faces)
                {
                    Console.WriteLine($"\t\t\tFace {count++}:");
                    Console.WriteLine($"\t\t\t      Age: {f.Age}");
                    Console.WriteLine($"\t\t\t  Emotion: {f.Emotion} ({f.EmotionScore:0.00})");
                    Console.WriteLine($"\t\t\t   Gender: {f.Gender}");
                    Console.WriteLine($"\t\t\t  Glasses: {f.HasGlasses} ({f.GlassesType})");
                    Console.WriteLine($"\t\t\t    Beard: {f.HasBeard} ({f.BeardScore:0.00})");
                    Console.WriteLine($"\t\t\tSideburns: {f.HasSideburns} ({f.SideburnsScore:0.00})");
                    Console.WriteLine($"\t\t\tMoustache: {f.HasMoustache} ({f.MoustacheScore:0.00})");
                    Console.WriteLine($"\t\t\t     Pose: {f.HeadPose.Roll} {f.HeadPose.Pitch} {f.HeadPose.Yaw}");
                    Console.WriteLine($"\t\t\t    Smile: {f.SmileScore:0.00}");
                    Console.WriteLine();

                    // FaceIds are valid for next 24hours, unless we store them in a FaceList
                    faceDetailsList.Add(f);
                }

                // Draw bounding boxes over image and save it in results)
                using (var img = Image.Load(picPath))
                {
                    img.Mutate(ctx => ctx.ApplyFaceInfo(faceDetailsList, Rgba32.Red, Rgba32.White));
                    img.Save($"{resultsPath}\\{Path.GetFileNameWithoutExtension(picPath)}_overlay{Path.GetExtension(picPath)}");
                }

                faceDetailsList.Clear();

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Analysis completed. Press any key to terminate...");
            Console.ReadLine();
        }

        private static IList<string> GetPictures()
        {
            var list = new List<string>();

            // Retrieve pictures from the following configured paths
            var folders = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            };

            foreach (var folder in folders)
            {
                list.AddRange(Directory.GetFiles(folder, "*.jpg"));
            }

            return list;
        }

        private static async Task<FaceAnalysisResult> PerformFaceAnalysisIfNotAlreadyDoneAsync(string partialFileName, string path)
        {
            string fileExt = path.Substring(path.Length - 3);
            string destinationFile = $"{partialFileName}.{fileExt}";

            Console.WriteLine($"\t\t{path}");
            destinationFile = $"{partialFileName}_faces_results.txt";
            if (!File.Exists(destinationFile))
            {
                var faceAnalysisResult = await m_ContentAnalyzer.AnalyzeFacesFromLocalFileAsync(path);

                // Store results for later user
                await FileSerializer.StoreInFileAsync(faceAnalysisResult, destinationFile);

                return faceAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<FaceAnalysisResult>(destinationFile);
            }
        }

        private static async Task<ImageAnalysisResult> PerformImageAnalysisIfNotAlreadyDoneAsync(string partialFileName, string path)
        {
            string fileExt = path.Substring(path.Length - 3);
            string destinationFile = $"{partialFileName}.{fileExt}";

            Console.WriteLine($"\t\t{path}");
            destinationFile = $"{partialFileName}_image_results.txt";
            if (!File.Exists(destinationFile))
            {
                var imageAnalysisResult = await m_ContentAnalyzer.AnalyzeImageFromLocalFileAsync(path);

                // Store results for later user
                await FileSerializer.StoreInFileAsync(imageAnalysisResult, destinationFile);

                return imageAnalysisResult;
            }
            else
            {
                return await FileSerializer.LoadFromFileAsync<ImageAnalysisResult>(destinationFile);
            }
        }

        private static void SetupServices(IConfigurationRoot configuration)
        {
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
            Console.WriteLine("TTG - December 2017 - Microsoft Cognitive Services demo - Picture Analyzer");
            Console.WriteLine("Copyright (C) 2017 Gianni Rosa Gallina. Released under MIT license.");
            Console.WriteLine("See LICENSE file for details.");
            Console.WriteLine("==========================================================================");
        }
    }

    static class Extensions
    {
        public static IImageProcessingContext<TPixel> ApplyFaceInfo<TPixel>(this IImageProcessingContext<TPixel> processingContext, IList<FaceDetails> faces, TPixel color, TPixel textColor)
           where TPixel : struct, IPixel<TPixel>
        {
            return processingContext.Apply(img =>
            {
                var gfxOptions = new GraphicsOptions(true) { };
                var gfxTextOptions = new TextGraphicsOptions(true) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                var font = SystemFonts.CreateFont("Arial", 39, FontStyle.Bold);
                foreach (var face in faces)
                {
                    PointF left_top = new PointF((float)face.BoundingBox.X, (float)face.BoundingBox.Y);
                    PointF left_bottom = new PointF((float)face.BoundingBox.X, (float)face.BoundingBox.Y + (float)face.BoundingBox.Height);
                    PointF right_top = new PointF((float)face.BoundingBox.X + (float)face.BoundingBox.Width, (float)face.BoundingBox.Y);
                    PointF right_bottom = new PointF((float)face.BoundingBox.X + (float)face.BoundingBox.Width, (float)face.BoundingBox.Y + (float)face.BoundingBox.Height);
                    PointF[] boundingBox = new PointF[] { left_top, right_top, right_bottom, left_bottom };

                    img.Mutate(i => i.DrawPolygon(color, 5f, boundingBox, gfxOptions));

                    PointF text_Age_pos = new PointF((float)face.BoundingBox.X + (float)face.BoundingBox.Width / 2, (float)face.BoundingBox.Y - 45f);
                    SizeF size = TextMeasurer.Measure($"{face.Age}", new RendererOptions(font) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Origin = text_Age_pos });
                    PointF[] text_Background_Age = new PointF[] { new PointF(text_Age_pos.X - size.Width / 2 - 20f, text_Age_pos.Y - 20f), new PointF(text_Age_pos.X + size.Width / 2 + 20f, text_Age_pos.Y - 20f), new PointF(text_Age_pos.X + size.Width / 2 + 20f, text_Age_pos.Y + size.Height - 10f), new PointF(text_Age_pos.X - size.Width / 2 - 20f, text_Age_pos.Y + size.Height - 10f) };
                    img.Mutate(i => i.FillPolygon(color, text_Background_Age, gfxOptions));
                    img.Mutate(i => i.DrawText($"{face.Age}", font, textColor, text_Age_pos, gfxTextOptions));

                    PointF text_Emotion_pos = new PointF((float)face.BoundingBox.X + (float)face.BoundingBox.Width / 2, (float)face.BoundingBox.Y + (float)face.BoundingBox.Height + 45f);
                    size = TextMeasurer.Measure($"{face.Emotion}", new RendererOptions(font));
                    PointF[] text_Background_Emotion = new PointF[] { new PointF(text_Emotion_pos.X - size.Width / 2 - 20f, text_Emotion_pos.Y - 20f), new PointF(text_Emotion_pos.X + size.Width / 2 + 20f, text_Emotion_pos.Y - 20f), new PointF(text_Emotion_pos.X + size.Width / 2 + 20f, text_Emotion_pos.Y + size.Height - 10f), new PointF(text_Emotion_pos.X - size.Width / 2 - 20f, text_Emotion_pos.Y + size.Height - 10f) };
                    img.Mutate(i => i.FillPolygon(color, text_Background_Emotion, gfxOptions));
                    img.Mutate(i => i.DrawText($"{face.Emotion}", font, textColor, text_Emotion_pos, gfxTextOptions));
                }
            });
        }
    }
}
