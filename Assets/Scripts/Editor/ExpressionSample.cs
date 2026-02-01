using System;
using FaceDetection;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Represents an expression captured from a texture using the MediaPipe API.
    /// </summary>
    [CreateAssetMenu(menuName = "Facial Recognition/Expression Sample")]
    public class ExpressionSample : ScriptableObject
    {
        [Tooltip("Name of the expression.")]
        public string Name;
    
        [Tooltip("Emotion associated with the expression.")]
        public string Emotion;

        [SerializeReference]
        [Tooltip("Collection of blend shapes for the expression.")]
        public BlendShapeInfo[] BlendShapes;

        [SerializeReference]
        [Tooltip("Source texture the expression was created from.")]
        public Texture2D Source;

        // TODO: Is it necessary for us to add link to masks that we spawn in-game?
        [Tooltip("In-game representation of this expression as a mask.")]
        public Texture2D InGameMaskSprite;

        /// <summary>
        /// Expression sample wrapper class to simplify the amount of data exported to Json.
        /// This is only because I didn't want all the extra fields Unity will dump.
        /// </summary>
        private struct ExpressionSampleJsonWrapper
        {
            public string Name;
            public BlendShapeInfo[] BlendShapes;
        }

        [ContextMenu("Export To JSON")]
        public void ExportToJson()
        { 
            ExpressionSampleJsonWrapper e = new()
            {
                Name = Name,
                BlendShapes = BlendShapes
            };
            
            string json = JsonUtility.ToJson(e, true);
            
            // Just dumping this to the Desktop for now.
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = System.IO.Path.Combine(desktopPath, $"{Name}.expression.json");
            
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"Exported {Name} to {path}...");
        }
    }
}