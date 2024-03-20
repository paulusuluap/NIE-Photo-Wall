using UnityEngine;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleScrollSnap
{
    public class Recolour : TransitionEffectBase<Graphic>
    {
        public override void OnTransition(Graphic graphic, float colour)
        {
            //graphic.color = new Color(red, graphic.color.g, graphic.color.b, graphic.color.a);
            graphic.transform.GetChild(0).GetComponent<Graphic>().color = new Color(colour, colour, colour, graphic.color.a);
        }
    }
}
