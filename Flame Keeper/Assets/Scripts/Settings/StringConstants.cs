using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// If you ever have to hard-code strings in any script, you can instead
/// define them here which should keep refactoring them less of a headache.
/// And hopefully this class doesn't get out of control lol.
/// </summary>
public class StringConstants
{
    public class SceneNames
    {
        public const string TutorialSceneName = "TutorialScene";
    }

    public class Input
    {
        public const string ActivateButton = "Controller_X";
        public const string DeactivateButton = "Controller_B";
        public const string JumpButton = "Controller_A";
        public const string HorizontalMovement = "Horizontal";
        public const string VerticalMovement = "Vertical";
    }

    public class Tags
    {
        public const string CrystalTag = "Crystal";
    }
}
