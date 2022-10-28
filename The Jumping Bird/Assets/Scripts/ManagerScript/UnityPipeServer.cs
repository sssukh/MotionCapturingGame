using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Body
{
    Head,
    Neck,
    RShoulder,
    RElbow,
    RWrist,
    LShoulder,
    LElbow,
    LWrist,
    RHip,
    RKnee,
    RAnkle,
    LHip,
    LKnee,
    LAnkle,
    End
}

public class UnityPipeServer : MonoBehaviour
{
    // 추가
    // 유저를 트래킹한 값을 적용할 모델 오브젝트
    private GameObject[] UserBody = new GameObject[14];
    // 파이프가 연결이 3초 이상 끊겨있을 경우 화면에 나타낼 신호 및 시간 측정
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    /// <summary>
    /// 
    /// </summary>
    // 유저를 트래킹해서 받아온 포인트 값들을 저장할 배열
    private int[] publicBuffer;
  
    // 파이프
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        // 파이프 초기화
        pipeServer = new NamedPipeServerStream("CSServer", PipeDirection.In);
        // 파이프 연결끊김 표시할 오브젝트 
        connectionLost = GameObject.Find("connectionLostSymbol");
        connectionLost.SetActive(false);
        // 유저를 트래킹할 모델
        for (Body i = Body.Head; i < Body.End; ++i)
        {
            UserBody[(int)i] = GameObject.Find(i.ToString());
        }

        /*
        for (Body i = Body.Head; i < Body.End; ++i)
        {
            UserBody[(int)i].GetComponent<SpriteRenderer>().color = Color.green;
        }
        */


        serverReadThread = new Thread(ServerThread_Read);
        serverReadThread.Start();
    }



    // 서버 쓰레드에서 파이프 연결
    private void ServerThread_Read()
    {
        Debug.Log("Waiting....");
        pipeServer.WaitForConnection();
        Debug.Log("Client has connected!");
    }

    // 제대로 추적되지 않는 포인트 출력제외, 각 씬에 맞는 출력부분 처리하기
    private void ControllRenderPoints(int _offsetX, int _offsetY)
    {
        // 현재 씬이 어떤 씬인지 추적
        bool isTesting;
        Scene curScene = SceneManager.GetActiveScene();
        // Debug.Log(curScene.name);
        if (curScene.name == "Test")
            isTesting = true;
        else
            isTesting = false;

        if (isTesting)
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                // NaN 일 시 해당 포인트 출력하지 않기
                if (publicBuffer[2 * i] == -1 || publicBuffer[2 * i + 1] == -1)
                {
                    UserBody[i].SetActive(false);
                }
                else
                {
                    // test 씬이면 전부 출력
                    if (isTesting)
                    {
                        UserBody[i].SetActive(true);
                        //Debug.Log("isTesting");
                    }
                    // test 씬이 아니면 손만 출력
                    else if (!isTesting && (i == (int)Body.LWrist || i == (int)Body.RWrist))
                    {
                        UserBody[i].SetActive(true);
                        //Debug.Log("isnotTesting");
                    }
                }
                publicBuffer[2 * i] -= _offsetX;
                publicBuffer[2 * i + 1] -= _offsetY;
            }
        }
        else
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                if ((i==(int)Body.RWrist||i==(int)Body.LWrist) && publicBuffer[2 * i] != -1 && publicBuffer[2 * i + 1] != -1)
                {
                    UserBody[i].SetActive(true);
                }
                else
                {
                    UserBody[i].SetActive(false);
                }
                publicBuffer[2 * i] -= _offsetX;
                publicBuffer[2 * i + 1] -= _offsetY;
            }
        }
    }

    void Update()
    {
        if (pipeServer.IsConnected)
        {
            connectionLostTime = 0f;
            connectionLost.SetActive(false);
            try
            {

                byte[] buffer = new byte[144];
                int[] fBuffer = new int[36];
                var num = pipeServer.Read(buffer, 0, 144);

                Debug.Log(num + "got 144bytes");

                for (int idx = 0; idx < 36; ++idx)
                {
                    ArraySegment<byte> arrSegInt = new ArraySegment<byte>(buffer, idx * 4, 4);
                    int tmp = BitConverter.ToInt32(arrSegInt);
                    fBuffer[idx] = tmp;
                }
                publicBuffer = fBuffer;

                Debug.Log("Read....");

                int offsetX = publicBuffer[((int)Body.Neck) * 2];
                int offsetY = publicBuffer[((int)Body.Neck * 2) + 1];

                ControllRenderPoints(offsetX, offsetY);

                for (int i = (int)Body.Head; i < (int)Body.End; ++i)
                {
                    /*
                    Vector3 temp = new Vector3(publicBuffer[2 * i] , publicBuffer[(2 * i) + 1] * -1 , 0);
                    Debug.Log("x : " + temp.x + ", y : " + temp.y + ", z : " + temp.z);
                    */
                    UserBody[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(publicBuffer[2 * i], publicBuffer[(2 * i) + 1] * -1, 0);

                }

            }
            catch (IOException e)
            {
                Debug.Log("ERROR : " + e.Message);
            }
            catch (System.ObjectDisposedException e)
            {
                Debug.Log(e.Message);
            }
        }
        //Debug.Log("pipeServer Closed");

        else
        {
            // 3초이상 연결 끊길 시 화면에 표시
            connectionLostTime += Time.deltaTime;
            if (connectionLostTime >= 1.0f)
            {
                connectionLost.SetActive(true);
            }

            // 서버가 연결되어있지 않고 쓰레드가 죽어있으면 서버연결 쓰레드 실행
            if(!serverReadThread.IsAlive)
            {
                serverReadThread.Start();
                Debug.Log("Server Connecting Start");
            }
            // 서버 연결 쓰레드가 3초이상 유지되면(waitforconnection 대기) interrupt
            else if(connectionLostTime>=3.0f)
            {
                serverReadThread.Interrupt();
                Debug.Log("Close Connection");
            }

        }
    }

}
