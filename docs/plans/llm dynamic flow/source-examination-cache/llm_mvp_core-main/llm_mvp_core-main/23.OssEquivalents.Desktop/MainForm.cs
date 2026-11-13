using System.Text;
using System.Windows.Forms;

namespace OssEquivalents.Desktop;

// ─── MainForm — desktop host for sub-project 23 ─────────────────────────────
// Three demo entries cover the headline OSS-equivalent components:
//   * Machine Control Protocol dispatcher (MCP)
//   * SKILL.md / Markdown frontmatter parser
//   * Schema validator + StructuredLlmClient retry loop
//   * Agent state machine + checkpointer (SQLite-backed)
// Each demo is self-contained and uses only in-memory or temp-file artifacts.

public sealed class MainForm : Form
{
    private readonly ListBox _demoList = new();
    private readonly Button  _runButton = new();
    private readonly TextBox _outputBox = new();
    private readonly StatusStrip   _status      = new();
    private readonly ToolStripStatusLabel _statusLabel = new("Ready");

    private readonly List<(string Name, Func<Task<string>> Run)> _demos;

    public MainForm()
    {
        _demos =
        [
            ("MCP — Tool registration + dispatch over JSON",      RunMcpDemoAsync),
            ("Frontmatter — gray-matter equivalent parsing",      RunFrontmatterDemoAsync),
            ("Schema validator — zod equivalent + retry loop",    RunSchemaValidatorDemoAsync),
            ("State machine — LangGraph equivalent + SQLite ckpt",RunStateMachineDemoAsync),
        ];

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        Text          = "Sub-project 23 — OSS Equivalents Explorer";
        Width         = 1100;
        Height        = 720;
        StartPosition = FormStartPosition.CenterScreen;

        var split = new SplitContainer
        {
            Dock             = DockStyle.Fill,
            Orientation      = Orientation.Vertical,
            SplitterDistance = 380,
            FixedPanel       = FixedPanel.Panel1,
        };

        _demoList.Dock           = DockStyle.Fill;
        _demoList.IntegralHeight = false;
        foreach (var (name, _) in _demos) _demoList.Items.Add(name);
        if (_demoList.Items.Count > 0) _demoList.SelectedIndex = 0;

        _runButton.Text   = "Run selected demo";
        _runButton.Dock   = DockStyle.Bottom;
        _runButton.Height = 36;
        _runButton.Click += async (_, _) => await RunSelectedAsync();

        split.Panel1.Controls.Add(_demoList);
        split.Panel1.Controls.Add(_runButton);

        _outputBox.Dock       = DockStyle.Fill;
        _outputBox.Multiline  = true;
        _outputBox.ReadOnly   = true;
        _outputBox.ScrollBars = ScrollBars.Both;
        _outputBox.WordWrap   = false;
        _outputBox.Font       = new System.Drawing.Font("Consolas", 9.5f);
        split.Panel2.Controls.Add(_outputBox);

        _status.Items.Add(_statusLabel);
        Controls.Add(split);
        Controls.Add(_status);
    }

    private async Task RunSelectedAsync()
    {
        int index = _demoList.SelectedIndex;
        if (index < 0 || index >= _demos.Count) return;

        var (name, runner) = _demos[index];
        _runButton.Enabled = false;
        _statusLabel.Text  = $"Running {name}...";
        _outputBox.Text    = $"=== {name} ===\r\n\r\n";

        try
        {
            string result = await runner();
            _outputBox.AppendText(result);
            _statusLabel.Text = "Done.";
        }
        catch (Exception ex)
        {
            _outputBox.AppendText($"[unexpected exception]\r\n{ex}");
            _statusLabel.Text = "Failed.";
        }
        finally
        {
            _runButton.Enabled = true;
        }
    }

    // ─── Demo 1 — MCP dispatcher ────────────────────────────────────────────

