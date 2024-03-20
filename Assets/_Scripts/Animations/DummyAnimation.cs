using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAnimation : MonoBehaviour
{
    public Transform container;
    public List<Picture> pictureList;

    private void Awake()
    {
        PhotoDataCenter.OnNewPhotoAdded += ProcessImages;
        PhotoDataCenter.OnOldPhotoRemoved += DestroyImages;
    }

    private void OnDestroy()
    {
        PhotoDataCenter.OnNewPhotoAdded -= ProcessImages;
        PhotoDataCenter.OnOldPhotoRemoved -= DestroyImages;
    }


    private void ProcessImages(Picture picture)
    {
        var newPict = Instantiate(picture, container);
        pictureList.Add(newPict);
    }

    private void DestroyImages(string id)
    {
        var oldPict = pictureList.Find(x => x._id == id);
        
        if(oldPict != null)
        {
            pictureList.Remove(oldPict);
        }
    }
}
