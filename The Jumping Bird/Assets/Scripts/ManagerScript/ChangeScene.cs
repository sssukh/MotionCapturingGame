using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;





public class ChangeScene : MonoBehaviour
{
    public GameObject sceneChangeManager;

    string prePath = "Assets/Contents";
    string happy = "/SoHappy/SoHappy";
    string heroes = "/HeroesTonight/HeroesTonight";

    public void Awake()
    {
        sceneChangeManager = GameObject.Find("SceneChangeManager");
        if(sceneChangeManager==null)
        {
            Debug.Log("Error : No SCM");
        }
    }
    public void ChangeSceneBtn()
    {
        switch (this.gameObject.name)
        {
            // changeScene에 인자로 노래정보 주기.
            
            case "TestBtn":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.Test);
                break;
            case "BackBtn":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.MainMenu);
                break;
            case "Music1":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.Game1, prePath , happy);
                break;
            case "Music2":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.Game1, prePath , heroes);
                break;
            default:
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.MainMenu);
                break;
        }
    }

    private void ClickBtn()
    {
        switch (this.gameObject.name)
        {
            case "PauseBtn":
                break;
            case "ResumeBtn":
                break;
        }
        

    }
}
