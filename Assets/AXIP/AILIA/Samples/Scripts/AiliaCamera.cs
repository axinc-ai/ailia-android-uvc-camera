/* AILIA Unity Plugin Camera Sample */
/* Copyright 2018-2019 AXELL CORPORATION */

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.UI;

public class AiliaCamera {
    //Camera settings
    private int CAMERA_WIDTH = 1280;
    private int CAMERA_HEIGHT = 720;
    private int CAMERA_FPS = 30;

    //WebCamera Instance
    private WebCamTexture webcamTexture=null;
    private Texture2D uvcTexture=null;

    //Camera ID
    public void CreateCamera(int camera_id){
        DestroyCamera();
        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length==0){
            Debug.Log("Web Camera not found");
            return;
        }
        int id=camera_id % devices.Length;
        webcamTexture = new WebCamTexture(devices[id].name, CAMERA_WIDTH, CAMERA_HEIGHT, CAMERA_FPS );
        webcamTexture.Play();
    }

    public void CreateUvcCamera(Texture2D tex){
        uvcTexture = tex;
    }

    public bool IsEnable(){
        if(uvcTexture!=null){
            return true;
        }

        if(webcamTexture==null){
            return false;
        }

        //Wait until a good frame can be captured
        if(webcamTexture.width>16 && webcamTexture.height>16){
            return true;
        }else{
            return false;
        }
    }

    private float not_available_cnt = 0;

    public bool CheckIsValidCamera(float deltaTime){
        float threshold_sec = 3;
        if(not_available_cnt < threshold_sec){
            not_available_cnt = not_available_cnt + deltaTime;
            if(not_available_cnt >= threshold_sec){
                Debug.Log(webcamTexture.deviceName+" not available, please change camera_id of controller on inspector");
                return false;
            }
            return true;
        }
        return false;
    }

    public WebCamTexture GetTexture(){
        return webcamTexture;
    }

    private int GetAngle(){
        if(uvcTexture!=null){
            return 0;
        }
        return webcamTexture.videoRotationAngle;
    }

    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    public Color32[] GetPixels32(){
        Color32[] image;
        int width, height;
        if(uvcTexture!=null){
            image=duplicateTexture(uvcTexture).GetPixels32();
            width=uvcTexture.width;
            height=uvcTexture.height;
        }else{
            image=webcamTexture.GetPixels32();
            width=webcamTexture.width;
            height=webcamTexture.height;
        }

        //Crop to square
        int size=width;
        if(size>height){
            size=height;
        }
        Color32[] crop=new Color32[size*size];
        int x_offset=(width-size)/2;
        int y_offset=(height-size)/2;
        int angle=GetAngle();

        bool rotate90=(angle == 90 || angle == 270);
        bool v_flip=false;
        if(angle==90) v_flip=true;
        if(angle==180) v_flip=true;
        if(angle==270) v_flip=false;

        if (rotate90){
            for(int y=0;y<size;y++){
                int src_adr_y=(y+y_offset)*width;
                for(int x=0;x<size;x++){
                    int x2=y;
                    int y2=x;
                    if(v_flip){
                        y2=size-1-y2;
                    }
                    crop[y2*size+x2]=image[src_adr_y+(x+x_offset)];
                }
            }
        }else{
            for(int y=0;y<size;y++){
                int y2=y;
                if(v_flip){
                    y2=size-1-y2;
                }
                int dst_adr_y=y2*size;
                int src_adr_y=(y+y_offset)*width;
                for(int x=0;x<size;x++){
                    crop[dst_adr_y+x]=image[src_adr_y+(x+x_offset)];
                }
            }
        }
        return crop;
    }

    public int GetWidth(){
        int width, height;
        if(uvcTexture!=null){
            width=uvcTexture.width;
            height=uvcTexture.height;
        }else{
            width=webcamTexture.width;
            height=webcamTexture.height;
        }
        if(height>width){
            return width;
        }
        return height;
    }

    public int GetHeight(){
        int width, height;
        if(uvcTexture!=null){
            width=uvcTexture.width;
            height=uvcTexture.height;
        }else{
            width=webcamTexture.width;
            height=webcamTexture.height;
        }
        if(height>width){
            return width;
        }
        return height;
    }

    public void DestroyCamera(){
        if(webcamTexture!=null){
            webcamTexture.Stop();
            webcamTexture=null;
        }
    }
}
