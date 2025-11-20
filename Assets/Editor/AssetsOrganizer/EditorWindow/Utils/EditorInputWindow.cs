using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.AssetsOrganizer.EditorWindow.Utils
{
    public class EditorInputWindow : UnityEditor.EditorWindow
    {
        private Action<string> _onSubmit;

        public static void Show(string title, Action<string> onSubmit)
        {
            var window = CreateInstance<EditorInputWindow>();
            window.titleContent = new GUIContent(title);
            window._onSubmit = onSubmit;
            window.minSize = new Vector2(300, 120);
            window.ShowUtility();
        }

        public void CreateGUI()
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/View/Utils/EditorInputWindow.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Editor/AssetsOrganizer/View/Utils/EditorInputWindow.uss");

            VisualElement root = rootVisualElement;

            root.Add(uxml.CloneTree());
            root.styleSheets.Add(uss);
            
            var titleLabel = root.Q<Label>("titleLabel");
            var inputField = root.Q<TextField>("inputField");
            var cancelButton = root.Q<Button>("cancelButton");
            var okButton = root.Q<Button>("okButton");

            titleLabel.text = titleContent.text;
            
            cancelButton.clicked += Close;

            okButton.clicked += () =>
            {
                _onSubmit?.Invoke(inputField.value);
                Close();
            };
            
            inputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return)
                {
                    _onSubmit?.Invoke(inputField.value);
                    Close();
                }
            });
        }
    }
}