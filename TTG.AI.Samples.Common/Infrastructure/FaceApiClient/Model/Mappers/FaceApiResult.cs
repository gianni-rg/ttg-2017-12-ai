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

namespace TTG.AI.Samples.Common.Infrastructure.FaceApiClient.Model.Mappers
{
    using System;
    using System.Linq;
    using TTG.AI.Samples.Common.Model;

    public static partial class Mappers
    {
        private const double FacialHairThreshold = 0.15;

        public static FaceAnalysisResult MapToDomain(this Microsoft.ProjectOxford.Face.Contract.Face[] faces)
        {
            var domainEntity = new FaceAnalysisResult()
            {
                Faces = faces.Select(MapToDomain).ToList(),
            };

            return domainEntity;
        }

        private static FaceDetails MapToDomain(this Microsoft.ProjectOxford.Face.Contract.Face face)
        {
            var emotion = GetEmotionValueFromScores(face.FaceAttributes.Emotion);

            var domainEntity = new FaceDetails()
            {
                FaceId = face.FaceId.ToString(),
                Age = face.FaceAttributes.Age,
                Emotion = emotion.emotionValue,
                EmotionScore = emotion.emotionScore,
                HasBeard = face.FaceAttributes.FacialHair.Beard > FacialHairThreshold,
                BeardScore = face.FaceAttributes.FacialHair.Beard,
                HasMoustache = face.FaceAttributes.FacialHair.Moustache > FacialHairThreshold,
                MoustacheScore = face.FaceAttributes.FacialHair.Moustache,
                HasSideburns = face.FaceAttributes.FacialHair.Sideburns > FacialHairThreshold,
                SideburnsScore = face.FaceAttributes.FacialHair.Sideburns,
                Gender = face.FaceAttributes.Gender == "male" ? Gender.Male : Gender.Female,
                HasGlasses = face.FaceAttributes.Glasses != Microsoft.ProjectOxford.Face.Contract.Glasses.NoGlasses,
                GlassesType = (GlassesType)face.FaceAttributes.Glasses,
                HeadPose = new HeadPose() { Pitch = face.FaceAttributes.HeadPose.Pitch, Roll = face.FaceAttributes.HeadPose.Roll, Yaw = face.FaceAttributes.HeadPose.Yaw },
                SmileScore = face.FaceAttributes.Smile,
                BoundingBox = new Rectangle { X = face.FaceRectangle.Left, Y = face.FaceRectangle.Top, Width = face.FaceRectangle.Width, Height = face.FaceRectangle.Height },
            };

            return domainEntity;
        }

        private static (EmotionValue emotionValue, double emotionScore) GetEmotionValueFromScores(Microsoft.ProjectOxford.Common.Contract.EmotionScores scores)
        {
            var highestEmotionScore = scores.ToRankedList().First();
            return (emotionValue: (EmotionValue)Enum.Parse(typeof(EmotionValue), highestEmotionScore.Key), emotionScore: highestEmotionScore.Value);
        }
    }
}
