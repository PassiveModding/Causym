using System;
using System.Collections.Generic;
using System.Text;

namespace Causym.Services.Help
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ModuleButtonAttribute : Attribute
    {
        public readonly string ButtonCode;

        public ModuleButtonAttribute(string buttonCode)
        {
            this.ButtonCode = buttonCode;
        }
    }
}
