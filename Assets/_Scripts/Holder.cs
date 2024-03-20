using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holder : MonoBehaviour
{
    public HolderType type;
    public Picture[] pictures;
    public int index;
    public bool isOccupied = false;

    public void SetPhoto(APIManager api, APIManager.Data data)
    {
        api.SetPhotoData(pictures[index], data);
        
        index++;
        if(index == pictures.Length) 
        {
            isOccupied = true;
        }
    }

    public void SetPhoto(Picture photo)
    {
        pictures[index]._id = photo._id;
        pictures[index]._imageEP = photo._imageEP;
        pictures[index].createdAt = photo.createdAt;
        pictures[index].priority = photo.priority;
        pictures[index].image.texture = photo.image.mainTexture;
        pictures[index].hasTexture = photo.hasTexture;

        index++;
        if (index == pictures.Length)
        {
            isOccupied = true;
        }
    }
}
