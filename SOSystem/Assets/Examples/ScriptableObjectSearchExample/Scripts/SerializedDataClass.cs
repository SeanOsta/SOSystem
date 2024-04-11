using Data;
using UnityEngine;

//Disable the assigned but never used warning since this script is purely used as an example for the search reference window
#pragma warning disable CS0414
[System.Serializable]
public class SerializedDataClassA
{
    [SerializeField] private SOBaseData m_Data = null;
    [SerializeField] private SOBaseData[] m_DataArray = null;
    [SerializeField] private SerializeDataClassB m_NestedDataClass = null;
}

[System.Serializable]
public class SerializeDataClassB
{
    [SerializeField] private SOBaseData m_Data = null;
    [SerializeField] private SOBaseData[] m_DataArray = null;
}
#pragma warning restore CS0414