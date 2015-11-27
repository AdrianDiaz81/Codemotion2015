using Hel10.Oxford.Speak;
using Hel10.Oxford.SpeehRecognition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Audio;
using Windows.Media.Streaming.Adaptive;
using Windows.Media.Core;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.Storage.Streams;
using Windows.ApplicationModel.VoiceCommands;
using Hel10.App.View;
using Hel10.App.ViewModel;
using Hel10.Azure.Search;
using Helo10.Oxford.Vision;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hel10.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool Completed { get; set; }
        private SpeechRecognizer speechRecognizer { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
  
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PresentRodolfo();
            DisplayLoading();
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private async void PresentRodolfo()
        {
            var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
            var token = auth.GetAccessToken();
            var requestUri = "https://speech.platform.bing.com/synthesize";

            var sb = new StringBuilder();
            sb.AppendFormat("Hola, soy Rodolfo, la mascota de ENCAMINA.");
            sb.AppendFormat("¿Qué tipo de imagen quieres analizar?");
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
            cortana = null;
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

        private async void Current_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                Windows.Media.Playback.BackgroundMediaPlayer.Current.MediaEnded -= Current_MediaEnded;
                BitmapImage img = new BitmapImage(new Uri("ms-appx:///Assets/Voice/voice-on.png"));
                this.Completed = false;
                iconVoiceStatus.Source = img;
                InitializeSpeechRecognizer();
            });
        }

        private async void InitializeSpeechRecognizer()
        {
            if (speechRecognizer != null)
            {
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
            speechRecognizer = new SpeechRecognizer();
            var topicConstraing = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "Development");
            speechRecognizer.Constraints.Add(topicConstraing);
            await speechRecognizer.CompileConstraintsAsync();

            var operation = await speechRecognizer.RecognizeAsync();
            if (!this.Completed && operation.Status == SpeechRecognitionResultStatus.Success)
            {
                this.Completed = true;
                ResultGenerated(operation.Text);
                speechRecognizer.RecognizeAsync().Cancel();
                speechRecognizer.Dispose();
                speechRecognizer = null;
            }
        }

        private void OnRecognitionCompleteHandler(IAsyncOperation<SpeechRecognitionResult> asyncInfo, AsyncStatus asyncStatus)
        {
            asyncInfo.Close();
        }

        private async void DisplayLoading()
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                LoadingIndicator.IsActive = true;
            });
        }

        private async void ResultGenerated(string mensaje)
        {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var cleanText = mensaje.Replace(".", string.Empty);
                var vm = new ImageSearchViewModel();
                vm.Search(cleanText);
                if (vm.SearchResult.Count > 0)
                {
                    Frame.Navigate(typeof(SearchListView), vm);
                }
                else
                {
                    var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
                    var token = auth.GetAccessToken();
                    var requestUri = "https://speech.platform.bing.com/synthesize";

                    var sb = new StringBuilder();
                    sb.AppendFormat("No se han encontrado resultados de búsqueda.");

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
                    cortana.Speak(CancellationToken.None);
                    cortana = null;
                }

            });
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (!this.Completed)
            {

                try
                {
                    //sender.StopAsync();
                    //this.Completed = true;
                    sender.CancelAsync();
                }
                catch (Exception)
                {

                }
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {

                        var mensaje = args.Result.Text.Replace(".", string.Empty);
                        if (mensaje.Contains("Text")) mensaje = "Texto";
                        var vm = new ImageSearchViewModel();
                    // mensaje = "Texto";
                    vm.Search(mensaje);
                        if (vm.SearchResult.Count > 0)
                        {
                            Frame.Navigate(typeof (SearchListView), vm);
                        }
                        else
                        {
                            var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
                            var token = auth.GetAccessToken();
                            var requestUri = "https://speech.platform.bing.com/synthesize";

                            var sb = new StringBuilder();
                            sb.AppendFormat("No se han encontrado resultados de búsqueda.");
                            
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
                             cortana.Speak(CancellationToken.None);
                            cortana = null;
                        }
                    });
            }
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
        }

        private async void VoiceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var speechRecognizer = new SpeechRecognizer();
            var topicConstraing = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "Development");
            speechRecognizer.Constraints.Add(topicConstraing);
            await speechRecognizer.CompileConstraintsAsync();

            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;

            speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }
    }
}
