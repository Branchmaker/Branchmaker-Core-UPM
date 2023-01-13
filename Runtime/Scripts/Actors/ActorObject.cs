using System;
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
        public List<CharacterExpression> expressions;
        public string current_emotion;
    }

    [Serializable]
    public class CharacterExpression
    {
        public string expression;
        public Sprite characterImage;

    }

}