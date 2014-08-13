using UnityEngine;
using System.Collections;

namespace Picus.Utils.Txt
{
	[ExecuteInEditMode]
	public class MeshWrapper : Picus.MonoBehaviourExtend 
	{
		public bool WrapOnStart = false;
		public int MaxWidth = 0; // 0 = infinite
		public float DefaultCharacterWidth = 10;
		
		private TextMesh _textMesh = null;

		public int Wrap()
		{
			if (_textMesh == null)
				_textMesh = GetComponent<TextMesh>();

			return Wrap(_textMesh.text);
		}

		public int Wrap(string source)
		{
			if (_textMesh == null)
				_textMesh = GetComponent<TextMesh>();

			if (MaxWidth == 0 || _textMesh == null) 
				return 1;

			_textMesh.text = source;
			int linesCount = 1;

			Font f = _textMesh.font;
			Debug.Assert(f != null,"Font not found.");
	        string str = _textMesh.text;
	        int nLastWordInd = 0;
	        int nIter = 0;
	        float fCurWidth = 0.0f;
	        float fCurWordWidth = 0.0f;
	        while (nIter < str.Length)
	        {
	            // get char info
	            char c = str[nIter];
	            CharacterInfo charInfo;
	 
	            if (c == '\n' || c == '\r')
	            {
	                nLastWordInd = nIter;
	                fCurWidth = 0.0f;
	            }
	            else
	            {
					if (!f.GetCharacterInfo(c, out charInfo, _textMesh.fontSize, _textMesh.fontStyle))
					{
						Debug.Log("Unrecognized character: " + c);
						// TODO: why GetCharacterInfo is not working?
						Debug.Assert(false, "Unrecognized character encountered (" + (int)c + "): " + c, true);
						if (!f.GetCharacterInfo('x', out charInfo))
						{
							Debug.Assert(false, "Unrecognized character encountered for default (" + (int)c + "): " + c, true);
							charInfo = new CharacterInfo();
							charInfo.width = DefaultCharacterWidth;
						}
		            }
					
	                if (c == ' ')
	                {
	                    nLastWordInd = nIter; // replace this character with '/n' if breaking here
	                    fCurWordWidth = 0.0f;
	                }
	 
	                fCurWidth += charInfo.width;
	                fCurWordWidth += charInfo.width;
	                if (fCurWidth >= MaxWidth && nLastWordInd != 0)
	                {
						char cTmp = str[nLastWordInd];
	                    str = str.Remove(nLastWordInd, 1);
	                    str = str.Insert(nLastWordInd, "\n");
	                    fCurWidth = fCurWordWidth;
						if(cTmp != '\n') 
						{
							linesCount++;
						}
	                }
	            }
	 
	            ++nIter;
	        }
	 
			_textMesh.text = str;

			return linesCount;
		}
			
		void Awake() 
		{
		}
		
		void Start()
		{
			if (WrapOnStart) 
				Wrap();
		}

#if UNITY_EDITOR
		void Update() 
		{
			if (!Application.isPlaying)
				Wrap();
		}
#endif
	}
}
