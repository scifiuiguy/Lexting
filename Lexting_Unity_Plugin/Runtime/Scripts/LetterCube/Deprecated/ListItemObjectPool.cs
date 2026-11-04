using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGestureText.LetterCube
{
    /// <summary>
    /// deprecated
    /// Used in an old implementation fo the 3-zone cube
    /// </summary>
    public class ListItemObjectPool : Singleton<ListItemObjectPool>
    {
        [SerializeField] private Transform _targetParent;
        [SerializeField] private GameObject _lineItemPrefab;

        public GameObject AddFromPool()
        {
            if (transform.childCount == 0)
            {
                GameObject newLineItem = Instantiate(_lineItemPrefab, _targetParent.position, _targetParent.rotation) as GameObject;
                newLineItem.transform.SetParent(null,true);
                newLineItem.transform.SetParent(_targetParent, true);
                newLineItem.transform.localScale = Vector3.one;
                newLineItem.transform.position = _targetParent.position;
                newLineItem.transform.rotation = _targetParent.rotation;
                newLineItem.GetComponent<TextFalloffHandler>().SetFalloff();
                return newLineItem;
            }
            else {
                GameObject newLineItem = transform.GetChild(0).gameObject;
                newLineItem.transform.SetParent(null, true);
                newLineItem.transform.SetParent(_targetParent, true);
                newLineItem.transform.position = _targetParent.position;
                newLineItem.transform.rotation = _targetParent.rotation;
                newLineItem.GetComponent<TextFalloffHandler>().SetFalloff();
                return newLineItem;
            }
        }

        public void ReturnToPool(GameObject lineItem)
        {
            lineItem.transform.SetParent(transform);
            lineItem.transform.localScale = Vector3.one;
            lineItem.GetComponent<Text>().text = "";
        }
    }
}
