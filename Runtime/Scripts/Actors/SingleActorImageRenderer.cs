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
        
        private void Awake()
        {
            _actorImage = GetComponent<Image>();
            preloadActors.ForEach(ActorDatabase.PreloadActor);
            _actorImage.enabled = false;
        }

        private void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
        }

        private void ProcessBlock(BranchNodeBlock CurrentBlock)
        {
            if (!string.IsNullOrEmpty(CurrentBlock.character))
            {
                _currentlyShowingActor = ActorDatabase.ActorByKey(CurrentBlock.character);
                if (!_currentlyShowingActor) return;
                if (!string.IsNullOrEmpty(CurrentBlock.emotion)) _currentlyShowingActor.CurrentEmotion = CurrentBlock.emotion;
                
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
