using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GalleryAnimation : Animation, IAnimation
{
    #region Fields
    [Header("Basics")]
    [SerializeField]
    bool isPlaying;
    [SerializeField]
    GridLayoutGroup photoContainer;
    [SerializeField]
    Transform viewPort;
    [SerializeField]
    CanvasGroup canvasGroup;
    [SerializeField]
    bool isGalleryRandom;
    [SerializeField]
    RectTransform tempPhotoContainer;
    #endregion

    #region Properties
    //public bool IsPlaying { get => isPlaying; set => isPlaying = value; }

    #endregion

    #region Collections
    public List<Transform> gallery = new List<Transform>();

    Dictionary<string, Picture> pictures = new Dictionary<string, Picture>(); // string is to store id
    public Dictionary<string, Picture> Pictures { get => pictures; }

    #endregion

    #region Private Fields

    //protected APIManager api;
    //protected PhotoWallManager manager;

    private int maxPageSize = 16;
    private Vector2 cellSize;
    private Vector2 pagePosition;
    private Vector2 pageAnchorPosition;
    private int constraintCount;

    private int lastGalleryPageRandom;
    private int newGalleryPageRandom;
    private System.Random rand;
    private int pageIndex = 0;
    private int pageNamingIndex = 0;    
    private IEnumerator switchGalleryCoroutine;
    private bool onSwitchGrid;

    #endregion


    private void Awake()
    {
        PhotoWallEvents.OnInitPhotoAdded += ProcessImages;
        PhotoWallEvents.OnPhotoLoaded += ProcessImages;
        PhotoWallEvents.OnInitializeDone += DisplayInactiveGridPhotos;
        PhotoWallEvents.OnLargePhotoShow += DisplayInactiveGridPhotos;
        PhotoWallEvents.OnPhotoDeleted += RemoveImage;
    }

    private void OnDestroy()
    {
        PhotoWallEvents.OnInitPhotoAdded -= ProcessImages;
        PhotoWallEvents.OnPhotoLoaded -= ProcessImages;
        PhotoWallEvents.OnInitializeDone -= DisplayInactiveGridPhotos;
        PhotoWallEvents.OnLargePhotoShow -= DisplayInactiveGridPhotos;
        PhotoWallEvents.OnPhotoDeleted -= RemoveImage;
    }

    private void Start()
    {
        api = APIManager.Instance;
        manager = PhotoWallManager.Instance;

        ManageGalleryContainers();
    }

    public void Activate()
    {
        isPlaying = !isPlaying;

        switch (isPlaying)
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

    private void Initialize()
    {
        maxPageSize = api.galleryType switch
        {
            0 => Constants.MAXSIZE_4X2,
            1 => Constants.MAXSIZE_4X3, // 4x3 no banner
            2 => Constants.MAXSIZE_4X3,
            3 => Constants.MAXSIZE_4X4,
            4 => Constants.MAXSIZE_5X3,
            5 => Constants.MAXSIZE_5X4,
            6 => Constants.MAXSIZE_6X3,
            7 => Constants.MAXSIZE_6X4,
            _ => Constants.MAXSIZE_4X3
        };
        cellSize = api.galleryType switch
        {
            0 => Constants.SIZE_4X2,
            1 => Constants.SIZE_4X3, 
            2 => Constants.SIZE_4X3_BANNER, // 4x3 banner
            3 => Constants.SIZE_4X4,
            4 => Constants.SIZE_5X3,
            5 => Constants.SIZE_5X4,
            6 => Constants.SIZE_6X3,
            7 => Constants.SIZE_6X4,
            _ => Constants.SIZE_4X3
        };
        constraintCount = api.galleryType switch
        {
            0 => Constants.CONSTRAINT_4,
            1 => Constants.CONSTRAINT_4, // 4x3 no banner
            2 => Constants.CONSTRAINT_4,
            3 => Constants.CONSTRAINT_4,
            4 => Constants.CONSTRAINT_5,
            5 => Constants.CONSTRAINT_5,
            6 => Constants.CONSTRAINT_6,
            7 => Constants.CONSTRAINT_6,
            _ => Constants.CONSTRAINT_4,
        };
        pagePosition = api.galleryType switch
        {
            0 => Constants.POSITION_4X2,
            2 => Constants.POSITION_4X3_BANNER, // 4x3 banner
            4 => Constants.POSITION_5X3,
            7 => Constants.POSITION_6x4,
            _ => Constants.POSITION_MID
        };
        pageAnchorPosition = api.galleryType switch
        {
            0 => Constants.ANCHORS_WHEN_BANNER,
            2 => Constants.ANCHORS_WHEN_BANNER, // 4x3 banner
            4 => Constants.ANCHORS_WHEN_BANNER,
            7 => Constants.ANCHORS_WHEN_BANNER,
            _ => Constants.ANCHORS_WHEN_NORMAL
        };
    }

    public void StartAnimation()
    {
        if (manager.IsPhotoWallReady)
        BannerManager.instance.SetBanner(api.galleryType);

        switchGalleryCoroutine = SwitchGalleryPage(api.galleryRepeatRate);

        foreach (Transform page in viewPort)
        {
            page.GetComponent<GridLayoutGroup>().cellSize = cellSize;
            page.GetComponent<GridLayoutGroup>().constraintCount = constraintCount;
            page.GetComponent<RectTransform>().anchorMin = pageAnchorPosition;
            page.GetComponent<RectTransform>().anchorMax = pageAnchorPosition;
            page.GetComponent<RectTransform>().anchoredPosition = pagePosition;
        }

        StartCoroutine(switchGalleryCoroutine);
    }

    public void StopAnimation()
    {
        StopCoroutine(switchGalleryCoroutine);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space)) //TEST
    //    {
    //        //Transform[] array = gallery[0].GetComponentsInChildren<Transform>();
    //        //RandomizePosition(array);
    //    }
    //}

    //private void ProcessImages(Picture photo)
    //{
    //    ManageGalleryContainers();
    //    SpawnSinglePhoto(photo);
    //}

    public override void ProcessImages(Picture photo)
    {
        ManageGalleryContainers();
        SpawnSinglePhoto(photo);
    }

    public override void RemoveImage(string id)
    {
        StartCoroutine(RemoveImageWait(id));
    }

    private IEnumerator RemoveImageWait(string id)
    {
        yield return new WaitForSeconds(1.5f);        

        if (pictures.ContainsKey(id))
        {
            Destroy(pictures[id].gameObject);
            pictures.Remove(id);
            //print("removed " + id);
        }
    }

    private void SpawnSinglePhoto(Picture photo)
    {
        //Check data priority, add priorities to new List 
        //redirect single photo to spacy continer
        Transform availablePage = !manager.IsPhotoWallReady ? 
                                gallery.Find(x => x.childCount < maxPageSize) :
                                gallery.Find(x => x.childCount < maxPageSize && x != gallery[pageIndex]);

        Picture _photo = Instantiate(photo, availablePage);

        pictures.Add(_photo._id, _photo);
    }

    private void SpawnSinglePhoto(APIManager.Data data)
    {
        //Check data priority, add priorities to new List 
        //redirect single photo to spacy continer
        Transform availablePage = !manager.IsPhotoWallReady ?
                                gallery.Find(x => x.childCount < maxPageSize) :
                                gallery.Find(x => x.childCount < maxPageSize && x != gallery[pageIndex]);

        Picture photo = Instantiate(manager.photoPrefab, availablePage);
        api.SetPhotoData(photo, data);

        pictures.Add(data._id, photo);
        photo.image.color = Color.black;
    }

    public void DisplayInactiveGridPhotos()
    {
        if (pictures.Count < 1) return;

        foreach (var pict in pictures)
            if (!pict.Value.gameObject.activeInHierarchy && pict.Value.image.texture != null)
            {
                pict.Value.image.color = Color.white;
                pict.Value.gameObject.SetActive(true);
            }
    }

    public void ShowTakenPhotoBatch(string photoId)
    {
        Picture result;

        if (manager.HasInit)
            StopAnimation();

        if (pictures.TryGetValue(photoId, out result))
        {
            gallery[pageIndex].GetChild(gallery.Count - 1).SetParent(result.transform.parent);
            result.transform.SetParent(gallery[pageIndex]);

            switch (maxPageSize)
            {
                case 15: result.transform.SetSiblingIndex(7); break;
                case 24: result.transform.SetSiblingIndex(8); break;
                default: result.transform.SetAsFirstSibling(); break;
            }

            StartAnimation();
        }
    }

    private void ManageGalleryContainers()
    {
        //add one container to initiate
        if (gallery.Count < 1)
        {
            AddNewPage();
            return;
        }

        //if all gallery containers have reached max size, will instantiate new
        if (!manager.HasInit || onSwitchGrid)
        {
            if (gallery[gallery.Count - 1].childCount == maxPageSize)
            {
                AddNewPage();
            }
        }
        else
        {
            if (pictures.Count == maxPageSize * gallery.Count)
            {
                AddNewPage();
            }
        }
    }

    GridLayoutGroup AddNewPage()
    {
        var page = Instantiate(photoContainer, viewPort);
        var rect = page.GetComponent<RectTransform>();
        page.name = $"Page {pageNamingIndex}";
        page.cellSize = cellSize;
        page.constraintCount = constraintCount;
        rect.anchorMin = pageAnchorPosition;
        rect.anchorMax = pageAnchorPosition;
        rect.anchoredPosition = pagePosition;
        ++pageNamingIndex;
        gallery.Add(page.transform);
        page.gameObject.SetActive(false);

        return page;
    }

    public void SwitchGridSize()
    {
        onSwitchGrid = true;

        Initialize();
        ManageGalleryContainers();

        StartCoroutine(OnGridSizeChanged());
        StopAnimation();
    }

    private IEnumerator OnGridSizeChanged()
    {
        //print("OnGridSizeChanged");
        //move photo to a temp container

        canvasGroup.DOFade(0f, 0.3f);
        yield return new WaitUntil(() => canvasGroup.alpha == 0f);
        
        foreach (var photo in pictures.Values)
            photo.transform.SetParent(tempPhotoContainer);

        foreach (var page in gallery)
            Destroy(page.gameObject);
        gallery.Clear();

        pageNamingIndex = 0;
        pageIndex = 0;

        yield return new WaitForSeconds(0.5f);
        
        while (tempPhotoContainer.childCount > 0)
        {
            ManageGalleryContainers();

            foreach (Transform page in gallery)
            {
                if (page.childCount == maxPageSize) continue;
                tempPhotoContainer.GetChild(0).SetParent(page);
            }
        }

        foreach (Transform page in gallery)
            page.gameObject.SetActive(false);

        if (!manager.HasInit || onSwitchGrid)
            gallery[0].gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        if(isPlaying)
            canvasGroup.DOFade(1f, 0.3f);

        onSwitchGrid = false;

        StartAnimation();
    }

    #region Random functions

    public static void Randomize<T>(T[] items)
    {
        System.Random rand = new System.Random();

        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < items.Length - 1; i++)
        {
            int j = rand.Next(i, items.Length);
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
    }

    public static void RandomizePosition(Transform[] page)
    {
        System.Random rand = new System.Random();

        for(int i = 0; i < page.Length - 1; i++)
        {
            int r = rand.Next(i, page.Length);
            int temp = page[i].GetSiblingIndex();
            page[i].SetSiblingIndex(page[r].GetSiblingIndex());
            page[r].SetSiblingIndex(temp);
        }
    }

    #endregion

    protected int NewGeneratedIndex()
    {
        switch(isGalleryRandom)
        {
            case true:
                rand = new System.Random();

                newGalleryPageRandom = rand.Next(0, gallery.Count);

                while (newGalleryPageRandom == lastGalleryPageRandom)
                {
                    newGalleryPageRandom = rand.Next(0, gallery.Count);
                    if (newGalleryPageRandom != lastGalleryPageRandom) break;
                }
                lastGalleryPageRandom = newGalleryPageRandom;

                return newGalleryPageRandom;
            case false:
                pageIndex = (pageIndex + 1) % (gallery.Count == 0 ? 1 : gallery.Count);
                return pageIndex;
        }
    }

    protected IEnumerator SwitchGalleryPage(float repeatRate)
    {
        if (!isPlaying) yield break;

        yield return new WaitForSeconds(repeatRate);

        if (gallery.Count == 1) yield break;

        int newpageNamingIndex = NewGeneratedIndex();

        //switch gallery page, set every page off
        foreach (var page in gallery)
            page.gameObject.SetActive(false);

        //if targeted page photos are less than maxPageSize, take it from other pages
        Transform _pageToDonate = gallery.Find(x => x.childCount == maxPageSize);
        Transform _pageToReceive = gallery.Find(x => x.childCount < maxPageSize);

        while (gallery[newpageNamingIndex].childCount < maxPageSize)
        {
            _pageToDonate.GetChild(_pageToDonate.childCount - 1).SetParent(gallery[newpageNamingIndex]);
        }

        while (gallery[newpageNamingIndex].childCount > maxPageSize)
        {
            gallery[newpageNamingIndex].GetChild(gallery[newpageNamingIndex].childCount - 1).SetParent(_pageToReceive);
        }

        //randomize photos
        Transform[] collection = gallery[newpageNamingIndex].GetComponentsInChildren<Transform>();
        RandomizePosition(collection);
        gallery[newpageNamingIndex].gameObject.SetActive(true);

        //repeat the process in given time span
        switchGalleryCoroutine = SwitchGalleryPage(repeatRate);
        StartCoroutine(switchGalleryCoroutine);
    }
}
