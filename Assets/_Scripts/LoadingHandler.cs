using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LoadingHandler : MonoBehaviour
{
    public Transform icon;
    public CanvasGroup canvas;
    public CanvasGroup LoadingCircle;
    public CanvasGroup texts;

    private PhotoWallManager manager;
    private APIManager api;
    private Tween yoyo;
    private bool HasStartLoading;
    private float monitorTime = 2f;

    protected bool IsPhotoWallReady = false;

    private void OnEnable()
    {
        PhotoWallEvents.OnResponseGet += LoadData;
    }

    private void OnDisable()
    {
        PhotoWallEvents.OnResponseGet -= LoadData;
    }

    private void Start()
    {
        manager = PhotoWallManager.Instance;
        api = APIManager.Instance;

        icon.localScale = Vector3.one;
        LoadingCircle.alpha = 0f;
        texts.alpha = 0f;
    }

    private void LoadData()
    {
        if (HasStartLoading) return;

        StartAnimation();
    }

    private void StartAnimation()
    {
        HasStartLoading = true;

        yoyo = icon.DOScale(1.2f, 0.75f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(texts.DOFade(1f, 0.75f));
        seq.Append(LoadingCircle.DOFade(1f, 0.3f));

        LoadingCoroutine = MonitorLoadedData();
        StartCoroutine(LoadingCoroutine);
    }

    private void StopAnimation()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(LoadingCircle.DOFade(0f, 0.35f));
        seq.Append(texts.DOFade(0f, 0.5f));
        seq.Append(icon.DOScale(0f, 1f).SetEase(Ease.OutBounce));
        seq.OnComplete(() => { 
            canvas.DOFade(0f, 1f).SetEase(Ease.Linear);
            manager.IsPhotoWallReady = true;
            manager.AnimationIndex = api.firstAnimation;
            manager.animations[api.firstAnimation].Activate();
            DOTween.Kill(yoyo);
        });
    }

    private IEnumerator LoadingCoroutine;

    private IEnumerator MonitorLoadedData()
    {
        yield return new WaitForSeconds(monitorTime);
        
        for(int i = 0; i < manager.basePhotos.Count - 1; i++)
        {
            if (!manager.basePhotos[i].hasTexture)
            {
                //print(manager.basePhotos[i]._id + " texture has not set.");
                LoadingCoroutine = MonitorLoadedData();
                StartCoroutine(LoadingCoroutine);
                yield break;
            }
            else
            {
                if(i < manager.basePhotos.Count - 1) continue;
            }
        }

        IsPhotoWallReady = manager.basePhotos[manager.basePhotos.Count - 1].hasTexture;

        if (IsPhotoWallReady)
        {
            manager.BroadcastPhotosToAnimations();

            yield return new WaitForSeconds(monitorTime);

            StopAnimation();
            PhotoWallEvents.OnPhotoWallReady?.Invoke();
        }
    }
}
