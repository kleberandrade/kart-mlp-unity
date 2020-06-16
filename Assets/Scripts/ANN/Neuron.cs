using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Neuron
{
    public int numInputs;
    public double bias;
    public double output;
    public double error;
    public List<double> weights = new List<double>();
    public List<double> inputs = new List<double>();

    public Neuron(int numInputs, double bias, double output, double error, List<double> weights, List<double> inputs)
    {
        this.numInputs = numInputs;
        this.bias = bias;
        this.output = output;
        this.error = error;
        this.weights = weights;
        this.inputs = inputs;
    }

    public Neuron(int nInputs)
    {
        bias = Random.Range(-2.0f, 2.0f);
        numInputs = nInputs;

        for (int i = 0; i < nInputs; i++)
            weights.Add(Random.Range(-2.0f, 2.0f));
    }
}
