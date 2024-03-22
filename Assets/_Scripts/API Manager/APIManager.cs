using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Proyecto26;
using System.Collections;

public class APIManager : Configuration
{
    public static APIManager Instance;

    #region Body
    [Serializable]
    public class ServerResponse
    {
        public bool status;
        public List<Data> data;
    }
    [Serializable]
    public class Data
    {
        public string _id;
        public string title;
        public string image;
        public string image_thumb;
        public string image_smaller;
        public string image_original;
        public string description;
        public int high_priority;
        public string createdAt;
    }
    #endregion

    #region Swap Animation API (Custom Settings)

    [Serializable]
    public class APIAnimation
    {
        public bool status;
        public string message;
        public AnimationData data;
    }
    [Serializable]
    public class AnimationData
    {
        public string _id;
        public string value;
    }

    #endregion

    #region Gallery Type API (Custom Settings)

    [Serializable]
    public class APIGalleryType
    {
        public bool status;
        public string message;
        public StyleData data;
    }
    [Serializable]
    public class StyleData
    {
        public string _id;
        public string value;
    }

    #endregion

    #region Fields

    [Header("Response")]
    public ServerResponse serverResponse = new ServerResponse();
    //[SerializeField]
    //float startTime = 60f;
    [SerializeField]
    float repeatRate = 3f;
    [Header("Animation API")]
    public APIAnimation animationResponse = new APIAnimation();
    [SerializeField]
    LoadImages imageLoader;
    PhotoWallManager manager;


    #endregion

    //public static Action OnResponseGet;
    //public static Action<Picture> OnPhotoReady;

