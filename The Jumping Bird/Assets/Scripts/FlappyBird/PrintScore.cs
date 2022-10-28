using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrintScore : MonoBehaviour
{
    
    public TextMeshProUGUI text;
    public GameObject ScoreCatcher;
    private uint uScore;
    
    // Start is called before the first frame update
    void Start()
    {
        text.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

        uScore = ScoreCatcher.GetComponent<ScoreCatcher>().GetScore();
        text.text = "Score : " + uScore.ToString();
        
    }
}
