using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhotoWallManager : MonoBehaviour
{
    public static PhotoWallManager Instance;

    [Header("Photo prefab")]
    public Picture photoPrefab;

    [Space]

    public LargePhotosHandler largePhotoHandler;
    public GalleryAnimation galleryAnimation;

    public Transform baseTransform;
    public List<Picture> basePhotos = new List<Picture>();
    public List<Transform> animationCollections = new List<Transform>();
    public List<IAnimation> animations = new List<IAnimation>();
    private Dictionary<string, int> dataDict = new Dictionary<string, int>(); //key = id, values = high_prio
    private List<APIManager.Data> datas = new List<APIManager.Data>();

    private int animationIndex = 0;
    public int AnimationIndex
    {
        get { return animationIndex; }
        set { animationIndex = value; }
    }

    private bool hasInit;
    public bool HasInit
    {
        get => hasInit;
        set
        {
            hasInit = value;

            if (hasInit)
                PhotoWallEvents.OnInitializeDone?.Invoke();
        }
    }

    public bool IsPhotoWallReady { get; set; } = false;

    private APIManager api;
    private LoadImages loader;
    bool IsWaiting;

    private void Awake()
    {
        Instance = this;

        PhotoWallEvents.OnResponseGet += ManagePhotos;
        PhotoWallEvents.OnPhotoDeleted += RemovePhotos;
    }

    private void OnDestroy()
    {
        PhotoWallEvents.OnResponseGet -= ManagePhotos;
        PhotoWallEvents.OnPhotoDeleted -= RemovePhotos;
    }

    private void Start()
    {
        api = APIManager.Instance;
        loader = LoadImages.Instance;

        //set to fullscreen
        Screen.SetResolution(3360, 1350, true);

        foreach (var item in animationCollections)
        {
            IAnimation animation = item.GetComponent<IAnimation>();
            animations.Add(animation);
            animation.Hide();
            //CanvasGroup cg = item.GetComponent<CanvasGroup>();
            //cg.alpha = 0;
        }

        //HideAllAnimations();
    }

    private void HideAllAnimations()
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].Hide();
        }
    }

    public void Swap(string key = "")
    {
        if (IsWaiting) return;
        StartCoroutine(SwapAnimation(key));
    }

    protected IEnumerator SwapAnimation(string key = "")
    {
        IsWaiting = true;
        animations[animationIndex].Activate();

        animationIndex = Int32.Parse(key);
        animationIndex %= animationCollections.Count;
        yield return new WaitForSeconds(1f);

        BannerManager.instance.DeactivateBanner();
        animations[animationIndex].Activate();

        yield return new WaitForSeconds(0.5f);

        IsWaiting = false;
    }

    public void ManagePhotos()
    {
        if (api.serverResponse.status && api.serverResponse.data.Count > 0)
        {
            if(!IsPhotoWallReady)
            {
                foreach(var data in api.serverResponse.data)
                {
                    dataDict.Add(data._id, data.high_priority);
                    datas.Add(data);
                }

                loader.LoadInitPhotos(api.serverResponse.data, basePhotos, photoPrefab, baseTransform);
                hasInit = true;
            }
            else
            {
                //check if any new photo or updated data
                foreach (var data in api.serverResponse.data)
                {
                    bool hasPhoto = dataDict.ContainsKey(data._id);
                    bool hasNewData = dataDict.ContainsKey(data._id) && dataDict[data._id] != data.high_priority;

                    if (!hasPhoto)
                    {
                        dataDict.Add(data._id, data.high_priority);
                        datas.Add(data);

                        //largePhotoHandler.SetLargePhoto();
                        Picture photo = Instantiate(photoPrefab, baseTransform);
                        api.SetPhotoData(photo, data);
                        basePhotos.Add(photo);
                    }

                    if (hasNewData)
                    {
                        //update
                        dataDict[data._id] = data.high_priority;
                        PhotoWallEvents.OnDataUpdated?.Invoke(data._id, data.high_priority);
                    }
                }

                //delete if there's data that has been removed
                var filteredResults = datas.Where(data => !api.serverResponse.data.Select(i => i._id).Contains(data._id));

                if(filteredResults.Count() > 0)
                OnPhotoDeleted(filteredResults);
            }
        }
    }

    private void OnPhotoDeleted(IEnumerable<APIManager.Data> filteredData)
    {
        foreach (var data in filteredData)
        {
            datas.Remove(data);
            //print(data._id);
            PhotoWallEvents.OnPhotoDeleted?.Invoke(data._id);
        }
    }

    private void RemovePhotos(string _id)
    {
        //print(_id);
        if (dataDict.ContainsKey(_id))
        {
            dataDict.Remove(_id);
        }

        int baseIndex = basePhotos.FindIndex(pic => pic._id == _id);
        if (baseIndex != -1)
        {
            Destroy(basePhotos[baseIndex].gameObject);
            basePhotos.RemoveAt(baseIndex);
        }
    }

    public void BroadcastPhotosToAnimations()
    {
        foreach(var photo in basePhotos)
        {
            PhotoWallEvents.OnInitPhotoAdded?.Invoke(photo);
        }
    }

    private IEnumerator FinishInitialization()
    {
        yield return new WaitForSeconds(2f);

        hasInit = true;
    }
}