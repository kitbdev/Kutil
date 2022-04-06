using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// Manages Menu Screens
    /// forces menu screens to only have one active at a time.
    /// like a toggle group
    /// </summary>
    [DefaultExecutionOrder(1)]// after menu screens
    public class MenuScreenGroup : MonoBehaviour {

        [SerializeField] bool hideAllOnStart = false;
        // allow multiple option (for just management)
        [SerializeField] private bool _allowMultipleShown = false;

        List<MenuScreen> menuScreens = new List<MenuScreen>();

        public IEnumerable<MenuScreen> shownMenuScreen => menuScreens.Where(ms => ms.isShown);
        public IEnumerable<MenuScreen> allMenuScreens => menuScreens;

        public bool allowMultipleShown {
            get => _allowMultipleShown;
            set {
                _allowMultipleShown = value;
                if (!_allowMultipleShown) {
                    // hide all but first
                    HideAllScreensExcept(menuScreens.First(ms => ms.isShown));
                }
            }
        }

        private void Start() {
            if (hideAllOnStart) {
                HideAllScreens();
            }
        }

        public bool AnyScreensShown() {
            return menuScreens.Any(ms => ms.isShown);
        }
        public void ShowMenuScreen(MenuScreen menuScreen) {
            if (!allowMultipleShown) {
                HideAllScreensExcept(menuScreen);
            }
            if (!menuScreen.isShown) {
                menuScreen.SetShown(true, false);
            }
        }
        public void HideMenuScreen(MenuScreen menuScreen) {
            if (menuScreen.isShown) {
                menuScreen.SetShown(false, false);
            }
        }
        public void HideAllScreens() {
            foreach (var ms in menuScreens) {
                ms.SetShown(false);
            }
        }
        void HideAllScreensExcept(params MenuScreen[] menuScreen) {
            foreach (var ms in menuScreens.Except(menuScreen)) {
                ms.SetShown(false);
            }
        }

        // for MenuScreen

        public void NotifyMenuScreenOn(MenuScreen menuScreen) {
            if (!allowMultipleShown) {
                HideAllScreensExcept(menuScreen);
            }
        }
        public void RegisterMenuScreen(MenuScreen menuScreen) {
            if (menuScreens.Contains(menuScreen)) {
                Debug.LogWarning($"Menu Screen {menuScreen} cannot be registered, it is already registered");
                return;
            }
            bool anyShown = AnyScreensShown();
            menuScreens.Add(menuScreen);
            if (!allowMultipleShown) {
                if (anyShown) {
                    // new one does not get priority
                    HideMenuScreen(menuScreen);
                }
            }
        }
        public void UnRegisterMenuScreen(MenuScreen menuScreen) {
            if (!menuScreens.Contains(menuScreen)) {
                Debug.LogWarning($"Menu Screen {menuScreen} cannot be unregistered, it is not registered");
                return;
            }
            menuScreens.Remove(menuScreen);
        }
    }
}