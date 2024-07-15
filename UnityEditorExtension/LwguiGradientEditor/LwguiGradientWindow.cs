// Copyright (c) Jason Ma

using System;
using UnityEngine;
using UnityEditor;
using LWGUI.Runtime.LwguiGradient;

namespace LWGUI.LwguiGradientEditor
{
    internal class PresetLibraryLwguiGradientEditor : PresetLibraryEditor<LwguiGradientPresetLibrary>
    {
        public PresetLibraryLwguiGradientEditor(ScriptableObjectSaveLoadHelper<LwguiGradientPresetLibrary> helper,
            PresetLibraryEditorState state,
            Action<int, object> itemClickedCallback
        ) : base(helper, state, itemClickedCallback)
        {}

        public ColorSpace colorSpace { get; set; }
        public LwguiGradient.ChannelMask viewChannelMask { get; set; }

        protected override void DrawPreset(PresetLibrary lib, Rect rect, object presetObject)
        {
            ((LwguiGradientPresetLibrary)lib).Draw(rect, presetObject as LwguiGradient, colorSpace, viewChannelMask);
        }
    }

    public class LwguiGradientWindow : EditorWindow
    {
        #region Fields
        
        private static LwguiGradientWindow _lwguiGradientWindow;
        public const string presetsEditorPrefID = "LwguiGradient";

        private LwguiGradientEditor _lwguiGradientEditor;
        private PresetLibraryLwguiGradientEditor _lwguiGradientLibraryEditor;
        [SerializeField] private PresetLibraryEditorState m_GradientLibraryEditorState;
        
        [NonSerialized] public LwguiGradient lwguiGradient;
        [NonSerialized] public ColorSpace colorSpace;
        [NonSerialized] public LwguiGradient.ChannelMask viewChannelMask;
        [NonSerialized] public LwguiGradient.GradientTimeRange gradientTimeRange;
        
        private GUIView _viewToUpdate;
        private Action<LwguiGradient> _onChange;
        private bool _changed { get; set; }

        #endregion

        #region GUI Layout

        private static readonly Vector2 _minWindowSize = new (750, 500);
        private static readonly float _presetLibraryHeight = 100;
        private Rect _gradientEditorRect => new Rect(0, 0, position.width, position.height - _presetLibraryHeight);
        private Rect _presetLibraryRect => new Rect(0, position.height - _presetLibraryHeight, position.width, _presetLibraryHeight);

        #endregion
        
        public static LwguiGradientWindow instance
        {
            get
            {
                if (!_lwguiGradientWindow)
                    Debug.LogError("Lwgui Gradient Window not initalized, did you call Show first?");
                return _lwguiGradientWindow;
            }
        }
        
        public string currentPresetLibrary
        {
            get
            {
                Init(false);
                return _lwguiGradientLibraryEditor.currentLibraryWithoutExtension;
            }
            set
            {
                Init(false);
                _lwguiGradientLibraryEditor.currentLibraryWithoutExtension = value;
            }
        }
        
        public static bool visible => _lwguiGradientWindow != null;

        private void Init(bool force = true, bool forceRecreate = false)
        {
            if (_lwguiGradientEditor == null || force || forceRecreate)
            {
                if (_lwguiGradientEditor == null || forceRecreate)
                {
                    _lwguiGradientEditor = new LwguiGradientEditor();
                }
                _lwguiGradientEditor.Init(_gradientEditorRect, lwguiGradient, colorSpace, viewChannelMask, gradientTimeRange, _onChange);
            }
            
            if (m_GradientLibraryEditorState == null || forceRecreate)
            {
                m_GradientLibraryEditorState = new PresetLibraryEditorState(presetsEditorPrefID);
                m_GradientLibraryEditorState.TransferEditorPrefsState(true);
            }
            
            if (_lwguiGradientLibraryEditor == null || force || forceRecreate)
            {
                if (_lwguiGradientLibraryEditor == null || forceRecreate)
                {
                    var saveLoadHelper = new ScriptableObjectSaveLoadHelper<LwguiGradientPresetLibrary>("lwguigradients", SaveType.Text);
                    _lwguiGradientLibraryEditor = new PresetLibraryLwguiGradientEditor(saveLoadHelper, m_GradientLibraryEditorState, PresetClickedCallback);
                }
                _lwguiGradientLibraryEditor.showHeader = true;
                _lwguiGradientLibraryEditor.colorSpace = colorSpace;
                _lwguiGradientLibraryEditor.viewChannelMask = viewChannelMask;
                _lwguiGradientLibraryEditor.minMaxPreviewHeight = new Vector2(14f, 14f);
            }

        }

