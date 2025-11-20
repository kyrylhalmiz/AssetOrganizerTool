using System;
using System.Collections;
using System.Collections.Generic;
using Editor.AssetsOrganizer.Model;
using UnityEditor;
using UnityEngine;

namespace Editor.AssetsOrganizer.Services
{
    public class AssetScanner
    {
        public List<GameItemConfig> FindAllItems(out float progress)
        {
            var guids = AssetDatabase.FindAssets("t:GameItemConfig");
            var results = new List<GameItemConfig>();

            float count = guids.Length;
            progress = 0f;

            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var item = AssetDatabase.LoadAssetAtPath<GameItemConfig>(path);
                if (item != null)
                    results.Add(item);

                progress = (i + 1) / count;
            }

            return results;
        }
        
        public IEnumerator ScanAsync(Action<float> onProgress, Action<List<GameItemConfig>> onComplete)
        {
            var guids = AssetDatabase.FindAssets("t:GameItemConfig");
            var results = new List<GameItemConfig>();

            int count = guids.Length;
            for (int i = 0; i < count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var item = AssetDatabase.LoadAssetAtPath<GameItemConfig>(path);
                if (item != null)
                    results.Add(item);

                onProgress?.Invoke((i + 1f) / count);
                
                if (i % 20 == 0)
                    yield return null;
            }

            onComplete?.Invoke(results);
        }

        public GameItemConfig CreateItem(string folder, string name)
        {
            var asset = ScriptableObject.CreateInstance<GameItemConfig>();
            var path = $"{folder}/{name}.asset";

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
        }
    }
}