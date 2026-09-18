#:package System.Reflection.Metadata@10.0.0

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

// Dumps public type names from an assembly, optionally filtered by substring.
// Usage: dotnet run typedump.cs -- <path-to-dll> [filter]

var dll = args[0];
var filter = args.Length > 1 ? args[1] : null;

using var fs = File.OpenRead(dll);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();

foreach (var handle in md.TypeDefinitions)
{
    var t = md.GetTypeDefinition(handle);
    var ns = md.GetString(t.Namespace);
    var name = md.GetString(t.Name);
    var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;

    if (!t.Attributes.HasFlag(System.Reflection.TypeAttributes.Public))
    {
        continue;
    }

    if (filter is null || full.Contains(filter, StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine(full);
    }
}

foreach (var handle in md.ExportedTypes)
{
    var t = md.GetExportedType(handle);
    var ns = md.GetString(t.Namespace);
    var name = md.GetString(t.Name);
    var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;

    if (filter is null || full.Contains(filter, StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[forwarded] " + full);
    }
}
