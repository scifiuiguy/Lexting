using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialGestureText.Utilities
{
	public static class ConditionExtensions 
	{
		public static void IF(this MonoBehaviour behavior, bool condition, UnityAction THEN)
		{
			if (condition)
				THEN?.I();
		}

		public static void IFAND(this MonoBehaviour behavior, bool condition, bool condition2, UnityAction THEN)
		{
			if (condition && condition2)
				THEN?.I();
		}

		public static void IFOR(this MonoBehaviour behavior, bool condition, bool condition2, UnityAction THEN)
		{
			if (condition || condition2)
				THEN?.I();
		}


		public static void IFELSE(this MonoBehaviour behavior, bool condition, UnityAction THEN, UnityAction ELSETHEN)
		{
			if (condition)
				THEN?.I();
			else ELSETHEN?.I();
		}

		public static void IFANDELSE(this MonoBehaviour behavior, bool condition, bool condition2, UnityAction THEN, UnityAction ELSETHEN)
		{
			if (condition && condition2)
				THEN?.I();
			else ELSETHEN?.I();
		}

		public static void IFORELSE(this MonoBehaviour behavior, bool condition, bool condition2, UnityAction THEN)
		{
			if (condition || condition2)
				THEN?.I();
		}




	}
}
