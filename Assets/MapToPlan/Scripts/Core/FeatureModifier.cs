using System;
using UnityEngine;

namespace MapToPlan.Scripts.Core
{
    public abstract class FeatureModifier<T> //where T: PlanFeature<T>
    {
        public ModifierType Type { get; private set; } = ModifierType.AlwaysAfter;
        private int Countdown { get; set; } = 0;
        private int Steps { get; set; } = 0;
        private int Delay { get; set; } = 0;

        /// <summary>
        /// Default is AlwaysAfter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected FeatureModifier<T> SetType(ModifierType type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Set cycle steps
        /// </summary>
        /// <param name="steps">How many features should this modifier skip and repeat? For example, 2 means that it will run after each two features (2,4,6...) </param>
        /// <returns></returns>
        protected FeatureModifier<T> SetCycles(int steps)
        {
            Steps = steps;
            return this;
        }  
        
        /// <summary>
        /// Set starting delay in features
        /// </summary>
        /// <param name="delay">How many features should this modifier skip? For example, 1 means it will run after first feature and so on.</param>
        /// <returns></returns>
        protected FeatureModifier<T> SetDelay(int delay)
        {
            Delay = delay + 1;
            return this;
        }
        
        public void ApplyMe(T input, Transform parent, AxisType axisType)
        {
            switch (Type)
            {
                case ModifierType.AlwaysAfter:
                    Apply(input, parent, axisType);
                    return;
                case ModifierType.AlwaysBefore:
                    Apply(input, parent, axisType);
                    return;
                case ModifierType.Cyclic:
                    Countdown = (Countdown + 1) % Steps;
                    if (Countdown != 0) return;
                    Apply(input, parent, axisType);
                    return;
                case ModifierType.DelayedAlwaysAfter:
                    Delay -= 1;
                    if(Delay > 0) return;
                    Apply(input, parent, axisType);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Apply your modifier
        /// </summary>
        /// <param name="input">Same as the feature input</param>
        /// <param name="parent">Same as the feature parent</param>
        /// <param name="axisType">Optional: target axis</param>
        protected virtual void Apply(T input, Transform parent, AxisType axisType)
        {
            
        }

        /// <summary>
        /// Optional modifier extends
        /// </summary>
        /// <returns></returns>
        public virtual bool GetMyExtends(out Bounds result)
        {
            result = default;
            return false;
        }
        
        public void CleanMe()
        {
            Countdown = 0;
            Steps = 0;
            Delay = 0;
            Clean();
        }

        /// <summary>
        /// Clean your modifier
        /// </summary>
        protected virtual void Clean()
        {
            
        }
    }

    /// <summary>
    /// How a modifer should run
    /// </summary>
    public enum ModifierType
    {
        AlwaysAfter,
        AlwaysBefore,
        Cyclic,
        DelayedAlwaysAfter
    }
}