using System.Linq;
using MapToPlan.Scripts.Core;
using UnityEngine;

namespace MapToPlan.Example._Scripts.Features
{
    /// <summary>
    /// Use LineRenderers to draw lines on the output map
    /// </summary>
    public class LineRendererFeature : PlanFeature<Vector3[]>
    {
        public LineRendererFeature(Vector3[] input, LineRenderer prefab) : base(input)
        {
            Line = prefab;
        }

        private LineRenderer Line { get; }
        private LineRenderer InstantiatedLine { get; set; }

        public override void FillPlan(Transform parent, AxisType axisType)
        {
            InstantiatedLine = Object.Instantiate(Line, parent);
            InstantiatedLine.positionCount = Data.Length;
            InstantiatedLine.SetPositions(Data);

            if (Vector3.Distance(InstantiatedLine.GetPosition(InstantiatedLine.positionCount - 1), InstantiatedLine.GetPosition(0)) < 0.1f)
            {
                InstantiatedLine.loop = true;
            }
        }
        
        public override Bounds GetMyExtends()
        {
            return InstantiatedLine.bounds;
        }

        public override void ApplyScaleChange(float newScale)
        {
            InstantiatedLine.startWidth *= newScale;
            InstantiatedLine.endWidth = InstantiatedLine.startWidth;
        }
        
        
    }
}