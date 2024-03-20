using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class VerticalAnimation : Animation, IAnimation
{
    public Enums.ScrollType scrollType;

    //[SerializeField]
    //bool IsPlaying;
    //public bool IsPlaying
    //{
    //    get => isPlaying;
    //}
    //protected APIManager api;
    //protected PhotoWallManager manager;

    int sequenceIndex;
    int maxSequence = 5;
    bool IsAnimationReady;

    [Header("Etc.")]
    public float startTime = 2f;
    public float photoWidth = 455f;
    public float photoHeight = 245f;
    public float direction = 1f;
    public Transform content;
    public CanvasGroup canvasGroup;
    public ContainerSequence[] sequences;

    public List<RectTransform> clones = new List<RectTransform>();
    public List<Picture> photos = new List<Picture>();

    private void Awake()
    {
        PhotoWallEvents.OnInitPhotoAdded += ProcessImages;
        PhotoWallEvents.OnPhotoLoaded += ProcessImages;
        PhotoWallEvents.OnPhotoDeleted += RemoveImage;
    }
    private void OnDestroy()
    {
        PhotoWallEvents.OnInitPhotoAdded -= ProcessImages;
        PhotoWallEvents.OnPhotoLoaded -= ProcessImages;
        PhotoWallEvents.OnPhotoDeleted -= RemoveImage;
    }

    private void Start()
    {
        api = APIManager.Instance;
        manager = PhotoWallManager.Instance;

        //StartAnimation();
        //isPlaying = true;
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
        Invoke("StartPhotoAnimation", startTime);
    }

    public void StopAnimation()
    {
        IsAnimationReady = false;
        foreach (var clone in clones)
            Destroy(clone.gameObject);
        clones.Clear();

        ResetPosition(sequences[0].GetComponent<RectTransform>(), true);
        ResetPosition(sequences[1].GetComponent<RectTransform>(), false);
        ResetPosition(sequences[2].GetComponent<RectTransform>(), true);
        ResetPosition(sequences[3].GetComponent<RectTransform>(), false);
        ResetPosition(sequences[4].GetComponent<RectTransform>(), true);

        StopMonitorClone();
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
        sequences[sequenceIndex].SpawnPicture(photos, photo, photoWidth, photoHeight);

        sequenceIndex++;
        sequenceIndex %= maxSequence;
    }

    public override void RemoveImage(string id)
    {
        int index = photos.FindIndex(x => x._id == id);

        if (index != -1)
        {
            Destroy(photos[index].gameObject);
            photos.RemoveAt(index);
        }
    }

    protected void StartPhotoAnimation()
    {
        StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        Initialize(sequences[0], true);
        Initialize(sequences[1], false);
        Initialize(sequences[2], true);
        Initialize(sequences[3], false);
        Initialize(sequences[4], true);

        yield return new WaitForSeconds(startTime);

        //Monitor clone contents
        StartMonitorClone();
        IsAnimationReady = true;
    }

    private void Initialize(ContainerSequence sequence, bool isMovingUpward)
    {
        RectTransform original = sequence.GetComponent<RectTransform>();

        var clone = Instantiate(original, content);
        clone.anchorMax = original.anchorMax;
        clone.anchorMin = original.anchorMin;
        sequence.clone = clone;

        clones.Add(clone);

        if (isMovingUpward)
        {   
            original.anchoredPosition = new Vector2(original.anchoredPosition.x, -Screen.height);
            clone.anchoredPosition = new Vector2(original.anchoredPosition.x, -Screen.height - original.rect.height);
        }
        else
        {
            original.anchoredPosition = new Vector2(original.anchoredPosition.x, Screen.height);
            clone.anchoredPosition = new Vector2(original.anchoredPosition.x, Screen.height + original.rect.height);
        }

        //Randomize photo indexes
        //RandomizePosition(parent_1, parent_2);
    }

    private void ResetPosition(RectTransform original, bool isMovingRight)
    {
        if (isMovingRight)
        {
            original.anchoredPosition = new Vector2(original.anchoredPosition.x, -Screen.width);
        }
        else
        {
            original.anchoredPosition = new Vector2(original.anchoredPosition.x, Screen.width);
        }
    }


    private void Update()
    {
        if (!IsPlaying || !IsAnimationReady) return;

        ChainAndAnimatePhotos(sequences[0].GetComponent<RectTransform>(), clones[0], true);
        ChainAndAnimatePhotos(sequences[1].GetComponent<RectTransform>(), clones[1], false);
        ChainAndAnimatePhotos(sequences[2].GetComponent<RectTransform>(), clones[2], true);
        ChainAndAnimatePhotos(sequences[3].GetComponent<RectTransform>(), clones[3], false);
        ChainAndAnimatePhotos(sequences[4].GetComponent<RectTransform>(), clones[4], true);
    }

    private void ChainAndAnimatePhotos(RectTransform original, RectTransform clone, bool isMovingUpward)
    {
        if (isMovingUpward)
        {
            //original photo collage animation
            if (original.anchoredPosition.y < original.rect.height)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ? GlobalVariable.verticalSpeed : GlobalVariable.filmStrip_verticalSpeed) * Time.deltaTime;
                //original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.anchoredPosition.x, original.rect.height), _speed);
                original.anchoredPosition = original.anchoredPosition + new Vector2(0f, direction * _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(original.anchoredPosition.x, clone.anchoredPosition.y - clone.rect.height);
            }

            //clone photo collage animation
            if (clone.anchoredPosition.y < clone.rect.height)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ? GlobalVariable.verticalSpeed : GlobalVariable.filmStrip_verticalSpeed) * Time.deltaTime;
                //clone.anchoredPosition = Vector2.MoveTowards(clone.anchoredPosition, new Vector2(clone.anchoredPosition.x, clone.rect.height), _speed);
                clone.anchoredPosition = clone.anchoredPosition + new Vector2(0f, direction * _speed);
            }
            else
            {
                clone.anchoredPosition = new Vector2(clone.anchoredPosition.x, original.anchoredPosition.y - original.rect.height);
            }
        }
        else
        {
            //original photo collage animation
            if (original.anchoredPosition.y > -original.rect.height)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ? GlobalVariable.verticalSpeed : GlobalVariable.filmStrip_verticalSpeed) * Time.deltaTime;
                //original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.anchoredPosition.x, -original.rect.height), _speed);
                original.anchoredPosition = original.anchoredPosition + new Vector2(0f, -direction * _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(original.anchoredPosition.x, clone.anchoredPosition.y + clone.rect.height);
            }

            //clone photo collage animation
            if (clone.anchoredPosition.y > -clone.rect.height)
            {
                var _speed = (scrollType == Enums.ScrollType.Normal ? GlobalVariable.verticalSpeed : GlobalVariable.filmStrip_verticalSpeed) * Time.deltaTime;
                //clone.anchoredPosition = Vector2.MoveTowards(clone.anchoredPosition, new Vector2(clone.anchoredPosition.x, -clone.rect.height), _speed);
                clone.anchoredPosition = clone.anchoredPosition + new Vector2(0f, -direction * _speed);
            }
            else
            {
                clone.anchoredPosition = new Vector2(clone.anchoredPosition.x, original.anchoredPosition.y + original.rect.height);
            }
        }
    }

    protected void MonitorCloneContent()
    {
        foreach (ContainerSequence con in sequences)
        {
            if (con.transform.childCount > con.clone.childCount)
               Instantiate(con.transform.GetChild(con.transform.childCount - 1), con.clone.transform);
        }
    }
    void StartMonitorClone()
    {
        InvokeRepeating("MonitorCloneContent", 2f, 5f);
    }
    void StopMonitorClone()
    {
        CancelInvoke("MonitorCloneContent");
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
}
