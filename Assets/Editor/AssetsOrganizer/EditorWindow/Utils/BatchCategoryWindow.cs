using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Editor.AssetsOrganizer.Model;

namespace Editor.AssetsOrganizer.EditorWindow.Utils
{
    public class BatchCategoryWindow : UnityEditor.EditorWindow
    {
        private Action<ItemCategory> _onSubmit;

        public static void Show(string title, Action<ItemCategory> onSubmit)
        {
            var window = CreateInstance<BatchCategoryWindow>();
            window.titleContent = new GUIContent(title);
            window.minSize = new Vector2(300, 140);
            window._onSubmit = onSubmit;
            window.ShowUtility();
        }

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/BatchCategoryWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Editor/AssetsOrganizer/View/BatchCategoryWindow.uss");

            var root = rootVisualElement;
            root.Add(uxml.CloneTree());
            root.styleSheets.Add(uss);

            // Query UI
            var titleLabel = root.Q<Label>("titleLabel");
            var categoryDropdown = root.Q<DropdownField>("categoryDropdown");
            var cancelButton = root.Q<Button>("cancelButton");
            var okButton = root.Q<Button>("okButton");

            titleLabel.text = titleContent.text;
            
            var names = Enum.GetNames(typeof(ItemCategory));
            categoryDropdown.choices = new List<string>(names);
            categoryDropdown.index = 0;

            cancelButton.clicked += Close;
            okButton.clicked += () =>
            {
                var selectedName = categoryDropdown.value;
                var category = (ItemCategory)Enum.Parse(typeof(ItemCategory), selectedName);
                _onSubmit?.Invoke(category);
                Close();
            };

            categoryDropdown.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    var selectedName = categoryDropdown.value;
                    var category = (ItemCategory)Enum.Parse(typeof(ItemCategory), selectedName);
                    _onSubmit?.Invoke(category);
                    Close();
                }
            });
        }
    }
}
