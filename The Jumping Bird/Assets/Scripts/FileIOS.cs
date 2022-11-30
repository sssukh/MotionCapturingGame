using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Playables;





// �����÷����ϴ� ���� ������ �о���� ���
// deltatime�� ���缭 �о sync�� ���ߵ��� �Ѵ�.
public class Fileios : MonoBehaviour
{
    // E:\Unity\Projects\My project\Assets\Contents\SoHappy
    public string fileName = "test.bin";
    private string prePath = "Assets/Contents";
    private string happy = "/SoHappy/SoHappy.bin";
    private string heroes = "/HeroesTonight/HeroesTonight.bin";
    // Assets/Contents/SoHappy/Raven & Kreyn - So Happy[NCS Release].mp3
    // Assets/Contents/SoHappy/SoHappy.jpg
    // Assets/Contents/HeroesTonight/Janji - Heroes Tonight (feat. Johnning) [NCS Release].mp3
    // Assets/Contents/HeroesTonight/HeroesTonightCover.jpg

    FileStream fs;
    BinaryWriter binaryWriter;
    BinaryReader binaryReader;

    // Start is called before the first frame update
    void Start()
    {
        // fs = new FileStream(fileName, FileMode.OpenOrCreate,FileAccess.ReadWrite);
        
    }
    public void GetName()
    {
        Debug.LogError(fs.Name);
    }
    // Update is called once per frame
    
    // ���޹��� array �ۼ�
    public void bWrite(float _deltaTime, ref float[] _array)
    {
        // deltatime �ۼ�
        binaryWriter.Write(_deltaTime);
        Debug.Log(_deltaTime);
        byte[] lByteBuffer = new byte[_array.Length * 4];
        // ���ڷ� ���޹��� float �迭 byte�迭�� ��ȯ
        Buffer.BlockCopy(_array, 0, lByteBuffer, 0, lByteBuffer.Length);
        binaryWriter.Write(lByteBuffer, 0, lByteBuffer.Length);
        /*
        for(int i=0;i< lByteBuffer.Length; ++i)
        {
            Debug.Log(lByteBuffer[i]);
        }
        Debug.Log("EndLine");
        */
        binaryWriter.Flush();
        //binaryWriter.Dispose();
    }

    // array�� ���̸�ŭ �о�´�.
    public void bRead(int _arrayLength, float _deltaTime, ref float[] floatResult, float _timeDiff)
    {
        Debug.LogError("Binary Read called");
        float var1;
        byte[] byteBuffer = new byte[_arrayLength*4];
        float[] floatBuffer = new float[byteBuffer.Length / 4];
        // �ð� ������
        //_timeDiff = 100;
        while (true)
        {
            Debug.LogError("Binary Read while form");
            try
            {
                // time �� �о����
                var1 = binaryReader.ReadSingle();
                // ���� �����Ӱ� �ð����� ���� �����Ӻ��� ���� �������� �� ���� �� 
                if (_timeDiff > Mathf.Abs(_deltaTime-var1))
                {
                    Debug.LogError("Time diff : " + Mathf.Abs(_deltaTime - var1));
                    // timeDiff �缳��
                    _timeDiff = Mathf.Abs(_deltaTime - var1);


                    // �������� buffer�� �о�� byte�� ������ �����Ѵ�.
                    int readbytelength = binaryReader.Read(byteBuffer, 0, byteBuffer.Length) ;
                    // Debug.Log(readbytelength);
                    /*
                    for (int i = 0; i < byteBuffer.Length; ++i)
                    {
                        // Debug.Log(byteBuffer[i]);
                    }
                    */
                    // float������ ��ȯ
                    Buffer.BlockCopy(byteBuffer, 0, floatBuffer, 0, byteBuffer.Length);
                    
                    /*
                    for(int idx=0;idx<floatBuffer.Length;idx++)
                    {
                        Debug.Log(floatBuffer[idx]);
                    }
                    */
                }
                // ���� �������� �ð����� �� Ŀ�� ��
                // ���� �����ӿ��� �о�� ���� ����
                else
                {
                    // ���� ������ �ð��� �о�� �� �ֵ��� �������� ������
                    // �ð� 4byte, ������ 9�� * 4byte = 36byte
                    fs.Seek(-40, SeekOrigin.Current);
                    GetName();
                    Debug.LogError("time synchronized");
                    // �о�� �� �־��ֱ�.
                    floatResult = floatBuffer;
                    
                    return;
                }
               
            }
            catch (EndOfStreamException e)
            {
                // ���� �����ϸ� ó������ ���ư���
                fs.Seek(0, SeekOrigin.Begin);
                
                // �о�� �� �־��ֱ�.
                floatResult = floatBuffer;
                Debug.LogError(e.Message);
                break;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                break;
            }
        }
    }
    public void setFileName(string _filename)
    {
        fileName = _filename;
        //fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

    }

    public void createWriter()
    {
        binaryWriter = new BinaryWriter(fs);
    }
    public void createReader()
    {
        binaryReader = new BinaryReader(fs);
        fs.Seek(0, SeekOrigin.Begin);
    }
    public void deleteWriter()
    {
        binaryWriter.Close();
    }
    public void deleteReader()
    {
        binaryReader.Close();
    }
}
