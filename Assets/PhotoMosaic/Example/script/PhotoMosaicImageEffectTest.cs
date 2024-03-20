using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using sunny;

public class PhotoMosaicImageEffectTest : MonoBehaviour {
	public bool demostrateLoading;
	public PhotoMosaicImageEffect mosaic;
	private PhotoMosaicDataSet dataSet;
	public PhotoMosaicDataSet presetDataSet;

	void Start(){
		if(demostrateLoading){
			dataSet = new PhotoMosaicDataSet(1,true);
			StartCoroutine("startLoading");
		}else{
			mosaic.dataSet =presetDataSet;
		}
	}


	IEnumerator startLoading(){
		foreach(string filePath in Directory.GetFiles(Application.streamingAssetsPath+"/PhotoMosaicExample")){
			string ext = Path.GetExtension(filePath).ToLower();
			if(ext == ".png" || ext == ".jpg"  || ext == ".gif" ){
    //            WWW www = new WWW("File://" + filePath);
    //            yield return www;
				//print(www.url);
				//dataSet.addTexture(www.texture);

                using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("File://" + filePath))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result == UnityWebRequest.Result.Success)
                    {
                        var texture = DownloadHandlerTexture.GetContent(uwr);
                        dataSet.addTexture(texture);
                    }
                }
			}
		}
		StartCoroutine( dataSet.makeIndexMapAsync(5, delegate() {
			mosaic.dataSet = dataSet;
		}));

	}
}
