using Data;
using UnityEngine;

public class SOBehaviour : MonoBehaviour
{
    //Disable the assigned but never used warning since this script is purely used as an example for the search reference window
#pragma warning disable CS0414
    [SerializeField] private SOBaseData data;
    [SerializeField] private SOBaseData[] m_DataArray = null;
    [SerializeField] SerializedDataClassA m_NestedDataClass = null;
#pragma warning restore CS0414
}