    private async Task<string> RunMcpDemoAsync()
    {
        var dispatcher = new McpToolDispatcher();

        // Register a single read-only "ping" tool that echoes its arguments back.
        dispatcher.Register(
            new McpToolDefinition(
                Name:             "ping",
                Description:      "Echoes the arguments back to the caller",
                ParameterSchema:  new Dictionary<string, object> { ["text"] = "string" }),
            args => Task.FromResult(new Dictionary<string, object>
            {
                ["echo"]      = args.GetValueOrDefault("text") ?? "<no text>",
                ["timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
            }));

        var output = new StringBuilder();
        output.AppendLine($"Registered tools: {dispatcher.Definitions.Count}");
        foreach (var definition in dispatcher.Definitions)
            output.AppendLine($"  - {definition.Name}: {definition.Description}");
        output.AppendLine();

        // Dispatch a well-formed request to the registered tool.
        string okJson = "{\"toolName\":\"ping\",\"arguments\":{\"text\":\"hello\"}}";
        var okResult  = await dispatcher.DispatchAsync(okJson);
        output.AppendLine($"Dispatch (known tool): {okResult.Value}");

        // Dispatch a request for an unknown tool — should produce an error response.
        string unknownJson = "{\"toolName\":\"missing\",\"arguments\":{}}";
        var unknownResult  = await dispatcher.DispatchAsync(unknownJson);
        output.AppendLine($"Dispatch (unknown):   {unknownResult.Value}");

        // Dispatch invalid JSON — should produce a parse failure result.
        var malformedResult = await dispatcher.DispatchAsync("{not valid json");
        output.AppendLine(malformedResult.IsSuccess
            ? $"Dispatch (malformed): unexpectedly succeeded with {malformedResult.Value}"
            : $"Dispatch (malformed): failure {malformedResult.ErrorCode} — {malformedResult.ErrorMessage}");

        return output.ToString();
    }

    // ─── Demo 2 — frontmatter parser (gray-matter equivalent) ──────────────

    private Task<string> RunFrontmatterDemoAsync()
    {
        var parser = new FrontmatterParser();

        string sample = """
            ---
            name: example-skill
            description: Demonstrates the FrontmatterParser
            version: 2.1
            ---
            # Skill body

            This text appears AFTER the frontmatter delimiter and is treated as the
            content portion of the SKILL.md file.
            """;

        var result = parser.Parse(sample);
        if (!result.IsSuccess)
            return Task.FromResult($"Failed: {result.ErrorMessage} ({result.ErrorCode})");

        var (metadata, content) = result.Value;

        var sb = new StringBuilder();
        sb.AppendLine($"Metadata fields ({metadata.Count}):");
        foreach (var (key, value) in metadata)
            sb.AppendLine($"  {key,-12} = {value}");
        sb.AppendLine();
        sb.AppendLine("Content portion:");
        sb.AppendLine(content);

        // Also exercise a malformed input to show the error path.
        var malformedResult = parser.Parse("---\nname: forgot-to-close\n");
        sb.AppendLine();
        sb.AppendLine($"Malformed input: success={malformedResult.IsSuccess}, code={malformedResult.ErrorCode}");

        return Task.FromResult(sb.ToString());
    }

    // ─── Demo 3 — schema validator + structured LLM retry ──────────────────

    private async Task<string> RunSchemaValidatorDemoAsync()
    {
        var validator = new SchemaValidator();

        // Schema describing the expected output shape.
        var schema = new Dictionary<string, object>
        {
            ["name"]       = "string",
            ["confidence"] = "number",
            ["tags"]       = "array",
        };

        var goodValue = new Dictionary<string, object>
        {
            ["name"]       = "skill-A",
            ["confidence"] = 0.92,
            ["tags"]       = new List<string> { "demo", "validation" },
        };

        var badValue = new Dictionary<string, object>
        {
            ["name"]       = "skill-A",
            ["confidence"] = "ninety-two-percent", // wrong type
        };

        var sb = new StringBuilder();
        sb.AppendLine($"Valid object → IsSuccess = {validator.Validate(goodValue, schema).IsSuccess}");

        var invalidResult = validator.Validate(badValue, schema);
        sb.AppendLine($"Invalid object → IsSuccess = {invalidResult.IsSuccess}, code = {invalidResult.ErrorCode}");
        sb.AppendLine($"  message = {invalidResult.ErrorMessage}");
        sb.AppendLine();

        // Use the InMemoryStructuredLlmClient to demonstrate the schema-retry loop.
        // The client returns invalid JSON twice, then a valid object on the third call.
        var inMemoryClient = new InMemoryStructuredLlmClient(
        [
            "{\"name\": \"skill-A\"}",                                      // missing required field
            "{\"name\": \"skill-A\", \"confidence\": \"oops\"}",            // wrong type
            "{\"name\": \"skill-A\", \"confidence\": 0.91, \"tags\": []}",  // valid
        ]);

        var clientResult = await inMemoryClient.SendWithSchemaAsync(
            messageHistory:    new List<ConversationMessage> { new("user", "Pick a skill.") },
            expectedSchema:    schema,
            maximumRetryCount: 3);

        sb.AppendLine($"Structured LLM retry result: success = {clientResult.IsSuccess}");
        if (clientResult.IsSuccess)
            sb.AppendLine($"  validated dictionary keys: {string.Join(", ", clientResult.Value!.Keys)}");
        else
            sb.AppendLine($"  failure: {clientResult.ErrorMessage}");

        return sb.ToString();
    }

    // ─── Demo 4 — agent state machine + SQLite checkpointer ────────────────

    private async Task<string> RunStateMachineDemoAsync()
    {
        // SQLite checkpointer writes to a temp file scoped to this demo run.
        string tempDb = Path.Combine(Path.GetTempPath(), $"oss-23-demo-{Guid.NewGuid():N}.sqlite");

        await using var checkpointer = new StateCheckpointer(tempDb);
        var machine = new AgentStateMachine(checkpointer);

        // Three nodes: analyze → execute → respond.
        machine.AddNode(new AgentStateNode("analyze",
            state =>
            {
                var next = new Dictionary<string, object>(state) { ["plan"] = "do work" };
                return Task.FromResult(next);
            }));

        machine.AddNode(new AgentStateNode("execute",
            state =>
            {
                var next = new Dictionary<string, object>(state) { ["resultText"] = "done" };
                return Task.FromResult(next);
            }));

        machine.AddNode(new AgentStateNode("respond",
            state =>
            {
                var next = new Dictionary<string, object>(state) { ["finalAnswer"] = $"answer: {state["resultText"]}" };
                return Task.FromResult(next);
            }));

        // Linear conditional edges that always advance to the next stage.
        machine.AddConditionalEdge("analyze", _ => true, "execute");
        machine.AddConditionalEdge("execute", _ => true, "respond");
        machine.AddConditionalEdge("respond", _ => true, "done");

        var initialState = new Dictionary<string, object> { ["task"] = "demo task" };

        var result = await machine.RunAsync(initialState,
            startingStateName:  "analyze",
            terminalStateName:  "done");

        var sb = new StringBuilder();
        sb.AppendLine($"Run succeeded: {result.IsSuccess}");
        sb.AppendLine($"Checkpoints saved during run: {machine.CheckpointsSaved}");

        if (result.IsSuccess)
        {
            sb.AppendLine($"Final state keys: {string.Join(", ", result.Value!.Keys)}");
            sb.AppendLine($"finalAnswer = {result.Value!.GetValueOrDefault("finalAnswer")}");
        }
        else
        {
            sb.AppendLine($"Failure: {result.ErrorMessage} ({result.ErrorCode})");
        }

        // Read the latest checkpoint back out of SQLite to prove persistence.
        var latest = await checkpointer.LoadLatestAsync();
        if (latest.IsSuccess)
        {
            sb.AppendLine();
            sb.AppendLine($"Latest checkpoint state name: {latest.Value.StateName}");
            sb.AppendLine($"Latest checkpoint keys: {string.Join(", ", latest.Value.StateData.Keys)}");
        }

        // Best-effort cleanup of the temp database file.
        try { File.Delete(tempDb); } catch { /* ignore on Windows file lock */ }

        return sb.ToString();
    }
}
