using UnityEngine;

public class Animation : MonoBehaviour
{
    public bool IsPlaying;
    public APIManager api;
    public PhotoWallManager manager;


    public virtual void ProcessImages(Picture photo) {}
    public virtual void RemoveImage(string id) {}
}
