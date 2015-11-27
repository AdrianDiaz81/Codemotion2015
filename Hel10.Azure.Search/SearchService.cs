
namespace Hel10.Azure.Search
{
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;    
    public class SearchService
    {
        private string searchServiceName; //demohelo10
        private string apiKey; //CDB98C274D37BEFF46E052983EA38BB0;
        private SearchServiceClient serviceClient;
        private SearchIndexClient indexClient;

        public SearchService(string searchServiceName, string apiKey, string index)
        {

            this.searchServiceName = searchServiceName;
            this.apiKey = apiKey;
            serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            indexClient = serviceClient.Indexes.GetClient(index);
        }

        public ICollection<Base> Search(string query)
        {

            var resultSearch = new Collection<Base>();
            var searchParameters = new SearchParameters();

            DocumentSearchResponse<Base> response = indexClient.Documents.Search<Base>(query, searchParameters);

            foreach (var result in response)
            {
                resultSearch.Add(result.Document);
            }

            return resultSearch;
        }
    }
}
