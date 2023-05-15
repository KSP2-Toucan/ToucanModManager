using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ToucanServices.Services.Data
{
    public enum AvailableApis
    {
        Toucan,
        Spacedock
    };

    /* Browser */

    public enum BrowseCategories
    {
        DEFAULT,
        TOP,
        FEATURED,
        NEW
    }

    public enum SortBy
    {
        NAME,
        UPDATED,
        CREATED
    }

    public enum SortingDirection
    {
        ASC,
        DESC
    }

    public static class ServicesHandler
    {
        public static HttpClient GetHttpClient()
        {
            HttpClient ToucanHttpClient = new HttpClient();
            ToucanHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:93.0; mm:Toucan-Mod-Manager;) Gecko/20100101 Firefox/93.0");

            return ToucanHttpClient;
        }
    }
}
