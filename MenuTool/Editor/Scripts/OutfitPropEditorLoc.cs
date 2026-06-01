#if UNITY_EDITOR
using UnityEditor;

namespace AvatarOutfitPropEditor
{
    public enum OutfitPropEditorLanguage
    {
        Chinese = 0,
        Japanese = 1,
        English = 2,
        Korean = 3
    }

    public static class OutfitPropEditorLoc
    {
        private const string PrefsKey = "OutfitPropEditor.Language";

        private static OutfitPropEditorLanguage _current = OutfitPropEditorLanguage.Chinese;

        static OutfitPropEditorLoc()
        {
            _current = (OutfitPropEditorLanguage)EditorPrefs.GetInt(PrefsKey, (int)OutfitPropEditorLanguage.Chinese);
        }

        public static OutfitPropEditorLanguage Current
        {
            get => _current;
            set
            {
                if (_current == value)
                    return;
                _current = value;
                EditorPrefs.SetInt(PrefsKey, (int)value);
            }
        }

        public static readonly string[] LanguageToolbarLabels = { "中文", "日本語", "EN", "한국어" };

        private static string T(string zh, string ja, string en, string ko)
        {
            switch (_current)
            {
                case OutfitPropEditorLanguage.Japanese:
                    return ja;
                case OutfitPropEditorLanguage.English:
                    return en;
                case OutfitPropEditorLanguage.Korean:
                    return ko;
                default:
                    return zh;
            }
        }

        public static string WindowTitle => T(
            "菜单一键制作工具",
            "メニュー一括作成ツール",
            "Menu Builder",
            "메뉴 일괄 제작 도구");

        public static string WindowSubtitle => T(
            "一键配置模型功能菜单",
            "アバターの機能メニューを一括設定",
            "Configure avatar expression menus in one place",
            "아바타 기능 메뉴를 한 번에 설정");

        public static string LanguageLabel => T("语言", "言語", "Language", "언어");

        public static string SelectAvatar => T("选择模型", "モデルを選択", "Avatar", "모델 선택");
        public static string SelectAvatarHint => T("选择你的模型。", "モデルを選択してください。", "Select your avatar.", "모델을 선택하세요.");
        public static string AvatarSdkOnly => T(
            "本插件仅支持挂有 VRChatSDK3 AvatarDescriptor 的模型。",
            "VRChat SDK3 の AvatarDescriptor が付いたモデルのみ対応しています。",
            "This tool only supports models with a VRChat SDK3 AvatarDescriptor.",
            "VRChat SDK3 AvatarDescriptor가 있는 모델만 지원합니다.");

        public static string TabCloth => T("衣服", "衣装", "Outfits", "의상");
        public static string TabOrnament => T("配饰", "アクセサリー", "Props", "액세서리");
        public static string TabExtra => T("扩展", "拡張", "Extra", "확장");

        public static string NextPage => T("下一页", "次のページ", "Next Page", "다음 페이지");

        public static string ApplyToAvatar => T("一键应用到模型", "モデルに一括適用", "Apply to Avatar", "모델에 일괄 적용");
        public static string ClearTab => T("一键清空", "一括クリア", "Clear Tab", "일괄 비우기");

        public static string Notice => T("提醒", "お知らせ", "Notice", "알림");
        public static string Attention => T("注意", "注意", "Warning", "주의");
        public static string Confirm => T("确认", "確認", "OK", "확인");
        public static string Cancel => T("取消", "キャンセル", "Cancel", "취소");
        public static string Download => T("前往下载", "ダウンロード", "Download", "다운로드");

        public static string CreateCloth => T("创建衣服", "衣装を追加", "Add Outfit", "의상 추가");
        public static string NoClothHint => T(
            "当前没有衣服，点击上方“创建衣服”。",
            "衣装がありません。上の「衣装を追加」を押してください。",
            "No outfits yet. Click \"Add Outfit\" above.",
            "의상이 없습니다. 위의 「의상 추가」를 누르세요.");

        public static string DefaultMark => T("（默认）", "（デフォルト）", " (default)", "（기본）");
        public static string BasicInfo => T("基本信息", "基本情報", "Basic", "기본 정보");
        public static string Icon => T("图标", "アイコン", "Icon", "아이콘");
        public static string ClothName => T("衣服名称", "衣装名", "Outfit name", "의상 이름");
        public static string MoveUp => T("上移", "上へ", "Up", "위로");
        public static string MoveDown => T("下移", "下へ", "Down", "아래로");
        public static string Preview => T("预览", "プレビュー", "Preview", "미리보기");
        public static string Delete => T("删除", "削除", "Delete", "삭제");
        public static string ClothElements => T("衣服元素", "衣装オブジェクト", "Outfit objects", "의상 오브젝트");
        public static string SubMenuClothDesc => T(
            "用于控制衣服中的子物体",
            "衣装内の子オブジェクトを制御します",
            "Toggle child objects within this outfit",
            "의상 내 하위 오브젝트를 제어합니다");

