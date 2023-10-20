using System;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore.Flags;

internal class CustomFlagSerializerAttribute : Attribute
{
    public Type Type { get; }
    public CustomFlagSerializerAttribute(Type type)
    {
        if(!typeof(IFlagSerializer).IsAssignableFrom(type))
        {
            throw new ArgumentException("Flag serializer type must implement IFlagSerializer");
        }
        Type = type;
    }
}
