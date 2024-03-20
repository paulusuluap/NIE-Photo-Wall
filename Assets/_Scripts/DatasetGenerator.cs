using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using UnityEditor;
using sunny;

public class DatasetGenerator : MonoBehaviour
{
    public class PhotoWallConfig
    {
        public string mosaicFolderPath;
        public float loopInterval;
    }

    public MosaicHandler mosaic;
    public PhotoWallConfig photoWallConfig;
    public PhotoWallDataset dataset;
    public string mosaicFolderPath;
    public string datasetPath;
    public float ratio = 1;
    public List<string> list = new List<string>();

    protected IEnumerator current;
    public static Action<PhotoWallDataset> OnDatasetReady;

    private void Awake()
    {
        photoWallConfig = new PhotoWallConfig();
    }

    private void Start()
    {
        ConfigureSettings();
    }

    private void ConfigureSettings()
    {
        string configFileTxt = File.ReadAllText(Application.streamingAssetsPath + "/config.json");
        photoWallConfig = JsonUtility.FromJson<PhotoWallConfig>(configFileTxt);
        mosaicFolderPath = photoWallConfig.mosaicFolderPath;
        mosaic.loopInterval = photoWallConfig.loopInterval;

        //Get all Photos path
        var folderPath = mosaicFolderPath;
        if (!string.IsNullOrEmpty(folderPath))
        {
            //Debug.Log(folderPath);
            foreach (string filepath in Directory.GetFiles(folderPath))
            {
                string ext = Path.GetExtension(filepath).ToLower();
                if (ext == ".jpg" || ext == ".png")
                {
                    list.Add(filepath);
                }
            }
        }

        if (string.IsNullOrEmpty(datasetPath))
        {
            print("dataset path is null");
            return;
        }

        GenerateDataSet(ratio, list.ToArray(), datasetPath);
    }

    protected void GenerateDataSet(float ratio, string[] filePaths, string datasetPath)
    {
        current = DoGenerateDataSetRuntime(ratio, filePaths, datasetPath);
        StartCoroutine(current);
    }

