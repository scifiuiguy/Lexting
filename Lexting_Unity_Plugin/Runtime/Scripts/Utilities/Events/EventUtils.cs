using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Events;
using SpatialGestureText.Input;
using SpatialGestureText.LetterCube;
using SpatialGestureText.Zones;
using System;
using System.Reflection;
using System.Linq.Expressions;
using SpatialGestureText;

public class SpatialGestureEvent : UnityEvent { }
public class SpatialGestureEvent<T0> : UnityEvent<T0> { }
public class SpatialGestureEvent<T0, T1> : UnityEvent<T0, T1> { }
public class SpatialGestureEvent<T0, T1, T2> : UnityEvent<T0, T1, T2> { }
public class SpatialGestureEvent<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3> { }

public static class EventUtils
{
    #region Spatial Gesture Utils
    public static void GetGestures(this MonoBehaviour behaviour, UnityAction<bool, HandGestureState, HandGestureState> callback)
    {
        Subscribe(behaviour, SpatialGestureInput.Instance.OnStateChanged, callback);
    }

    public static void GetGestures(
        this MonoBehaviour behaviour,
        UnityAction<bool, HandGestureState, HandGestureState> gestureCallback,
        UnityAction<bool, CubeRotationState, CubeRotationState> rotationCallback)
    {
        Subscribe(behaviour, SpatialGestureInput.Instance.OnStateChanged, gestureCallback);
        Subscribe(behaviour, SpatialGestureInput.Instance.OnRotationStateChanged, rotationCallback);
    }

    public static void Register(this MonoBehaviour behaviour, UnityEvent thisEvent, UnityAction callback)
    {
        behaviour.OnEnableAsObservable().Subscribe(_ => thisEvent.AddListener(callback));
        behaviour.OnDisableAsObservable().Subscribe(_ => thisEvent.RemoveListener(callback));
    }

    /// <summary>
    /// Auto-instantiates an update loop that begins loop upon onEvent trigger and ceases looping upon offEvent trigger
    /// </summary>
    /// <param name="behaviour">the behaviour that hosts the update loop</param>
    /// <param name="onEvent">the event that triggers the update loop to begin</param>
    /// <param name="offEvent">the event that triggers the update loop to end</param>
    /// <param name="callback">the code that executes upon each iteration of the update loop</param>
    public static void RegisterUpdate(this MonoBehaviour behaviour, UnityEvent onEvent, UnityEvent offEvent, UnityAction callback)
    {
        IDisposable _update = null; 
        behaviour.Register(onEvent, () => _update = behaviour.UpdateAsObservable().Subscribe(_ => callback.I()));
        behaviour.Register(offEvent, () => _update?.Dispose());
    }

