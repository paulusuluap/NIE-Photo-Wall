using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PhotoClub : MonoBehaviour
{
    public bool highPriority;
    public List<Picture> pictures = new List<Picture>();

    //private APIManager api;
    private int currentIndex = 1; // why 1 ? cuz it will be switch to 0
    public bool hasInit { get; set; }

    //private void Start()
    //{
    //    api = APIManager.Instance;
    //}

    public void Initialize()
    {
        foreach(var pic in pictures)
        {
            pic.image.color = Color.black;
        }
    }

    public void SwitchPhoto(APIManager.Data data = null)
    {
        pictures[currentIndex].image.DOColor(Color.black, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            pictures[currentIndex].image.enabled = false;

            //Set new photo
            if (hasInit && data != null)
            {
                pictures[currentIndex]._id = data._id;
                //pictures[currentIndex].image.texture = FadeAnimation.tempPhotos.Find(x => x._id == data._id).image.mainTexture;
                //api.SetPhotoData(pictures[currentIndex], data, false, false, true);
            }

            currentIndex++;
            currentIndex %= pictures.Count;

            pictures[currentIndex].image.enabled = true;
            pictures[currentIndex].image.color = Color.black;
            pictures[currentIndex].image.DOColor(Color.white, 1f).SetEase(Ease.Linear);
        });
    }
}
