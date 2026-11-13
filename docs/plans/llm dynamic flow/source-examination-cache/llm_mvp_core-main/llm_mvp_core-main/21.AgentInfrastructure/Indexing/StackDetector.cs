namespace AgentInfrastructure;

public interface IStackDetector
{
    OperationResult<StackInfo> Detect(string sourceText);
}

public record StackInfo(StackLanguage Language, string[] Markers);

public enum StackLanguage { CSharp, TypeScript, Python, Unknown }

// Detects programming language from source markers — run BEFORE any text normalization (E20 Scenario B).
// Normalization destroys 72% of language markers (Task<→task, Promise→promis).
public sealed class StackDetector : IStackDetector
{
    private static readonly string[] CSharpMarkers =
        ["Task<", "IEnumerable", "namespace ", "using ", " var ", ": string", "async ", "await ",
         "{ get; set; }", "public class ", "= string.Empty"];

    private static readonly string[] TypeScriptMarkers =
        ["Promise<", "interface ", "const ", "export ", "import ", " => ", ": void"];

    private static readonly string[] PythonMarkers =
        ["def ", "class ", "import ", "self.", "print(", "__name__", "    "];

    public OperationResult<StackInfo> Detect(string sourceText)
    {
        int csScore = CSharpMarkers.Count(m => sourceText.Contains(m));
        int tsScore = TypeScriptMarkers.Count(m => sourceText.Contains(m));
        int pyScore = PythonMarkers.Count(m => sourceText.Contains(m));

        if (csScore == 0 && tsScore == 0 && pyScore == 0)
            return OperationResult<StackInfo>.Ok(new StackInfo(StackLanguage.Unknown, []));

        var language = (csScore >= tsScore && csScore >= pyScore) ? StackLanguage.CSharp
                     : (tsScore >= pyScore)                       ? StackLanguage.TypeScript
                     :                                              StackLanguage.Python;

        return OperationResult<StackInfo>.Ok(new StackInfo(language, []));
    }
}
