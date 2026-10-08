#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VFXDuAN.BlackHole.Editor
{
    public static class CreateBlackHoleMenu
    {
        [MenuItem("GameObject/VFX/VFXDuAN Black Hole", false, 10)]
        private static void CreateBlackHole(MenuCommand menuCommand)
        {
            GameObject root = new GameObject("VFX_BlackHole");
            GameObjectUtility.SetParentAndAlign(root, menuCommand.context as GameObject);

            BlackHoleEffect effect = root.AddComponent<BlackHoleEffect>();
            effect.Build();

            Undo.RegisterCreatedObjectUndo(root, "Create VFX Black Hole");
            Selection.activeGameObject = root;
        }
    }
}
#endif
