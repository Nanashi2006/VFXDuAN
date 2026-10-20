#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VFXDuAN.DungeonTank.Editor
{
    public static class DungeonTankPackageValidator
    {
        private static readonly string[] RequiredFolders =
        {
            "Assets/StylizedHandPaintedDungeon(Free)",
            "Assets/Crusader tank",
            "Assets/DungeonCharacters",
            "Assets/Particlecollection_Free samples"
        };

        [MenuItem("Tools/VFXDuAN/Validate 4 Imported Packages")]
        public static void Validate()
        {
            Debug.Log("========== VFXDuAN PACKAGE VALIDATION ==========");
            int ok = 0;

            foreach (string folder in RequiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Debug.LogError("[MISSING] " + folder);
                    continue;
                }

                string[] objects = AssetDatabase.FindAssets("t:GameObject", new[] { folder });
                string[] materials = AssetDatabase.FindAssets("t:Material", new[] { folder });
                string[] textures = AssetDatabase.FindAssets("t:Texture", new[] { folder });

                Debug.Log($"[OK] {folder} | GameObjects: {objects.Length} | Materials: {materials.Length} | Textures: {textures.Length}");
                PrintSampleAssets(folder, objects, 5);
                ok++;
            }

            if (ok == RequiredFolders.Length)
                Debug.Log("[VFXDuAN] ALL 4 PACKAGES FOUND. You can now run Tools > VFXDuAN > Build Dungeon Tank Showcase.");
            else
                Debug.LogWarning($"[VFXDuAN] Found {ok}/{RequiredFolders.Length} required package folders. Missing folders will use safe fallbacks.");

            Debug.Log("===============================================");
        }

        private static void PrintSampleAssets(string folder, string[] guids, int max)
        {
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("  No GameObject assets discovered inside " + folder);
                return;
            }

            List<string> names = new List<string>();
            int count = Mathf.Min(max, guids.Length);
            for (int i = 0; i < count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                names.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }

            Debug.Log("  Samples: " + string.Join(", ", names));
        }
    }
}
#endif
