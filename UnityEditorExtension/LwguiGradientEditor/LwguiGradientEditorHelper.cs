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

        public static void DrawGradientWithBackground(Rect position, LwguiGradient gradient, ColorSpace colorSpace, LwguiGradient.ChannelMask viewChannelMask)
        {
            Texture2D gradientTexture = gradient.GetPreviewRampTexture(256, 1, colorSpace, viewChannelMask);
            Rect r2 = new Rect(position.x + 1, position.y + 1, position.width - 2, position.height - 2);

            // Background checkers
            Texture2D backgroundTexture = GradientEditor.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, r2.width / backgroundTexture.width, r2.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(r2, backgroundTexture, texCoordsRect, false);

            // Outline for Gradinet Texture, used to be Frame over texture.
            GUI.Box(position, GUIContent.none);

            // Gradient texture
            Color oldColor = GUI.color;
            GUI.color = Color.white;            //Dont want the Playmode tint to be applied to gradient textures.
            if (gradientTexture != null)
                GUI.DrawTexture(r2, gradientTexture, ScaleMode.StretchToFill, true);
            GUI.color = oldColor;

            // HDR label
            // float maxColorComponent = GetMaxColorComponent(gradient);
            // if (maxColorComponent > 1.0f)
            // {
            //     GUI.Label(new Rect(position.x, position.y, position.width - 3, position.height), "HDR", EditorStyles.centeredGreyMiniLabel);
            // }
        }

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
                            LwguiGradientWindow.Show(gradient, colorSpace, viewChannelMask, timeRange, GUIView.current);
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
                    if (GUIUtility.keyboardControl == id && (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
                    {
                        evt.Use();
                        LwguiGradientWindow.Show(gradient, colorSpace, viewChannelMask, timeRange, GUIView.current);
                        GUIUtility.ExitGUI();
                    }

                    break;
                case EventType.Repaint:
                    DrawGradientWithBackground(rect, gradient, colorSpace, viewChannelMask);
                    // var previewRect = new Rect(rect.x + 2.5f, rect.y + 2.5f, rect.width - 5.0f, rect.height - 5.0f);
                    // EditorGUI.DrawPreviewTexture(previewRect, gradient.GetPreviewRampTexture(256, 1, colorSpace, viewChannelMask));
                    // var outlineRect = new Rect(rect.x + 0.5f, rect.y + 0.5f, rect.width - 1.0f, rect.height - 1.0f);
                    // EditorStyles.colorPickerBox.Draw(outlineRect, GUIContent.none, id);
                    break;
                case EventType.ExecuteCommand:
                    // When drawing the modifying Gradient Field and it has changed
                    if ((GUIUtility.keyboardControl == id || s_LwguiGradientID == id)
                        && evt.commandName == LwguiGradientWindow.LwguiGradientChangedCommand)
                    {
                        // evt.Use();
                        GUI.changed = true;
                        HandleUtility.Repaint();
                    }
                    break;
            }
        }

        /// Lwgui Gradient Field with full Undo/Redo/ContextMenu functions
        public static void GradientField(Rect position, GUIContent label, SerializedProperty property, LwguiGradient gradient, 
            ColorSpace colorSpace = ColorSpace.Gamma, 
            LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All, 
            LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            
            GradientField(position, label, gradient, colorSpace, viewChannelMask, timeRange);

            if (EditorGUI.EndChangeCheck())
            {
                GUI.changed = true;
                // TODO: Undo/Redo
                // Undo.RecordObject(property.serializedObject.targetObject, "Editing Lwgui Gradient");
                property.serializedObject.UpdateIfRequiredOrScript();
            }
            EditorGUI.EndProperty();
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
                && evt.commandName == LwguiGradientWindow.LwguiGradientChangedCommand)
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
                    LwguiGradientWindow.Show(gradient, colorSpace, viewChannelMask, timeRange, GUIView.current);
                    GUIUtility.ExitGUI();
                }
            }
            
            return clicked;
        }
    }
}