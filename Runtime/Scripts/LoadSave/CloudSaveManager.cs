using BranchMaker.Story;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BranchMaker.LoadSave
{
    [Serializable]
    public class SteamCloudPrefs
    {
        // Game Data //
        public string activeCase = "";
        public string activeCaseScene = "";
        public BranchMakerCloudSave saveFile;
    }

    public class CloudSaveManager : MonoBehaviour
    {
        static SteamCloudPrefs LocalStorage = new SteamCloudPrefs();

        private void Start()
        {
            LocalStorage = Load();

            if (LocalStorage == null)
            {
                LocalStorage = new SteamCloudPrefs();
            }

            if (ResumeSaveButtonManager.manager != null) ResumeSaveButtonManager.manager.CheckButton();
        }

        public static void UpdateSaveFile()
        {
            if (LocalStorage == null) LocalStorage = new SteamCloudPrefs();
            if (LocalStorage.saveFile == null) LocalStorage.saveFile = new BranchMakerCloudSave();
            LocalStorage.saveFile.Populate();
            Save(LocalStorage);
        }

        private void OnDestroy()
        {
            Save(LocalStorage);
        }


        private const string FILENAME = "/SteamCloud_SockNoir.sav";

        static string FilePos => Application.persistentDataPath + FILENAME;

        public static void Save(SteamCloudPrefs steamCloudPrefs)
        {
            if (StoryManager.manager == null) return;
            FileStream stream = new FileStream(FilePos, FileMode.Create);
            steamCloudPrefs.activeCaseScene = SceneManager.GetActiveScene().name;

            //Debug.Log(steamCloudPrefs.ToString());

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, steamCloudPrefs);
            // Debug.Log("<color=pink>SaveFile:</color> Saved to " + FilePos);
            stream.Close();
        }

        public static bool CheckForSaveFile()
        {
            if (LocalStorage == null) return false;
            if (string.IsNullOrEmpty(LocalStorage.activeCaseScene)) return false;
            return (LocalStorage != null);
        }

        public static string Resume()
        {
            StoryManager.forceLoad = LocalStorage.saveFile;
            return LocalStorage.activeCaseScene;
        }

        public static SteamCloudPrefs Load()
        {
            if (File.Exists(FilePos))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = new FileStream(FilePos, FileMode.Open);

                SteamCloudPrefs data = null;
                try
                {
                    data = bf.Deserialize(stream) as SteamCloudPrefs;
                }
                catch
                {

                }

                ;

                stream.Close();

                Debug.Log("<color=pink>SaveFile:</color> File found at " + FilePos);

                return data;
            }
            else
            {
                //Debug.LogError("File not found.");
                return null;
            }
        }
    }
}