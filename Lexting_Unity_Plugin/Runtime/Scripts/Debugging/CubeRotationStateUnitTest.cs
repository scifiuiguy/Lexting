using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CubeRotationStateUnitTest : MonoBehaviour
{
    private GameObject testCube;
    private MeshRenderer cubeRenderer;
    public bool right;

    private void Awake()
    {
        testCube = this.gameObject;
        cubeRenderer = gameObject.GetComponent<MeshRenderer>();

        if (right)
            this.OnMonoEnableAsObservable()
                .Subscribe(_ => Lexting.input.OnRightRotationStateChanged?.AddListener(OnRightRotationStateChanged));
        else this.OnMonoDisableAsObservable()
                .Subscribe(_ => Lexting.input.OnRightRotationStateChanged?.AddListener(OnRightRotationStateChanged));
    }

    private void OnLeftRotationStateChanged(CubeRotationState current, CubeRotationState previous)
    {
        Debug.Log("Left CubeRotationStageChanged - current: " + current + " previous: " + previous);
        cubeRenderer.enabled = current == CubeRotationState.other;
    }

    private void OnRightRotationStateChanged(CubeRotationState current, CubeRotationState previous)
    {
        Debug.Log("Right CubeRotationStageChanged - current: " + current + " previous: " + previous);
        cubeRenderer.enabled = current == CubeRotationState.other;
    }
}
