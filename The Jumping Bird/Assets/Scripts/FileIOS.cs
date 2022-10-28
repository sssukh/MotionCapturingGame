using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


// �����÷����ϴ� ���� ������ �о���� ���
// deltatime�� ���缭 �о sync�� ���ߵ��� �Ѵ�.
public class FileIOS : MonoBehaviour
{
    private string fileName = "test.bin";
    private float deltaTime=0f;
    FileStream fs;
    BinaryWriter binaryWriter;
    BinaryReader binaryReader;

    float[] iTest = { 1.2f, 2.8f, 3.6f };
    byte[] bTest;

    float count = 0f;
    // Start is called before the first frame update
    void Start()
    {
        fs = new FileStream(fileName, FileMode.OpenOrCreate,FileAccess.ReadWrite);
        binaryWriter = new BinaryWriter(fs);
        binaryReader = new BinaryReader(fs);
        

        // int array�� byte array�� ��ȯ
        bTest = Array.ConvertAll<float, byte>(iTest, Convert.ToByte);
        Debug.Log(bTest.Length);
    }

    // Update is called once per frame
    void Update()
    {
        // 1�� �������� bin ���Ͽ� �ۼ��ϱ�
        deltaTime += Time.deltaTime;
        if (deltaTime < 5.0f&& deltaTime>=count)
        {
            bWrite(deltaTime);
            count+=1f;
        }

        // �ۼ��� �� ������ ���� Ŀ�� ��ġ �Ű��ֱ�
        if (deltaTime >= 5.0f)
        {
            fs.Seek(0, SeekOrigin.Begin);
            bRead();
        }
    }

    void bWrite(float _deltaTime)
    {
        binaryWriter.Write(_deltaTime);
        Debug.Log(_deltaTime + " second now");
        binaryWriter.Write(bTest, 0, bTest.Length);
        for(int i=0;i<bTest.Length;++i)
        {
            Debug.Log(bTest[i]);
        }
        Debug.Log("EndLine");
        binaryWriter.Flush();
        //binaryWriter.Dispose();
    }

    void bRead()
    {
        float var1;
        byte[] byteBuffer = new byte[bTest.Length];
        while(true)
        {
            try
            {
                var1 = binaryReader.ReadSingle();
                Debug.Log("it's " + var1 + " seconds now");

                if (var1 >= 3.0f)
                {

                    // �������� buffer�� �о�� byte�� ������ �����Ѵ�.
                    binaryReader.Read(byteBuffer, 0, bTest.Length);
                    for (int i = 0; i < byteBuffer.Length; ++i)
                    {
                        Debug.Log(byteBuffer[i]);
                    }
                }
                else
                {
                    // 
                    fs.Seek(byteBuffer.Length, SeekOrigin.Current);
                }
            }
            catch(EndOfStreamException e)
            {
                binaryReader.Close();
                break;
            }
        }
        
    }
}
