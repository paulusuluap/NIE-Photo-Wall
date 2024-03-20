using UnityEngine;

public class ShortcutController : MonoBehaviour
{
    int index = 0;

    private void Start()
    {
        Cursor.visible = false;

        InvokeRepeating("DisableCursor", 5f, 5f);
    }

    void Update()
    {
        if (!PhotoWallManager.Instance.IsPhotoWallReady)
            return;

        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            ChangeScreenResolution();
        }
    }

    private void ChangeScreenResolution()
    {
        //switching between windowed 1920x1080, windowed 1536x864, and fullsceen
        switch (index)
        {
            case 0:
                Screen.SetResolution(3360, 1350, false);
                index++;
                break;
            case 1:
                Screen.SetResolution(1680, 675, false);
                index++;
                break;
            case 2:
                Screen.SetResolution(3360, 1350, true);
                index = 0;
                break;
        }
    }

    private void DisableCursor()
    {
        if (Cursor.visible)
            Cursor.visible = false;
    }

    void ScreenShot()
    {
        // capture screen shot on left mouse button down

        string folderPath = "Assets/Screenshots/"; // the path of your project folder

        if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
            System.IO.Directory.CreateDirectory(folderPath);  // it will get created

        var screenshotName =
                                "Screenshot_" +
                                System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
                                ".png"; // put youre favorite data format here
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 2); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
        Debug.Log(folderPath + screenshotName); // You get instant feedback in the console
    }
}
