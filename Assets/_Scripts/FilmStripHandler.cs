using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class FilmStripHandler : MonoBehaviour
{
    public StripDirection _stripDirection;

    public List<RectTransform> strips = new List<RectTransform>();
    private List<RectTransform> tails = new List<RectTransform>();

    public Transform holder;

    private VerticalAnimation vertical;
    private HorizontalAnimation horizontal;

    public float offset;


    private void Start()
    {
        for (int i = 0; i < strips.Count; i++)
        {
            var tail = Instantiate(strips[i], holder);
            tails.Add(tail);
        }

        switch(_stripDirection)
        {
            case StripDirection.Horizontal:
                horizontal = this.GetComponent<HorizontalAnimation>();

                for (int i = 0; i < strips.Count; i++)
                {
                    //even
                    if (i % 2 == 0){
                        tails[i].anchoredPosition = new Vector2(strips[i].rect.width, strips[i].anchoredPosition.y);
                    }
                    //odd
                    else {
                        tails[i].anchoredPosition = new Vector2(-strips[i].rect.width, strips[i].anchoredPosition.y);
                    }
                }                
                break;
            case StripDirection.Vertical:
                vertical = this.GetComponent<VerticalAnimation>();

                for (int i = 0; i < strips.Count; i++)
                {
                    //even
                    if (i % 2 == 0)
                    {
                        tails[i].anchoredPosition = new Vector2(strips[i].anchoredPosition.x, -strips[i].rect.height);
                    }
                    //odd
                    else
                    {
                        tails[i].anchoredPosition = new Vector2(strips[i].anchoredPosition.x, strips[i].rect.height);
                    }
                }
                break;
        }
    }

    private void Update()
    {
        switch (_stripDirection)
        {
            case StripDirection.Horizontal:
                if (!horizontal.IsPlaying) return;
                for (int i = 0; i < strips.Count; i++)
                {
                    if (i % 2 == 0) {
                        //even
                        PlayStripHorizontal(strips[i], tails[i], false);
                    }
                    else {
                        //odd
                        PlayStripHorizontal(strips[i], tails[i], true);
                    }
                }
                break;
            case StripDirection.Vertical:
                if (!vertical.IsPlaying) return;
                for (int i = 0; i < strips.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        //even
                        PlayStripVertical(strips[i], tails[i], true);
                    }
                    else
                    {
                        //odd
                        PlayStripVertical(strips[i], tails[i], false);
                    }
                }
                break;
        }
    }

    private void PlayStripHorizontal(RectTransform original, RectTransform tail, bool isMovingRight)
    {
        if (isMovingRight)
        {
            //original photo collage animation
            if (original.anchoredPosition.x < original.rect.width)
            {
                var _speed = GlobalVariable.filmStrip_middle_horizontalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.rect.width, original.anchoredPosition.y), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(tail.anchoredPosition.x - tail.rect.width - offset, original.anchoredPosition.y);
            }

            //tail photo collage animation
            if (tail.anchoredPosition.x < original.rect.width)
            {
                var _speed = GlobalVariable.filmStrip_middle_horizontalSpeed * Time.deltaTime;
                tail.anchoredPosition = Vector2.MoveTowards(tail.anchoredPosition, new Vector2(tail.rect.width, tail.anchoredPosition.y), _speed);
            }
            else
            {
                tail.anchoredPosition = new Vector2(original.anchoredPosition.x - original.rect.width - offset, tail.anchoredPosition.y);
            }
        }
        else
        {
            //original photo collage animation
            if (original.anchoredPosition.x > -original.rect.width)
            {
                var _speed = GlobalVariable.filmStrip_topBottom_horizontalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(-original.rect.width, original.anchoredPosition.y), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(tail.anchoredPosition.x + tail.rect.width + offset, original.anchoredPosition.y);
            }

            //tail photo collage animation
            if (tail.anchoredPosition.x > -tail.rect.width)
            {
                var _speed = GlobalVariable.filmStrip_topBottom_horizontalSpeed * Time.deltaTime;
                tail.anchoredPosition = Vector2.MoveTowards(tail.anchoredPosition, new Vector2(-tail.rect.width, tail.anchoredPosition.y), _speed);
            }
            else
            {
                tail.anchoredPosition = new Vector2(original.anchoredPosition.x + original.rect.width + offset, tail.anchoredPosition.y);
            }
        }
    }


    private void PlayStripVertical(RectTransform original, RectTransform tail, bool isMovingUpward)
    {
        if (isMovingUpward)
        {
            //original photo collage animation
            if (original.anchoredPosition.y < original.rect.height)
            {
                var _speed = GlobalVariable.filmStrip_verticalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.anchoredPosition.x, original.rect.height), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(original.anchoredPosition.x, tail.anchoredPosition.y - tail.rect.height - offset);
            }

            //tail photo collage animation
            if (tail.anchoredPosition.y < tail.rect.height)
            {
                var _speed = GlobalVariable.filmStrip_verticalSpeed * Time.deltaTime;
                tail.anchoredPosition = Vector2.MoveTowards(tail.anchoredPosition, new Vector2(tail.anchoredPosition.x, tail.rect.height), _speed);
            }
            else
            {
                tail.anchoredPosition = new Vector2(tail.anchoredPosition.x, original.anchoredPosition.y - original.rect.height - offset);
            }
        }
        else
        {
            //original photo collage animation
            if (original.anchoredPosition.y > -original.rect.height)
            {
                var _speed = GlobalVariable.filmStrip_verticalSpeed * Time.deltaTime;
                original.anchoredPosition = Vector2.MoveTowards(original.anchoredPosition, new Vector2(original.anchoredPosition.x, -original.rect.height), _speed);
            }
            else
            {
                original.anchoredPosition = new Vector2(original.anchoredPosition.x, tail.anchoredPosition.y + tail.rect.height + offset);
            }

            //tail photo collage animation
            if (tail.anchoredPosition.y > -tail.rect.height)
            {
                var _speed = GlobalVariable.filmStrip_verticalSpeed * Time.deltaTime;
                tail.anchoredPosition = Vector2.MoveTowards(tail.anchoredPosition, new Vector2(tail.anchoredPosition.x, -tail.rect.height), _speed);
            }
            else
            {
                tail.anchoredPosition = new Vector2(tail.anchoredPosition.x, original.anchoredPosition.y + original.rect.height + offset);
            }
        }
    }
}
