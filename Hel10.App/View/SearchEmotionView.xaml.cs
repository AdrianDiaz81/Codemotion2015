using Hel10.App.ViewModel;
using Hel10.Oxford.Speak;
using Hel10.Oxford.SpeehRecognition;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hel10.App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchEmotionView : Page
    {
        public SearchEmotionView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            var vm = e.Parameter as EmotionViewModel;
            GridImages.ItemsSource = vm.ImageCollection;
            var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
            var token = auth.GetAccessToken();
            var requestUri = "https://speech.platform.bing.com/synthesize";

            var sb = new StringBuilder();
            sb.Append(vm.ImageCollection.ToList().FirstOrDefault().Nombre);
            if (vm.ImageCollection.ToList().FirstOrDefault().Happiness > 70)
            {
                sb.AppendFormat("Esta feliz  ", vm.ImageCollection.ToList().FirstOrDefault().Happiness);
            }
            if (vm.ImageCollection.ToList().FirstOrDefault().Disgust > 70)
            {

                sb.AppendFormat("Esta enfandado ", vm.ImageCollection.ToList().FirstOrDefault().Disgust);
            }
            if (vm.ImageCollection.ToList().FirstOrDefault().Contempt > 70)
            {

                sb.AppendFormat("Esta contento ", vm.ImageCollection.ToList().FirstOrDefault().Contempt);
            }
            if (vm.ImageCollection.ToList().FirstOrDefault().Sadness > 70)
            {

                sb.AppendFormat("Esta triste ", vm.ImageCollection.ToList().FirstOrDefault().Sadness);
            }
            if (vm.ImageCollection.ToList().FirstOrDefault().Neutral > 60)
            {

                sb.AppendFormat("Esta neutro ", vm.ImageCollection.ToList().FirstOrDefault().Sadness);
            }
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
            cortana.Speak(CancellationToken.None).Wait();
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.Frame.CanGoBack) this.Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested -= CurrentView_BackRequested;
        }

        static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
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
            });
        }
    }
}
