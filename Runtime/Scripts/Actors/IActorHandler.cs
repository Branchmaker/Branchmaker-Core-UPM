using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.Actors
{
    public interface IActorHandler
    {
        public void ResetActors();
        
        public void ActorUpdate(string actorKey, BranchNodeBlock updateBlock);

        public void HideActors(BranchNodeBlock updateBlock);

    }
}
