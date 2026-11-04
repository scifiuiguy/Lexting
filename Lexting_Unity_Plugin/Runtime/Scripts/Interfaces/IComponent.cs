using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Interfaces
{
	public interface IComponent
	{
        Transform transform { get; }
        GameObject gameObject { get; }
        bool IsReady();
    }
}
