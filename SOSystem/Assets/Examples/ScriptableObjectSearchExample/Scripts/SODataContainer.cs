using Data;
using UnityEngine;

//Disable the assigned but never used warning since this script is purely used as an example for the search reference window
#pragma warning disable CS0414
[CreateAssetMenu(menuName = "Examples/SOData")]
public class SODataContainer : ScriptableObject
{
    [SerializeField] private SOBaseData m_Data = null;
    [SerializeField] private SOBaseData[] m_DataArray = null;
    [SerializeField] private SerializedDataClassA m_NestedDataClass = null;
}
#pragma warning restore CS0414