#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace AvatarOutfitPropEditor
{
    public class OutfitPropEditorWindow : OutfitPropEditorCore
    {
        private Vector2 scrollPos;
        private int tabIndex;
        protected SerializedObject serializedObject;

        private GameObject avatar;
        private OutfitPropConfig parameter;
        private string avatarId;
        private int defaultClothIndex = -1;

        public List<ClothObjInfo> clothInfoList = new List<ClothObjInfo>();
        public List<OrnamentObjInfo> ornamentInfoList = new List<OrnamentObjInfo>();
        public List<ExtraGroupObjInfo> extraGroupList = new List<ExtraGroupObjInfo>();

        private void OnEnable()
        {
            titleContent = new GUIContent(OutfitPropEditorLoc.WindowTitle);
            serializedObject = new SerializedObject(this);
            foreach (var info in clothInfoList)
            {
                info.animBool.valueChanged.RemoveAllListeners();
                info.animBool.valueChanged.AddListener(Repaint);
            }
            foreach (var info in ornamentInfoList)
            {
                info.animBool.valueChanged.RemoveAllListeners();
                info.animBool.valueChanged.AddListener(Repaint);
            }
            foreach (var group in extraGroupList)
            {
                group.animBool.valueChanged.RemoveAllListeners();
                group.animBool.valueChanged.AddListener(Repaint);
                foreach (var set in group.setList)
                {
                    set.animBool.valueChanged.RemoveAllListeners();
                    set.animBool.valueChanged.AddListener(Repaint);
                }
            }
        }

        private void OnGUI()
        {
            OutfitPropEditorGui.EnsureStyles();

            GUILayout.Space(8);
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(OutfitPropEditorLoc.WindowTitle);
            GUI.skin.label.fontSize = 12;
            GUILayout.Label(OutfitPropEditorLoc.WindowSubtitle);
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.Space(6);
            if (OutfitPropEditorGui.DrawLanguageSelector())
            {
                titleContent = new GUIContent(OutfitPropEditorLoc.WindowTitle);
                Repaint();
            }
            GUILayout.Space(6);

            var newAvatar = (GameObject)EditorGUILayout.ObjectField(OutfitPropEditorLoc.SelectAvatar, avatar, typeof(GameObject), true);

            if (avatar != newAvatar)
            {
                avatar = newAvatar;
                tabIndex = 0;
                if (newAvatar != null && newAvatar.GetComponent<VRCAvatarDescriptor>() == null)
                {
                    avatar = null;
                    EditorUtility.DisplayDialog(
                        OutfitPropEditorLoc.Notice,
                        OutfitPropEditorLoc.AvatarSdkOnly,
                        OutfitPropEditorLoc.Confirm);
                }
                avatarId = OutfitPropEditorUtils.GetAvatarId(avatar);
                parameter = GetParameter(avatarId);
                if (parameter == null && avatar != null)
                    parameter = CreateParameter(avatar);
                ReadParameter();
            }

            if (avatar == null)
            {
                OutfitPropEditorGui.SectionGap(10);
                EditorGUILayout.HelpBox(OutfitPropEditorLoc.SelectAvatarHint, MessageType.Info);
                GUILayout.FlexibleSpace();
                OutfitPropEditorGui.DrawAuthorCredit();
                return;
            }

            OutfitPropEditorGui.SectionGap(10);
            tabIndex = GUILayout.Toolbar(tabIndex, new[]
            {
                OutfitPropEditorLoc.TabCloth,
                OutfitPropEditorLoc.TabOrnament,
                OutfitPropEditorLoc.TabExtra
            });
            OutfitPropEditorGui.SectionGap(10);

            if (tabIndex == 0)
                OnGUICloth();
            else if (tabIndex == 1)
                OnGUIOrnament();
            else
                OnGUIExtraSet();

            OutfitPropEditorGui.SectionGap(12);
            if (GUILayout.Button(OutfitPropEditorLoc.ApplyToAvatar, GUILayout.Height(28)))
                ApplyToAvatar(avatar, clothInfoList, defaultClothIndex, ornamentInfoList, extraGroupList);

            OutfitPropEditorGui.SectionGap(6);
            if (GUILayout.Button(OutfitPropEditorLoc.ClearTab, GUILayout.Height(24)))
                ClearCurrentTab();

            GUILayout.FlexibleSpace();
            OutfitPropEditorGui.DrawAuthorCredit();
        }

        private void OnGUICloth()
        {
            if (GUILayout.Button(OutfitPropEditorLoc.CreateCloth, GUILayout.Height(24)))
                AddCloth();

            OutfitPropEditorGui.SectionGap(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            if (clothInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox(OutfitPropEditorLoc.NoClothHint, MessageType.Info);
            }
            else
            {
                serializedObject.Update();
                var nameList = new List<string>();
                foreach (var info in clothInfoList)
                    nameList.Add(info.name);

                EditorGUI.BeginChangeCheck();
                for (var index = 0; index < clothInfoList.Count; index++)
                {
                    if (index > 0)
                        OutfitPropEditorGui.SectionGap(6);

                    var info = clothInfoList[index];
                    var title = info.name + (defaultClothIndex == index ? OutfitPropEditorLoc.DefaultMark : "");
                    var newTarget = EditorGUILayout.Foldout(info.animBool.target, title, true);
                    if (newTarget != info.animBool.target)
                    {
                        if (newTarget)
                            foreach (var item in clothInfoList)
                                item.animBool.target = false;
                        info.animBool.target = newTarget;
                    }
                    if (!EditorGUILayout.BeginFadeGroup(info.animBool.faded))
                    {
                        EditorGUILayout.EndFadeGroup();
                        continue;
                    }

                    OutfitPropEditorGui.DrawSectionTitle(OutfitPropEditorLoc.BasicInfo);
                    info.image = (Texture2D)EditorGUILayout.ObjectField(OutfitPropEditorLoc.Icon, info.image, typeof(Texture2D), true);
                    var newName = EditorGUILayout.TextField(OutfitPropEditorLoc.ClothName, info.name).Trim();
                    if (!string.IsNullOrEmpty(newName) && !nameList.Contains(newName))
                        info.name = newName;

                    EditorGUILayout.BeginHorizontal();
                    if (index > 0 && GUILayout.Button(OutfitPropEditorLoc.MoveUp))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref clothInfoList, index, index - 1);
                        if (defaultClothIndex == index) defaultClothIndex--;
                        else if (defaultClothIndex == index - 1) defaultClothIndex++;
                        break;
                    }
                    if (index < clothInfoList.Count - 1 && GUILayout.Button(OutfitPropEditorLoc.MoveDown))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref clothInfoList, index, index + 1);
                        if (defaultClothIndex == index) defaultClothIndex++;
                        else if (defaultClothIndex == index + 1) defaultClothIndex--;
                        break;
                    }
                    if (GUILayout.Button(OutfitPropEditorLoc.Preview))
                    {
                        defaultClothIndex = index;
                        PrviewCloth(clothInfoList, index);
                    }
                    if (GUILayout.Button(OutfitPropEditorLoc.Delete))
                    {
                        DelCloth(index);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ObjectElements);
                    DrawObjectList(OutfitPropEditorLoc.ClothElements, info.showObjectList, avatar);
                    OutfitPropEditorGui.EndSection();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ExtraHide);
                    DrawExtraHideSection(ref info.enableExtraHide, info.hideObjectList, avatar);
                    OutfitPropEditorGui.EndSection();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.SubMenu);
                    DrawSubMenuSection(ref info.enableSubMenu, OutfitPropEditorLoc.SubMenuClothDesc, info.subToggleList, avatar);
                    OutfitPropEditorGui.EndSection();

                    EditorGUILayout.EndFadeGroup();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    WriteParameter();
                }
            }
            GUILayout.EndScrollView();
        }

        private void OnGUIExtraSet()
        {
            EditorGUILayout.HelpBox(OutfitPropEditorLoc.ExtraHelp, MessageType.Info);
            OutfitPropEditorGui.SectionGap(6);

            if (GUILayout.Button(OutfitPropEditorLoc.AddGroup, GUILayout.Height(24)))
                AddExtraGroup();

            OutfitPropEditorGui.SectionGap(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            if (extraGroupList.Count == 0)
            {
                EditorGUILayout.HelpBox(OutfitPropEditorLoc.NoExtraHint, MessageType.Info);
            }
            else
            {
                serializedObject.Update();
                var groupNameList = new List<string>();
                foreach (var g in extraGroupList)
                    groupNameList.Add(g.name);

                EditorGUI.BeginChangeCheck();
                for (var groupIndex = 0; groupIndex < extraGroupList.Count; groupIndex++)
                {
                    if (groupIndex > 0)
                        OutfitPropEditorGui.SectionGap(6);

                    var group = extraGroupList[groupIndex];
                    var groupTitle = OutfitPropEditorLoc.GroupTitleFormat(group.name, group.setList.Count);
                    var newGroupTarget = EditorGUILayout.Foldout(group.animBool.target, groupTitle, true);
                    if (newGroupTarget != group.animBool.target)
                    {
                        if (newGroupTarget)
                            foreach (var item in extraGroupList)
                                item.animBool.target = false;
                        group.animBool.target = newGroupTarget;
                    }
                    if (!EditorGUILayout.BeginFadeGroup(group.animBool.faded))
                    {
                        EditorGUILayout.EndFadeGroup();
                        continue;
                    }

                    OutfitPropEditorGui.DrawSectionTitle(OutfitPropEditorLoc.GroupSection);
                    var newGroupName = EditorGUILayout.TextField(OutfitPropEditorLoc.GroupName, group.name).Trim();
                    if (!string.IsNullOrEmpty(newGroupName) && !groupNameList.Contains(newGroupName))
                        group.name = newGroupName;

                    EditorGUILayout.BeginHorizontal();
                    if (groupIndex > 0 && GUILayout.Button(OutfitPropEditorLoc.MoveUpGroup))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref extraGroupList, groupIndex, groupIndex - 1);
                        break;
                    }
                    if (groupIndex < extraGroupList.Count - 1 && GUILayout.Button(OutfitPropEditorLoc.MoveDownGroup))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref extraGroupList, groupIndex, groupIndex + 1);
                        break;
                    }
                    if (GUILayout.Button(OutfitPropEditorLoc.DeleteGroup))
                    {
                        DelExtraGroup(groupIndex);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    OutfitPropEditorGui.SectionGap(8);
                    if (GUILayout.Button(OutfitPropEditorLoc.AddSetInGroup))
                        AddExtraSetToGroup(groupIndex);

                    var setNameList = new List<string>();
                    foreach (var s in group.setList)
                        setNameList.Add(s.name);

                    for (var setIndex = 0; setIndex < group.setList.Count; setIndex++)
                    {
                        OutfitPropEditorGui.SectionGap(6);
                        var info = group.setList[setIndex];
                        var setTitle = "  " + info.name + (group.defaultSetIndex == setIndex ? OutfitPropEditorLoc.DefaultMark : "");
                        var newSetTarget = EditorGUILayout.Foldout(info.animBool.target, setTitle, true);
                        if (newSetTarget != info.animBool.target)
                        {
                            if (newSetTarget)
                                foreach (var item in group.setList)
                                    item.animBool.target = false;
                            info.animBool.target = newSetTarget;
                        }
                        if (!EditorGUILayout.BeginFadeGroup(info.animBool.faded))
                        {
                            EditorGUILayout.EndFadeGroup();
                            continue;
                        }

                        info.image = (Texture2D)EditorGUILayout.ObjectField(OutfitPropEditorLoc.Icon, info.image, typeof(Texture2D), true);
                        var newSetName = EditorGUILayout.TextField(OutfitPropEditorLoc.SetName, info.name).Trim();
                        if (!string.IsNullOrEmpty(newSetName) && !setNameList.Contains(newSetName))
                            info.name = newSetName;

                        EditorGUILayout.BeginHorizontal();
                        if (setIndex > 0 && GUILayout.Button(OutfitPropEditorLoc.MoveUp))
                        {
                            OutfitPropEditorUtils.MoveListItem(ref group.setList, setIndex, setIndex - 1);
                            if (group.defaultSetIndex == setIndex) group.defaultSetIndex--;
                            else if (group.defaultSetIndex == setIndex - 1) group.defaultSetIndex++;
                            break;
                        }
                        if (setIndex < group.setList.Count - 1 && GUILayout.Button(OutfitPropEditorLoc.MoveDown))
                        {
                            OutfitPropEditorUtils.MoveListItem(ref group.setList, setIndex, setIndex + 1);
                            if (group.defaultSetIndex == setIndex) group.defaultSetIndex++;
                            else if (group.defaultSetIndex == setIndex + 1) group.defaultSetIndex--;
                            break;
                        }
                        if (GUILayout.Button(OutfitPropEditorLoc.Preview))
                        {
                            group.defaultSetIndex = setIndex;
                            PrviewExtraSet(group.setList, setIndex);
                        }
                        if (GUILayout.Button(OutfitPropEditorLoc.Delete))
                        {
                            DelExtraSetFromGroup(groupIndex, setIndex);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();

                        OutfitPropEditorGui.SectionGap(8);
                        OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ObjectElements);
                        DrawObjectList(OutfitPropEditorLoc.ShowObjects, info.showObjectList, avatar);
                        OutfitPropEditorGui.EndSection();

                        OutfitPropEditorGui.SectionGap(8);
                        OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ExtraHide);
                        DrawExtraHideSection(ref info.enableExtraHide, info.hideObjectList, avatar);
                        OutfitPropEditorGui.EndSection();

                        OutfitPropEditorGui.SectionGap(8);
                        OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.SubMenu);
                        DrawSubMenuSection(ref info.enableSubMenu, OutfitPropEditorLoc.SubMenuExtraDesc, info.subToggleList, avatar);
                        OutfitPropEditorGui.EndSection();

                        EditorGUILayout.EndFadeGroup();
                    }

                    EditorGUILayout.EndFadeGroup();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    WriteParameter();
                }
            }
            GUILayout.EndScrollView();
        }

        private void OnGUIOrnament()
        {
            if (GUILayout.Button(OutfitPropEditorLoc.CreateOrnament, GUILayout.Height(24)))
                AddOrnament();

            OutfitPropEditorGui.SectionGap(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            if (ornamentInfoList.Count == 0)
            {
                EditorGUILayout.HelpBox(OutfitPropEditorLoc.NoOrnamentHint, MessageType.Info);
            }
            else
            {
                serializedObject.Update();
                var nameList = new List<string>();
                foreach (var info in ornamentInfoList)
                    nameList.Add(info.name);

                EditorGUI.BeginChangeCheck();
                for (var index = 0; index < ornamentInfoList.Count; index++)
                {
                    if (index > 0)
                        OutfitPropEditorGui.SectionGap(6);

                    var info = ornamentInfoList[index];
                    var title = info.name + (info.isShow ? OutfitPropEditorLoc.VisibleMark : OutfitPropEditorLoc.HiddenMark);
                    var newTarget = EditorGUILayout.Foldout(info.animBool.target, title, true);
                    if (newTarget != info.animBool.target)
                    {
                        if (newTarget)
                            foreach (var item in ornamentInfoList)
                                item.animBool.target = false;
                        info.animBool.target = newTarget;
                    }

                    if (!EditorGUILayout.BeginFadeGroup(info.animBool.faded))
                    {
                        EditorGUILayout.EndFadeGroup();
                        continue;
                    }

                    OutfitPropEditorGui.DrawSectionTitle(OutfitPropEditorLoc.BasicInfo);
                    info.image = (Texture2D)EditorGUILayout.ObjectField(OutfitPropEditorLoc.Texture, info.image, typeof(Texture2D), true);
                    var newName = EditorGUILayout.TextField(OutfitPropEditorLoc.OrnamentName, info.name).Trim();
                    if (!string.IsNullOrEmpty(newName) && !nameList.Contains(newName))
                        info.name = newName;
                    EditorGUI.BeginChangeCheck();
                    info.isShow = EditorGUILayout.Toggle(OutfitPropEditorLoc.DefaultShow, info.isShow);
                    if (EditorGUI.EndChangeCheck())
                        ApplyOrnamentPreviewVisibility(info);

                    EditorGUILayout.BeginHorizontal();
                    if (index > 0 && GUILayout.Button(OutfitPropEditorLoc.MoveUp))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref ornamentInfoList, index, index - 1);
                        break;
                    }
                    if (index < ornamentInfoList.Count - 1 && GUILayout.Button(OutfitPropEditorLoc.MoveDown))
                    {
                        OutfitPropEditorUtils.MoveListItem(ref ornamentInfoList, index, index + 1);
                        break;
                    }
                    if (GUILayout.Button(OutfitPropEditorLoc.Delete))
                    {
                        DelOrnament(index);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ObjectElements);
                    DrawObjectList(OutfitPropEditorLoc.OrnamentElements, info.objectList, avatar);
                    OutfitPropEditorGui.EndSection();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.ExtraHide);
                    DrawExtraHideSection(ref info.enableExtraHide, info.hideObjectList, avatar);
                    OutfitPropEditorGui.EndSection();

                    OutfitPropEditorGui.SectionGap(10);

                    OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.SubMenu);
                    DrawSubMenuSection(ref info.enableSubMenu, OutfitPropEditorLoc.SubMenuOrnamentDesc, info.subToggleList, avatar);
                    OutfitPropEditorGui.EndSection();

                    EditorGUILayout.EndFadeGroup();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    WriteParameter();
                }
            }
            GUILayout.EndScrollView();
        }

        private void ApplyOrnamentPreviewVisibility(OrnamentObjInfo info)
        {
            foreach (var obj in info.objectList)
            {
                if (obj != null)
                    obj.SetActive(info.isShow);
            }
            if (!info.enableExtraHide)
                return;
            foreach (var obj in info.hideObjectList)
            {
                if (obj != null)
                    obj.SetActive(!info.isShow);
            }
        }

        private static void ApplySubTogglePreviewVisibility(SubToggleObjInfo sub)
        {
            if (sub?.item != null)
                sub.item.SetActive(sub.defaultShow);
        }

        private void DrawObjectList(string label, List<GameObject> list, GameObject avatarRoot)
        {
            if (!string.IsNullOrEmpty(label))
                OutfitPropEditorGui.DrawSectionTitle(label);

            for (var i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = (GameObject)EditorGUILayout.ObjectField(list[i], typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    list.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            var dropRect = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            OutfitPropEditorGui.DrawDropZone(dropRect, OutfitPropEditorLoc.DragObjectsHere);
            var evt = Event.current;
            if (!dropRect.Contains(evt.mousePosition))
                return;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (!(obj is GameObject go) || go == null)
                        continue;
                    if (!OutfitPropEditorUtils.IsUnderAvatar(go.transform, avatarRoot.transform))
                    {
                        EditorUtility.DisplayDialog(
                            OutfitPropEditorLoc.Notice,
                            OutfitPropEditorLoc.NotInHierarchy(go.name),
                            OutfitPropEditorLoc.Confirm);
                        continue;
                    }
                    if (!list.Contains(go))
                        list.Add(go);
                }
                WriteParameter();
            }
            evt.Use();
        }

        private void DrawExtraHideSection(ref bool enabled, List<GameObject> hideList, GameObject avatarRoot)
        {
            enabled = EditorGUILayout.Toggle(OutfitPropEditorLoc.ExtraHide, enabled);
            if (!enabled)
                return;

            GUILayout.Space(6);
            DrawObjectList(null, hideList, avatarRoot);
        }

        private void DrawSubMenuSection(ref bool enabled, string description, List<SubToggleObjInfo> list, GameObject avatarRoot)
        {
            enabled = EditorGUILayout.Toggle(OutfitPropEditorLoc.MakeSubMenu, enabled);
            if (!string.IsNullOrEmpty(description))
                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            if (!enabled)
                return;

            GUILayout.Space(6);
            DrawSubToggleList(null, list, avatarRoot);
        }

        private void DrawSubToggleList(string label, List<SubToggleObjInfo> list, GameObject avatarRoot)
        {
            if (!string.IsNullOrEmpty(label))
                OutfitPropEditorGui.DrawSectionTitle(label);

            for (var i = 0; i < list.Count; i++)
            {
                var sub = list[i];
                if (sub.item != null)
                    sub.name = sub.item.name;

                OutfitPropEditorGui.BeginSection(OutfitPropEditorGui.SectionKind.SubMenuItem);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.IsNullOrEmpty(sub.name) ? OutfitPropEditorLoc.Unnamed : sub.name, EditorStyles.boldLabel);
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    list.RemoveAt(i);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    OutfitPropEditorGui.EndSection();
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                sub.item = (GameObject)EditorGUILayout.ObjectField(OutfitPropEditorLoc.ControlObject, sub.item, typeof(GameObject), true);
                if (EditorGUI.EndChangeCheck() && sub.item != null)
                    ApplySubTogglePreviewVisibility(sub);
                if (sub.item != null)
                    sub.name = sub.item.name;
                sub.image = (Texture2D)EditorGUILayout.ObjectField(OutfitPropEditorLoc.ButtonTextureOptional, sub.image, typeof(Texture2D), true);
                EditorGUI.BeginChangeCheck();
                sub.defaultShow = EditorGUILayout.Toggle(OutfitPropEditorLoc.DefaultShow, sub.defaultShow);
                if (EditorGUI.EndChangeCheck())
                    ApplySubTogglePreviewVisibility(sub);
                OutfitPropEditorGui.EndSection();

                if (i < list.Count - 1)
                    GUILayout.Space(6);
            }

            GUILayout.Space(4);
            var dropRect = GUILayoutUtility.GetRect(0f, 36f, GUILayout.ExpandWidth(true));
            OutfitPropEditorGui.DrawDropZone(dropRect, OutfitPropEditorLoc.DragSubObjectsHere);
            var evt = Event.current;
            if (!dropRect.Contains(evt.mousePosition))
                return;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type != EventType.DragPerform)
                return;

            DragAndDrop.AcceptDrag();
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (!(obj is GameObject go) || go == null)
                    continue;
                if (!OutfitPropEditorUtils.IsUnderAvatar(go.transform, avatarRoot.transform))
                {
                    EditorUtility.DisplayDialog(
                        OutfitPropEditorLoc.Notice,
                        OutfitPropEditorLoc.NotInHierarchy(go.name),
                        OutfitPropEditorLoc.Confirm);
                    continue;
                }
                if (list.Exists(s => s.item == go))
                    continue;
                var subToggle = new SubToggleObjInfo
                {
                    name = go.name,
                    item = go,
                    defaultShow = true
                };
                list.Add(subToggle);
                ApplySubTogglePreviewVisibility(subToggle);
            }
            WriteParameter();
            evt.Use();
        }

        private void ClearCurrentTab()
        {
            string tabName;
            switch (tabIndex)
            {
                case 1:
                    tabName = OutfitPropEditorLoc.TabOrnament;
                    break;
                case 2:
                    tabName = OutfitPropEditorLoc.TabExtra;
                    break;
                default:
                    tabName = OutfitPropEditorLoc.TabCloth;
                    break;
            }

            if (!EditorUtility.DisplayDialog(
                    OutfitPropEditorLoc.ClearTitle,
                    OutfitPropEditorLoc.ClearMessage(tabName),
                    OutfitPropEditorLoc.Confirm,
                    OutfitPropEditorLoc.Cancel))
                return;

            switch (tabIndex)
            {
                case 1:
                    ornamentInfoList.Clear();
                    break;
                case 2:
                    extraGroupList.Clear();
                    break;
                default:
                    clothInfoList.Clear();
                    defaultClothIndex = -1;
                    break;
            }

            scrollPos = Vector2.zero;
            WriteParameter();
            Repaint();
        }

        private bool CanAdd()
        {
            return clothInfoList.Count + ornamentInfoList.Count + CountExtraSets(extraGroupList) < maxClothNum;
        }

        private void AddCloth()
        {
            if (!CanAdd())
            {
                EditorUtility.DisplayDialog(OutfitPropEditorLoc.Notice, OutfitPropEditorLoc.Limit255, OutfitPropEditorLoc.Confirm);
                return;
            }
            foreach (var info in clothInfoList)
                info.animBool.target = false;
            var infoNew = new ClothObjInfo(OutfitPropEditorLoc.DefaultClothName(clothInfoList.Count + 1));
            infoNew.animBool.valueChanged.AddListener(Repaint);
            infoNew.animBool.target = true;
            clothInfoList.Add(infoNew);
            if (defaultClothIndex < 0)
                defaultClothIndex = 0;
            WriteParameter();
        }

        private void DelCloth(int index)
        {
            if (!EditorUtility.DisplayDialog(OutfitPropEditorLoc.Attention, OutfitPropEditorLoc.DeleteCloth, OutfitPropEditorLoc.Confirm, OutfitPropEditorLoc.Cancel))
                return;
            clothInfoList.RemoveAt(index);
            if (defaultClothIndex >= clothInfoList.Count)
                defaultClothIndex = clothInfoList.Count - 1;
            WriteParameter();
        }

        private void AddExtraGroup()
        {
            foreach (var g in extraGroupList)
                g.animBool.target = false;
            var groupNew = new ExtraGroupObjInfo(OutfitPropEditorLoc.DefaultGroupName(extraGroupList.Count + 1));
            groupNew.animBool.valueChanged.AddListener(Repaint);
            groupNew.animBool.target = true;
            extraGroupList.Add(groupNew);
            WriteParameter();
        }

        private void DelExtraGroup(int groupIndex)
        {
            if (!EditorUtility.DisplayDialog(OutfitPropEditorLoc.Attention, OutfitPropEditorLoc.DeleteGroupConfirm, OutfitPropEditorLoc.Confirm, OutfitPropEditorLoc.Cancel))
                return;
            extraGroupList.RemoveAt(groupIndex);
            WriteParameter();
        }

        private void AddExtraSetToGroup(int groupIndex)
        {
            if (!CanAdd())
            {
                EditorUtility.DisplayDialog(OutfitPropEditorLoc.Notice, OutfitPropEditorLoc.Limit255Extra, OutfitPropEditorLoc.Confirm);
                return;
            }
            var group = extraGroupList[groupIndex];
            foreach (var info in group.setList)
                info.animBool.target = false;
            var infoNew = new ExtraSetObjInfo(OutfitPropEditorLoc.DefaultSetName(group.setList.Count + 1));
            infoNew.animBool.valueChanged.AddListener(Repaint);
            infoNew.animBool.target = true;
            group.setList.Add(infoNew);
            if (group.defaultSetIndex < 0)
                group.defaultSetIndex = 0;
            WriteParameter();
        }

        private void DelExtraSetFromGroup(int groupIndex, int setIndex)
        {
            if (!EditorUtility.DisplayDialog(OutfitPropEditorLoc.Attention, OutfitPropEditorLoc.DeleteSet, OutfitPropEditorLoc.Confirm, OutfitPropEditorLoc.Cancel))
                return;
            var group = extraGroupList[groupIndex];
            group.setList.RemoveAt(setIndex);
            if (group.defaultSetIndex >= group.setList.Count)
                group.defaultSetIndex = group.setList.Count - 1;
            WriteParameter();
        }

        private void AddOrnament()
        {
            if (!CanAdd())
            {
                EditorUtility.DisplayDialog(OutfitPropEditorLoc.Notice, OutfitPropEditorLoc.Limit255, OutfitPropEditorLoc.Confirm);
                return;
            }
            foreach (var info in ornamentInfoList)
                info.animBool.target = false;
            var infoNew = new OrnamentObjInfo(OutfitPropEditorLoc.DefaultOrnamentName(ornamentInfoList.Count + 1));
            infoNew.animBool.valueChanged.AddListener(Repaint);
            infoNew.animBool.target = true;
            ornamentInfoList.Add(infoNew);
            WriteParameter();
        }

        private void DelOrnament(int index)
        {
            if (!EditorUtility.DisplayDialog(OutfitPropEditorLoc.Attention, OutfitPropEditorLoc.DeleteOrnament, OutfitPropEditorLoc.Confirm, OutfitPropEditorLoc.Cancel))
                return;
            ornamentInfoList.RemoveAt(index);
            WriteParameter();
        }

        private void ReadParameter()
        {
            defaultClothIndex = -1;
            clothInfoList.Clear();
            ornamentInfoList.Clear();
            extraGroupList.Clear();
            if (parameter == null || avatar == null)
                return;

            foreach (var info in parameter.clothList)
            {
                var cloth = new ClothObjInfo
                {
                    name = info.name,
                    image = info.menuImage,
                    enableSubMenu = info.enableSubMenu || info.subToggleList.Count > 0,
                    enableExtraHide = info.enableExtraHide || info.hidePaths.Count > 0
                };
                cloth.animBool.valueChanged.AddListener(Repaint);
                foreach (var showPath in info.showPaths)
                {
                    var t = avatar.transform.Find(showPath);
                    if (t != null)
                        cloth.showObjectList.Add(t.gameObject);
                }
                foreach (var hidePath in info.hidePaths)
                {
                    var t = avatar.transform.Find(hidePath);
                    if (t != null)
                        cloth.hideObjectList.Add(t.gameObject);
                }
                foreach (var sub in info.subToggleList)
                {
                    var t = avatar.transform.Find(sub.itemPath);
                    if (t == null)
                        continue;
                    cloth.subToggleList.Add(new SubToggleObjInfo
                    {
                        name = t.gameObject.name,
                        image = sub.menuImage,
                        defaultShow = sub.defaultShow,
                        item = t.gameObject
                    });
                }
                clothInfoList.Add(cloth);
            }

            defaultClothIndex = parameter.defaultClothIndex;

            if (parameter.extraGroupList != null && parameter.extraGroupList.Count > 0)
            {
                foreach (var groupInfo in parameter.extraGroupList)
                {
                    var group = new ExtraGroupObjInfo
                    {
                        name = string.IsNullOrEmpty(groupInfo.groupName) ? OutfitPropEditorLoc.DefaultGroupFallback : groupInfo.groupName,
                        defaultSetIndex = groupInfo.defaultSetIndex
                    };
                    group.animBool.valueChanged.AddListener(Repaint);
                    foreach (var info in groupInfo.setList ?? new List<OutfitPropConfig.ExtraSetInfo>())
                        group.setList.Add(LoadExtraSetFromConfig(info));
                    extraGroupList.Add(group);
                }
            }
            else if (parameter.extraSetList != null && parameter.extraSetList.Count > 0)
            {
                var migrated = new ExtraGroupObjInfo(OutfitPropEditorLoc.MigratedGroupName)
                {
                    defaultSetIndex = parameter.defaultExtraSetIndex
                };
                migrated.animBool.valueChanged.AddListener(Repaint);
                foreach (var info in parameter.extraSetList)
                    migrated.setList.Add(LoadExtraSetFromConfig(info));
                extraGroupList.Add(migrated);
            }

            foreach (var info in parameter.ornamentList)
            {
                var ornament = new OrnamentObjInfo
                {
                    name = info.name,
                    image = info.menuImage,
                    isShow = info.isShow,
                    enableSubMenu = info.enableSubMenu || info.subToggleList.Count > 0,
                    enableExtraHide = info.enableExtraHide || info.hidePaths.Count > 0
                };
                ornament.animBool.valueChanged.AddListener(Repaint);
                foreach (var path in info.itemPaths)
                {
                    var t = avatar.transform.Find(path);
                    if (t != null)
                        ornament.objectList.Add(t.gameObject);
                }
                foreach (var hidePath in info.hidePaths)
                {
                    var t = avatar.transform.Find(hidePath);
                    if (t != null)
                        ornament.hideObjectList.Add(t.gameObject);
                }
                foreach (var sub in info.subToggleList)
                {
                    var t = avatar.transform.Find(sub.itemPath);
                    if (t == null)
                        continue;
                    ornament.subToggleList.Add(new SubToggleObjInfo
                    {
                        name = t.gameObject.name,
                        image = sub.menuImage,
                        defaultShow = sub.defaultShow,
                        item = t.gameObject
                    });
                }
                ornamentInfoList.Add(ornament);
            }
        }

        private ExtraSetObjInfo LoadExtraSetFromConfig(OutfitPropConfig.ExtraSetInfo info)
        {
            var extraSet = new ExtraSetObjInfo
            {
                name = info.name,
                image = info.menuImage,
                enableSubMenu = info.enableSubMenu || info.subToggleList.Count > 0,
                enableExtraHide = info.enableExtraHide || info.hidePaths.Count > 0
            };
            extraSet.animBool.valueChanged.AddListener(Repaint);
            foreach (var showPath in info.showPaths)
            {
                var t = avatar.transform.Find(showPath);
                if (t != null)
                    extraSet.showObjectList.Add(t.gameObject);
            }
            foreach (var hidePath in info.hidePaths)
            {
                var t = avatar.transform.Find(hidePath);
                if (t != null)
                    extraSet.hideObjectList.Add(t.gameObject);
            }
            foreach (var sub in info.subToggleList)
            {
                var t = avatar.transform.Find(sub.itemPath);
                if (t == null)
                    continue;
                extraSet.subToggleList.Add(new SubToggleObjInfo
                {
                    name = t.gameObject.name,
                    image = sub.menuImage,
                    defaultShow = sub.defaultShow,
                    item = t.gameObject
                });
            }
            return extraSet;
        }

        private void WriteParameter()
        {
            if (avatar == null || parameter == null)
                return;
            var clothList = new List<OutfitPropConfig.ClothInfo>();
            var clothItemList = new List<GameObject>();

            foreach (var info in clothInfoList)
            {
                var cloth = new OutfitPropConfig.ClothInfo
                {
                    name = info.name,
                    menuImage = info.image,
                    enableSubMenu = info.enableSubMenu,
                    enableExtraHide = info.enableExtraHide
                };
                for (var i = 0; i < info.showObjectList.Count; i++)
                {
                    var obj = info.showObjectList[i];
                    if (obj == null) continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                    if (path == null)
                    {
                        info.showObjectList[i] = null;
                        continue;
                    }
                    if (!cloth.showPaths.Contains(path))
                        cloth.showPaths.Add(path);
                    if (!clothItemList.Contains(obj))
                        clothItemList.Add(obj);
                }
                for (var i = 0; i < info.hideObjectList.Count; i++)
                {
                    var obj = info.hideObjectList[i];
                    if (obj == null) continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                    if (path == null || clothItemList.Contains(obj))
                    {
                        info.hideObjectList[i] = null;
                        continue;
                    }
                    if (!cloth.hidePaths.Contains(path))
                        cloth.hidePaths.Add(path);
                }
                foreach (var sub in info.subToggleList)
                {
                    if (sub.item == null)
                        continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(sub.item.transform, avatar.transform);
                    if (path == null)
                        continue;
                    cloth.subToggleList.Add(new OutfitPropConfig.SubToggleInfo
                    {
                        name = sub.item.name,
                        menuImage = sub.image,
                        defaultShow = sub.defaultShow,
                        itemPath = path
                    });
                }
                clothList.Add(cloth);
            }

            var extraGroupConfigList = new List<OutfitPropConfig.ExtraGroupInfo>();
            foreach (var group in extraGroupList)
            {
                var groupConfig = new OutfitPropConfig.ExtraGroupInfo
                {
                    groupName = group.name,
                    defaultSetIndex = group.defaultSetIndex
                };
                foreach (var info in group.setList)
                {
                    var extraSet = new OutfitPropConfig.ExtraSetInfo
                    {
                        name = info.name,
                        menuImage = info.image,
                        enableSubMenu = info.enableSubMenu,
                        enableExtraHide = info.enableExtraHide
                    };
                    for (var i = 0; i < info.showObjectList.Count; i++)
                    {
                        var obj = info.showObjectList[i];
                        if (obj == null) continue;
                        var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                        if (path == null)
                        {
                            info.showObjectList[i] = null;
                            continue;
                        }
                        if (!extraSet.showPaths.Contains(path))
                            extraSet.showPaths.Add(path);
                    }
                    for (var i = 0; i < info.hideObjectList.Count; i++)
                    {
                        var obj = info.hideObjectList[i];
                        if (obj == null) continue;
                        var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                        if (path == null)
                        {
                            info.hideObjectList[i] = null;
                            continue;
                        }
                        if (!extraSet.hidePaths.Contains(path))
                            extraSet.hidePaths.Add(path);
                    }
                    foreach (var sub in info.subToggleList)
                    {
                        if (sub.item == null)
                            continue;
                        var path = OutfitPropEditorUtils.GetAvatarRelativePath(sub.item.transform, avatar.transform);
                        if (path == null)
                            continue;
                        extraSet.subToggleList.Add(new OutfitPropConfig.SubToggleInfo
                        {
                            name = sub.item.name,
                            menuImage = sub.image,
                            defaultShow = sub.defaultShow,
                            itemPath = path
                        });
                    }
                    groupConfig.setList.Add(extraSet);
                }
                extraGroupConfigList.Add(groupConfig);
            }

            var ornamentList = new List<OutfitPropConfig.OrnamentInfo>();
            foreach (var info in ornamentInfoList)
            {
                var ornament = new OutfitPropConfig.OrnamentInfo
                {
                    name = info.name,
                    menuImage = info.image,
                    isShow = info.isShow,
                    enableSubMenu = info.enableSubMenu,
                    enableExtraHide = info.enableExtraHide
                };
                for (var i = 0; i < info.objectList.Count; i++)
                {
                    var obj = info.objectList[i];
                    if (obj == null) continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                    if (path == null)
                    {
                        info.objectList[i] = null;
                        continue;
                    }
                    if (!ornament.itemPaths.Contains(path))
                        ornament.itemPaths.Add(path);
                }
                for (var i = 0; i < info.hideObjectList.Count; i++)
                {
                    var obj = info.hideObjectList[i];
                    if (obj == null) continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(obj.transform, avatar.transform);
                    if (path == null)
                    {
                        info.hideObjectList[i] = null;
                        continue;
                    }
                    if (ornament.itemPaths.Contains(path))
                    {
                        info.hideObjectList[i] = null;
                        continue;
                    }
                    if (!ornament.hidePaths.Contains(path))
                        ornament.hidePaths.Add(path);
                }
                foreach (var sub in info.subToggleList)
                {
                    if (sub.item == null)
                        continue;
                    var path = OutfitPropEditorUtils.GetAvatarRelativePath(sub.item.transform, avatar.transform);
                    if (path == null)
                        continue;
                    ornament.subToggleList.Add(new OutfitPropConfig.SubToggleInfo
                    {
                        name = sub.item.name,
                        menuImage = sub.image,
                        defaultShow = sub.defaultShow,
                        itemPath = path
                    });
                }
                ornamentList.Add(ornament);
            }

            parameter.defaultClothIndex = defaultClothIndex;
            parameter.clothList = clothList;
            parameter.extraGroupList = extraGroupConfigList;
            parameter.extraSetList = new List<OutfitPropConfig.ExtraSetInfo>();
            parameter.defaultExtraSetIndex = -1;
            parameter.ornamentList = ornamentList;
            EditorUtility.SetDirty(parameter);
        }
    }
}
#endif
