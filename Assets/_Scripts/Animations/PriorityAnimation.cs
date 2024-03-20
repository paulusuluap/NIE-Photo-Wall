using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PriorityAnimation : Animation, IAnimation
{
    //[SerializeField]
    //bool isPlaying;
    [SerializeField]
    GridLayoutGroup photoContainer;
    [SerializeField]
    Transform tempContainer;
    [SerializeField]
    Transform viewPort;
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    CanvasGroup randomCG;

    //public bool IsPlaying { get => isPlaying; set => isPlaying = value; }
    public List<Picture> superPriorities = new List<Picture>();
    public List<Picture> highPriorities = new List<Picture>();
    public List<Picture> normalPriorities = new List<Picture>();

    protected int[] superIndexes = new int[] { 1, 2, 3 };
    protected int[] highIndexes = new int[] { 6, 8 };
    protected int[] normalIndexes = new int[] { 7, 0, 4, 5, 9, 10, 11, 12, 13, 14 };

    //protected APIManager api;
    //protected PhotoWallManager manager;

    int superIndexSelector = 0;
    int highIndexSelector = 0;
    int NormalPriorityMax = 10;

    float priorityRate = 5f;
    float randomRate = 5f;
    float fadeRate = 0.25f;

    IEnumerator arrangeCoroutine;
    IEnumerator randomCoroutine;


    private void Awake()
    {
        PhotoWallEvents.OnInitPhotoAdded += ProcessImages;
        PhotoWallEvents.OnPhotoLoaded += ProcessImages;
        PhotoWallEvents.OnDataUpdated += UpdateImages;
        PhotoWallEvents.OnPhotoDeleted += RemoveImage;
    }

    private void OnDestroy()
    {
        PhotoWallEvents.OnInitPhotoAdded -= ProcessImages;
        PhotoWallEvents.OnPhotoLoaded -= ProcessImages;
        PhotoWallEvents.OnDataUpdated -= UpdateImages;
        PhotoWallEvents.OnPhotoDeleted -= RemoveImage;
    }

    private void Start()
    {
        api = APIManager.Instance;
        manager = PhotoWallManager.Instance;
    }

    public void Activate()
    {
        IsPlaying = !IsPlaying;

        switch (IsPlaying)
        {
            case true:
                canvasGroup.DOFade(1f, 0.3f);
                StartAnimation();
                break;
            case false:
                canvasGroup.DOFade(0f, 0.3f);
                StopAnimation();
                break;
        }
    }

    public void Hide()
    {
        this.canvasGroup.alpha = 0;
    }

    public void StartAnimation()
    {
        if (IsPlaying)
        BannerManager.instance.SetBanner(4);
        
        randomCG.alpha = 0;

        SortPictureLists();
        MoveNormalToTemp();
        MoveTempToMain();

        //Loop Super and High prio
        arrangeCoroutine = ArrangePhotoAndPriority();
        randomCoroutine = RandomPhoto();

        StartCoroutine(arrangeCoroutine);
        StartCoroutine(randomCoroutine);
    }

    public void StopAnimation()
    {
        superIndexSelector = 0;
        highIndexSelector = 0;

        StopAllCoroutines();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
    //        Activate();
    //    }
    //    if (Input.GetKeyDown(KeyCode.L))
    //    {
    //        SortPictureLists();
    //    }
    //}

    public override void ProcessImages(Picture photo)
    {
        SpawnPhoto(photo);
    }

    public override void RemoveImage(string id)
    {
        StopAnimation();

        //print("Priority : " + id);

        Picture superData = superPriorities.Find(x => x._id == id);
        Picture highData = highPriorities.Find(x => x._id == id);
        Picture normalData = normalPriorities.Find(x => x._id == id);

        if(superData != null)
        {
            superPriorities.Remove(superData);
            Destroy(superData.gameObject);
        }
        if(highData != null)
        {
            highPriorities.Remove(highData);
            Destroy(highData.gameObject);
        }
        if (normalData != null)
        {
            normalPriorities.Remove(normalData);
            Destroy(normalData.gameObject);
        }

        if (IsPlaying)
        StartAnimation();
    }

    private void UpdateImages(string id, int priority)
    {
        //print("UpdateImages");
        StopAnimation();

        Picture superData = superPriorities.Find(x => x._id == id);
        Picture highData = highPriorities.Find(x => x._id == id);
        Picture normalData = normalPriorities.Find(x => x._id == id);

        if (superData != null)
        {
            superData.priority = priority;
            if(priority == Constants.NORMAL_PRIORITY)
            {
                normalPriorities.Add(superData);
                superPriorities.Remove(superData);
            }
            else if(priority == Constants.HIGH_PRIORITY)
            {
                highPriorities.Add(superData);
                superPriorities.Remove(superData);
            }
        }
        if (highData != null)
        {
            highData.priority = priority;
            if (priority == Constants.NORMAL_PRIORITY)
            {
                normalPriorities.Add(highData);
                highPriorities.Remove(highData);
            }
            else if (priority == Constants.SUPER_PRIORITY)
            {
                superPriorities.Add(highData);
                highPriorities.Remove(highData);
            }
        }
        if (normalData != null)
        {
            normalData.priority = priority;
            if (priority == Constants.HIGH_PRIORITY)
            {
                highPriorities.Add(normalData);
                normalPriorities.Remove(normalData);
            }
            else if (priority == Constants.SUPER_PRIORITY)
            {
                superPriorities.Add(normalData);
                normalPriorities.Remove(normalData);
            }
        }

        StartAnimation();
    }

    void SpawnPhoto(APIManager.Data data)
    {
        Picture photo = Instantiate(manager.photoPrefab, photoContainer.transform);

        switch (data.high_priority)
        {
            case Constants.HIGH_PRIORITY: //High
                highPriorities.Add(photo);
                break;
            case Constants.SUPER_PRIORITY://Super
                superPriorities.Add(photo);
                break;
            case Constants.NORMAL_PRIORITY: //Normal
                normalPriorities.Add(photo);
                break;
        }

        api.SetPhotoData(photo, data);
    }

    void SpawnPhoto(Picture photo)
    {
        Picture _photo = Instantiate(photo, photoContainer.transform);

        switch (_photo.priority)
        {
            case Constants.HIGH_PRIORITY: //High
                highPriorities.Add(_photo);
                break;
            case Constants.SUPER_PRIORITY://Super
                superPriorities.Add(_photo);
                break;
            case Constants.NORMAL_PRIORITY: //Normal
                normalPriorities.Add(_photo);
                break;
        }
    }


    public void ShowNewlyTakenPhoto()
    {
        SortPictureLists();
        Arrange();
    }

    void SortPictureLists()
    {
        if (superPriorities.Count > 0)
            superPriorities.Sort((x, y) => string.Compare(y.createdAt, x.createdAt));
        if (highPriorities.Count > 0)
            highPriorities.Sort((x, y) => string.Compare(y.createdAt, x.createdAt));
        if (normalPriorities.Count > 0)
            normalPriorities.Sort((x, y) => string.Compare(y.createdAt, x.createdAt));
    }

    void MoveNormalToTemp()
    {
        if(normalPriorities.Count > NormalPriorityMax)
        for(int i = NormalPriorityMax; i < normalPriorities.Count; i++)
            normalPriorities[i].transform.SetParent(tempContainer);
    }

    void MoveTempToMain()
    {
        foreach(var super in superPriorities)
        {
            if(super.transform.parent == tempContainer)
            {
                super.transform.SetParent(photoContainer.transform);
            }
        }

        foreach (var high in highPriorities)
        {
            if (high.transform.parent == tempContainer)
            {
                high.transform.SetParent(photoContainer.transform);
            }
        }
    }

    void Arrange()
    {
        normalPriorities[1].transform.SetSiblingIndex(0);

        superPriorities[superIndexSelector].transform.SetSiblingIndex(1);
        superIndexSelector++;
        superIndexSelector %= superPriorities.Count;

        superPriorities[superIndexSelector].transform.SetSiblingIndex(2);
        superIndexSelector++;
        superIndexSelector %= superPriorities.Count;

        superPriorities[superIndexSelector].transform.SetSiblingIndex(3);
        superIndexSelector++;
        superIndexSelector %= superPriorities.Count;

        normalPriorities[2].transform.SetSiblingIndex(4);
        normalPriorities[3].transform.SetSiblingIndex(5);

        highPriorities[highIndexSelector].transform.SetSiblingIndex(6);
        highIndexSelector++;
        highIndexSelector %= highPriorities.Count;

        normalPriorities[0].transform.SetSiblingIndex(7);

        highPriorities[highIndexSelector].transform.SetSiblingIndex(8);
        highIndexSelector++;
        highIndexSelector %= highPriorities.Count;

        normalPriorities[4].transform.SetSiblingIndex(9);
        normalPriorities[5].transform.SetSiblingIndex(10);
        normalPriorities[6].transform.SetSiblingIndex(11);
        normalPriorities[7].transform.SetSiblingIndex(12);
        normalPriorities[8].transform.SetSiblingIndex(13);
        normalPriorities[9].transform.SetSiblingIndex(14);
    }

    IEnumerator ArrangePhotoAndPriority()
    {
        Arrange();

        yield return new WaitForSeconds(priorityRate);

        arrangeCoroutine = ArrangePhotoAndPriority();
        StartCoroutine(arrangeCoroutine);
    }

    IEnumerator RandomPhoto()
    {
        //Add photo to Random Container
        System.Random rand = new System.Random();
        
        while(randomCG.transform.childCount < 2)
        {
            //Find photos from temp
            int index = rand.Next(0, tempContainer.childCount);
            tempContainer.GetChild(index).SetParent(randomCG.transform);
        }

        //Fade in
        randomCG.DOFade(1f, fadeRate);

        yield return new WaitForSeconds(randomRate);

        //Fade out
        randomCG.DOFade(0f, fadeRate);

        yield return new WaitForSeconds(fadeRate);
        //Dispose Photo from random container
        while (randomCG.transform.childCount > 0)
        {
            randomCG.transform.GetChild(0).SetParent(tempContainer.transform);
        }

        //Add photo to Random Container
        yield return new WaitForSeconds(randomRate);

        randomCoroutine = RandomPhoto();
        StartCoroutine(randomCoroutine);
    }
}
