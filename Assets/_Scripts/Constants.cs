
using UnityEngine;

public class Constants : MonoBehaviour
{
    #region Custom Settings Keys

    public const string PHOTO_WALL_ANIMATION = "PHOTO_WALL_ANIMATION";
    public const string PHOTO_WALL_GALLERY_TYPE = "PHOTO_WALL_GALLERY_TYPE";
    public const string PHOTO_WALL_BANNER_CAPTION = "PHOTO_WALL_BANNER_CAPTION";
    public const string PHOTO_WALL_LARGE_PICTURE_DURATION = "PHOTO_WALL_LARGE_PICTURE_DURATION";

    public const int MAXSIZE_4X2 = 8;
    public const int MAXSIZE_4X3 = 12;
    public const int MAXSIZE_4X4 = 16;
    public const int MAXSIZE_5X3 = 15;
    public const int MAXSIZE_5X4 = 20;
    public const int MAXSIZE_6X3 = 18;
    public const int MAXSIZE_6X4 = 24;

    public const int CONSTRAINT_4 = 4;
    public const int CONSTRAINT_5 = 5;
    public const int CONSTRAINT_6 = 6;

    public static Vector2 SIZE_4X2 = new Vector2(840f, 472.5f);
    public static Vector2 SIZE_4X3 = new Vector2(800f, 450f);
    public static Vector2 SIZE_4X3_BANNER = new Vector2(672f, 378f);
    public static Vector2 SIZE_4X4 = new Vector2(600f, 337.5f);
    public static Vector2 SIZE_5X3 = new Vector2(672f, 378f);
    public static Vector2 SIZE_5X4 = new Vector2(600f, 337.5f);
    public static Vector2 SIZE_6X3 = new Vector2(560f, 315f);
    public static Vector2 SIZE_6X4 = new Vector2(560f, 315f);

    public static Vector2 ANCHORS_WHEN_NORMAL = new Vector2(0.5f, 0.5f);
    public static Vector2 ANCHORS_WHEN_BANNER = new Vector2(0.5f, 0f);

    public static Vector2 POSITION_BANNER_4x2 = new Vector2(0f, -202.5f);
    public static Vector2 POSITION_BANNER_4x3 = new Vector2(0f, -108f);
    public static Vector2 POSITION_BANNER_5x3 = new Vector2(0f, -108f);
    public static Vector2 POSITION_BANNER_6x4 = new Vector2(0f, -45f);
    public static Vector2 POSITION_BANNER_HORIZONTAL = new Vector2(0f, -82.19f);
    public static Vector2 POSITION_BANNER_PRIORITY = new Vector2(0f, -135f);

    public static Vector2 SIZE_BANNER_4x2 = new Vector2(0f, 405f);
    public static Vector2 SIZE_BANNER_4x3 = new Vector2(0f, 216f);
    public static Vector2 SIZE_BANNER_5x3 = new Vector2(0f, 216f);
    public static Vector2 SIZE_BANNER_6x4 = new Vector2(0f, 90f);
    public static Vector2 SIZE_BANNER_HORIZONTAL = new Vector2(0f, 164.38f);
    public static Vector2 SIZE_BANNER_PRIORITY = new Vector2(0f, 270f);
    //public static Vector2 SIZE_BANNER_VERTICAL = new Vector2(0f, 90f);

    public static Vector2 POSITION_MID = Vector2.zero;
    public static Vector2 POSITION_4X2 = new Vector2(0f, 472.5f);
    public static Vector2 POSITION_4X3_BANNER = new Vector2(0f, 567f);
    public static Vector2 POSITION_5X3 = new Vector2(0f, 567f);
    public static Vector2 POSITION_6x4 = new Vector2(0f, 630f);

    public const int FONT_4x = 96;
    public const int FONT_5x = 96;
    public const int FONT_6x = 60;
    public const int FONT_HORIZONTAL = 80;

    public const int NORMAL_PRIORITY = 0;
    public const int HIGH_PRIORITY = 1;
    public const int SUPER_PRIORITY = 2;

    #endregion
}
