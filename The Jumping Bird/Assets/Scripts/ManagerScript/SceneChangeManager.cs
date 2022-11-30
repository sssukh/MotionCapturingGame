using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SceneName
{
    MainMenu,
    Test,
    Game1,
    ScoreResult,
}

public class SceneChangeManager : MonoBehaviour
{
    private GameObject pipeServer = null;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        pipeServer = GameObject.Find("UserInput");
    }
    public void ChangeScene(SceneName p_sceneName,string _prePath="",string _content="",float _score = 0)
    {
        StartCoroutine(LoadAsyncScene(p_sceneName, _prePath, _content,_score));
        /*
        // �� ������ �����鼭 �Ⱦ��� �͵� ����
        pipeServer.GetComponent<UnityPipeServer>().ExitScene(p_sceneName.ToString());
        // �� ��ȯ
        SceneManager.LoadScene(p_sceneName.ToString());

        // �� ���� �����ϸ鼭 �ʿ��� �͵� ����
        pipeServer.GetComponent<UnityPipeServer>().EnterScene(p_sceneName.ToString(),_prePath,_content);
        */
    }

    public IEnumerator LoadAsyncScene(SceneName p_sceneName, string _prePath, string _content, float _score)
    {
        // �� ������ �����鼭 �Ⱦ��� �͵� ����
        pipeServer.GetComponent<UnityPipeServer>().ExitScene(p_sceneName.ToString());
        // �� ��ȯ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(p_sceneName.ToString());
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
        // �� ���� �����ϸ鼭 �ʿ��� �͵� ����
        pipeServer.GetComponent<UnityPipeServer>().EnterScene(p_sceneName.ToString(), _prePath, _content,_score);
    }

}
