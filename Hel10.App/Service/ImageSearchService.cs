using Hel10.App.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hel10.App.ViewModel
{
    public class ImageSearchService
    {
        public  ObservableCollection<ImageSearch> GetSearch()

        {
            ImageSearch item1 = new ImageSearch { Id = "1", Categoria = "Personas", Url = "http://desafiomundial.com/wp-content/uploads/2015/08/cr7.jpg", Nombre = "Adrian" };
            var result = new ObservableCollection<ImageSearch>();
            result.Add(item1);
            return result;
      //      return null ;
        }

    }
}