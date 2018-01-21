using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    public class RustContentDefinition
    {
        [Export]
        [Name("rust")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
#pragma warning disable CS0649
        internal static ContentTypeDefinition RustContentTypeDefinition;
#pragma warning restore CS0649

        [Export]
        [FileExtension(".rs")]
        [ContentType("rust")]
#pragma warning disable CS0649
        internal static FileExtensionToContentTypeDefinition RustFileExtensionDefinition;
#pragma warning restore CS0649
    }
}
