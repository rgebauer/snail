using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Reflection;
using System;

namespace Picus
{
	/* Attribute used for ToolTip in unity inspector */
	public class GuiToolTip : Attribute
	{
		private string _toolTip;
		
		public GuiToolTip(string toolTip) { _toolTip = toolTip; }
		public string ToolTip { get { return _toolTip; } }
	}
}
