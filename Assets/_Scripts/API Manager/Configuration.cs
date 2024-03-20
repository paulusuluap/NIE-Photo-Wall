using System;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions.Must;

public class Configuration : MonoBehaviour
{
    #region Configuration Body

    [Serializable]
    public class PhotoWallConfig
    {
        public string baseUrl;
        public string listEndpoint;
        public string photoEndpoint;
        public string gallery_id;
        public int maxPhotos;
        public Gallery galleryConfig;
        public Scroll scrollAnimationConfig;
    }

    [Serializable]
    public class Gallery
    {
        public float repeatRate;
        public int quality;
    }

    [Serializable]
    public class Scroll
    {
        public float topBottom_horizontalSpeed;
        public float middle_horizontalSpeed;
        public float verticalSpeed;
        public float filmStrip_middle_horizontalSpeed;
        public float filmStrip_topBottom_horizontalSpeed;
        public float filmStrip_verticalSpeed = 120f;
    }

    #endregion

    #region APIs
    [Header("API")]
    public string baseUrl = "https://photo-kiosk.fxwebapps.com";
    public string listEndpoint = "/api/public-gallery/photos/";
    public string customSettingEndpoint = "/api/public/custom-setting";
    public string photoEndpoint = "/upload/photo/";
    public string galleryId;
    public string all = "1";
    [Header("Gallery")]
    public float galleryRepeatRate;
    [Header("Vertical Animation Speed")]
    public float verticalSpeed;
    [Header("Horizontal Animation Speed")]
    public float horizontalSpeed;
    [Header("Animation to show first")]
    public int firstAnimation;
    [Header("Gallery dimension type")]
    public int galleryType = -1;
    [Header("Photo Quality")]
    public int quality;
    [Header("Large Photo appear duration")]
    public int appearDuration = -1;
    [Header("Max Photos to show, -1 is showing all")]
    public int maxPhotos = -1;

    #endregion

    [Header("Config")]
    PhotoWallConfig photoWallConfig = new PhotoWallConfig();

    private void Awake()
    {
        //ConfigureSettings();
        //WriteJsonConfiguration();
    }

    public void ConfigureSettings()
    {
        string configFileTxt = File.ReadAllText(Application.streamingAssetsPath + "/config.json");
        photoWallConfig = JsonUtility.FromJson<PhotoWallConfig>(configFileTxt);

        baseUrl = photoWallConfig.baseUrl;
        listEndpoint = photoWallConfig.listEndpoint;
        photoEndpoint = photoWallConfig.photoEndpoint;
        galleryId = photoWallConfig.gallery_id;
        maxPhotos = photoWallConfig.maxPhotos;
        galleryRepeatRate = photoWallConfig.galleryConfig.repeatRate;
        quality = photoWallConfig.galleryConfig.quality;

        GlobalVariable.topBottom_horizontalSpeed = photoWallConfig.scrollAnimationConfig.topBottom_horizontalSpeed;
        GlobalVariable.middle_horizontalSpeed = photoWallConfig.scrollAnimationConfig.middle_horizontalSpeed;
        GlobalVariable.verticalSpeed = photoWallConfig.scrollAnimationConfig.verticalSpeed;
        GlobalVariable.filmStrip_middle_horizontalSpeed = photoWallConfig.scrollAnimationConfig.filmStrip_middle_horizontalSpeed;
        GlobalVariable.filmStrip_topBottom_horizontalSpeed = photoWallConfig.scrollAnimationConfig.filmStrip_topBottom_horizontalSpeed;
        GlobalVariable.filmStrip_verticalSpeed = photoWallConfig.scrollAnimationConfig.filmStrip_verticalSpeed;

        //print($"topBot_hor : {GlobalVariable.topBottom_horizontalSpeed} \n" +
        //    $"middle_hor : {GlobalVariable.middle_horizontalSpeed} \n" +
        //    $"verti : {GlobalVariable.verticalSpeed} \n" +
        //    $"film_middle_hor : {GlobalVariable.filmStrip_middle_horizontalSpeed} \n" +
        //    $"film_topBot_hor : {GlobalVariable.filmStrip_topBottom_horizontalSpeed} \n" +
        //    $"film_verti : {GlobalVariable.filmStrip_verticalSpeed}");
    }

    public void WriteJsonConfiguration()
    {
        photoWallConfig.baseUrl = "https://photo-kiosk.fxwebapps.com";
        photoWallConfig.listEndpoint = "/api/public-gallery/photos/";
        photoWallConfig.photoEndpoint = "/upload/gallery/";
        photoWallConfig.gallery_id = "652e11b09aff88f664db4122";
        photoWallConfig.maxPhotos = -1;
        photoWallConfig.galleryConfig.repeatRate = 5f;
        photoWallConfig.galleryConfig.quality = 1;
        photoWallConfig.scrollAnimationConfig.topBottom_horizontalSpeed = GlobalVariable.topBottom_horizontalSpeed;
        photoWallConfig.scrollAnimationConfig.middle_horizontalSpeed = GlobalVariable.middle_horizontalSpeed;
        photoWallConfig.scrollAnimationConfig.verticalSpeed = GlobalVariable.verticalSpeed;
        photoWallConfig.scrollAnimationConfig.filmStrip_middle_horizontalSpeed = GlobalVariable.filmStrip_middle_horizontalSpeed;
        photoWallConfig.scrollAnimationConfig.filmStrip_topBottom_horizontalSpeed = GlobalVariable.filmStrip_topBottom_horizontalSpeed;
        photoWallConfig.scrollAnimationConfig.filmStrip_verticalSpeed = GlobalVariable.filmStrip_verticalSpeed;

        string json = JsonUtility.ToJson(photoWallConfig, true);
        File.WriteAllText(Application.streamingAssetsPath + "/config.json", json);
    }

    //private void OnValidate()
    //{
    //    WriteJsonConfiguration();
    //}
}
