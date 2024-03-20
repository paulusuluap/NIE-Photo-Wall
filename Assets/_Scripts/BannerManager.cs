using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class BannerManager : MonoBehaviour
{
    public static BannerManager instance;

    [SerializeField]
    RectTransform banner; 
    [SerializeField]
    TextMeshProUGUI bannerTxt;

    public TextMeshProUGUI BannerTxt
    {
        get => bannerTxt; 
        set => bannerTxt = value;
    }

    private void Awake()
    {
        instance = this;
    }

    public void SetBanner(int type)
    {
        //print("SetBannerForGallery");
        switch (type)
        {
            case 0: banner.gameObject.SetActive(true); break;
            case 2: banner.gameObject.SetActive(true); break;
            case 4: banner.gameObject.SetActive(true); break;
            case 7: banner.gameObject.SetActive(true); break;
            default: banner.gameObject.SetActive(false); break;
        }
        banner.anchoredPosition = type switch
        {
            0 => Constants.POSITION_BANNER_4x2,
            2 => Constants.POSITION_BANNER_4x3,
            4 => Constants.POSITION_BANNER_5x3,
            7 => Constants.POSITION_BANNER_6x4,
            _ => Constants.POSITION_BANNER_5x3
        };
        banner.sizeDelta = type switch
        {
            0 => Constants.SIZE_BANNER_4x2,
            2 => Constants.SIZE_BANNER_4x3,
            4 => Constants.SIZE_BANNER_5x3,
            7 => Constants.SIZE_BANNER_6x4,
            _ => Constants.SIZE_BANNER_5x3
        };
        bannerTxt.fontSizeMax = type switch
        {
            0 => Constants.FONT_4x,
            2 => Constants.FONT_4x,
            4 => Constants.FONT_5x,
            7 => Constants.FONT_6x,
            _ => Constants.FONT_5x
        };
    }

    public void SetBannerByAxis(Enums.MovementAxis axis)
    {
        //print("SetBannerByAxis");
        banner.gameObject.SetActive(true);
        banner.anchoredPosition = axis switch
        {
            Enums.MovementAxis.Horizontal => Constants.POSITION_BANNER_HORIZONTAL,
            Enums.MovementAxis.Vertical => Vector3.zero,
        };
        banner.sizeDelta = axis switch
        {
            Enums.MovementAxis.Horizontal => Constants.SIZE_BANNER_HORIZONTAL,
            Enums.MovementAxis.Vertical => Vector3.zero,
        };
        bannerTxt.fontSizeMax = axis switch
        {
            Enums.MovementAxis.Horizontal => Constants.FONT_HORIZONTAL,
            Enums.MovementAxis.Vertical => Constants.FONT_6x,
        };
    }

    public void DeactivateBanner()
    {
        banner.gameObject.SetActive(false);
    }
}
