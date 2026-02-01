using System.Collections.Generic;
using System.Linq;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Unity.Barracuda;
using UnityEngine;

namespace FaceDetection
{
    /// <summary>
    /// The 'BianLianExpressionModel' or BLEM is created in this project off a naive set of MediaPipe category data.
    /// This loads the exported model and runs inference on it, during game runtime, in order to determine the expression
    /// of the player relative to the expressions we have set in the <see cref="Expression"/> enumeration.
    /// </summary>
    public class BlemBarracudaRunner : MonoBehaviour
    {
        public NNModel BlemModelAsset;

        [Header("Confidence Parameters")] 
        [Range(0, 1)]
        [SerializeField]
        private float MinimumConfidence = 0.65f;

        [Range(0, 1)] 
        [SerializeField] 
        private float EnterThreshold = 0.85f;

        [Range(0, 1)]
        [SerializeField] 
        private float ExitThreshold = 0.75f; 

        [SerializeField] 
        private PlayerController m_PlayerController;
        
        private IWorker m_Worker;
        private Model m_Model;

        private Queue<float[]> m_InferenceQueue = new();
        private float[] m_SmoothedProbabilities = new float[(int)Expression.Max];

        private void Awake()
        {
            if (BlemModelAsset is null)
                Debug.Log("The BlemBarracudaRunner does not have a reference to a BLEM model to perform inference on! Inference will fail!");
            
            m_Model = ModelLoader.Load(BlemModelAsset);
            m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, m_Model);
        }

        private void Update()
        {
            // No inference tasks to perform. Dequeue five rows at once.
            if (m_InferenceQueue.Count < 5)
                return;
            
            // Dequeue five rows because the first layer of the NN expects 5 sets of input features.
            // Data across five frames seems to help it account for motion/subtle changes in motion a bit more.
            float[] features1 = m_InferenceQueue.Dequeue();
            float[] features2 = m_InferenceQueue.Dequeue();
            float[] features3 = m_InferenceQueue.Dequeue();
            float[] features4 = m_InferenceQueue.Dequeue();
            float[] features5 = m_InferenceQueue.Dequeue();
            float[] allFeatures = features1
                .Concat(features2)
                .Concat(features3)
                .Concat(features4)
                .Concat(features5)
                .ToArray();
            
            using Tensor input = new(1, allFeatures.Length, allFeatures);
            m_Worker.Execute(input);

            using Tensor outputLogitsTensor = m_Worker.PeekOutput(); // Logits, since my model does not output softmax by default
            float[] outputLogits = outputLogitsTensor.ToReadOnlyArray();
            // TODO: Having the model do this would be way better.
            float[] probabilities = Softmax(outputLogits);
            
            // 3. EMA Smoothing
            float smoothingAlpha = 0.2f;
            for (int i = 0; i < (int)Expression.Max; ++i)
            {
                m_SmoothedProbabilities[i] = smoothingAlpha * probabilities[i] + (1f - smoothingAlpha) * m_SmoothedProbabilities[i];
            }
            
            // 4. Pick Candidate
            int expression = System.Array.IndexOf(m_SmoothedProbabilities, m_SmoothedProbabilities.Max());
            float confidence = m_SmoothedProbabilities[expression];
            Expression expressionValue = (Expression)expression;
            
            // 5. Confidence Threshold
            // Only change the expression if confidence is above a certain value
            if (confidence < MinimumConfidence)
                return;
            
            // 6. Hysteresis
            // Check if we have enough confidence to stay in the same expression.
            if (m_PlayerController.CurrentExpression == expressionValue)
            {
                // Example: The player was in the 'Happy' state, but now the confidence for 'Happy' has dropped below 40%.
                // This may yield a more natural shift in emotion: 'Happy' -> 'Neutral' -> 'Sad'.
                if (confidence < ExitThreshold)
                {
                    m_PlayerController.CurrentExpression = Expression.Neutral;
                    Debug.Log($"BLEM exited {expressionValue} as a result of having only {confidence * 100}% confidence.");
                }
            }
            else
            {
                if (confidence > EnterThreshold)
                {
                    m_PlayerController.CurrentExpression = expressionValue;
                    Debug.Log($"BLEM entered {expressionValue}, with {confidence * 100}% confidence.");
                }
            }
        }

        private void OnDestroy()
            => m_Worker.Dispose();

        public void CheckExpressionNextFrame(FaceLandmarkerResult result)
        {
            if (result.faceBlendshapes is null)
                return;
            
            // Flatten this result into the input features. Using the exporter function ensures that the score
            // array is in-line with the data the model was trained on. It also ensures that the '_neutral' feature
            // does not end up here, which is dumped by MediaPipe by default.
            float[] features = ExpressionSampleExporter.GetFeaturesFromResultMatchingModelData(result);
            m_InferenceQueue.Enqueue(features);
        }
        
        private static float[] Softmax(float[] logits)
        {
            float max = logits.Max();
            float sum = 0f;

            float[] exp = new float[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                exp[i] = Mathf.Exp(logits[i] - max);
                sum += exp[i];
            }

            for (int i = 0; i < exp.Length; i++)
                exp[i] /= sum;

            return exp;
        } 
    }
}
