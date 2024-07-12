// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;
using LWGUI.Runtime.LwguiGradient;

namespace LWGUI.LwguiGradientEditor
{
    public static class LwguiGradientEditorHelper
    {
        private static readonly int s_LwguiGradientHash = "s_LwguiGradientHash".GetHashCode();
        private static int s_LwguiGradientID;

        public static void GradientField(Rect position, GUIContent label, LwguiGradient gradient, 
            ColorSpace colorSpace = ColorSpace.Gamma, 
            LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All, 
            LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One)
        {
            int id = GUIUtility.GetControlID(s_LwguiGradientHash, FocusType.Keyboard, position);
            var rect = EditorGUI.PrefixLabel(position, id, label);
            var evt = Event.current;

            // internal static Gradient DoGradientField(Rect position, int id, Gradient value, SerializedProperty property, bool hdr, ColorSpace space)
            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (rect.Contains(evt.mousePosition))
                    {
                        if (evt.button == 0)
                        {
                            s_LwguiGradientID = id;
                            GUIUtility.keyboardControl = id;
                            LwguiGradientWindow.ShowWindow(gradient, colorSpace, viewChannelMask, timeRange, GUIView.current);
                            GUIUtility.ExitGUI();
                        }
                        else if (evt.button == 1)
                        {
                            // if (property != null)
                            //     GradientContextMenu.Show(property.Copy());
                            // // TODO: make work for Gradient value
                        }
                    }
                    break;
                case EventType.KeyDown:
                    // if (GUIUtility.keyboardControl == id && (current.keyCode == KeyCode.Space || current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter))
                    // {
                    //     UnityEngine.Event.current.Use();
                    //     GradientPicker.Show(property != null ? property.gradientValue : value, hdr, space);
                    //     GUIUtility.ExitGUI();
                    //     break;
                    // }

                    break;
                case EventType.Repaint:
                    var previewRect = new Rect(rect.x + 2.5f, rect.y + 2.5f, rect.width - 5.0f, rect.height - 5.0f);
                    EditorGUI.DrawPreviewTexture(previewRect, gradient.GetPreviewRampTexture(256, 1, colorSpace, viewChannelMask));
                    var outlineRect = new Rect(rect.x + 0.5f, rect.y + 0.5f, rect.width - 1.0f, rect.height - 1.0f);
                    EditorStyles.colorPickerBox.Draw(outlineRect, GUIContent.none, id);
                    break;
                case EventType.ExecuteCommand:
                    // When drawing the modifying Gradient Field and it has changed
                    if ((GUIUtility.keyboardControl == id || s_LwguiGradientID == id)
                        && evt.commandName == LwguiGradientWindow.onChangeCommandName)
                    {
                        // evt.Use();
                        GUI.changed = true;
                        HandleUtility.Repaint();
                    }
                    break;
            }
        }

        public static bool GradientEditButton(Rect position, GUIContent icon, LwguiGradient gradient,
            ColorSpace colorSpace = ColorSpace.Gamma,
            LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All,
            LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One,
            Func<bool> shouldOpenWindowAfterClickingEvent = null)
        {
            int id = GUIUtility.GetControlID(s_LwguiGradientHash, FocusType.Keyboard, position);
            var evt = Event.current;
            
            // When drawing the modifying Gradient Field and it has changed
            if ((GUIUtility.keyboardControl == id || s_LwguiGradientID == id)
                && evt.GetTypeForControl(id) == EventType.ExecuteCommand 
                && evt.commandName == LwguiGradientWindow.onChangeCommandName)
            {
                GUI.changed = true;
                HandleUtility.Repaint();
            }

            var clicked = ReflectionHelper.GUI_Button(position, id, icon, GUI.skin.button);
            if (clicked)
            {
                if (shouldOpenWindowAfterClickingEvent == null || shouldOpenWindowAfterClickingEvent.Invoke())
                {
                    s_LwguiGradientID = id;
                    GUIUtility.keyboardControl = id;
                    LwguiGradientWindow.ShowWindow(gradient, colorSpace, viewChannelMask, timeRange, GUIView.current);
                    GUIUtility.ExitGUI();
                }
            }
            
            return clicked;
        }
    }
}