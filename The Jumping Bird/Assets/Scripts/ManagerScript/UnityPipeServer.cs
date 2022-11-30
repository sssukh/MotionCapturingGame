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

public enum Music
{
    SoHappy,
    HeroesTonight,
}


public class UnityPipeServer : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneChangeManagerObj = null;

    // ������ Ʈ��ŷ�� ���� ������ �� ������Ʈ
    private GameObject[] UserBody = new GameObject[14];
    // �������� ������ 3�� �̻� �������� ��� ȭ�鿡 ��Ÿ�� ��ȣ �� �ð� ����
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    // ���� ���� �ʿ��� ���� ������
    public Fileios fileios = null;
    private Scoreeffect scoreEffect = null;
    public GameObject gameTimerObj = null;
    public GameObject fileiosObject = null;
    public GameObject effectObject = null;
    public GameObject audioObject = null;
    private AudioManager audioManager = null;
    private GameTimer gameTimer = null;
    private SceneChangeManager sceneChangeManager = null;

    // ��� ���� �ʿ��� ������
    private GameObject scoreCounterObj = null;
    private ScoreCounter scoreCounter = null;

    // frame counter
    private int frameCounter;
    private float totalScore;

    // path
    string prePath = "/Assets/Contents";
    string happy = "/SoHappy/SoHappy";
    string heroes = "/HeroesTonight/HeroesTonight";

    // ������ Ʈ��ŷ�ؼ� �޾ƿ� ����Ʈ ������ ������ �迭
    // private int[] publicBuffer;
    private float[] publicBuffer;


    // �������Լ� �޾ƿ� �������� ������ ����
    private float[] userAnglesBuffer = new float[9];
    // ���Ͽ� ������ִ� �������� ������ ����
    private float[] fileAngleBuffer = new float[9];
  
    // ������
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;

    // ���� �� ��ġ
    private Scene curScene;


    // float ���� input ����Ʈ �����ϱ�
    public int xScale;
    public int yScale;

    public int xOffset;
    public int yOffset;

    // ������ ��� �� �������� ��
    [SerializeField]
    private int frame_amounts = 60;

    private bool isServerConnected = false;

    public int musicIdx;

    // ������ ���� ���� �׸��� ������ �̾��� �� ������ ���ڷ� �־ ������ ���Ѵ�.
    private float GetAngle(Body _first,Body _second, Body _middle)
    {
        // Debug.Log("check3");

        if (publicBuffer[(int)_first * 2] < 0 || publicBuffer[(int)_second * 2] < 0 ||
            publicBuffer[(int)_middle * 2] < 0 )
        {
            return -1f;
        }
        Vector2 first = new Vector2(publicBuffer[(int)_first * 2], publicBuffer[(int)_first * 2 + 1]);
        //Debug.Log("posx : " + publicBuffer[(int)_first * 2] + ", posy : " + publicBuffer[(int)_first * 2 + 1]);
        Vector2 second = new Vector2(publicBuffer[(int)_second * 2], publicBuffer[(int)_second * 2 + 1]);
        //Debug.Log("posx : " + publicBuffer[(int)_second * 2] + ", posy : " + publicBuffer[(int)_second * 2 + 1]);

        Vector2 middle = new Vector2(publicBuffer[(int)_middle * 2], publicBuffer[(int)_middle * 2 + 1]);
        //Debug.Log("posx : " + publicBuffer[(int)_middle * 2] + ", posy : " + publicBuffer[(int)_middle * 2 + 1]);

        // Debug.Log("first vector : (" + first.x + " ," + first.y + ") , second vector : (" + second.x + " , " + second.y + ") , middle vector : (" + middle.x + " , " + middle.y + ")");

        first -= middle;
        second -= middle;
        // Debug.Log("first vector : (" + first.x + " ," + first.y + ") , second vector : (" + second.x + " , " + second.y + ")");
        // ����� �������� �����ش�.(0<= angle <=180)
        return Vector2.Angle(first, second);
        /*
        float Dot = Vector2.Dot(first, second);

        // ���� �� ���� 0<= value <=pi
        return Mathf.Acos(Dot);
        */
    }

    // ������ ���� ������ ���ڷ� ���� �迭�� ���� ���� ä���.
    private void SetJointAngles(ref float[] _angles)
    {
        // Debug.Log("check2");

        // ��            �Ӹ� - �� - ������ ���
        //_angles[0] = GetAngle(Body.Head, Body.RShoulder, Body.Neck);
        userAnglesBuffer[0] = GetAngle(Body.Head, Body.RShoulder, Body.Neck);


        // ���Ȳ�ġ         �޼� - ���Ȳ�ġ - �޾��      
        //_angles[1] = GetAngle(Body.LWrist, Body.LShoulder, Body.LElbow);
        userAnglesBuffer[1] = GetAngle(Body.LWrist, Body.LShoulder, Body.LElbow);


        // �޾��          �� - �޾�� - ���Ȳ�ġ
       // _angles[2] = GetAngle(Body.Neck, Body.LElbow, Body.LShoulder);
        userAnglesBuffer[2] = GetAngle(Body.Neck, Body.LElbow, Body.LShoulder);


        // �����Ȳ�ġ        ������ - �����Ȳ�ġ - �������
       // _angles[3] = GetAngle(Body.RWrist, Body.RShoulder, Body.RElbow);
        userAnglesBuffer[3] = GetAngle(Body.RWrist, Body.RShoulder, Body.RElbow);


        // �������            �� - ������� - �����Ȳ�ġ
        //_angles[4] = GetAngle(Body.Neck, Body.RElbow, Body.RShoulder);
        userAnglesBuffer[4] = GetAngle(Body.Neck, Body.RElbow, Body.RShoulder);

        // �޹���              �ް�� - �޹��� - �޹�
        //_angles[5] = GetAngle(Body.LHip, Body.LAnkle, Body.LKnee);
        userAnglesBuffer[5] = GetAngle(Body.LHip, Body.LAnkle, Body.LKnee);

        // �ް��              ������� - �ް�� - �޹���
        //_angles[6] = GetAngle(Body.RHip, Body.LKnee, Body.LHip);
        userAnglesBuffer[6] = GetAngle(Body.RHip, Body.LKnee, Body.LHip);

        // ��������             ������� - �������� - ������
       // _angles[7] = GetAngle(Body.RHip, Body.RAnkle, Body.RKnee);
        userAnglesBuffer[7] = GetAngle(Body.RHip, Body.RAnkle, Body.RKnee);

        // �������             �ް�� - ������� - ��������
        //_angles[8] = GetAngle(Body.LHip, Body.RKnee, Body.RHip);
        userAnglesBuffer[8] = GetAngle(Body.LHip, Body.RKnee, Body.RHip);
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        // ������ �ʱ�ȭ
        pipeServer = new NamedPipeServerStream("CSServer", PipeDirection.In);
        Debug.Log("Opend pipe");
        // ������ ������� ǥ���� ������Ʈ 
        connectionLost = GameObject.Find("connectionLostSymbol");
        connectionLost.SetActive(false);
        // ������ Ʈ��ŷ�� ��
        for (Body i = Body.Head; i < Body.End; ++i)
        {
            UserBody[(int)i] = GameObject.Find(i.ToString());
        }

        serverReadThread = new Thread(ServerThread_Read);
        serverReadThread.Start();

        frameCounter = 0;


        curScene = SceneManager.GetActiveScene();

        sceneChangeManager = sceneChangeManagerObj.GetComponent<SceneChangeManager>();

    }


    public void EnterScene(string _sceneName,string _prePath="",string _content = "",float _score=0f)
    {
        curScene = SceneManager.GetActiveScene();

        if (_sceneName==SceneName.Game1.ToString())
        {
            /*
            GameObject[] lists = GameObject.FindObjectsOfType<GameObject>();
            Debug.LogError(lists.Length);
            for(int i=0;i<lists.Length;++i)
            {
                Debug.LogError(lists[i].name);
            }
            */
            gameTimerObj = GameObject.Find("Timer");
            
            fileiosObject = GameObject.Find("Fileios");
            
            effectObject = GameObject.Find("Effect");

            audioObject = GameObject.Find("Audio");

            gameTimer = gameTimerObj.GetComponent<GameTimer>();
            scoreEffect = effectObject.GetComponent<Scoreeffect>();
            fileios = fileiosObject.GetComponent<Fileios>();
            fileios.setFileName(_prePath + _content + ".bin");
            audioManager = audioObject.GetComponent<AudioManager>();

            if (_content[1] == 'S')
            {
                audioManager.SetMusic(0);
                musicIdx = (int)Music.SoHappy;
            }
            else
            { 
                audioManager.SetMusic(1);
                musicIdx = (int)Music.HeroesTonight;
            }

            fileios.GetName();
            fileios.createWriter();
            fileios.createReader();
            // ���� Ȯ���� ���� ������. Ȯ���ڵ� �߰��� ��.
            Time.timeScale = 0f;

            gameTimer.PreparingTime();

        }
        else if(_sceneName == SceneName.ScoreResult.ToString())
        {
            scoreCounterObj = GameObject.Find("ResultBackground");
            scoreCounter = scoreCounterObj.GetComponent<ScoreCounter>();

            scoreCounter.StartCount(_score);
        }
    }
    
    public void ExitScene(string _sceneName)
    {
        if (_sceneName == SceneName.Game1.ToString())
        {
            gameTimer = null;
            gameTimerObj = null;
            fileiosObject = null;
            effectObject = null;
            fileios = null;
            scoreEffect = null;
        }
    }

    // ���� �����忡�� ������ ����
    private void ServerThread_Read()
    {
        try
        {
            Debug.Log("Waiting for connection...");
            //pipeServer.WaitForConnection();
            pipeServer.BeginWaitForConnection(new AsyncCallback(this.PipeConnected), null);
            //Debug.Log("Client has connected!");
        }
        catch(IOException e)
        {
            Debug.LogError("IOException : " + e.Message);
        }
        catch(Exception e)
        {
            Debug.LogError("Error : " + e.Message);
        }



    }
    protected void PipeConnected(IAsyncResult ar)
    {
        try
        {
            this.pipeServer.EndWaitForConnection(ar);
            Debug.Log("Client connected!");
            isServerConnected = true;
        }
        catch(Exception e)
        {
            Debug.LogError("Error : " +  e.Message);
        }

    }


    // ����� �������� �ʴ� ����Ʈ �������, �� ���� �´� ��ºκ� ó���ϱ�
    private void ControllRenderPoints(int _offsetX, int _offsetY,string _curSceneName)
    {
        // ���� ���� � ������ ����
        // �ӽ÷� true
        bool isTesting;
        // Debug.Log(curScene.name);

        // �ӽ÷� �ּ�ó��
        
        if (_curSceneName == "Test")
            isTesting = true;
        else
            isTesting = false;
        

        if (isTesting)
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                // NaN �� �� �ش� ����Ʈ ������� �ʱ�
                if (publicBuffer[2 * i] <0 || publicBuffer[2 * i + 1] <0)
                {
                    UserBody[i].SetActive(false);
                    publicBuffer[2 * i] = 0;
                    publicBuffer[2 * i + 1] = 0;
                }
                else
                {
                    // test ���̸� ���� ���

                    UserBody[i].SetActive(true);
                    //Debug.Log("isTesting");
                   
                }
                //if (publicBuffer[2 * i] >0 || publicBuffer[2 * i + 1] >0)
                //{
                //    publicBuffer[2 * i] -= _offsetX;
                //    publicBuffer[2 * i + 1] -= _offsetY;
                //}
            }
        }
        else
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                if ((i==(int)Body.RWrist||i==(int)Body.LWrist) && publicBuffer[2 * i] >0 && publicBuffer[2 * i + 1] >0)
                {
                    UserBody[i].SetActive(true);
                }
                else
                {
                    UserBody[i].SetActive(false);
                    //publicBuffer[2 * i] = 0;
                    //publicBuffer[2 * i + 1] = 0;
                }
                //if (publicBuffer[2 * i] >0 || publicBuffer[2 * i + 1] >0)
                //{
                //    publicBuffer[2 * i] -= _offsetX;
                //    publicBuffer[2 * i + 1] -= _offsetY;
                //}
            }
        }
    }

    void Update()
    {
        //Debug.Log("updating....");
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
                    //Debug.LogWarning(idx + " : " + (float)fBuffer[idx]);
                    // NaN�� �� �ǵ帮�� �ʱ�
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
                //for(int i=0;i<publicBuffer.Length;++i)
                //{
                //    Debug.LogWarning(i + " : " + (float)fBuffer[i] + " , public : " + (float)publicBuffer[i]);
                //}
                Debug.Log("Read....");

                

                ControllRenderPoints(xOffset, yOffset,curScene.name);

                // ���� ������ �۵�
                InGameScene(curScene.name);
                

                for (int i = (int)Body.Head; i < (int)Body.End; ++i)
                {
                    UserBody[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(publicBuffer[2 * i]-xOffset, (publicBuffer[(2 * i) + 1] * -1 )- yOffset, 0);
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
            // 3���̻� ���� ���� �� ȭ�鿡 ǥ��
            connectionLostTime += Time.deltaTime;
            if (connectionLostTime >= 1.0f)
            {
                connectionLost.SetActive(true);
            }

            /*
            // ������ ����Ǿ����� �ʰ� �����尡 �׾������� �������� ������ ����
            if(!serverReadThread.IsAlive)
            {
                serverReadThread.Start();
                Debug.Log("Server Connecting Start");
            }
            // ���� ���� �����尡 3���̻� �����Ǹ�(waitforconnection ���) interrupt
            else if(connectionLostTime>=3.0f)
            {
                serverReadThread.Interrupt();
                // serverReadThread.Abort();
                Debug.Log("Close Connection");
            }
            */
        }
    }

    // ���� ��
    float checkScore(ref float[] _userArray, ref float[] _fileArray)
    {
        float result = 0;
        int count = 0;
        for(int i=0;i<_userArray.Length;++i)
        {
            float userData = _userArray[i];
            float fileData = _fileArray[i];

            Debug.Log("User : " + _userArray[i] + " , File : " + _fileArray[i]);

            // NaN�� ��� �� ���� �ʱ�
            if (userData < 0 || fileData < 0)
                continue;
            // �������� result�� ���ϱ�
            result += Math.Abs(userData - fileData) / 180f;
            count++;
            Debug.Log("result : " + result);
        }
        return 100 - (result/count)*100;
    }


    void InGameScene(string _curSceneName)
    {
       
        if (_curSceneName != "Game1")
        {
            return;
        }

        if (gameTimer.IsMusicOver(musicIdx, 3f)&&!audioManager.IsPlaying())
        {
            sceneChangeManager.ChangeScene(SceneName.ScoreResult, "", "", 20000);
        }

        Debug.Log("check1");

        SetJointAngles(ref userAnglesBuffer);
        for(int i=0;i<userAnglesBuffer.Length;++i)
        {
            Debug.Log("Angle" + i + " : " + (float)userAnglesBuffer[i]);
        }
        // write
        //fileios.bWrite(gameTimer.GetComponent<GameTimer>().getTimer(), ref userAnglesBuffer);

        // read
        fileios.bRead(fileAngleBuffer.Length, gameTimer.GetComponent<GameTimer>().getTimer(), ref fileAngleBuffer,100f);
        //for(int i=0;i<fileAngleBuffer.Length;++i)
        //{
        //    Debug.LogWarning("file " + i + " : " + fileAngleBuffer[i]);
        //}
        float score = checkScore(ref userAnglesBuffer, ref fileAngleBuffer);

        Debug.Log("Score : " + score);
        totalScore += score;
        if (frameCounter >= frame_amounts)
        {
            frameCounter = 0;
            // 30�����ӵ��� ���� ��ճ���
            float average = totalScore / frame_amounts;
            totalScore = 0;
            int num;
            // ����Ʈ ȭ�鿡 ��Ÿ����
            if (average > 80)
            {
                // great
                num = 3;
            }
            else if (average > 60)
            {
                // good
                num = 2;
            }
            else if (average > 40)
            {
                // not bad
                num = 1;
            }
            else
            {
                // bad
                num = 0;
            }
            scoreEffect.JudgementEffect(num);
        }
        else
        {
            frameCounter++;
        }


    }

    public bool IsConnected()
    {
        return isServerConnected;
    }
    ~UnityPipeServer()
    {
        pipeServer.Close();
    }
}