    private void Awake()
    {
        StartCoroutine(FetchPhotoWallConfiguration());
        Instance = this;
    }
    private void Start()
    {
        manager = PhotoWallManager.Instance;

        //FetchGalleryTypeData();
        FetchServerData();
        FetchBannerCaptionData();
        FetchLargePhotoDurationData();
        OverwriteFirstAnimation();

        //testing
        //FetchPhotoData();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            FetchAnimation();
        }
    }

    private void OnEnable()
    {
        PhotoWallEvents.OnPhotoWallReady += FetchPhotoData;
        PhotoWallEvents.OnPhotoWallReady += FetchAnimationData;
        PhotoWallEvents.OnPhotoWallReady += FetchLargePhotoDurationData;
        PhotoWallEvents.OnPhotoWallReady += FetchGalleryTypeData;
    }
    private void OnDisable()
    {
        PhotoWallEvents.OnPhotoWallReady -= FetchPhotoData;
        PhotoWallEvents.OnPhotoWallReady -= FetchAnimationData;
        PhotoWallEvents.OnPhotoWallReady -= FetchLargePhotoDurationData;
        PhotoWallEvents.OnPhotoWallReady -= FetchGalleryTypeData;
    }

    IEnumerator FetchPhotoWallConfiguration()
    {
        ConfigureSettings();

        yield return new WaitUntil (() => !string.IsNullOrEmpty(baseUrl));

        RestClient.Get<APIAnimation>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_CONFIGURATION).Then(
            response =>
            {
                //print(response.data.value);
                ConfigurePhotoWallFromKioskSettings(response.data.value);
            });
    }

    //Base of syncing data
    private void FetchServerData()
    {
        RestClient.Get<ServerResponse>(baseUrl + listEndpoint + galleryId + $"?show-all={all}").Then(
            response =>
            {
                serverResponse = response;
                PhotoWallEvents.OnResponseGet?.Invoke();
            });
    }

    //Fetch playing animation set on web portal
    private void FetchAnimation()
    {
        RestClient.Get<APIAnimation>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_ANIMATION).Then(
            response =>
            {
                print(response.data.value);
                //set animation
                if (manager.AnimationIndex.ToString() != response.data.value && Int32.Parse(response.data.value) <= manager.animationCollections.Count-1)
                {
                    manager.Swap(response.data.value);
                }
            });
    }

    //Fetch gallery type
    int outcome;
    private void FetchGalleryType()
    {
        RestClient.Get<APIGalleryType>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_GALLERY_TYPE).Then(
            response =>
            {
                //set gallery type
                if (int.TryParse(response.data.value, out outcome))
                {
                    if (galleryType == outcome) return;

                    galleryType = outcome;
                    manager.galleryAnimation.SwitchGridSize();
                }
            });
    }

    //Fetch banner caption
    private void FetchBannerCaption()
    {
        RestClient.Get<APIAnimation>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_BANNER_CAPTION).Then(
            response =>
            {
                //set caption
                BannerManager.instance.BannerTxt.text = response.data.value;
            });
    }

    //Fetch large photo appear duration
    int result;
    private void FetchLargePhotoAppearDuration()
    {
        RestClient.Get<APIGalleryType>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_LARGE_PICTURE_DURATION).Then(
            response =>
            {
                //set gallery type
                if (int.TryParse(response.data.value, out result))
                {
                    if (appearDuration == result) return;

                    appearDuration = result;
                    manager.largePhotoHandler.ChangeAppearDuration(result);
                    //FindObjectOfType<PhotoDataCenter>().largePhotoHandler.ChangeAppearDuration(result);
                }
            });
    }

    #region Fetch-Loop Data functions

    public void FetchPhotoData()
    {
        InvokeRepeating("FetchServerData", 1f, repeatRate);
    }
    public void FetchAnimationData()
    {
        InvokeRepeating("FetchAnimation", 1f, repeatRate);
    }
    public void FetchGalleryTypeData()
    {
        InvokeRepeating("FetchGalleryType", 0f, repeatRate);
    }
    public void FetchBannerCaptionData()
    {
        InvokeRepeating("FetchBannerCaption", 1f, repeatRate);
    }
    public void FetchLargePhotoDurationData()
    {
        InvokeRepeating("FetchLargePhotoAppearDuration", 1f, repeatRate);
    }

    #endregion


    public void OverwriteFirstAnimation()
    {
        RestClient.Get<APIAnimation>(baseUrl + customSettingEndpoint + $"?key=" + Constants.PHOTO_WALL_ANIMATION).Then(
            response =>
            {
                firstAnimation = Int32.Parse(response.data.value);
                manager.AnimationIndex = firstAnimation;
            });
    }

    //public void SetPhotoData(Picture photo, Data data, bool exception = false, bool isPhotoReady = false)
    //{
    //    if (photo.image.mainTexture != null && photo._id != string.Empty)
    //        Destroy(photo.image.mainTexture);

    //    string url = baseUrl + photoEndpoint + galleryId + "/" + (quality == 0 ? data.image : data.image_smaller);
    //    //print(url);

    //    photo._id = data._id;
    //    photo._imageEP = (quality == 0 ? data.image : data.image_smaller);
    //    photo.createdAt = data.createdAt;
    //    photo.priority = data.high_priority;

    //    RestClient.Get(new RequestHelper
    //    {
    //        Uri = url,
    //        DownloadHandler = new DownloadHandlerTexture()
    //    }).Then(res =>
    //    {
    //        Texture2D tempTexture = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
    //        var mipTexture = new Texture2D(tempTexture.width, tempTexture.height);
    //        mipTexture.SetPixels(tempTexture.GetPixels());
    //        mipTexture.wrapMode = TextureWrapMode.Clamp;
    //        mipTexture.Apply();

    //        photo.image.texture = mipTexture;
    //        //photo.image.texture.hideFlags = HideFlags.HideAndDontSave; //new
    //        photo.image.color = exception == false ? Color.white : Color.black; //exception is used on Large photo
    //        photo.hasTexture = true;

    //        if (isPhotoReady) OnPhotoReady?.Invoke(photo);

    //    }).Catch(err =>
    //    {
    //        Debug.LogWarning($"{err.Message} : {data._id}");
    //    });
    //}

    public void SetPhotoData(Picture photo, Data data, bool isPhotoReady = false)
    {
        if (photo.image.mainTexture != null && photo._id != string.Empty)
            Destroy(photo.image.mainTexture);

        //string url = GetImageUrl(data);

        imageLoader.LoadPhoto(GetImageUrl(data), photo, data);

        //if(isPhotoReady)
        //StartCoroutine(DelayShowingLargePhoto(photo));
    }

    string ImageQuality(Data data)
    {
        switch(quality)
        {
            case 0: return data.image;
            case 1: return data.image_original;
            case 2: return data.image_smaller;
        }

        return string.Empty;
    }

    public string GetImageUrl(Data data)
    {
        string url = baseUrl + photoEndpoint + galleryId + "/" + ImageQuality(data);
        return url;
    }

    //urgent, if assigning photo fail after photo wall is ready
    public void SetUnsetPhotoTexture(Picture photo)
    {
        string url = baseUrl + photoEndpoint + galleryId + "/" + photo._imageEP;

        RestClient.Get(new RequestHelper
        {
            Uri = url,
            DownloadHandler = new DownloadHandlerTexture()
        }).Then(res =>
        {
            Texture2D tempTexture = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
            var mipTexture = new Texture2D(tempTexture.width, tempTexture.height);
            mipTexture.SetPixels(tempTexture.GetPixels());
            mipTexture.wrapMode = TextureWrapMode.Clamp;
            mipTexture.Apply();

            photo.image.texture = mipTexture;
            photo.image.color = Color.white;
        });
    }

    private IEnumerator DelayShowingLargePhoto(Picture photo)
    {
        yield return new WaitForSeconds(1.5f);
        PhotoWallEvents.OnPhotoReady?.Invoke(photo);
    }

    // used in animation 2, animation is not used
    public void SetPhotoData(GameObject photo, Data data)
    {
        //string url = baseUrl + photoEndpoint + galleryId + "/" + (quality == 0 ? data.image : data.image_smaller);
        //Picture tempPhoto = photo.GetComponent<Picture>();
        //Data tempData = data;
        //////print(url);

        //RestClient.Get(new RequestHelper
        //{
        //    Uri = url,
        //    DownloadHandler = new DownloadHandlerTexture()
        //}).Then(res =>
        //{
        //    Texture2D tempTexture = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
        //    var mipTexture = new Texture2D(tempTexture.width, tempTexture.height);
        //    mipTexture.SetPixels(tempTexture.GetPixels());
        //    mipTexture.wrapMode = TextureWrapMode.Clamp;
        //    mipTexture.Apply();

        //    photo.GetComponent<Picture>().Img.sprite = Sprite.Create
        //    (
        //        mipTexture,
        //        new Rect(0.0f, 0.0f, mipTexture.width, mipTexture.height), 
        //        new Vector2(0.5f, 0.5f), 
        //        100.0f
        //    );

        //    photo.GetComponent<Picture>().Img.color = Color.white;

        //}).Catch(err =>
        //{
        //    Debug.LogWarning(err.Message);
        //    if (err.Message.Contains("SSL connection"))
        //        SetPhotoData(tempPhoto, tempData);
        //});
    }
}
