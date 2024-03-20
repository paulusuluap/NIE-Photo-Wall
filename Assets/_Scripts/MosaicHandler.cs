using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using sunny;
using System.IO;

public class MosaicHandler : MonoBehaviour
{
    public bool loop;
    public bool isInitialized;
    public PhotoWallMosaic mosaic;
    private PhotoWallDataset dataSet;
    public PhotoWallDataset presetDataSet;
    public Texture2D targetTexture;
    private Dictionary<GameObject, float> animationData;
    public float tweenDuration = 2f;
    public float loopInterval = 120f;
    private Texture2D picked;

    // Start is called before the first frame update
    private void OnEnable()
    {
        DatasetGenerator.OnDatasetReady += Init;
    }

    private void OnDisable()
    {
        DatasetGenerator.OnDatasetReady -= Init;
    }

    void Init(PhotoWallDataset _dataset)
    {
        animationData = new Dictionary<GameObject, float>();
        presetDataSet = _dataset;
        dataSet = _dataset;
        picked = targetTexture;
        mosaic.generate(targetTexture, dataSet);
        mosaic.onSpawn += onSpawn;

        if (loop)
        {
            //InvokeRepeating("LoadMosaic", loopInterval, loopInterval);
            StartCoroutine(LoadMosaicCoroutine());
        }

        isInitialized = true;
    }

    void onSpawn(GameObject piece, PhotoWallDataset.TextureData data)
    {
        animationData.Add(piece, 0);
    }

    void LateUpdate()
    {
        if (!isInitialized)
            return;

        List<GameObject> keys = new List<GameObject>();
        keys.AddRange(animationData.Keys);
        foreach (GameObject key in keys)
        {
            animationData[key] = Mathf.Clamp01(animationData[key] + Time.deltaTime / tweenDuration);
            key.transform.localPosition = new Vector3(key.transform.localPosition.x, key.transform.localPosition.y,
                Mathf.Lerp(120, 0, animationData[key]));
        }
    }

    void LoadMosaic()
    {
        animationData.Clear();
        picked = targetTexture;
        mosaic.generate(targetTexture, dataSet);
    }

    public void ReloadMosaic()
    {
        StopAllCoroutines();
        StartCoroutine(LoadMosaicCoroutine());
    }

    private IEnumerator LoadMosaicCoroutine()
    {
        animationData.Clear();
        picked = targetTexture;
        mosaic.generate(targetTexture, dataSet);

        yield return new WaitForSeconds(loopInterval);

        StartCoroutine(LoadMosaicCoroutine());
    }
}