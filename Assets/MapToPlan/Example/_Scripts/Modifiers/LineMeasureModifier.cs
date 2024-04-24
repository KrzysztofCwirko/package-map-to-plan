using MapToPlan.Scripts.Core;
using TMPro;
using UnityEngine;

namespace MapToPlan.Example._Scripts.Modifiers
{
    public class LineMeasureModifier : FeatureModifier<Vector3[]>
    {
        private TMP_Text TextPrefab { get; }

        public LineMeasureModifier(TMP_Text textPrefab)
        {
            TextPrefab = textPrefab;
        }

        protected override void Apply(Vector3[] input, Transform parent)
        {
            for (var i = 1; i < input.Length; i++)
            {
                var a = input[i - 1];
                var b = input[i];
                var dist = Vector3.Distance(a, b);

                var measureText = Object.Instantiate(TextPrefab, parent);
                measureText.text = dist.ToString("0.00") + " m";

                var lookAt = Quaternion.LookRotation(b - a, Vector3.up);
                lookAt = Quaternion.Euler(lookAt.eulerAngles.With(90, -90, 0));
                measureText.transform.rotation = lookAt;
                measureText.transform.position = (a + (b - a) / 2f) + measureText.transform.up * 0.2f;

            }
        }
    }
}