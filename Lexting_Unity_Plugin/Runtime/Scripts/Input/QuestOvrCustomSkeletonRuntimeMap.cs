#if LEXTING_FOUND_OCULUS_SDK && UNITY_ANDROID
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// OVRCustomHandPrefab assets in the repo ship with empty CustomBones lists; editor auto-map is editor-only.
    /// Maps skinned mesh bones (b_l_ / b_r_) at runtime before <see cref="OVRSkeleton"/> initializes.
    /// </summary>
    internal static class QuestOvrCustomSkeletonRuntimeMap
    {
        static readonly string[] HandSidePrefix = { "l_", "r_" };
        const string FbxHandBonePrefix = "b_";

        static readonly string[] FbxHandBoneNames =
        {
            "wrist",
            "forearm_stub",
            "thumb0",
            "thumb1",
            "thumb2",
            "thumb3",
            "index1",
            "index2",
            "index3",
            "middle1",
            "middle2",
            "middle3",
            "ring1",
            "ring2",
            "ring3",
            "pinky0",
            "pinky1",
            "pinky2",
            "pinky3",
        };

        static readonly string[] FbxHandFingerNames =
        {
            "thumb",
            "index",
            "middle",
            "ring",
            "pinky",
        };

        public static void EnsureMapped(OVRCustomSkeleton skeleton)
        {
            if (skeleton == null)
                return;

            var bones = skeleton.CustomBones;
            if (bones == null || bones.Count == 0)
                return;

            if (bones[(int)OVRSkeleton.BoneId.Hand_WristRoot] != null)
                return;

            var skeletonType = skeleton.GetSkeletonType();
            if (skeletonType != OVRSkeleton.SkeletonType.HandLeft
                && skeletonType != OVRSkeleton.SkeletonType.HandRight)
            {
                Debug.LogWarning($"[QuestOvrCustomSkeletonRuntimeMap] Unexpected skeleton type {skeletonType}.");
                return;
            }

            var start = OVRSkeleton.BoneId.Hand_Start;
            var end = OVRSkeleton.BoneId.Hand_MaxSkinnable;
            var mapped = 0;

            for (var bi = (int)start; bi < (int)end; ++bi)
            {
                var boneId = (OVRSkeleton.BoneId)bi;
                var boneName = FbxBoneNameFromBoneId(skeletonType, boneId);
                var t = skeleton.transform.FindChildRecursive(boneName);
                if (t == null)
                    continue;

                bones[(int)boneId] = t;
                mapped++;
            }

            QuestOvrHandTrackingFlags.EnableRootPoseTracking(skeleton);

            Debug.Log($"[QuestOvrCustomSkeletonRuntimeMap] Mapped {mapped} custom bones on {skeleton.name} ({skeletonType}).");
        }

        static string FbxBoneNameFromBoneId(OVRSkeleton.SkeletonType skeletonType, OVRSkeleton.BoneId boneId)
        {
            var sideIndex = skeletonType == OVRSkeleton.SkeletonType.HandLeft ? 0 : 1;

            if (boneId >= OVRSkeleton.BoneId.Hand_ThumbTip && boneId <= OVRSkeleton.BoneId.Hand_PinkyTip)
            {
                var fingerIndex = (int)boneId - (int)OVRSkeleton.BoneId.Hand_ThumbTip;
                return HandSidePrefix[sideIndex] + FbxHandFingerNames[fingerIndex] + "_finger_tip_marker";
            }

            return FbxHandBonePrefix + HandSidePrefix[sideIndex] + FbxHandBoneNames[(int)boneId];
        }
    }

    internal static class QuestOvrHandTrackingFlags
    {
        public static void EnableRootPoseTracking(OVRSkeleton skeleton)
        {
            if (skeleton == null)
                return;

            var type = typeof(OVRSkeleton);
            type.GetField("_updateRootPose", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(skeleton, true);
            type.GetField("_updateRootScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(skeleton, true);
        }
    }
}
#endif
