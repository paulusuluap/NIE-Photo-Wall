using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class HorizontalAnimation : Animation, IAnimation
{
    public Enums.ScrollType scrollType;

    //[SerializeField]
    //bool isPlaying;
    //public bool IsPlaying
    //{
    //    get => isPlaying;
    //}

    //protected APIManager api;
    //protected PhotoWallManager manager;

    int sequenceIndex = 1;
    int maxSequence = 3;
    bool IsAnimationReady;

    [Header("Etc.")]
    public float speed = 120f;
    public float startTime = 2f;
    public float photoWidth = 655.19f;
    public float photoHeight = 368.54f;
    public float direction = 1f;
    public int maxMidPhotos = 15;
    public Transform content;
    public CanvasGroup canvasGroup;
    public ContainerSequence[] sequences;

    public List<RectTransform> clones = new List<RectTransform>();
    public List<Picture> superPriorities = new List<Picture>();
    public List<Picture> highPriorities = new List<Picture>();
    public List<Picture> normalPriorities = new List<Picture>();

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
        if(scrollType == Enums.ScrollType.Normal)
        BannerManager.instance.SetBannerByAxis(Enums.MovementAxis.Horizontal);

        Invoke("StartPhotoAnimation", startTime);
    }

    public void StopAnimation()
    {
        IsAnimationReady = false;
        foreach (var clone in clones)
            Destroy(clone.gameObject);
        clones.Clear();

        ResetPosition(sequences[0].GetComponent<RectTransform>(), false);
        ResetPosition(sequences[1].GetComponent<RectTransform>(), true);
        ResetPosition(sequences[2].GetComponent<RectTransform>(), false);

        StopMonitorClone();
        StopReallignSlides();
        CancelInvoke("StartPhotoAnimation");
    }

    //private void ProcessImages(APIManager.Data data)
    //{
    //    sequences[sequenceIndex].SpawnPicture(manager, api, data, photoWidth, photoHeight);

    //    sequenceIndex++;
    //    sequenceIndex %= maxSequence;
    //}

    public override void ProcessImages(Picture photo)
    {
        //Old
        //sequences[sequenceIndex].SpawnPicture(photos, photo, photoWidth, photoHeight);

        //sequenceIndex++;
        //sequenceIndex %= maxSequence;

        switch (photo.priority)
        {
            case Constants.NORMAL_PRIORITY:
                if(!IsAnimationReady)
                {
                    if(sequences[1].transform.childCount < maxMidPhotos)
                    {
                        sequences[1].SpawnPicture(normalPriorities, photo, photoWidth, photoHeight);
                    }
                    else
                    {
                        sequences[2].SpawnPicture(normalPriorities, photo, photoWidth, photoHeight);
                    }
                }
                else
                {
                    sequences[1].SpawnPicture(normalPriorities, photo, photoWidth, photoHeight);
                }

                //sequences[sequenceIndex].SpawnPicture(normalPriorities, photo, photoWidth, photoHeight);
                //sequenceIndex++;

                //if (sequenceIndex == maxSequence)
                //    sequenceIndex = 1;
                break;
            default:
                var photoList = photo.priority == Constants.HIGH_PRIORITY ? highPriorities : superPriorities;
                sequences[0].SpawnPicture(photoList, photo, photoWidth, photoHeight);
                Arrange();
                break;
        }
    }

    private void UpdateImages(string id, int priority)
    {
        //print("UpdateImages");

        Picture superData = superPriorities.Find(x => x._id == id);
        Picture highData = highPriorities.Find(x => x._id == id);
        Picture normalData = normalPriorities.Find(x => x._id == id);

        if (superData != null)
        {
            superData.priority = priority;
            if (priority == Constants.NORMAL_PRIORITY)
            {
                normalPriorities.Add(superData);
                superPriorities.Remove(superData);
            }
            else if (priority == Constants.HIGH_PRIORITY)
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

        for(int i = 0; i < superPriorities.Count; i++)
        {
            if (superPriorities[i].transform.parent != sequences[0].transform)
            {
                superPriorities[i].transform.SetParent(sequences[0].transform);
            }
        }

        for (int i = 0; i < highPriorities.Count; i++)
        {
            if (highPriorities[i].transform.parent != sequences[0].transform)
            {
                highPriorities[i].transform.SetParent(sequences[0].transform);
            }
        }

        for (int i = 0; i < normalPriorities.Count; i++)
        {
            if (normalPriorities[i].transform.parent == sequences[0].transform)
            {
                normalPriorities[i].transform.SetParent(sequences[sequenceIndex].transform);
                sequenceIndex++;

                if (sequenceIndex == maxSequence)
                    sequenceIndex = 1;
            }
        }

        if (!IsPlaying) return;

        Arrange();
        StartCoroutine(RearrangeClones());
    }

    public override void RemoveImage(string id)
    {
        Picture superData = superPriorities.Find(x => x._id == id);
        Picture highData = highPriorities.Find(x => x._id == id);
        Picture normalData = normalPriorities.Find(x => x._id == id);

        if (superData != null)
        {
            superPriorities.Remove(superData);
            Destroy(superData.gameObject);
        }
        if (highData != null)
        {
            highPriorities.Remove(highData);
            Destroy(highData.gameObject);
        }
        if (normalData != null)
        {
            normalPriorities.Remove(normalData);
            Destroy(normalData.gameObject);
        }

        if (!IsPlaying) return;

        Arrange();
        StartCoroutine(RearrangeClones());
    }

    protected void StartPhotoAnimation()
    {
        StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        Arrange();

        Initialize(sequences[0], false);
        Initialize(sequences[1], true);
        Initialize(sequences[2], false);

        yield return new WaitForSeconds(startTime);

        //Monitor clone contents
        StartMonitorClone();
        StartReallignSlides();
        IsAnimationReady = true;
    }

    private void Initialize(ContainerSequence sequence, bool isMovingRight)
    {
        RectTransform original = sequence.GetComponent<RectTransform>();

        var clone = Instantiate(original, content);
        clone.anchorMax = original.anchorMax;
        clone.anchorMin = original.anchorMin;
        sequence.clone = clone;

        clones.Add(clone);

        if (isMovingRight)
        {
            original.anchoredPosition = new Vector2(-Screen.width, original.anchoredPosition.y);
            clone.anchoredPosition = new Vector2(-Screen.width - original.rect.width, original.anchoredPosition.y);
        }
        else
        {
            original.anchoredPosition = new Vector2(Screen.width, original.anchoredPosition.y);
            clone.anchoredPosition = new Vector2(Screen.width + original.rect.width, original.anchoredPosition.y);
        }
    }

    private void ResetPosition(RectTransform original, bool isMovingRight)
    {
        if (isMovingRight)
        {
            original.anchoredPosition = new Vector2(-Screen.width, original.anchoredPosition.y);
        }
        else
        {
            original.anchoredPosition = new Vector2(Screen.width, original.anchoredPosition.y);
        }
    }

    private void Update()
    {
        if (!IsPlaying || !IsAnimationReady) return;

        ChainAndAnimatePhotos(sequences[0].GetComponent<RectTransform>(), clones[0], false);
        ChainAndAnimatePhotos(sequences[1].GetComponent<RectTransform>(), clones[1], true);
        ChainAndAnimatePhotos(sequences[2].GetComponent<RectTransform>(), clones[2], false);
    }

    private void ChainAndAnimatePhotos(RectTransform original, RectTransform clone, bool isMovingRight)
    {
        if (isMovingRight) //middle
        {
            //first photo collage animation
            if (original.anchoredPosition.x < original.rect.width)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ?
                    GlobalVariable.middle_horizontalSpeed :
                    GlobalVariable.filmStrip_middle_horizontalSpeed) * Time.deltaTime;

                original.anchoredPosition = original.anchoredPosition + new Vector2(direction * _speed, 0f);
            }
            else
            {
                original.anchoredPosition = new Vector2(clone.anchoredPosition.x - clone.rect.width, original.anchoredPosition.y);
            }

            //clone photo collage animation
            if (clone.anchoredPosition.x < original.rect.width)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ?
                    GlobalVariable.middle_horizontalSpeed :
                    GlobalVariable.filmStrip_middle_horizontalSpeed) * Time.deltaTime;

                clone.anchoredPosition = clone.anchoredPosition + new Vector2(direction * _speed, 0f);
            }
            else
            {
                clone.anchoredPosition = new Vector2(original.anchoredPosition.x - original.rect.width, clone.anchoredPosition.y);
            }
        }
        else //top and bottom
        {
            //first photo collage animation
            if (original.anchoredPosition.x > -original.rect.width)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ?
                    GlobalVariable.topBottom_horizontalSpeed :
                    GlobalVariable.filmStrip_topBottom_horizontalSpeed) * Time.deltaTime;

                original.anchoredPosition = original.anchoredPosition + new Vector2(-direction * _speed, 0f);
            }
            else
            {
                original.anchoredPosition = new Vector2(clone.anchoredPosition.x + clone.rect.width, original.anchoredPosition.y);
            }

            //clone photo collage animation
            if (clone.anchoredPosition.x > -clone.rect.width)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ?
                    GlobalVariable.topBottom_horizontalSpeed :
                    GlobalVariable.filmStrip_topBottom_horizontalSpeed) * Time.deltaTime;

                clone.anchoredPosition = clone.anchoredPosition + new Vector2(-direction * _speed, 0f);
            }
            else
            {
                clone.anchoredPosition = new Vector2(original.anchoredPosition.x + original.rect.width, clone.anchoredPosition.y);
            }
        }
    }

    protected void MonitorCloneContent()
    {
        foreach(ContainerSequence con in sequences)
        {
            if (con.transform.childCount > con.clone.childCount)
            {
                Instantiate(con.transform.GetChild(con.transform.childCount - 1), con.clone.transform);
            }
        }
    }
    void ReallignSlides()
    {
        sequences[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(
            sequences[0].GetComponent<RectTransform>().anchoredPosition.x, 
            sequences[2].GetComponent<RectTransform>().anchoredPosition.y
        );
        clones[2].anchoredPosition = new Vector2(
            sequences[2].GetComponent<RectTransform>().anchoredPosition.x + sequences[2].GetComponent<RectTransform>().rect.width,
            clones[2].anchoredPosition.y
        );
    }
    void StartMonitorClone()
    {
        InvokeRepeating("MonitorCloneContent", 2f, 5f);
    }
    void StopMonitorClone()
    {
        CancelInvoke("MonitorCloneContent");
    }
    void StartReallignSlides()
    {
        InvokeRepeating("ReallignSlides", 5400f, 5400f);
    }
    void StopReallignSlides()
    {
        CancelInvoke("ReallignSlides");
    }

    public static void RandomizePosition(Transform[] original, Transform[] clone)
    {
        System.Random rand = new System.Random();

        for (int i = 0; i < original.Length - 1; i++)
        {
            int r = rand.Next(i, original.Length);
            int temp = original[i].GetSiblingIndex();
            original[i].SetSiblingIndex(original[r].GetSiblingIndex());
            clone[i].SetSiblingIndex(clone[r].GetSiblingIndex());
            original[r].SetSiblingIndex(temp);
            clone[r].SetSiblingIndex(temp);
        }
    }

    //Photo Priorities management
    int highPriorityIndex = 3;
    int superPriorityIndex = 0;
    public List<int> HighPriorityIndices = new List<int>();
    public List<int> SuperPriorityIndices = new List<int>();
    const int multiplier = 5;

    private void CollectPriorityIndices()
    {
        if (sequences[0].transform.childCount == 0)
            return;

        HighPriorityIndices.Clear();
        while (highPriorityIndex < sequences[0].transform.childCount || highPriorityIndex + 1 <= sequences[0].transform.childCount)
        {
            HighPriorityIndices.Add(highPriorityIndex);
            HighPriorityIndices.Add(highPriorityIndex + 1);
            highPriorityIndex += multiplier;
        }

        highPriorityIndex = 3;

        SuperPriorityIndices.Clear();
        while (superPriorityIndex < sequences[0].transform.childCount
            || superPriorityIndex + 1 <= sequences[0].transform.childCount
            || superPriorityIndex + 2 <= sequences[0].transform.childCount)
        {
            SuperPriorityIndices.Add(superPriorityIndex);
            SuperPriorityIndices.Add(superPriorityIndex + 1);
            SuperPriorityIndices.Add(superPriorityIndex + 2);
            superPriorityIndex += multiplier;
        }

        superPriorityIndex = 0;
    }

    private int PriorityValue(int currentIndex)
    {
        if (HighPriorityIndices.Contains(currentIndex))
            return 1;
        if (SuperPriorityIndices.Contains(currentIndex))
            return 2;

        return -1;
    }

    void ArrangePhotosPriority()
    {
        int length = sequences[0].transform.childCount;
        int arrangeIndex = 0;

        if (length == 0) return;

        for(int i = 0; i < length-1 ; i++)
        {
            arrangeIndex = i;
            //print($"arIndex : {arrangeIndex}, Prio : {sequences[0].transform.GetChild(arrangeIndex).GetComponent<Picture>().priority}, ExpectedPrio : {PriorityValue(arrangeIndex)}");

            if (sequences[0].transform.GetChild(arrangeIndex).GetComponent<Picture>().priority == PriorityValue(arrangeIndex))
            {
                //print($" A : {arrangeIndex}, {sequences[0].transform.GetChild(arrangeIndex).GetComponent<Picture>().priority}, {PriorityValue(arrangeIndex)}");
                continue;
            }
            else
            {
                for (int j = i+1; j < length; j++)
                {
                    if (sequences[0].transform.GetChild(j).GetComponent<Picture>().priority == PriorityValue(arrangeIndex))
                    {    
                        arrangeIndex = j;
                        //print($" B : {arrangeIndex}, {sequences[0].transform.GetChild(j).GetComponent<Picture>().priority}, {PriorityValue(i)}");
                        break;
                    }
                }

                int icopy = i;
                SwapPosition(icopy, arrangeIndex);
            }
        }
    }

    private void SwapPosition(int current, int found)
    {
        Transform iTransform = sequences[0].transform.GetChild(current);
        sequences[0].transform.GetChild(found).SetSiblingIndex(current);
        iTransform.SetSiblingIndex(found);
    }

    private void Arrange()
    {
        CollectPriorityIndices();
        ArrangePhotosPriority();
    }

    private IEnumerator RearrangeClones()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < clones.Count; i++)
        {
            for(int j = 0; j < clones[i].childCount; j++)
            {
                Destroy(clones[i].GetChild(j).gameObject);
                //print(clones[i].GetChild(j).gameObject.name);
            }
        }

        for (int i = 0; i < sequences.Length; i++)
        {
            for (int j = 0; j < sequences[i].transform.childCount; j++)
            {
                Instantiate(sequences[i].transform.GetChild(j).gameObject, clones[i]);
            }
        }
    }
}
