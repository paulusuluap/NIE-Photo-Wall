using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LargePhotosHandler : MonoBehaviour
{
    public List<LargePicture> largePhotos;

    protected int largePhotoIndex = 0;
    protected Picture largePhoto;
    protected float appearDuration;
    protected const float bounceDuration = 0.65f;

    //public static Action OnLargePhotoShow;

    private void Start()
    {
        foreach(var large in largePhotos) 
        {
            large.gallery = FindObjectOfType<GalleryAnimation>();
            large.priorityAnim = FindObjectOfType<PriorityAnimation>();
        }
    }

    private void OnEnable()
    {
        //APIManager.OnPhotoReady += Display;
        PhotoWallEvents.OnPhotoLoaded += Display;
    }

    private void OnDisable()
    {
        //APIManager.OnPhotoReady -= Display;
        PhotoWallEvents.OnPhotoLoaded -= Display;
    }

    /// <summary>
    /// Display grid photos
    /// </summary>
    /// <param name="data"></param>
    public void SetLargePhoto()
    {
        if (largePhotos != null && largePhotos.Count > 1)
        {
            largePhoto = null;
            int largeIndex = largePhotos.FindIndex(largePhoto => !largePhoto.gameObject.activeInHierarchy);

            //if inactive was found
            if (largeIndex != -1)
            {
                largePhotoIndex = largeIndex;
                largePhoto = largePhotos[largePhotoIndex];
                largePhoto.gameObject.SetActive(false);
                largePhotoIndex = (largePhotoIndex + 1) % largePhotos.Count;
            }
            //if all large photos active so we reset
            else
            {
                PhotoWallEvents.OnLargePhotoShow?.Invoke();
                largePhoto = largePhotos[largePhotoIndex];
                largePhoto.gameObject.SetActive(false);
                largePhotoIndex = (largePhotoIndex + 1) % largePhotos.Count;
            }

            //APIManager.Instance.SetPhotoData(largePhoto, data, true);
        }
    }

    private void Display(Picture newPhoto)
    {
        SetLargePhoto();

        largePhoto.image.texture = newPhoto.image.texture;
        largePhoto.transform.localScale = Vector3.zero;
        largePhoto.gameObject.SetActive(true);
        largePhoto.transform.DOScale(Vector3.one, bounceDuration).SetEase(Ease.InBounce);
    }

    public void ChangeAppearDuration(int time)
    {
        appearDuration = (float) time;

        foreach (var large in largePhotos)
            large.appearDuration = appearDuration;
    }
}
