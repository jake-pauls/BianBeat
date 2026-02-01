using System;
using System.Collections.Generic;
using Mediapipe.Tasks.Components.Containers;
using UnityEngine;

namespace FaceDetection
{
    /// <summary>
    /// POC representing the name and score for a blend shape in a single <see cref="ExpressionSample"/>.
    /// </summary>
    [Serializable]
    public class BlendShapeInfo
    {
        [Tooltip("Name of the blend shape.")]
        public string Name;

        [Range(0.0f, 1.0f)] 
        [Tooltip("Score of the blend shape for this emotion.")]
        public float Score;
        
        /// <summary>
        /// Creates a list of <see cref="BlendShapeInfo"/> from a set of categories MediaPipe provides.
        /// </summary>
        /// <param name="categories">Enumeration of categories provided by the MediaPipe API.</param>
        /// <returns>Collection of POCs containing information for each blend shape.</returns>
        public static List<BlendShapeInfo> CreateBlendShapeInfosFromCategories(IEnumerable<Category> categories)
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