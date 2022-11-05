using Newtonsoft.Json;
using System;
using System.Buffers;
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

    private FileIOS fileios;
    
    // 유저를 트래킹해서 받아온 포인트 값들을 저장할 배열
    // private int[] publicBuffer;
    private float[] publicBuffer;


    // 유저에게서 받아온 각도들을 저장할 버퍼
    private float[] userAnglesBuffer = new float[9];
    // 파일에 저장되있던 각도들을 저장할 버퍼
    private float[] fileAngleBuffer = new float[9];
  
    // 파이프
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;

    // float 유저 input 리스트 조절하기
    public int xScale;
    public int yScale;

    public int xOffset;
    public int yOffset;
    // 각도를 구할 관절 그리고 관절과 이어진 양 끝점을 인자로 넣어서 각도를 구한다.
    private float GetRadiabAngle(Body _first,Body _second, Body _middle)
    {
        Vector2 first = new Vector2(publicBuffer[(int)_first * 2], publicBuffer[(int)_first * 2 + 1]);
        Vector2 second = new Vector2(publicBuffer[(int)_second * 2], publicBuffer[(int)_second * 2 + 1]);
        Vector2 middle = new Vector2(publicBuffer[(int)_middle * 2], publicBuffer[(int)_middle * 2 + 1]);
        first -= middle;
        second -= middle;

        // 양수의 각도값을 구해준다.(0<= angle <=180)
        return Vector2.Angle(first, second);
        /*
        float Dot = Vector2.Dot(first, second);

        // 라디안 값 리턴 0<= value <=pi
        return Mathf.Acos(Dot);
        */
    }

    // 다음과 같은 순서로 인자로 받은 배열에 각도 값을 채운다.
    private void SetJointAngles(ref float[] _angles)
    {
        // 목            머리 - 목 - 오른쪽 어깨
        _angles[0] = GetRadiabAngle(Body.Head, Body.RShoulder, Body.Neck);

        // 왼팔꿈치         왼손 - 왼팔꿈치 - 왼어깨      
        _angles[1] = GetRadiabAngle(Body.LWrist, Body.LShoulder, Body.LElbow);

        // 왼어깨          목 - 왼어깨 - 왼팔꿈치
        _angles[2] = GetRadiabAngle(Body.Neck, Body.LElbow, Body.LShoulder);

        // 오른팔꿈치        오른손 - 오른팔꿈치 - 오른어깨
        _angles[3] = GetRadiabAngle(Body.RWrist, Body.RShoulder, Body.RElbow);

        // 오른어깨            목 - 오른어깨 - 오른팔꿈치
        _angles[4] = GetRadiabAngle(Body.Neck, Body.RElbow, Body.RShoulder);

        // 왼무릎              왼골반 - 왼무릎 - 왼발
        _angles[5] = GetRadiabAngle(Body.LHip, Body.LAnkle, Body.LKnee);

        // 왼골반              오른골반 - 왼골반 - 왼무릎
        _angles[6] = GetRadiabAngle(Body.RHip, Body.LKnee, Body.LHip);

        // 오른무릎             오른골반 - 오른무릎 - 오른발
        _angles[7] = GetRadiabAngle(Body.RHip, Body.RAnkle, Body.RKnee);

        // 오른골반             왼골반 - 오른골반 - 오른무릎
        _angles[8] = GetRadiabAngle(Body.LHip, Body.RKnee, Body.RHip);
    }

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
                if (publicBuffer[2 * i] == Single.NaN || publicBuffer[2 * i + 1] == Single.NaN)
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
                if (publicBuffer[2 * i] != Single.NaN || publicBuffer[2 * i + 1] != Single.NaN)
                {
                    publicBuffer[2 * i] -= _offsetX;
                    publicBuffer[2 * i + 1] -= _offsetY;
                }
            }
        }
        else
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                if ((i==(int)Body.RWrist||i==(int)Body.LWrist) && publicBuffer[2 * i] != Single.NaN && publicBuffer[2 * i + 1] != Single.NaN)
                {
                    UserBody[i].SetActive(true);
                }
                else
                {
                    UserBody[i].SetActive(false);
                }
                if (publicBuffer[2 * i] != Single.NaN || publicBuffer[2 * i + 1] != Single.NaN)
                {
                    publicBuffer[2 * i] -= _offsetX;
                    publicBuffer[2 * i + 1] -= _offsetY;
                }
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
                float[] fBuffer = new float[36];
                // int[] fBuffer = new int[36];
                var num = pipeServer.Read(buffer, 0, 144);

                Debug.Log("got " + num + " bytes");
                
                Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

                for (int idx = 0; idx < 36; ++idx)
                {
                    // NaN일 시 건드리지 않기
                    if (fBuffer[idx] == Single.NaN)
                    {
                        continue;
                    }
                    else if(idx%2==0)
                    {
                        fBuffer[idx] *= xScale;
                    }
                    else
                    {
                        fBuffer[idx] *= yScale;
                    }
                }
                
                /*
                for (int idx = 0; idx < 36; ++idx)
                {
                    ArraySegment<byte> arrSegInt = new ArraySegment<byte>(buffer, idx * 4, 4);
                    int tmp = BitConverter.ToInt32(arrSegInt);
                    fBuffer[idx] = tmp;
                }
                */
                publicBuffer = fBuffer;

                Debug.Log("Read....");

                

                ControllRenderPoints(xOffset, yOffset);

                for (int i = (int)Body.Head; i < (int)Body.End; ++i)
                {
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

    // 각도 비교
    float checkScore(ref float[] _userArray, ref float[] _fileArray)
    {
        float result = 0;
        int count = 0;
        for(int i=0;i<_userArray.Length;++i)
        {
            float userData = _userArray[i];
            float fileData = _fileArray[i];
            // NaN일 경우 비교 하지 않기
            if (userData < 0 || fileData < 0)
                continue;
            // 오차값들 result에 더하기
            result += Math.Abs(userData - fileData) / 180f;
            count++;
        }
        return 100 - (result/count);
    }

}
