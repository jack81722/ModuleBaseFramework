

using System;
using System.Collections.Generic;

namespace Game.Math.Eaze {
    public static class EazeFunc {
        public static IEazeFunc GetEazeFunc<T>() where T : IEazeFunc, new() {
            return GenericEazeCache<T>.Singleton;
        }

        public static IEazeFunc GetEazeFunc(EEazeType eazeType) {
            IEazeFunc eazeFunc;
            switch (eazeType) {
                case EEazeType.QuadIn:
                    eazeFunc = GetEazeFunc<QuadIn>();
                    break;
                case EEazeType.QuadOut:
                    eazeFunc = GetEazeFunc<QuadOut>();
                    break;
                case EEazeType.QuadInOut:
                    eazeFunc = GetEazeFunc<QuadInOut>();
                    break;
                case EEazeType.CubicIn:
                    eazeFunc = GetEazeFunc<CubicIn>();
                    break;
                case EEazeType.CubicOut:
                    eazeFunc = GetEazeFunc<CubicOut>();
                    break;
                case EEazeType.CubicInOut:
                    eazeFunc = GetEazeFunc<CubicInOut>();
                    break;
                default:
                    eazeFunc = GetEazeFunc<Linear>();
                    break;
            }
            return eazeFunc;
        }

        public static float Linear(float t) {
            return t;
        }

        public static float QuadIn(float t) {
            return Clamp01(t * t);
        }

        public static float QuadOut(float t) {
            return Clamp01(2 * t - t * t);
        }

        public static float QuadInOut(float t) {
            return t < 0.5f ? QuadIn(t) : QuadOut(t);
        }

        public static float CubicIn(float t) {
            return Clamp01(t * t * t);
        }

        public static float CubicOut(float t) {
            float tt = Clamp01(1 - t);
            return 1 - tt * tt * tt;
        }

        public static float CubicInOut(float t) {
            return t < 0.5f ? CubicIn(t) : CubicOut(t);
        }

        private static float Clamp01(float f) {
            if (f < 0)
                f = 0;
            if (f > 1)
                f = 1;
            return f;
        }
    }

    [Serializable]
    public enum EEazeType {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
    }

    internal class GenericEazeCache<T> where T : IEazeFunc, new() {
        public readonly static T Singleton = new T();
    }

    #region -- EazeFunc instances --
    public interface IEazeFunc {
        float Eaze(float t);
    }

    public class Linear : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.Linear(t);
        }
    }

    public class QuadIn : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.QuadIn(t);
        }
    }

    public class QuadOut : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.QuadOut(t);
        }
    }

    public class QuadInOut : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.QuadInOut(t);
        }
    }

    public class CubicIn : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.CubicIn(t);
        }
    }

    public class CubicOut : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.CubicOut(t);
        }
    }

    public class CubicInOut : IEazeFunc {
        public float Eaze(float t) {
            return EazeFunc.CubicInOut(t);
        }
    }
    #endregion
}