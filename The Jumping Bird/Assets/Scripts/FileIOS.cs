using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Playables;


// 게임플레이하는 동안 파일을 읽어오는 기능
// deltatime에 맞춰서 읽어서 sync를 맞추도록 한다.
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
    
    // 전달받은 array 작성
    void bWrite(float _deltaTime, ref float[] _array)
    {
        // deltatime 작성
        binaryWriter.Write(_deltaTime);
        Debug.Log(_deltaTime);
        byte[] lByteBuffer = new byte[_array.Length * 4];
        // 인자로 전달받은 float 배열 byte배열로 변환
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

    // array의 길이만큼 읽어온다.
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
                // time 값 읽어오기
                var1 = binaryReader.ReadSingle();
                // 유저 프레임과 시간차가 이전 프레임보다 현재 프레임이 더 작을 때 
                if (timeDiff>Mathf.Abs(_deltaTime-var1))
                {
                    // timeDiff 재설정
                    timeDiff = Mathf.Abs(_deltaTime - var1);


                    // 리턴으로 buffer에 읽어온 byte의 개수를 리턴한다.
                    int readbytelength = binaryReader.Read(byteBuffer, 0, byteBuffer.Length) ;
                    // Debug.Log(readbytelength);
                    /*
                    for (int i = 0; i < byteBuffer.Length; ++i)
                    {
                        // Debug.Log(byteBuffer[i]);
                    }
                    */
                    // float값으로 변환
                    Buffer.BlockCopy(byteBuffer, 0, floatBuffer, 0, byteBuffer.Length);
                    /*
                    for(int idx=0;idx<floatBuffer.Length;idx++)
                    {
                        Debug.Log(floatBuffer[idx]);
                    }
                    */
                }
                // 현재 프레임이 시간차가 더 커질 때
                // 이전 프레임에서 읽어온 값을 전달
                else
                {
                    // 현재 프레임 시간값 읽어올 수 있도록 이전으로 돌리기
                    fs.Seek(-1, SeekOrigin.Current);

                    // 읽어온 값 넣어주기.
                    floatResult = floatBuffer;

                    return;
                }
               
            }
            catch (EndOfStreamException e)
            {
                // 끝에 도달하면 처음으로 돌아가기
                fs.Seek(0, SeekOrigin.Begin);

                // 읽어온 값 넣어주기.
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
