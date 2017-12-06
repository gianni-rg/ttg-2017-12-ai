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
    using System.Collections.Generic;

    public class ImageAnalysisResult
    {
        public IList<ImageAnalysisCategoryResult> Categories { get; set; }
        public IList<ImageAnalysisFaceResult> Faces { get; set; }
        public IList<ImageAnalysisTagResult> Tags { get; set; }
        public IList<ImageAnalysisDescriptionResult> Descriptions { get; set; }
        public IList<ImageAnalysisTextResult> Text { get; set; }
        public ImageAnalysisColorResult Colors { get; set; }
        public ImageAnalysisAdultContentResult AdultContent { get; set; }

        public ImageAnalysisResult()
        {
            Categories = new List<ImageAnalysisCategoryResult>();
            Faces = new List<ImageAnalysisFaceResult>();
            Tags = new List<ImageAnalysisTagResult>();
            Descriptions = new List<ImageAnalysisDescriptionResult>();
            Text = new List<ImageAnalysisTextResult>();
            Colors = new ImageAnalysisColorResult();
            AdultContent = new ImageAnalysisAdultContentResult();
        }
    }

    public class ImageAnalysisFaceResult
    {
        public Gender Gender { get; set; }
        public double Age { get; set; }
    }

    public class ImageAnalysisDescriptionResult
    {
        public string Text { get; set; }
        public double Score { get; set; }
    }

    public class ImageAnalysisCategoryResult
    {
        public string Text { get; set; }
        public double Score { get; set; }
    }

    public class ImageAnalysisColorResult
    {
        public string AccentColor { get; set; }
        public string DominantColorForeground { get; set; }
        public string DominantColorBackground { get; set; }
        public bool IsBWImg { get; set; }
    }

    public class ImageAnalysisAdultContentResult
    {
        public bool IsAdultContent { get; set; }
        public bool IsRacyContent { get; set; }
        public double AdultScore { get; set; }
        public double RacyScore { get; set; }
    }

    public class ImageAnalysisTagResult
    {
        public string Text { get; set; }
        public double Score { get; set; }
        public string Hint { get; set; }
    }

    public class ImageAnalysisTextResult
    {
        public string Language { get; set; }
        public string Orientation { get; set; }
        public double TextAngle { get; set; }
        public string Text { get; set; }
    }
}
