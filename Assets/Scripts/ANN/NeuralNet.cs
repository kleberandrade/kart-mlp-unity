using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NeuralNet
{
    public int numInputs;
    public int numOutputs;
    public int numHidden;
    public int numNeuronPerHidden;
    public double learnRate;
    public List<Layer> layers = new List<Layer>();

    public NeuralNet(int numInputs, int numOutputs, int numHidden, int numNeuronPerHidden, double learnRate, List<Layer> layers)
    {
        this.numInputs = numInputs;
        this.numOutputs = numOutputs;
        this.numHidden = numHidden;
        this.numNeuronPerHidden = numNeuronPerHidden;
        this.learnRate = learnRate;
        this.layers = layers;
    }

    public NeuralNet(int numInputs, int numOutputs, int numHidden, int numNeuronPerHidden, double learnRate)
    {
        this.numInputs = numInputs;
        this.numOutputs = numOutputs;
        this.numHidden = numHidden;
        this.numNeuronPerHidden = numNeuronPerHidden;
        this.learnRate = learnRate;

        if (numHidden > 0)
        {
            layers.Add(new Layer(numNeuronPerHidden, numInputs));

            for (int i = 0; i < numHidden - 1; i++)
            {
                layers.Add(new Layer(numNeuronPerHidden, numNeuronPerHidden));
            }

            layers.Add(new Layer(numOutputs, numNeuronPerHidden));
        }
        else
        {
            layers.Add(new Layer(numOutputs, numInputs));
        }
    }

    public List<double> Train(List<double> inputValues, List<double> desiredOutput)
    {
        List<double> outputValues = new List<double>();
        outputValues = Calculate(inputValues, desiredOutput);
        UpdateWeights(outputValues, desiredOutput);
        return outputValues;
    }

    public List<double> Calculate(List<double> inputValues, List<double> desiredOutput)
    {
        List<double> inputs = new List<double>();
        List<double> outputValues = new List<double>();
        int currentInput = 0;

        if (inputValues.Count != numInputs)
        {
            Debug.Log("ERROR: Number of Inputs must be " + numInputs);
            return outputValues;
        }

        inputs = new List<double>(inputValues);
        for (int i = 0; i < numHidden + 1; i++)
        {
            if (i > 0)
            {
                inputs = new List<double>(outputValues);
            }
            outputValues.Clear();

            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                double sum = 0;
                layers[i].neurons[j].inputs.Clear();

                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    layers[i].neurons[j].inputs.Add(inputs[currentInput]);
                    sum += layers[i].neurons[j].weights[k] * inputs[currentInput];
                    currentInput++;
                }

                sum -= layers[i].neurons[j].bias;

                if (i == numHidden)
                    layers[i].neurons[j].output = ActivationFunctionOutput(sum);
                else
                    layers[i].neurons[j].output = ActivationFunction(sum);

                outputValues.Add(layers[i].neurons[j].output);
                currentInput = 0;
            }
        }
        return outputValues;
    }

    private void UpdateWeights(List<double> outputs, List<double> desiredOutput)
    {
        double error;
        for (int i = numHidden; i >= 0; i--)
        {
            for (int j = 0; j < layers[i].numNeurons; j++)
            {
                if (i == numHidden)
                {
                    error = desiredOutput[j] - outputs[j];
                    layers[i].neurons[j].error = outputs[j] * (1 - outputs[j]) * error;
                }
                else
                {
                    layers[i].neurons[j].error = layers[i].neurons[j].output * (1 - layers[i].neurons[j].output);
                    double errorTotal = 0;
                    for (int p = 0; p < layers[i + 1].numNeurons; p++)
                    {
                        errorTotal += layers[i + 1].neurons[p].error * layers[i + 1].neurons[p].weights[j];
                    }
                    layers[i].neurons[j].error *= errorTotal;
                }
                for (int k = 0; k < layers[i].neurons[j].numInputs; k++)
                {
                    if (i == numHidden)
                    {
                        error = desiredOutput[j] - outputs[j];
                        layers[i].neurons[j].weights[k] += learnRate * layers[i].neurons[j].inputs[k] * error;
                    }
                    else
                    {
                        layers[i].neurons[j].weights[k] += learnRate * layers[i].neurons[j].inputs[k] * layers[i].neurons[j].error;
                    }
                }
                layers[i].neurons[j].bias += learnRate * -1 * layers[i].neurons[j].error;
            }
        }
    }

    private double ActivationFunction(double value)
    {
        return TanH(value);
    }

    private double ActivationFunctionOutput(double value)
    {
        return TanH(value);
    }

    private double TanH(double value)
    {
        double k = (double)System.Math.Exp(-2 * value);
        return 2 / (1.0f + k) - 1;
    }

    private double ReLu(double value)
    {
        if (value > 0) return value;
        else return 0;
    }

    private double LeakyReLu(double value)
    {
        if (value < 0) return 0.01 * value;
        else return value;
    }

    private double Sigmoid(double value)
    {
        double k = (double)System.Math.Exp(value);
        return k / (1.0f + k);
    }
}
