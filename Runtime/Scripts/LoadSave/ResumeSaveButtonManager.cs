using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResumeSaveButtonManager : MonoBehaviour
{
    public static ResumeSaveButtonManager manager;
    public GameObject resumeButton;
    // Start is called before the first frame update
    private void Awake()
    {
        manager = this;
    }

    private void OnEnable()
    {
        CheckButton();
    }
    public void CheckButton()
    {
        resumeButton.SetActive(CloudSaveManager.CheckForSaveFile());
    }

    public void ResumeGame() {
        string scene = CloudSaveManager.Resume();
        SceneManager.LoadScene(scene);
    }
}
