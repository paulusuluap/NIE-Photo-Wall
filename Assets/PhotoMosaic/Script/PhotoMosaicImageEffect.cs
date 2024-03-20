using UnityEngine;
using System.Collections;

namespace sunny{
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
	public class PhotoMosaicImageEffect : MonoBehaviour {
		/// <summary>
		/// there will be no effect if there is no dataSet ready
		/// </summary>
		public PhotoMosaicDataSet dataSet;
		/// <summary>
		/// link PhotoMosaicEffectShader to this
		/// </summary>
		public Shader shader;
		/// <summary>
		/// It's a cheat changing the color of grids
		/// </summary>
		[Range(0,1)]
		public float colorCheat = .5f;
		/// <summary>
		/// It's a cheat overlaping the target image on grids
		/// </summary>
		[Range(0,1)]
		public float photoCheat = .5f;
		[Range(1,180)]

		/// <summary>
		/// the number of grids on y axis
		/// </summary>
		public float scale = 24;
		/// <summary>
		/// randomness can make it looks like more images in the pool
		/// </summary>
		public PhotoMosaicDataSet.Randomness randomness;
		private Material _material = null;
		protected Material material {
			get {
				if (_material == null && shader!=null) {
					_material = new Material (shader);
					_material.hideFlags = HideFlags.DontSave;
					Update();
				}
				return _material;
			} 
		}
		void Update() {
			if(material!=null && dataSet!=null && dataSet.isReadyForImageEffect()){
				if(dataSet.cache!=null)
					material.SetTexture("_PhotoTex",dataSet.cache);
				if(dataSet.cache2D!=null)
					material.SetTexture("_PhotoTex",dataSet.cache2D);
				material.SetTexture("_IndexTex",dataSet.indexMap);
				material.SetInt("_IndexTexLevel",dataSet.indexMap.width/dataSet.indexMap.height);
				material.SetFloat("_ColorCheat",colorCheat);
				material.SetFloat("_PhotoCheat",photoCheat);
				material.SetInt("_Randomness",(int)randomness);
				material.SetFloat("_Scale",scale);
			}
		}
		void OnRenderImage(RenderTexture src, RenderTexture dest) {
			if(dataSet != null && material!=null && dataSet.isReadyForImageEffect()){

				Graphics.Blit(src, dest,material);
			}else{
				Graphics.Blit(src, dest);
			}
		}
	}
}