    private IEnumerator DoGenerateDataSetRuntime(float ratio, string[] filePaths, string datasetPath)
    {
        //Create and set dataSet
        dataset._ratio = ratio;
        dataset.textures = new List<PhotoWallDataset.TextureData>();
        dataset.makeCache();
        
        Texture2D tex = new Texture2D(2, 2);
        byte[] b;
        for (int i = 0; i < filePaths.Length; i++)
        {
            string filepath = filePaths[i];

            yield return null;

            b = File.ReadAllBytes(filepath);
            tex.LoadImage(b);
            tex.Apply();
            dataset.addTexture(tex);
        }

        yield return null;

        //Set texture
        tex = new Texture2D(dataset.cache.width, dataset.cache.height, TextureFormat.RGB24, false);
        RenderTexture.active = dataset.cache;
        tex.ReadPixels(new Rect(0, 0, dataset.cache.width, dataset.cache.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        //Create _tex png
        //string pngPath = Path.GetDirectoryName(datasetPath) + "/" + Path.GetFileNameWithoutExtension(datasetPath) + "_tex.png";
        string pngPath = Application.streamingAssetsPath + "/Dataset/PhotoWall_tex.png";
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());

        pngPath = MakeRelative(pngPath, Path.GetFullPath("./"));

        yield return null;

        //Texture2D finalPng = Resources.Load<Texture2D>($"Dataset/{Path.GetFileNameWithoutExtension(datasetPath)}_tex");
        byte[] pngBytes = File.ReadAllBytes(pngPath);
        Texture2D finalPng = new Texture2D(2, 2);
        finalPng.LoadImage(pngBytes);

        dataset.makeIndexMap(4);
        for (int i = 0; i < dataset.textures.Count; i++)
        {
            dataset.textures[i].texture = finalPng;
        }
        dataset.cache = null;
        dataset.cache2D = finalPng;

        yield return null;

        //Create map texture
        //string indexPath = Path.GetDirectoryName(datasetPath) + "/" + Path.GetFileNameWithoutExtension(datasetPath) + "_map.png";
        string indexPath = Application.streamingAssetsPath + "/Dataset/PhotoWall_map.png";
        File.WriteAllBytes(indexPath, dataset.indexMap.EncodeToPNG());

        indexPath = MakeRelative(indexPath, Path.GetFullPath("./"));
        //dataset.indexMap = Resources.Load<Texture2D>($"Dataset/{Path.GetFileNameWithoutExtension(datasetPath)}_map");

        byte[] mapBytes = File.ReadAllBytes(indexPath);
        Texture2D mapPng = new Texture2D(2, 2);
        mapPng.LoadImage(pngBytes);
        dataset.indexMap = mapPng;

        OnDatasetReady?.Invoke(dataset);
    }

    //private IEnumerator DoGenerateDataSet(float ratio, string[] filePaths, string datasetPath)
    //{
    //    //Create and set dataSet SO
    //    PhotoMosaicDataSet dataset = ScriptableObject.CreateInstance<PhotoMosaicDataSet>();
    //    dataset._ratio = ratio;
    //    dataset.textures = new List<PhotoMosaicDataSet.TextureData>();
    //    dataset.makeCache();
    //    //
    //    Texture2D tex = new Texture2D(2, 2);
    //    byte[] b;
    //    for (int i = 0; i < filePaths.Length; i++)
    //    {
    //        string filepath = filePaths[i];

    //        yield return null;

    //        b = File.ReadAllBytes(filepath);
    //        tex.LoadImage(b);
    //        tex.Apply();
    //        dataset.addTexture(tex);
    //    }

    //    yield return null;

    //    //Set texture
    //    tex = new Texture2D(dataset.cache.width, dataset.cache.height, TextureFormat.RGB24, false);
    //    RenderTexture.active = dataset.cache;
    //    tex.ReadPixels(new Rect(0, 0, dataset.cache.width, dataset.cache.height), 0, 0);
    //    tex.Apply();
    //    RenderTexture.active = null;
    //    string pngPath = Path.GetDirectoryName(datasetPath) + "/" + Path.GetFileNameWithoutExtension(datasetPath) + "_tex.png";
    //    File.WriteAllBytes(pngPath, tex.EncodeToPNG());
    //    pngPath = MakeRelative(pngPath, Path.GetFullPath("./"));

    //    yield return null;

    //    AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
    //    TextureImporter pngTi = (TextureImporter)TextureImporter.GetAtPath(pngPath);
    //    pngTi.mipmapEnabled = false;
    //    AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
    //    Texture2D finalPng = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
    //    print("pngPath : " + pngPath);
    //    //Texture2D finalPng = Resources.Load<Texture2D>($"Dataset/{Path.GetFileNameWithoutExtension(datasetPath)}_tex.png");

    //    //TEST

    //    dataset.makeIndexMap(4);
    //    for (int i = 0; i < dataset.textures.Count; i++)
    //    {
    //        dataset.textures[i].texture = finalPng;
    //    }
    //    dataset.cache = null;
    //    dataset.cache2D = finalPng;

    //    yield return null;

    //    //Create map texture
    //    string indexPath = Path.GetDirectoryName(datasetPath) + "/" + Path.GetFileNameWithoutExtension(datasetPath) + "_map.png";
    //    File.WriteAllBytes(indexPath, dataset.indexMap.EncodeToPNG());

    //    indexPath = MakeRelative(indexPath, Path.GetFullPath("./"));
    //    AssetDatabase.ImportAsset(indexPath, ImportAssetOptions.ForceUpdate);
    //    TextureImporter indexTi = TextureImporter.GetAtPath(indexPath) as TextureImporter;
    //    indexTi.mipmapEnabled = false;
    //    indexTi.textureCompression = TextureImporterCompression.CompressedLQ;
    //    indexTi.filterMode = FilterMode.Point;
    //    print("indexMap : " + indexPath);
    //    dataset.indexMap = AssetDatabase.LoadAssetAtPath<Texture2D>(indexPath);
    //    //dataset.indexMap = Resources.Load<Texture2D>($"Dataset/{Path.GetFileNameWithoutExtension(datasetPath)}_map.png");
    //    AssetDatabase.ImportAsset(indexPath, ImportAssetOptions.ForceUpdate);
    //    AssetDatabase.CreateAsset(dataset, MakeRelative(datasetPath, Path.GetFullPath("./")));
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //    Selection.activeObject = dataset;

    //    mosaic.presetDataSet = dataset;

    //    OnDatasetReady?.Invoke();
    //}

    public static string MakeRelative(string filePath, string referencePath)
    {
        var fileUri = new System.Uri(filePath);
        var referenceUri = new System.Uri(referencePath);
        return referenceUri.MakeRelativeUri(fileUri).ToString();
    }
}
