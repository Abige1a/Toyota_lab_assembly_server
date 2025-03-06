using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraScreenshot : MonoBehaviour
{
    public CameraStreamer streamer;
    public RawImage image;
    public SendGPTService service;

    private Texture2D texture;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetImage()
    {
        if(streamer.textureData != null)
        {
            service.textureData = streamer.textureData;
            service.textureHeight = streamer.textureHeight;
            service.textureWidth = streamer.textureWidth;
            if(texture != null)
            {
                Destroy(texture);
            }
            texture = new Texture2D(streamer.textureWidth, streamer.textureHeight);
            texture.LoadImage(streamer.textureData);
            image.texture = texture;
            //Destroy(temp);
        }
    }
}
