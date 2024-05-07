using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private LineRenderer[] testLines;

        [SerializeField] private LineRenderer linePrefab;
        [SerializeField] private TMP_Text measureModifierPrefab;

        [SerializeField] private Texture2D[] results;

        private IEnumerator Start()
        {
            var maps = new PlanDataEntity[testLines.Length];

            for (var i = 0; i < maps.Length; i++)
            {
                var temp = new Vector3[testLines[i].positionCount];
                testLines[i].GetPositions(temp);
                var feature = new LineRendererFeature(temp, linePrefab).SetModifiers(new LineMeasureModifier(measureModifierPrefab));
                maps[i] = new PlanDataEntity((AxisType)Enum.GetNames(typeof(AxisType)).ToList().IndexOf(testLines[i].name[5..]), 500, 1, 1024)
                {
                    Features = new List<PlanFeature> {feature}
                };
            }
         

            var result = Scripts.Core.MapToPlan.Instance.MakeMeasureMaps(maps);

            while (!result.IsCompleted)
            {
                yield return null;
            }

            results = result.Result;
            StartCoroutine(Scroll());
       }

        private IEnumerator Scroll()
        {
            var id = 0;
            while (true)
            {
                testImage.texture = results[id];
                testImage.rectTransform.sizeDelta = new Vector2(testImage.texture.width, testImage.texture.height);
                
                var t = 1f;

                while (t > 0)
                {
                    t -= Time.deltaTime;
                    yield return null;
                }

                id += 1;
                id %= results.Length;
            }
        }
    }
}