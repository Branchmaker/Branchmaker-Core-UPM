using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BranchMaker.WebServices
{
    public static class APIRequest
    { 
        public static async Task<string> FetchFromApi(string url, string cacheKey)
        {
            var content = "";
            StoryManager.Instance.Log("Loading: " + url);
    
            var fetch = UnityWebRequest.Get(url);
            //fetch.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            //fetch.SetRequestHeader("Pragma", "no-cache");
    
            var operation = fetch.SendWebRequest();
    
            while (!operation.isDone)
            {
                await Task.Yield(); // wait for the next frame
            }

            var backupFileName = Application.persistentDataPath + "/" + cacheKey + ".txt";
            if (!string.IsNullOrEmpty(fetch.error))
            {
                if (File.Exists(backupFileName))
                {
                    content = File.ReadAllText(backupFileName);
                }
            }
            else
            {
                File.WriteAllText(backupFileName, fetch.downloadHandler.text);
                content = fetch.downloadHandler.text;
            }
    
            return content;
        }

    }
}
