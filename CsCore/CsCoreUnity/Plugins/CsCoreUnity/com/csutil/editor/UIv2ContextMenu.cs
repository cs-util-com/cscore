using System;
using com.csutil.ui;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.editor {

    public static class UIv2ContextMenu {

        /// <summary> The priority value of the default Unity GameObject/UI menu </summary>
        private const int priorityOfUiMenu = 6;
        private const string UIv2menu = "GameObject/UI v2/";
        private const string UIv2menuOld = UIv2menu + "Old/";

        [MenuItem(UIv2menu + "Root Canvas", false, priorityOfUiMenu + 1)]
        static void AddRootCanvas() {
            var foundCanvas = RootCanvas.GetAllRootCanvases().FirstOrDefault();
            if (foundCanvas != null) {
                Log.e("There is already a root canvas in the scene");
                SelectInHirarchyUi(foundCanvas);
                return;
            }
            AddViewInViewStack();
        }

        [MenuItem(UIv2menu + "Views/ViewStack View", false, priorityOfUiMenu + 2)]
        static void AddViewInViewStack() {
            var view = AddViewToMainViewStack(() =>ResourcesV2.LoadPrefab("Canvas/DefaultViewStackView"));
            view.name = "View " + (view.transform.GetSiblingIndex() + 1);
        }

        [MenuItem(UIv2menu + "Views/DefaultSideBar", false, priorityOfUiMenu + 2)]
        static void DefaultSideBar() { AddViewToMainViewStack(() => ResourcesV2.LoadPrefab("DefaultSideBar")); }

        [MenuItem(UIv2menu + "DefaultScrollView", false, priorityOfUiMenu + 3)]
        static void DefaultScrollView() { AddPrefabToActiveView("DefaultScrollView"); }

        [MenuItem(UIv2menu + "DefaultActionBar", false, priorityOfUiMenu + 3)]
        static void DefaultActionBar() {
            var actionBar = AddPrefabToActiveView("DefaultActionBar");
            var viewName = actionBar.GetViewStack()?.GetRootViewOf(actionBar).name;
            actionBar.GetLinkMap().Get<Text>("Caption").text = viewName;
        }

        [MenuItem(UIv2menu + "DefaultCard", false, priorityOfUiMenu + 3)]
        static void DefaultCard() { AddPrefabToActiveView("DefaultCard"); }

        [MenuItem(UIv2menu + "DefaultPanel", false, priorityOfUiMenu + 3)]
        static void DefaultPanel() { AddPrefabToActiveView("DefaultPanel"); }

        [MenuItem(UIv2menu + "DefaultButton", false, priorityOfUiMenu + 4)]
        static void DefaultButton() { AddPrefabToActiveView("DefaultButton"); }

        [MenuItem(UIv2menu + "More Buttons/DefaultIconButton", false, priorityOfUiMenu + 5)]
        static void DefaultIconButton() { AddPrefabToActiveView("DefaultIconButton"); }

        [MenuItem(UIv2menu + "More Buttons/DefaultButtonWithIcon", false, priorityOfUiMenu + 5)]
        static void DefaultButtonWithIcon() { AddPrefabToActiveView("DefaultButtonWithIcon"); }

        [MenuItem(UIv2menu + "More Buttons/DefaultIconButtonWithText", false, priorityOfUiMenu + 5)]
        static void DefaultIconButtonWithText() { AddPrefabToActiveView("DefaultIconButtonWithText"); }

        [MenuItem(UIv2menu + "More Buttons/ButtonNoBackground", false, priorityOfUiMenu + 5)]
        static void ButtonNoBackground() { AddPrefabToActiveView("ButtonNoBackground"); }

        [MenuItem(UIv2menu + "More Buttons/TwoHorizontalButtons", false, priorityOfUiMenu + 5)]
        static void TwoHorizontalButtons() { AddPrefabToActiveView("TwoHorizontalButtons"); }

        [MenuItem(UIv2menu + "More Buttons/DefaultFloatingActionButton", false, priorityOfUiMenu + 5)]
        static void DefaultFloatingActionButton() { AddPrefabToActiveView("DefaultFloatingActionButton"); }

        [MenuItem(UIv2menu + "DefaultToggle", false, priorityOfUiMenu + 6)]
        static void DefaultToggle() { AddPrefabToActiveView("DefaultToggle"); }

        [MenuItem(UIv2menu + "DefaultRadioButton", false, priorityOfUiMenu + 6)]
        static void DefaultRadioButton() { AddPrefabToActiveView("DefaultRadioButton"); }

        [MenuItem(UIv2menuOld + "DefaultInput", false, priorityOfUiMenu + 6)]
        static void DefaultInput() { AddPrefabToActiveView("DefaultInput"); }

        [MenuItem(UIv2menu + "DefaultInputV2", false, priorityOfUiMenu + 6)]
        static void DefaultInputV2() { AddPrefabToActiveView("DefaultInputV2"); }

        [MenuItem(UIv2menu + "DefaultDropDown", false, priorityOfUiMenu + 6)]
        static void DefaultDropDown() { AddPrefabToActiveView("DefaultDropDown"); }

        [MenuItem(UIv2menu + "DefaultImage", false, priorityOfUiMenu + 6)]
        static void DefaultImage() { AddPrefabToActiveView("DefaultImage"); }

        [MenuItem(UIv2menu + "DefaultSlider", false, priorityOfUiMenu + 6)]
        static void DefaultSlider() { AddPrefabToActiveView("DefaultSlider"); }

        [MenuItem(UIv2menu + "DefaultSliderWithText", false, priorityOfUiMenu + 6)]
        static void DefaultSliderWithText() { AddPrefabToActiveView("DefaultSliderWithText"); }

        [MenuItem(UIv2menu + "DefaultProgressBar", false, priorityOfUiMenu + 6)]
        static void DefaultProgressBar() { AddPrefabToActiveView("DefaultProgressBar"); }

        [MenuItem(UIv2menu + "DefaultProgressBarWithText", false, priorityOfUiMenu + 6)]
        static void DefaultProgressBarWithText() { AddPrefabToActiveView("DefaultProgressBarWithText"); }

        [MenuItem(UIv2menu + "Messages/Tooltip", false, priorityOfUiMenu + 7)]
        static void Tooltip() { AddPrefabToActiveView("Messages/Tooltip"); }

        [MenuItem(UIv2menu + "Menus/DefaultMenu1", false, priorityOfUiMenu + 8)]
        static void DefaultMenu1() { AddPrefabToActiveView("Menus/DefaultMenu1"); }

        [MenuItem(UIv2menu + "Menus/DefaultMenu1 Example", false, priorityOfUiMenu + 8)]
        static void DefaultMenu1Example() { AddPrefabToActiveView("Menus/DefaultMenu1 Example", false); }

        [MenuItem(UIv2menu + "Menus/ButtonGroup Example 1", false, priorityOfUiMenu + 8)]
        static void ButtonGroup1() { AddPrefabToActiveView("ButtonGroupTemplates/ButtonGroup1", false); }

        [MenuItem(UIv2menu + "Menus/ButtonGroup Example 2", false, priorityOfUiMenu + 8)]
        static void DefaultMenu2() { AddPrefabToActiveView("ButtonGroupTemplates/ButtonGroup2", false); }

        [MenuItem(UIv2menu + "DefaultVerticalGroupWithHorizontalChildren", false, priorityOfUiMenu + 8)]
        static void DefaultVerticalGroupWithHorizontalChildren() { AddPrefabToActiveView("DefaultVerticalGroupWithHorizontalChildren", false); }

        [MenuItem(UIv2menu + "Add UI Save Area Resizer", false, priorityOfUiMenu + 9)]
        static void AddUiSaveAreaResizer() {
            if (!GetSelectedCanvasChild().HasComponent<ViewStack>(out var _)) {
                throw new InvalidOperationException($"The {nameof(SafeAreaResizer)} should be typically added on the level of the ViewStack. "
                    + $"But if needed you can add it manually on any other UI level as well. Dont add it on the root canvas level though!");
            }
            GetSelectedCanvasChild().AddComponent<SafeAreaResizer>();
        }

        private static GameObject AddPrefabToActiveView(string uiPrefabName, bool keepReferenceToEditorPrefab = true) {
            var go = GetSelectedCanvasChild().AddChild(ResourcesV2.LoadPrefab(uiPrefabName, keepReferenceToEditorPrefab));
            SelectInHirarchyUi(go);
            return go;
        }

        private static GameObject AddViewToMainViewStack(Func<GameObject> viewInViewStackCreator) {
            GameObject mainViewStack = ViewStackHelper.MainViewStack().gameObject;
            GameObject viewInViewStack = viewInViewStackCreator();
            mainViewStack.AddChild(viewInViewStack);
            viewInViewStack.name += " " + mainViewStack.GetChildCount();
            viewInViewStack.GetOrAddComponent<RectTransform>().SetAnchorsStretchStretch();
            SelectInHirarchyUi(viewInViewStack);
            return viewInViewStack;
        }

        /// <summary> Returns the current selected element in a canvas or a view in a view stack </summary>
        private static GameObject GetSelectedCanvasChild() {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo?.GetComponentV2<RectTransform>() != null) { return selectedGo; }
            if (GetLastActiveView() == null) { AddViewInViewStack(); }
            return GetLastActiveView();
        }

        private static GameObject GetLastActiveView() { return GetAllActiveViewsInMainViewStack().LastOrDefault(); }

        private static IEnumerable<GameObject> GetAllActiveViewsInMainViewStack() {
            return ViewStackHelper.MainViewStack().gameObject.GetChildrenIEnumerable().Filter(v => v.activeInHierarchy);
        }

        private static void SelectInHirarchyUi(UnityEngine.Object objectToSelect) {
            if (objectToSelect is GameObject go) { Selection.activeGameObject = go; }
            if (objectToSelect is Component c) { Selection.activeGameObject = c.gameObject; }
        }

    }

}