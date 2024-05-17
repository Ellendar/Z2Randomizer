
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace RandomizerCore.Asm;

public class Assembler
{
    public List<AsmModule> Modules { get; } = new();

    public void Add(AsmModule asmModule)
    {
        Modules.Add(asmModule);
    }

    public AsmModule Module()
    {
        var mod = new AsmModule();
        Add(mod);
        return mod;
    }
}

public static class AssemblerExtensions
{
    public static List<List<ExpandoObject>> AsExpando(this Assembler a)
    {
        var modules = new List<List<ExpandoObject>>();
        foreach (var module in a.Modules)
        {
            var outmodule = new List<ExpandoObject>();
            foreach (var dict in module.Actions)
            {
                outmodule.Add(dict.ToExpandoObject());
            }
            modules.Add(outmodule);
        }
        return modules;
    }
}

internal static class DictionaryExtensions
{
    public static ExpandoObject ToExpandoObject(this IDictionary<string, object> dictionary)
    {
        var bag = new ExpandoObject();
        var dict = bag as IDictionary<string, object>;
        foreach (var kvp in dictionary)
        {
            switch (kvp.Value)
            {
                case IDictionary<string, object> objects:
                {
                    var inner = objects.ToExpandoObject() as IDictionary<string, object>;
                    dict.Add(kvp.Key, inner);
                    break;
                }
                case ICollection list:
                {
                    var itemList = new List<object>();
                    foreach (var item in list)
                    {
                        if (item is IDictionary<string, object> objs)
                        {
                            var bagitem = objs.ToExpandoObject();
                            itemList.Add(bagitem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    dict.Add(kvp.Key, itemList);
                    break;
                }
                default:
                    dict.Add(kvp.Key, kvp.Value);
                    break;
            }
        }

        return bag;
    }
}