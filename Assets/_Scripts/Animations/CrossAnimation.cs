using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;

public class CrossAnimation : Animation, IAnimation
{
	public CanvasGroup canvasGroup;
	public Transform Horizontal;
	public Transform Vertical;
	public Transform[] HighPriority;
	public Transform[] VeryHighPriority;
	public Transform Latest;

    public List<Picture> Normal = new List<Picture>();
    public List<Picture> High = new List<Picture>();
    public List<Picture> VeryHigh = new List<Picture>();
    public List<Picture> allPhotos = new List<Picture>();
    private List<GameObject> notMoving = new List<GameObject>();
    private List<GameObject> moving = new List<GameObject>();

	private bool spawnFlag = true;
	private int normalCount = 0;
	private int veryHighCount = 0;
	private int highCount = 0;
	private Picture LatestTempt;
    private GameObject latestObject;
    private bool isAnimationReady;
    //private CancellationTokenSource cancelSwapAnimation;

    private void Awake()
	{
		PhotoWallEvents.OnInitPhotoAdded += ProcessImages;
		PhotoWallEvents.OnPhotoLoaded += ProcessImages;
        PhotoWallEvents.OnDataUpdated += UpdateImages;
        PhotoWallEvents.OnPhotoDeleted += RemoveImage;
	}
	
	private void OnDestroy()
	{
		PhotoWallEvents.OnInitPhotoAdded -= ProcessImages;
		PhotoWallEvents.OnPhotoLoaded -= ProcessImages;
        PhotoWallEvents.OnDataUpdated += UpdateImages;
        PhotoWallEvents.OnPhotoDeleted -= RemoveImage;
	}
	
	private void Start()
	{
		api = APIManager.Instance;
		manager = PhotoWallManager.Instance;
	}
	
	public void Activate()
	{
		IsPlaying = !IsPlaying;

		switch (IsPlaying)
		{
			case true:
				canvasGroup.DOFade(1f, 0.3f);
				StartAnimation();
				break;
			case false:
				canvasGroup.DOFade(0f, 0.3f);
				StopAnimation();
				break;
		}
	}
	public void Hide()
	{
		this.canvasGroup.alpha = 0;
	}
	
	public void StartAnimation()
	{
        Debug.Log("startanimation");

        //cancelSwapAnimation = new CancellationTokenSource();
        StartCoroutine(swapPriorityPicture());
        
        isAnimationReady = true;
	}
	
	public void StopAnimation()
	{
        Debug.Log("stopanimation");
        isAnimationReady = false;
        StopAllCoroutines();
        //cancelSwapAnimation.Cancel();
    }

    private void UpdateImages(string id, int priority)
    {
        StopAnimation();

        //Debug.Log("update img");

        Picture normalPic = Normal.Find(x => x._id == id);
        Picture highPic = High.Find(x => x._id == id);
        Picture veryHighPic = VeryHigh.Find(x => x._id == id);

        if(normalPic != null)
        {
            //Debug.Log("normal update");
            normalPic.priority = priority;
            if (priority == 1)
            {
                Normal.Remove(normalPic);
                High.Add(normalPic);
            }
            else if(priority == 2)
            {
                Normal.Remove(normalPic);
                VeryHigh.Add(normalPic);
            }
        } 
        if(highPic != null)
        {
            //Debug.Log("high update");
            highPic.priority = priority;
            if (priority == 0)
            {
                High.Remove(highPic);
                Normal.Add(highPic);
            }
            else if (priority == 2)
            {
                High.Remove(highPic);
                VeryHigh.Add(highPic);
            }
        }
        if(veryHighPic != null)
        {
            //Debug.Log("veryhigh update");
            veryHighPic.priority = priority;
            if (priority == 0)
            {
                VeryHigh.Remove(veryHighPic);
                Normal.Add(veryHighPic);
            }
            else if (priority == 1)
            {
                VeryHigh.Remove(veryHighPic);
                High.Add(veryHighPic);
            }
        }

        foreach(GameObject x in notMoving)
        {
            Destroy(x);
        }

        foreach(GameObject x in moving)
        {
            Destroy(x);
        }

        Destroy(latestObject);

        LatestTempt = null;

        normalCount = 0;
        highCount = 0;
        veryHighCount = 0;
        spawnFlag = true;
        notMoving.Clear();
        moving.Clear();

        //int a = 0;

        foreach(Picture x in allPhotos)
        {
            //a++;
            spawnPictureAfterReset(x);
        }

        //Debug.Log("loop spawn after reset : " + a);

        StartAnimation();
    }

    public override void ProcessImages(Picture photo)
	{
        arrangeList(photo);

        spawnPictureAfterReset(photo);
    }

    private IEnumerator swapPriorityPicture()
    {
        yield return new WaitForSeconds(5);

        //await Task.Delay(5000);

        //Debug.Log("5 detik untuk mengganti");

        foreach (GameObject x in notMoving)
        {
            //Debug.Log("destroy notmoving");
            Destroy(x);
        }

        notMoving.Clear();

        int loop = 0;

        for (int a = 0; a <= VeryHigh.Count - 1; a++)
        {
            if (veryHighCount == VeryHigh.Count - 1)
            {
                //Debug.Log("masukVHigh");
                veryHighCount = 0;
                a = 0;
            }
            if (a == veryHighCount)
            {
                if (loop == 2)
                {
                    break;
                }
                if (VeryHigh[a] != LatestTempt)
                {
                    //Debug.Log("loop " + loop);
                    Picture _photo = Instantiate(VeryHigh[a], VeryHighPriority[loop]);
                    _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
                    notMoving.Add(_photo.gameObject);
                    //veryHighCount++;
                    loop++;
                }
                veryHighCount++;
            }
        }

        loop = 0;

        for (int a = 0; a <= High.Count - 1; a++)
        {
            if (highCount == High.Count - 1)
            {
                //Debug.Log("masukHigh");
                highCount = 0;
                a = 0;
            }
            if (a == highCount)
            {
                if (loop == 2)
                {
                    break;
                }
                //Debug.Log("highcount : " + highCount + " high.count : " + High.Count);
                if (High[a] != LatestTempt)
                {
                    //Debug.Log("highcount 2 : " + highCount + " high.count 2 : " + High.Count);
                    Picture _photo = Instantiate(High[a], HighPriority[loop]);
                    _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
                    notMoving.Add(_photo.gameObject);
                    loop++;
                }
                highCount++;
            }
        }

        yield return new WaitForSeconds(5);

        //await Task.Delay(5000);

        //Debug.Log("5 detik menunggu");
        StartCoroutine(swapPriorityPicture());

        //swapPriorityPicture();
    }

