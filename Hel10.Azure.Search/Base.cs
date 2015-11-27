namespace Hel10.Azure.Search
{
    using Microsoft.Azure.Search.Models;
    [SerializePropertyNamesAsCamelCase]
    public class Base
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
    }
}
