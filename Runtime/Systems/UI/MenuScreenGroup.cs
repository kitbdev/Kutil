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

        [Tooltip("Hide all MenuScreens on start")]
        [SerializeField] protected bool hideAllOnStart = false;
        /// <summary>
        /// Can multiple MenuScreens be shown at the same time
        /// </summary>
        [Tooltip("Can multiple MenuScreens be shown at the same time")]
        [SerializeField] protected bool _allowMultipleShown = false;
        /// <summary>
        /// Should a history of MenuScreens shown be recorded
        /// </summary>
        [Tooltip("Should a history of MenuScreens shown be recorded")]
        [SerializeField] protected bool _useHistory = true;
        // todo option to clear history on scene load or something


        [SerializeField]
        protected bool debug = false;


        // all menu screens
        // [SerializeField]
        // [ConditionalHide(nameof(debug), true)]
        protected List<MenuScreen> menuScreens = new List<MenuScreen>();
        // does not include current
        [SerializeField]
        [ConditionalHide(nameof(debug), true)]
        protected SerializableStack<ScreenSet> menuScreenHistory = new SerializableStack<ScreenSet>();

        /// <summary>
        /// All MenuScreens that are currently shown
        /// </summary>
        public IEnumerable<MenuScreen> shownMenuScreens => menuScreens.Where(ms => ms.isShown);
        /// <summary>
        /// All MenuScreens registered with this MenuScreenGroup
        /// </summary>
        public IEnumerable<MenuScreen> allMenuScreens => menuScreens;

        // todo subgroups? (for unified history, ?)

        [System.Serializable]
        protected struct ScreenSet : System.IEquatable<ScreenSet>, IEnumerable<MenuScreen> {
            [SerializeReference]
            public MenuScreen[] menuScreens;

            public bool Equals(ScreenSet other) {
                return menuScreens == other.menuScreens;
            }
            public IEnumerator<MenuScreen> GetEnumerator() {
                return menuScreens.AsEnumerable().GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return menuScreens.GetEnumerator();
            }

            public static implicit operator MenuScreen[](ScreenSet s) => s.menuScreens;
            public static implicit operator ScreenSet(MenuScreen[] s) => new ScreenSet() { menuScreens = s };
            // public static implicit operator IEnumerable<MenuScreen>(ScreenSet s) => s.menuScreens;
            // public static implicit operator ScreenSet(IEnumerable<MenuScreen> s) => new ScreenSet() { menuScreens = s.ToArray() };
        }

        public bool allowMultipleShown {
            get => _allowMultipleShown;
            set {
                _allowMultipleShown = value;
                if (!_allowMultipleShown && menuScreens != null) {
                    // hide all but first
                    HideAllMenuScreensExcept(menuScreens.FirstOrDefault(ms => ms.isShown));
                }
            }
        }

        private void OnValidate() {
            if (Application.isPlaying) {
                if (!_allowMultipleShown && menuScreens != null) {
                    HideAllMenuScreensExcept(menuScreens.FirstOrDefault(ms => ms.isShown), false);
                }
            }
        }

        private void Start() {
            if (hideAllOnStart) {
                HideAllMenuScreens();
            }
            UpdateHistory();
        }
        /// <summary>
        /// Are any MenuScreens currently shown?
        /// </summary>
        /// <returns></returns>
        public bool AnyMenuScreensShown() {
            return menuScreens.Any(ms => ms.isShown);
        }
        /// <summary>
        /// Show some MenuScreens additively. Will not hide other screens.
        /// Allow Multiple Shown must be true if there is more than one.
        /// </summary>
        /// <param name="showMenuScreens">The MenuScreens to Show</param>
        public void ShowMenuScreensAdditively(IEnumerable<MenuScreen> showMenuScreens) {
            ShowMenuScreens(showMenuScreens, false);
        }
        /// <summary>
        /// Show some MenuScreens exclusively. Will hide other screens.
        /// Allow Multiple Shown must be true if there is more than one.
        /// </summary>
        /// <param name="showMenuScreens">The MenuScreens to Show</param>
        public void ShowMenuScreensExclusively(IEnumerable<MenuScreen> showMenuScreens) {
            ShowMenuScreens(showMenuScreens, true);
        }
        /// <summary>
        /// Show some MenuScreens.
        /// Allow Multiple Shown must be true if there is more than one.
        /// </summary>
        /// <param name="showMenuScreens">The MenuScreens to Show</param>
        /// <param name="exclusively">if allowMultiple, show show exlusively or additively?</param>
        public void ShowMenuScreens(IEnumerable<MenuScreen> showMenuScreens, bool exclusively) {
            int numSceensToShow = showMenuScreens.Count();
            if (showMenuScreens == null || numSceensToShow == 0) {
                HideAllMenuScreens();
                return;
            }
            if (numSceensToShow == 1) {
                ShowMenuScreen(showMenuScreens.First(), exclusively);
                return;
            }
            if (!allowMultipleShown && numSceensToShow > 1) {
                Debug.LogWarning($"Trying to show {numSceensToShow} MenuScreens but multiple MenuScreens are not allowed!", this);
                return;
            }
            if (!allowMultipleShown || exclusively) {
                HideAllMenuScreensNoHistory();
            }
            foreach (var ms in showMenuScreens) {
                ms.SetShown(true, false);
            }
            UpdateHistory();
        }
        /// <summary>
        /// Show a MenuScreen.
        /// if allowMultiple, show additively. Will not hide other screens.
        /// </summary>
        /// <param name="menuScreen">The MenuScreen to Show</param>
        public void ShowMenuScreen(MenuScreen menuScreen) {
            ShowMenuScreen(menuScreen, false);
        }
        /// <summary>
        /// Show a MenuScreen.
        /// if allowMultiple, show exlusively. Will hide other screens.
        /// </summary>
        /// <param name="menuScreen">The MenuScreen to Show</param>
        public void ShowMenuScreenExlusively(MenuScreen menuScreen) {
            ShowMenuScreen(menuScreen, true);
        }
        /// <summary>
        /// Show a MenuScreen.
        /// </summary>
        /// <param name="menuScreen">The MenuScreen to Show</param>
        /// <param name="exclusively">if allowMultiple, show show exlusively or additively?</param>
        public void ShowMenuScreen(MenuScreen menuScreen, bool exclusively) {
            if (debug) Debug.Log($"showing {menuScreen.name} exl:{exclusively} curshown:{shownMenuScreens.Count()} multallowed:{allowMultipleShown}");
            if (shownMenuScreens.Count() > 0) {
                if (!allowMultipleShown || exclusively) {
                    HideAllMenuScreensExcept(menuScreen, false);
                }
            }
            if (!menuScreen.isShown) {
                menuScreen.SetShown(true, false);
                UpdateHistory();
            }
        }
        // public void HideMenuScreens(IEnumerable<MenuScreen> hideMenuScreens) {
        // }
        /// <summary>
        /// Hide a MenuScreen.
        /// </summary>
        /// <param name="menuScreen">The MenuScreen to hide</param>
        public void HideMenuScreen(MenuScreen menuScreen) {
            if (menuScreen.isShown) {
                menuScreen.SetShown(false, false);
                UpdateHistory();
            }
        }
        public void HideAllMenuScreens() {
            HideAllMenuScreensNoHistory();
            UpdateHistory();
        }
        protected void HideAllMenuScreensNoHistory() {
            foreach (var ms in menuScreens) {
                ms.SetShown(false, false);
            }
        }
        protected void HideAllMenuScreensExcept(MenuScreen menuScreen, bool updateHistory = true) {
            HideAllMenuScreensExcept(new MenuScreen[] { menuScreen }, updateHistory);
        }
        protected void HideAllMenuScreensExcept(IEnumerable<MenuScreen> menuScreen, bool updateHistory = true) {
            foreach (var ms in menuScreens.Except(menuScreen)) {
                ms.SetShown(false, false);
            }
            if (updateHistory) {
                UpdateHistory();
            }
        }

        // for ui unityevents
        public void GoBack() => GoBack(1);
        /// <summary>
        /// Go back in history and show a previous set of menu screens.
        /// </summary>
        /// <param name="steps">times to go back in history (default = 1)</param>
        public void GoBack(int steps = 1) {
            if (!_useHistory) return;
            if (debug) Debug.Log("History:" +
                menuScreenHistory.ToStringFull(mss => mss.menuScreens.ToStringFull(ms => ms.name), true, true, ",\n"), this);
            IEnumerable<MenuScreen> showScreens = null;
            // pop the current screens and the prior ones (showing will re add to history the current)
            for (int i = steps; i >= 0; i--) {
                if (menuScreenHistory.TryPop(out var screens)) {
                    showScreens = screens;
                } else {
                    // no more history!
                    Debug.LogWarning($"MenuScreen Group cannot go back {steps} steps, no more history!", this);
                    showScreens = null;
                    // ? return instead?
                    break;
                }
            }
            if (showScreens != null && showScreens.Count() > 0) {
                ShowMenuScreens(showScreens, true);
            } else {
                HideAllMenuScreens();
            }
        }
        // todo go forward?
        // public void GoFoward(int steps = 1){

        // }
        /// <summary>
        /// Can we go back in MenuScreen for the number of steps?
        /// </summary>
        /// <param name="steps"></param>
        /// <returns></returns>
        public bool CanGoBack(int steps = 1) {
            return menuScreenHistory.Count() > steps;
        }
        public int HistoryLength() {
            // not including current
            return menuScreenHistory.Count() - 1;
        }

        /// <summary>
        /// Add the current MenuScreens to the history stack
        /// </summary>
        protected void UpdateHistory() {
            if (!_useHistory) return;
            MenuScreen[] curShownScreens = shownMenuScreens.ToArray();
            if (!menuScreenHistory.TryPeek(out ScreenSet lastScreens) || !lastScreens.menuScreens.SequenceEqual(curShownScreens)) {
                // either no history or the last entry was different
                menuScreenHistory.Push(curShownScreens);
            }
            if (debug) {
                Debug.Log("Updating History:" +
                menuScreenHistory.ToStringFull(mss => mss.ToStringFull(ms => ms.name), true, true, ",\n"), this);
            }
        }
        /// <summary>
        /// Clear MenuScreen history
        /// </summary>
        public void ClearHistory() {
            menuScreenHistory.Clear();
            // add the current screens back
            UpdateHistory();
        }

        // for MenuScreen to call

        public void NotifyMenuScreenState(bool isOn, MenuScreen menuScreen) {
            if (isOn) {
                if (!allowMultipleShown) {
                    HideAllMenuScreensExcept(menuScreen);
                }
            }
            UpdateHistory();
        }

        public void RegisterMenuScreen(MenuScreen menuScreen) {
            if (menuScreens.Contains(menuScreen)) {
                Debug.LogWarning($"Menu Screen {menuScreen} cannot be registered, it is already registered");
                return;
            }
            bool anyShown = AnyMenuScreensShown();
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