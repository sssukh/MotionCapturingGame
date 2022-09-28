using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallSpawner : MonoBehaviour
{
    public GameObject UpperWall;
    public GameObject LowerWall;
    public  uint i_Wall_Width;
    public float f_Wall_Speed;
    public float f_Wall_Interval;

    public float f_BlankHeight;

    public GameObject player;
    private float fRoof_y = 10.0f;
    private float fFloor_y = -10.0f;
    private float fStart_x = 25.0f;
    private float fEnd_x = -25.0f;
    private float f_Timer=0.0f;
    private float f_renderRoofy;


    LinkedList<GameObject> Wall_List = new LinkedList<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        f_renderRoofy = fRoof_y + 2;
        AddtoWalllist();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.GetComponent<Player>().GetPlayerHit())
        {
            f_Timer += Time.deltaTime;
            while (Wall_List.First.Value.GetComponent<Transform>().position.x < fEnd_x)
            {
                Destroy(Wall_List.First.Value);
                Wall_List.RemoveFirst();
            }

            IEnumerator<GameObject> currentWall = Wall_List.GetEnumerator();

            while (currentWall.MoveNext())
            {
                Transform currentWall_Transform = currentWall.Current.GetComponent<Transform>();
                currentWall_Transform.Translate(-f_Wall_Speed, 0, 0);
            }
            float currentTime = Time.deltaTime;
            if (f_Timer > f_Wall_Interval)
            {
                AddtoWalllist();
                f_Timer = 0.0f;
            }
        }
    }



    void AddtoWalllist()
    {
        GameObject upperWall = Instantiate(UpperWall);
        float f_upperWallFloor = Random.Range(fFloor_y + f_BlankHeight, fRoof_y);
        upperWall.transform.position = new Vector2(fStart_x, (f_renderRoofy + f_upperWallFloor) / 2);
        upperWall.transform.localScale = new Vector2(i_Wall_Width, f_renderRoofy - f_upperWallFloor);
        Wall_List.AddLast(upperWall);

        GameObject lowerWall = Instantiate(LowerWall);
        float f_lowerWallRoof = f_upperWallFloor - f_BlankHeight;
        lowerWall.transform.position = new Vector2(fStart_x, (fFloor_y + f_lowerWallRoof) / 2);
        lowerWall.transform.localScale = new Vector2(i_Wall_Width, f_lowerWallRoof - fFloor_y);
        Wall_List.AddLast(lowerWall);
    }
}
