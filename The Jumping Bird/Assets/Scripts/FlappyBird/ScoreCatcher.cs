using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCatcher : MonoBehaviour
{
    public uint m_uScore;
    public GameObject wall;
    // Start is called before the first frame update
    void Start()
    {
        m_uScore = 0;
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Finish"))
        {
            m_uScore++;
        }
    }
    public uint GetScore()
    {
        return m_uScore;
    }
}
