using System;
using UnityEngine;

namespace BranchMaker.Runtime.Utility
{
    public abstract class BaseController<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        [SerializeField] protected bool debugLog;
        [SerializeField] protected bool persist;
    
        public static T Instance => _instance != null ? _instance : null;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (persist) DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            _instance = this as T;
        }


        protected void Log(string log)
        {
            #if UNITY_EDITOR
            if (debugLog) Debug.Log("<color=#00FFFF><b>"+typeof(T)+"</b></color>: "+log);
            #endif
        }

        protected void LogError(string log)
        {
            #if UNITY_EDITOR
            if (debugLog) Debug.LogError("<color=#00FFFF><b>"+typeof(T)+"</b></color>: "+log);
            #endif
        }
    }
}
