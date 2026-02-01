using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FaceDetection
{
    public class ExpressionSampleExporter : MonoBehaviour
    {
        [Header("Recording")] 
        public bool Record;
        public Expression CurrentExpression = Expression.Neutral;

        private StringBuilder m_CsvBuilder;
        private object m_CsvWriteLock = new();
        private Stopwatch m_Stopwatch = new();
        
        private static List<string> s_CsvCategoryColumns;
        
        private const string NEW_ASSET_EMOTION = "DefaultEmotion";
        private const string SAVE_FOLDER_PATH = "Assets/Data/FaceSampleData";

        private const string CSV_FILE_NAME = "mediapipe_emotion_dataset.csv";
        private const string MEDIA_PIPE_CATEGORIES_CSV_HEADER = "timestamp,browDownLeft,browDownRight,browInnerUp,browOuterUpLeft,browOuterUpRight,cheekPuff,cheekSquintLeft,cheekSquintRight,eyeBlinkLeft,eyeBlinkRight,eyeLookDownLeft,eyeLookDownRight,eyeLookInLeft,eyeLookInRight,eyeLookOutLeft,eyeLookOutRight,eyeLookUpLeft,eyeLookUpRight,eyeSquintLeft,eyeSquintRight,eyeWideLeft,eyeWideRight,jawForward,jawLeft,jawOpen,jawRight,mouthClose,mouthDimpleLeft,mouthDimpleRight,mouthFrownLeft,mouthFrownRight,mouthFunnel,mouthLeft,mouthLowerDownLeft,mouthLowerDownRight,mouthPressLeft,mouthPressRight,mouthPucker,mouthRight,mouthRollLower,mouthRollUpper,mouthShrugLower,mouthShrugUpper,mouthSmileLeft,mouthSmileRight,mouthStretchLeft,mouthStretchRight,mouthUpperUpLeft,mouthUpperUpRight,noseSneerLeft,noseSneerRight,label";

        private void Start()
        {
            // Cache the individual headers for lookup.
            s_CsvCategoryColumns = new List<string>();
            string[] splitCategoryHeaders = MEDIA_PIPE_CATEGORIES_CSV_HEADER.Split(",");
            foreach (string header in splitCategoryHeaders)
            {
                // Skip the timestamp and label features, which are filled in manually.
                if (header is "timestamp" or "label") 
                    continue;
                
                s_CsvCategoryColumns.Add(header);
            }
            
            // Write the header with all 52 blend shapes, the timestamp, and the label.
            m_CsvBuilder = new StringBuilder();
            m_CsvBuilder.AppendLine(MEDIA_PIPE_CATEGORIES_CSV_HEADER);
            
            // Start the timer.
            m_Stopwatch.Start();
        }

        /// <summary>
        /// Records a single frame of data to a CSV string builder. Entire list of data is dumped to the CSV file <see cref="OnApplicationQuit"/>.
        /// </summary>
        /// <param name="result">The result of a frame of inference from the MediaPipe FaceLandmark task.</param>
        public void RecordFrame(FaceLandmarkerResult result)
        {
            if (!Record || result.faceBlendshapes is null || result.faceBlendshapes.Count == 0)
                return;
            
            // Take the incoming categories and convert them to serializable blend shapes.
            IEnumerable<Category> categories = result.faceBlendshapes.SelectMany(c => c.categories);
            Dictionary<string, float> blendShapeInfos = CreateBlendShapeInfosFromCategories(categories)
                .ToDictionary(b => b.Name, b => b.Score);

            // TODO: We write to the data right away to avoid keeping it in memory. However, this lock sucks.
            lock (m_CsvWriteLock)
            {
                // Add the timestamp to the CSV.
                m_CsvBuilder.Append(m_Stopwatch.Elapsed.TotalSeconds).Append(',');
            
                // Add each of the scores.
                foreach (string csvColumnName in s_CsvCategoryColumns)
                {
                    // We will always have to skip the "_neutral" category.
                    if (!blendShapeInfos.TryGetValue(csvColumnName, out float score))
                        continue;
                    m_CsvBuilder.Append(score).Append(',');
                }
            
                // Add the current expression label.
                m_CsvBuilder.Append(CurrentExpression).AppendLine();
            }
        }

        public static float[] GetFeaturesFromResultMatchingModelData(FaceLandmarkerResult result)
        {
            IEnumerable<Category> categories = result.faceBlendshapes.SelectMany(c => c.categories);
            Dictionary<string, float> blendShapeInfos = CreateBlendShapeInfosFromCategories(categories)
                .ToDictionary(b => b.Name, b => b.Score);
        
            // Add each of the scores.
            List<float> featureScores = new();
            foreach (string csvColumnName in s_CsvCategoryColumns)
            {
                // We will always have to skip the "_neutral" category.
                if (!blendShapeInfos.TryGetValue(csvColumnName, out float score))
                    continue;
                featureScores.Add(score);
            }

            return featureScores.ToArray();
        }

        private void OnApplicationQuit()
        {
            if (string.IsNullOrEmpty(m_CsvBuilder.ToString()))
                return;
            
            // Just dumping this to the Desktop for now.
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktopPath, CSV_FILE_NAME);
            
            // When the game stops dump all cached emotion data.
            File.WriteAllText(path, m_CsvBuilder.ToString());
            Debug.Log($"Saved emotion data to: {path}");
        }

        /// <summary>
        /// Used to export categories from a <see cref="FaceLandmarkerResult"/> to an <see cref="ExpressionSample"/> scriptable object.
        /// Note that this must be called from the main thread, as it writes to, and updates, the asset database.
        /// </summary>
        /// <param name="result">The face landmarker result provided from the MediaPipe API.</param>
        /// <param name="sourceTexture">The texture asset associated with the result, if applicable (if an image is used, for instance).</param>
        public static void ExportCategoriesToScriptableObject(FaceLandmarkerResult result, Texture sourceTexture)
        {
            // Take the incoming categories and convert them to serializable blend shapes.
            IEnumerable<Category> categories = result.faceBlendshapes.SelectMany(c => c.categories);
            List<BlendShapeInfo> blendShapeInfos = CreateBlendShapeInfosFromCategories(categories);
        
            // Create a new scriptable object containing the categories and the reference texture.
            var sample = ScriptableObject.CreateInstance<ExpressionSample>();
            string name = sourceTexture is not null ? $"{sourceTexture.name}_ExpressionData" : "NewExpressionData";
            sample.Name = name;
            sample.Emotion = NEW_ASSET_EMOTION;
            sample.Source = sourceTexture as Texture2D;
            sample.BlendShapes = blendShapeInfos.ToArray();

            // Save it to the SAVE_FOLDER_PATH.
            string path = System.IO.Path.Combine(SAVE_FOLDER_PATH, $"{name}.asset");
            AssetDatabase.CreateAsset(sample, path);
            AssetDatabase.SaveAssets();
        
            Debug.Log($"Created new face sample data from image: {path}");
        }
        
        /// <summary>
        /// Creates a list of <see cref="BlendShapeInfo"/> from a set of categories MediaPipe provides.
        /// </summary>
        /// <param name="categories">Enumeration of categories provided by the MediaPipe API.</param>
        /// <returns>Collection of POCs containing information for each blend shape.</returns>
        private static List<BlendShapeInfo> CreateBlendShapeInfosFromCategories(IEnumerable<Category> categories)
        {
            List<BlendShapeInfo> blendShapeInfos = new();
        
            foreach (Category category in categories)
            {
                BlendShapeInfo info = new()
                {
                    Name = category.categoryName,
                    Score = category.score,
                };
            
                blendShapeInfos.Add(info);
            }

            return blendShapeInfos;
        }
    }
}
