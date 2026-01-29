#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using BranchMaker.Api;
using BranchMaker.WebServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace BranchMaker.Editor.Editors
{
    [CustomEditor(typeof(StoryManager))]
    public sealed class StoryManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _storybookIdProp;
        private SerializedProperty _debugLogProp;
        private GUIContent _storybookIdLabel;
        private UnityWebRequest _cacheRequest;
        private bool _isDownloadingCache;
        private double _downloadStartTime;
        private BranchmakerCacheObject _downloadingTarget;
        private SerializedProperty _storyCacheProp;
        
        private static readonly Regex UuidRegex = new (
            "^[{(]?[0-9a-fA-F]{8}(-?[0-9a-fA-F]{4}){3}-?[0-9a-fA-F]{12}[)}]?$",
            RegexOptions.Compiled
        );

        private void OnEnable()
        {
            _storybookIdProp = serializedObject.FindProperty("storybookId");
            _storybookIdLabel = new GUIContent("Storybook API Key");
            _debugLogProp = serializedObject.FindProperty("debugLog");
            _storyCacheProp = serializedObject.FindProperty("StoryCache");
        }
        private void OnDisable()
        {
            CleanupDownload(clearProgressBar: true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BranchmakerEditorHeader.DrawHeader("BranchMaker Manager", "This is the beating heart of the BranchMaker experience.");

            DrawHeaderControls();

            DrawStoryCacheControls();

            EditorGUILayout.Space(8);

            //DrawPropertiesExcluding(serializedObject, "m_Script", "storybookId");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderControls()
        {
                EditorGUILayout.LabelField("Basic Setup:", EditorStyles.boldLabel);
                var value = (_storybookIdProp.stringValue ?? string.Empty).Trim();
                if (_storybookIdProp != null)
                {
                    EditorGUILayout.PropertyField(_storybookIdProp, _storybookIdLabel);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Could not find serialized field 'storybookId'. Make sure the field name matches exactly.",
                        MessageType.Warning
                    );
                }
                if (string.IsNullOrEmpty(value) || LooksLikePlaceholder(value))
                {
                    EditorGUILayout.HelpBox(
                        "Enter your Storybook ID (UUID format). Example: fb0911d0-62d3-11eb-9576-430b45eedfef",
                        MessageType.Info
                    );
                }
                else if (!IsUuid(value))
                {
                    EditorGUILayout.HelpBox(
                        "This doesn't look like a valid UUID. Expected format like: fb0911d0-62d3-11eb-9576-430b45eedfef",
                        MessageType.Error
                    );
                }

                
                EditorGUILayout.PropertyField(
                    _debugLogProp,
                    new GUIContent("Debug Logging", "Enable verbose BranchMaker logging")
                );
                
                EditorGUILayout.Space(6);

                if (GUILayout.Button("Open Dashboard", GUILayout.Height(24)))
                {
                    Application.OpenURL("https://branchmaker.com");
                }
        }
        
        #region CACHE OBJECT

         private void DrawStoryCacheControls()
        {
            EditorGUILayout.LabelField("Cache:", EditorStyles.boldLabel);

            if (_storyCacheProp == null)
            {
                EditorGUILayout.HelpBox(
                    "Could not find serialized field 'StoryCache'. Make sure StoryManager has a field named exactly 'StoryCache' and it is serialized (public or [SerializeField]).",
                    MessageType.Warning
                );
                return;
            }

            EditorGUILayout.PropertyField(
                _storyCacheProp,
                new GUIContent("Story Cache", "ScriptableObject asset used to store downloaded story cache data")
            );

            var cacheObj = _storyCacheProp.objectReferenceValue as BranchmakerCacheObject;

            // Label under the cache object showing LastUpdateDate + NodeCount
            if (cacheObj != null)
            {
                // (Formatting: local time + ISO-ish)
                var last = cacheObj.LastUpdateDate;
                var lastText = last == default ? "Never" : last.ToString("yyyy-MM-dd HH:mm:ss");
                EditorGUILayout.LabelField("Last Update", lastText);
                EditorGUILayout.LabelField("Node Count", cacheObj.NodeCount.ToString());
            }

            EditorGUILayout.Space(4);

            // Update Cache button under the cache value
            using (new EditorGUI.DisabledScope(!cacheObj || _isDownloadingCache))
            {
                if (GUILayout.Button("Update Cache", GUILayout.Height(22)))
                {
                    TryStartCacheDownload(cacheObj);
                }
            }

            if (!cacheObj)
            {
                EditorGUILayout.HelpBox("No StoryCache assigned.", MessageType.Info);
                return;
            }

            // Show any basic validation under the button
            if (string.IsNullOrWhiteSpace(cacheObj.cacheUrl))
            {
                EditorGUILayout.HelpBox("Cache URL is empty on the cache asset (cacheUrl). Set it to the JSON feed URI.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Feed URL", cacheObj.cacheUrl);
            }
        }

        private async void TryStartCacheDownload(BranchmakerCacheObject cacheObj)
        {
            if (!cacheObj) return;
            StoryManager.ForceLocate();

            var url = (cacheObj.cacheUrl ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(url))
            {
                EditorUtility.DisplayDialog("Update Cache", "Cache URL (cacheUrl) is empty.", "OK");
                return;
            }

            EditorUtility.DisplayProgressBar("BranchMaker", "Downloading cache update...", 0f);
            var json = await APIRequest.FetchFromApi(
                BranchmakerPaths.StoryNodes(true, _storybookIdProp.stringValue)+"?noncache",
                "story"
            );
            cacheObj.cacheData = json;
            cacheObj.LastUpdateDate = DateTime.Now;
            cacheObj.NodeCount = ComputeNodeCount(json);

            EditorUtility.SetDirty(cacheObj);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            CleanupDownload(clearProgressBar: true);
            StoryManager.Instance.Log("Download complete. Cache object updated.");
        }
        
        private void CleanupDownload(bool clearProgressBar)
        {
            if (clearProgressBar)
                EditorUtility.ClearProgressBar();

            _isDownloadingCache = false;

            _downloadingTarget = null;
        }

        private static int ComputeNodeCount(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return 0;
            
            try
            {
                var docType = Type.GetType("System.Text.Json.JsonDocument, System.Text.Json");
                if (docType != null)
                {
                    // dynamic-free reflection approach
                    var parseMethod = docType.GetMethod("Parse", new[] { typeof(string) });
                    var doc = parseMethod.Invoke(null, new object[] { json });

                    var rootProp = docType.GetProperty("RootElement");
                    var root = rootProp.GetValue(doc);

                    // JsonElement API via reflection
                    var jsonElementType = root.GetType();
                    var valueKindProp = jsonElementType.GetProperty("ValueKind");
                    var valueKind = valueKindProp.GetValue(root).ToString();

                    // If root is array, count it
                    if (valueKind == "Array")
                    {
                        var getArrayLength = jsonElementType.GetMethod("GetArrayLength", Type.EmptyTypes);
                        var len = (int)getArrayLength.Invoke(root, null);

                        // dispose doc
                        docType.GetMethod("Dispose")?.Invoke(doc, null);
                        return len;
                    }

                    // If root is object, try "nodes" array
                    if (valueKind == "Object")
                    {
                        var tryGetProp = jsonElementType.GetMethod("TryGetProperty", new[] { typeof(string), jsonElementType.MakeByRefType() });
                        object[] args = { "nodes", Activator.CreateInstance(jsonElementType) };
                        var ok = (bool)tryGetProp.Invoke(root, args);

                        if (ok)
                        {
                            var nodesEl = args[1];
                            var nodesKind = valueKindProp.GetValue(nodesEl).ToString();
                            if (nodesKind == "Array")
                            {
                                var getArrayLength = jsonElementType.GetMethod("GetArrayLength", Type.EmptyTypes);
                                var len = (int)getArrayLength.Invoke(nodesEl, null);

                                docType.GetMethod("Dispose")?.Invoke(doc, null);
                                return len;
                            }
                        }
                    }

                    docType.GetMethod("Dispose")?.Invoke(doc, null);
                }
            }
            catch
            {
                // ignore and fall through
            }

            // Heuristic fallback:
            // Prefer counting `"uuid"` occurrences, else `"id"`.
            var uuidCount = Regex.Matches(json, "\"uuid\"\\s*:").Count;
            if (uuidCount > 0) return uuidCount;

            var idCount = Regex.Matches(json, "\"id\"\\s*:").Count;
            return idCount;
        }

        #endregion
        private static bool IsUuid(string s)
        {
            return UuidRegex.IsMatch(s);
        }
        

        private static bool LooksLikePlaceholder(string s)
        {
            var lower = s.ToLowerInvariant();
            return lower.Contains("place storybook") || lower.Contains("api key") || lower.Contains("uuid here");
        }
    }
}
#endif
