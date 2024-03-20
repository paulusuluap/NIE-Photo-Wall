// Simple Scroll-Snap - https://assetstore.unity.com/packages/tools/gui/simple-scroll-snap-140884
// Copyright (c) Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.SimpleScrollSnap
{
    public class Scale : TransitionEffectBase<RectTransform>
    {
        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = Vector3.one * scale;
            //rectTransform.localScale = new Vector3(Mathf.Lerp(1f, 1.197f, scale), Mathf.Lerp(1f, 1.197f, scale));
            //rectTransform.SetSiblingIndex(scale > 0.3f ? rectTransform.parent.childCount - 1 : 0);
        }
    }
}