    /// <summary>
    /// Subscribes any MonoBehaviour to ALL keyboard output callbacks executed by the LextingOutputManager.
    /// 
    /// You can choose to subscribe to any one event in your own script like this:
    /// this.Subscribe(LextingOutputManager.Instance.OnTab, ()=>{/*your code here when TAB is pressed*/});
    /// 
    /// OR you can subscribe to all keyboard events at once with this extension method:
    ///
    /// /////// COPY THIS AND PASTE IT IN YOUR AWAKE METHOD:
    // this.GetLextingOutput(
    //     ()=>{ /*when TAB is activated, execute your code here*/ },
    //     ()=>{ /*when Alt+TAB is activated, execute your code here*/ },
    //     ()=>{ /*when Alt+Shift+TAB is activated, execute your code here*/ },
    //     ()=>{ /*when Enter is activated, execute your code here*/ },
    //     ()=>{ /*when Delete is activated, execute your code here*/ },
    //     ()=>{ /*when Backspace is activated, execute your code here*/ },
    //     (numCharsRemoved)=>{ /*when BackWord is activated, execute your code here*/ },
    //     ()=>{ /*when Undo is activated, execute your code here*/ },
    //     ()=>{ /*when Redo is activated, execute your code here*/ },
    //     (direction)=>{ /*when Cursor Single-Char Move is activated, execute your code here*/ },
    //     (direction, highlightedString)=>{ /*when Cursor Single-Char Highlight is activated, execute your code here*/ },
    //     (direction, numCharsJustSelected, highlightedString)=>{ /*when Cursor Word Highlight is activated, execute your code here*/ },
    //     (on)=>{/*when SHIFT is toggled, execute your code here*/},
    //     (on)=>{/*when CAPS is toggled, execute your code here*/},
    //     (key)=>{/*when a KEY (alpha, numeric, or special char) is toggled, execute your code here*/});
    // 
    /// OR TRY THE SAME METHOD MINIFIED:
    // this.GetLextingOutput(()=>{ /*TAB*/ }, ()=>{ /*Alt+TAB */ }, ()=>{ /*Alt+Shift+TAB*/ }, ()=>{ /*Enter*/ }, 
    //     ()=>{ /*Delete*/ }, ()=>{ /*Backspace*/ }, (numCharsRemoved)=>{ /*BackWord*/ }, ()=>{ /*Undo*/ }, ()=>{ /*Redo*/ },
    //     (direction)=>{ /*Cursor Single-Char Move*/ }, (direction, highlightedString)=>{ /*Cursor Single-Char Highlight*/ },
    //     (direction, numCharsJustSelected, highlightedString)=>{ /*Cursor Word Highlight */ },
    //     (on)=>{/*SHIFT*/}, (on)=>{/*CAPS*/}, (key)=>{/*KEY*/});
    ///     
    /// OR MINIFIED W/ NO COMMENTS:
    // this.GetLextingOutput(()=>{},()=>{},()=>{},()=>{},()=>{},()=>{},(numCharsRemoved)=>{},()=>{},()=>{},(direction)=>{},
    //     (direction, highlightedString)=>{},(direction, numCharsJustSelected, highlightedString)=>{},(on)=>{},(on)=>{},(key)=>{});
    ///     
    /// ///////
    /// </summary>
    public static void GetLextingOutput(
        this MonoBehaviour behaviour,
        UnityAction tabCallback,
        UnityAction altTabCallback,
        UnityAction altShiftTabCallback,
        UnityAction enterCallback,
        UnityAction deleteCallback,
        UnityAction backspaceCallback,
        UnityAction<int> backWordCallback,
        UnityAction undoCallback,
        UnityAction redoCallback,
        UnityAction<ArrowKey> cursorMoveCallback,
        UnityAction<ArrowKey, string> cursorShiftMoveCallback,
        UnityAction<ArrowKey, int, string> cursorShiftWordMoveCallback,
        UnityAction<bool> altCallback,
        UnityAction<bool> ctrlCallback,
        UnityAction<bool> shiftCallback,
        UnityAction<bool> capsCallback,
        UnityAction<string> keyCallback)
    {
        Subscribe(behaviour, LextingOutputManager.Instance.OnTab, tabCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnAltTab, altTabCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnAltShiftTab, altShiftTabCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnEnter, enterCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnDelete, deleteCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnBackspace, backspaceCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnBackWord, backWordCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnUndo, undoCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnRedo, redoCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnCursorMove, cursorMoveCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnCursorShiftMove, cursorShiftMoveCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnCursorShiftWordMove, cursorShiftWordMoveCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnAlt, altCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnCtrl, ctrlCallback);

        Subscribe(behaviour, LextingOutputManager.Instance.OnShift, shiftCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnCaps, capsCallback);
        Subscribe(behaviour, LextingOutputManager.Instance.OnKey, keyCallback);
    }


    #endregion Spatial Gesture Utils

