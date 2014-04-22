﻿#region Copyright 2014 Exceptionless

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
//     http://www.apache.org/licenses/LICENSE-2.0

#endregion

using System;
using Exceptionless.Models;

namespace Exceptionless.Models.Legacy {
    public class Parameter {
        public Parameter() {
            ExtendedData = new DataDictionary();
            GenericArguments = new GenericArguments();
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeNamespace { get; set; }

        public string TypeFullName {
            get {
                if (String.IsNullOrEmpty(Name))
                    return "<UnknownType>";

                return !String.IsNullOrEmpty(TypeNamespace) ? String.Concat(TypeNamespace, ".", Type.Replace('+', '.')) : Type.Replace('+', '.');
            }
        }

        public DataDictionary ExtendedData { get; set; }
        public GenericArguments GenericArguments { get; set; }
    }
}