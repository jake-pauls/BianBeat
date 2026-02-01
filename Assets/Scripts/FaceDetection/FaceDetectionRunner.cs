using System;
using System.Collections;
using System.Linq;
using Mediapipe;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;
using UnityEngine;
using UnityEngine.Rendering;

namespace FaceDetection
{
    /// <summary>
    /// Drives the execution of landmark detection tasks using the MediaPipe API. This is originally stemming from
    /// MediaPipeUnity/Samples/Scenes/Face Landmark Detection/FaceLandmarkRunner.cs
    /// </summary>
    public class FaceDetectionRunner : VisionTaskApiRunner<FaceLandmarker>
    {
        public PlayerController PlayerController;
        
        [SerializeField] 
        private FaceLandmarkerResultAnnotationController m_FaceLandmarkerResultAnnotationController;
        
        [SerializeField] 
        [Tooltip("Used to run inference and check expressions using the BLEM model.")]
        private BlemBarracudaRunner m_BlemBarracudaRunner;

        private TextureFramePool m_TextureFramePool;
        private readonly FaceLandmarkDetectionConfig m_FaceLandmarkerConfig = new();

        public override void Stop()
        {
            base.Stop();
            m_TextureFramePool?.Dispose();
            m_TextureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            yield return AssetLoader.PrepareAssetAsync(m_FaceLandmarkerConfig.ModelPath);

            var options = m_FaceLandmarkerConfig.GetFaceLandmarkerOptions(
                m_FaceLandmarkerConfig.RunningMode == Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnFaceLandmarkDetectionOutput
                    : null);
            taskApi = FaceLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
            m_TextureFramePool =
                new TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

            // NOTE: The screen will be resized later, keeping the aspect ratio.
            screen.Initialize(imageSource);

            SetupAnnotationController(m_FaceLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions =
                new Mediapipe.Tasks.Vision.Core.ImageProcessingOptions(
                    rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var result = FaceLandmarkerResult.Alloc(options.numFaces);

            // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
            var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
                                 GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!m_TextureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return null;
                    continue;
                }

                // Build the input Image
                Image image;
                switch (m_FaceLandmarkerConfig.ImageReadMode)
                {
                    case ImageReadMode.GPU:
                        if (!canUseGpuImage)
                        {
                            throw new System.Exception("ImageReadMode.GPU is not supported");
                        }

                        textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildGPUImage(glContext);
                        // TODO: Currently we wait here for one frame to make sure the texture is fully copied to the TextureFrame before sending it to MediaPipe.
                        // This usually works but is not guaranteed. Find a proper way to do this. See: https://github.com/homuler/MediaPipeUnityPlugin/pull/1311
                        yield return waitForEndOfFrame;
                        break;
                    case ImageReadMode.CPU:
                        yield return waitForEndOfFrame;
                        textureFrame.ReadTextureOnCPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                    case ImageReadMode.CPUAsync:
                    default:
                        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally,
                            flipVertically);
                        yield return waitUntilReqDone;

                        if (req.hasError)
                        {
                            Debug.LogWarning($"Failed to read texture from the image source");
                            continue;
                        }

                        image = textureFrame.BuildCPUImage();
                        textureFrame.Release();
                        break;
                }

                switch (taskApi.runningMode)
                {
                    case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            m_FaceLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            m_FaceLandmarkerResultAnnotationController.DrawNow(default);
                        }

                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions,
                                ref result))
                        {
                            m_FaceLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            m_FaceLandmarkerResultAnnotationController.DrawNow(default);
                        }

                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        private void OnFaceLandmarkDetectionOutput(FaceLandmarkerResult result, Image image, long timestamp)
        {
            m_FaceLandmarkerResultAnnotationController.DrawLater(result);
            
            // The BLEM runner will update the player's expression as it performs inference,
            // so just give it the MediaPipe results and let it do work. :)
            m_BlemBarracudaRunner.CheckExpressionNextFrame(result);
        }

        /// <summary>
        /// Old (pre-BLEM) heuristic/idea for detecting emotions on a per-category basis.
        /// </summary>
        [Obsolete]
        private Expression DetermineExpressionFromResult(FaceLandmarkerResult result)
        {
            if (result.faceBlendshapes is null || !result.faceBlendshapes.Any())
            {
                return Expression.Neutral;
            }
            
            var expression = Expression.Neutral;
            
            // TODO: Do we need to use the Landmarker? I just realized we don't use the landmark data.
            var categories = result.faceBlendshapes
                .SelectMany(c => c.categories)
                .ToDictionary(c => c.categoryName, c => c.score);
            
            // This is a really naive way of detecting emotions at the moment. Emotions are normally dictated by more than
            // just a few blend shapes. Luckily, the API exposes simple ones for 'smile' and 'frown'. That being said,
            // the weights/scores for frown seem to be much less extreme than those for the smile. For instance, the smile
            // score will go from ~1 to ~0 if a smile is made or not, where, in contrast, the frown score will range from
            // ~0 to ~0.1 at max.

            // Smile / Happy
            float smileLeft = categories[MOUTH_SMILE_LEFT_CATEGORY_LABEL];
            float smileRight = categories[MOUTH_SMILE_RIGHT_CATEGORY_LABEL];
            if (smileLeft > 0.8 && smileRight > 0.8)
            {
                expression = Expression.Happy;
            }
            
            // Frown
            float frownLeft = categories[MOUTH_FROWN_LEFT_CATEGORY_LABEL];
            float frownRight = categories[MOUTH_FROWN_RIGHT_CATEGORY_LABEL];
            if (frownLeft > 0.01 && frownRight > 0.01)
            {
                expression = Expression.Sad;
            }

            return expression;
        }

        private const string MOUTH_SMILE_LEFT_CATEGORY_LABEL = "mouthSmileLeft";
        private const string MOUTH_SMILE_RIGHT_CATEGORY_LABEL = "mouthSmileRight";
        
        private const string MOUTH_FROWN_LEFT_CATEGORY_LABEL = "mouthFrownLeft";
        private const string MOUTH_FROWN_RIGHT_CATEGORY_LABEL = "mouthFrownRight";
    }
}