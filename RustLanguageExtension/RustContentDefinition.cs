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
        internal static ContentTypeDefinition RustContentTypeDefinition;

        [Export]
        [FileExtension(".rs")]
        [ContentType("rust")]
        internal static FileExtensionToContentTypeDefinition RustFileExtensionDefinition;
    }
}
