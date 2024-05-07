using System;
using MapToPlan.Scripts.Core;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapToPlan.Example._Scripts.Modifiers
{
    public class LineMeasureModifier : FeatureModifier<Vector3[]>
    {
        private TMP_Text TextPrefab { get; }

        public LineMeasureModifier(TMP_Text textPrefab)
        {
            TextPrefab = textPrefab;
        }

        protected override void Apply(Vector3[] input, Transform parent, AxisType axisType)
        {
            for (var i = 1; i < input.Length; i++)
            {
                var a = input[i - 1];
                var b = input[i];
                var dist = Vector3.Distance(a, b);

                var measureText = Object.Instantiate(TextPrefab, parent);
                measureText.text = dist.ToString("0.00") + " m";

                Quaternion lookAt;
                switch (axisType)
                {
                    case AxisType.XZ:
                        lookAt = Quaternion.LookRotation(b - a, Vector3.up);
                        lookAt = Quaternion.Euler(lookAt.eulerAngles.With(90, -90, 0));
                        break;
                    case AxisType.XY:
                        lookAt = Quaternion.LookRotation(b - a, Vector3.right);
                        var zR = 180f;
                        var xR = lookAt.eulerAngles.x - 180f > 0f ? 0f: 180f;
                        lookAt *= Quaternion.Euler(xR,90, zR);
                        break;
                    case AxisType.YZ:
                        // lookAt = Quaternion.LookRotation(b - a, Vector3.forward);
                        // zR = lookAt.eulerAngles.z - 180f > 0f ? 180: 0f;
                        // var sign = Mathf.Sign(lookAt.eulerAngles.x-180f);
                        //
                        // if (sign > 0 || Math.Abs(lookAt.eulerAngles.x) < 0.001f)
                        // {
                        //     lookAt *= Quaternion.Euler(0f,90, zR);
                        // }
                        // else
                        // {
                        //     lookAt *= Quaternion.Euler(180f, 180, lookAt.eulerAngles.x*sign);
                        // }
                        lookAt = Quaternion.LookRotation(b - a, Vector3.up);
                        lookAt = Quaternion.Euler(lookAt.eulerAngles.With(0, -90, 90));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(axisType), axisType, null);
                }

                measureText.transform.rotation = lookAt;
                measureText.transform.position = (a + (b - a) / 2f) + measureText.transform.up * 0.2f;

            }
        }
    }
}