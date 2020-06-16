
using System.Collections.Generic;

[System.Serializable]
public class DataSet
{
    public List<double> inputs;
    public List<double> outputs;

    public DataSet(List<double> inputs, List<double> outputs)
    {
        this.inputs = inputs;
        this.outputs = outputs;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;

        DataSet other = obj as DataSet;
        for (int i = 0; i < inputs.Count; i++)
        {
            if (System.Math.Abs(inputs[i] - other.inputs[i]) > 0.01)
                return false;
        }

        for (int i = 0; i < outputs.Count; i++)
        {
            if (System.Math.Abs(outputs[i] - other.outputs[i]) > 0.01)
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hashCode = 11;
        hashCode = hashCode * 13 + inputs.GetHashCode();
        hashCode = hashCode * 13 + outputs.GetHashCode();
        return hashCode;
    }
}
