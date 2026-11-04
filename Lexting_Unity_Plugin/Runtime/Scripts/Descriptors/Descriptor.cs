using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// deprecated for use w/ descriptors - see ReadOnlyDescriptor.cs
public class ReadOnlyAttribute : PropertyAttribute
{

}


namespace SpatialGestureText.Descriptors
{
	public  class Descriptor : MonoBehaviour
	{
		#region vars
		#region public vars
		//[ReadOnly]
		[TextArea(25, 20)]
		public string description;
		#endregion public vars
		#endregion vars
	}
}
