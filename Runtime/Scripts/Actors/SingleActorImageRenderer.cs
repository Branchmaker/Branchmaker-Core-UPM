using System.Collections.Generic;
using BranchMaker.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Actors
{
    [RequireComponent(typeof(Image))]
    public class SingleActorImageRenderer : MonoBehaviour, IActorHandler
    {
        public List<ActorObject> preloadActors = new();
        private Image _actorImage;
        private ActorObject _currentlyShowingActor;
        private bool _initialized;
        
        private void Awake()
        {
            Prepare();
        }
        
        public void Prepare()
        {
            if (_initialized) return;
            _initialized = true;
            _actorImage = GetComponent<Image>();
            preloadActors.ForEach(ActorDatabase.PreloadActor);
            _actorImage.enabled = false;
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
        }

        private void ProcessBlock(BranchNodeBlock currentBlock)
        {
            ShowActor(currentBlock.character, currentBlock.emotion);
        }

        public void ShowActor(string character, string emotion)
        {
            if (!string.IsNullOrEmpty(character))
            {
                _currentlyShowingActor = ActorDatabase.ActorByKey(character);
                if (!_currentlyShowingActor) return;
                if (!string.IsNullOrEmpty(emotion)) _currentlyShowingActor.CurrentEmotion = emotion;
                
                _actorImage.sprite = _currentlyShowingActor.PortraitSprite();
                _actorImage.enabled = (_actorImage.sprite != null);
                //dialogue = "<color=#" + ColorUtility.ToHtmlStringRGB(actor.themeColor) + ">" + actor.displayName + "</color>\n" + dialogue;
            }
        }

        public void ResetActors()
        {
            _actorImage.enabled = false;
        }

        public void ActorUpdate(string actorKey, BranchNodeBlock updateBlock)
        {
            _currentlyShowingActor = ActorDatabase.ActorByKey(actorKey);
            _actorImage.sprite = _currentlyShowingActor.PortraitSprite();
            _actorImage.enabled = (_actorImage.sprite != null);
        }

        public void HideActors(BranchNodeBlock updateBlock)
        {
            throw new System.NotImplementedException();
        }
    }
}
