using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Marten.AnalyzerTool.Infrastructure
{
    public sealed class MartenContext
    {        
        public readonly Version Version;
        public MartenContext(Compilation compilation)
        {            
            Version = compilation.ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals(Constants.MartenAssembly, StringComparison.OrdinalIgnoreCase))
                ?.Version;            
        }
    }
}