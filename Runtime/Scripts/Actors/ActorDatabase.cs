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
            Actorpool.FirstOrDefault(a => a.name.ToLower() == q.ToLower());

        public static void PreloadActor(ActorObject newActor)
        {
            if (!Actorpool.Contains(newActor))
            {
                Actorpool.Add(newActor);
            }
        }
    }
}
