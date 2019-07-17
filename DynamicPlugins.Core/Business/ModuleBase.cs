﻿using DynamicPlugins.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicPlugins.Core.Business
{
    public class ModuleBase : IModule
    {
        public ModuleBase(string name)
        {
            Name = name;
            Version = "1.0.0";
        }

        public ModuleBase(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public ModuleBase(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public string Name
        {
            get;
            private set;
        }

        public Version Version
        {
            get;
            private set;
        }

        public string DllPath { get; set; }

        public string ViewDllPath { get; set; }
    }
}