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
        /// <param name="pairToRotateCamera">Should camera be rotated to match this two(!) vectors (as leverer)?</param>
        /// <param name="rotateAllExtends">Should the camera rotation also rotate all extends? Use it if you are not rotating Vectors yourself</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Texture2D[]> MakeMeasureMaps(PlanDataEntity[] input, AxisType targetAxis = AxisType.XZ,
            float pixelsPerUnit = 512, float padding = 0.2f, Vector3[] pairToRotateCamera = default,
            bool rotateAllExtends = false)
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
                    feature.ApplyModifiers(ModifierType.AlwaysBefore, _resultParent);
                    feature.FillPlan(_resultParent);
                    feature.ApplyModifiers(ModifierType.AlwaysAfter, _resultParent);
                    feature.ApplyModifiers(ModifierType.Cyclic, _resultParent);
                    feature.ApplyModifiers(ModifierType.DelayedAlwaysAfter, _resultParent);

                    boundary.Encapsulate(feature.GetMyExtends());
                    if (feature.GetModifiersExtend(out var extends))
                    {
                        boundary.Encapsulate(extends);
                    }
                }

                _resultParent.gameObject.SetLayerRecursively(LayerMask.NameToLayer(Layer));

                var targetAngle = 0f;

                if (pairToRotateCamera != default)
                {
                    pairToRotateCamera[0] -= targetAxis switch
                    {
                        AxisType.XZ => Vector3.up * pairToRotateCamera[0].y,
                        AxisType.XY => Vector3.forward * pairToRotateCamera[0].z,
                        AxisType.YZ => Vector3.right * pairToRotateCamera[0].x,
                        _ => throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null)
                    };

                    pairToRotateCamera[1] -= targetAxis switch
                    {
                        AxisType.XZ => Vector3.up * pairToRotateCamera[1].y,
                        AxisType.XY => Vector3.forward * pairToRotateCamera[1].z,
                        AxisType.YZ => Vector3.right * pairToRotateCamera[1].x,
                        _ => throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null)
                    };

                    targetAngle = Vector3.Angle(pairToRotateCamera[0], pairToRotateCamera[1]);

                    if (rotateAllExtends)
                    {
                        _resultParent.eulerAngles = targetAxis switch
                        {
                            AxisType.XZ => new Vector3(0, targetAngle, 0),
                            AxisType.XY => pairToRotateCamera[1],
                            AxisType.YZ => pairToRotateCamera[1],
                            _ => throw new ArgumentOutOfRangeException(nameof(targetAxis))
                        };
                    }
                }
                
                float cameraDistance;
                float minA;
                float maxA;
                float minB;
                float maxB;

                Quaternion targetCameraRotation;

                switch (targetAxis)
                {
                    case AxisType.XZ:
                        targetCameraRotation = Quaternion.Euler(90, 90 + targetAngle, 0);
                        cameraDistance = boundary.max.y;
                        minA = boundary.min.x;
                        maxA = boundary.max.x;
                        minB = boundary.min.z;
                        maxB = boundary.max.z;
                        break;
                    case AxisType.XY:
                        targetCameraRotation = Quaternion.Euler(0, 0, targetAngle);
                        cameraDistance = boundary.max.z;
                        minA = boundary.min.x;
                        maxA = boundary.max.x;
                        minB = boundary.min.y;
                        maxB = boundary.max.y;
                        break;
                    case AxisType.YZ:
                        targetCameraRotation = Quaternion.Euler(targetAngle, 90, 0);
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

                var baseDimension = Mathf.RoundToInt(Mathf.Max(width, height, 2) * pixelsPerUnit);

                if (height <= 0.0001f || width <= 0.0001f)
                {
                    throw new Exception("Your bounds are probably all zeros");
                }
                
                var aspect = width / height;

                if (renderCamera.targetTexture != null)
                {
                    renderCamera.targetTexture.Release();
                }

                renderCamera.targetTexture = width > height
                    ? new RenderTexture(baseDimension, Mathf.RoundToInt(baseDimension * aspect), 24)
                    : new RenderTexture(Mathf.RoundToInt(baseDimension * (1f / aspect)), baseDimension, 24);

                renderCamera.orthographicSize = (width + padding) * .5f;
                Transform t;
                (t = renderCamera.transform).rotation = targetCameraRotation;

                var targetCameraPosition = targetAxis switch
                {
                    AxisType.XZ => new Vector3(minA + width / 2f, cameraDistance + 1f, minB + height / 2f),
                    AxisType.XY => new Vector3(minA + width / 2f, minB + height / 2f, cameraDistance + 1f),
                    AxisType.YZ => new Vector3(cameraDistance + 1f, minA + width / 2f, minB + height / 2f),
                    _ => throw new ArgumentOutOfRangeException(nameof(targetAxis), targetAxis, null)
                };

                t.position = targetCameraPosition;
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
                    Destroy(_resultParent.transform.GetChild(i).gameObject);
                }

                //wait for destroy
                await Task.Yield();
            }

            _resultParent.gameObject.SetActive(false);

            return result;
        }
    }
}