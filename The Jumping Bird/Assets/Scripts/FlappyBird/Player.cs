using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D playerRigidbody;
    private Transform playerTransform;
    public Vector2 v_JumpForce;

    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public GameObject wall;
    public CanvasGroup exitBackgroundImageCanvasGroup;

    public Animator Bird_Animator;

    bool b_IsPlayerHit = false;
    float m_Timer;

    public bool GetPlayerHit()
    {
        return b_IsPlayerHit;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
        Bird_Animator = GetComponent<Animator>();
        Bird_Animator.transform.localScale = new Vector3(1, 1, 0) * 10;
        Bird_Animator.SetBool("Is_End", false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!b_IsPlayerHit&&Input.GetKey(KeyCode.Space))
        {
            Bird_Animator.SetBool("Is_Flapping", true);
            playerRigidbody.AddForce(v_JumpForce);
        }
        else
        {
            Bird_Animator.SetBool("Is_Flapping", false);
        }


        if (playerTransform.position.y > 9)
        {
            playerTransform.position = new Vector3(playerTransform.position.x, 9f, 0);
        }
        else if (playerTransform.position.y < -9)
        {
            playerTransform.position = new Vector3(playerTransform.position.x, -9f, 0);

        }

        if (b_IsPlayerHit)
        {
            //playerRigidbody.gravityScale = 0;
            Bird_Animator.SetBool("Is_End", true);
            EndLevel();
        }
        
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Finish"))
        {
            b_IsPlayerHit = true;
        }
    }
    void EndLevel()
    {
        m_Timer += Time.deltaTime;
        exitBackgroundImageCanvasGroup.alpha = m_Timer / fadeDuration;
        if (m_Timer > fadeDuration + displayImageDuration)
        {
            Application.Quit();
        }
    }
}
