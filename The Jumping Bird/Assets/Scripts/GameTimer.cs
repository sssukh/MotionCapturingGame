using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float gameTimer;

    [SerializeField]
    private Sprite[] timeCounts;

    private Image thisImage;

    void Start()
    {
        gameTimer = 0f;
        thisImage = GetComponent<Image>();
    }
    private void Update()
    {
        gameTimer += Time.deltaTime;
    }
    // Update is called once per frame


    public float getTimer() { return gameTimer; }

    public void PauseGame()
    {
        Debug.LogError("Pause");
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void PreparingTime()
    {
        StartCoroutine(PrepTime());
    }

    // 게임 시작 혹은 재개 시에 유저가 준비할 수 있도록 3초의 대기시간을 준다.
    private IEnumerator PrepTime()
    {
        //gameObject.SetActive(true);
        PauseGame();

        thisImage.color = new Color(255,255,255,255);
        thisImage.sprite = timeCounts[0];
        yield return new WaitForSecondsRealtime(1f);
        thisImage.sprite = timeCounts[1];
        yield return new WaitForSecondsRealtime(1f);
        thisImage.sprite = timeCounts[2];
        yield return new WaitForSecondsRealtime(1f);
        thisImage.color = new Color(0, 0, 0, 0);

        ResumeGame();

    }
    /*
    public void PrepareTime()
    {
        float crtTime = 0f;
        while(crtTime<3f)
        {
            if (crtTime <= 1f)
            {
                thisImage.sprite = timeCounts[0];
                Debug.Log("01");

            }
            else if (crtTime < 2f)
            {
                thisImage.sprite = timeCounts[1];
                Debug.Log("02");

            }
            else
            {
                thisImage.sprite = timeCounts[2];
                Debug.Log("03");

            }
            crtTime += Time.unscaledDeltaTime;
        }
        gameObject.SetActive(false);
    }
    */
}
