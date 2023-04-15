using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BranchMaker.Actors
{
    [CreateAssetMenu]
    [SerializeField]
    public class ActorObject : ScriptableObject
    {
        public string displayName;
        public Sprite mainSprite;
        public Color themeColor;
        public List<CharacterExpression> expressions;
        [NonSerialized] public string CurrentEmotion;
        
        public Sprite PortraitSprite()
        {
            if (expressions.Count == 0) return mainSprite;
            if (string.IsNullOrEmpty(CurrentEmotion)) return expressions.First().characterImage;
            foreach (var expression in expressions) {
                if (expression.expression == CurrentEmotion) return expression.characterImage;
            }
            //Debug.LogError("Could not find sprite for expression = "+expressions);
            return mainSprite;
        }
    }

    [Serializable]
    public class CharacterExpression
    {
        public string expression;
        public Sprite characterImage;

    }

}