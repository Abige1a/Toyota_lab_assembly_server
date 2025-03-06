using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStreamer : MonoBehaviour
{
    [HideInInspector] public byte[] textureData;
    [HideInInspector] public int textureWidth;
    [HideInInspector] public int textureHeight;
    private Material material;
    private Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTexture(int width, int height, byte[] data)
    {
        textureData = data;
        textureWidth = width;
        textureHeight = height;
        if(texture != null)
        {
            Destroy(texture);
        }

        texture = new Texture2D(width, height);
        

        texture.LoadImage(data);
        //for (int i = 0; i < width; i++)
        //{
        //    for (int j = 0; j < height; j++)
        //    {
        //        //Set Pixels
        //        int index = (i * width + j) * 3;
        //        if (i == 0 && j < 3)
        //        {
        //            Debug.Log(data[index] + ", " + data[index + 1] + ", " + data[index + 2]);
        //        }
        //        texture.SetPixel(i, j, new Color(data[index] / 255.0f, data[index + 1] / 255.0f, data[index + 2] / 255.0f));
        //   }
        //}
        //texture.Apply();

        GetComponent<MeshRenderer>().material.mainTexture = texture;
        //Destroy(texture);
    }
}
