using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Api
{
    public class BranchmakerPaths
    {
        const string serverRoot = "https://branchmaker.com/";
        static public string jsonurl = serverRoot + "client/storynodes/";
        public static string jsonorderurl = serverRoot + "suggestions/forceblock/";
        public static string jsonupdateurl = serverRoot + "suggestions/updateblock/";
        public static string suggesturl = serverRoot + "suggestions/playersuggest/";
        
        public static string StoryNodes(bool staticPath, string storyId)
        {
            return serverRoot+"/api/"+storyId+"/feed/story.json";
            //return serverRoot + (staticPath ? "static/" : "") +"client/storynodes/";
        }
    }
}