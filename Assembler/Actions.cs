

using Microsoft.ClearScript;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text;
using System.Xml.Linq;

namespace Assembler;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Actions : List<PropertyBag>
{
    public string GetDebuggerDisplay()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append('[');
        int count = 0;
        foreach (PropertyBag propertyBag in this) 
        {
            count++;
            sb.Append('{');
            int count2 = 0;
            foreach(KeyValuePair<string, object> property in propertyBag)
            {
                sb.Append(property.Key + " : " + property.Value + ",");
                count2++;
            }
            if(count2 > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("},");
        }
        if (count > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        sb.Append(']');

        return sb.ToString();
    }
}
