using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class KartBrain : MonoBehaviour
{
    [Header("Sensor")]
    public float m_SensorDistance = 20.0f;
    [Range(1, 5)]
    public int m_Accuracy = 1;
    public LayerMask m_IgnoreLayer;

    [Header("Neural Net")]
    public bool m_LoadNetFromFile;
    public string m_NetFilename = "net.json";
    [Range(0, 10)]
    public int m_NumberHiddenLayer = 1;
    [Range(1, 1000)]
    public int m_NeuronPerHiddenLayer = 20;
    [Range(0.01f, 1.0f)]
    public float m_LearnRate = 0.01f;
    private NeuralNet m_Net;

    [Header("Training")]
    public string m_SampleFilename = "samples.json";
    public int m_Epochs = 1000;
    public Text m_TextUI;
    private float m_TrainingProgress = 0;
    private double m_LastSSE = 1;
    private double m_SSE = 0;
    private Database m_Database = new Database();
    private bool m_TrainingDone;

    [Header("Inputs (Debug)")]
    public double m_Vertical;
    public double m_Horizontal;
    public bool m_Brake;

    private List<double> m_Inputs = new List<double>();
    private CarKinematics m_Controller;
    private RaycastHit m_Hit;
    private float m_Distance;

    private void Start()
    {
        m_Controller = GetComponent<CarKinematics>();
        m_Net = new NeuralNet(3, 3, m_NumberHiddenLayer, m_NeuronPerHiddenLayer, m_LearnRate);

        if (m_LoadNetFromFile)
        {
            LoadNeuralNet();
            m_TrainingDone = true;
        }
        else
        {
            StartCoroutine(LoadTrainingDataSet());
        }
    }

    private void LoadNeuralNet()
    {
        var textFile = FileHelper.LoadTextResource(m_NetFilename);
        if (textFile != null)
            m_Net = JsonUtility.FromJson<NeuralNet>(textFile.text);
    }

    private void SaveNeuralNet()
    {
        string data = JsonUtility.ToJson(m_Net);
        FileHelper.SaveFile(m_NetFilename, data);
    }

    IEnumerator LoadTrainingDataSet()
    {
        SaveNeuralNet();

        var textFile = FileHelper.LoadTextResource(m_SampleFilename);
        if (textFile != null)
            m_Database = JsonUtility.FromJson<Database>(textFile.text);

        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();
        List<double> calcOutputs = new List<double>();
        float error;

        if (m_Database.Length > 0)
        {
            for (int epoch = 0; epoch < m_Epochs; epoch++)
            {
                m_SSE = 0;
                // SaveNeuralNet();
                for (int sample = 0; sample < m_Database.Length; sample++)
                {
                    inputs = m_Database.dataSets[sample].inputs;
                    outputs = m_Database.dataSets[sample].outputs;
                    calcOutputs = m_Net.Train(inputs, outputs);

                    error = (Mathf.Pow((float)(outputs[0] - calcOutputs[0]), 2) +
                            Mathf.Pow((float)(outputs[1] - calcOutputs[1]), 2) +
                            Mathf.Pow((float)(outputs[2] - calcOutputs[2]), 2)) / 3.0f;

                    m_SSE += error;
                }

                m_TrainingProgress = epoch / (float)m_Epochs;
                m_SSE /= (double)m_Database.Length;

                if (m_LastSSE < m_SSE)
                {
                    //LoadNeuralNet();
                    m_Net.learnRate = Mathf.Clamp((float)m_Net.learnRate - 0.001f, 0.001f, 0.9f);
                }
                else
                {
                    m_Net.learnRate = Mathf.Clamp((float)m_Net.learnRate + 0.001f, 0.001f, 0.9f);
                    m_LastSSE = m_SSE;
                }

                m_LearnRate = (float)m_Net.learnRate;
                yield return null;
            }
        }

        m_TrainingDone = true;
        SaveNeuralNet();
    }

    private void Update()
    {
        m_TextUI.text = $"SSE: {m_SSE:0.0000}\nTraining progress: {Mathf.Round(m_TrainingProgress * 100.0f)}%";

        if (!m_TrainingDone) return;

        m_Inputs.Clear();
        m_Inputs.Add(CalculateDistanceSensor(transform.forward));
        m_Inputs.Add(CalculateDistanceSensor(transform.right));
        m_Inputs.Add(CalculateDistanceSensor(-transform.right));
        m_Inputs.Add(CalculateDistanceSensor(Quaternion.AngleAxis(-45, Vector3.up) * transform.right));
        m_Inputs.Add(CalculateDistanceSensor(Quaternion.AngleAxis(45, Vector3.up) * -transform.right));

        List<double> outputs = m_Net.Calculate(m_Inputs, new List<double>());

        m_Horizontal = outputs[0];
        m_Vertical = outputs[1];
        m_Brake = outputs[2] > 0;

        m_Controller.HorizontalInput = (float)m_Horizontal;
        m_Controller.VerticalInput = (float)m_Vertical;
        m_Controller.BrakeInput = m_Brake;
    }

    private double CalculateDistanceSensor(Vector3 direction)
    {
        m_Distance = 1.0f;
        if (Physics.Raycast(transform.position, direction, out m_Hit, m_SensorDistance, m_IgnoreLayer))
            m_Distance = m_Hit.distance / m_SensorDistance;

        Debug.DrawRay(transform.position, direction * (m_Distance * m_SensorDistance), Color.green);
        return System.Math.Round(m_Distance, m_Accuracy);
    }
}