        public static string ExtraHelp => T(
            "先添加分组（如发型、特效），每组内再添加多套方案；组内互斥切换，不同分组可同时生效。",
            "グループ（髪型・エフェクトなど）を追加し、各グループに複数プリセットを登録します。グループ内は排他、グループ間は同時に有効です。",
            "Add groups (hair, effects, etc.), then add presets per group. Presets exclude each other within a group; groups work independently.",
            "그룹(헤어, 이펙트 등)을 추가한 뒤 각 그룹에 여러 프리셋을 넣습니다. 그룹 내는 배타, 그룹 간은 동시 적용됩니다.");

        public static string AddGroup => T("添加分组", "グループを追加", "Add Group", "그룹 추가");
        public static string NoExtraHint => T(
            "点击「添加分组」开始配置。",
            "「グループを追加」から設定を始めてください。",
            "Click \"Add Group\" to get started.",
            "「그룹 추가」를 눌러 설정을 시작하세요.");

        public static string GroupSection => T("分组", "グループ", "Group", "그룹");
        public static string GroupName => T("分组名称", "グループ名", "Group name", "그룹 이름");
        public static string MoveUpGroup => T("上移分组", "グループを上へ", "Move group up", "그룹 위로");
        public static string MoveDownGroup => T("下移分组", "グループを下へ", "Move group down", "그룹 아래로");
        public static string DeleteGroup => T("删除分组", "グループを削除", "Delete group", "그룹 삭제");
        public static string AddSetInGroup => T("在本组添加一套方案", "このグループにプリセットを追加", "Add preset to group", "이 그룹에 프리셋 추가");
        public static string SetName => T("方案名称", "プリセット名", "Preset name", "프리셋 이름");
        public static string ShowObjects => T("要显示的物体", "表示するオブジェクト", "Objects to show", "표시할 오브젝트");
        public static string SubMenuExtraDesc => T(
            "用于控制本套内的子物体",
            "このプリセット内の子オブジェクトを制御します",
            "Toggle child objects within this preset",
            "이 프리셋 내 하위 오브젝트를 제어합니다");

        public static string CreateOrnament => T("创建配饰", "アクセサリーを追加", "Add Prop", "액세서리 추가");
        public static string NoOrnamentHint => T(
            "当前没有配饰，点击上方“创建配饰”。",
            "アクセサリーがありません。上の「アクセサリーを追加」を押してください。",
            "No props yet. Click \"Add Prop\" above.",
            "액세서리가 없습니다. 위의 「액세서리 추가」를 누르세요.");

        public static string Texture => T("贴图", "テクスチャ", "Icon", "텍스처");
        public static string OrnamentName => T("配饰名称", "アクセサリー名", "Prop name", "액세서리 이름");
        public static string DefaultShow => T("默认显示", "初期表示", "Shown by default", "기본 표시");
        public static string VisibleMark => T("（显示）", "（表示）", " (on)", "（표시）");
        public static string HiddenMark => T("（隐藏）", "（非表示）", " (off)", "（숨김）");
        public static string OrnamentElements => T("配饰元素", "アクセサリーオブジェクト", "Prop objects", "액세서리 오브젝트");
        public static string SubMenuOrnamentDesc => T(
            "用于控制配饰中的子物体",
            "アクセサリー内の子オブジェクトを制御します",
            "Toggle child objects within this prop",
            "액세서리 내 하위 오브젝트를 제어합니다");

        public static string ExtraHide => T("额外隐藏", "追加で非表示", "Extra hide", "추가 숨김");
        public static string MakeSubMenu => T("制作子菜单", "サブメニューを作る", "Sub-menu toggles", "서브 메뉴 만들기");
        public static string Unnamed => T("（未命名）", "（無名）", "(unnamed)", "（이름 없음）");
        public static string ControlObject => T("控制物体", "制御オブジェクト", "Controlled object", "제어 오브젝트");
        public static string ButtonTextureOptional => T("按钮贴图（可选）", "ボタン画像（任意）", "Button icon (optional)", "버튼 텍스처（선택）");
        public static string DragObjectsHere => T(
            "拖拽层级物体到这里自动添加",
            "ヒエラルキーのオブジェクトをここにドラッグして追加",
            "Drag hierarchy objects here to add",
            "계층 오브젝트를 여기로 끌어다 추가");
        public static string DragSubObjectsHere => T(
            "拖拽需要控制的子物体到这里",
            "制御する子オブジェクトをここにドラッグ",
            "Drag child objects to control here",
            "제어할 하위 오브젝트를 여기로 끌어다 놓기");