    #region Collision Detection
    /// <summary>
    /// Sets up trigger entry and exit detection of any game object whose tag contains tagSegment (receieve the whole tag in the callback)
    /// </summary>
    public static void HandleTriggerWhoseTagContains(
        this MonoBehaviour behaviour, 
        string tagSegment, 
        UnityAction<GameObject> entryCallback, 
        UnityAction<GameObject> exitCallback)
    {
        behaviour.OnTriggerEnterAsObservable().Where(otherCollider => otherCollider.tag.Contains(tagSegment))
            .Subscribe((otherCollider) => entryCallback.I(otherCollider.gameObject));
        behaviour.OnTriggerExitAsObservable().Where(otherCollider => otherCollider.tag.Contains(tagSegment))
            .Subscribe((otherCollider) => exitCallback.I(otherCollider.gameObject));
    }

    /// <summary>
    /// Sets up trigger entry and exit detection of any game object whose tag contains tagSegment and also specifies handedness
    /// by observing it in the tag's name
    /// </summary>
    public static void HandleHandedTriggerWhoseTagContains(
        this MonoBehaviour behaviour,
        string tagSegment,
        UnityAction<bool,GameObject> entryCallback,
        UnityAction<bool,GameObject> exitCallback)
    {
        behaviour.OnTriggerEnterAsObservable().Where(otherCollider => otherCollider.tag.Contains(tagSegment))
            .Subscribe((otherCollider) => {
                if (otherCollider.tag.ToLower().Contains("right") || otherCollider.tag.ToLower().Contains("left"))
                    entryCallback.I(otherCollider.tag.ToLower().Contains("right"), otherCollider.gameObject);
            });
        behaviour.OnTriggerExitAsObservable().Where(otherCollider => otherCollider.tag.Contains(tagSegment))
            .Subscribe((otherCollider) => {
                if (otherCollider.tag.ToLower().Contains("right") || otherCollider.tag.ToLower().Contains("left"))
                    exitCallback.I(otherCollider.tag.ToLower().Contains("right"), otherCollider.gameObject);
            });
    }

    /// <summary>
    /// Sets up trigger entry and exit detection of any game object whose tag contains tagSegment (receieve the whole tag in the callback)
    /// </summary>
    public static void HandleTriggerWithTag(
        this MonoBehaviour behaviour,
        string tagSegment,
        UnityAction<GameObject> entryCallback,
        UnityAction<GameObject> exitCallback)
    {
        behaviour.OnTriggerEnterAsObservable().Where(otherCollider => otherCollider.tag.Equals(tagSegment))
            .Subscribe((otherCollider) => entryCallback.I(otherCollider.gameObject));
        behaviour.OnTriggerExitAsObservable().Where(otherCollider => otherCollider.tag.Equals(tagSegment))
            .Subscribe((otherCollider) => exitCallback.I(otherCollider.gameObject));
    }


    public static void HandleTrigger(this MonoBehaviour behaviour, Type target, UnityAction<GameObject> entryCallback, UnityAction<GameObject> exitCallback)
    { 
        behaviour.OnTriggerEnterAsObservable().Where(otherCollider => otherCollider.transform.GetComponent(target) != null)
            .Subscribe(otherCollider => entryCallback.I(otherCollider.gameObject));
        behaviour.OnTriggerExitAsObservable().Where(otherCollider => otherCollider.transform.GetComponent(target) != null)
            .Subscribe(otherCollider => entryCallback.I(otherCollider.gameObject));
    }
    #endregion Collision Detection

    #region raycasting
    public static void Raycast<T>(this MonoBehaviour behaviour, UnityAction<T> hitObject, UnityAction<T> lostObject)
    {
        IDisposable update;
        RaycastHit hit, previousHit;
        previousHit = new RaycastHit();
        bool isHovering = false;
        update = behaviour.UpdateAsObservable().Subscribe(_ =>
                HandleRaycast<T>(ref isHovering, behaviour, out hit, ref previousHit, hitObject, lostObject));
        behaviour.OnEnableAsObservable().Subscribe(_ => {
            if (update == null)
                update = behaviour.UpdateAsObservable().Subscribe(_ =>
                    HandleRaycast(ref isHovering, behaviour, out hit, ref previousHit, hitObject, lostObject));
        });
        behaviour.OnDisableAsObservable().Subscribe(_ => update?.Dispose());
    }

