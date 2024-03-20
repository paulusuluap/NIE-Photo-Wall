using UnityEngine;

public class ActivateDisplay : MonoBehaviour
{
    private void Start()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            print("activate");
        }
    }
}