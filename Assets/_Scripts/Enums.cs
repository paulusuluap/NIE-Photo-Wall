using System;

public class Enums
{
    public enum MovementType
    {
        Fixed,
        Free
    }
    public enum MovementAxis
    {
        Horizontal,
        Vertical
    }
    public enum SizeControl
    {
        Manual,
        Fit
    }
    public enum SnapTarget
    {
        Nearest,
        Previous,
        Next
    }
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum StripDirection
    {
        Vertical,
        Horizontal
    }

    public enum ScrollType
    {
        Normal,
        FilmStrip
    }

    [Serializable]
    public class Margins
    {
        public float Left, Right, Top, Bottom;

        public Margins(float m)
        {
            Left = Right = Top = Bottom = m;
        }
        public Margins(float x, float y)
        {
            Left = Right = x;
            Top = Bottom = y;
        }
        public Margins(float l, float r, float t, float b)
        {
            Left = l;
            Right = r;
            Top = t;
            Bottom = b;
        }
    }
}
