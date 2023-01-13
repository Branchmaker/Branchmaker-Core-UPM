using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Actors
{
    public class SingleActorImageRenderer : MonoBehaviour, IActorHandler
    {
        public List<ActorObject> preloadActors = new();
        public Image actorImage;
        private ActorObject _currentlyShowingActor;
        
        private void Awake()
        {
            preloadActors.ForEach(ActorDatabase.PreloadActor);
        }

        public void ResetActors()
        {
            actorImage.enabled = false;
        }

        public void ActorUpdate(string actorKey, BranchNodeBlock updateBlock)
        {
            _currentlyShowingActor = ActorDatabase.ActorByKey(actorKey);
            actorImage.sprite = _currentlyShowingActor.PortraitSprite();
            actorImage.enabled = (actorImage.sprite != null));
        }

        public void HideActors(BranchNodeBlock updateBlock)
        {
            throw new System.NotImplementedException();
        }
    }
}
