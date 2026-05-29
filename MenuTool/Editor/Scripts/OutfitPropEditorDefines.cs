namespace AvatarOutfitPropEditor
{
    public static class OutfitPropEditorDefines
    {
        public const string MenuRoot = "菜单一键制作工具";
        public const string WindowTitle = "菜单一键制作工具";
        public const string WindowSubtitle = "一键配置模型功能菜单";
        public const string AuthorCredit = "by：ずっと子都";

        public const string MenuClothRoot = "服装";
        public const string MenuPropRoot = "道具";
        public const string MenuExtraRoot = "扩展";
        public const string FxClothLayerName = "服装切换";
        public const string FxExtraLayerLegacyName = "扩展切换";

        public const string ParamClothInt = "OutfitProp_Cloth";
        public const string ParamExtraGroupIntPrefix = "OutfitProp_ExtraG_";
        public const string ParamOrnPrefix = "OutfitProp_Orn_";
        public const string ParamClothSubPrefix = "OutfitProp_ClothSub_";
        public const string ParamOrnSubPrefix = "OutfitProp_OrnSub_";
        public const string ParamExtraSubPrefix = "OutfitProp_ExtraSub_";

        public const string DataRoot = "Assets/OutfitPropData";
        public const string ConfigFileName = "OutfitPropConfig.asset";
        public const string DefaultExpressionParametersTemplate =
            "Assets/com.zuto.outfit-prop/Editor/Resources/DefaultExpressionParameters.asset";

        public static string ParamOrn(int index) => ParamOrnPrefix + index;
        public static string ParamClothSub(int clothIndex, int subIndex) => ParamClothSubPrefix + clothIndex + "_" + subIndex;
        public static string ParamOrnSub(int ornamentIndex, int subIndex) => ParamOrnSubPrefix + ornamentIndex + "_" + subIndex;
        public static string ParamExtraGroupInt(int groupIndex) => ParamExtraGroupIntPrefix + groupIndex;
        public static string ParamExtraSub(int groupIndex, int setIndex, int subIndex) =>
            ParamExtraSubPrefix + groupIndex + "_" + setIndex + "_" + subIndex;

        public static string FxExtraGroupLayerName(int groupIndex, string groupName)
        {
            if (!string.IsNullOrWhiteSpace(groupName))
                return "扩展_" + groupName.Trim();
            return "扩展切换_" + (groupIndex + 1);
        }

        public static bool IsManagedParameter(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;
            return name.StartsWith("OutfitProp_") || name.StartsWith("Wardrobe");
        }

        public static bool IsLegacyFxLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
                return false;
            return layerName.StartsWith("Wardrobe") || layerName == "换装" || layerName == "WardrobeCloth"
                || layerName == FxExtraLayerLegacyName || layerName.StartsWith("扩展_") || layerName.StartsWith("扩展切换_");
        }

        public static bool IsClothMenuControlName(string name) => name == MenuClothRoot || name == "换装";
        public static bool IsPropMenuControlName(string name) => name == MenuPropRoot || name == "配饰";
        public static bool IsExtraMenuControlName(string name) => name == MenuExtraRoot;
    }
}
