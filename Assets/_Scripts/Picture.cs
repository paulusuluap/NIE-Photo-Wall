using UnityEngine;
using UnityEngine.UI;

public class Picture : MonoBehaviour
{
    public string _id;
    public string _imageEP;
    public RawImage image;
    public Image Img;
    public bool hasTexture { get; set; }

    public RectTransform rect;
    public string createdAt;
    public int priority;

    //public void RedoFetchPhoto()
    //{
    //    InvokeRepeating("FixMe", 20f, 10f);
    //}

    //if after photo wall is started but this picture has no texture then fix.
    protected void FixMe()
    {
        if (image == null)
        {
            print("raw image is null");
            return;
        }

        if (!string.IsNullOrEmpty(_imageEP))
        {
            //print($"{_imageEP} has been fetched");
            CancelInvoke("FixMe");
            return;
        }

        //print("Fix Me: " + _imageEP);
        APIManager.Instance.SetUnsetPhotoTexture(this);
    }
}
