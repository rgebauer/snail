using System;
using UnityEngine;
using System.Collections.Generic;

// based on http://wiki.unity3d.com/index.php?title=VectorLine
namespace Picus.Utils
{	
	public class VectorLine
	{
		public Color Color = Color.red;
		public int Width = 2;
		public List<Vector2> Points = new List<Vector2>(); // points are in 0-1 format. eg new Vector2(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height)

		public VectorLine()
		{
		}

		public VectorLine(Vector2 min, Vector2 max) // rectangle in 0-1 format
		{
			Points.Add(new Vector2(min.x, min.y));
			Points.Add(new Vector2(min.x, max.y));
			Points.Add(new Vector2(max.x, max.y));
			Points.Add(new Vector2(max.x, min.y));
			Points.Add(new Vector2(min.x, min.y));
		}
	}

	// draw all lines
	[RequireComponent(typeof (Camera))]
	public class VectorLinesDrawer : MonoBehaviour
	{
		public bool Active = true;

		private Material _lineMaterial;
		private Camera _camera;
		private Dictionary<int, VectorLine> _lines = new Dictionary<int, VectorLine>();
		private List<VectorLine> _oneFrameLines = new List<VectorLine>();
		private int _lastId = 0;
		private float _lineWidth;
		private float _nearClip;

		public void DrawLine(VectorLine line) // will be drawed only next frame
		{
			_oneFrameLines.Add(line);
		}

		public int AddLine(VectorLine line) // will be drawed until remove called
		{
			_lines.Add(++_lastId, line);

			return _lastId;
		}

		public void RemoveLine(int id)
		{
			_lines.Remove(id);
		}

		void Awake()
		{
			_lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
			                            "SubShader { Pass {" +
			                            "   BindChannels { Bind \"Color\",color }" +
			                            "   Blend SrcAlpha OneMinusSrcAlpha" +
			                            "   ZWrite Off Cull Off Fog { Mode Off }" +
			                            "} } }");
			_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			_lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			_camera = camera;
			_lineWidth = 1f/Screen.width * 0.5f;
			_nearClip = _camera.nearClipPlane + 0.00001f;
		}			
		
		// Creates a simple two point line
		void OnPostRender()
		{
			if (!Active) return;

			foreach(VectorLine line in _oneFrameLines)
			{
				GlDrawLine(line);
			}
			_oneFrameLines.Clear();

			foreach(KeyValuePair<int, VectorLine> linePair in _lines)
			{
				GlDrawLine(linePair.Value);
			}
		}

		void GlDrawLine(VectorLine line)
		{
			int end = line.Points.Count - 1;
			float thisWidth = _lineWidth * line.Width;
			_lineMaterial.SetPass(0);

			if (line.Width == 1)
			{
				GL.Begin(GL.LINES);
				GL.Color(line.Color);
				for (int i = 0; i < end; ++i)
				{
					GL.Vertex(_camera.ViewportToWorldPoint(new Vector3(line.Points[i].x, line.Points[i].y, _nearClip)));
					GL.Vertex(_camera.ViewportToWorldPoint(new Vector3(line.Points[i+1].x, line.Points[i+1].y, _nearClip)));
				}
			}
			else
			{
				GL.Begin(GL.QUADS);
				GL.Color(line.Color);

				for (int i = 0; i < end; ++i)
				{
					Vector3 perpendicular = (new Vector3(line.Points[i+1].y, line.Points[i].x, _nearClip) -
					                         new Vector3(line.Points[i].y, line.Points[i+1].x, _nearClip)).normalized * thisWidth;
					Vector3 v1 = new Vector3(line.Points[i].x, line.Points[i].y, _nearClip);
					Vector3 v2 = new Vector3(line.Points[i+1].x, line.Points[i+1].y, _nearClip);
					GL.Vertex(_camera.ViewportToWorldPoint(v1 - perpendicular));
					GL.Vertex(_camera.ViewportToWorldPoint(v1 + perpendicular));
					GL.Vertex(_camera.ViewportToWorldPoint(v2 + perpendicular));
					GL.Vertex(_camera.ViewportToWorldPoint(v2 - perpendicular));
				}
			}
			GL.End();
		}
		
		void OnDestroy()
		{
			Destroy(_lineMaterial);
		}
	}
}


