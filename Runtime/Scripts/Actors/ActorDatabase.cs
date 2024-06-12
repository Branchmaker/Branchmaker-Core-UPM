using System;
using System.Collections.Generic;
using System.Linq;

namespace BranchMaker.Actors
{
    public static class ActorDatabase
    {
        static List<ActorObject> Actorpool = new();

        static ActorDatabase()
        {
            Actorpool.Clear();
        }

        public static ActorObject ActorByKey(string q) =>
            Actorpool.FirstOrDefault(a => string.Equals(a.name, q, StringComparison.CurrentCultureIgnoreCase));

        public static void PreloadActor(ActorObject newActor)
        {
            if (!Actorpool.Contains(newActor))
            {
                Actorpool.Add(newActor);
            }
        }
    }
}
