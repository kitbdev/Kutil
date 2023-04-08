using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace Kutil.Editor.PropertyDrawers {
    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(ArrayOptionsAttribute))]
    public class ArrayOptionsDrawer : DecoratorDrawer {

        public static readonly string arrayOptionsClass = "kutil-array-options";
        VisualElement decorator;
        PropertyField propertyField;
        ListView listView;

        // todo extended decorator with access to stuff property drawer has, if wanted

        ArrayOptionsAttribute arrayOptions => (ArrayOptionsAttribute)attribute;
        public override VisualElement CreatePropertyGUI() {
            decorator = new VisualElement();
            decorator.AddToClassList(arrayOptionsClass);
            decorator.RegisterCallback<GeometryChangedEvent>(OnSetup);
            return decorator;
        }

        private void OnSetup(GeometryChangedEvent evt) {
            decorator.UnregisterCallback<GeometryChangedEvent>(OnSetup);

            propertyField = decorator.GetFirstAncestorOfType<PropertyField>();

            listView = propertyField.Q<ListView>();
            if (listView == null) {
                return;
            }
            // list items arent populated yet, so wait for the lists next geochange
            listView.RegisterCallback<GeometryChangedEvent>(OnSetupList);
        }
        private void OnSetupList(GeometryChangedEvent evt) {
            listView.UnregisterCallback<GeometryChangedEvent>(OnSetupList);

            // todo disable foldout and content options

            // todo? add remove validation functions?

            // listView.reorderable = false;
            if (arrayOptions.CanAddAndRemove()) {
                return;
            }

            if (arrayOptions.CannotAddAndRemove()) {
                listView.showAddRemoveFooter = false;
            } else {
                var listFooter = listView.Query(null, ListView.footerUssClassName).Last();
                if (listFooter == null) {
                    Debug.LogError($"{GetType().Name} {propertyField.name} listview {listView.name} has no listfooter!");
                    return;
                }
                // Debug.Log(listFooter);
                // disable individually
                string removeButtonName = $"{ListView.ussClassName}__remove-button";
                string addButtonName = $"{ListView.ussClassName}__add-button";
                if (arrayOptions.CanAddOnly()) {
                    var listFooterBtn = listFooter.Query<Button>(removeButtonName).Last();
                    if (listFooterBtn == null) {
                        Debug.LogError($"{GetType().Name} {propertyField.name} listview {listView.name} has no listfooterBtn! {ListView.ussClassName}");
                        return;
                    }
                    listFooterBtn.SetEnabled(false);
                }
                if (arrayOptions.CanRemoveOnly()) {
                    var listFooterBtn = listFooter.Query<Button>(addButtonName).Last();
                    if (listFooterBtn == null) {
                        Debug.LogError($"{GetType().Name} {propertyField.name} listview {listView.name} has no listfooterBtn! '{addButtonName}'");
                        return;
                    }
                    listFooterBtn.SetEnabled(false);
                }
            }
            var listSize = listView.Query(null, ListView.arraySizeFieldUssClassName).Last();
            if (listSize == null) {
                Debug.LogError($"{GetType().Name} {propertyField.name} listview {listView.name} has no listsizefield!");
                return;
            }
            listSize.SetEnabled(false);

            // override context menu
            // var listSource = listView.itemsSource;
            // if (listSource == null) return;
            // for (int i = 0; i < listSource.Count; i++) {
            //     VisualElement itemElement = listView.GetRootElementForIndex(i);
            //     // Debug.Log($"context menu for {itemElement?.name ?? "null"} {i}/{listSource.Count}");
            //     if (itemElement == null) continue;
            //     // per list item
            //     PropertyField lip = itemElement.Q<PropertyField>();
            //     ContextualMenuManipulator m = new(ContextMenuOverride);
            //     lip.AddManipulator(m);
            //     // itemElement.AddManipulator(m);
            //     // itemElement.RegisterCallback<ContextualMenuPopulateEvent>(ContextMenuOverride);
            // }

            // todo override paste?

            // todo add option to override drag and drop

        }
        void ContextMenuOverride(ContextualMenuPopulateEvent e) {
            //? how to append instead?
            // ! dont want to override the menu, cause useful stuff is added, like prefab handling and copy paste


            // IEventHandler newTarg = e.currentTarget;
            // if (e.currentTarget is VisualElement ve) {
            //     PropertyField lip = ve.Q<PropertyField>();
            //     if (lip != null) {
            //         newTarg = lip;
            //     }
            // }
            // if (propertyField.panel?.contextualMenuManager == null) {
            //     Debug.LogError("no cmm!");
            //     return;
            // }
            // propertyField.panel?.contextualMenuManager?.DisplayMenuIfEventMatches(e, e.currentTarget);
            // Debug.Log(propertyField.panel?.contextualMenuManager + "cmm t" + e.target + " c" + e.currentTarget + " " + newTarg);
            // using (ContextualMenuPopulateEvent cme = ContextualMenuPopulateEvent.GetPooled(e, e.menu, newTarg, propertyField.panel.contextualMenuManager)) {
            // this causes some inf loop and crashes :(
            //     newTarg?.SendEvent(cme);
            // }
            // e.PreventDefault();
            // e.menu.PrepareForDisplay(e);
            // empty!
            // List<DropdownMenuItem> dropdownMenuItems = e.menu.MenuItems();
            // Debug.Log("cmenu " + dropdownMenuItems.ToStringFull(null, true));

            // e.menu.AppendAction("hi", a => { }, DropdownMenuAction.AlwaysEnabled);
            e.menu.AppendSeparator();

            // DropdownMenu
        }
        void MenuUpdated(ContextualMenuPopulateEvent ce) {

        }
    }
}