#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;

namespace AvatarOutfitPropEditor
{
    internal static class OutfitPropEditorGui
    {
        public enum SectionKind
        {
            Window,
            ItemPanel,
            BasicInfo,
            ObjectElements,
            ExtraHide,
            SubMenu,
            SubMenuItem,
            Actions
        }

        private static GUIStyle[] _sectionStyles;
        private static GUIStyle _sectionTitleStyle;
        private static GUIStyle _dropZoneStyle;
        private static GUIStyle _authorCreditStyle;

        public static void EnsureStyles()
        {
            if (_sectionStyles != null)
                return;

            var pro = EditorGUIUtility.isProSkin;
            _sectionStyles = new[]
            {
                CreateBoxStyle(pro, new Color(0.17f, 0.17f, 0.17f), new Color(0.80f, 0.80f, 0.80f)),
                CreateBoxStyle(pro, new Color(0.21f, 0.21f, 0.21f), new Color(0.84f, 0.84f, 0.84f)),
                CreateBoxStyle(pro, new Color(0.20f, 0.23f, 0.27f), new Color(0.82f, 0.88f, 0.94f)),
                CreateBoxStyle(pro, new Color(0.27f, 0.23f, 0.20f), new Color(0.94f, 0.90f, 0.84f)),
                CreateBoxStyle(pro, new Color(0.24f, 0.21f, 0.27f), new Color(0.91f, 0.87f, 0.94f)),
                CreateBoxStyle(pro, new Color(0.19f, 0.19f, 0.21f), new Color(0.88f, 0.88f, 0.90f)),
                CreateBoxStyle(pro, new Color(0.18f, 0.24f, 0.20f), new Color(0.84f, 0.92f, 0.86f)),
                CreateBoxStyle(pro, new Color(0.16f, 0.16f, 0.16f), new Color(0.78f, 0.78f, 0.78f))
            };

            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                margin = new RectOffset(0, 0, 0, 4)
            };

            _dropZoneStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                padding = new RectOffset(8, 8, 10, 10)
            };
        }

        public static void BeginSection(SectionKind kind)
        {
            EnsureStyles();
            EditorGUILayout.BeginVertical(_sectionStyles[(int)kind]);
        }

        public static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }

        public static void SectionGap(float space = 12f)
        {
            GUILayout.Space(space);
        }

        public static void DrawSectionTitle(string title)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(title, _sectionTitleStyle);
        }

        public static void DrawDropZone(Rect rect, string text)
        {
            EnsureStyles();
            GUI.Box(rect, text, _dropZoneStyle);
        }

        public static bool DrawLanguageSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(OutfitPropEditorLoc.LanguageLabel, GUILayout.Width(40));
            var newIndex = GUILayout.Toolbar((int)OutfitPropEditorLoc.Current, OutfitPropEditorLoc.LanguageToolbarLabels);
            EditorGUILayout.EndHorizontal();
            if (newIndex == (int)OutfitPropEditorLoc.Current)
                return false;
            OutfitPropEditorLoc.Current = (OutfitPropEditorLanguage)newIndex;
            return true;
        }

        public static void DrawAuthorCredit()
        {
            EnsureStyles();
            if (_authorCreditStyle == null)
            {
                _authorCreditStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontSize = 10,
                    clipping = TextClipping.Clip,
                    padding = new RectOffset(0, 2, 2, 2)
                };
                var muted = EditorGUIUtility.isProSkin
                    ? new Color(0.52f, 0.52f, 0.52f)
                    : new Color(0.42f, 0.42f, 0.42f);
                _authorCreditStyle.normal.textColor = muted;
            }

            GUILayout.Label(OutfitPropEditorDefines.AuthorCredit, _authorCreditStyle);
        }

        private static GUIStyle CreateBoxStyle(bool proSkin, Color proColor, Color personalColor)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 0, 0)
            };
            style.normal.background = MakeTex(proSkin ? proColor : personalColor);
            return style;
        }

        private static Texture2D MakeTex(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
#endif
