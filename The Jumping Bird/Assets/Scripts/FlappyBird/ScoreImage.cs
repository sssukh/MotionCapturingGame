using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreImage : MonoBehaviour
{
    public Sprite[] spriteArray;
    public GameObject ScoreCatcher;
    public bool IsPos10;
    public Sprite test;

    private uint m_uScore = 0;
    private Image currentImage;
    // Start is called before the first frame update
    void Start()
    {
        currentImage.GetComponent<Image>();
        currentImage.sprite = spriteArray[1];
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Size : " + spriteArray.Length);
        Debug.Log(spriteArray[0].name);
        m_uScore =  ScoreCatcher.GetComponent<ScoreCatcher>().GetScore();
        Debug.Log(m_uScore);
        if(IsPos10)
        {
            currentImage.sprite = spriteArray[m_uScore / 10];        }
        else
        {
            currentImage.sprite = spriteArray[m_uScore % 10];
        }
        Debug.Log(currentImage.name);
    }
}
