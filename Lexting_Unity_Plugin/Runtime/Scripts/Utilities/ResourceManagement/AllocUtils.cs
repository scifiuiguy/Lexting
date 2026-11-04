using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Events;

public static class AllocUtils
{
    #region diagnostics
    public static bool HasComponent<T>(this GameObject obj)
    {
        return obj.GetComponent(typeof(T)) != null;
    }

    public static bool HasComponent<T>(this Transform obj)
    {
        return obj.GetComponent(typeof(T)) != null;
    }
    #endregion diagnostics

    #region Get in ancestor
    /// <summary>
    /// Get a reference to the first instance of component T found in nearest grandparent
    /// </summary>
    /// <typeparam name="T">the target component attached to the target grandparent</typeparam>
    /// <returns></returns>
    public static T GinG<T>(this Transform transform)
    {
        
        Transform target = transform.parent;
        while (target.GetComponent<T>() == null)
        {
            if (target.parent == null)
                Debug.LogError("This thumb marker script is not in a child hierarchy under an AttachmentHand script. " +
                    "It needs to have AttachmentHand somewhere in its ancestor hierarchy to work properly.");
            else target = target.parent;
        }
        return target.GetComponent<T>();
        
    }

    /// <summary>
    /// Get a reference to the transform of the nearest grandparent who has component T
    /// </summary>
    /// <typeparam name="T">the target component attached to the target grandparent</typeparam>
    /// <returns></returns>
    public static Transform GG<T>(this Transform transform)
    {

        Transform target = transform.parent;
        while (target.GetComponent<T>() == null)
        {
            if (target.parent == null)
                Debug.LogError("This thumb marker script is not in a child hierarchy under an AttachmentHand script. " +
                    "It needs to have AttachmentHand somewhere in its ancestor hierarchy to work properly.");
            else target = target.parent;
        }
        return target;

    }
    #endregion get in ancestor

    #region Null Check
    public static bool isNull(this object obj)
    {
        return obj == null;
    }

    
    public static bool Ready(this MonoBehaviour behaviour)
    {
        return behaviour != null;
    }

    public static bool Ready(this GameObject gameObject)
    {
        return gameObject != null;
    }

    public static bool Ready(this Transform transform)
    {
        return transform != null;
    }

    public static bool Ready(this MonoBehaviour behaviour, object[] objects)
    {
        foreach (var obj in objects)
            if (obj == null)
                return false;
        return true;
    }

    public static bool Ready(this MonoBehaviour behaviour, object obj1)
    {
        if (obj1 == null)
            return false;
        return true;
    }
    
    public static bool Ready(this MonoBehaviour behaviour, object obj1, object obj2)
    {
        if (obj1 == null)
            return false;
        if (obj2 == null)
            return false;
        return true;
    }

    public static bool Ready(this MonoBehaviour behaviour, object obj1, object obj2, object obj3)
    {
        if (obj1 == null)
            return false;
        if (obj2 == null)
            return false;
        if (obj3 == null)
            return false;
        return true;
    }

    public static bool Ready(this MonoBehaviour behaviour, object obj1, object obj2, object obj3, object obj4)
    {
        if (obj1 == null)
            return false;
        if (obj2 == null)
            return false;
        if (obj3 == null)
            return false;
        if (obj4 == null)
            return false;
        return true;
    }

    public static bool Ready(this MonoBehaviour behaviour, object obj1, object obj2, object obj3, object obj4, object obj5)
    {
        if (obj1 == null)
            return false;
        if (obj2 == null)
            return false;
        if (obj3 == null)
            return false;
        if (obj4 == null)
            return false;
        if (obj5 == null)
            return false;
        return true;
    }


    #endregion Null Check

    #region foreach over acquired refs to do stuff
    /// <summary>
    /// Bulk Foreach: iterates over each element of up to five arrays to perform a specified action on each
    /// </summary>
    public static void FE<T0>(this MonoBehaviour behaviour, ref T0[] var0, UnityAction<T0> action0)
    {
        foreach (T0 element in var0)
            action0.I(element);
    }

