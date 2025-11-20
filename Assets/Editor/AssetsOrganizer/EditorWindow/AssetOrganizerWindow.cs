using System;
using System.Collections.Generic;
using System.Linq;
using Editor.AssetsOrganizer.Model;
using Editor.AssetsOrganizer.ViewModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.AssetsOrganizer.EditorWindow
{
    
    public class AssetOrganizerWindow : UnityEditor.EditorWindow
    {
        private AssetOrganizerViewModel _vm;

        private ListView _listView;
        private Label _statusLabel;
        private ProgressBar _progressBar;
        private ScrollView _detailsContainer;

        private SerializedObject _selectedSO;
        
        private class RowRefs
        {
            public VisualElement icon;
            public Label title;
            public Label subtitle;
        }

        [MenuItem("Tools/Asset Organizer Editor Tool")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<AssetOrganizerWindow>();
            wnd.titleContent = new GUIContent("Asset Organizer");
        }

        private void CreateGUI()
        {
            // Load UXML/USS
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/AssetOrganizerWindow.uxml");
            var root = uxml.CloneTree();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Editor/AssetsOrganizer/View/AssetOrganizerWindow.uss"));

            rootVisualElement.Add(root);

            // ViewModel
            _vm = new AssetOrganizerViewModel();

            // Query UI Elements 
            _listView = root.Q<ListView>("itemList");
            _statusLabel = root.Q<Label>("lblStatus");
            _progressBar = root.Q<ProgressBar>("progressBar");
            _detailsContainer = root.Q<ScrollView>("detailsContainer");
            var searchField = root.Q<ToolbarSearchField>("searchField");
            
            var filterContainer = root.Q<VisualElement>("filterCategoryContainer");
            
            List<string> options = new List<string> { "All" };

            options.AddRange(System.Enum.GetNames(typeof(ItemCategory)));

            var popup = new PopupField<string>("Category", options, 0)
            {
                name = "filterCategoryPopup"
            };

            filterContainer.Add(popup);
            
            var rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/ItemRow.uxml");

            var btnScan = root.Q<ToolbarButton>("btnScan");
            var btnCreate = root.Q<ToolbarButton>("btnCreate");
            var btnValidate = root.Q<ToolbarButton>("btnValidate");

            // Connect commands 
            btnScan.clicked += _vm.ScanAssets;
            btnCreate.clicked += ShowCreateDialog;
            btnValidate.clicked += _vm.ValidateSelected;

            //  Bind ListView 
            _vm.Items.OnListChanged += RefreshList;
            _listView.selectionChanged += OnListSelectionChanged;

            // Bind status text 
            _vm.StatusMessage.OnChanged += s => _statusLabel.text = s;

            // Bind progress 
            _vm.Progress.OnChanged += p => _progressBar.value = p;
            
            searchField.RegisterValueChangedCallback(evt =>
            {
                _vm.SearchQuery.Value = evt.newValue;
                RefreshList();
            });
            
            popup.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "All")
                    _vm.FilterCategory.Value = null;
                else
                    _vm.FilterCategory.Value = Enum.Parse<ItemCategory>(evt.newValue);

                RefreshList();
            });
            
            _listView.makeItem = () =>
            {
                var ve = rowTemplate.CloneTree();
                ve.userData = new RowRefs
                {
                    icon = ve.Q<VisualElement>("icon"),
                    title = ve.Q<Label>("title"),
                    subtitle = ve.Q<Label>("subtitle")
                };
                return ve;
            };

            _listView.bindItem = (element, index) =>
            {
                var row = (RowRefs)element.userData;
                var item = (GameItemConfig)_listView.itemsSource[index];
                
                row.title.text = item.DisplayName;
                
                row.subtitle.text = $"{item.Category} • {item.Price}";
                
                if (item.Icon != null)
                {
                    var tex = AssetPreview.GetAssetPreview(item.Icon);
                    if (tex != null)
                    {
                        row.icon.style.backgroundImage = new StyleBackground(tex);
                    }
                }
                else
                {
                    row.icon.style.backgroundImage = null;
                }
            };
            
            _listView.selectionChanged += OnListSelectionChanged;
            
            _listView.fixedItemHeight = 36;
            
            _vm.StatusMessage.Value = "Ready.";
        }

        private void RefreshList()
        {
            var filtered = _vm.GetFilteredItems();
            
            if (_vm.SelectedItem.Value != null && !filtered.Contains(_vm.SelectedItem.Value))
            {
                _listView.SetSelectionWithoutNotify(new int[]{}); // Clears selection
                _vm.Select(null);
                _detailsContainer.Clear();
            }

            _listView.itemsSource = filtered;
            _listView.Rebuild();
        }

        private void OnListSelectionChanged(System.Collections.Generic.IEnumerable<object> selected)
        {
            GameItemConfig item = selected?.FirstOrDefault() as GameItemConfig;

            _vm.Select(item);

            DisplaySelectedItem(item);
        }

        private void DisplaySelectedItem(GameItemConfig item)
        {
            _detailsContainer.Clear();

            if (item == null)
            {
                _statusLabel.text = "No item selected.";
                return;
            }

            _selectedSO = new SerializedObject(item);

            // Bind auto inspector fields
            var iterator = _selectedSO.GetIterator();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                PropertyField field = new PropertyField(iterator.Copy());
                field.Bind(_selectedSO);
                _detailsContainer.Add(field);
            }
        }

        private void ShowCreateDialog()
        {
            string folder = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (string.IsNullOrEmpty(folder))
                return;

            folder = folder.Replace(Application.dataPath, "Assets");

            string itemName = EditorUtility.SaveFilePanel("Item Name", folder, "NewItem", "asset");
            if (string.IsNullOrEmpty(itemName))
                return;

            itemName = System.IO.Path.GetFileNameWithoutExtension(itemName);

            _vm.CreateNewItem(folder, itemName);
        }
    }
}
