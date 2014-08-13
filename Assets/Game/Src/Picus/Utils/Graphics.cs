using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Picus.Utils
{
	public class Graphics
	{
		/** Create texture */
		public static Texture2D MakeTex(int width, int height, Color32 color)
		{
			var pix = new Color[width * height];
			
			for (int i = 0; i < pix.Length; i++) 
				pix[i] = color;

			var result = new Texture2D(width, height, TextureFormat.ARGB32, false);
			result.hideFlags = HideFlags.DontSave; // to avoid "Texture2D has been leaked X times."
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		/** Create texture 1x1 */
		public static Texture2D MakeTex(Color32 color)
		{
			return MakeTex(1, 1, color);
		}
	}
}