#if UNITY_EDITOR
using BranchMaker.Story;
using UnityEditor;
using UnityEngine;
namespace BranchMaker
{
    [CustomEditor(typeof(StoryManager))]
    public class StoryManagerEditor : Editor
    {
        StoryManager targetPlayer;// = (StoryManager)target;
    
        void OnEnable()
        {
           // lookAtPoint = serializedObject.FindProperty("lookAtPoint");
        }

        public override void OnInspectorGUI()
        {
            //EditorGUILayout.LabelField ("Some help", "Some other text");
           // serializedObject.Update();
            //EditorGUILayout.PropertyField(lookAtPoint);
            //serializedObject.ApplyModifiedProperties();
            
            DrawDefaultInspector();
        }
    }
}

#endif