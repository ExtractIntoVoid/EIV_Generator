namespace EIV.Generator;

public class CustomItemStructure_Source
{
    public const string ReplaceBaseType = "{BaseType}";
    public const string ReplaceNewType = "{NewType}";
    public const string AddNameSpace = "{NameSpaceToUsing}";
    public const string Source =
"""
// <generated>
// This file has been auto generated using the EIV.Generator.
// </generated>
using {NameSpaceToUsing};
#pragma warning disable
namespace {NameSpaceToUsing}.CustomBase;

public abstract partial class {NewType}<MyStuct> : {BaseType} where MyStuct : struct
{
    public MyStuct CustomStruct { get; internal set; }

    public override void _Ready()
    {
        base._Ready();
        CreateStruct();
    }

    public virtual void CreateStruct()
    {
        CustomStruct = new();
    }
}
""";

}
