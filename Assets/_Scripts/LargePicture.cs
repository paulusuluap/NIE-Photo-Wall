using DG.Tweening;
using UnityEngine;

public class LargePicture : Picture
{
    public GalleryAnimation gallery;
    public PriorityAnimation priorityAnim;
    public float appearDuration = 20f;

    private void OnEnable()
    {
        //this.gameObject.name = "P" + UnityEngine.Random.Range(100, 1000);
        Invoke("HideLargePhoto", appearDuration);
    }

    private void OnDisable()
    {
        CancelInvoke("HideLargePhoto");
    }

    protected void HideLargePhoto()
    {
        //Hide photo and trigger things
        this.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            this.gameObject.SetActive(false);

            //if (gallery != null && gallery.IsPlaying)
            //{
            //    gallery.DisplayInactiveGridPhotos();
            //    gallery.ShowTakenPhotoBatch(_id);
            //}
            
            if(priorityAnim != null)
            {
                priorityAnim.ShowNewlyTakenPhoto();
            }
            
            _id = string.Empty;
        });
    }
}
