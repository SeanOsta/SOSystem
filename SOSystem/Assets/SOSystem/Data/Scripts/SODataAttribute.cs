using System;

namespace Data
{
    public class SODataAttribute : Attribute
    {
        public Type m_DataType;
        public SODataAttribute(Type aDataType)
        {
            m_DataType = aDataType;
        }
    }
}