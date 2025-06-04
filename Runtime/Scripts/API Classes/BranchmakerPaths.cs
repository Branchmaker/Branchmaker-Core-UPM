namespace BranchMaker.Api
{
    public static class BranchmakerPaths
    {
        const string serverRoot = "https://api.ranchmaker.com/";
        public static string jsonorderurl = serverRoot + "api/{0}/feed/story.json";
        public static string jsonupdateurl = serverRoot + "suggestions/updateblock/";
        public static string suggesturl = serverRoot + "suggestions/playersuggest/";

        public enum PluginApi
        {
            Suggestions,
        }
        
        public static string StoryNodes(bool staticPath, string storyId)
        {
            return serverRoot+"api/"+storyId+"/feed/story.json";
        }
        
        
        public static string PluginApiRoute(PluginApi type)
        {
            return type switch
            {
                PluginApi.Suggestions => serverRoot + "api/steam/suggestion",
                _ => null
            };
        }
    }
}
