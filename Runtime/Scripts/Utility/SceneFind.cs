using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BranchMaker.Utility
{
    public static class SceneFind
    {
        public static List<T> All<T>() where T : class =>
            Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<T>()
                .ToList();

        public static T First<T>() where T : class =>
            Object
                .FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .OfType<T>()
                .FirstOrDefault();
    }
}