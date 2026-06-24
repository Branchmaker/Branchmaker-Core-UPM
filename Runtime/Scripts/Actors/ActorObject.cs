using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BranchMaker.Actors
{
    [CreateAssetMenu]
    public class ActorObject : ScriptableObject
    {
        public string displayName;
        public string rootNode;
        public Sprite mainSprite;
        public Color themeColor;
        public List<CharacterExpression> expressions;
        public List<CharacterExpressionSet> expressionLayers = new();

        [NonSerialized] public string CurrentEmotion;

        public Sprite PortraitSprite()
        {
            if (expressions == null || expressions.Count == 0) return mainSprite;

            if (string.IsNullOrEmpty(CurrentEmotion)) return expressions.First().characterImage;
            foreach (var expression in expressions.Where(expression => expression.expression == CurrentEmotion))
                return expression.characterImage;

            return mainSprite;
        }

        public void ResetExpressions()
        {
            CurrentEmotion = string.Empty;
        }

        public Dictionary<string, Sprite> ParseExpressionString(string input)
        {
            var requested = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(input))
                foreach (var part in input.Split('+'))
                {
                    var pieces = part.Split(':');

                    if (pieces.Length == 2) requested[pieces[0].Trim()] = pieces[1].Trim();
                }

            var result = new Dictionary<string, Sprite>();

            foreach (var layer in expressionLayers)
            {
                Sprite sprite = null;

                if (requested.TryGetValue(layer.prefix, out var requestedKey))
                {
                    var match = layer.expressions?
                        .FirstOrDefault(x => x.expression == requestedKey);

                    if (match != null) sprite = match.characterImage;
                }

                result[layer.prefix] = sprite;
            }

            return result;
        }

        [Serializable]
        public class CharacterExpression
        {
            public string expression;
            public Sprite characterImage;
        }

        [Serializable]
        public class CharacterExpressionSet
        {
            public string prefix;
            public List<CharacterExpression> expressions;
            public bool startsEmpty;
        }
    }
}