        public static string NotInHierarchy(string objectName) => T(
            $"「{objectName}」不在当前模型层级中，已跳过。",
            $"「{objectName}」は現在のモデル階層にありません。スキップしました。",
            $"\"{objectName}\" is not under the current avatar and was skipped.",
            $"「{objectName}」이(가) 현재 모델 계층에 없어 건너뛰었습니다.");

        public static string ClearTitle => T("一键清空", "一括クリア", "Clear tab", "일괄 비우기");
        public static string ClearMessage(string tabName) => T(
            $"确认清空当前「{tabName}」选项卡中的所有内容？\n清空后需重新配置或再次点击「一键应用到模型」才会更新模型。",
            $"現在の「{tabName}」タブの内容をすべて削除しますか？\n削除後は再設定するか、「モデルに一括適用」で反映してください。",
            $"Clear everything in the \"{tabName}\" tab?\nReconfigure and click \"Apply to Avatar\" to update the model.",
            $"현재 「{tabName}」 탭의 모든 내용을 비울까요?\n비운 뒤 다시 설정하거나 「모델에 일괄 적용」으로 반영하세요.");

        public static string Limit255 => T(
            "衣服+配饰+扩展总数已达到上限 255。",
            "衣装・アクセサリー・拡張の合計が上限 255 に達しました。",
            "Combined outfits, props, and extra presets reached the limit of 255.",
            "의상+액세서리+확장 합계가 상한 255에 도달했습니다.");

        public static string Limit255Extra => T(
            "衣服+配饰+扩展方案总数已达到上限 255。",
            "衣装・アクセサリー・拡張プリセットの合計が上限 255 に達しました。",
            "Combined count reached the limit of 255.",
            "의상+액세서리+확장 프리셋 합계가 상한 255에 도달했습니다.");

        public static string DeleteCloth => T("确认删除这套衣服？", "この衣装を削除しますか？", "Delete this outfit?", "이 의상을 삭제할까요?");
        public static string DeleteGroupConfirm => T(
            "确认删除该分组及其下所有方案？",
            "このグループとすべてのプリセットを削除しますか？",
            "Delete this group and all its presets?",
            "이 그룹과 모든 프리셋을 삭제할까요?");
        public static string DeleteSet => T("确认删除这套方案？", "このプリセットを削除しますか？", "Delete this preset?", "이 프리셋을 삭제할까요?");
        public static string DeleteOrnament => T("确认删除这件配饰？", "このアクセサリーを削除しますか？", "Delete this prop?", "이 액세서리를 삭제할까요?");

        public static string DefaultClothName(int index) => T($"衣服{index}", $"衣装{index}", $"Outfit {index}", $"의상{index}");
        public static string DefaultOrnamentName(int index) => T($"配饰{index}", $"アクセサリー{index}", $"Prop {index}", $"액세서리{index}");
        public static string DefaultGroupName(int index) => T($"分组{index}", $"グループ{index}", $"Group {index}", $"그룹{index}");
        public static string DefaultSetName(int index) => T($"方案{index}", $"プリセット{index}", $"Preset {index}", $"프리셋{index}");
        public static string DefaultGroupFallback => T("分组", "グループ", "Group", "그룹");
        public static string MigratedGroupName => T("默认分组", "デフォルトグループ", "Default group", "기본 그룹");

        public static string GroupTitleFormat(string groupName, int setCount) => T(
            $"{groupName}（{setCount} 套）",
            $"{groupName}（{setCount} 件）",
            $"{groupName} ({setCount} presets)",
            $"{groupName}（{setCount}개）");

        // Conflict dialog (editor display paths)
        public static string MenuClothDisplay => T("服装", "衣装", "Outfits", "의상");
        public static string MenuPropDisplay => T("道具", "小物", "Props", "소품");
        public static string MenuExtraDisplay => T("扩展", "拡張", "Extra", "확장");
        public static string RoleClothElements => T("衣服元素", "衣装オブジェクト", "outfit objects", "의상 오브젝트");
        public static string RoleOrnamentObjects => T("配饰物体", "アクセサリーオブジェクト", "prop objects", "액세서리 오브젝트");
        public static string RoleShowObjects => T("要显示的物体", "表示オブジェクト", "objects to show", "표시 오브젝트");
        public static string RoleExtraHide => T("额外隐藏", "追加非表示", "extra hide", "추가 숨김");
        public static string RoleSubMenu(string name) => T($"子菜单「{name}」", $"サブメニュー「{name}」", $"sub-menu \"{name}\"", $"서브 메뉴 「{name}」");

