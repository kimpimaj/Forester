namespace Forester.Domain.Streams.GetCompletedOrders
{
    internal class TreeTracker
    {
        public readonly Dictionary<string, List<string>> SiteToTrees = new Dictionary<string, List<string>>();
        public readonly Dictionary<string, List<string>> TreesToLogs = new Dictionary<string, List<string>>();
        public readonly Dictionary<string, string> LogsToTrees = new Dictionary<string, string>();

        public void Identify(string tree, string site)
        {
            if (!SiteToTrees.ContainsKey(site))
                SiteToTrees.Add(site, new List<string>());
            SiteToTrees[site].Add(tree);
        }

        public void CutToLength(string tree, string log)
        {
            if (!TreesToLogs.ContainsKey(tree))
                TreesToLogs.Add(tree, new List<string>());
            TreesToLogs[tree].Add(log);
            LogsToTrees.Add(log, tree);
        }
    }
}
