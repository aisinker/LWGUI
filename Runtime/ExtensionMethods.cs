using UnityEngine;


// Disable Keyframe.tangentMode obsolete warning
#pragma warning disable 0618


namespace LWGUI.Runtime
{
    public static class ExtensionMethods
    {
        #region Lwgui Gradient

        public static Keyframe SetLinearTangentMode(this Keyframe key)
        {
            key.tangentMode = 69;
            return key;
        }

        public static void SetLinearTangent(this AnimationCurve curve)
        {
            for (int i = 1; i < curve.keys.Length; i++)
            {
                var keyStart = curve.keys[i - 1];
                var keyEnd = curve.keys[i];
                float tangent = (keyEnd.value - keyStart.value) / (keyEnd.time - keyStart.time);
                keyStart.outTangent = tangent;
                keyEnd.inTangent = tangent;
                curve.MoveKey(i - 1, keyStart);
                curve.MoveKey(i, keyEnd);
            }
        }

        #endregion
    }
}