using System;
using System.Collections.Generic;
using System.Linq;

namespace BranchMaker.Actors
{
    public static class ActorDatabase
    {
        [NonSerialized] private static readonly List<ActorObject> ActorPool = new();

        static ActorDatabase()
        {
            ActorPool.Clear();
        }

        public static ActorObject ActorByKey(string q) =>
            ActorPool.FirstOrDefault(a => string.Equals(a.name, q, StringComparison.CurrentCultureIgnoreCase));

        public static void PreloadActor(ActorObject newActor)
        {
            if (!newActor) return;
            if (!ActorPool.Contains(newActor))
            {
                ActorPool.Add(newActor);
            }
        }
    }
}
