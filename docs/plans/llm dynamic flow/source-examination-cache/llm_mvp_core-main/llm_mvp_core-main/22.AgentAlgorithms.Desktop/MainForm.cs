using System.Windows.Forms;

namespace AgentAlgorithms.Desktop;

// ─── MainForm — desktop explorer for sub-project 22 ─────────────────────────
// Eight buttons, one per algorithm group from coding-agent-algorithms.jsx.
// Click a button → run a self-contained demo → see the output below.

public sealed class MainForm : Form
{
    private readonly ListBox _algorithmList = new();
    private readonly Button  _runButton     = new();
    private readonly TextBox _outputBox     = new();
    private readonly StatusStrip   _status      = new();
    private readonly ToolStripStatusLabel _statusLabel = new("Ready");

    private readonly List<(string Name, Func<Task<string>> Run)> _demos;

    public MainForm()
    {
        _demos =
        [
            ("01 — Pipeline (4-stage agent flow with mocks)",            RunPipelineDemoAsync),
            ("02 — RepoMap indexing (signatures + simplified PageRank)", RunRepoMapDemoAsync),
            ("03 — ReAct agent loop (mock LLM)",                         RunReactLoopDemoAsync),
            ("04 — Context compaction (long history → summary + tail)",  RunContextCompactionDemoAsync),
            ("05 — Multi-agent orchestrator (sub-task routing)",         RunOrchestratorDemoAsync),
            ("06 — Permission model (ReadOnly / WriteWithApproval / Denied)", RunPermissionModelDemoAsync),
            ("07 — Rules file parser (sections + @allow/@deny)",         RunRulesParserDemoAsync),
            ("08 — Complex task decomposition (and/then splitter)",      RunDecomposerDemoAsync),
        ];

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        Text          = "Sub-project 22 — Coding Agent Algorithms Explorer";
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

        _algorithmList.Dock           = DockStyle.Fill;
        _algorithmList.IntegralHeight = false;
        foreach (var (name, _) in _demos) _algorithmList.Items.Add(name);
        if (_algorithmList.Items.Count > 0) _algorithmList.SelectedIndex = 0;

        _runButton.Text   = "Run selected algorithm";
        _runButton.Dock   = DockStyle.Bottom;
        _runButton.Height = 36;
        _runButton.Click += async (_, _) => await RunSelectedAsync();

        split.Panel1.Controls.Add(_algorithmList);
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
        int index = _algorithmList.SelectedIndex;
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

    // ─── Algorithm 01 — pipeline ────────────────────────────────────────────

    private async Task<string> RunPipelineDemoAsync()
    {
        var symbols = new MockSymbolGraphProvider();
        var search  = new MockSemanticSearchProvider();
        var llm     = new MockLlmGateway("Plan: read auth.cs, then refactor login()");

        var pipeline = new AgentPipeline(symbols, search, llm);
        var result   = await pipeline.RunAsync("Refactor the login function to add error handling");

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        return $"Stage 1 (index): {symbols.Calls} call(s)\r\n" +
               $"Stage 2 (search): {search.Calls} call(s), top chunk = {search.LastTopChunk}\r\n" +
               $"Stage 3 (LLM): {llm.Calls} call(s)\r\n" +
               $"Pipeline summary: {result.Value!.Summary}\r\n" +
               $"Changed files: [{string.Join(", ", result.Value!.ChangedFiles)}]";
    }

    // ─── Algorithm 02 — repo-map indexing ───────────────────────────────────

    private async Task<string> RunRepoMapDemoAsync()
    {
        // Use the application base directory so the demo runs against real .cs files
        // shipped with the desktop assembly. (None expected, but the API is exercised.)
        var builder = new RepoMapBuilder();
        var result  = await builder.BuildAsync(AppContext.BaseDirectory);

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        var symbols = result.Value!;
        var preview = symbols
            .OrderByDescending(s => s.InDegreeScore)
            .Take(15)
            .Select(s => $"  score={s.InDegreeScore:F3}  {s.Signature}  in {Path.GetFileName(s.FilePath)}")
            .ToList();

        return $"Indexed {symbols.Count} method symbol(s).\r\n" +
               $"Top by simplified PageRank:\r\n{string.Join("\r\n", preview)}\r\n";
    }

    // ─── Algorithm 03 — ReAct agent loop ────────────────────────────────────

    private async Task<string> RunReactLoopDemoAsync()
    {
        var llm = new MockLlmGateway(
        [
            "Thinking about the task...",
            "TOOL:read_file:{\"path\":\"auth.cs\"}",
            "TASK_COMPLETE:Read the file successfully.",
        ]);

        var compactor = new ContextCompactor(recentTurnsToPreserve: 5);
        var loop      = new ReactAgentLoop(
            llm,
            compactor,
            toolExecutor: (name, args) => Task.FromResult(
                $"[mock-{name} returned: ok]"));

        var result = await loop.RunAsync("Read auth.cs and report what it does");
        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        var loopResult = result.Value!;
        return $"Iterations: {loopResult.TotalIterations}, complete = {loopResult.IsComplete}\r\n" +
               $"Final response: {loopResult.FinalResponse}\r\n\r\n" +
               $"History:\r\n" +
               string.Join("\r\n", loopResult.FinalHistory
                   .Select((m, i) => $"  {i:D2} [{m.Role}] {m.Content}"));
    }

    // ─── Algorithm 04 — context compaction ──────────────────────────────────

    private async Task<string> RunContextCompactionDemoAsync()
    {
        var compactor = new ContextCompactor(recentTurnsToPreserve: 3);

        // Build a 12-message history; 9 will be summarized, 3 kept verbatim.
        var history = Enumerable.Range(1, 12)
            .Select(turnNumber => new ConversationMessage(
                turnNumber % 2 == 0 ? "user" : "assistant",
                $"message #{turnNumber}"))
            .ToList();

        var result = await compactor.CompactAsync(
            history,
            messages => Task.FromResult($"summary of {messages.Count} earlier messages"));

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        var compacted = result.Value!;
        return $"Original message count: {history.Count}\r\n" +
               $"Compacted message count: {compacted.Count} (1 summary + 3 recent)\r\n\r\n" +
               $"Compacted history:\r\n" +
               string.Join("\r\n", compacted.Select((m, i) => $"  {i:D2} [{m.Role}] {m.Content}"));
    }

    // ─── Algorithm 05 — multi-agent orchestration ───────────────────────────

    private async Task<string> RunOrchestratorDemoAsync()
    {
        var bus        = new MockAgentMessageBus();
        var decomposer = new ComplexTaskDecomposer();
        var orchestrator = new AgentOrchestrator(bus, decomposer);

        orchestrator.Register(new SubAgentDefinition("ReaderBot",     "reader",     ["read_file", "search_code"]));
        orchestrator.Register(new SubAgentDefinition("RefactorerBot", "refactorer", ["edit_file", "apply_patch"]));

        var result = await orchestrator.RunAsync(
            "read the auth module and then refactor the login function");

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        return $"Orchestrator run complete.\r\n" +
               $"Sub-agent calls observed: {bus.Calls.Count}\r\n" +
               string.Join("\r\n", bus.Calls.Select(c => $"  {c.Agent} ← '{c.Task}'")) +
               $"\r\n\r\nCombined result:\r\n{result.Value}";
    }

    // ─── Algorithm 06 — permission model ────────────────────────────────────

    private Task<string> RunPermissionModelDemoAsync()
    {
        var model = new PermissionModel();

        var probes = new[] { "read_file", "list_directory", "search_code",
                             "edit_file", "apply_patch",
                             "delete_file", "run_command",
                             "unknown_tool" };

        var lines = probes.Select(toolName =>
        {
            var decision = model.Check(toolName);
            string text  = decision.IsSuccess ? decision.Value.ToString() : decision.ErrorMessage;
            return $"  {toolName,-16} → {text}";
        }).ToList();

        return Task.FromResult("Permission model decisions:\r\n" + string.Join("\r\n", lines));
    }

    // ─── Algorithm 07 — rules file parser ──────────────────────────────────

    private Task<string> RunRulesParserDemoAsync()
    {
        var parser = new RulesFileParser();

        const string content =
            "Always speak clearly.\n\n" +
            "# Style\n" +
            "Prefer short sentences.\n\n" +
            "# Tools\n" +
            "@allow read_file\n" +
            "@allow search_code\n" +
            "@deny  delete_file\n";

        var result = parser.Parse(content);
        if (!result.IsSuccess)
            return Task.FromResult($"Failed: {result.ErrorMessage} ({result.ErrorCode})");

        var parsed   = result.Value!;
        // The parser stores the text before any heading under the "preamble" key.
        string preamble = parsed.Sections.TryGetValue("preamble", out var p) ? p.Trim() : string.Empty;
        var nonPreamble = parsed.Sections.Where(section => section.Key != "preamble").ToList();

        return Task.FromResult(
            $"Preamble: {preamble}\r\n\r\n" +
            $"Sections ({nonPreamble.Count}):\r\n" +
            string.Join("\r\n", nonPreamble.Select(section => $"  [{section.Key}] {section.Value.Trim()}")) +
            "\r\n\r\n" +
            $"Allowed tools: [{string.Join(", ", parsed.AllowedTools)}]\r\n" +
            $"Denied tools:  [{string.Join(", ", parsed.DeniedTools)}]");
    }

    // ─── Algorithm 08 — complex task decomposer ─────────────────────────────

    private Task<string> RunDecomposerDemoAsync()
    {
        var decomposer = new ComplexTaskDecomposer();

        var result = decomposer.Decompose(
            "read the auth module and then refactor the login function and verify with tests");

        if (!result.IsSuccess)
            return Task.FromResult($"Failed: {result.ErrorMessage} ({result.ErrorCode})");

        var subTasks = result.Value!;
        return Task.FromResult(
            $"Sub-tasks ({subTasks.Count}):\r\n" +
            string.Join("\r\n", subTasks.Select((task, index) =>
                $"  {index:D2}. role={task.RequiredRole,-12} desc='{task.Description}'")));
    }
}

// ─── Mock fabric implementations used by the demos ──────────────────────────

internal sealed class MockSymbolGraphProvider : ISymbolGraphProvider
{
    public int Calls { get; private set; }

