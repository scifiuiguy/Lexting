using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pingleton is a singleton declared as a property instead of as a class
/// so as to avoid interference with any existing inheritance structure.
/// 
/// /// Setup:
/// public class YOURCLASS : MonoBehaviour
/// {
///     public static Pingleton<YOURCLASS> _;
///     
///     public void DoStuff()
///     {}
/// }
/// 
/// /// Usage:
/// YOURCLASS._.Instance.DoStuff();
/// 
/// </summary>
/// <typeparam name="T">the type you want to declare as a singleton</typeparam>
public class Pingleton<T> where T : MonoBehaviour
{
    //public Pingleton()
    //{
    //    m_Instance = (T)GameObject.FindObjectOfType(typeof(T));
    //}

    #region vars
    #region public vars
    public T Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = (T)GameObject.FindObjectOfType(typeof(T));
            return m_Instance;
        }
    }
    #endregion public vars
    #region private vars
    private static T m_Instance;
    #endregion private vars
    #endregion vars
    
}
