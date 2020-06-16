using System.Collections.Generic;

[System.Serializable]
public class Database
{
    public List<DataSet> dataSets = new List<DataSet>();

    public int Length => dataSets.Count;
}