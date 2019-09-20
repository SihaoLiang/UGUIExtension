using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiBehaviorTest : UIBehaviour {

    /// <summary>
    ///   <para>See MonoBehaviour.Reset.</para>
    /// </summary>
    protected override void Reset()
    {
        Debug.LogError("Reset");
    }

    /// <summary>
    ///   <para>This callback is called if an associated RectTransform has its dimensions changed. The call is also made to all child rect transforms, even if the child transform itself doesn't change - as it could have, depending on its anchoring.</para>
    /// </summary>
    protected override void OnRectTransformDimensionsChange()
    {
        Debug.LogError("OnRectTransformDimensionsChange");

    }

    /// <summary>
    ///   <para>See MonoBehaviour.OnBeforeTransformParentChanged.</para>
    /// </summary>
    protected override void OnBeforeTransformParentChanged()
    {
        Debug.LogError("OnBeforeTransformParentChanged");
    }

    /// <summary>
    ///   <para>See MonoBehaviour.OnRectTransformParentChanged.</para>
    /// </summary>
    protected override void OnTransformParentChanged()
    {
        Debug.LogError("OnTransformParentChanged");
    }

    /// <summary>
    ///   <para>See UI.LayoutGroup.OnDidApplyAnimationProperties.</para>
    /// </summary>
    protected override void OnDidApplyAnimationProperties()
    {
        Debug.LogError("OnDidApplyAnimationProperties");
    }

    /// <summary>
    ///   <para>See MonoBehaviour.OnCanvasGroupChanged.</para>
    /// </summary>
    protected override void OnCanvasGroupChanged()
    {
        Debug.LogError("OnCanvasGroupChanged");

    }

    /// <summary>
    ///   <para>Called when the state of the parent Canvas is changed.</para>
    /// </summary>
    protected override void OnCanvasHierarchyChanged()
    {
        Debug.LogError("OnCanvasHierarchyChanged");
    }


    protected void OnTransformChildrenChanged()
    {
        Debug.Log("OnTransformChildrenChanged");
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        Debug.Log("OnValidate");
    }
#endif

}
