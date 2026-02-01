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
        public bool Check;
        public NNModel BlemModelAsset;

        // TODO: Setup when the Barracuda runner goes to the main sccene
        // [SerializeField] 
        // private PlayerController m_PlayerController;
        private Expression m_CachedExpression;
        
        private IWorker m_Worker;
        private Model m_Model;

        private Queue<float[]> m_InferenceQueue = new();

        private void Awake()
        {
            if (BlemModelAsset is null)
                Debug.Log("The BlemBarracudaRunner does not have a reference to a BLEM model to perform inference on! Inference will fail!");
            
            m_Model = ModelLoader.Load(BlemModelAsset);
            m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, m_Model);
        }

        private void Update()
        {
            // No inference tasks to perform.
            if (m_InferenceQueue.Count <= 0)
                return;
            
            float[] features = m_InferenceQueue.Dequeue();
            using Tensor input = new(1, features.Length, features);
            m_Worker.Execute(input);

            using Tensor outputLogitsTensor = m_Worker.PeekOutput(); // Logits, since my model does not output softmax by default
            float[] outputLogits = outputLogitsTensor.ToReadOnlyArray();
            // TODO: Having the model do this would be way better.
            float[] probs = Softmax(outputLogits);
        
            int expression = System.Array.IndexOf(probs, probs.Max());

            Expression expressionValue = (Expression)expression;
            if (m_CachedExpression != expressionValue)
            {
                m_CachedExpression = expressionValue;
                float confidence = probs[expression];
                Debug.Log($"BLEM predicted your expression changed to {expressionValue}, with {confidence}% confidence.");
            }
        }

        private void OnDestroy()
            => m_Worker.Dispose();

        public void CheckExpressionNextFrame(FaceLandmarkerResult result)
        {
            if (!Check)
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
