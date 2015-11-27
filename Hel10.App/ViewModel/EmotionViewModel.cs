using Hel10.App.Model;
using Hel10.App.ViewModel.Base;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hel10.App.ViewModel
{
    public class EmotionViewModel: BaseViewModel
    {
        public Azure.Search.Base ImageResult { get; set; }
        public Emotion[] EmotionCollection { get; set; }
        public ICollection<EmotionView> ImageCollection { get; set; }

        public EmotionViewModel(Azure.Search.Base image)
        {
            this.ImageResult = image;
            this.ImageCollection = new List<EmotionView>();
        }

        public async Task<Emotion[]> GetEmotion()
        {
            try {
                var emotionServiceClient = new EmotionServiceClient("27f1806f6cdf4720bff4f68654bbd241");

                var emotion = await emotionServiceClient.RecognizeAsync(this.ImageResult.Url);
                this.EmotionCollection = emotion;
                var image = new EmotionView
                {
                    Anger = emotion[0].Scores.Anger * 100,
                    Nombre = ImageResult.Nombre,
                    Url = ImageResult.Url,
                    Disgust = emotion[0].Scores.Disgust * 100,
                    Contempt = emotion[0].Scores.Contempt * 100,
                    Fear = emotion[0].Scores.Fear * 100,
                    Happiness = emotion[0].Scores.Happiness * 100,
                    Neutral = emotion[0].Scores.Neutral * 100,
                    Sadness = emotion[0].Scores.Sadness * 100,
                    Surprise = emotion[0].Scores.Surprise * 100
                };


                ImageCollection.Add(image);
                return emotion;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
