using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class TrainAnimation : Animation, IAnimation
{
    //[SerializeField]
    //bool isPlaying;

    //protected APIManager api;
    //protected PhotoWallManager manager;

    int sequenceIndex;
    int maxSequence = 3;
    bool IsAnimationReady;
    int textIndex;

    [Header("Etc.")]
    public float speed = 120f;
    public float startTime = 2f;
    public Transform content;
    public CanvasGroup canvasGroup;
    public TrainSequence[] sequences;
    public Texture[] photoTexs;

    public List<RectTransform> clones = new List<RectTransform>();


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
        BannerManager.instance.SetBannerByAxis(Enums.MovementAxis.Horizontal);
        Invoke("StartPhotoAnimation", 1f);
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
        CancelInvoke("StartPhotoAnimation");
    }

    private void ProcessImages(APIManager.Data data)
    {
        sequences[sequenceIndex].SpawnHolder(api, data);

        sequenceIndex++;
        sequenceIndex %= maxSequence;
    }

    public override void ProcessImages(Picture photo)
    {
        sequences[sequenceIndex].SpawnHolder(photo);

        sequenceIndex++;
        sequenceIndex %= maxSequence;
    }

    public override void RemoveImage(string id)
    {
        foreach(var seq in sequences)
        {
            for(int i = 0; i < seq.activeHolders.Count; i++)
            {
                Picture deletedPict = Array.Find(seq.activeHolders[i].pictures, pict => pict._id == id);

                if (deletedPict != null)
                {
                    deletedPict._id = string.Empty;
                    deletedPict._imageEP = string.Empty;
                    deletedPict.createdAt = string.Empty;
                    deletedPict.priority = 0;
                    deletedPict.image.texture = photoTexs[textIndex];
                    deletedPict.hasTexture = true;

                    textIndex++;
                    textIndex %= photoTexs.Length;
                }
            }
        }
    }

    protected void StartPhotoAnimation()
    {
        StartCoroutine(InitializeCoroutine());
    }

    private IEnumerator InitializeCoroutine()
    {
        Initialize(sequences[0], false);
        Initialize(sequences[1], true);
        Initialize(sequences[2], false);

        yield return new WaitForSeconds(startTime);

        IsAnimationReady = true;
        StartMonitorClone();
    }

    private void Initialize(TrainSequence sequence, bool isMovingRight)
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

    private void ChainAndAnimatePhotos(RectTransform original, RectTransform clone, bool isMovingRight, float offset = 0)
    {
        if (isMovingRight)
        {
            if (original.anchoredPosition.x < original.rect.width)
            {
                var _speed = GlobalVariable.middle_horizontalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.rect.width, original.anchoredPosition.y), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(clone.anchoredPosition.x - clone.rect.width, original.anchoredPosition.y);
            }

            if (clone.anchoredPosition.x < original.rect.width)
            {
                var _speed = GlobalVariable.middle_horizontalSpeed * Time.deltaTime;
                clone.anchoredPosition = Vector2.MoveTowards(clone.anchoredPosition, new Vector2(clone.rect.width, clone.anchoredPosition.y), _speed);
            }
            else
            {
                clone.anchoredPosition = new Vector2(original.anchoredPosition.x - original.rect.width, clone.anchoredPosition.y);
            }
        }
        else
        {
            if (original.anchoredPosition.x > -original.rect.width)
            {
                var _speed = GlobalVariable.topBottom_horizontalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(-original.rect.width, original.anchoredPosition.y), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(clone.anchoredPosition.x + clone.rect.width, original.anchoredPosition.y);
            }

            if (clone.anchoredPosition.x > -clone.rect.width)
            {
                var _speed = GlobalVariable.topBottom_horizontalSpeed * Time.deltaTime;
                clone.anchoredPosition = Vector2.MoveTowards(clone.anchoredPosition, new Vector2(-clone.rect.width, clone.anchoredPosition.y), _speed);
            }
            else
            {
                clone.anchoredPosition = new Vector2(original.anchoredPosition.x + original.rect.width, clone.anchoredPosition.y);
            }
        }
    }
    protected void MonitorCloneContent()
    {
        foreach (TrainSequence con in sequences)
        {
            Destroy(con.clone.GetChild(con.clone.childCount - 1).gameObject);
            Instantiate(con.transform.GetChild(con.transform.childCount - 1), con.clone.transform);

            if (con.transform.childCount > con.clone.childCount)
            {
                Instantiate(con.transform.GetChild(con.transform.childCount - 1), con.clone.transform);
            }
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
}

[Serializable]
public enum HolderType
{
    A,
    B,
    C
}
