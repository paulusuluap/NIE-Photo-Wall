using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GridAnimation : Animation, IAnimation
{
	public CanvasGroup canvasGroup;
	public Transform content;

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
        PhotoWallEvents.OnDataUpdated -= UpdateImages;
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
		Debug.Log("startAnimation2");
		BannerManager.instance.SetBanner(4);
	}
	
	public void StopAnimation()
	{
		
	}

	private void UpdateImages(string id, int priority)
	{
		foreach(Transform photo in content.transform)
		{
			if(photo.GetComponent<Picture>()._id == id)
			{
				if(priority > 0)
				{
					photo.SetSiblingIndex(0);
				}
				else
				{
					photo.SetSiblingIndex(content.transform.childCount - 1);
				}
				photo.GetComponent<Picture>()._id = id;
				print("new priority" + priority);
			}
		}
	}


    public override void ProcessImages(Picture photo)
	{
		Picture _photo = Instantiate(photo, content);

		if(photo.priority > 0)
		{
			_photo.transform.SetSiblingIndex(0);
		}
	}

	public override void RemoveImage(string id)
	{
		foreach(Transform photo in content)
		{
			if(photo.GetComponent<Picture>()._id == id)
			{
				Destroy(photo.gameObject);
			}
		}
	}
}