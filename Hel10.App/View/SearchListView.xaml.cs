using Hel10.App.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Hel10.App.Model;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Hel10.Azure.Search;
using Windows.UI.Core;
using Hel10.Oxford.SpeehRecognition;
using System.Text;
using Hel10.Oxford.Speak;
using Windows.Storage.Streams;
using System.Threading;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hel10.App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchListView : Page
    {
        public ImageSearchViewModel vm { get; set; }
        private SpeechRecognizer speechRecognizer { get; set; }
        private SpeechRecognitionResult Operation { get; set; }

        public SearchListView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PresentRodolfo();

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += CurrentView_BackRequested;

            this.vm = e.Parameter as ImageSearchViewModel;

            GridImages.ItemsSource = vm.SearchResult;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested -= CurrentView_BackRequested;
        }

        private async void PresentRodolfo()
        {
            var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
            var token = auth.GetAccessToken();
            var requestUri = "https://speech.platform.bing.com/synthesize";

            var sb = new StringBuilder();
            sb.AppendFormat("Selecciona una imagen para analizar");
            var cortana = new Synthesize(new Synthesize.InputOptions()
            {
                RequestUri = new Uri(requestUri),
                Text = sb.ToString(),
                VoiceType = Gender.Female,
                Locale = "es-es",
                VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + token.access_token,
            });

            cortana.OnAudioAvailable += PlayAudio;
            cortana.OnError += ErrorHandler;
            await cortana.Speak(CancellationToken.None);
        }

        private void ErrorHandler(object sender, GenericEventArgs<Exception> e)
        {
        }

        async void PlayAudio(object sender, GenericEventArgs<Stream> args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                Stream str = args.EventData;
                InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                await str.CopyToAsync(ras.AsStreamForWrite());
                Windows.Media.Playback.BackgroundMediaPlayer.Current.SetStreamSource(ras);
                Windows.Media.Playback.BackgroundMediaPlayer.Current.MediaEnded += Current_MediaEnded;
            });
        }

        private async void Current_MediaEnded(MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                Windows.Media.Playback.BackgroundMediaPlayer.Current.MediaEnded -= Current_MediaEnded;
                BitmapImage img = new BitmapImage(new Uri("ms-appx:///Assets/Voice/voice-on.png"));
                iconVoiceStatus.Source = img;
                InitializeSpeechRecognizer();
            });
        }

        private async void InitializeSpeechRecognizer()
        {
            try
            {
                if (speechRecognizer != null)
                {
                    speechRecognizer.RecognizeAsync().Cancel();
                    speechRecognizer.RecognizeAsync().Close();
                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }
                speechRecognizer = new SpeechRecognizer();
                var topicConstraing = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "Development");
                speechRecognizer.Constraints.Add(topicConstraing);
                await speechRecognizer.CompileConstraintsAsync();

                this.Operation = await speechRecognizer.RecognizeAsync();
                if (Operation.Status == SpeechRecognitionResultStatus.Success)
                {
                    ResultGenerated(Operation.Text);
                    speechRecognizer.RecognizeAsync().Cancel();
                    speechRecognizer.Dispose();
                    speechRecognizer = null;
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ResultGenerated(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var item = vm.SearchResult.Where(r => r.Nombre.ToUpper().Contains(text.ToUpper().Replace(".", ""))).FirstOrDefault();
            if (item != null)
            {
                if (item.Categoria.Equals("Emociones"))
                {
                    var vm = new EmotionViewModel(item);
                    await vm.GetEmotion();
                    Frame.Navigate(typeof(SearchEmotionView), vm);
                }
                else
                {

                    if (!item.Categoria.Equals("Frases"))
                    {
                        var vm = new ImageViewModel(item);
                        await vm.GetAge();
                        Frame.Navigate(typeof(SearchImageView), vm);
                    }
                    else
                    {
                        var vm = new TextViewModel(item);
                        await vm.GetText();
                        Frame.Navigate(typeof(SearchTextView), vm);

                    }
                }
            }
        }

        private async void GridImages_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Base)e.ClickedItem;
            if (item.Categoria.Equals("Emociones"))
            {
                var vm = new EmotionViewModel(item);
                await vm.GetEmotion();
                Frame.Navigate(typeof(SearchEmotionView), vm);
            }
            else
            {

                if (!item.Categoria.Equals("Frases"))
                {
                    var vm = new ImageViewModel(item);
                    await vm.GetAge();
                    Frame.Navigate(typeof(SearchImageView), vm);
                }
                else
                {
                    var vm = new TextViewModel(item);
                    await vm.GetText();
                    Frame.Navigate(typeof(SearchTextView), vm);
                }
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (speechRecognizer != null && speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                //speechRecognizer.RecognizeAsync().Cancel();
                //speechRecognizer.RecognizeAsync().Close();
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }

    }
}
