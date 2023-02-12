using BranchMaker.Story;
using System;
using System.Collections.Generic;


namespace BranchMaker.LoadSave
{
    
[Serializable]
public class BranchMakerCloudSave
{
    public string currentNode;
    public string backgroundScene;
    public List<string> unlockedWords;
    public List<string> customData = new List<string>();
    public static List<ILoadSaveElement> SaveElements = new();
    public Dictionary<string, List<string>> ElementDict = new();
    public Dictionary<string, List<string[]>> ElementBitsDict = new();

    public static void RegisterLoadSaveElement(ILoadSaveElement element)
    {
        if (!SaveElements.Contains(element)) SaveElements.Add(element);
    }

    public void Populate()
    {
        currentNode = StoryManager.currentnode.id;
        unlockedWords = StoryButton.playerkeys;
        backgroundScene = StorySceneManager.CurrentSceneLoaded();

        foreach (var elements in SaveElements)
        {
            elements.WriteData(this);
        }
        
    }

    public void Resume()
    {
        StoryButton.playerkeys = unlockedWords;
        foreach (var elements in SaveElements)
        {
            elements.WriteData(this);
        }
    }
}

}