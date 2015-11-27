
using System.Collections;
using System.Collections.Generic;
using Hel10.App.Model;
using Hel10.App.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Hel10.Azure.Search;

namespace Hel10.App.ViewModel
{
    public class ImageSearchViewModel : BaseViewModel
    {


        public ICollection<Azure.Search.Base> SearchResult { get; set; }
        public string SpeechText { get; set; } 

        public ImageSearchViewModel()
        {
        }


        public void Search(string speechText)
        {
            this.SpeechText = speechText;
            var searchService = new SearchService("NOMBREdelserviciodebusqueda", "keysearch", "nombredelindice");
            this.SearchResult = searchService.Search(speechText);
        }
    }
}
