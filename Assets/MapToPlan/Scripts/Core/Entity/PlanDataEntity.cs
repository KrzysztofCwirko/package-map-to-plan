using System.Collections.Generic;

namespace MapToPlan.Scripts.Core.Entity
{
    public class PlanDataEntity
    {
        public AxisType TargetAxis { get; }
        public float PixelsPerUnit { get; }
        public float Padding { get; }
        public int MaxOutputDimension { get; }
        
        /// <param name="targetAxis">Where to put camera and calculate orthographic size</param>
        /// <param name="pixelsPerUnit">How big will be your output image. 500 is enough to always be readable for bigger maps (>4m). Decreasing this value will result in faster, but more blurry maps.</param>
        /// <param name="padding">Padding around result</param>
        /// <param name="maxOutputDimension">Optional: for high-dimension images scale down the output to reach this parameter max</param>
        public PlanDataEntity(AxisType targetAxis = AxisType.XZ,
            float pixelsPerUnit = 512, float padding = 0.2f, int maxOutputDimension = 8192)
        {
            Features = new List<PlanFeature>();
            TargetAxis = targetAxis;
            PixelsPerUnit = pixelsPerUnit;
            Padding = padding;
            MaxOutputDimension = maxOutputDimension;
        }
        
        public List<PlanFeature> Features { get; set; }

        public void Clear()
        {
            foreach (var planFeature in Features)
            {
                planFeature.Clean();
                planFeature.CleanModifiers();
            }
            
            Features.Clear();
        }
    }
}