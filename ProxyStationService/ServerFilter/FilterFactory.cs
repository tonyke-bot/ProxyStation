namespace ProxyStation.ServerFilter
{
    public static class FilterFactory
    {
        public static BaseFilter GetFilter(string name)
        {
            return name switch
            {
                "name" => new NameFilter(),
                "regex" => new RegexFilter(),
                _ => null,
            };
        }
    }
}