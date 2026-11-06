using Pixelplacement;
using Pixelplacement.TweenSystem;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialGestureText.Tutorial
{
    /// <summary>
    /// Lerps a skinned hand rig between two poses captured from reference hierarchies (open vs closed).
    /// Assign disabled duplicate roots in the prefab as pose references; they are read once at startup, not spawned at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public class TutorialProceduralHandPose : MonoBehaviour
    {
        [SerializeField] private Transform poseRoot;
        [Tooltip("Duplicate hand posed open/neutral. Can be an inactive sibling; not enabled for gameplay.")]
        [SerializeField] private Transform openPoseReference;
        [Tooltip("Duplicate hand posed closed (e.g. index–thumb pinch / fist). Inactive sibling is fine.")]
        [SerializeField] private Transform closedPoseReference;
        [SerializeField] private float defaultTransitionDuration = 0.6f;

        [Range(0f, 1f)]
        [SerializeField] private float previewClosedAmount;

        private struct LocalPose
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        private readonly Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
        private readonly Dictionary<string, LocalPose> _openPoses = new Dictionary<string, LocalPose>();
        private readonly Dictionary<string, LocalPose> _closedPoses = new Dictionary<string, LocalPose>();

        private float _closedAmount;
        private TweenBase _blendTween;

        public float ClosedAmount => _closedAmount;

        public Transform PoseRoot => poseRoot != null ? poseRoot : transform;

        private void Awake()
        {
            if (poseRoot == null)
                poseRoot = transform;
            RebuildSnapshots();
            SetClosedAmountImmediate(0f);
        }

        private void OnValidate()
        {
            if (poseRoot == null)
                poseRoot = transform;
#if UNITY_EDITOR
            if (!Application.isPlaying && openPoseReference != null && closedPoseReference != null)
            {
                RebuildSnapshots();
                ApplyClosedAmount(previewClosedAmount);
            }
#endif
        }

        [ContextMenu("Rebuild Pose Snapshots")]
        public void RebuildSnapshots()
        {
            _bones.Clear();
            _openPoses.Clear();
            _closedPoses.Clear();

            IndexBones(poseRoot, _bones);

            if (openPoseReference == null || closedPoseReference == null)
            {
                Debug.LogWarning($"{nameof(TutorialProceduralHandPose)} on {name}: assign open and closed pose references.", this);
                return;
            }

            var openIndex = new Dictionary<string, Transform>();
            var closedIndex = new Dictionary<string, Transform>();
            IndexBones(openPoseReference, openIndex);
            IndexBones(closedPoseReference, closedIndex);

            int matched = 0;
            foreach (var boneName in _bones.Keys)
            {
                if (!openIndex.TryGetValue(boneName, out var openBone) || !closedIndex.TryGetValue(boneName, out var closedBone))
                    continue;

                _openPoses[boneName] = CaptureLocalPose(openBone);
                _closedPoses[boneName] = CaptureLocalPose(closedBone);
                matched++;
            }

            if (matched == 0)
                Debug.LogWarning($"{nameof(TutorialProceduralHandPose)} on {name}: no bones matched between driven root and references. Use identical Oculus hand hierarchies.", this);
        }

        /// <summary>0 = open reference pose, 1 = closed reference pose.</summary>
        public void SetClosedAmountImmediate(float closedAmount)
        {
            _closedAmount = Mathf.Clamp01(closedAmount);
            ApplyClosedAmount(_closedAmount);
        }

        public void AnimateClosedAmount(float targetClosedAmount, float duration = -1f)
        {
            if (duration < 0f)
                duration = defaultTransitionDuration;

            _blendTween?.Stop();
            float start = _closedAmount;
            float end = Mathf.Clamp01(targetClosedAmount);
            _blendTween = Tween.Value(start, end, ApplyClosedAmount, duration, 0f);
        }

        public void AnimateToOpen(float duration = -1f) => AnimateClosedAmount(0f, duration);

        public void AnimateToClosed(float duration = -1f) => AnimateClosedAmount(1f, duration);

        private void ApplyClosedAmount(float closedAmount)
        {
            _closedAmount = closedAmount;
            foreach (var entry in _bones)
            {
                string boneName = entry.Key;
                Transform bone = entry.Value;
                if (!_openPoses.TryGetValue(boneName, out LocalPose open) || !_closedPoses.TryGetValue(boneName, out LocalPose closed))
                    continue;

                bone.localPosition = Vector3.Lerp(open.localPosition, closed.localPosition, closedAmount);
                bone.localRotation = Quaternion.Slerp(open.localRotation, closed.localRotation, closedAmount);
                bone.localScale = Vector3.Lerp(open.localScale, closed.localScale, closedAmount);
            }
        }

        private static void IndexBones(Transform root, Dictionary<string, Transform> into)
        {
            if (root == null)
                return;

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t == root)
                    continue;
                if (!into.ContainsKey(t.name))
                    into.Add(t.name, t);
            }
        }

        private static LocalPose CaptureLocalPose(Transform t)
        {
            return new LocalPose
            {
                localPosition = t.localPosition,
                localRotation = t.localRotation,
                localScale = t.localScale
            };
        }
    }
}
