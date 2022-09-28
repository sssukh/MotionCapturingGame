using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneChangeManager
{
    public enum SceneName
    {
        MainMenu,
        Option,
        FlappyBird,
    }
    public class SceneChangeManager : MonoBehaviour
    {
       

        public void ChangeScene(SceneName p_sceneName)
        {
            SceneManager.LoadScene(p_sceneName.ToString());
        }

    }
}