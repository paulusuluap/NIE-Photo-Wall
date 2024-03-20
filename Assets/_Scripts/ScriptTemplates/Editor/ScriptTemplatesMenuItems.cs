using UnityEditor;
using UnityEngine;

/// <summary>
/// Definitions for all the Code templates menu items.
/// </summary>
public class ScriptTemplatesMenuItems
{
    /// <summary>
    /// The root path for code templates menu items.
    /// </summary>
    private const string MENU_ITEM_PATH = "Assets/Create/";

    /// <summary>
    /// Menu items priority (so they will be grouped/shown next to existing scripting menu items).
    /// </summary>
    private const int MENU_ITEM_PRIORITY = 70;

    /// <summary>
    /// Creates a new C# class.
    /// </summary>
    [MenuItem(MENU_ITEM_PATH + "C# PW Animation", false, MENU_ITEM_PRIORITY)]
    private static void CreateClass()
    {
        ScriptTemplates.CreateFromTemplate(
            "NewAnimation.cs",
            @"Assets/_Scripts/ScriptTemplates/Editor/Templates/ClassTemplate.txt");
    }
}