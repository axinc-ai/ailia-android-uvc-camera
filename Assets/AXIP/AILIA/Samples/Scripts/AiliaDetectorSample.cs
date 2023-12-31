/* AILIA Unity Plugin Detector Sample */
/* Copyright 2018-2019 AXELL CORPORATION */

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.UI;

public class AiliaDetectorSample : AiliaRenderer {
    //Settings
    public bool gpu_mode = false;
    public int camera_id = 0;

    //Result
    public RawImage raw_image=null;
    public Text label_text=null;
    public Text mode_text=null;

    //Preview
    private Texture2D preview_texture=null;

    //AILIA
    private AiliaDetectorModel ailia_detector=new AiliaDetectorModel();

    private AiliaCamera ailia_camera=new AiliaCamera();
    #if UNITY_ANDROID
    private AiliaDownload ailia_download=new AiliaDownload();
    #endif

    //UVC Texture
    public RawImage uvc_texture = null;

    private void CreateAiliaDetector(){
        string asset_path = Application.streamingAssetsPath+"/AILIA";
        uint category_n=80;
        if(gpu_mode){
            ailia_detector.Environment(Ailia.AILIA_ENVIRONMENT_TYPE_GPU);
        }
        ailia_detector.Settings (AiliaFormat.AILIA_NETWORK_IMAGE_FORMAT_BGR, AiliaFormat.AILIA_NETWORK_IMAGE_CHANNEL_FIRST, AiliaFormat.AILIA_NETWORK_IMAGE_RANGE_UNSIGNED_INT8, AiliaDetector.AILIA_DETECTOR_ALGORITHM_YOLOX, category_n, AiliaDetector.AILIA_DETECTOR_FLAG_NORMAL);
        #if UNITY_ANDROID
        ailia_detector.OpenMem (ailia_download.DownloadModel (asset_path + "/yolox_tiny.opt.onnx.prototxt"), ailia_download.DownloadModel (asset_path + "/yolox_tiny.opt.onnx"));
        #else
        ailia_detector.OpenFile(asset_path+"/yolox_tiny.opt.onnx.prototxt",asset_path+"/yolox_tiny.opt.onnx");
        #endif
    }

    private void DestroyAiliaDetector(){
        ailia_detector.Close();
    }

    // Use this for initialization
    void Start () {
        mode_text.text="ailia Detector";
        CreateAiliaDetector();
        if (camera_id != -1){
            ailia_camera.CreateCamera(camera_id);
        }
    }

    // Update is called once per frame
    void Update () {
        if(uvc_texture != null){
            if(uvc_texture.texture != null){
                ailia_camera.CreateUvcCamera((Texture2D)uvc_texture.texture);
            }
        }

        if(!ailia_camera.IsEnable()){
            ailia_camera.CheckIsValidCamera(Time.deltaTime);
            return;
        }

        //Clear result
        Clear();

        //Get camera image
        int tex_width = ailia_camera.GetWidth();
        int tex_height = ailia_camera.GetHeight();
        if(preview_texture==null || preview_texture.width != tex_width || preview_texture.height != tex_height){
            preview_texture = new Texture2D(tex_width,tex_height);
            raw_image.texture = preview_texture;
        }
        Color32[] camera  = ailia_camera.GetPixels32();

        //Detection
        float threshold=0.2f;
        float iou=0.25f;
        long start_time=DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        List<AiliaDetector.AILIADetectorObject> list=ailia_detector.ComputeFromImageB2T(camera,tex_width,tex_height,threshold,iou);
        long end_time=DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

        //Error
        if(list==null){
            return;
        }

        //Display detected object
        string result="";
        foreach(AiliaDetector.AILIADetectorObject obj in list){
            DisplayDetectedObject(obj,camera,tex_width,tex_height);
        }

        if(label_text!=null){
            label_text.text=result+""+(end_time-start_time)+"ms\n"+ailia_detector.EnvironmentName();
        }

        //Apply
        preview_texture.SetPixels32(camera);
        preview_texture.Apply();
    }

    private void DisplayDetectedObject(AiliaDetector.AILIADetectorObject box,Color32 [] camera,int tex_width,int tex_height){
        //Convert to pixel domain
        int x1=(int)(box.x*tex_width);
        int y1=(int)(box.y*tex_height);
        int x2=(int)((box.x+box.w)*tex_width);
        int y2=(int)((box.y+box.h)*tex_height);

        int w=(x2-x1);
        int h=(y2-y1);

        if(w<=0 || h<=0){
            return;
        }

        Color color=Color.white;
        color=Color.HSVToRGB (box.category/80.0f, 1.0f, 1.0f);
        DrawRect2D(color, x1, y1, w, h, tex_width, tex_height);

        float p=(int)(box.prob*100)/100.0f;
        string text="";
        text+=AiliaClassifierLabel.COCO_CATEGORY[box.category];
        text+=" "+p;
        int margin=4;
        DrawText(color,text,x1+margin,y1+margin,tex_width,tex_height);
    }

    void OnApplicationQuit () {
        DestroyAiliaDetector();
        ailia_camera.DestroyCamera();
    }

    void OnDestroy () {
        DestroyAiliaDetector();
        ailia_camera.DestroyCamera();
    }
}
