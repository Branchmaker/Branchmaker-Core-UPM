#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace BranchMaker.Editor.Editors
{
    public class BranchmakerEditorHeader
    {
        private const string LogoAssetPath = "Packages/com.branchmaker.core/Runtime/Icons/logo.png";

        private static Texture2D _logo;

        public static void PrepLogo()
        {
            if (!_logo) _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoAssetPath);
        }

        public static void DrawHeader(string header, string subhead, float margin = 0f)
        {
            if (!_logo) _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoAssetPath);
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawLogo(margin);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(subhead);
                    }
                }
            }
            EditorGUILayout.Space(8, false);
        }
        
        private static void DrawLogo(float margin)
        {
            const float size = 40f;
            EditorGUILayout.Space(margin, false);

            if (!_logo)
            {
                // Lightweight fallback if asset wasn't found.
                var r = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
                EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.12f));
                GUI.Label(r, "?", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            GUI.DrawTexture(rect, _logo, ScaleMode.ScaleToFit, true);
        }
    }
}

#endif