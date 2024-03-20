using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
//using OpenCVForUnity.UnityUtils.Helper;
using Proyecto26;
using ZXing;
using ZXing.QrCode;

public class PhotoBoothHandler : MonoBehaviour
{
    #region APIs

    [Serializable]
    public class APIPhoto
    {
        public bool status;
        public Data photo;
    }
    [Serializable]
    public class Data //will be used for QR Code
    {
        public string id;
        public string title;
        public string image;
        public string image_thumb;
        public string description;
        public string date_created;
        public string date_modified;
    }

    public class Body
    {
        public string image;
    }

    #endregion


    #region Fields

    [SerializeField]
    string baseUrl = "https://photobooth.fxwebapps.com";
    [SerializeField]
    string submitEndpoint = "/api/photo/";
    [SerializeField]
    string group = "booth";
    public APIPhoto apiPhoto = new APIPhoto();

    [SerializeField]
    UIHandler uiHandler;
    [SerializeField]
    Image photoDisplayArea;
    [SerializeField]
    RawImage qrCodePattern;
    [SerializeField]
    Animator countdownAnimation;
    [SerializeField]
    Animator fadeInAnimation;
    [SerializeField]
    Animator fadeOutAnimation;
    [SerializeField]
    Animator fadingAnimation;
    [SerializeField]
    GameObject photoFrame;
    [SerializeField]
    GameObject qrCode;

    private Texture2D screenCapture;
    //private WebCamTextureToMatHelper webcam;
    private byte[] textureBytes; 
    private int countdownTimer = 3;
    private bool isCountdownStart;

    #endregion

    private void Start()
    {
        //webcam = FindObjectOfType<WebCamTextureToMatHelper>();
        //screenCapture = new Texture2D(webcam.requestedWidth, webcam.requestedHeight, TextureFormat.RGBA32, false);
        //webcam.Stop();

        //Debug.Log(Application.dataPath);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            var devices = WebCamTexture.devices;
            print(WebCamTexture.devices[1].availableResolutions.Length);
            //foreach (var dev in WebCamTexture.devices)
            //{
            //    print(dev.name);
            //    foreach (var res in dev.availableResolutions)
            //    {
            //        print($"{dev.name} available resolutions : {res.width}x{res.height}");
            //    }
            //}

            //for (int i = 0; i < devices.Length; i++)
            //{
            //    Debug.Log(i.ToString() + " " + devices[i].name);

            //    //for (int j = 0; j < devices[0].availableResolutions.Length; j++)
            //    //{
            //    //    Debug.Log("width : " + devices[0].availableResolutions[0].width);
            //    //}
            //}

        }
    }

    public void CapturePhoto()
    {
        StartCoroutine(CapturePhotoCoroutine());
    }

    private IEnumerator CapturePhotoCoroutine()
    {
        yield return new WaitForEndOfFrame();

        //screenCapture.SetPixels(webcam.WebCamTexture.GetPixels());
        screenCapture.Apply();
        //textureBytes = FlipTexture(screenCapture).EncodeToPNG();
        textureBytes = ImageConversion.EncodeToPNG(FlipTexture(screenCapture));
        string encodePhotoBytes = Convert.ToBase64String(textureBytes);
        //print(encodePhotoBytes);
        File.WriteAllBytes(Application.dataPath + "/../PIX.png", textureBytes);
        //TODO : Send Post request to Server
        PostPhoto(encodePhotoBytes);
    }
    
    public void StartCountdown()
    {
        if(!isCountdownStart)
        {
            isCountdownStart = true;
            StartCoroutine(Countdown());
        }
    }

    private IEnumerator Countdown()
    {
        while (countdownTimer > 0)
        {
            uiHandler.countdownDisplay.text = countdownTimer.ToString();
            //countdownAnimation.Play("Countdown");

            yield return new WaitForSeconds(1f);

            countdownTimer--;
        }

        uiHandler.countdownDisplay.text = "SMILE";

        yield return new WaitForSeconds(1f);

        fadeInAnimation.Play("FadeIn");

        //Capture photo after countdown ends
        CapturePhoto();

        yield return new WaitForSeconds(0.85f);

        uiHandler.HandleObject(uiHandler.photoEditing);
    }

    public void RemovePhoto() //to retake photo
    {
        photoFrame.SetActive(false);
        qrCode.SetActive(false);
        apiPhoto = null;
        photoDisplayArea.sprite = null;
        qrCodePattern.texture = null;
        uiHandler.countdownDisplay.text = "";
        countdownTimer = 3;
        uiHandler.fadePanel.alpha = 0;
        isCountdownStart = false;

        //webcam.Play();
    }
    private void PostPhoto(string encodedPhotoData)
    {
        RestClient.Post<APIPhoto>(baseUrl + submitEndpoint + $"submit?group={group}", new Body
        {
            image = encodedPhotoData
        }
        ).Then(response => 
        {
            if(response.status)
            {
                print("Post Request Successfull : " + response.status);
                apiPhoto = response;

                ShowPhoto(response.photo.id);
            }
            else
            {
                print("Post Request ERROR");
            }
        });
    }

    private void ShowPhoto(string photoId)
    {
        Sprite photoSprite = Sprite.Create(FlipTexture(screenCapture), new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height), new Vector2(0.5f, 0.5f), 100.0f);
        photoDisplayArea.sprite = photoSprite;

        //TODO : Set Barcode Texture
        qrCodePattern.texture = GenerateQR(baseUrl + $"/description?id={photoId}&group={group}");

        photoFrame.SetActive(true);
        qrCode.SetActive(true);
        fadingAnimation.Play("PhotoFade");

        //webcam.Stop();
    }

    Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
            }
        }
        flipped.Apply();

        return flipped;
    }


    //TODO : Generate QR Code in Unity
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    public Texture2D GenerateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }
}