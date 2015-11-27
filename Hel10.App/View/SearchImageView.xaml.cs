using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Hel10.App.ViewModel;
using Hel10.Oxford.Speak;
using Hel10.Oxford.SpeehRecognition;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Hel10.App.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchImageView : Page
    {
        public SearchImageView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            var vm = e.Parameter as ImageViewModel;
            GridImages.ItemsSource = vm.ImageCollection;
            var auth = new Authentication("713034f5c7994f089c1d5a70c1a12ede", "54c4cd393679455d90a48250cde0cfa4");
            var token = auth.GetAccessToken();
            var requestUri = "https://speech.platform.bing.com/synthesize";

            var sb= new StringBuilder();
            sb.Append(vm.ImageCollection.ToList().FirstOrDefault().Nombre);
            sb.AppendFormat("Es {0} ", vm.ImageCollection.ToList().FirstOrDefault().Sexo);
            sb.AppendFormat("Tiene {0} años",vm.ImageCollection.ToList().FirstOrDefault().Edad);
            sb.AppendFormat("Tiene un parecido con {0} de un {1} por ciento ",(vm.ImageCollection.ToList().FirstOrDefault().Sexo.Equals("Hombre")?"Chuck Norris":"Angelina Jolie"), vm.ImageCollection.ToList().FirstOrDefault().Similar);
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
            //currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
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
