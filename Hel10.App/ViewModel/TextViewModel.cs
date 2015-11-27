using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Hel10.App.Model;
using Hel10.App.ViewModel.Base;
using Hel10.Azure.Search;
using Helo10.Oxford.Vision;
using Microsoft.ProjectOxford.Vision;

namespace Hel10.App.View
{
    internal class TextViewModel: BaseViewModel
    {
        private Base item;
        public string TextRecognized;
        public ICollection<ImageView> TextCollection { get; set; }
        public TextViewModel(Base item)
        {
            this.item = item;
            TextCollection= new List<ImageView>();
        }

        public async   Task  GetText()
        {
            VisionServiceClient visionServiceClient= new VisionServiceClient("key");

            var result = await visionServiceClient.RecognizeTextAsync(this.item.Url);
            var sb= new StringBuilder();
            foreach (var word in result.Regions[0].Lines.SelectMany(lines => lines.Words))
            {
                sb.Append(word.Text +" ");
            }
         
            var item = new ImageView {Nombre = sb.ToString(), Url = this.item.Url};
            TextCollection.Add(item);
          

        }
    }
}