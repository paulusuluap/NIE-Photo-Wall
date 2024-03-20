using UnityEngine;
using System.Collections;

namespace sunny{
	[RequireComponent(typeof(PhotoMosaic))]
	/// <summary>
	/// it's a simple script for you generate one photo mosaic effect without scripting
	/// </summary>
	public class PhotoMosaicInstant : MonoBehaviour {
		/// <summary>
		/// it has to be readable Texture2D
		/// </summary>
		public Texture2D target;
		void Start () {
			PhotoMosaic p = GetComponent<PhotoMosaic>();
			p.generate(target);
		}

	}
}
