#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AvatarOutfitPropEditor
{
    public static class OutfitPropEditorUtils
    {
        private const string AvatarTagPrefix = "outfitprop_";
        private const int AvatarTagLength = 43;

        private static MethodInfo clearConsoleMethod;

        public static string DataRoot => OutfitPropEditorDefines.DataRoot;

        public static string GetAvatarDataDir(string avatarId) => $"{DataRoot}/{avatarId}";

        public static string GetConfigAssetPath(string avatarId) =>
            $"{GetAvatarDataDir(avatarId)}/{OutfitPropEditorDefines.ConfigFileName}";

        public static void ClearConsole()
        {
            if (clearConsoleMethod == null)
            {
                var logType = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
                clearConsoleMethod = logType?.GetMethod("Clear");
            }
            clearConsoleMethod?.Invoke(null, null);
        }

        public static List<T> LinkGameObjectList<T>(List<T> list1, List<T> list2)
        {
            var newList = new List<T>();
            foreach (var obj in list1)
                if (obj != null && !newList.Contains(obj))
                    newList.Add(obj);
            foreach (var obj in list2)
                if (obj != null && !newList.Contains(obj))
                    newList.Add(obj);
            return newList;
        }

        public static void MoveListItem<T>(ref List<T> list, int src, int tar)
        {
            var item = list[tar];
            list[tar] = list[src];
            list[src] = item;
        }

        public static string GetHierarchyPath(Transform transform, Transform relativeTo = null)
        {
            if (transform == null)
                return "";
            var path = transform.name;
            var current = transform.parent;
            while (current != null && current != relativeTo)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        public static bool IsUnderAvatar(Transform transform, Transform avatarRoot)
        {
            if (transform == null || avatarRoot == null)
                return false;
            if (transform == avatarRoot)
                return true;
            return transform.IsChildOf(avatarRoot);
        }

        public static string GetAvatarRelativePath(Transform transform, Transform avatarRoot)
        {
            if (!IsUnderAvatar(transform, avatarRoot))
                return null;
            if (transform == avatarRoot)
                return "";
            return GetHierarchyPath(transform, avatarRoot);
        }

        public static string GetAvatarId(GameObject avatar)
        {
            if (avatar == null)
                return null;
            var tag = avatar.tag;
            if (!tag.StartsWith(AvatarTagPrefix) || tag.Length != AvatarTagLength)
                return null;
            return tag.Substring(AvatarTagPrefix.Length);
        }

        public static string GetOrCreateAvatarId(GameObject avatar)
        {
            var id = GetAvatarId(avatar);
            if (id != null)
                return id;
            id = CreateRandomCode(32);
            while (HasTag(AvatarTagPrefix + id))
                id = CreateRandomCode(32);
            AddTag(AvatarTagPrefix + id, avatar);
            return id;
        }

        private static string CreateRandomCode(int len)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var builder = new StringBuilder();
            var random = new System.Random();
            for (var i = 0; i < len; i++)
                builder.Append(chars[random.Next(chars.Length)]);
            return builder.ToString();
        }

        private static void AddTag(string tag, GameObject obj)
        {
            if (HasTag(tag))
                return;
            var tagManager = new SerializedObject(obj);
            var iterator = tagManager.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.name != "m_TagString")
                    continue;
                iterator.stringValue = tag;
                tagManager.ApplyModifiedProperties();
                break;
            }
        }

        private static bool HasTag(string tag)
        {
            foreach (var existing in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existing.Contains(tag))
                    return true;
            }
            return false;
        }
    }
}
#endif
