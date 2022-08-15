﻿// ---------------------------------------------------------------------------------------
// Copyright (C) Riccardo De Agostini and contributors.
// Licensed under the MIT license.
// See the LICENSE file in the project root for full license information.
//
// Part of this file may be third-party code, distributed under a compatible license.
// See the THIRD-PARTY-NOTICES file in the project root for third-party copyright notices.
// ---------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

#pragma warning disable CA1050 // Declare types in namespaces

/// <summary>
/// Adds polyfill source files to a project.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class PolyfillGenerator : IIncrementalGenerator
{
    // All polyfill source files are saved as UTF-8 with BOM, with lines separated by CR+LF.
    // (see .editorconfig in the project root)
    private const string Header
        = "// <auto-generated>\r\n"
        + "// This file has been generated by PolyKit version " + ThisAssembly.InformationalVersion + "\r\n"
        + "// and is provided under one or more license agreements.\r\n"
        + "// Please see https://github.com/Buildvana/PolyKit for full license information.\r\n"
        + "// </auto-generated>\r\n"
        + "\r\n"
        + "#nullable enable\r\n"
        + "\r\n"
        + "#pragma warning disable RS0016 // Add public types and members to the declared API\r\n"
        + "\r\n";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterPostInitializationOutput(static ctx =>
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames()
                                .Where(n => n.EndsWith(".cs", StringComparison.Ordinal))
                                .Select(s => s.Substring(0, s.Length - 3));

            var sb = new StringBuilder(16 * 1024); // Avoid reallocations
            foreach (var name in names)
            {
                string source;
                using (var inStream = assembly.GetManifestResourceStream(name + ".cs"))
                using (var reader = new StreamReader(inStream, Encoding.UTF8))
                {
                    source = reader.ReadToEnd();
                }

                _ = sb.Append(Header).Append(source);
                ctx.AddSource(name + ".g.cs", sb.ToString());
                _ = sb.Clear();
            }
        });
}
