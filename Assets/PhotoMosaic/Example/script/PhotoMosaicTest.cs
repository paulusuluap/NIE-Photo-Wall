using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using sunny;

public class PhotoMosaicTest : MonoBehaviour {
	public bool demostrateLoading;
	public PhotoMosaic mosaic;
	private PhotoMosaicDataSet dataSet;
	private List<Texture2D> textures;
	public PhotoMosaicDataSet presetDataSet;
	public Texture2D targetTexture;
	private Dictionary<GameObject,float> animationData;
	public float tweenDuration = 4;

	void Start()
    {
		animationData = new Dictionary<GameObject, float>();

		if(demostrateLoading)
        {
			dataSet = new PhotoMosaicDataSet(1);
			textures = new List<Texture2D>();
			foreach(Texture2D tex in Resources.LoadAll<Texture2D>("MosaicExample")){

				dataSet.addTexture(tex);
				textures.Add(tex);
			}
			generateOne();
		}
        else
        {
			dataSet = presetDataSet;
			picked = targetTexture;
			mosaic.generate(targetTexture,dataSet);
		}

		mosaic.onSpawn += onSpawn;
	}

	void onSpawn (GameObject piece, PhotoMosaicDataSet.TextureData data)
    {
		animationData.Add(piece,0);
	}


	void LateUpdate(){
		List<GameObject> keys = new List<GameObject>();
		keys.AddRange(animationData.Keys);
		foreach(GameObject key in keys){
			animationData[key]= Mathf.Clamp01(animationData[key]+Time.deltaTime/tweenDuration);
			key.transform.localPosition = new Vector3(key.transform.localPosition.x,key.transform.localPosition.y,
				Mathf.Lerp(120,0,animationData[key]));
		}
	}
	Texture2D picked;
	void OnGUI(){
		if(picked!=null){
			GUI.DrawTexture(new Rect(0,0,100,100),picked);

		}
		if(demostrateLoading){
			if(GUI.Button(new Rect(Screen.width-100,Screen.height-60,100,60),"Random")){
				generateOne();
			}
			GUI.color = Color.black;
		}else{
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 60, 100, 60), "Replay"))
            {
                animationData.Clear();
                picked = targetTexture;
                mosaic.generate(targetTexture, dataSet);
            }
        }
	}
	public void generateOne(){
		animationData.Clear();
		picked = textures[Random.Range(0,textures.Count)];
		mosaic.generate(picked,dataSet);
	}
}
