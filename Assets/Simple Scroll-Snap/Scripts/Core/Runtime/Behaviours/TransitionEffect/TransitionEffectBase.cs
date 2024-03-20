// Simple Scroll-Snap - https://assetstore.unity.com/packages/tools/gui/simple-scroll-snap-140884
// Copyright (c) Daniel Lochner

using System.Collections.Generic;
using UnityEngine;

namespace DanielLochner.Assets.SimpleScrollSnap
{
    [RequireComponent(typeof(SimpleScrollSnap))]
    public abstract class TransitionEffectBase<T> : MonoBehaviour where T : Component
    {
        #region Fields
        [SerializeField] protected MinMax minMaxDisplacement = new MinMax(-1000, 1000);
        [SerializeField] protected MinMax minMaxValue = new MinMax(0, 1);
        [SerializeField] protected AnimationCurve function = AnimationCurve.Linear(0, 0, 1, 1);

        private Dictionary<GameObject, T> cachedComponents = new Dictionary<GameObject, T>();
        #endregion

        #region Methods
        public void OnTransition(GameObject panel, float displacement)
        {
            if (!cachedComponents.ContainsKey(panel))
            {
                cachedComponents.Add(panel, panel.GetComponent<T>());
            }

            float t = Mathf.InverseLerp(minMaxDisplacement.min, minMaxDisplacement.max, displacement);
            float f = function.Evaluate(t);
            float v = Mathf.Lerp(minMaxValue.min, minMaxValue.max, f);

            //panel.transform.position = new Vector3(panel.transform.position.x, panel.transform.position.y, Mathf.Lerp(0f, -1.5f, f));
            panel.transform.localScale = new Vector3(Mathf.Lerp(1f, 1.197f, v), Mathf.Lerp(1f, 1.197f, v), panel.transform.localScale.z);
            panel.transform.SetSiblingIndex(v > 0.3f ? panel.transform.parent.childCount - 1 : 0);
            //Debug.Log($"{panel.name} and {v}");

            OnTransition(cachedComponents[panel], v);
        }
        public abstract void OnTransition(T component, float value);
        #endregion
    }
}