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
            
            _vm.StatusMessage.Value = "Ready.";
        }

        private void RefreshList()
        {
            _listView.itemsSource = _vm.Items.Items;
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
