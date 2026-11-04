using UnityEngine;

namespace SpatialGestureText
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown = false;
        private static object m_Lock = new object();
        private static T m_Instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        m_Instance = (T)FindObjectOfType(typeof(T));
                       

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            /// I hate that most Unity singleton implementations indiscriminately 
                            /// auto-gen themselves. It helps never, and causes problems often.
                            /// 
                            /// If the singleton isn't instanced on time by editor config
                            /// or code, throw an error or warning so race conditions can addressed.
                            /// 
                            /// Auto-gen causes singletons to init in the wrong hierarchy location,
                            /// which is often a nightmare.
                            /// 
                            /// Rant over. Back to our regularly scehduled programming.
                            /// ---AJ

                            //// Need to create a new GameObject to attach the singleton to.
                            //var singletonObject = new GameObject();
                            //m_Instance = singletonObject.AddComponent<T>();
                            //singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            //// Make instance persistent.
                            //DontDestroyOnLoad(singletonObject);
                            Debug.LogWarning($"A Lexting Singleton of type {typeof(T)} was called before it could" +
                                $"be created. All Lexting singetons should instantiate prior to being referenced. " +
                                $"It is highly recommended that you don't call any Lexting singleton instances " +
                                $"in Awake. Call them in Start or later. If you create an instance of the Lexting" +
                                $"UI after Start, wait at least one frame before calling its singletons.");
                        }
                    }

                    return m_Instance;
                }
            }
        }


        private void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }


        private void OnDestroy()
        {
            m_ShuttingDown = true;
        }
    }
}