    public Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync()
    {
        Calls++;
        return Task.FromResult(OperationResult<List<Dictionary<string, object>>>.Ok(
            new List<Dictionary<string, object>>
            {
                new() { ["name"] = "VerifyCredentials", ["kind"] = "Method" },
                new() { ["name"] = "GenerateToken",     ["kind"] = "Method" },
            }));
    }

    public Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId)
        => Task.FromResult(OperationResult<List<Dictionary<string, object>>>.Ok(
            new List<Dictionary<string, object>>()));
}

internal sealed class MockSemanticSearchProvider : ISemanticSearchProvider
{
    public int     Calls         { get; private set; }
    public string  LastTopChunk  { get; private set; } = string.Empty;

    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
    {
        Calls++;
        var hits = new List<(string ChunkId, double Score)>
        {
            ("auth.cs#VerifyCredentials", 0.92),
            ("auth.cs#GenerateToken",     0.85),
        };
        LastTopChunk = hits[0].ChunkId;
        return Task.FromResult(OperationResult<List<(string, double)>>.Ok(hits));
    }
}

internal sealed class MockLlmGateway : ILlmGateway
{
    public int Calls { get; private set; }

    private readonly Queue<string> _responses;

    public MockLlmGateway(string singleResponse)
        => _responses = new Queue<string>([singleResponse]);

    public MockLlmGateway(IEnumerable<string> responses)
        => _responses = new Queue<string>(responses);

    public Task<OperationResult<string>> SendAsync(
        IReadOnlyList<ConversationMessage> history,
        CancellationToken cancellationToken = default)
    {
        Calls++;
        string response = _responses.Count > 0 ? _responses.Dequeue() : "TASK_COMPLETE:done";
        return Task.FromResult(OperationResult<string>.Ok(response));
    }
}

internal sealed class MockAgentMessageBus : IAgentMessageBus
{
    public List<(string Agent, string Task)> Calls { get; } = [];

    public Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task)
    {
        Calls.Add((subAgentName, task));
        return Task.FromResult(OperationResult<string>.Ok($"[{subAgentName} done: {task}]"));
    }
}
