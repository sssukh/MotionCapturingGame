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
    // �߰�
    // ������ Ʈ��ŷ�� ���� ������ �� ������Ʈ
    private GameObject[] UserBody = new GameObject[14];
    // �������� ������ 3�� �̻� �������� ��� ȭ�鿡 ��Ÿ�� ��ȣ �� �ð� ����
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    /// <summary>
    /// 
    /// </summary>
    // ������ Ʈ��ŷ�ؼ� �޾ƿ� ����Ʈ ������ ������ �迭
    private int[] publicBuffer;
  
    // ������
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;


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

        /*
        for (Body i = Body.Head; i < Body.End; ++i)
        {
            UserBody[(int)i].GetComponent<SpriteRenderer>().color = Color.green;
        }
        */


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
                if (publicBuffer[2 * i] == -1 || publicBuffer[2 * i + 1] == -1)
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

}
