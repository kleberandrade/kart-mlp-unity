
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KartManual : MonoBehaviour
{
    [Header("Controller")]
    public string m_HorizontalAxisName = "Horizontal";
    public string m_VerticalAxisName = "Vertical";
    public string m_BrakeButtonName = "Jump";

    [Header("Sample")]
    public string m_SampleFilename = "samples.json";
    public float m_SensorDistance = 20.0f;
    public LayerMask m_IgnoreLayer;
    [Range(1, 5)]
    public int m_Accuracy = 1;

    private Database m_Database = new Database();
    private CarKinematics m_Controller;
    private RaycastHit m_Hit;
    private float m_Distance;

    private void Awake()
    {
        m_Controller = GetComponent<CarKinematics>();
    }

    private void Update()
    {
        m_Controller.HorizontalInput = Input.GetAxis(m_HorizontalAxisName);
        m_Controller.VerticalInput = Input.GetAxis(m_VerticalAxisName);
        m_Controller.BrakeInput = Input.GetButton(m_BrakeButtonName);

        if (m_Controller.HorizontalInput == 0 && m_Controller.VerticalInput == 0 && !m_Controller.BrakeInput)
            return;

        var inputs = new List<double>();
        inputs.Add(CalculateDistanceSensor(transform.forward));
        inputs.Add(CalculateDistanceSensor(transform.right));
        inputs.Add(CalculateDistanceSensor(-transform.right));
        inputs.Add(CalculateDistanceSensor(Quaternion.AngleAxis(-45, Vector3.up) * transform.right));
        inputs.Add(CalculateDistanceSensor(Quaternion.AngleAxis(45, Vector3.up) * -transform.right));

        var outputs = new List<double>();
        outputs.Add(System.Math.Round(m_Controller.HorizontalInput, m_Accuracy));
        outputs.Add(System.Math.Round(m_Controller.VerticalInput, m_Accuracy));
        outputs.Add(m_Controller.BrakeInput ? 1 : -1);

        var data = new DataSet(inputs, outputs);
        if (!m_Database.dataSets.Contains(data))
            m_Database.dataSets.Add(data);
    }

    private void OnApplicationQuit()
    { 
        string data = JsonUtility.ToJson(m_Database);
        FileHelper.SaveFile(m_SampleFilename, data);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private double CalculateDistanceSensor(Vector3 direction)
    {
        m_Distance = 1.0f;
        if (Physics.Raycast(transform.position, direction, out m_Hit, m_SensorDistance, m_IgnoreLayer))
            m_Distance = m_Hit.distance / m_SensorDistance;

        Debug.DrawRay(transform.position, direction * (m_Distance * m_SensorDistance), Color.green);
        return System.Math.Round(1 - m_Distance, m_Accuracy);
    }
}