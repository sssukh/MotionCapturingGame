using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SceneName
{
    MainMenu,
    Test,
    Game1,
    FlappyBird,
}
public class SceneChangeManager : MonoBehaviour
{

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void ChangeScene(SceneName p_sceneName)
    {
        SceneManager.LoadScene(p_sceneName.ToString());
    }

}
