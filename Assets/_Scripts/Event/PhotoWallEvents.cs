using System;

public class PhotoWallEvents
{
    public static Action OnResponseGet;
    public static Action OnInitializeDone;
    public static Action OnPhotoWallReady;
    public static Action OnLargePhotoShow;
    public static Action<APIManager.Data> OnNewPhotoAdded;
    public static Action<Picture> OnPhotoReady;
    public static Action<Picture> OnInitPhotoAdded;
    public static Action<Picture> OnPhotoLoaded;
    public static Action<string, int> OnDataUpdated;
    public static Action<string> OnPhotoDeleted;
}
