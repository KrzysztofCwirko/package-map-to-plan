using System.Collections;
using System.Collections.Generic;
using MapToPlan.Example._Scripts.Features;
using MapToPlan.Example._Scripts.Modifiers;
using MapToPlan.Scripts.Core;
using MapToPlan.Scripts.Core.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MapToPlan.Example._Scripts.Test
{
    public class TestMapToPlan : MonoBehaviour
    {
        [SerializeField] private RawImage testImage;
        [SerializeField] private LineRenderer testLine;

        [SerializeField] private LineRenderer linePrefab;
        [SerializeField] private TMP_Text measureModifierPrefab;

        private IEnumerator Start()
        {
            var temp = new Vector3[testLine.positionCount];
            testLine.GetPositions(temp);
            var feature = new LineRendererFeature(temp, linePrefab).SetModifiers(new LineMeasureModifier(measureModifierPrefab));

            var result = Scripts.Core.MapToPlan.Instance.MakeMeasureMaps(new[]
            {
                new PlanDataEntity
                {
                    Features = new List<PlanFeature> {feature}
                }
            }, AxisType.YZ, 500, 1, 1024);

            while (!result.IsCompleted)
            {
                yield return null;
            }

            testImage.texture = result.Result[0];
            testImage.rectTransform.sizeDelta = new Vector2(testImage.texture.width, testImage.texture.height);
        }
    }
}