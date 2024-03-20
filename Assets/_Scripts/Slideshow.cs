using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DanielLochner.Assets.SimpleScrollSnap;

public class Slideshow : MonoBehaviour, IAnimation
{
    [SerializeField] bool isPlaying;
    protected APIManager api;
    protected PhotoWallManager manager;

    public ScrollSnap scrollSnap;
    public ContentDynamic dynamicContent;
    public float startTime = 5f;
    public float repeatTime = 3f;
    public CanvasGroup canvasGroup; 
    public Slider slider;

    private float sliderCurrentValue;

    private void Awake()
    {
        PhotoWallEvents.OnNewPhotoAdded += ProcessImages;
    }
    private void OnDestroy()
    {
        PhotoWallEvents.OnNewPhotoAdded -= ProcessImages;
    }

    private void Start()
    {
        slider.value = 0;
        api = APIManager.Instance;
        manager = PhotoWallManager.Instance;
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

    public void StartAnimation()
    {
        InvokeRepeating("StartSlideShow", startTime, repeatTime);
    }

    public void StopAnimation()
    {
        CancelInvoke("StartSlideShow");
    }

    private void ProcessImages(APIManager.Data data)
    {
        dynamicContent.AddToBack(data);
    }

    private void StartSlideShow()
    {
        if(scrollSnap.NumberOfPanels > 0)
        {
            sliderCurrentValue = (((float)scrollSnap.GetNearestPanel() + 1f) / (float)scrollSnap.NumberOfPanels);
            slider.value = sliderCurrentValue % (float)scrollSnap.NumberOfPanels == 0f ? 0f : sliderCurrentValue;
            scrollSnap.GoToNextPanel();
        }
    }
}
