#if LEXTING_FOUND_OCULUS_SDK && UNITY_ANDROID
using System.Collections;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// XRRig uses Unity <see cref="UnityEngine.XR.Hands.XRHandSubsystem"/> visualizers, which only run with OpenXR.
    /// Quest M1 builds use the Oculus XR Plugin loader; spawn Meta OVRCustomHand prefabs under hand anchors for tracking.
    /// </summary>
    public class QuestOvrHandRigSpawner : MonoBehaviour
    {
        [SerializeField] GameObject _leftHandPrefab;
        [SerializeField] GameObject _rightHandPrefab;

        Transform _pendingLeft;
        Transform _pendingRight;
        Coroutine _spawnRoutine;

        public static void EnsureUnderAnchors(Transform leftAnchor, Transform rightAnchor)
        {
            if (leftAnchor == null || rightAnchor == null)
                return;

            var spawner = leftAnchor.GetComponentInParent<QuestOvrHandRigSpawner>();
            if (spawner == null)
                spawner = Object.FindFirstObjectByType<QuestOvrHandRigSpawner>();

            if (spawner == null)
            {
                Debug.LogWarning("[QuestOvrHandRigSpawner] No spawner on XRRig; cannot instantiate OVRCustomHand prefabs.");
                return;
            }

            spawner._pendingLeft = leftAnchor;
            spawner._pendingRight = rightAnchor;
            if (spawner._spawnRoutine == null)
                spawner._spawnRoutine = spawner.StartCoroutine(spawner.SpawnWhenOvrReady());
        }

        IEnumerator SpawnWhenOvrReady()
        {
            const int maxFrames = 600;
            for (var i = 0; i < maxFrames && !OVRPlugin.initialized; i++)
                yield return null;

            yield return null;
            yield return null;

            if (_pendingLeft != null && _pendingRight != null)
                EnsureSpawned(_pendingLeft, _pendingRight);

            _spawnRoutine = null;
        }

        void EnsureSpawned(Transform leftAnchor, Transform rightAnchor)
        {
            SpawnIfMissing(leftAnchor, _leftHandPrefab, "OVRCustomHand_L");
            SpawnIfMissing(rightAnchor, _rightHandPrefab, "OVRCustomHand_R");
        }

        static bool AnchorHasOvrHandRig(Transform anchor)
        {
            if (anchor == null)
                return false;

            foreach (var behaviour in anchor.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                    continue;
                var typeName = behaviour.GetType().Name;
                if (typeName is "OVRCustomSkeleton" or "OVRHand" or "OVRSkeleton")
                    return true;
            }

            return false;
        }

        static void SpawnIfMissing(Transform anchor, GameObject prefab, string instanceName)
        {
            if (anchor == null || prefab == null)
                return;

            if (AnchorHasOvrHandRig(anchor))
                return;

            var instance = Object.Instantiate(prefab, anchor);
            instance.name = instanceName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            if (instance.GetComponent<OVRSkeleton>() != null)
                instance.AddComponent<QuestOvrSkeletonInitRetry>();

            var customSkeleton = instance.GetComponent<OVRCustomSkeleton>();
            if (customSkeleton != null)
                QuestOvrCustomSkeletonRuntimeMap.EnsureMapped(customSkeleton);
        }
    }
}
#endif
