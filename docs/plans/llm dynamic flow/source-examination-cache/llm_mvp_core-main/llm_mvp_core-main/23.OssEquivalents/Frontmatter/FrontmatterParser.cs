using YamlDotNet.Serialization;

namespace OssEquivalents;

// Splits a Markdown file with YAML frontmatter into structured metadata and plain content.
// This is the .NET equivalent of the 'gray-matter' npm package.
//
// A frontmatter block is delimited by "---" on the first line and another "---" line.
// Everything between the delimiters is YAML; everything after is the Markdown content.
public sealed class FrontmatterParser
{
    private static readonly IDeserializer _yaml = new DeserializerBuilder().Build();

    // Parse a Markdown file that may contain a YAML frontmatter block.
    public OperationResult<(Dictionary<string, object> Metadata, string Content)> Parse(
        string markdownText)
    {
        if (string.IsNullOrEmpty(markdownText))
            return OperationResult<(Dictionary<string, object>, string)>.Ok(
                (new Dictionary<string, object>(), string.Empty));

        // Normalize line endings.
        var lines = markdownText.Replace("\r\n", "\n").Split('\n');

        if (lines.Length == 0 || lines[0].Trim() != "---")
        {
            // No frontmatter — return empty metadata and the full text.
            return OperationResult<(Dictionary<string, object>, string)>.Ok(
                (new Dictionary<string, object>(), markdownText));
        }

        // Find the closing "---" delimiter.
        int closeIdx = -1;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---") { closeIdx = i; break; }
        }

        if (closeIdx < 0)
            return OperationResult<(Dictionary<string, object>, string)>.Fail(
                "Frontmatter opened with '---' but never closed.", "MALFORMED_FRONTMATTER");

        string yamlText    = string.Join('\n', lines[1..closeIdx]);
        string contentText = string.Join('\n', lines[(closeIdx + 1)..]).TrimStart('\n');

        Dictionary<string, object> metadata;
        try
        {
            metadata = _yaml.Deserialize<Dictionary<string, object>>(yamlText)
                       ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            return OperationResult<(Dictionary<string, object>, string)>.Fail(
                $"YAML parse error: {ex.Message}", "YAML_ERROR");
        }

        return OperationResult<(Dictionary<string, object>, string)>.Ok((metadata, contentText));
    }
}
