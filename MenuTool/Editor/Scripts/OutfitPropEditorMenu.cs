#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AvatarOutfitPropEditor
{
    public static class OutfitPropEditorMenu
    {
        [MenuItem(OutfitPropEditorDefines.MenuRoot + "/打开制作器", false, 0)]
        public static void OpenEditor()
        {
#if VRC_SDK_VRCSDK3
            EditorWindow.GetWindow<OutfitPropEditorWindow>(false, OutfitPropEditorLoc.WindowTitle, true);
#else
            TipNeedSdk();
#endif
        }

        private static void TipNeedSdk()
        {
            if (EditorUtility.DisplayDialog(
                    OutfitPropEditorLoc.Notice,
                    OutfitPropEditorLoc.SdkRequired,
                    OutfitPropEditorLoc.Download,
                    OutfitPropEditorLoc.Cancel))
                Application.OpenURL("https://vrchat.com/home/download");
        }
    }
}
#endif
