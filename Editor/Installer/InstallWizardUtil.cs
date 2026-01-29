#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SockpuppetNoir.BranchMaker.Installer
{
    internal static class InstallWizardUtil
    {
        internal static Type FindTypeInLoadedAssemblies(string typeName)
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Fast path by full name
                    var t = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;

                    // Scan types (robust)
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { types = e.Types; }

                    if (types == null) continue;

                    foreach (var tt in types)
                    {
                        if (tt == null) continue;
                        if (tt.Name == typeName) return tt;
                    }
                }
            }
            catch { }

            return null;
        }

        internal static Component FindFirstComponentInActiveScene(Type componentType)
        {
            var all = Resources.FindObjectsOfTypeAll(componentType);
            var active = SceneManager.GetActiveScene();

            foreach (var obj in all)
            {
                if (obj is Component c && c.gameObject.scene == active)
                    return c;
            }
            return null;
        }

        internal static Canvas FindAnyCanvasInActiveScene()
        {
            var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            var active = SceneManager.GetActiveScene();

            foreach (var c in canvases)
            {
                if (c && c.gameObject.scene == active)
                    return c;
            }
            return null;
        }

        internal static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go, "Create Canvas");

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            return canvas;
        }

        internal static void EnsureEventSystemExists(Scene scene)
        {
            var existing = Resources.FindObjectsOfTypeAll<EventSystem>();
            foreach (var es in existing)
            {
                if (es && es.gameObject.scene == scene)
                    return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        internal static GameObject FindInActiveSceneByName(string name)
        {
            var gos = Resources.FindObjectsOfTypeAll<GameObject>();
            var active = SceneManager.GetActiveScene();

            foreach (var go in gos)
            {
                if (go && go.scene == active && go.name == name)
                    return go;
            }
            return null;
        }
    }
}
#endif
