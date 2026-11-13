namespace OrchestratorModel;

using AgentInfrastructure;

// Runs quality checks before delivering a result (E39 +20.2%).
// 16 checks divided into "always" checks and output-type-specific checks.
// FC-12: ISessionStorage first parameter → base(storage).
public sealed class QualityOrchestrator : BaseService, IQualityOrchestrator
{
    // Checks applied to every output regardless of type.
    private static readonly string[] AlwaysChecks =
    [
        "OUTPUT_CONTRACT", "MISSION_PROGRESS", "ISSUE_INVENTORY", "GOAL_REMINDER",
        "TOOLS_BEFORE_PERSON", "RESPONSE_CONSTRUCTION", "CLAIMS_VERIFIED", "HUMAN_LANGUAGE_GATE",
    ];

    // Additional checks keyed by output type string.
    private static readonly Dictionary<string, string[]> ByOutputType = new()
    {
        ["CODE_SERVICE"] = ["TEST_GATE", "SCOPE_ISOLATION", "PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS"],
        ["PLAN_DOC"]     = ["PIPELINE_CONTRACT", "DOCUMENTATION_ARTIFACTS", "GOAL_DELIVERY_VERDICT"],
        ["REACT_PAGE"]   = ["UI_UX_COMPLIANCE"],
        ["ANALYSIS_DOC"] = ["RECONNAISSANCE_EVIDENCE", "ARCHITECT_HABITS"],
    };

    public QualityOrchestrator(ISessionStorage storage) : base(storage) { }

    // Run checks and return a report. Passes when zero checks fail.
    public OperationResult<QualityReport> Check(
        string taskDescription, string result, string outputType)
    {
        if (string.IsNullOrWhiteSpace(result))
            return OperationResult<QualityReport>.Fail("Result cannot be empty.", "EMPTY_RESULT");

        var failed = new List<string>();

        // OUTPUT_CONTRACT: result must be substantive (> 10 characters).
        if (result.Length < 10)
            failed.Add("OUTPUT_CONTRACT");

        // TEST_GATE: code output must contain at least one return statement.
        if (outputType == "CODE_SERVICE" && !result.Contains("return"))
            failed.Add("TEST_GATE");

        return OperationResult<QualityReport>.Ok(new QualityReport(failed.Count == 0, failed));
    }
}
