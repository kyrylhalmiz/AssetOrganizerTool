using System.Collections.Generic;
using System.Linq;
using Editor.AssetsOrganizer.Model;
using Editor.AssetsOrganizer.Services;
using Editor.AssetsOrganizer.ViewModel.Observables;
using UnityEditor;

namespace Editor.AssetsOrganizer.ViewModel
{
    public class AssetOrganizerViewModel
    {
        public ObservableList<GameItemConfig> Items { get; } = new();
        public Observable<GameItemConfig> SelectedItem { get; } = new(null);
        public Observable<string> StatusMessage { get; } = new("");
        public Observable<float> Progress { get; } = new(0f);
        public Observable<string> SearchQuery { get; } = new("");
        public Observable<ItemCategory?> FilterCategory { get; } = new(null);


        private readonly AssetScanner _scanner;

        public AssetOrganizerViewModel()
        {
            _scanner = new AssetScanner();
        }
        
        public void ScanAssets()
        {
            StatusMessage.Value = "Scanning assets...";
            
            var results = _scanner.FindAllItems(out float progress);
            Progress.Value = progress;

            Items.Set(results);
            StatusMessage.Value = $"Found {results.Count} items.";
        }

        public void CreateNewItem(string folderPath, string name)
        {
            var item = _scanner.CreateItem(folderPath, name);
            if (item != null)
            {
                Items.Add(item);
                StatusMessage.Value = $"Created item '{name}'.";
            }
        }

        public void Select(GameItemConfig item)
        {
            SelectedItem.Value = item;
        }

        public void ValidateSelected()
        {
            if (SelectedItem.Value == null)
            {
                StatusMessage.Value = "No item selected.";
                return;
            }

            var selected = SelectedItem.Value;
            
            var assetPath = AssetDatabase.GetAssetPath(selected);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (fileName != selected.DisplayName)
            {
                StatusMessage.Value = "Filename does not match DisplayName.";
            }
            else
            {
                StatusMessage.Value = "Item is valid.";
            }
        }
        
        public List<GameItemConfig> GetFilteredItems()
        {
            IEnumerable<GameItemConfig> list = Items.Items;

            if (!string.IsNullOrEmpty(SearchQuery.Value))
            {
                string q = SearchQuery.Value.ToLower();
                list = list.Where(i =>
                    i.DisplayName?.ToLower().Contains(q) == true ||
                    i.Description?.ToLower().Contains(q) == true);
            }

            if (FilterCategory.Value.HasValue)
            {
                var cat = FilterCategory.Value.Value;
                list = list.Where(i => i.Category == cat);
            }

            return list.ToList();
        }

    }
}
