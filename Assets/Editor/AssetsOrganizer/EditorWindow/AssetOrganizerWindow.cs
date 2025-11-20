using System;
using System.Collections.Generic;
using System.Linq;
using Editor.AssetsOrganizer.EditorWindow.Utils;
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
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/AssetOrganizerWindow.uxml");
            var root = uxml.CloneTree();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Editor/AssetsOrganizer/View/AssetOrganizerWindow.uss"));

            rootVisualElement.Add(root);

            _vm = new AssetOrganizerViewModel();

            _listView = root.Q<ListView>("itemList");
            _statusLabel = root.Q<Label>("lblStatus");
            _progressBar = root.Q<ProgressBar>("progressBar");
            _detailsContainer = root.Q<ScrollView>("detailsContainer");

            var searchField = root.Q<ToolbarSearchField>("searchField");
            var filterContainer = root.Q<VisualElement>("filterCategoryContainer");
            var emptyLabel = root.Q<Label>("emptyStateLabel");
            var loadingOverlay = root.Q<VisualElement>("loadingOverlay");

            var btnScan = root.Q<ToolbarButton>("btnScan");
            var btnCreate = root.Q<ToolbarButton>("btnCreate");
            var btnValidate = root.Q<ToolbarButton>("btnValidate");
            var btnBatchRename = root.Q<ToolbarButton>("btnBatchRename");
            var btnBatchCategory = root.Q<ToolbarButton>("btnBatchCategory");

            var rowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/ItemRow.uxml");

            // Filter dropdown
            List<string> options = new List<string>() { "All" };
            options.AddRange(Enum.GetNames(typeof(ItemCategory)));

            var popup = new PopupField<string>("Category", options, 0);
            filterContainer.Add(popup);

            popup.RegisterValueChangedCallback(evt =>
            {
                _vm.FilterCategory.Value = evt.newValue == "All"
                    ? null
                    : Enum.Parse<ItemCategory>(evt.newValue);

                RefreshList();
            });

            // Button callbacks
            btnScan.clicked += () =>
            {
                loadingOverlay.style.display = DisplayStyle.Flex;
                _vm.ScanAssetsAsync(this);
            };

            btnCreate.clicked += ShowCreateDialog;
            btnValidate.clicked += _vm.ValidateSelected;
            btnBatchRename.clicked += ShowBatchRenameDialog;
            btnBatchCategory.clicked += ShowBatchCategoryDialog;

            // Search
            searchField.RegisterValueChangedCallback(evt =>
            {
                _vm.SearchQuery.Value = evt.newValue;
                RefreshList();
            });

            // ListView Template
            _listView.fixedItemHeight = 48;
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
                        row.icon.style.backgroundImage = new StyleBackground(tex);
                }
                else
                {
                    row.icon.style.backgroundImage = null;
                }
                
                var badgeContainer = element.Q<VisualElement>("badgeContainer");
                badgeContainer.Clear();

                var results = GameItemValidator.Validate(item);

                foreach (var r in results)
                {
                    var badge = new VisualElement();
                    badge.AddToClassList("item-badge");

                    if (r.Severity == ValidationSeverity.Error)
                        badge.AddToClassList("item-badge-error");
                    else if (r.Severity == ValidationSeverity.Warning)
                        badge.AddToClassList("item-badge-warning");

                    badge.tooltip = r.Message;

                    badgeContainer.Add(badge);
                }
            };

            _listView.selectionChanged += OnListSelectionChanged;

            // Smooth progress bar
            float smoothProgress = 0f;
            root.schedule.Execute(() =>
            {
                smoothProgress = Mathf.Lerp(smoothProgress, _vm.Progress.Value, 0.1f);
                _progressBar.value = smoothProgress;

                if (smoothProgress >= 0.999f)
                    loadingOverlay.style.display = DisplayStyle.None;

            }).Every(16);

            _vm.Items.OnListChanged += RefreshList;
            _vm.StatusMessage.OnChanged += msg => _statusLabel.text = msg;

            _vm.Progress.Value = 0f;
            _vm.StatusMessage.Value = "Ready";
        }

        private void RefreshList()
        {
            var emptyLabel = rootVisualElement.Q<Label>("emptyStateLabel");

            var filtered = _vm.GetFilteredItems();
            
            if (filtered.Count == 0)
            {
                emptyLabel.style.display = DisplayStyle.Flex;
                _listView.style.display = DisplayStyle.None;
            }
            else
            {
                emptyLabel.style.display = DisplayStyle.None;
                _listView.style.display = DisplayStyle.Flex;
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
                var propCopy = iterator.Copy();

                PropertyField field = new PropertyField(propCopy);
                field.Bind(_selectedSO);
                
                field.RegisterValueChangeCallback(_ =>
                {
                    _selectedSO.ApplyModifiedProperties();
                    RefreshList();
                });
                
                field.RegisterValueChangeCallback(_ =>
                {
                    Undo.RecordObject(item, "Modify Game Item");
                    _selectedSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                    RefreshList();
                });

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
        
        private void ShowBatchRenameDialog()
        {
            int choice = EditorUtility.DisplayDialogComplex(
                "Batch Rename",
                "Choose rename mode:",
                "Add Prefix",
                "Add Suffix",
                "Cancel"
            );

            if (choice == 2) return; // Cancel

            bool isPrefix = choice == 0;

            EditorInputWindow.Show(
                isPrefix ? "Enter Prefix" : "Enter Suffix",
                (text) =>
                {
                    if (!string.IsNullOrEmpty(text))
                        ApplyBatchRename(isPrefix, text);
                });
        }

        
        private void ShowBatchCategoryDialog()
        {
            BatchCategoryWindow.Show(
                "Select Category",
                ApplyBatchCategory
            );
        }
        
        private void ApplyBatchRename(bool isPrefix, string text)
        {
            var items = _vm.GetFilteredItems();

            foreach (var item in items)
            {
                string newName = isPrefix
                    ? text + item.DisplayName
                    : item.DisplayName + text;

                Undo.RecordObject(item, "Batch Rename GameItems");

                item.DisplayName = newName;
                
                string assetPath = AssetDatabase.GetAssetPath(item);
                AssetDatabase.RenameAsset(assetPath, newName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshList();
        }
        
        private void ApplyBatchCategory(ItemCategory category)
        {
            var items = _vm.GetFilteredItems();

            foreach (var item in items)
            {
                Undo.RecordObject(item, "Batch Category Apply");
                item.Category = category;

                EditorUtility.SetDirty(item);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshList();
        }

    }
}
