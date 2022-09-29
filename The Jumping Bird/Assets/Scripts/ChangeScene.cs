using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;





public class ChangeScene : MonoBehaviour
{
    public GameObject sceneChangeManager;

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
            case "StartBtn":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.Game1);
                //sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.FlappyBird);
                break;
            case "TestBtn":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.Test);
                break;
            case "BackBtn":
                sceneChangeManager.GetComponent<SceneChangeManager>().ChangeScene(SceneName.MainMenu);
                break;

        }
        
    }
}
