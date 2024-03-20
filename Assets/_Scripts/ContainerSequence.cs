using UnityEngine;
using System.Collections.Generic;

public class ContainerSequence : MonoBehaviour
{
    public RectTransform clone;

    public void SpawnPicture(PhotoWallManager manager, APIManager api, APIManager.Data data, float width, float height)
    {
        Picture photo = Instantiate(manager.photoPrefab, this.transform);

        photo.rect.sizeDelta = new Vector2(width, height);

        api.SetPhotoData(photo, data);
    }

    public void SpawnPicture(List<Picture> photos, Picture photo, float width, float height)
    {
        Picture _photo = Instantiate(photo, this.transform);
        photos.Add(_photo);
        _photo.rect.sizeDelta = new Vector2(width, height);
    }
}