using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using SceneChangeManager;



public class ChangeScene : MonoBehaviour
{
   public void ChangeSceneBtn()
    {
        switch (this.gameObject.name)
        {
            case "StartBtn":
                SceneManager.LoadScene("Start");
                break;
            case "OptionBtn":
                SceneManager.LoadScene("Option");
                break;
            
        }
        
    }
}
