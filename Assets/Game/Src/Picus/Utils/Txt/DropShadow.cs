using UnityEngine;
using System.Collections;

namespace Picus.Utils.Txt
{
	/** Shadow under text */
	public class DropShadow : Picus.MonoBehaviourExtend 
	{
		TextMesh textMesh;

		UnityEngine.GameObject shadow;
		TextMesh shadowTextMesh;

		void Init()
		{
			textMesh = gameObject.GetComponent<TextMesh>();

			shadow = (UnityEngine.GameObject)Instantiate(gameObject);

			DropShadow script = shadow.GetComponent<DropShadow>();
			script.enabled = false;
			
			Vector3 shiftPos = new Vector3(-0.2f, 0.2f, 0);
			shiftPos.Scale(transform.localScale);
			shadow.transform.parent = transform.parent;
			shadow.transform.localPosition = transform.localPosition - shiftPos;
			shadow.transform.localRotation = transform.localRotation;
			shadow.transform.localScale = transform.localScale;

			shadow.hideFlags = HideFlags.NotEditable;
			transform.parent = shadow.transform;
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			meshRenderer.sortingOrder = meshRenderer.sortingOrder + 1;

			shadowTextMesh = shadow.GetComponent<TextMesh>();
			shadowTextMesh.color = new Color(0, 0, 0, 0.5f);
		}

		// Update is called once per frame
		void Update () 
		{
			if (!shadow)
				Init();
			else if (shadowTextMesh.text != textMesh.text)
				shadowTextMesh.text = textMesh.text;
		}
	}
}