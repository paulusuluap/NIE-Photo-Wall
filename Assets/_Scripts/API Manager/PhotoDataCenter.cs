using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoDataCenter : MonoBehaviour
{
    public APIManager api;
    public LargePhotosHandler largePhotoHandler;
    public Picture photoPrefab;
    public bool hasInit;
    public Transform container;
    public float trackPhotoTime = 1.5f;
    public List<Picture> basePictures = new List<Picture>();

    private Dictionary<string, int> datas = new Dictionary<string, int>();
    private IEnumerator repeatTrackPhoto;

    public static Action<Picture> OnNewPhotoAdded;
    public static Action<string> OnOldPhotoRemoved;
    public static Action<string, int> OnDataUpdated;
    public static Action<APIManager.Data> OnNew;

    private void Awake()
    {
        PhotoWallEvents.OnResponseGet += ManagePhotos;
    }

    private void OnDestroy()
    {
        PhotoWallEvents.OnResponseGet -= ManagePhotos;
    }

    private void Start()
    {
        repeatTrackPhoto = RepeatTrackPhotos();
        StartCoroutine(repeatTrackPhoto);
    }

    public void ManagePhotos()
    {
        if (api.serverResponse.status && api.serverResponse.data.Count > 0)
        {
            foreach (var data in api.serverResponse.data)
            {
                bool hasPhoto = datas.ContainsKey(data._id);
                bool hasNewData = datas.ContainsKey(data._id) && datas[data._id] != data.high_priority;

                if (!hasPhoto)
                {
                    Picture photo = Instantiate(photoPrefab, container);
                    //photo.RedoFetchPhoto();
                    //api.SetPhotoData(photo, data);

                    datas.Add(data._id, data.high_priority);
                    basePictures.Add(photo);
                    //OnNewPhotoAdded?.Invoke(Picture)

                    if (hasInit)
                    {
                        //largePhotoHandler.SetLargePhoto(data);
                    }

                    OnNew?.Invoke(data); //for test
                }

                if (hasNewData)
                {
                    //update
                    datas[data._id] = data.high_priority;
                    OnDataUpdated?.Invoke(data._id, data.high_priority);
                }
            }

            //foreach(var data in datas)
            //{
            //    int index = api.serverResponse.data.FindIndex(x => x._id)
            //}
        }


        StartCoroutine(FinishInitialization());
    }

    //private IEnumerator SendPhotoDataEvent(APIManager.Data data)
    //{
    //    yield return new WaitForEndOfFrame();

    //    OnNewPhotoAdded?.Invoke(data);
    //}

    private IEnumerator FinishInitialization()
    {
        yield return new WaitForSeconds(2f);

        hasInit = true;
    }

    public bool TrackPhotosDone()
    {
        if (basePictures.Count == 0) return false;

        for (int i = 0; i < basePictures.Count - 1; i++)
        {
            if (!basePictures[i].hasTexture)
            {
                return false;
            }
            else
            {
                if (i < basePictures.Count - 1) continue;
            }
        }
        bool AllHaveTextures = basePictures[basePictures.Count - 1].hasTexture;

        return AllHaveTextures;
    }

    public IEnumerator RepeatTrackPhotos()
    {
        yield return new WaitForSeconds(trackPhotoTime);

        bool texturesReady = TrackPhotosDone();

        if (texturesReady)
        {
            hasInit = true;

            ControlPhotosOverAnimations();
            StopCoroutine(repeatTrackPhoto);
        }
        else
        {
            repeatTrackPhoto = RepeatTrackPhotos();
            StartCoroutine(repeatTrackPhoto);
        }
    }

    List<Picture> comparisonPictures = new List<Picture>();

    public void ControlPhotosOverAnimations()
    {
        if (comparisonPictures.Count == basePictures.Count) return;

        //New picture
        if(comparisonPictures.Count < basePictures.Count)
        {
            foreach(var pict in basePictures)
            {
                if(!comparisonPictures.Contains(pict))
                {
                    comparisonPictures.Add(pict);
                    OnNewPhotoAdded?.Invoke(pict);
                }
            }
        }
        //Remove picture
        else if (comparisonPictures.Count > basePictures.Count)
        {
            foreach(var pict in comparisonPictures)
            {
                var _pict = basePictures.Find(x => !basePictures.Contains(pict));
                
                if(_pict != null)
                {
                    comparisonPictures.Remove(_pict);
                    OnOldPhotoRemoved?.Invoke(_pict._id);
                }
            }
        }
    }
}