    /// <summary>
    /// Bulk Foreach: iterates over each element of up to five arrays to perform a specified action on each
    /// </summary>
    public static void FE<T0, T1>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
         ref T1[] var1, UnityAction<T1> action1)
    {
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
    }

    /// <summary>
    /// Bulk Foreach: iterates over each element of up to five arrays to perform a specified action on each
    /// </summary>
    public static void FE<T0, T1, T2>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
         ref T1[] var1, UnityAction<T1> action1,
         ref T2[] var2, UnityAction<T2> action2)
    {
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
    }

    /// <summary>
    /// Bulk Foreach: iterates over each element of up to five arrays to perform a specified action on each
    /// </summary>
    public static void FE<T0, T1, T2, T3>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
         ref T1[] var1, UnityAction<T1> action1,
         ref T2[] var2, UnityAction<T2> action2,
         ref T3[] var3, UnityAction<T3> action3)
    {
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
        foreach (T3 element3 in var3)
            action3.I(element3);
    }

    /// <summary>
    /// Bulk Foreach: iterates over each element of up to five arrays to perform a specified action on each
    /// </summary>
    public static void FE<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
        ref T1[] var1, UnityAction<T1> action1,
        ref T2[] var2, UnityAction<T2> action2,
        ref T3[] var3, UnityAction<T3> action3,
        ref T4[] var4, UnityAction<T4> action4)
    {
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
        foreach (T3 element3 in var3)
            action3.I(element3);
        foreach (T4 element4 in var4)
            action4.I(element4);
    }
    #endregion foreach over acquired refs to do stuff

    #region Bulk Gets+Foreach combos
    /// <summary>
    /// Get-Components-In-Children followed by Foreach
    /// </summary>
    /// <typeparam name="T0">the target component to search for</typeparam>
    /// <param name="behaviour">the behaviour that will execute this method</param>
    /// <param name="var0">the resulting array of all instances of T0 found in children</param>
    /// <param name="action0">the action that will be executed on each instance of var0</param>
    public static void ChsFE<T0>(this MonoBehaviour behaviour, ref T0[] var0, UnityAction<T0> action0)
    {
        GetsInChildren(behaviour, ref var0);
        foreach (T0 element in var0)
            action0.I(element);
    }

    /// <summary>
    /// Bulk Gets-In-Children then Bulk Foreach
    /// </summary>
    public static void ChsFE<T0, T1>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> actionToPerformOnEachCell0,
         ref T1[] var1, UnityAction<T1> actionToPerformOnEachCell1)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        foreach (T0 element0 in var0)
            actionToPerformOnEachCell0.I(element0);
        foreach (T1 element1 in var1)
            actionToPerformOnEachCell1.I(element1);
    }

    /// <summary>
    /// Bulk Gets-InChildren then Bulk Foreach
    /// </summary>
    public static void ChsFE<T0, T1, T2>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
         ref T1[] var1, UnityAction<T1> action1,
         ref T2[] var2, UnityAction<T2> action2)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
    }

    /// <summary>
    /// Bulk Gets-InChildren then Bulk Foreach
    /// </summary>
    public static void ChsFE<T0, T1, T2, T3>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
         ref T1[] var1, UnityAction<T1> action1,
         ref T2[] var2, UnityAction<T2> action2,
         ref T3[] var3, UnityAction<T3> action3)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
        GetsInChildren(behaviour, ref var3);
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
        foreach (T3 element3 in var3)
            action3.I(element3);
    }

    /// <summary>
    /// Bulk Gets-InChildren then Bulk Foreach
    /// </summary>
    public static void ChsFE<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour,
        ref T0[] var0, UnityAction<T0> action0,
        ref T1[] var1, UnityAction<T1> action1,
        ref T2[] var2, UnityAction<T2> action2,
        ref T3[] var3, UnityAction<T3> action3,
        ref T4[] var4, UnityAction<T4> action4)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
        GetsInChildren(behaviour, ref var3);
        GetsInChildren(behaviour, ref var4);
        foreach (T0 element0 in var0)
            action0.I(element0);
        foreach (T1 element1 in var1)
            action1.I(element1);
        foreach (T2 element2 in var2)
            action2.I(element2);
        foreach (T3 element3 in var3)
            action3.I(element3);
        foreach (T4 element4 in var4)
            action4.I(element4);
    }
    #endregion Bulk Get+Foreach combos

    #region get from current transform for current behaviour
    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0>(this MonoBehaviour behaviour, ref T0 var0)
    {
        GetInChildren(behaviour, ref var0);
    } 
    
    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0>(this MonoBehaviour behaviour, ref T0 var0, bool all)
    {
        GetInChildren(behaviour, ref var0, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0>(this MonoBehaviour behaviour, ref T0[] var0)
    {
        GetsInChildren(behaviour, ref var0);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0>(this MonoBehaviour behaviour, ref T0[] var0, bool all)
    {
        GetsInChildren(behaviour, ref var0, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1)
    {
        GetInChildren(behaviour, ref var0);
        GetInChildren(behaviour, ref var1);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, bool all)
    {
        GetInChildren(behaviour, ref var0, all);
        GetInChildren(behaviour, ref var1, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, bool all)
    {
        GetsInChildren(behaviour, ref var0, all);
        GetsInChildren(behaviour, ref var1, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2)
    {
        GetInChildren(behaviour, ref var0);
        GetInChildren(behaviour, ref var1);
        GetInChildren(behaviour, ref var2);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2, bool all)
    {
        GetInChildren(behaviour, ref var0, all);
        GetInChildren(behaviour, ref var1, all);
        GetInChildren(behaviour, ref var2, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2, bool all)
    {
        GetsInChildren(behaviour, ref var0, all);
        GetsInChildren(behaviour, ref var1, all);
        GetsInChildren(behaviour, ref var2, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3)
    {
        GetInChildren(behaviour, ref var0);
        GetInChildren(behaviour, ref var1);
        GetInChildren(behaviour, ref var2);
        GetInChildren(behaviour, ref var3);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, bool all)
    {
        GetInChildren(behaviour, ref var0, all);
        GetInChildren(behaviour, ref var1, all);
        GetInChildren(behaviour, ref var2, all);
        GetInChildren(behaviour, ref var3, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
        GetsInChildren(behaviour, ref var3);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, bool all)
    {
        GetsInChildren(behaviour, ref var0, all);
        GetsInChildren(behaviour, ref var1, all);
        GetsInChildren(behaviour, ref var2, all);
        GetsInChildren(behaviour, ref var3, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, ref T4 var4)
    {
        GetInChildren(behaviour, ref var0);
        GetInChildren(behaviour, ref var1);
        GetInChildren(behaviour, ref var2);
        GetInChildren(behaviour, ref var3);
        GetInChildren(behaviour, ref var4);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, ref T4 var4, bool all)
    {
        GetInChildren(behaviour, ref var0, all);
        GetInChildren(behaviour, ref var1, all);
        GetInChildren(behaviour, ref var2, all);
        GetInChildren(behaviour, ref var3, all);
        GetInChildren(behaviour, ref var4, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, ref T4[] var4)
    {
        GetsInChildren(behaviour, ref var0);
        GetsInChildren(behaviour, ref var1);
        GetsInChildren(behaviour, ref var2);
        GetsInChildren(behaviour, ref var3);
        GetsInChildren(behaviour, ref var4);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, ref T4[] var4, bool all)
    {
        GetsInChildren(behaviour, ref var0, all);
        GetsInChildren(behaviour, ref var1, all);
        GetsInChildren(behaviour, ref var2, all);
        GetsInChildren(behaviour, ref var3, all);
        GetsInChildren(behaviour, ref var4, all);
    }

    #endregion get from current transform for current behaviour

    #region get from other transform for current behaviour
    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0>(this MonoBehaviour behaviour, Transform transform, ref T0 var0)
    {
        GetInChildren(behaviour, ref var0);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, bool all)
    {
        GetInChildren(behaviour, transform, ref var0, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0)
    {
        GetsInChildren(behaviour, transform, ref var0);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, bool all)
    {
        GetsInChildren(behaviour, transform, ref var0, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1)
    {
        GetInChildren(behaviour, transform, ref var0);
        GetInChildren(behaviour, transform, ref var1);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, bool all)
    {
        GetInChildren(behaviour, transform, ref var0, all);
        GetInChildren(behaviour, transform, ref var1, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1)
    {
        GetsInChildren(behaviour, transform, ref var0);
        GetsInChildren(behaviour, transform, ref var1);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, bool all)
    {
        GetsInChildren(behaviour, transform, ref var0, all);
        GetsInChildren(behaviour, transform, ref var1, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2)
    {
        GetInChildren(behaviour, transform, ref var0);
        GetInChildren(behaviour, transform, ref var1);
        GetInChildren(behaviour, transform, ref var2);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2, bool all)
    {
        GetInChildren(behaviour, transform, ref var0, all);
        GetInChildren(behaviour, transform, ref var1, all);
        GetInChildren(behaviour, transform, ref var2, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2)
    {
        GetsInChildren(behaviour, transform, ref var0);
        GetsInChildren(behaviour, transform, ref var1);
        GetsInChildren(behaviour, transform, ref var2);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2, bool all)
    {
        GetsInChildren(behaviour, transform, ref var0, all);
        GetsInChildren(behaviour, transform, ref var1, all);
        GetsInChildren(behaviour, transform, ref var2, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3)
    {
        GetInChildren(behaviour, transform, ref var0);
        GetInChildren(behaviour, transform, ref var1);
        GetInChildren(behaviour, transform, ref var2);
        GetInChildren(behaviour, transform, ref var3);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, bool all)
    {
        GetInChildren(behaviour, transform, ref var0, all);
        GetInChildren(behaviour, transform, ref var1, all);
        GetInChildren(behaviour, transform, ref var2, all);
        GetInChildren(behaviour, transform, ref var3, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3)
    {
        GetsInChildren(behaviour, transform, ref var0);
        GetsInChildren(behaviour, transform, ref var1);
        GetsInChildren(behaviour, transform, ref var2);
        GetsInChildren(behaviour, transform, ref var3);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, bool all)
    {
        GetsInChildren(behaviour, transform, ref var0, all);
        GetsInChildren(behaviour, transform, ref var1, all);
        GetsInChildren(behaviour, transform, ref var2, all);
        GetsInChildren(behaviour, transform, ref var3, all);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, ref T4 var4)
    {
        GetInChildren(behaviour, transform, ref var0);
        GetInChildren(behaviour, transform, ref var1);
        GetInChildren(behaviour, transform, ref var2);
        GetInChildren(behaviour, transform, ref var3);
        GetInChildren(behaviour, transform, ref var4);
    }

    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static void Ch<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, Transform transform, ref T0 var0, ref T1 var1, ref T2 var2, ref T3 var3, ref T4 var4, bool all)
    {
        GetInChildren(behaviour, transform, ref var0, all);
        GetInChildren(behaviour, transform, ref var1, all);
        GetInChildren(behaviour, transform, ref var2, all);
        GetInChildren(behaviour, transform, ref var3, all);
        GetInChildren(behaviour, transform, ref var4, all);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, ref T4[] var4)
    {
        GetsInChildren(behaviour, transform, ref var0);
        GetsInChildren(behaviour, transform, ref var1);
        GetsInChildren(behaviour, transform, ref var2);
        GetsInChildren(behaviour, transform, ref var3);
        GetsInChildren(behaviour, transform, ref var4);
    }

    /// <summary>
    /// Gets components in children of provided type for each var
    /// </summary>
    public static void Chs<T0, T1, T2, T3, T4>(this MonoBehaviour behaviour, Transform transform, ref T0[] var0, ref T1[] var1, ref T2[] var2, ref T3[] var3, ref T4[] var4, bool all)
    {
        GetsInChildren(behaviour, transform, ref var0, all);
        GetsInChildren(behaviour, transform, ref var1, all);
        GetsInChildren(behaviour, transform, ref var2, all);
        GetsInChildren(behaviour, transform, ref var3, all);
        GetsInChildren(behaviour, transform, ref var4, all);
    }

    #endregion get from other transform for current behaviour

    #region get from this transform without ref for current transform
    /// <summary>
    /// Gets component in children of provided type for each var
    /// </summary>
    public static T0 Ch<T0>(this MonoBehaviour behaviour)
    {
        return GetInChildren<T0>(behaviour);
    }
    #endregion get from other transform without ref for current transform


    private static T GetInChildren<T>(MonoBehaviour behaviour)
    {
        T var = behaviour.GetComponentInChildren<T>(true);
        if (var == null)
        {
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                $" Is there a missing instance of {var.GetType()} in the scene?");
            return default;
        }
        else return var;
    }


    private static void GetInChildren<T>(MonoBehaviour behaviour, ref T var)
    {
        var = behaviour.GetComponentInChildren<T>(true);
        if (var == null)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} in the scene?");
    }

    private static void GetInChildren<T>(MonoBehaviour behaviour, ref T var, bool all)
    {
        var = behaviour.GetComponentInChildren<T>(all);
        if (var == null)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} in the scene?");
    }

    private static void GetsInChildren<T>(MonoBehaviour behaviour, ref T[] var)
    {
        
        var = behaviour.GetComponentsInChildren<T>(true);
        if (var == null || var.Length == 0)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} in the scene?");
    }

    private static void GetsInChildren<T>(MonoBehaviour behaviour, ref T[] var, bool all)
    {
        var = behaviour.GetComponentsInChildren<T>(true);
        if (var == null || var.Length == 0)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} in the scene?");
    }



    private static void GetInChildren<T>(MonoBehaviour behaviour, Transform transform, ref T var)
    {
        var = transform.GetComponentInChildren<T>(true);
        if (var == null)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} under the transform {transform.name} in the scene?");
    }

    private static void GetInChildren<T>(MonoBehaviour behaviour, Transform transform, ref T var, bool all)
    {
        var = behaviour.GetComponentInChildren<T>(all);
        if (var == null)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} under the transform {transform.name} in the scene?");
    }

    private static void GetsInChildren<T>(MonoBehaviour behaviour, Transform transform, ref T[] var)
    {
        var = behaviour.GetComponentsInChildren<T>(true);
        if (var == null || var.Length == 0)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} under the transform {transform.name} in the scene?");
    }

    private static void GetsInChildren<T>(MonoBehaviour behaviour, Transform transform, ref T[] var, bool all)
    {
        var = behaviour.GetComponentsInChildren<T>(true);
        if (var == null || var.Length == 0)
            Debug.LogError($"BROKEN RUNTIME REFERENCE: {behaviour.name} script on game object named " +
                    $"{behaviour.gameObject.name} could no longer locate a reference for its field named ({var.GetType()}." +
                    $" Is there a missing instance of {var.GetType()} under the transform {transform.name} in the scene?");
    }
}