    private void Update()
    {
        if (!isAnimationReady) return;

        animateHorizontal();
        animateVertical();
    }

    private void animateHorizontal()
    {
        if (Horizontal.gameObject.GetComponent<RectTransform>().anchoredPosition.x > Horizontal.gameObject.GetComponent<RectTransform>().rect.width) Horizontal.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3360, Horizontal.gameObject.GetComponent<RectTransform>().anchoredPosition.y);
        Horizontal.position += new Vector3(3f, 0f, 0f);
    }

    private void animateVertical()
    {
        if (Vertical.gameObject.GetComponent<RectTransform>().anchoredPosition.y > Vertical.gameObject.GetComponent<RectTransform>().rect.height) Vertical.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(Vertical.gameObject.GetComponent<RectTransform>().anchoredPosition.x, -1350);
        Vertical.position += new Vector3(0f, 3f, 0f);
    }

    private void spawnPictureAfterReset(Picture photo)
    {
        if (LatestTempt == null)
        {
            LatestTempt = photo;
            Picture _photo = Instantiate(photo, Latest);
            latestObject = _photo.gameObject;
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            return;
        }
        else if (string.Compare(LatestTempt.createdAt, photo.createdAt) == -1)
        {
            Destroy(latestObject);
            LatestTempt = photo;
            Picture _photo = Instantiate(photo, Latest);
            latestObject = _photo.gameObject;
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            return;
        }

        if (photo.priority == 0)
        {
            spawnMoving(photo);
        }
        else
        {
            spawnNotMoving(photo);
        }
    }

    private void spawnMoving(Picture photo)
    {
        if (spawnFlag)
        {
            Picture _photo = Instantiate(photo, Horizontal);
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            moving.Add(_photo.gameObject);
        }
        else if (!spawnFlag)
        {
            Picture _photo = Instantiate(photo, Vertical);
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            moving.Add(_photo.gameObject);
        }
        normalCount++;

        if (normalCount == 10)
        {
            spawnFlag = !spawnFlag;
        }
    }

    private void spawnNotMoving(Picture photo)
    {
        if (photo.priority == 2)
        {
            if (veryHighCount == 2) return;
            Picture _photo = Instantiate(photo, VeryHighPriority[veryHighCount]);
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            veryHighCount++;
            notMoving.Add(_photo.gameObject);
        }
        else if (photo.priority == 1)
        {
            if (highCount == 2) return;
            Picture _photo = Instantiate(photo, HighPriority[highCount]);
            _photo.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
            highCount++;
            notMoving.Add(_photo.gameObject);
        }
    }

	public override void RemoveImage(string id)
	{
        StopAnimation();

        Picture _photo = null;

        foreach(Picture x in allPhotos)
        {
            if(x._id == id)
            {
                _photo = x;
                break;
            }
        }

        if (_photo == null) return;

        if(_photo == LatestTempt)
        {
            Destroy(latestObject);
            LatestTempt = null;
            allPhotos.Remove(_photo);

            foreach(Picture x in allPhotos)
            {
                if (LatestTempt == null)
                {
                    LatestTempt = x;
                    Picture _photos = Instantiate(x, Latest);
                    latestObject = _photos.gameObject;
                    _photos.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
                }
                else if (string.Compare(LatestTempt.createdAt, x.createdAt) == -1)
                {
                    Destroy(latestObject);
                    LatestTempt = x;
                    Picture _photos = Instantiate(x, Latest);
                    latestObject = _photos.gameObject;
                    _photos.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 350);
                }
            }
        }

        if(_photo.priority == 0)
        {
            Normal.Remove(_photo);

            foreach(GameObject x in notMoving)
            {
                Destroy(x);
            }
            notMoving.Clear();

            spawnFlag = true;
            normalCount = 0;

            foreach(Picture x in Normal)
            {
                if(x != LatestTempt) spawnMoving(x);
            }
        }
        else
        {
            if(_photo.priority == 1)
            {
                High.Remove(_photo);
            }
            else if(_photo.priority == 2)
            {
                VeryHigh.Remove(_photo);
            }

            foreach (GameObject x in notMoving)
            {
                Destroy(x);
            }

            notMoving.Clear();

            highCount = 0;
            veryHighCount = 0;

            foreach(Picture x in High)
            {
                if (x != LatestTempt) spawnNotMoving(x);
            }

            foreach (Picture x in VeryHigh)
            {
                if (x != LatestTempt) spawnNotMoving(x);
            }
        }

        StartAnimation();
	}

	private void arrangeList(Picture photo)
    {
        allPhotos.Add(photo);

        if (photo.priority == 2)
        {
            VeryHigh.Add(photo);
        }
        else if (photo.priority == 1)
        {
            High.Add(photo);
        }
        else if (photo.priority == 0)
        {
            Normal.Add(photo);
        }
    }
}