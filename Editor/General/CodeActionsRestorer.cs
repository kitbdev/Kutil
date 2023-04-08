using System.Linq;
using System.Xml.Linq;
using UnityEditor;

namespace Kutil.Editor {
    /// <summary>
    /// Workaround to fix a bug that causes Quick Actions to not work
    /// https://github.com/OmniSharp/omnisharp-vscode/issues/5494
    /// https://forum.unity.com/threads/vs-22-throws-cs8032-after-updating-to-tech-stream-2022-2-0f1.1372701/#post-8715939
    /// </summary>
    public class CodeActionsRestorer : AssetPostprocessor {
        private static string OnGeneratedCSProject(string path, string content) {
            var document = XDocument.Parse(content);
            document.Root.Descendants()
                .Where(x => x.Name.LocalName == "Analyzer")
                .Where(x => x.Attribute("Include").Value.Contains("Unity.SourceGenerators"))
                .Remove();
            return document.Declaration + System.Environment.NewLine + document.Root;
        }
    }
}