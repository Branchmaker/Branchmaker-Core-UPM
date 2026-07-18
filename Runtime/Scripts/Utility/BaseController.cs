using UnityEngine;

namespace BranchMaker.Utility
{
    public abstract class BaseController<T> : MonoBehaviour
        where T : BaseController<T>
    {
        private static T _instance;
        [SerializeField] protected bool debugLog;
        [SerializeField] protected bool persist;
        private bool _prepared;
    
        public static T Instance
        {
            get
            {
                if (_instance)
                    return _instance;

#if UNITY_2023_1_OR_NEWER
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
                _instance = FindObjectOfType<T>();
#endif

                if (_instance) _instance.EnsurePrepared();

                return _instance;
            }
        }

        protected virtual void Prepare()
        {
        }

        private void EnsurePrepared()
        {
            if (_prepared) return;
            _prepared = true;
            Prepare();
        }


        protected virtual void Awake()
        {
            if (persist && _instance)
            {
                Destroy(this);
                return;
            }

            if (_instance) return;
            _instance = this as T;
            EnsurePrepared();
        }

        private void OnEnable()
        {
            _instance = this as T;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void Log(string log)
        {
            #if UNITY_EDITOR
            if (debugLog) Debug.Log("<color=#00FFFF><b>"+typeof(T)+"</b></color>: "+log);
            #endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void LogError(string log)
        {
            #if UNITY_EDITOR
            if (debugLog) Debug.LogError("<color=#00FFFF><b>"+typeof(T)+"</b></color>: "+log);
            #endif
        }
    }
}
