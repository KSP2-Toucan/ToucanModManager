namespace ToucanServices.SpacedockAPI
{
    public enum BrowseTypes
    {
        DEFAULT,
        TOP,
        FEATURED,
        NEW
    }

    public enum OrderBy
    {
        NAME,
        UPDATED,
        CREATED
    }

    public enum Order
    {
        ASC,
        DESC
    }

    public static class SpacedockAPIRetrieveBrowser
    {
        public static string GetBrowserApiUrl(BrowseTypes Type, UInt32 Page, OrderBy OrderBy, Order Order, UInt32 Count)
        {
            string Url = $"https://spacedock.info/api/browse";

            if (Type != BrowseTypes.DEFAULT) // Default Value
            {
                if (Type == BrowseTypes.TOP) { Url = Url + "/top"; }
                else if (Type == BrowseTypes.FEATURED) { Url = Url + "/featured"; }
                else if (Type == BrowseTypes.NEW) { Url = Url + "/new"; }
            }

            Url = Page != 0 || Page != 1 ? $"{Url}?page={Page}" : $"{Url}?page=1";
            
            if (OrderBy != OrderBy.CREATED) // Default Value
            {
                if (OrderBy == OrderBy.NAME) { Url = $"{Url}&orderby=name"; }
                else if (OrderBy == OrderBy.UPDATED) { Url = $"{Url}&orderby=updated"; }
            }

            Url = Order == Order.ASC ? Url : $"{Url}&order=desc";

            Url = Count != 0 ? $"{Url}&count={Page}" : $"{Url}&count=30";

            return Url;
        }
    }
}