    public static void HandleRaycast<T>(
        ref bool isHovering, MonoBehaviour behaviour, out RaycastHit hit, ref RaycastHit previousHit,
         UnityAction<T> hitObject, UnityAction<T> lostObject)
    {
        if (MustToggleRaycastState<T>(isHovering, behaviour, out hit, previousHit))
        {
            isHovering = !isHovering;
            if (isHovering) hitObject?.I(hit.transform.GetComponent<T>());
            else if (previousHit.transform.Ready())
                lostObject?.I(previousHit.transform.GetComponent<T>());
            previousHit = hit;
        }
    }

    private static bool MustToggleRaycastState<T>(bool isHovering, MonoBehaviour behaviour,
        out RaycastHit hit, RaycastHit previousHit)
    {
        return isHovering != DidRaycastHitTarget<T>(behaviour, out hit) ||
            (isHovering && IsNew(hit, previousHit));
    }

    /// <summary>
    /// Identifies when a raycast hit immediately immediately from one colliding object to another
    /// </summary>
    private static bool IsNew(RaycastHit current, RaycastHit previous)
    {
        return current.transform.Ready()
            && !current.transform.GetInstanceID().Equals(previous.transform.GetInstanceID());
    }

    public static bool DidRaycastHitTarget<T>(MonoBehaviour behaviour, out RaycastHit hit)
    {
        Physics.Raycast(behaviour.transform.position, behaviour.transform.forward, out hit);
        return hit.transform.Ready() && hit.transform.HasComponent<T>();
    }
    #endregion raycasting

    #region Variable & Property Update tracking
    //private static Queue<IDisposable> changeObservers;
    //public static void OnChange(this MonoBehaviour behavior, string propertyName, UnityAction callback)
    //{
    //    behavior.OnEnableAsObservable().Subscribe(_ => {
    //        changeObservers.Enqueue(
    //            behavior.ObserveEveryValueChanged(x => x.GetType().GetProperty(propertyName))
    //                .Subscribe(pValue => callback?.I())
    //        );
    //    });
    //    behavior.OnDisableAsObservable().Subscribe(_=>changeObservers.Dequeue().Dispose());
    //}

