using Editor.AssetsOrganizer.EditorWindow;
using Editor.AssetsOrganizer.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.AssetsOrganizer.Inspectors
{
    [CustomEditor(typeof(GameItemConfig))]
    public class GameItemConfigInspector : UnityEditor.Editor
    {
        private void RebuildBadges(GameItemConfig item, VisualElement badgeContainer)
        {
            badgeContainer.Clear();

            var results = GameItemValidator.Validate(item);

            foreach (var r in results)
            {
                var badge = new Label(r.Severity == ValidationSeverity.Error ? "ERR" : "WARN");
                badge.AddToClassList("gi-badge");

                if (r.Severity == ValidationSeverity.Error)
                    badge.AddToClassList("gi-badge-error");
                else
                    badge.AddToClassList("gi-badge-warning");

                badge.tooltip = r.Message;
                badgeContainer.Add(badge);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Editor/AssetsOrganizer/Inspectors/GameItemConfigInspector.uxml");

            var root = uxml.Instantiate();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Editor/AssetsOrganizer/Inspectors/GameItemConfigInspector.uss"));

            GameItemConfig item = (GameItemConfig)target;
            SerializedObject so = serializedObject;

            var header = root.Q<Label>("headerLabel");
            var badgeContainer = root.Q<VisualElement>("badgeContainer");
            var propertiesContainer = root.Q<VisualElement>("propertiesContainer");
            var previewImg = root.Q<Image>("previewImage");
            var openBtn = root.Q<Button>("openInOrganizerBtn");

            header.text = item.DisplayName;
            
            RebuildBadges(item, badgeContainer);
            
            var iterator = so.GetIterator();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                var propCopy = iterator.Copy();
                var field = new PropertyField(propCopy);

                field.Bind(so);

                field.RegisterValueChangeCallback(_ =>
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(item);
                    
                    RebuildBadges(item, badgeContainer);
                });

                propertiesContainer.Add(field);
            }
            
            if (item.Icon != null)
            {
                previewImg.image = AssetPreview.GetAssetPreview(item.Icon);
            }
            else
            {
                previewImg.style.display = DisplayStyle.None;
            }

            openBtn.clicked += () =>
            {
                var wnd = UnityEditor.EditorWindow.GetWindow<AssetOrganizerWindow>();
                wnd.Focus();
            };

            return root;
        }
    }
}
