using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;

internal sealed class EnsureCSharp8Postprocessor : AssetPostprocessor
{
    private static string OnGeneratedCSProject(string path, string content)
    {
        var document = XDocument.Parse(content);

        var root = document.Root;
        if (root is null)
            throw new ArgumentNullException(nameof(root));

        var ns = root.GetDefaultNamespace();

        var element = root.Descendants(ns + "LangVersion").SingleOrDefault();
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        element.Value = "8.0";

        using (var writer = new Utf8StringWriter())
        {
            document.Save(writer);

            content = writer.ToString();
        }

        return content;
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}