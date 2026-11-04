#if LEXTING_FOUND_OCULUS_SDK && UNITY_ANDROID
using System.Reflection;
using UnityEngine;

namespace SpatialGestureText.Input
{
    /// <summary>
    /// On device, <see cref="OVRSkeleton"/> only calls <c>Initialize()</c> from <c>Start()</c>.
    /// If OVR/XR is not ready yet, <c>GetSkeleton2</c> fails and the skeleton never retries (unlike Editor Update).
    /// </summary>
    [DefaultExecutionOrder(50)]
    public sealed class QuestOvrSkeletonInitRetry : MonoBehaviour
    {
        OVRSkeleton _skeleton;
        MethodInfo _shouldInitialize;
        MethodInfo _initialize;
        int _attempts;

        void Awake()
        {
            _skeleton = GetComponent<OVRSkeleton>();
            if (_skeleton == null)
                return;

            var type = typeof(OVRSkeleton);
            _shouldInitialize = type.GetMethod("ShouldInitialize", BindingFlags.NonPublic | BindingFlags.Instance);
            _initialize = type.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        void Update()
        {
            if (_skeleton == null || _skeleton.IsInitialized || _shouldInitialize == null || _initialize == null)
                return;

            if (!OVRPlugin.initialized)
                return;

            if (_attempts > 1200)
                return;

            _attempts++;

            try
            {
                if ((bool)_shouldInitialize.Invoke(_skeleton, null))
                    _initialize.Invoke(_skeleton, null);
            }
            catch (MissingReferenceException)
            {
                enabled = false;
            }
        }
    }
}
#endif
