using System.Collections.Generic;
using UnityEngine;

namespace AvatarOutfitPropEditor
{
    public class OutfitPropConfig : ScriptableObject
    {
        internal string avatarId;
        public int defaultClothIndex;
        public List<ClothInfo> clothList = new List<ClothInfo>();
        public List<OrnamentInfo> ornamentList = new List<OrnamentInfo>();
        public List<ExtraGroupInfo> extraGroupList = new List<ExtraGroupInfo>();

        // 旧版单列表扩展（仅用于读取迁移，保存时写入 extraGroupList）
        public int defaultExtraSetIndex;
        public List<ExtraSetInfo> extraSetList = new List<ExtraSetInfo>();

        [System.Serializable]
        public class SubToggleInfo
        {
            public string name;
            public string itemPath;
            public Texture2D menuImage;
            public bool defaultShow = true;
        }

        [System.Serializable]
        public class ClothInfo
        {
            public string name;
            public string type;
            public Texture2D menuImage;
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<string> showPaths = new List<string>();
            public List<string> hidePaths = new List<string>();
            public List<SubToggleInfo> subToggleList = new List<SubToggleInfo>();
        }

        [System.Serializable]
        public class ExtraGroupInfo
        {
            public string groupName;
            public int defaultSetIndex;
            public List<ExtraSetInfo> setList = new List<ExtraSetInfo>();
        }

        [System.Serializable]
        public class ExtraSetInfo
        {
            public string name;
            public Texture2D menuImage;
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<string> showPaths = new List<string>();
            public List<string> hidePaths = new List<string>();
            public List<SubToggleInfo> subToggleList = new List<SubToggleInfo>();
        }

        [System.Serializable]
        public class OrnamentInfo
        {
            public string name;
            public string type;
            public Texture2D menuImage;
            public bool isShow = true;
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<string> itemPaths = new List<string>();
            public List<string> hidePaths = new List<string>();
            public List<SubToggleInfo> subToggleList = new List<SubToggleInfo>();
        }
    }
}
