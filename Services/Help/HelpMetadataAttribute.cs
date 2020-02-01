using System;
using System.Collections.Generic;
using System.Text;
using Disqord;

namespace Causym.Services.Help
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HelpMetadataAttribute : Attribute
    {
        public HelpMetadataAttribute(string buttonCode, string colorHex = null)
        {
            this.ButtonCode = buttonCode;
            Color = colorHex == null ? Color.Aquamarine : Extensions.ColorConvert(colorHex) ?? Color.Aquamarine;
        }

        public string ButtonCode { get; }

        public Color Color { get; }
    }
}