        public static string ObjectConflictTitle => T("物体控制冲突", "オブジェクト制御の競合", "Object control conflict", "오브젝트 제어 충돌");
        public static string ObjectConflictIntro => T(
            "以下物体被两处及以上菜单项控制，同时应用可能导致显示异常：",
            "次のオブジェクトは複数のメニュー項目で制御されています。適用すると表示が不安定になる可能性があります：",
            "These objects are controlled by multiple menu entries. Applying may cause display issues:",
            "다음 오브젝트가 두 곳 이상의 메뉴에서 제어됩니다. 적용 시 표시 문제가 생길 수 있습니다:");
        public static string ObjectConflictMore(int count) => T(
            $"… 另有 {count} 个物体存在冲突，请返回检查配置。",
            $"… 他 {count} 件の競合があります。設定を確認してください。",
            $"… and {count} more conflict(s). Review your setup.",
            $"… 외 {count}개의 충돌이 더 있습니다. 설정을 확인하세요.");
        public static string ApplyAnyway => T("仍要应用", "このまま適用", "Apply anyway", "그래도 적용");
        public static string GoBackEdit => T("返回更改", "戻って修正", "Go back", "돌아가서 수정");

        public static string AnimClipConflictTitle => T("动画片段名称冲突", "アニメクリップ名の競合", "Animation clip name conflict", "애니메이션 클립 이름 충돌");
        public static string AnimLayerConflictTitle => T("动画层名称冲突", "アニメレイヤー名の競合", "Animator layer name conflict", "애니메이터 레이어 이름 충돌");
        public static string DuplicateClipNamesError => T(
            "插件配置中存在重复的动画片段名称，请修改衣服/配饰/子菜单名称后再应用：\n\n",
            "設定内に重複するアニメクリップ名があります。名前を変更してから適用してください：\n\n",
            "Duplicate animation clip names in your setup. Rename items before applying:\n\n",
            "설정에 중복된 애니메이션 클립 이름이 있습니다. 이름을 수정한 뒤 적용하세요：\n\n");
        public static string DuplicateFileNamesError => T(
            "动画片段保存为资源文件时文件名重复（可能含非法字符被替换后重名），请修改名称后再应用：\n\n",
            "アニメクリップのファイル名が重複しています（不正文字の置換による可能性あり）。名前を変更してください：\n\n",
            "Duplicate animation clip file names (often after sanitizing invalid characters). Rename before applying:\n\n",
            "애니메이션 클립 파일 이름이 중복됩니다（잘못된 문자 치환 후 중복 가능）. 이름을 수정하세요：\n\n");
        public static string DuplicateLayerNamesError => T(
            "插件配置中存在重复的动画层名称，请修改衣服/配饰/子菜单名称后再应用：\n\n",
            "設定内に重複するアニメレイヤー名があります。名前を変更してから適用してください：\n\n",
            "Duplicate animator layer names in your setup. Rename items before applying:\n\n",
            "설정에 중복된 애니메이터 레이어 이름이 있습니다. 이름을 수정한 뒤 적용하세요：\n\n");
        public static string FxLayerExistsError => T(
            "FX 控制器中已存在同名动画层（且不是本插件上次生成的层），请重命名后再应用：\n\n",
            "FX コントローラーに同名レイヤーが既にあります（本プラグインの以前の生成層以外）。名前を変更してください：\n\n",
            "The FX controller already has layer(s) with these names (not from a previous apply). Rename before applying:\n\n",
            "FX 컨트롤러에 동일 이름 레이어가 이미 있습니다（이전 적용분 제외）. 이름을 변경하세요：\n\n");

        public static string ErrorTitle => T("错误", "エラー", "Error", "오류");
        public static string PlayableLayersError => T(
            "发生意料之外的情况，请重新设置 AvatarDescriptor 中的 Playable Layers 后再试。",
            "予期しないエラーです。AvatarDescriptor の Playable Layers を設定し直してください。",
            "Something went wrong. Reconfigure Playable Layers on the AvatarDescriptor and try again.",
            "예기치 않은 오류입니다. AvatarDescriptor의 Playable Layers를 다시 설정하세요.");
        public static string ApplySuccess => T(
            "配置成功，已应用到 FX / 参数 / 菜单。",
            "適用が完了しました（FX・パラメータ・メニュー）。",
            "Applied successfully to FX, parameters, and menus.",
            "FX / 파라미터 / 메뉴에 적용되었습니다.");

        public static string SdkRequired => T(
            "未找到 VRChat Avatar SDK 3，请先安装后再使用本插件。",
            "VRChat Avatar SDK 3 が見つかりません。インストールしてからご利用ください。",
            "VRChat Avatar SDK 3 was not found. Install it before using this tool.",
            "VRChat Avatar SDK 3을 찾을 수 없습니다. 설치 후 사용하세요.");
    }
}
#endif
