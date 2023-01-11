using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BranchMaker.Actors
{

    [CreateAssetMenu]
    [SerializeField]
    public class ActorObject : ScriptableObject
    {
        public string displayName;
        public Color themeColor;
    }

}