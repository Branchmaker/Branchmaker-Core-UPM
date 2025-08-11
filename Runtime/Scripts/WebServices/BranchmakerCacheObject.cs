using UnityEngine;

namespace BranchMaker.WebServices
{
    [CreateAssetMenu]
    public class BranchmakerCacheObject : ScriptableObject
    {
        public string cacheUrl;
        [TextArea(2,30)]
        public string cacheData;

    }
}