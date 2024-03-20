using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace sunny{
	public class PhotoMosaicDataSetWizard:EditorWindow {
		public static PhotoMosaicDataSetWizard currentWindow;
		public List<string> list=new List<string>();
		public float ratio = 1;
		public Vector2 scrollPos ;

		static void openWindow(List<string> addPaths = null){

			PhotoMosaicDataSetWizard window = (PhotoMosaicDataSetWizard)EditorWindow.GetWindow (
				typeof (PhotoMosaicDataSetWizard), true,"PhotoMosaicDataSetWizard" 
			);
			currentWindow = window;
			if(addPaths!=null){
				window.list.AddRange(addPaths);
			}
			window.Show();

		}
		void OnGUI () {
			EditorGUILayout.BeginHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false,true, GUILayout.ExpandHeight(true));
			if(list.Count==0){
				GUILayout.Label("0 image selected");
			}else{
				foreach(string path in list){
					GUILayout.Label(path);
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.BeginVertical(GUILayout.Width(100));
			if(GUILayout.Button("Add Whole Folder"))
			{
                //1
				string folderPath = EditorUtility.OpenFolderPanel("Load Image Folder","Assets","");
				if(!string.IsNullOrEmpty(folderPath)){
                    Debug.Log(folderPath);
					foreach(string filepath in Directory.GetFiles(folderPath)){
						string ext = Path.GetExtension(filepath).ToLower();
						if(ext == ".jpg" ||ext  == ".png"){
							list.Add(filepath);
						}
					}
				}
			}
			if(GUILayout.Button("Add One File"))
			{
				string filepath = EditorUtility.OpenFilePanel("Load Image","Assets","");
				string ext = Path.GetExtension(filepath).ToLower();
				if(ext == ".jpg" ||ext  == ".png"){
					list.Add(filepath);
				}

			}
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Clear"))
			{
				list.Clear();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUI.enabled = (list.Count>0);
			if(GUILayout.Button("Generate Dataset"))
			{
				string datasetPath =  EditorUtility.SaveFilePanel("Save As","Assets","undefined","asset");
                Debug.Log("datasetPath : " + datasetPath);
				if(string.IsNullOrEmpty(datasetPath)){
					return;
				}
				generateDataSet(ratio,list.ToArray(),datasetPath);
				Close();
			}
		}

		[MenuItem ("Assets/Generate PhotoMosaicDataSet")]
		static void generateDataSetWithAsset() {
			List<string> filePaths = new List<string>();
			Object[] objList =  Selection.GetFiltered(typeof(Texture2D),SelectionMode.Assets);
			for (int i = 0; i < objList.Length; ++i)
			{ 
				Object obj = objList[i];
				string szPath = AssetDatabase.GetAssetPath(obj);
				filePaths.Add(szPath);
			}
			if(currentWindow)currentWindow.list.AddRange(filePaths);
			else openWindow(filePaths);
		}

		[MenuItem ("Tools/PhotoMosaic Wizard")]
		static void generateDataSetFromExternal() {
			openWindow();
		}
		static IEnumerator current;
        //2
		static void generateDataSet(float ratio , string[] filePaths ,string datasetPath){
			current = doGenerateDataSet(ratio , filePaths,datasetPath);
			EditorApplication.update += onLoop;
		}
		static void onLoop(){
			if(current.MoveNext()){
			}else{
				EditorApplication.update -= onLoop;
			}
		}
        //3
		static IEnumerator doGenerateDataSet(float ratio , string[] filePaths ,string datasetPath){
			PhotoMosaicDataSet dataset = ScriptableObject.CreateInstance<PhotoMosaicDataSet>();
			dataset._ratio = ratio;
			dataset.textures = new List<PhotoMosaicDataSet.TextureData>();
			dataset.makeCache();
			//
			Texture2D tex = new Texture2D(2,2);
			byte[] b;
			for(int i = 0 ;i<filePaths.Length ; i++){
				string filepath = filePaths[i];
				EditorUtility.DisplayProgressBar("Converting pictures to PhotoMosaic dataset",
					i+"/"+filePaths.Length,i/(float)filePaths.Length);

				yield return null;

				b = File.ReadAllBytes(filepath);
				tex.LoadImage(b);
				tex.Apply();
				dataset.addTexture(tex);
			}
			EditorUtility.DisplayProgressBar("Saving",filePaths.Length+"/"+filePaths.Length,1);

			yield return null;

			tex = new Texture2D(dataset.cache.width,dataset.cache.height,TextureFormat.RGB24,false);
			RenderTexture.active = dataset.cache;
			tex.ReadPixels(new Rect(0,0,dataset.cache.width,dataset.cache.height),0,0);
			tex.Apply();
			RenderTexture.active = null;
			string pngPath = Path.GetDirectoryName(datasetPath)+"/"+Path.GetFileNameWithoutExtension(datasetPath)+"_tex.png";
			File.WriteAllBytes(pngPath,tex.EncodeToPNG());
			pngPath = MakeRelative(pngPath,Path.GetFullPath("./"));
			EditorUtility.DisplayProgressBar("Reimporting",0+"/"+2,.5f);

			yield return null;

			AssetDatabase.ImportAsset(pngPath,ImportAssetOptions.ForceUpdate);
			TextureImporter pngTi = (TextureImporter)TextureImporter.GetAtPath(pngPath);
			pngTi.mipmapEnabled = false;
			AssetDatabase.ImportAsset(pngPath,ImportAssetOptions.ForceUpdate);
			Texture2D finalPng = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
			dataset.makeIndexMap(4);
			for(int i = 0 ;i<dataset.textures.Count;i++){
				dataset.textures[i].texture = finalPng;
			}
			dataset.cache = null;
			dataset.cache2D = finalPng;
			//AssetDatabase.CreateAsset(dataset.indexMap,MakeRelative(Path.GetDirectoryName(datasetPath)+"/"+Path.GetFileNameWithoutExtension(datasetPath)+"_map.asset",Path.GetFullPath("./")));
			EditorUtility.DisplayProgressBar("Reimporting",1+"/"+2,1);

			yield return null;

			string indexPath = Path.GetDirectoryName(datasetPath)+"/"+Path.GetFileNameWithoutExtension(datasetPath)+"_map.png";
			File.WriteAllBytes(indexPath,dataset.indexMap.EncodeToPNG());
			indexPath = MakeRelative(indexPath,Path.GetFullPath("./"));
			AssetDatabase.ImportAsset(indexPath,ImportAssetOptions.ForceUpdate);
			TextureImporter indexTi = TextureImporter.GetAtPath(indexPath) as TextureImporter;
			indexTi.mipmapEnabled = false;
            //indexTi.textureFormat = TextureImporterFormat.ARGB32;
            indexTi.textureCompression = TextureImporterCompression.Compressed;
			indexTi.filterMode = FilterMode.Point;
			dataset.indexMap = AssetDatabase.LoadAssetAtPath<Texture2D>(indexPath);
			AssetDatabase.ImportAsset(indexPath,ImportAssetOptions.ForceUpdate);
			//
			EditorUtility.ClearProgressBar();
			AssetDatabase.CreateAsset(dataset,MakeRelative(datasetPath,Path.GetFullPath("./")));
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = dataset;
		}


		public static string MakeRelative(string filePath, string referencePath)
		{ 
			var fileUri = new System.Uri(filePath);
			var referenceUri = new System.Uri(referencePath);
			return referenceUri.MakeRelativeUri(fileUri).ToString();
		}
	}
}
