using System;
using UnityEngine;

namespace BranchMaker.WebServices
{
    [CreateAssetMenu]
    public class BranchmakerCacheObject : ScriptableObject
    {
        public DateTime LastUpdateDate;
        
        public int NodeCount;
        
        public string cacheUrl;
        [TextArea(2,30)]
        public string cacheData;

    }
}