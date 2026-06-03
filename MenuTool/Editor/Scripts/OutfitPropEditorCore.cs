#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace AvatarOutfitPropEditor
{
    public class OutfitPropEditorCore : EditorWindow
    {
        internal const int maxClothNum = 255;
        internal static string FxClothLayerName => OutfitPropEditorDefines.FxClothLayerName;

        [System.Serializable]
        public class ObjInfo
        {
            public string name = "";
            public string type = "";
            public Texture2D image;
            public AnimBool animBool = new AnimBool { speed = 3.0f };
        }

        [System.Serializable]
        public class SubToggleObjInfo
        {
            public string name = "子开关";
            public GameObject item;
            public Texture2D image;
            public bool defaultShow = true;
        }

        [System.Serializable]
        public class ClothObjInfo : ObjInfo
        {
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<GameObject> showObjectList;
            public List<GameObject> hideObjectList;
            public List<SubToggleObjInfo> subToggleList;

            public ClothObjInfo(string _name = "衣服")
            {
                name = _name;
                showObjectList = new List<GameObject>();
                hideObjectList = new List<GameObject>();
                subToggleList = new List<SubToggleObjInfo>();
            }
        }

        [System.Serializable]
        public class OrnamentObjInfo : ObjInfo
        {
            public bool isShow = true;
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<GameObject> objectList;
            public List<GameObject> hideObjectList;
            public List<SubToggleObjInfo> subToggleList;

            public OrnamentObjInfo(string _name = "配饰")
            {
                name = _name;
                objectList = new List<GameObject>();
                hideObjectList = new List<GameObject>();
                subToggleList = new List<SubToggleObjInfo>();
            }
        }

        [System.Serializable]
        public class ExtraSetObjInfo : ObjInfo
        {
            public bool enableSubMenu;
            public bool enableExtraHide;
            public List<GameObject> showObjectList;
            public List<GameObject> hideObjectList;
            public List<SubToggleObjInfo> subToggleList;

            public ExtraSetObjInfo(string _name = "方案1")
            {
                name = _name;
                showObjectList = new List<GameObject>();
                hideObjectList = new List<GameObject>();
                subToggleList = new List<SubToggleObjInfo>();
            }
        }

        [System.Serializable]
        public class ExtraGroupObjInfo : ObjInfo
        {
            public int defaultSetIndex = -1;
            public List<ExtraSetObjInfo> setList = new List<ExtraSetObjInfo>();

            public ExtraGroupObjInfo(string _name = "分组1")
            {
                name = _name;
                setList = new List<ExtraSetObjInfo>();
            }
        }

        internal static int CountExtraSets(List<ExtraGroupObjInfo> extraGroupList)
        {
            var count = 0;
            foreach (var group in extraGroupList)
                count += group.setList.Count;
            return count;
        }

        internal static OutfitPropConfig CreateParameter(GameObject avatar)
        {
            if (avatar == null)
                return null;
            var parameter = CreateInstance<OutfitPropConfig>();
            var avatarId = OutfitPropEditorUtils.GetOrCreateAvatarId(avatar);
            parameter.avatarId = avatarId;
            parameter.clothList.Add(new OutfitPropConfig.ClothInfo { name = "衣服1" });
            var dir = OutfitPropEditorUtils.GetAvatarDataDir(avatarId);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(parameter, OutfitPropEditorUtils.GetConfigAssetPath(avatarId));
            return parameter;
        }

        internal static OutfitPropConfig GetParameter(string avatarId)
        {
            if (avatarId == null)
                return null;
            var path = OutfitPropEditorUtils.GetConfigAssetPath(avatarId);
            if (File.Exists(path))
                return AssetDatabase.LoadAssetAtPath(path, typeof(OutfitPropConfig)) as OutfitPropConfig;
            return null;
        }

        internal static Dictionary<string, List<GameObject>> GetClothParameter(List<ClothObjInfo> clothInfoList, int index)
        {
            if (index < 0 || index >= clothInfoList.Count)
                return null;
            var showList = new List<GameObject>();
            var hideList = new List<GameObject>();

            for (var a = 0; a < clothInfoList.Count; a++)
            {
                if (a == index)
                    continue;
                var info = clothInfoList[a];
                hideList = OutfitPropEditorUtils.LinkGameObjectList(hideList, info.showObjectList);
                if (info.enableExtraHide)
                    showList = OutfitPropEditorUtils.LinkGameObjectList(showList, info.hideObjectList);
            }

            var current = clothInfoList[index];
            showList = OutfitPropEditorUtils.LinkGameObjectList(showList, current.showObjectList);
            if (current.enableExtraHide)
                hideList = OutfitPropEditorUtils.LinkGameObjectList(hideList, current.hideObjectList);

            foreach (var obj in current.showObjectList)
                hideList.Remove(obj);
            if (current.enableExtraHide)
            {
                foreach (var obj in current.hideObjectList)
                    showList.Remove(obj);
            }

            return new Dictionary<string, List<GameObject>>
            {
                { "show", showList },
                { "hide", hideList }
            };
        }

        internal static Dictionary<string, List<string>> GetClothPathParameter(GameObject avatar, List<ClothObjInfo> clothInfoList, int index)
        {
            var map = GetClothParameter(clothInfoList, index);
            if (map == null || avatar == null)
                return null;

            var avatarPath = OutfitPropEditorUtils.GetHierarchyPath(avatar.transform) + "/";
            var showPathList = new List<string>();
            var hidePathList = new List<string>();

            foreach (var obj in map["show"])
                showPathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));
            foreach (var obj in map["hide"])
                hidePathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));

            return new Dictionary<string, List<string>>
            {
                { "show", showPathList },
                { "hide", hidePathList }
            };
        }

        internal static void PrviewCloth(List<ClothObjInfo> clothInfoList, int index)
        {
            if (index < 0 || index >= clothInfoList.Count)
                return;
            var map = GetClothParameter(clothInfoList, index);
            foreach (var item in map["hide"])
                if (item != null)
                    item.SetActive(false);
            foreach (var item in map["show"])
                if (item != null)
                    item.SetActive(true);
        }

        internal static Dictionary<string, List<GameObject>> GetExtraSetParameter(List<ExtraSetObjInfo> extraSetList, int index)
        {
            if (index < 0 || index >= extraSetList.Count)
                return null;
            var showList = new List<GameObject>();
            var hideList = new List<GameObject>();

            for (var a = 0; a < extraSetList.Count; a++)
            {
                if (a == index)
                    continue;
                var info = extraSetList[a];
                hideList = OutfitPropEditorUtils.LinkGameObjectList(hideList, info.showObjectList);
                if (info.enableExtraHide)
                    showList = OutfitPropEditorUtils.LinkGameObjectList(showList, info.hideObjectList);
            }

            var current = extraSetList[index];
            showList = OutfitPropEditorUtils.LinkGameObjectList(showList, current.showObjectList);
            if (current.enableExtraHide)
                hideList = OutfitPropEditorUtils.LinkGameObjectList(hideList, current.hideObjectList);

            foreach (var obj in current.showObjectList)
                hideList.Remove(obj);
            if (current.enableExtraHide)
            {
                foreach (var obj in current.hideObjectList)
                    showList.Remove(obj);
            }

            return new Dictionary<string, List<GameObject>>
            {
                { "show", showList },
                { "hide", hideList }
            };
        }

        internal static Dictionary<string, List<string>> GetExtraSetPathParameter(GameObject avatar, List<ExtraSetObjInfo> extraSetList, int index)
        {
            var map = GetExtraSetParameter(extraSetList, index);
            if (map == null || avatar == null)
                return null;

            var showPathList = new List<string>();
            var hidePathList = new List<string>();

            foreach (var obj in map["show"])
                showPathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));
            foreach (var obj in map["hide"])
                hidePathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));

            return new Dictionary<string, List<string>>
            {
                { "show", showPathList },
                { "hide", hidePathList }
            };
        }

        internal static void PrviewExtraSet(List<ExtraSetObjInfo> extraSetList, int index)
        {
            if (index < 0 || index >= extraSetList.Count)
                return;
            var map = GetExtraSetParameter(extraSetList, index);
            foreach (var item in map["hide"])
                if (item != null)
                    item.SetActive(false);
            foreach (var item in map["show"])
                if (item != null)
                    item.SetActive(true);
        }

        internal static AnimationClip GetExtraSetAnimClip(
            ExtraGroupObjInfo group,
            int groupIndex,
            List<ExtraSetObjInfo> extraSetList,
            GameObject avatar,
            int index)
        {
            var map = GetExtraSetPathParameter(avatar, extraSetList, index);
            var clipName = ResolveFxLayerName(
                group.name + "_" + extraSetList[index].name,
                "未命名_" + (groupIndex + 1) + "_" + (index + 1));
            var clip = BuildActiveClip(clipName, map["show"], true);
            foreach (var path in map["hide"])
            {
                var frame = new Keyframe { time = 0, value = 0 };
                var curve = new AnimationCurve { keys = new[] { frame } };
                var bind = new EditorCurveBinding
                {
                    path = path,
                    propertyName = "m_IsActive",
                    type = typeof(GameObject)
                };
                AnimationUtility.SetEditorCurve(clip, bind, curve);
            }
            return clip;
        }

        private static AnimationClip BuildActiveClip(string name, IEnumerable<string> paths, bool active)
        {
            var clip = new AnimationClip { name = name };
            foreach (var path in paths)
            {
                var frame = new Keyframe { time = 0, value = active ? 1 : 0 };
                var curve = new AnimationCurve { keys = new[] { frame } };
                var bind = new EditorCurveBinding
                {
                    path = path,
                    propertyName = "m_IsActive",
                    type = typeof(GameObject)
                };
                AnimationUtility.SetEditorCurve(clip, bind, curve);
            }
            return clip;
        }

        internal static AnimationClip GetClothAnimClip(List<ClothObjInfo> clothInfoList, GameObject avatar, int index)
        {
            var map = GetClothPathParameter(avatar, clothInfoList, index);
            var clipName = ResolveFxLayerName(clothInfoList[index].name, "未命名衣服_" + (index + 1));
            var clip = BuildActiveClip(clipName, map["show"], true);
            foreach (var path in map["hide"])
            {
                var frame = new Keyframe { time = 0, value = 0 };
                var curve = new AnimationCurve { keys = new[] { frame } };
                var bind = new EditorCurveBinding
                {
                    path = path,
                    propertyName = "m_IsActive",
                    type = typeof(GameObject)
                };
                AnimationUtility.SetEditorCurve(clip, bind, curve);
            }
            return clip;
        }

        internal static AnimationClip[] GetOrnamentAnimClip(List<OrnamentObjInfo> ornamentObjInfos, GameObject avatar, int index)
        {
            var info = ornamentObjInfos[index];
            var showPathList = new List<string>();
            foreach (var obj in info.objectList)
                showPathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));

            var hidePathList = new List<string>();
            if (info.enableExtraHide)
            {
                foreach (var obj in info.hideObjectList)
                    hidePathList.Add(OutfitPropEditorUtils.GetHierarchyPath(obj.transform, avatar.transform));
            }

            var layerName = ResolveFxLayerName(info.name, "未命名配饰_" + (index + 1));
            var hideClip = BuildActiveClip("隐藏_" + layerName, showPathList, false);
            foreach (var path in hidePathList)
            {
                var frame = new Keyframe { time = 0, value = 1 };
                var curve = new AnimationCurve { keys = new[] { frame } };
                var bind = new EditorCurveBinding
                {
                    path = path,
                    propertyName = "m_IsActive",
                    type = typeof(GameObject)
                };
                AnimationUtility.SetEditorCurve(hideClip, bind, curve);
            }

            var showClip = BuildActiveClip("显示_" + layerName, showPathList, true);
            foreach (var path in hidePathList)
            {
                var frame = new Keyframe { time = 0, value = 0 };
                var curve = new AnimationCurve { keys = new[] { frame } };
                var bind = new EditorCurveBinding
                {
                    path = path,
                    propertyName = "m_IsActive",
                    type = typeof(GameObject)
                };
                AnimationUtility.SetEditorCurve(showClip, bind, curve);
            }

            return new[] { hideClip, showClip };
        }

        internal static AnimationClip[] GetSubToggleAnimClip(GameObject avatar, GameObject target, string layerName)
        {
            var path = OutfitPropEditorUtils.GetHierarchyPath(target.transform, avatar.transform);
            var resolvedName = ResolveFxLayerName(layerName, "未命名子开关");
            return new[]
            {
                BuildActiveClip("隐藏_" + resolvedName, new[] { path }, false),
                BuildActiveClip("显示_" + resolvedName, new[] { path }, true)
            };
        }

        internal static string ResolveFxLayerName(string name, string fallback)
        {
            return string.IsNullOrWhiteSpace(name) ? fallback : name.Trim();
        }

        internal static string SanitizeAnimAssetFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "未命名";
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                    chars[i] = '_';
            }
            var result = new string(chars);
            return string.IsNullOrWhiteSpace(result) ? "未命名" : result;
        }

        internal static List<string> BuildPlannedFxAnimClipNames(
            List<ClothObjInfo> clothList,
            List<OrnamentObjInfo> ornamentList,
            List<ExtraGroupObjInfo> extraGroupList)
        {
            var names = new List<string>();
            for (var clothIndex = 0; clothIndex < clothList.Count; clothIndex++)
                names.Add(ResolveFxLayerName(clothList[clothIndex].name, "未命名衣服_" + (clothIndex + 1)));

            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    var set = group.setList[setIndex];
                    names.Add(ResolveFxLayerName(
                        group.name + "_" + set.name,
                        "未命名_" + (groupIndex + 1) + "_" + (setIndex + 1)));
                }
            }

            for (var ornamentIndex = 0; ornamentIndex < ornamentList.Count; ornamentIndex++)
            {
                var layerName = ResolveFxLayerName(ornamentList[ornamentIndex].name, "未命名配饰_" + (ornamentIndex + 1));
                names.Add("隐藏_" + layerName);
                names.Add("显示_" + layerName);
            }

            for (var clothIndex = 0; clothIndex < clothList.Count; clothIndex++)
            {
                var info = clothList[clothIndex];
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var layerName = ResolveFxLayerName(sub.item.name, "子开关_衣服_" + (clothIndex + 1) + "_" + (subIndex + 1));
                    names.Add("隐藏_" + layerName);
                    names.Add("显示_" + layerName);
                }
            }

            for (var ornamentIndex = 0; ornamentIndex < ornamentList.Count; ornamentIndex++)
            {
                var info = ornamentList[ornamentIndex];
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var layerName = ResolveFxLayerName(sub.item.name, "子开关_配饰_" + (ornamentIndex + 1) + "_" + (subIndex + 1));
                    names.Add("隐藏_" + layerName);
                    names.Add("显示_" + layerName);
                }
            }

            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    var info = group.setList[setIndex];
                    if (!info.enableSubMenu)
                        continue;
                    for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                    {
                        var sub = info.subToggleList[subIndex];
                        if (sub.item == null)
                            continue;
                        var layerName = ResolveFxLayerName(
                            sub.item.name,
                            "子开关_扩展_" + (groupIndex + 1) + "_" + (setIndex + 1) + "_" + (subIndex + 1));
                        names.Add("隐藏_" + layerName);
                        names.Add("显示_" + layerName);
                    }
                }
            }
            return names;
        }

        internal static bool ValidateFxAnimClipNames(List<string> plannedClipNames, out string errorMessage)
        {
            errorMessage = null;
            var duplicateClipNames = plannedClipNames
                .GroupBy(n => n)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateClipNames.Count > 0)
            {
                errorMessage = OutfitPropEditorLoc.DuplicateClipNamesError + string.Join("\n", duplicateClipNames);
                return false;
            }

            var fileNames = plannedClipNames.Select(SanitizeAnimAssetFileName).ToList();
            var duplicateFileNames = fileNames
                .GroupBy(n => n)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateFileNames.Count > 0)
            {
                errorMessage = OutfitPropEditorLoc.DuplicateFileNamesError + string.Join("\n", duplicateFileNames);
                return false;
            }
            return true;
        }

        private static void SaveAnimClipAsset(AnimationClip clip, string animDir, string clipName)
        {
            clip.name = clipName;
            AssetDatabase.CreateAsset(clip, animDir + SanitizeAnimAssetFileName(clipName) + ".anim");
        }

        internal static List<string> BuildPlannedFxLayerNames(
            List<ClothObjInfo> clothList,
            List<OrnamentObjInfo> ornamentList,
            List<ExtraGroupObjInfo> extraGroupList)
        {
            var names = new List<string> { FxClothLayerName };
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                if (group.setList.Count == 0)
                    continue;
                names.Add(OutfitPropEditorDefines.FxExtraGroupLayerName(groupIndex, group.name));
            }
            for (var ornamentIndex = 0; ornamentIndex < ornamentList.Count; ornamentIndex++)
            {
                var info = ornamentList[ornamentIndex];
                names.Add(ResolveFxLayerName(info.name, "未命名配饰_" + (ornamentIndex + 1)));
            }
            for (var clothIndex = 0; clothIndex < clothList.Count; clothIndex++)
            {
                var info = clothList[clothIndex];
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    names.Add(ResolveFxLayerName(sub.item.name, "子开关_衣服_" + (clothIndex + 1) + "_" + (subIndex + 1)));
                }
            }
            for (var ornamentIndex = 0; ornamentIndex < ornamentList.Count; ornamentIndex++)
            {
                var info = ornamentList[ornamentIndex];
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    names.Add(ResolveFxLayerName(sub.item.name, "子开关_配饰_" + (ornamentIndex + 1) + "_" + (subIndex + 1)));
                }
            }
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    var info = group.setList[setIndex];
                    if (!info.enableSubMenu)
                        continue;
                    for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                    {
                        var sub = info.subToggleList[subIndex];
                        if (sub.item == null)
                            continue;
                        names.Add(ResolveFxLayerName(
                            sub.item.name,
                            "子开关_扩展_" + (groupIndex + 1) + "_" + (setIndex + 1) + "_" + (subIndex + 1)));
                    }
                }
            }
            return names;
        }

        internal static HashSet<string> GetFxLayersToRemove(AnimatorController fx, List<string> plannedLayerNames)
        {
            var remove = new HashSet<string>(plannedLayerNames);
            remove.Add("WardrobeCloth");
            remove.Add("换装");
            remove.Add(OutfitPropEditorDefines.FxExtraLayerLegacyName);
            foreach (var layer in fx.layers)
            {
                if (OutfitPropEditorDefines.IsLegacyFxLayer(layer.name))
                    remove.Add(layer.name);
            }
            return remove;
        }

        internal static bool ValidateFxLayerNames(AnimatorController fx, List<string> plannedLayerNames, HashSet<string> layersToRemove, out string errorMessage)
        {
            errorMessage = null;
            var duplicateNames = plannedLayerNames
                .GroupBy(n => n)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateNames.Count > 0)
            {
                errorMessage = OutfitPropEditorLoc.DuplicateLayerNamesError + string.Join("\n", duplicateNames);
                return false;
            }

            var conflicts = new List<string>();
            foreach (var layer in fx.layers)
            {
                if (layersToRemove.Contains(layer.name))
                    continue;
                if (plannedLayerNames.Contains(layer.name))
                    conflicts.Add(layer.name);
            }
            if (conflicts.Count > 0)
            {
                errorMessage = OutfitPropEditorLoc.FxLayerExistsError + string.Join("\n", conflicts);
                return false;
            }
            return true;
        }

        private sealed class ObjectControlRegistry
        {
            private readonly Dictionary<int, (GameObject go, HashSet<string> labels)> _entries = new();

            public void Add(GameObject go, Transform avatarRoot, string label)
            {
                if (go == null || avatarRoot == null || string.IsNullOrEmpty(label))
                    return;
                if (!OutfitPropEditorUtils.IsUnderAvatar(go.transform, avatarRoot))
                    return;

                var id = go.GetInstanceID();
                if (!_entries.TryGetValue(id, out var entry))
                {
                    entry = (go, new HashSet<string>());
                    _entries[id] = entry;
                }
                entry.labels.Add(label);
            }

            public List<(GameObject go, List<string> labels)> GetConflicts()
            {
                var conflicts = new List<(GameObject go, List<string> labels)>();
                foreach (var entry in _entries.Values)
                {
                    if (entry.labels.Count < 2)
                        continue;
                    conflicts.Add((entry.go, entry.labels.OrderBy(l => l, StringComparer.Ordinal).ToList()));
                }
                conflicts.Sort((a, b) => string.Compare(a.go.name, b.go.name, StringComparison.Ordinal));
                return conflicts;
            }
        }

        private static void RegisterClothObjectControls(
            ObjectControlRegistry registry,
            Transform avatarRoot,
            List<ClothObjInfo> clothInfoList)
        {
            for (var index = 0; index < clothInfoList.Count; index++)
            {
                var info = clothInfoList[index];
                var menuPath = OutfitPropEditorLoc.MenuClothDisplay + " / " + info.name;
                foreach (var obj in info.showObjectList)
                    registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleClothElements);
                if (info.enableExtraHide)
                {
                    foreach (var obj in info.hideObjectList)
                        registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleExtraHide);
                }
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var subName = string.IsNullOrEmpty(sub.name) ? sub.item.name : sub.name;
                    registry.Add(sub.item, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleSubMenu(subName));
                }
            }
        }

        private static void RegisterOrnamentObjectControls(
            ObjectControlRegistry registry,
            Transform avatarRoot,
            List<OrnamentObjInfo> ornamentInfoList)
        {
            for (var index = 0; index < ornamentInfoList.Count; index++)
            {
                var info = ornamentInfoList[index];
                var menuPath = OutfitPropEditorLoc.MenuPropDisplay + " / " + info.name;
                foreach (var obj in info.objectList)
                    registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleOrnamentObjects);
                if (info.enableExtraHide)
                {
                    foreach (var obj in info.hideObjectList)
                        registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleExtraHide);
                }
                if (!info.enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                {
                    var sub = info.subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var subName = string.IsNullOrEmpty(sub.name) ? sub.item.name : sub.name;
                    registry.Add(sub.item, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleSubMenu(subName));
                }
            }
        }

        private static void RegisterExtraObjectControls(
            ObjectControlRegistry registry,
            Transform avatarRoot,
            List<ExtraGroupObjInfo> extraGroupList)
        {
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    var info = group.setList[setIndex];
                    var menuPath = OutfitPropEditorLoc.MenuExtraDisplay + " / " + group.name + " / " + info.name;
                    foreach (var obj in info.showObjectList)
                        registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleShowObjects);
                    if (info.enableExtraHide)
                    {
                        foreach (var obj in info.hideObjectList)
                            registry.Add(obj, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleExtraHide);
                    }
                    if (!info.enableSubMenu)
                        continue;
                    for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                    {
                        var sub = info.subToggleList[subIndex];
                        if (sub.item == null)
                            continue;
                        var subName = string.IsNullOrEmpty(sub.name) ? sub.item.name : sub.name;
                        registry.Add(sub.item, avatarRoot, menuPath + " / " + OutfitPropEditorLoc.RoleSubMenu(subName));
                    }
                }
            }
        }

        internal static bool TryConfirmObjectControlConflicts(
            GameObject avatar,
            List<ClothObjInfo> clothInfoList,
            List<OrnamentObjInfo> ornamentInfoList,
            List<ExtraGroupObjInfo> extraGroupList)
        {
            if (avatar == null)
                return true;

            var registry = new ObjectControlRegistry();
            var avatarRoot = avatar.transform;
            RegisterClothObjectControls(registry, avatarRoot, clothInfoList);
            RegisterOrnamentObjectControls(registry, avatarRoot, ornamentInfoList);
            RegisterExtraObjectControls(registry, avatarRoot, extraGroupList);

            var conflicts = registry.GetConflicts();
            if (conflicts.Count == 0)
                return true;

            const int maxObjects = 12;
            var message = new StringBuilder();
            message.AppendLine(OutfitPropEditorLoc.ObjectConflictIntro);
            message.AppendLine();
            for (var i = 0; i < conflicts.Count && i < maxObjects; i++)
            {
                var conflict = conflicts[i];
                var hierarchyPath = OutfitPropEditorUtils.GetHierarchyPath(conflict.go.transform, avatarRoot);
                message.Append("「").Append(conflict.go.name).Append("」");
                if (!string.IsNullOrEmpty(hierarchyPath))
                    message.Append("（").Append(hierarchyPath).Append(')');
                message.AppendLine();
                foreach (var label in conflict.labels)
                    message.AppendLine("  · " + label);
                message.AppendLine();
            }
            if (conflicts.Count > maxObjects)
                message.AppendLine(OutfitPropEditorLoc.ObjectConflictMore(conflicts.Count - maxObjects));

            return EditorUtility.DisplayDialog(
                OutfitPropEditorLoc.ObjectConflictTitle,
                message.ToString().TrimEnd(),
                OutfitPropEditorLoc.ApplyAnyway,
                OutfitPropEditorLoc.GoBackEdit);
        }

        private static void AddFxBoolLayer(AnimatorController fxController, string layerName, string parameterName, bool defaultShow, AnimationClip[] clips)
        {
            fxController.AddParameter(new AnimatorControllerParameter
            {
                name = parameterName,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = defaultShow
            });
            var stateMachine = new AnimatorStateMachine
            {
                name = layerName,
                hideFlags = HideFlags.HideInHierarchy
            };
            AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(fxController));
            for (var show = 0; show <= 1; show++)
            {
                var state = stateMachine.AddState(clips[show].name);
                state.motion = clips[show];
                if (Convert.ToInt32(defaultShow) == show)
                    stateMachine.defaultState = state;
                var tran = stateMachine.AddAnyStateTransition(state);
                tran.duration = 0;
                tran.AddCondition(show == 1 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameterName);
            }
            fxController.AddLayer(new AnimatorControllerLayer
            {
                name = layerName,
                defaultWeight = 1f,
                stateMachine = stateMachine
            });
        }

        private static void FinalizeExpressionsMenuAsset(VRCExpressionsMenu menu, string assetPath)
        {
            if (menu == null)
                return;
            EditorUtility.SetDirty(menu);
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(menu)))
                AssetDatabase.CreateAsset(menu, assetPath);
            AssetDatabase.SaveAssetIfDirty(menu);
        }

        private static void TryAdvanceMenuPage(
            ref VRCExpressionsMenu current,
            VRCExpressionsMenu root,
            ref int pageIndex,
            string menuDir,
            string pageAssetPrefix)
        {
            if (current.controls.Count != OutfitPropEditorDefines.MenuControlsBeforeNextPage)
                return;

            var next = CreateInstance<VRCExpressionsMenu>();
            current.controls.Add(new VRCExpressionsMenu.Control
            {
                name = OutfitPropEditorLoc.NextPage,
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = next
            });
            if (current != root)
                FinalizeExpressionsMenuAsset(current, menuDir + pageAssetPrefix + pageIndex++ + ".asset");
            current = next;
        }

        private static void FinalizeMenuPages(
            VRCExpressionsMenu current,
            VRCExpressionsMenu root,
            int pageIndex,
            string menuDir,
            string pageAssetPrefix,
            string rootAssetPath)
        {
            if (current != root)
                FinalizeExpressionsMenuAsset(current, menuDir + pageAssetPrefix + pageIndex + ".asset");
            EditorUtility.SetDirty(root);
            FinalizeExpressionsMenuAsset(root, menuDir + rootAssetPath);
        }

        private sealed class PaginatedMenuBuilder
        {
            private readonly string _menuDir;
            private readonly string _rootAssetFileName;
            private readonly string _pageAssetPrefix;
            public VRCExpressionsMenu Root { get; }
            private VRCExpressionsMenu _current;
            private int _pageIndex;

            internal PaginatedMenuBuilder(string menuDir, string rootAssetFileName, string pageAssetPrefix)
            {
                _menuDir = menuDir;
                _rootAssetFileName = rootAssetFileName;
                _pageAssetPrefix = pageAssetPrefix;
                Root = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                _current = Root;
            }

            internal void AddControl(VRCExpressionsMenu.Control control)
            {
                TryAdvanceMenuPage(ref _current, Root, ref _pageIndex, _menuDir, _pageAssetPrefix);
                _current.controls.Add(control);
            }

            internal VRCExpressionsMenu FinalizeAll()
            {
                FinalizeMenuPages(_current, Root, _pageIndex, _menuDir, _pageAssetPrefix, _rootAssetFileName);
                return Root;
            }
        }

        internal static void ApplyToAvatar(
            GameObject avatar,
            List<ClothObjInfo> clothInfoList,
            int defaultClothIndex,
            List<OrnamentObjInfo> ornamentInfoList,
            List<ExtraGroupObjInfo> extraGroupList)
        {
            OutfitPropEditorUtils.ClearConsole();
            if (!TryConfirmObjectControlConflicts(avatar, clothInfoList, ornamentInfoList, extraGroupList))
                return;

            var avatarId = OutfitPropEditorUtils.GetAvatarId(avatar);
            var dirPath = OutfitPropEditorUtils.GetAvatarDataDir(avatarId);
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            var expressionParameters = descriptor.expressionParameters;
            var expressionsMenu = descriptor.expressionsMenu;

            var plannedFxLayerNames = BuildPlannedFxLayerNames(clothInfoList, ornamentInfoList, extraGroupList);
            var plannedFxAnimClipNames = BuildPlannedFxAnimClipNames(clothInfoList, ornamentInfoList, extraGroupList);
            if (!ValidateFxAnimClipNames(plannedFxAnimClipNames, out var earlyClipError))
            {
                EditorUtility.DisplayDialog(OutfitPropEditorLoc.AnimClipConflictTitle, earlyClipError, OutfitPropEditorLoc.Confirm);
                return;
            }

            AnimatorController fxForValidate = null;
            for (var i = 0; i < descriptor.baseAnimationLayers.Length; i++)
            {
                if (descriptor.baseAnimationLayers[i].type != VRCAvatarDescriptor.AnimLayerType.FX)
                    continue;
                fxForValidate = descriptor.baseAnimationLayers[i].animatorController as AnimatorController;
                break;
            }
            if (fxForValidate != null)
            {
                var fxLayersToRemoveEarly = GetFxLayersToRemove(fxForValidate, plannedFxLayerNames);
                if (!ValidateFxLayerNames(fxForValidate, plannedFxLayerNames, fxLayersToRemoveEarly, out var earlyLayerError))
                {
                    EditorUtility.DisplayDialog(OutfitPropEditorLoc.AnimLayerConflictTitle, earlyLayerError, OutfitPropEditorLoc.Confirm);
                    return;
                }
            }

            if (expressionParameters == null || expressionParameters.parameters == null)
            {
                expressionParameters = CreateInstance<VRCExpressionParameters>();
                var parameterTemplate = AssetDatabase.LoadAssetAtPath(
                    OutfitPropEditorDefines.DefaultExpressionParametersTemplate,
                    typeof(VRCExpressionParameters)) as VRCExpressionParameters;
                expressionParameters.parameters = parameterTemplate != null
                    ? parameterTemplate.parameters
                    : Array.Empty<VRCExpressionParameters.Parameter>();
                AssetDatabase.CreateAsset(expressionParameters, OutfitPropEditorUtils.JoinAssetPath(dirPath, "ExpressionParameters.asset"));
            }

            var newParameters = new List<VRCExpressionParameters.Parameter>();
            foreach (var parameter in expressionParameters.parameters)
                if (!OutfitPropEditorDefines.IsManagedParameter(parameter.name) && parameter.name != "")
                    newParameters.Add(parameter);

            newParameters.Add(new VRCExpressionParameters.Parameter
            {
                name = OutfitPropEditorDefines.ParamClothInt,
                valueType = VRCExpressionParameters.ValueType.Int,
                defaultValue = defaultClothIndex,
                saved = true
            });

            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                if (extraGroupList[groupIndex].setList.Count == 0)
                    continue;
                newParameters.Add(new VRCExpressionParameters.Parameter
                {
                    name = OutfitPropEditorDefines.ParamExtraGroupInt(groupIndex),
                    valueType = VRCExpressionParameters.ValueType.Int,
                    defaultValue = extraGroupList[groupIndex].defaultSetIndex,
                    saved = true
                });
            }

            for (var i = 0; i < ornamentInfoList.Count; i++)
            {
                newParameters.Add(new VRCExpressionParameters.Parameter
                {
                    name = OutfitPropEditorDefines.ParamOrn(i),
                    valueType = VRCExpressionParameters.ValueType.Bool,
                    defaultValue = ornamentInfoList[i].isShow ? 1 : 0,
                    saved = true
                });
            }

            for (var clothIndex = 0; clothIndex < clothInfoList.Count; clothIndex++)
            {
                if (!clothInfoList[clothIndex].enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < clothInfoList[clothIndex].subToggleList.Count; subIndex++)
                {
                    var sub = clothInfoList[clothIndex].subToggleList[subIndex];
                    newParameters.Add(new VRCExpressionParameters.Parameter
                    {
                        name = OutfitPropEditorDefines.ParamClothSub(clothIndex, subIndex),
                        valueType = VRCExpressionParameters.ValueType.Bool,
                        defaultValue = sub.defaultShow ? 1 : 0,
                        saved = true
                    });
                }
            }

            for (var ornamentIndex = 0; ornamentIndex < ornamentInfoList.Count; ornamentIndex++)
            {
                if (!ornamentInfoList[ornamentIndex].enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < ornamentInfoList[ornamentIndex].subToggleList.Count; subIndex++)
                {
                    var sub = ornamentInfoList[ornamentIndex].subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    newParameters.Add(new VRCExpressionParameters.Parameter
                    {
                        name = OutfitPropEditorDefines.ParamOrnSub(ornamentIndex, subIndex),
                        valueType = VRCExpressionParameters.ValueType.Bool,
                        defaultValue = sub.defaultShow ? 1 : 0,
                        saved = true
                    });
                }
            }

            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    if (!group.setList[setIndex].enableSubMenu)
                        continue;
                    for (var subIndex = 0; subIndex < group.setList[setIndex].subToggleList.Count; subIndex++)
                    {
                        var sub = group.setList[setIndex].subToggleList[subIndex];
                        newParameters.Add(new VRCExpressionParameters.Parameter
                        {
                            name = OutfitPropEditorDefines.ParamExtraSub(groupIndex, setIndex, subIndex),
                            valueType = VRCExpressionParameters.ValueType.Bool,
                            defaultValue = sub.defaultShow ? 1 : 0,
                            saved = true
                        });
                    }
                }
            }
            expressionParameters.parameters = newParameters.ToArray();

            OutfitPropEditorUtils.DeleteLegacyConcatenatedFolders(dirPath);
            var menuDir = OutfitPropEditorUtils.PrepareGeneratedAssetFolder(dirPath, "Menu/OutfitPropEditor");

            var mainClothMenu = CreateInstance<VRCExpressionsMenu>();
            {
                var clothMenu = mainClothMenu;
                var pageIndex = 0;
                for (var clothIndex = 0; clothIndex < clothInfoList.Count; clothIndex++)
                {
                    var info = clothInfoList[clothIndex];
                    TryAdvanceMenuPage(ref clothMenu, mainClothMenu, ref pageIndex, menuDir, "ClothMenu_");

                    var clothItemControl = new VRCExpressionsMenu.Control
                    {
                        name = info.name,
                        icon = info.image,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamClothInt },
                        value = clothIndex
                    };
                    if (info.enableSubMenu)
                    {
                        var clothSubBuilder = new PaginatedMenuBuilder(
                            menuDir,
                            "ClothSub_" + clothIndex + ".asset",
                            "ClothSub_" + clothIndex + "_Page_");
                        clothSubBuilder.AddControl(new VRCExpressionsMenu.Control
                        {
                            name = "穿戴",
                            icon = info.image,
                            type = VRCExpressionsMenu.Control.ControlType.Toggle,
                            parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamClothInt },
                            value = clothIndex
                        });
                        for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                        {
                            var sub = info.subToggleList[subIndex];
                            var displayName = sub.item != null
                                ? sub.item.name
                                : (string.IsNullOrEmpty(sub.name) ? ("子开关" + (subIndex + 1)) : sub.name);
                            clothSubBuilder.AddControl(new VRCExpressionsMenu.Control
                            {
                                name = displayName,
                                icon = sub.image,
                                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                                parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamClothSub(clothIndex, subIndex) }
                            });
                        }

                        var clothSubMenu = clothSubBuilder.FinalizeAll();
                        clothItemControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                        clothItemControl.subMenu = clothSubMenu;
                        clothItemControl.parameter = null;
                    }

                    clothMenu.controls.Add(clothItemControl);
                    EditorUtility.SetDirty(clothMenu);
                }
                FinalizeMenuPages(clothMenu, mainClothMenu, pageIndex, menuDir, "ClothMenu_", "ClothMenu.asset");
            }

            var mainExtraMenu = CreateInstance<VRCExpressionsMenu>();
            var hasExtraContent = false;
            for (var gi = 0; gi < extraGroupList.Count; gi++)
            {
                if (extraGroupList[gi].setList.Count > 0)
                {
                    hasExtraContent = true;
                    break;
                }
            }
            if (hasExtraContent)
            {
                var extraMenu = mainExtraMenu;
                var rootPageIndex = 0;
                for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
                {
                    var group = extraGroupList[groupIndex];
                    if (group.setList.Count == 0)
                        continue;

                    TryAdvanceMenuPage(ref extraMenu, mainExtraMenu, ref rootPageIndex, menuDir, "ExtraMenu_");

                    var groupMenu = CreateInstance<VRCExpressionsMenu>();
                    var groupPageIndex = 0;
                    var setMenu = groupMenu;
                    for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                    {
                        var info = group.setList[setIndex];
                        TryAdvanceMenuPage(
                            ref setMenu,
                            groupMenu,
                            ref groupPageIndex,
                            menuDir,
                            "ExtraGroup_" + groupIndex + "_Page_");

                        var extraItemControl = new VRCExpressionsMenu.Control
                        {
                            name = info.name,
                            icon = info.image,
                            type = VRCExpressionsMenu.Control.ControlType.Toggle,
                            parameter = new VRCExpressionsMenu.Control.Parameter
                            {
                                name = OutfitPropEditorDefines.ParamExtraGroupInt(groupIndex)
                            },
                            value = setIndex
                        };
                        if (info.enableSubMenu)
                        {
                            var extraSubBuilder = new PaginatedMenuBuilder(
                                menuDir,
                                "ExtraSub_" + groupIndex + "_" + setIndex + ".asset",
                                "ExtraSub_" + groupIndex + "_" + setIndex + "_Page_");
                            extraSubBuilder.AddControl(new VRCExpressionsMenu.Control
                            {
                                name = "穿戴",
                                icon = info.image,
                                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                                parameter = new VRCExpressionsMenu.Control.Parameter
                                {
                                    name = OutfitPropEditorDefines.ParamExtraGroupInt(groupIndex)
                                },
                                value = setIndex
                            });
                            for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                            {
                                var sub = info.subToggleList[subIndex];
                                var displayName = sub.item != null
                                    ? sub.item.name
                                    : (string.IsNullOrEmpty(sub.name) ? ("子开关" + (subIndex + 1)) : sub.name);
                                extraSubBuilder.AddControl(new VRCExpressionsMenu.Control
                                {
                                    name = displayName,
                                    icon = sub.image,
                                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                                    parameter = new VRCExpressionsMenu.Control.Parameter
                                    {
                                        name = OutfitPropEditorDefines.ParamExtraSub(groupIndex, setIndex, subIndex)
                                    }
                                });
                            }

                            var extraSubMenu = extraSubBuilder.FinalizeAll();
                            extraItemControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                            extraItemControl.subMenu = extraSubMenu;
                            extraItemControl.parameter = null;
                        }

                        setMenu.controls.Add(extraItemControl);
                    }

                    FinalizeMenuPages(
                        setMenu,
                        groupMenu,
                        groupPageIndex,
                        menuDir,
                        "ExtraGroup_" + groupIndex + "_Page_",
                        "ExtraGroup_" + groupIndex + ".asset");

                    extraMenu.controls.Add(new VRCExpressionsMenu.Control
                    {
                        name = group.name,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = groupMenu
                    });
                    EditorUtility.SetDirty(extraMenu);
                }
                FinalizeMenuPages(extraMenu, mainExtraMenu, rootPageIndex, menuDir, "ExtraMenu_", "ExtraMenu.asset");
            }

            var mainOrnamentMenu = CreateInstance<VRCExpressionsMenu>();
            if (ornamentInfoList.Count > 0)
            {
                var ornamentMenu = mainOrnamentMenu;
                var pageIndex = 0;
                for (var ornamentIndex = 0; ornamentIndex < ornamentInfoList.Count; ornamentIndex++)
                {
                    var info = ornamentInfoList[ornamentIndex];
                    TryAdvanceMenuPage(ref ornamentMenu, mainOrnamentMenu, ref pageIndex, menuDir, "OrnamentMenu_");

                    var ornamentItemControl = new VRCExpressionsMenu.Control
                    {
                        name = info.name,
                        icon = info.image,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamOrn(ornamentIndex) }
                    };
                    if (info.enableSubMenu)
                    {
                        var ornamentSubBuilder = new PaginatedMenuBuilder(
                            menuDir,
                            "OrnamentSub_" + ornamentIndex + ".asset",
                            "OrnamentSub_" + ornamentIndex + "_Page_");
                        ornamentSubBuilder.AddControl(new VRCExpressionsMenu.Control
                        {
                            name = "开关",
                            icon = info.image,
                            type = VRCExpressionsMenu.Control.ControlType.Toggle,
                            parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamOrn(ornamentIndex) }
                        });
                        for (var subIndex = 0; subIndex < info.subToggleList.Count; subIndex++)
                        {
                            var sub = info.subToggleList[subIndex];
                            var displayName = sub.item != null
                                ? sub.item.name
                                : (string.IsNullOrEmpty(sub.name) ? ("子开关" + (subIndex + 1)) : sub.name);
                            ornamentSubBuilder.AddControl(new VRCExpressionsMenu.Control
                            {
                                name = displayName,
                                icon = sub.image,
                                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                                parameter = new VRCExpressionsMenu.Control.Parameter { name = OutfitPropEditorDefines.ParamOrnSub(ornamentIndex, subIndex) }
                            });
                        }

                        var ornamentSubMenu = ornamentSubBuilder.FinalizeAll();
                        ornamentItemControl.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                        ornamentItemControl.subMenu = ornamentSubMenu;
                        ornamentItemControl.parameter = null;
                    }

                    ornamentMenu.controls.Add(ornamentItemControl);
                    EditorUtility.SetDirty(ornamentMenu);
                }
                FinalizeMenuPages(ornamentMenu, mainOrnamentMenu, pageIndex, menuDir, "OrnamentMenu_", "OrnamentMenu.asset");
            }

            if (expressionsMenu == null)
                expressionsMenu = CreateInstance<VRCExpressionsMenu>();

            VRCExpressionsMenu.Control clothControl = null;
            VRCExpressionsMenu.Control ornamentControl = null;
            VRCExpressionsMenu.Control extraControl = null;
            foreach (var control in expressionsMenu.controls)
            {
                if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
                    continue;
                if (OutfitPropEditorDefines.IsClothMenuControlName(control.name))
                    clothControl = control;
                else if (OutfitPropEditorDefines.IsPropMenuControlName(control.name))
                    ornamentControl = control;
                else if (OutfitPropEditorDefines.IsExtraMenuControlName(control.name))
                    extraControl = control;
            }

            if (clothInfoList.Count == 0)
            {
                if (clothControl != null)
                    expressionsMenu.controls.Remove(clothControl);
            }
            else if (clothControl == null)
            {
                expressionsMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = OutfitPropEditorDefines.MenuClothRoot,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = mainClothMenu
                });
            }
            else
            {
                clothControl.subMenu = mainClothMenu;
            }

            if (ornamentInfoList.Count == 0)
            {
                if (ornamentControl != null)
                    expressionsMenu.controls.Remove(ornamentControl);
            }
            else if (ornamentControl == null)
            {
                expressionsMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = OutfitPropEditorDefines.MenuPropRoot,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = mainOrnamentMenu
                });
            }
            else
            {
                ornamentControl.subMenu = mainOrnamentMenu;
            }

            if (!hasExtraContent)
            {
                if (extraControl != null)
                    expressionsMenu.controls.Remove(extraControl);
            }
            else if (extraControl == null)
            {
                expressionsMenu.controls.Add(new VRCExpressionsMenu.Control
                {
                    name = OutfitPropEditorDefines.MenuExtraRoot,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = mainExtraMenu
                });
            }
            else
            {
                extraControl.subMenu = mainExtraMenu;
            }

            if (AssetDatabase.GetAssetPath(expressionsMenu) == "")
                AssetDatabase.CreateAsset(expressionsMenu, OutfitPropEditorUtils.JoinAssetPath(dirPath, "ExpressionsMenu.asset"));
            else
                EditorUtility.SetDirty(expressionsMenu);

            var animDir = OutfitPropEditorUtils.PrepareGeneratedAssetFolder(dirPath, "Anim/OutfitPropEditor");

            var clothAnimClipList = new List<AnimationClip>();
            for (var index = 0; index < clothInfoList.Count; index++)
            {
                var clip = GetClothAnimClip(clothInfoList, avatar, index);
                clothAnimClipList.Add(clip);
                SaveAnimClipAsset(clip, animDir, clip.name);
            }

            var extraGroupAnimClips = new List<(int groupIndex, List<AnimationClip> clips)>();
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                if (group.setList.Count == 0)
                    continue;
                var clips = new List<AnimationClip>();
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    var clip = GetExtraSetAnimClip(group, groupIndex, group.setList, avatar, setIndex);
                    clips.Add(clip);
                    SaveAnimClipAsset(clip, animDir, clip.name);
                }
                extraGroupAnimClips.Add((groupIndex, clips));
            }

            var ornamentAnimClipList = new List<AnimationClip[]>();
            for (var index = 0; index < ornamentInfoList.Count; index++)
            {
                var clips = GetOrnamentAnimClip(ornamentInfoList, avatar, index);
                ornamentAnimClipList.Add(clips);
                SaveAnimClipAsset(clips[0], animDir, clips[0].name);
                SaveAnimClipAsset(clips[1], animDir, clips[1].name);
            }

            var clothSubClips = new List<(int clothIndex, int subIndex, bool defaultShow, AnimationClip[] clips)>();
            for (var clothIndex = 0; clothIndex < clothInfoList.Count; clothIndex++)
            {
                if (!clothInfoList[clothIndex].enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < clothInfoList[clothIndex].subToggleList.Count; subIndex++)
                {
                    var sub = clothInfoList[clothIndex].subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var layerName = ResolveFxLayerName(sub.item.name, "子开关_衣服_" + (clothIndex + 1) + "_" + (subIndex + 1));
                    var clips = GetSubToggleAnimClip(avatar, sub.item, layerName);
                    SaveAnimClipAsset(clips[0], animDir, clips[0].name);
                    SaveAnimClipAsset(clips[1], animDir, clips[1].name);
                    clothSubClips.Add((clothIndex, subIndex, sub.defaultShow, clips));
                }
            }

            var ornamentSubClips = new List<(int ornamentIndex, int subIndex, bool defaultShow, AnimationClip[] clips)>();
            for (var ornamentIndex = 0; ornamentIndex < ornamentInfoList.Count; ornamentIndex++)
            {
                if (!ornamentInfoList[ornamentIndex].enableSubMenu)
                    continue;
                for (var subIndex = 0; subIndex < ornamentInfoList[ornamentIndex].subToggleList.Count; subIndex++)
                {
                    var sub = ornamentInfoList[ornamentIndex].subToggleList[subIndex];
                    if (sub.item == null)
                        continue;
                    var layerName = ResolveFxLayerName(sub.item.name, "子开关_配饰_" + (ornamentIndex + 1) + "_" + (subIndex + 1));
                    var clips = GetSubToggleAnimClip(avatar, sub.item, layerName);
                    SaveAnimClipAsset(clips[0], animDir, clips[0].name);
                    SaveAnimClipAsset(clips[1], animDir, clips[1].name);
                    ornamentSubClips.Add((ornamentIndex, subIndex, sub.defaultShow, clips));
                }
            }

            var extraSubClips = new List<(int groupIndex, int setIndex, int subIndex, bool defaultShow, AnimationClip[] clips)>();
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                var group = extraGroupList[groupIndex];
                for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                {
                    if (!group.setList[setIndex].enableSubMenu)
                        continue;
                    for (var subIndex = 0; subIndex < group.setList[setIndex].subToggleList.Count; subIndex++)
                    {
                        var sub = group.setList[setIndex].subToggleList[subIndex];
                        if (sub.item == null)
                            continue;
                        var layerName = ResolveFxLayerName(
                            sub.item.name,
                            "子开关_扩展_" + (groupIndex + 1) + "_" + (setIndex + 1) + "_" + (subIndex + 1));
                        var clips = GetSubToggleAnimClip(avatar, sub.item, layerName);
                        SaveAnimClipAsset(clips[0], animDir, clips[0].name);
                        SaveAnimClipAsset(clips[1], animDir, clips[1].name);
                        extraSubClips.Add((groupIndex, setIndex, subIndex, sub.defaultShow, clips));
                    }
                }
            }

            AnimatorController fxController = null;
            descriptor.customizeAnimationLayers = true;
            for (var i = 0; i < descriptor.baseAnimationLayers.Length; i++)
            {
                var item = descriptor.baseAnimationLayers[i];
                if (item.type != VRCAvatarDescriptor.AnimLayerType.FX)
                    continue;
                fxController = item.animatorController as AnimatorController;
                if (fxController == null)
                {
                    fxController = new AnimatorController();
                    var stateMachine = new AnimatorStateMachine { name = "AllParts", hideFlags = HideFlags.HideInHierarchy };
                    var layer = new AnimatorControllerLayer
                    {
                        name = "AllParts",
                        defaultWeight = 1f,
                        stateMachine = stateMachine
                    };
                    fxController.AddLayer(layer);
                    AssetDatabase.CreateAsset(fxController, OutfitPropEditorUtils.JoinAssetPath(dirPath, "FXLayer.controller"));
                    AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(fxController));
                    descriptor.baseAnimationLayers[i].animatorController = fxController;
                    descriptor.baseAnimationLayers[i].isEnabled = true;
                    descriptor.baseAnimationLayers[i].isDefault = false;
                }
                break;
            }

            if (fxController == null)
            {
                EditorUtility.DisplayDialog(
                    OutfitPropEditorLoc.ErrorTitle,
                    OutfitPropEditorLoc.PlayableLayersError,
                    OutfitPropEditorLoc.Confirm);
                return;
            }

            var fxLayersToRemove = GetFxLayersToRemove(fxController, plannedFxLayerNames);

            for (var i = 0; i < fxController.parameters.Length; i++)
            {
                if (!OutfitPropEditorDefines.IsManagedParameter(fxController.parameters[i].name))
                    continue;
                fxController.RemoveParameter(i);
                i--;
            }
            for (var i = 0; i < fxController.layers.Length; i++)
            {
                if (!fxLayersToRemove.Contains(fxController.layers[i].name))
                    continue;
                fxController.RemoveLayer(i);
                i--;
            }

            fxController.AddParameter(new AnimatorControllerParameter
            {
                name = OutfitPropEditorDefines.ParamClothInt,
                type = AnimatorControllerParameterType.Int,
                defaultInt = defaultClothIndex
            });
            for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
            {
                if (extraGroupList[groupIndex].setList.Count == 0)
                    continue;
                fxController.AddParameter(new AnimatorControllerParameter
                {
                    name = OutfitPropEditorDefines.ParamExtraGroupInt(groupIndex),
                    type = AnimatorControllerParameterType.Int,
                    defaultInt = extraGroupList[groupIndex].defaultSetIndex
                });
            }
            {
                var clothLayerName = FxClothLayerName;
                var stateMachine = new AnimatorStateMachine
                {
                    name = clothLayerName,
                    hideFlags = HideFlags.HideInHierarchy
                };
                AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(fxController));
                stateMachine.defaultState = stateMachine.AddState("Idle");
                for (var index = 0; index < clothAnimClipList.Count; index++)
                {
                    var state = stateMachine.AddState(clothAnimClipList[index].name);
                    state.motion = clothAnimClipList[index];
                    var tran = stateMachine.AddAnyStateTransition(state);
                    tran.duration = 0;
                    tran.AddCondition(AnimatorConditionMode.Equals, index, OutfitPropEditorDefines.ParamClothInt);
                }
                fxController.AddLayer(new AnimatorControllerLayer
                {
                    name = clothLayerName,
                    defaultWeight = 1f,
                    stateMachine = stateMachine
                });
            }

            foreach (var groupAnim in extraGroupAnimClips)
            {
                var group = extraGroupList[groupAnim.groupIndex];
                var extraLayerName = OutfitPropEditorDefines.FxExtraGroupLayerName(groupAnim.groupIndex, group.name);
                var stateMachine = new AnimatorStateMachine
                {
                    name = extraLayerName,
                    hideFlags = HideFlags.HideInHierarchy
                };
                AssetDatabase.AddObjectToAsset(stateMachine, AssetDatabase.GetAssetPath(fxController));
                stateMachine.defaultState = stateMachine.AddState("Idle");
                for (var index = 0; index < groupAnim.clips.Count; index++)
                {
                    var state = stateMachine.AddState(groupAnim.clips[index].name);
                    state.motion = groupAnim.clips[index];
                    var tran = stateMachine.AddAnyStateTransition(state);
                    tran.duration = 0;
                    tran.AddCondition(
                        AnimatorConditionMode.Equals,
                        index,
                        OutfitPropEditorDefines.ParamExtraGroupInt(groupAnim.groupIndex));
                }
                fxController.AddLayer(new AnimatorControllerLayer
                {
                    name = extraLayerName,
                    defaultWeight = 1f,
                    stateMachine = stateMachine
                });
            }

            for (var index = 0; index < ornamentInfoList.Count; index++)
            {
                var info = ornamentInfoList[index];
                var layerName = ResolveFxLayerName(info.name, "未命名配饰_" + (index + 1));
                AddFxBoolLayer(fxController, layerName, OutfitPropEditorDefines.ParamOrn(index), info.isShow, ornamentAnimClipList[index]);
            }

            foreach (var item in clothSubClips)
            {
                var sub = clothInfoList[item.clothIndex].subToggleList[item.subIndex];
                var layerName = ResolveFxLayerName(sub.item.name, "子开关_衣服_" + (item.clothIndex + 1) + "_" + (item.subIndex + 1));
                AddFxBoolLayer(fxController, layerName, OutfitPropEditorDefines.ParamClothSub(item.clothIndex, item.subIndex), item.defaultShow, item.clips);
            }

            foreach (var item in ornamentSubClips)
            {
                var sub = ornamentInfoList[item.ornamentIndex].subToggleList[item.subIndex];
                var layerName = ResolveFxLayerName(sub.item.name, "子开关_配饰_" + (item.ornamentIndex + 1) + "_" + (item.subIndex + 1));
                AddFxBoolLayer(fxController, layerName, OutfitPropEditorDefines.ParamOrnSub(item.ornamentIndex, item.subIndex), item.defaultShow, item.clips);
            }

            foreach (var item in extraSubClips)
            {
                var sub = extraGroupList[item.groupIndex].setList[item.setIndex].subToggleList[item.subIndex];
                var layerName = ResolveFxLayerName(
                    sub.item.name,
                    "子开关_扩展_" + (item.groupIndex + 1) + "_" + (item.setIndex + 1) + "_" + (item.subIndex + 1));
                AddFxBoolLayer(
                    fxController,
                    layerName,
                    OutfitPropEditorDefines.ParamExtraSub(item.groupIndex, item.setIndex, item.subIndex),
                    item.defaultShow,
                    item.clips);
            }

            EditorUtility.SetDirty(expressionsMenu);
            EditorUtility.SetDirty(expressionParameters);
            descriptor.customExpressions = true;
            descriptor.expressionParameters = expressionParameters;
            descriptor.expressionsMenu = expressionsMenu;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(OutfitPropEditorLoc.Notice, OutfitPropEditorLoc.ApplySuccess, OutfitPropEditorLoc.Confirm);
        }
    }
}
#endif
