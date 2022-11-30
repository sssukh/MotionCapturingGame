using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField]
    private GameObject textObj = null;
    private TextMeshProUGUI myText = null;

    [SerializeField]
    private Animator CountAnimator = null;

    [SerializeField]
    private Animator ResultSceneBegin = null;

    string start = "start";
    // Start is called before the first frame update
    void Start()
    {
        myText = textObj.GetComponent<TextMeshProUGUI>();   
    }

    // Update is called once per frame
    public void StartCount(float _score)
    {
        StartCoroutine(Count(_score,0));
    }

    IEnumerator Count(float target,float current)
    {
        ResultSceneBegin.SetTrigger(start);
        yield return new WaitForSeconds(1.0f);
        CountAnimator.SetTrigger(start);
        // �ִϸ��̼� ������ ���� ��ٸ��� �ð�.
        yield return new WaitForSeconds(0.5f);
        float duration = 0.5f;
        float offset = (target - current) / duration;

        while(current<target)
        {
            current += offset * Time.deltaTime;
            myText.text = ((int)current).ToString();
            yield return new WaitForSeconds(0.001f);
        }
        current = target;
        myText.text = ((int)current).ToString();
    }

}
