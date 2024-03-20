using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    public GameObject landingPage;
    public GameObject photoBooth;
    public GameObject photoEditing;
    public GameObject tutorial_1;
    public GameObject tutorial_2;
    public CanvasGroup fadePanel;

    public TextMeshProUGUI countdownDisplay;

    /*STEPS
     * 1. landing page open press Start
     * 2. photoBooth is showing
     *    - tutorial 1 begin, press anywhere
     *    - tutorial 2 begin, press anywhere
     *    - take photo and countdown
     * 3. photo editing is opened
     * 4. click retake button to restart from number 1
     */
    private void Start()
    {
        HandleObject(landingPage);
    }

    public void HandleObject(GameObject obj)
    {
        landingPage.SetActive(landingPage == obj);
        photoBooth.SetActive(photoBooth == obj);
        photoEditing.SetActive(photoEditing == obj);

        if(landingPage.activeInHierarchy)
        {
            //reset tutorial
            tutorial_1.gameObject.SetActive(true);
            tutorial_2.gameObject.SetActive(false);
            countdownDisplay.text = "";
        }
    }
}