        public static void SetCurrentGradient(LwguiGradient lwguiGradient)
        {
            if (_lwguiGradientWindow == null)
                return;

            _lwguiGradientWindow.lwguiGradient = lwguiGradient;
            _lwguiGradientWindow.Init();
            GUI.changed = true;
        }
        
        private static LwguiGradientWindow GetWindow(bool focus = true) => (LwguiGradientWindow)GetWindow(typeof(LwguiGradientWindow), true, "LWGUI Gradient Editor", focus);

        internal static void Show(LwguiGradient gradient, ColorSpace colorSpace = ColorSpace.Gamma, LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All, LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One, GUIView viewToUpdate = null, Action<LwguiGradient> onChange = null)
        {
            if (_lwguiGradientWindow == null)
            {
                _lwguiGradientWindow = GetWindow();
                _lwguiGradientWindow.minSize = _minWindowSize;
                Undo.undoRedoEvent += _lwguiGradientWindow.OnUndoPerformed;
            }
            else
            {
                _lwguiGradientWindow = GetWindow();
            }
            
            _lwguiGradientWindow.lwguiGradient = gradient;
            _lwguiGradientWindow.colorSpace = colorSpace;
            _lwguiGradientWindow.viewChannelMask = viewChannelMask;
            _lwguiGradientWindow.gradientTimeRange = timeRange;
            _lwguiGradientWindow._viewToUpdate = viewToUpdate;
            _lwguiGradientWindow._onChange = onChange;

            _lwguiGradientWindow.Init();
            _lwguiGradientWindow.Show();
            // window.ShowAuxWindow();
        }

        public static void CloseWindow()
        {
            if (_lwguiGradientWindow == null)
                return;

            _lwguiGradientWindow.UnregisterEvents();
            _lwguiGradientWindow.Close();
            // GUIUtility.ExitGUI();
        }
        
        public static void RepaintWindow()
        {
            if (_lwguiGradientWindow == null)
                return;
            _lwguiGradientWindow.Repaint();
        }


        private void OnGUI()
        {
            if (lwguiGradient == null) 
                return;
            
            Init(false);
            
            // Separator
            EditorGUI.DrawRect(new Rect(_presetLibraryRect.x, _presetLibraryRect.y - 1, _presetLibraryRect.width, 1), new Color(0, 0, 0, 0.3f));
            EditorGUI.DrawRect(new Rect(_presetLibraryRect.x, _presetLibraryRect.y, _presetLibraryRect.width, 1), new Color(1, 1, 1, 0.1f));

            
            EditorGUI.BeginChangeCheck();
            _lwguiGradientEditor.OnGUI(_gradientEditorRect);
            if (EditorGUI.EndChangeCheck())
                _changed = true;
            _lwguiGradientLibraryEditor.OnGUI(_presetLibraryRect, lwguiGradient);
            if (_changed)
            {
                _changed = false;
                SendEvent(true);
            }
        }

        public const string LwguiGradientChangedCommand = "LwguiGradientChanged";

        void SendEvent(bool exitGUI)
        {
            if (_viewToUpdate != null)
            {
                Event e = EditorGUIUtility.CommandEvent(LwguiGradientChangedCommand);
                Repaint();
                _viewToUpdate.SendEvent(e);
                if (exitGUI)
                    GUIUtility.ExitGUI();
            }
            if (_onChange != null)
            {
                _onChange(lwguiGradient);
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += LwguiGradientEditor.CheckAddGradientKeyFailureLog;
            hideFlags = HideFlags.DontSave;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= LwguiGradientEditor.CheckAddGradientKeyFailureLog;

            m_GradientLibraryEditorState?.TransferEditorPrefsState(false);

            UnregisterEvents();
            Clear();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            _lwguiGradientLibraryEditor.UnloadUsedLibraries();

            Clear();
        }

        private void Clear()
        {
            _lwguiGradientEditor = null;
            _lwguiGradientWindow = null;
        }

        private void UnregisterEvents()
        {
            Undo.undoRedoEvent -= OnUndoPerformed;
        }

        private void OnUndoPerformed(in UndoRedoInfo info)
        {
            Init();
        }

        void OnPlayModeStateChanged()
        {
            Close();
        }
        
        void PresetClickedCallback(int clickCount, object presetObject)
        {
            LwguiGradient gradient = presetObject as LwguiGradient;
            if (gradient == null)
                Debug.LogError("Incorrect object passed " + presetObject);

            SetCurrentGradient(gradient);
            // UnityEditorInternal.GradientPreviewCache.ClearCache();
            _changed = true;
        }
    }
}