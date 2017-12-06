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

    public class FaceAnalysisResult
    {
        public IList<FaceDetails> Faces { get; set; }

        public FaceAnalysisResult()
        {
            Faces = new List<FaceDetails>();
        }
    }

    public class FaceDetails
    {
        public string FaceId { get; set; }
        public double Age { get; set; }
        public double EmotionScore { get; set; }
        public EmotionValue Emotion { get; set; }
        public bool HasMoustache { get; set; }
        public bool HasBeard { get; set; }
        public bool HasSideburns { get; set; }
        public Gender Gender { get; set; }
        public bool HasGlasses { get; set; }
        public GlassesType GlassesType { get; set; }
        public HeadPose HeadPose { get; set; }
        public double SmileScore { get; set; }
        public double BeardScore { get; set; }
        public double SideburnsScore { get; set; }
        public double MoustacheScore { get; set; }
        public Rectangle BoundingBox { get; set; }
    }

    public class HeadPose
    {
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
    }

    public class Rectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