    //public static void OnChange<T>(this T property, MonoBehaviour behavior, UnityAction<T> callback)
    //{
    //    behavior.OnEnableAsObservable().Subscribe(_ => {
    //        changeObservers.Enqueue(
    //            behavior.ObserveEveryValueChanged(x => property).Subscribe(pValue => callback?.I(pValue))
    //        );
    //    });
    //    behavior.OnDisableAsObservable().Subscribe(_ => changeObservers.Dequeue().Dispose());
    //}
    #endregion Variable & Property Update tracking

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I(this UnityEvent unityEvent)
    {
        unityEvent?.Invoke();
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T>(this UnityEvent<T> unityEvent, T param1)
    {
        unityEvent?.Invoke(param1);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T, T2>(this UnityEvent<T, T2> unityEvent, T param1, T2 param2)
    {
        unityEvent?.Invoke(param1, param2);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T, T2, T3>(this UnityEvent<T, T2, T3> unityEvent, T param1, T2 param2, T3 param3)
    {
        unityEvent?.Invoke(param1, param2, param3);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T, T2, T3, T4>(this UnityEvent<T, T2, T3, T4> unityEvent, T param1, T2 param2, T3 param3, T4 param4)
    {
        unityEvent?.Invoke(param1, param2, param3, param4);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I(this UnityAction action)
    {
        action?.Invoke();
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T>(this UnityAction<T> action, T param1)
    {
        action?.Invoke(param1);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T, T2>(this UnityAction<T, T2> action, T param1, T2 param2)
    {
        action?.Invoke(param1, param2);
    }

    /// <summary>
    /// Null-coallescing Invoke shortcut
    /// </summary>
    public static void I<T, T2, T3>(this UnityAction<T, T2, T3> action, T param1, T2 param2, T3 param3)
    {
        action?.Invoke(param1, param2, param3);
    }

    /// <summary>
    /// Invoke shortcut
    /// </summary>
    public static void I<T, T2, T3, T4>(this UnityAction<T, T2, T3, T4> action, T param1, T2 param2, T3 param3, T4 param4)
    {
        action?.Invoke(param1, param2, param3, param4);
    }

    

    #region generic event binding
    /// <summary>
    /// A MonoBehaviour extension method which allows a script to quickly subscribe its local UnityAction to any external UnityEvent instance
    /// </summary>
    /// <param name="receiverBehaviour">the behaviour that is about to subscribe</param>
    /// <param name="sender">the external event instance</param>
    /// <param name="receiver">the UnityAction designated to execute when the event is triggered</param>
    public static void Subscribe(this MonoBehaviour receiverBehaviour, UnityEvent sender, UnityAction receiver)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender?.AddListener(receiver));
        receiverBehaviour.OnDisableAsObservable().Subscribe(x => sender?.RemoveListener(receiver));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour, UnityEvent<T0> sender, UnityAction<T0> receiver)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender?.AddListener(receiver));
        receiverBehaviour.OnDisableAsObservable().Subscribe(x => sender?.RemoveListener(receiver));
    }
    
    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour, UnityEvent<T0, T1> sender, UnityAction<T0, T1> receiver)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender?.AddListener(receiver));
        receiverBehaviour.OnDisableAsObservable().Subscribe(x => sender?.RemoveListener(receiver));
    }
    
    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour, UnityEvent<T0, T1, T2> sender, UnityAction<T0, T1, T2> receiver)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender?.AddListener(receiver));
        receiverBehaviour.OnDisableAsObservable().Subscribe(x => sender?.RemoveListener(receiver));
    }
    
    public static void Subscribe<T0, T1, T2, T3>(this MonoBehaviour receiverBehaviour, UnityEvent<T0, T1, T2, T3> sender, UnityAction<T0, T1, T2, T3> receiver)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender?.AddListener(receiver));
        receiverBehaviour.OnDisableAsObservable().Subscribe(x => sender?.RemoveListener(receiver));
    }


    public static void Subscribe(this MonoBehaviour receiverBehaviour, 
        UnityEvent sender0, UnityAction receiver0, 
        UnityEvent sender1, UnityAction receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    public static void Subscribe(this MonoBehaviour receiverBehaviour,
        UnityEvent sender0, UnityAction receiver0,
        UnityEvent sender1, UnityAction receiver1,
        UnityEvent sender2, UnityAction receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe(this MonoBehaviour receiverBehaviour,
        UnityEvent sender0, UnityAction receiver0,
        UnityEvent sender1, UnityAction receiver1,
        UnityEvent sender2, UnityAction receiver2,
        UnityEvent sender3, UnityAction receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe(this MonoBehaviour receiverBehaviour,
        UnityEvent sender0, UnityAction receiver0,
        UnityEvent sender1, UnityAction receiver1,
        UnityEvent sender2, UnityAction receiver2,
        UnityEvent sender3, UnityAction receiver3,
        UnityEvent sender4, UnityAction receiver4)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender4?.AddListener(receiver4));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender4?.RemoveListener(receiver4));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T2> sender2, UnityAction<T2> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T0> sender3, UnityAction<T0> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T1> sender3, UnityAction<T1> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2,
        UnityEvent<T0> sender3, UnityAction<T0> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T0> sender3, UnityAction<T0> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T1> sender3, UnityAction<T1> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2,
        UnityEvent<T0> sender3, UnityAction<T0> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2,
        UnityEvent<T1> sender3, UnityAction<T1> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T1> sender2, UnityAction<T1> receiver2,
        UnityEvent<T2> sender3, UnityAction<T2> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T1> sender1, UnityAction<T1> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T2> sender3, UnityAction<T2> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
    UnityEvent<T0> sender0, UnityAction<T0> receiver0,
    UnityEvent<T1> sender1, UnityAction<T1> receiver1,
    UnityEvent<T1> sender2, UnityAction<T1> receiver2,
    UnityEvent<T2> sender3, UnityAction<T2> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
    UnityEvent<T0> sender0, UnityAction<T0> receiver0,
    UnityEvent<T1> sender1, UnityAction<T1> receiver1,
    UnityEvent<T2> sender2, UnityAction<T2> receiver2,
    UnityEvent<T0> sender3, UnityAction<T0> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
    UnityEvent<T0> sender0, UnityAction<T0> receiver0,
    UnityEvent<T1> sender1, UnityAction<T1> receiver1,
    UnityEvent<T2> sender2, UnityAction<T2> receiver2,
    UnityEvent<T1> sender3, UnityAction<T1> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
    UnityEvent<T0> sender0, UnityAction<T0> receiver0,
    UnityEvent<T1> sender1, UnityAction<T1> receiver1,
    UnityEvent<T2> sender2, UnityAction<T2> receiver2,
    UnityEvent<T2> sender3, UnityAction<T2> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0> sender0, UnityAction<T0> receiver0,
        UnityEvent<T0> sender1, UnityAction<T0> receiver1,
        UnityEvent<T0> sender2, UnityAction<T0> receiver2,
        UnityEvent<T0> sender3, UnityAction<T0> receiver3,
        UnityEvent<T0> sender4, UnityAction<T0> receiver4)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender4?.AddListener(receiver4));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender4?.RemoveListener(receiver4));
    }

    public static void Subscribe<T0>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T0> sender0, UnityAction<T0, T0> receiver0,
        UnityEvent<T0, T0> sender1, UnityAction<T0, T0> receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1> sender0, UnityAction<T0, T1> receiver0,
        UnityEvent<T0, T1> sender1, UnityAction<T0, T1> receiver1,
        UnityEvent<T0, T1> sender2, UnityAction<T0, T1> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T0> sender0, UnityAction<T0, T0> receiver0,
        UnityEvent<T0, T0> sender1, UnityAction<T0, T0> receiver1,
        UnityEvent<T0, T0> sender2, UnityAction<T0, T0> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

  
    
    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1> sender0, UnityAction<T0, T1> receiver0,
        UnityEvent<T0, T1> sender1, UnityAction<T0, T1> receiver1,
        UnityEvent<T0, T1> sender2, UnityAction<T0, T1> receiver2,
        UnityEvent<T0, T1> sender3, UnityAction<T0, T1> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1> sender0, UnityAction<T0, T1> receiver0,
        UnityEvent<T0, T1> sender1, UnityAction<T0, T1> receiver1,
        UnityEvent<T0, T1> sender2, UnityAction<T0, T1> receiver2,
        UnityEvent<T0, T1> sender3, UnityAction<T0, T1> receiver3,
        UnityEvent<T0, T1> sender4, UnityAction<T0, T1> receiver4)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender4?.AddListener(receiver4));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender4?.RemoveListener(receiver4));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2> sender0, UnityAction<T0, T1, T2> receiver0,
        UnityEvent<T0, T1, T2> sender1, UnityAction<T0, T1, T2> receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2> sender0, UnityAction<T0, T1, T2> receiver0,
        UnityEvent<T0, T1, T2> sender1, UnityAction<T0, T1, T2> receiver1,
        UnityEvent<T0, T1, T2> sender2, UnityAction<T0, T1, T2> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2> sender0, UnityAction<T0, T1, T2> receiver0,
        UnityEvent<T0, T1, T2> sender1, UnityAction<T0, T1, T2> receiver1,
        UnityEvent<T0, T1, T2> sender2, UnityAction<T0, T1, T2> receiver2,
        UnityEvent<T0, T1, T2> sender3, UnityAction<T0, T1, T2> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2> sender0, UnityAction<T0, T1, T2> receiver0,
        UnityEvent<T0, T1, T2> sender1, UnityAction<T0, T1, T2> receiver1,
        UnityEvent<T0, T1, T2> sender2, UnityAction<T0, T1, T2> receiver2,
        UnityEvent<T0, T1, T2> sender3, UnityAction<T0, T1, T2> receiver3,
        UnityEvent<T0, T1, T2> sender4, UnityAction<T0, T1, T2> receiver4)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender4?.AddListener(receiver4));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender4?.RemoveListener(receiver4));
    }

    public static void Subscribe<T0, T1, T2, T3>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2, T3> sender0, UnityAction<T0, T1, T2, T3> receiver0,
        UnityEvent<T0, T1, T2, T3> sender1, UnityAction<T0, T1, T2, T3> receiver1)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
    }

    public static void Subscribe<T0, T1, T2, T3>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2, T3> sender0, UnityAction<T0, T1, T2, T3> receiver0,
        UnityEvent<T0, T1, T2, T3> sender1, UnityAction<T0, T1, T2, T3> receiver1,
        UnityEvent<T0, T1, T2, T3> sender2, UnityAction<T0, T1, T2, T3> receiver2)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
    }

    public static void Subscribe<T0, T1, T2, T3>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2, T3> sender0, UnityAction<T0, T1, T2, T3> receiver0,
        UnityEvent<T0, T1, T2, T3> sender1, UnityAction<T0, T1, T2, T3> receiver1,
        UnityEvent<T0, T1, T2, T3> sender2, UnityAction<T0, T1, T2, T3> receiver2,
        UnityEvent<T0, T1, T2, T3> sender3, UnityAction<T0, T1, T2, T3> receiver3)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
    }

    public static void Subscribe<T0, T1, T2, T3>(this MonoBehaviour receiverBehaviour,
        UnityEvent<T0, T1, T2> sender0, UnityAction<T0, T1, T2> receiver0,
        UnityEvent<T0, T1, T2> sender1, UnityAction<T0, T1, T2> receiver1,
        UnityEvent<T0, T1, T2> sender2, UnityAction<T0, T1, T2> receiver2,
        UnityEvent<T0, T1, T2> sender3, UnityAction<T0, T1, T2> receiver3,
        UnityEvent<T0, T1, T2> sender4, UnityAction<T0, T1, T2> receiver4)
    {
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender0?.AddListener(receiver0));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender0?.RemoveListener(receiver0));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender1?.AddListener(receiver1));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender1?.RemoveListener(receiver1));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender2?.AddListener(receiver2));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender2?.RemoveListener(receiver2));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender3?.AddListener(receiver3));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender3?.RemoveListener(receiver3));
        receiverBehaviour.OnEnableAsObservable().Subscribe(_ => sender4?.AddListener(receiver4));
        receiverBehaviour.OnDisableAsObservable().Subscribe(_ => sender4?.RemoveListener(receiver4));
    }
    #endregion generic event binding

    #region reactive property shortcuts
    /// <summary>
    /// Shortcut to subscribe to a reactive property such that it auto-disposes itself in OnDisable
    /// </summary>
    public static void Subscribe<T>(this MonoBehaviour behaviour, ReactiveProperty<T> reactiveProperty, UnityAction<T> callback)
    {
        IDisposable subscription = reactiveProperty.Subscribe((value) => callback.I(value));
        behaviour.OnDisableAsObservable().Subscribe(_ =>subscription.Dispose());
    }
    #endregion reactive property shortcuts


}



