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
    // �߰�
    // ������ Ʈ��ŷ�� ���� ������ �� ������Ʈ
    private GameObject[] UserBody = new GameObject[14];
    // �������� ������ 3�� �̻� �������� ��� ȭ�鿡 ��Ÿ�� ��ȣ �� �ð� ����
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    private FileIOS fileios;
    
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

    // float ���� input ����Ʈ �����ϱ�
    public int xScale;
    public int yScale;

    public int xOffset;
    public int yOffset;
    // ������ ���� ���� �׸��� ������ �̾��� �� ������ ���ڷ� �־ ������ ���Ѵ�.
    private float GetRadiabAngle(Body _first,Body _second, Body _middle)
    {
        Vector2 first = new Vector2(publicBuffer[(int)_first * 2], publicBuffer[(int)_first * 2 + 1]);
        Vector2 second = new Vector2(publicBuffer[(int)_second * 2], publicBuffer[(int)_second * 2 + 1]);
        Vector2 middle = new Vector2(publicBuffer[(int)_middle * 2], publicBuffer[(int)_middle * 2 + 1]);
        first -= middle;
        second -= middle;

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
        // ��            �Ӹ� - �� - ������ ���
        _angles[0] = GetRadiabAngle(Body.Head, Body.RShoulder, Body.Neck);

        // ���Ȳ�ġ         �޼� - ���Ȳ�ġ - �޾��      
        _angles[1] = GetRadiabAngle(Body.LWrist, Body.LShoulder, Body.LElbow);

        // �޾��          �� - �޾�� - ���Ȳ�ġ
        _angles[2] = GetRadiabAngle(Body.Neck, Body.LElbow, Body.LShoulder);

        // �����Ȳ�ġ        ������ - �����Ȳ�ġ - �������
        _angles[3] = GetRadiabAngle(Body.RWrist, Body.RShoulder, Body.RElbow);

        // �������            �� - ������� - �����Ȳ�ġ
        _angles[4] = GetRadiabAngle(Body.Neck, Body.RElbow, Body.RShoulder);

        // �޹���              �ް�� - �޹��� - �޹�
        _angles[5] = GetRadiabAngle(Body.LHip, Body.LAnkle, Body.LKnee);

        // �ް��              ������� - �ް�� - �޹���
        _angles[6] = GetRadiabAngle(Body.RHip, Body.LKnee, Body.LHip);

        // ��������             ������� - �������� - ������
        _angles[7] = GetRadiabAngle(Body.RHip, Body.RAnkle, Body.RKnee);

        // �������             �ް�� - ������� - ��������
        _angles[8] = GetRadiabAngle(Body.LHip, Body.RKnee, Body.RHip);
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
    }



    // ���� �����忡�� ������ ����
    private void ServerThread_Read()
    {
        Debug.Log("Waiting....");
        pipeServer.WaitForConnection();
        Debug.Log("Client has connected!");
    }

    // ����� �������� �ʴ� ����Ʈ �������, �� ���� �´� ��ºκ� ó���ϱ�
    private void ControllRenderPoints(int _offsetX, int _offsetY)
    {
        // ���� ���� � ������ ����
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
                // NaN �� �� �ش� ����Ʈ ������� �ʱ�
                if (publicBuffer[2 * i] == Single.NaN || publicBuffer[2 * i + 1] == Single.NaN)
                {
                    UserBody[i].SetActive(false);
                }
                else
                {
                    // test ���̸� ���� ���
                    if (isTesting)
                    {
                        UserBody[i].SetActive(true);
                        //Debug.Log("isTesting");
                    }
                    // test ���� �ƴϸ� �ո� ���
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
            // 3���̻� ���� ���� �� ȭ�鿡 ǥ��
            connectionLostTime += Time.deltaTime;
            if (connectionLostTime >= 1.0f)
            {
                connectionLost.SetActive(true);
            }

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
                Debug.Log("Close Connection");
            }

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
            // NaN�� ��� �� ���� �ʱ�
            if (userData < 0 || fileData < 0)
                continue;
            // �������� result�� ���ϱ�
            result += Math.Abs(userData - fileData) / 180f;
            count++;
        }
        return 100 - (result/count);
    }

}
