using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RichTextAttribute : Attribute
{
    public bool Editable { get; }
    public string FieldName { get; }

    public RichTextAttribute(bool editable = false)
    {
        Editable = editable;
    }

    public RichTextAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}