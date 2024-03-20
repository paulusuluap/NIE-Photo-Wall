using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadImages : MonoBehaviour
{
    public static LoadImages Instance;

    LoadBalancer<GameObject> loader;
    LoadBalancer<Picture> photoLoader;
    LoadBalancer<Picture> photoInitLoader;
    //int xPos = 0;

    //public static Action<Picture> OnPhotoLoaded;

    private void Awake()
    {
        Instance = this;
    }

    //void Start()
    //{
    //    loader = new LoadBalancer<GameObject>(this, 10, HandleResponse);
    //    for (int i = 0; i < 1000; i++)
    //    {
    //        loader.AddTask(LoadImage("https://photo-kiosk.fxwebapps.com/upload/gallery/652e11b09aff88f664db4122/65aae62a9ef295d0f5fd55dd_smaller.png"), CreateCube(xPos++));
    //        loader.AddTask(LoadImage("https://photo-kiosk.fxwebapps.com/upload/gallery/652e11b09aff88f664db4122/65aae62a9ef295d0f5fd55dd_smaller.png"), CreateCube(xPos++));
    //        loader.AddTask(LoadImage("https://photo-kiosk.fxwebapps.com/upload/gallery/652e11b09aff88f664db4122/65aae62a9ef295d0f5fd55dd_smaller.png"), CreateCube(xPos++));
    //    }
    //}

    public void LoadPhoto(string aUrl, Picture aPhoto, APIManager.Data data)
    {
        photoLoader = new LoadBalancer<Picture>(this, 10, HandlePhotoResponse);
        photoLoader.AddTask(LoadImage(aUrl), SetPhotoData(aPhoto, data));
    }

    private int photoCounter;
    public void LoadInitPhotos(List<APIManager.Data> data, List<Picture> basePhotos, Picture prefab, Transform parent)
    {
        photoInitLoader = new LoadBalancer<Picture>(this, 10, HandlePhotoResponse);
        int _apiMaxPhotos = APIManager.Instance.maxPhotos;
        int _maxPhotos = _apiMaxPhotos == -1 ? data.Count : _apiMaxPhotos;

        //load very high and high priorities first
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].high_priority != 0)
            {
                photoInitLoader.AddTask(LoadImage(APIManager.Instance.GetImageUrl(data[i])), CreatePhoto(data[i], basePhotos, prefab, parent));
            }
        }

        //load normal priority photos with max Photos as threshold
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].high_priority == 0 && photoCounter < _maxPhotos)
            {
                photoInitLoader.AddTask(LoadImage(APIManager.Instance.GetImageUrl(data[i])), CreatePhoto(data[i], basePhotos, prefab, parent));
                photoCounter++;
            }
        }
    }

    Picture CreatePhoto(APIManager.Data data, List<Picture> basePhotos, Picture prefab, Transform parent)
    {
        Picture photo = Instantiate(prefab, parent);

        photo._id = data._id;
        photo._imageEP = data.image;
        photo.createdAt = data.createdAt;
        photo.priority = data.high_priority;
        basePhotos.Add(photo);

        return photo;
    }

    Picture SetPhotoData(Picture photo, APIManager.Data data)
    {
        photo._id = data._id;
        photo._imageEP = data.image;
        photo.createdAt = data.createdAt;
        photo.priority = data.high_priority;

        return photo;
    }

    void HandlePhotoResponse(UnityWebRequestAsyncOperation aResponse, Picture photo)
    {
        Texture2D tempTexture = (DownloadHandlerTexture.GetContent(aResponse.webRequest));
        var mipTexture = new Texture2D(tempTexture.width, tempTexture.height);
        mipTexture.SetPixels(tempTexture.GetPixels());
        mipTexture.wrapMode = TextureWrapMode.Clamp;
        mipTexture.Apply();

        photo.image.texture = mipTexture;
        photo.image.color = Color.white;
        photo.hasTexture = true;

        if(PhotoWallManager.Instance.IsPhotoWallReady)
            PhotoWallEvents.OnPhotoLoaded?.Invoke(photo);
    }

    UnityWebRequest LoadImage(string aURL)
    {
        var request = UnityWebRequest.Get(aURL);
        request.downloadHandler = new DownloadHandlerTexture();
        return request;
    }

    #region For test

    GameObject CreateCube(int aXPos)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(1.1f * aXPos, 0, 0);
        return cube;
    }

    void HandleResponse(UnityWebRequestAsyncOperation aResponse, GameObject aGO)
    {
        Debug.Log($"Completed: { aResponse.webRequest.url} data: { aResponse.webRequest.downloadedBytes} bytes");
        aGO.GetComponent<MeshRenderer>().material.mainTexture = DownloadHandlerTexture.GetContent(aResponse.webRequest);
    }

    #endregion
}

public class LoadBalancer<T>
{
    public class Request
    {
        public UnityWebRequest request;
        public UnityWebRequestAsyncOperation response = null;
        public T data;
        public Action<UnityWebRequestAsyncOperation, T> callback;
        public Request(UnityWebRequest aRequest, T aData, System.Action<UnityWebRequestAsyncOperation, T> aCallback)
        {
            request = aRequest;
            callback = aCallback;
            data = aData;
        }
    }
    private Queue<Request> m_Queue = new Queue<Request>();
    private List<Request> m_ActiveList = new List<Request>();
    private MonoBehaviour m_Host;
    private Coroutine m_Coroutine = null;
    private int m_MaxTasks;
    public bool IgnoreMissingCallback = false;
    public Action<UnityWebRequestAsyncOperation, T> defaultCallback;
    public LoadBalancer(MonoBehaviour aHost, int aMaxTasks, System.Action<UnityWebRequestAsyncOperation, T> aDefaultCallback = null)
    {
        m_Host = aHost;
        m_MaxTasks = aMaxTasks;
        defaultCallback = aDefaultCallback;
    }

    public void AddTask(UnityWebRequest aTask, T aData = default, System.Action<UnityWebRequestAsyncOperation, T> aCallback = null)
    {
        if (aCallback == null)
        {
            aCallback = defaultCallback;
            if (!IgnoreMissingCallback && aCallback == null)
                Debug.LogWarning("Added task without callback");
        }
        m_Queue.Enqueue(new Request(aTask, aData, aCallback));
        if (m_Coroutine == null)
            m_Coroutine = m_Host.StartCoroutine(RunRequests());
    }

    public void AbortAllRequests()
    {
        m_Host.StopCoroutine(m_Coroutine);
        foreach (var request in m_ActiveList)
            request.request.Dispose();
        m_ActiveList.Clear();
        m_Queue.Clear();
    }

    private IEnumerator RunRequests()
    {
        while (m_Queue.Count > 0 || m_ActiveList.Count > 0)
        {
            while (m_ActiveList.Count < m_MaxTasks && m_Queue.Count > 0)
            {
                var request = m_Queue.Dequeue();
                request.response = request.request.SendWebRequest();
                m_ActiveList.Add(request);
            }
            for (int i = 0; i < m_ActiveList.Count; i++)
            {
                var request = m_ActiveList[i];
                if (request.response.isDone)
                {
                    m_ActiveList[i] = m_ActiveList[m_ActiveList.Count - 1];
                    m_ActiveList.RemoveAt(m_ActiveList.Count - 1);
                    if (request.callback != null)
                        request.callback(request.response, request.data);
                }
            }
            yield return null;
        }
        m_Coroutine = null;
    }
}
