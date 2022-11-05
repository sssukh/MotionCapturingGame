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
public class FileIOS : MonoBehaviour
{
    private string fileName = "test.bin";
    
    FileStream fs;
    BinaryWriter binaryWriter;
    BinaryReader binaryReader;

    // Start is called before the first frame update
    void Start()
    {
        // fs = new FileStream(fileName, FileMode.OpenOrCreate,FileAccess.ReadWrite);
        binaryWriter = new BinaryWriter(fs);
        binaryReader = new BinaryReader(fs);
    }

    // Update is called once per frame
    
    // ���޹��� array �ۼ�
    void bWrite(float _deltaTime, ref float[] _array)
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
    void bRead(int _arrayLength, float _deltaTime, ref float[] floatResult)
    {
        float var1;
        byte[] byteBuffer = new byte[_arrayLength*4];
        float[] floatBuffer = new float[byteBuffer.Length / 4];
        float timeDiff = 100;
        while (true)
        {
            
            try
            {
                // time �� �о����
                var1 = binaryReader.ReadSingle();
                // ���� �����Ӱ� �ð����� ���� �����Ӻ��� ���� �������� �� ���� �� 
                if (timeDiff>Mathf.Abs(_deltaTime-var1))
                {
                    // timeDiff �缳��
                    timeDiff = Mathf.Abs(_deltaTime - var1);


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
                    fs.Seek(-1, SeekOrigin.Current);

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

                break;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                break;
            }
        }
    }
    void setFileName(string _filename)
    {
        fileName = _filename;
        fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }
   
    void createWriter()
    {
        binaryWriter = new BinaryWriter(fs);
    }
    void createReader()
    {
        binaryReader = new BinaryReader(fs);
    }
    void deleteWriter()
    {
        binaryWriter.Close();
    }
    void deleteReader()
    {
        binaryReader.Close();
    }
}
