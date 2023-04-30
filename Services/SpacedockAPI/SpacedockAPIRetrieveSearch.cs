using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToucanServices.Data;
using ToucanServices.SpacedockAPI.Models;

namespace ToucanServices.SpacedockAPI
{
    /// <summary>
    /// Selects what to search for.
    /// </summary>
    public enum SearchType
    {
        MOD,
        USER
    }

    public static class SpacedockAPIRetrieveSearch
    {
        /// <summary>
        /// Utility function to convert the SearchType input to its string equivalent to be
        /// used in the api url for example
        /// </summary>
        /// <param name="SearchType"></param>
        /// <returns></returns>
        public static string GetStringFromSearchType(SearchType SearchType) => (SearchType == SearchType.MOD ? "mod" : "user");
        
        /// <summary>
        /// Gets the api url for searching based off of two inputs, the search query which 
        /// defines what to search for, and the search type which selects from which category 
        /// to search in.
        /// </summary>
        /// <param name="SearchType"></param>
        /// <param name="SearchQuery"></param>
        /// <returns></returns>
        public static string GetSeachApiUrl(SearchType SearchType, string SearchQuery) 
            => $"https://spacedock.info/api/search/{GetStringFromSearchType(SearchType)}?query={SearchQuery}";
    }
}
