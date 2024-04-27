using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapToPlan.Scripts.Core.Entity;
using UnityEngine;

namespace MapToPlan.Scripts.Core
{
    /// <summary>
    /// Core class managing maps. Add this component to a GameObject in your scene. Call MakeMeasureMaps after Awake/OnEnable.
    /// Make sure that renderCamera has occlusion set to only the value of Layer const (on the line 21), and the other cameras don't include it.
    /// </summary>
    public class MapToPlan : MonoBehaviour
    {
        public static MapToPlan Instance { get; private set; }

        [Header("Setup")]
        [SerializeField] private Camera renderCamera;
        
        private Transform _resultParent;

        private const string Layer = "MapRender";
        
        private void Awake()
        {
            Instance = this;
            _resultParent = Instantiate(new GameObject { name = "Result Parent" }, transform).transform;
            renderCamera.gameObject.SetActive(false);
        }

        /// <summary>
        /// Generate Texture2D based on given features.
        /// </summary>
        /// <param name="targetAxis">Where to put camera and calculate orthographic size</param>
        /// <param name="pixelsPerUnit">How big will be your output image. 500 is enough to always be readable for bigger maps (>4m). Decreasing this value will result in faster, but more blurry maps.</param>
        /// <param name="padding">Padding around result</param>
        /// <param name="input">The features to be draw</param>
        /// <param name="maxOutputDimension">Optional: for high-dimension images scale down the output to reach this parameter max</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Texture2D[]> MakeMeasureMaps(PlanDataEntity[] input, AxisType targetAxis = AxisType.XZ,
            float pixelsPerUnit = 512, float padding = 0.2f, int maxOutputDimension = 8192)
        {
            if (LayerMask.NameToLayer(Layer) == -1)
            {
                throw new Exception($"You need to add a new layer: {Layer}. Go to Tags & Layers and add it.");
            }

            _resultParent.gameObject.SetActive(true);
            var result = new Texture2D[input.Length];

            for (var inputIndex = 0; inputIndex < input.Length; inputIndex++)
            {
                var data = input[inputIndex];

                var children = _resultParent.transform.childCount;
                for (var i = 0; i < children; i++)
                {
                    Destroy(_resultParent.transform.GetChild(i).gameObject);
                }

                //wait for destroy
                await Task.Yield();

                var boundary = new Bounds();
                foreach (var feature in data.Features)
                {
                    feature.ApplyModifiers(ModifierType.AlwaysBefore, _resultParent, targetAxis);
                    feature.FillPlan(_resultParent, targetAxis);
                    feature.ApplyModifiers(ModifierType.AlwaysAfter, _resultParent, targetAxis);
                    feature.ApplyModifiers(ModifierType.Cyclic, _resultParent, targetAxis);
                    feature.ApplyModifiers(ModifierType.DelayedAlwaysAfter, _resultParent, targetAxis);

                    boundary.Encapsulate(feature.GetMyExtends());
                    if (feature.GetModifiersExtend(out var extends))
                    {
                        boundary.Encapsulate(extends);
                    }
                }

                _resultParent.gameObject.SetLayerRecursively(LayerMask.NameToLayer(Layer));

                var targetCameraRotation = targetAxis switch
                {
                    AxisType.XZ => Quaternion.Euler(90, 90, 0),
                    AxisType.XY => Quaternion.Euler(0, 0, -90),
                    AxisType.YZ => Quaternion.Euler(0, 90, 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null)
                };
                
                var t = renderCamera.transform;
                t.rotation = targetCameraRotation;

                float cameraDistance;
                float minA;
                float maxA;
                float minB;
                float maxB;

                switch (targetAxis)
                {
                    case AxisType.XZ:
                        cameraDistance = boundary.max.y;
                        minA = boundary.min.x;
                        maxA = boundary.max.x;
                        minB = boundary.min.z;
                        maxB = boundary.max.z;
                        break;
                    case AxisType.XY:
                        cameraDistance = boundary.max.z;
                        minA = boundary.min.x;
                        maxA = boundary.max.x;
                        minB = boundary.min.y;
                        maxB = boundary.max.y;
                        break;
                    case AxisType.YZ:
                        cameraDistance = boundary.max.x;
                        minA = boundary.min.y;
                        maxA = boundary.max.y;
                        minB = boundary.min.z;
                        maxB = boundary.max.z;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null);
                }

                var width = Mathf.Abs(maxA - minA);
                var height = Mathf.Abs(maxB - minB);

                if (height <= 0.0001f || width <= 0.0001f)
                {
                    throw new Exception("Your bounds are probably all zeros");
                }

                var baseDimension = Mathf.RoundToInt(Mathf.Max(width, height, 2) * pixelsPerUnit);
                var aspect = width / height;

                if (renderCamera.targetTexture != null)
                {
                    renderCamera.targetTexture.Release();
                }

                var outputWidth =  width > height ? baseDimension : Mathf.RoundToInt(baseDimension * (1f / aspect));
                var outputHeight = width > height ? Mathf.RoundToInt(baseDimension * aspect) : baseDimension;

                renderCamera.orthographicSize = (width + padding) * .5f;

                var norm = 1f;

                if (Mathf.Max(outputHeight, outputWidth) > maxOutputDimension)
                {
                    norm =  maxOutputDimension / Mathf.Max((float)outputHeight, outputWidth);
                    _resultParent.localScale = Vector3.one * norm;
                    renderCamera.orthographicSize *= norm;
                    outputWidth = Mathf.RoundToInt(outputWidth * norm);
                    outputHeight = Mathf.RoundToInt(outputHeight * norm);
                }

                if (Mathf.Abs(norm - 1f) > 0.0001f)
                {
                    foreach (var feature in data.Features)
                    {
                        feature.ApplyScaleChange(norm);
                    }
                }
                
                renderCamera.targetTexture = new RenderTexture(outputWidth, outputHeight, 24);

                var targetCameraPosition = targetAxis switch
                {
                    AxisType.XZ => new Vector3(minA + width / 2f, cameraDistance + 1f/norm, minB + height / 2f),
                    AxisType.XY => new Vector3(minA + width / 2f, minB + height / 2f, -cameraDistance - 1f/norm),
                    AxisType.YZ => new Vector3(-cameraDistance - 1f/norm, minA + width / 2f, minB + height / 2f),
                    _ => throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null)
                };
                
                t.position = _resultParent.TransformPoint(targetCameraPosition);
                
                renderCamera.gameObject.SetActive(true);

                await Task.Yield();

                var targetTexture = renderCamera.targetTexture;
                RenderTexture.active = targetTexture;
                var tex2D = new Texture2D(targetTexture.width, targetTexture.height);
                tex2D.ReadPixels(new Rect(0, 0, tex2D.width, tex2D.height), 0, 0);
                tex2D.Apply();
                result[inputIndex] = tex2D;

                renderCamera.gameObject.SetActive(false);

                data.Clear();
                children = _resultParent.transform.childCount;
                for (var i = 0; i < children; i++)
                {
                    // Destroy(_resultParent.transform.GetChild(i).gameObject);
                }

                //wait for destroy
                await Task.Yield();
            }

            _resultParent.gameObject.SetActive(false);

            return result;
        }
    }
}