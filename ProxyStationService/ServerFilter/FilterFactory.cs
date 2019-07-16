namespace ProxyStation.ServerFilter
{
    public static class FilterFactory
    {
        public static BaseFilter GetFilter(string name)
        {
            switch (name)
            {
                case "name": return new NameFilter();
                default: return null;
            }
        }
    }
}