using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializingScene : MonoBehaviour
{
    private float fTimer = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fTimer += Time.deltaTime;
        if (fTimer >= 2.0f)
            SceneManager.LoadScene("MainMenu");
    }
}
