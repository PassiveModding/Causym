using System;
using System.Collections.Generic;
using System.Text;

namespace Causym.Services.Help
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleButtonAttribute : Attribute
    {
        public ModuleButtonAttribute(string buttonCode)
        {
            this.ButtonCode = buttonCode;
        }

        public string ButtonCode { get; }
    }
}
