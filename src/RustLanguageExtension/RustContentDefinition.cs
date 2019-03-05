// <copyright file="RustContentDefinition.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.LanguageServer.Client;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// Class containing the content type definitions for Rust files.
    /// </summary>
    public class RustContentDefinition
    {
        /// <summary>
        /// Gets the content type definition that ties the rust content type to the language server content type.
        /// </summary>
        [Export]
        [Name("rust")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
#pragma warning disable CS0649
        internal static ContentTypeDefinition RustContentTypeDefinition { get; }
#pragma warning restore CS0649

        /// <summary>
        /// Gets the file extension definition that ties .rs files to the rust content type.
        /// </summary>
        [Export]
        [FileExtension(".rs")]
        [ContentType("rust")]
#pragma warning disable CS0649
        internal static FileExtensionToContentTypeDefinition RustFileExtensionDefinition { get; }
#pragma warning restore CS0649
    }
}
