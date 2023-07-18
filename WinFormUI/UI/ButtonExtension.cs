using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.WinFormUI;

internal static class ButtonExtension
{
    internal static bool IsEllipsisShown(this Button @this)
    {
        Size sz = TextRenderer.MeasureText(@this.Text, @this.Font, @this.Size, TextFormatFlags.NoClipping);
        return (sz.Width >= (@this.ClientRectangle.Size.Width - (@this.Padding.Left+@this.Padding.Right)) - 6);
    }
}
