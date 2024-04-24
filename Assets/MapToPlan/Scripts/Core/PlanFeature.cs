using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapToPlan.Scripts.Core
{
    public abstract class PlanFeature
    {
        /// <summary>
        /// Fill the map. They will be automatically set to be on the MapRender layer.
        /// </summary>
        /// <param name="parent">Spawn your objects in this parent.</param>
        public abstract void FillPlan(Transform parent);

        /// <summary>
        /// How big is this feature, for example the bounds of LineRenderer
        /// </summary>
        /// <returns>Array of length 2, at [0] min bound and at [1] max bound</returns>
        public abstract Bounds GetMyExtends();
        
        /// <summary>
        /// Same as GetMyExtends, but calculates modifiers only
        /// </summary>
        /// <returns></returns>
        public abstract bool GetModifiersExtend(out Bounds result);

        /// <summary>
        /// Optional place to clean your data
        /// </summary>
        public abstract void Clean();
        
        /// <summary>
        /// Optional place to clean your modifiers
        /// </summary>
        public abstract void CleanModifiers();

        /// <summary>
        /// Apply modifiers of given ModifierType. This is called automatically
        /// </summary>
        /// <param name="target">Run modifiers of this type</param>
        /// <param name="parent">Same as the FillPlan parent</param>
        public abstract void ApplyModifiers(ModifierType target, Transform parent);
    }
    
    public class PlanFeature<T> : PlanFeature
    {
        protected T Data { get; }
        private List<FeatureModifier<T>> Modifiers { get; set; }

        protected PlanFeature(T input)
        {
            Data = input;
        }
        
        public override void FillPlan(Transform parent)
        {
            
        }

        public override Bounds GetMyExtends()
        {
            return default;
        }
        
        public override void Clean()
        {
            
        }

        /// <summary>
        /// Add custom modifiers to your feature
        /// </summary>
        /// <param name="modifiers">Modifiers to set (not add)</param>
        /// <returns>List of your modifiers</returns>
        public PlanFeature<T> SetModifiers(params FeatureModifier<T>[] modifiers)
        {
            Modifiers = modifiers.ToList();
            return this;
        }
        
        public override void ApplyModifiers(ModifierType target, Transform parent)
        {
            if(Modifiers == default || Modifiers.Count == 0) return;
            var targets = Modifiers.FindAll(m => m.Type == target);
            foreach (var featureModifier in targets)
            {
                featureModifier.ApplyMe(Data, parent);
            }
        }
        
        public override bool GetModifiersExtend(out Bounds result)
        {
            result = new Bounds();
            if(Modifiers == default || Modifiers.Count == 0) return false;
            
            foreach (var modifier in Modifiers)
            {
                if (modifier.GetMyExtends(out var x))
                {
                    result.Encapsulate(x);
                }
            }

            return Modifiers.Count != 0;
        }
        
        public override void CleanModifiers()
        {
            if(Modifiers == default || Modifiers.Count == 0) return;
            foreach (var featureModifier in Modifiers)
            {
                featureModifier.CleanMe();
            }
        }
    }
}