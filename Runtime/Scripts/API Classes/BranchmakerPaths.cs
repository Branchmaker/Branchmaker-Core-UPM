using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Api
{
    public class BranchmakerPaths
    {
        const string serverRoot = "https://branchmaker.com/";
        public static string jsonorderurl = serverRoot + "api/{0}/feed/story.json";
        public static string jsonupdateurl = serverRoot + "suggestions/updateblock/";
        public static string suggesturl = serverRoot + "suggestions/playersuggest/";
        
        public static string StoryNodes(bool staticPath, string storyId)
        {
            return serverRoot+"api/"+storyId+"/feed/story.json";
            //return serverRoot + (staticPath ? "static/" : "") +"client/storynodes/";
        }
    }
}
