using System.Windows.Forms;

namespace AgentInfrastructure.Desktop;

// ─── MainForm — the desktop host for sub-project 21 ─────────────────────────
// The form lets the user pick one of seven agent stages from the left panel
// and run a canned demo for that stage on the right panel. The output is a
// plain-text log so the user can see exactly what the algorithm produced
// without any extra formatting. No business logic lives here — every action
// delegates to the AgentInfrastructure class library.

public sealed class MainForm : Form
{
    // Left panel: list of selectable demo stages.
    private readonly ListBox _stageList = new();

    // Right panel: read-only multi-line text box showing the demo output.
    private readonly TextBox _outputBox = new();

    // The button under the list that runs the currently selected demo.
    private readonly Button _runButton = new();

    // Status strip at the bottom shows what the form is currently doing.
    private readonly StatusStrip   _status      = new();
    private readonly ToolStripStatusLabel _statusLabel = new("Ready");

    // The list of demos paired with the async function that produces text output.
    // Each tuple is (display name, demo function returning the multi-line result).
    private readonly List<(string Name, Func<Task<string>> Run)> _demos;

    // The directory used as the "project root" for every demo so all path-based
    // permission checks work even though the demos are running from disposable
    // in-memory state. We point at the application base directory.
    private readonly string _projectRoot = AppContext.BaseDirectory;

    public MainForm()
    {
        // Build the demo list before the constructor wires the controls.
        _demos =
        [
            ("Stage 01 — Configuration loader (deep merge of three layers)",
             RunConfigurationLoaderDemoAsync),
            ("Stage 02 — Codebase indexer (Roslyn symbol extraction)",
             RunCodebaseIndexerDemoAsync),
            ("Stage 03 — Context assembler (token budget + ranking)",
             RunContextAssemblerDemoAsync),
            ("Stage 04 — Agent loop (ReAct pattern with mock LLM)",
             RunAgentLoopDemoAsync),
            ("Stage 05 — Tool system (registry + permission gate)",
             RunToolSystemDemoAsync),
            ("Stage 06 — File change applier (search-and-replace patches)",
             RunFileChangeApplierDemoAsync),
            ("Stage 07 — Session state store (in-memory ISessionStorage)",
             RunSessionStateStoreDemoAsync),
        ];

        InitializeLayout();
    }

    // ─── Layout setup ───────────────────────────────────────────────────────

    private void InitializeLayout()
    {
        // Window properties.
        Text          = "Sub-project 21 — Agent Infrastructure Stage Explorer";
        Width         = 1100;
        Height        = 720;
        StartPosition = FormStartPosition.CenterScreen;

        // The split container divides the form into left (list+button) and right (output).
        var split = new SplitContainer
        {
            Dock             = DockStyle.Fill,
            Orientation      = Orientation.Vertical,
            SplitterDistance = 380,
            FixedPanel       = FixedPanel.Panel1,
        };

        // Left panel — list of stages with a Run button below it.
        _stageList.Dock           = DockStyle.Fill;
        _stageList.IntegralHeight = false;
        foreach (var (name, _) in _demos) _stageList.Items.Add(name);
        if (_stageList.Items.Count > 0) _stageList.SelectedIndex = 0;

        _runButton.Text   = "Run selected stage";
        _runButton.Dock   = DockStyle.Bottom;
        _runButton.Height = 36;
        _runButton.Click += async (_, _) => await RunSelectedAsync();

        split.Panel1.Controls.Add(_stageList);
        split.Panel1.Controls.Add(_runButton);

        // Right panel — output text box (monospaced for readable algorithm output).
        _outputBox.Dock       = DockStyle.Fill;
        _outputBox.Multiline  = true;
        _outputBox.ReadOnly   = true;
        _outputBox.ScrollBars = ScrollBars.Both;
        _outputBox.WordWrap   = false;
        _outputBox.Font       = new System.Drawing.Font("Consolas", 9.5f);
        split.Panel2.Controls.Add(_outputBox);

        // Status strip at the bottom of the form.
        _status.Items.Add(_statusLabel);

        // The form composes the split container in the centre and the status strip below.
        Controls.Add(split);
        Controls.Add(_status);
    }

    // Run the demo currently highlighted in the list, displaying any failures
    // in the output box rather than throwing an exception.
    private async Task RunSelectedAsync()
    {
        int index = _stageList.SelectedIndex;
        if (index < 0 || index >= _demos.Count) return;

        var (name, runner) = _demos[index];

        _runButton.Enabled = false;
        _statusLabel.Text  = $"Running {name}...";
        _outputBox.Text    = $"=== {name} ===\r\n\r\n";

        try
        {
            // Run the demo and append its output to the box.
            string result = await runner();
            _outputBox.AppendText(result);
            _statusLabel.Text = "Done.";
        }
        catch (Exception ex)
        {
            // Any unexpected exception is shown in the output instead of crashing.
            _outputBox.AppendText($"[unexpected exception]\r\n{ex}");
            _statusLabel.Text = "Failed.";
        }
        finally
        {
            _runButton.Enabled = true;
        }
    }

    // ─── Stage 01 demo — layered configuration merge ───────────────────────

    private async Task<string> RunConfigurationLoaderDemoAsync()
    {
        // Set up an in-memory IConfigurationProvider with three layers. The loader
        // looks up the global, project, and AGENTS.md files at hard-coded relative
        // paths inside the project root, so we synthesize those paths here.
        string globalPath  = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "agent", "agent.json");
        string projectPath = Path.Combine(_projectRoot, ".agent", "agent.json");
        string agentsPath  = Path.Combine(_projectRoot, "AGENTS.md");

        var provider = new InMemoryConfigurationProvider();
        provider.SetJson(globalPath,  "{\"model\":\"claude-3\",\"limits\":{\"maxTokens\":8000}}");
        provider.SetJson(projectPath, "{\"limits\":{\"maxTokens\":16000,\"timeoutSec\":30}}");
        provider.SetText(agentsPath,  "Always prefer correct code over fast code.");

        var loader = new ConfigurationLoader(provider);
        var result = await loader.LoadAsync(_projectRoot);

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        return $"Merged configuration keys: {string.Join(", ", result.Value!.Keys)}\r\n" +
               $"limits.maxTokens  = {DigInto(result.Value!, "limits", "maxTokens")} (project overrode global)\r\n" +
               $"limits.timeoutSec = {DigInto(result.Value!, "limits", "timeoutSec")} (project-only key kept)\r\n" +
               $"systemInstructions = {result.Value!.GetValueOrDefault("systemInstructions")}\r\n";
    }

    // ─── Stage 02 demo — Roslyn codebase indexer ────────────────────────────

    private async Task<string> RunCodebaseIndexerDemoAsync()
    {
        // The indexer requires an ICodeReader; we supply a tiny in-memory project
        // with a couple of canned C# source files so the demo runs without disk I/O.
        var reader = new InMemoryCodeReader();
        reader.Set("calculator.cs", """
            public sealed class Calculator
            {
                public int Add(int x, int y) => x + y;
                public int Multiply(int x, int y) => Add(x, x) + Add(y, y);
            }
            """);
        reader.Set("logger.cs", """
            public sealed class Logger
            {
                public void Info(string message) { /* write to console */ }
                public void Warn(string message) => Info("WARN: " + message);
            }
            """);

        var indexer = new CodebaseIndexer(reader);
        var result  = await indexer.IndexDirectoryAsync(_projectRoot);

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        var graph    = result.Value!;
        var symbols  = graph.AllNodes;
        var preview  = symbols
            .Take(15)
            .Select(node => $"  {node.Kind,-7} {node.Name,-12} score={graph.GetScore(node.Name):F3}  refs=[{string.Join(", ", node.References)}]")
            .ToList();

        return $"Indexed {symbols.Count} symbols across the demo project.\r\n" +
               $"PageRank-ranked sample:\r\n{string.Join("\r\n", preview)}\r\n";
    }

    // ─── Stage 03 demo — context assembler ──────────────────────────────────

    private Task<string> RunContextAssemblerDemoAsync()
    {
        // Build a list of candidate context chunks of varying relevance and
        // ask the assembler to fit them inside a small token budget.
        var chunks = new List<ContextChunk>
        {
            new("auth.cs",    "public bool VerifyCredentials(string user, string pass) { ... }", 0.92),
            new("token.cs",   "public string GenerateRefreshToken() { ... }",                   0.85),
            new("logger.cs",  "public void LogEvent(string name, object payload) { ... }",       0.10),
        };

        var assembler = new ContextAssembler(tokenBudget: 50);
        var result    = assembler.Assemble(chunks);

        if (!result.IsSuccess)
            return Task.FromResult($"Failed: {result.ErrorMessage} ({result.ErrorCode})");

        var assembled = result.Value!;
        return Task.FromResult(
            $"Token budget: 50\r\n" +
            $"Assembled {assembled.Count} chunks (priority order):\r\n" +
            string.Join("\r\n", assembled.Select(c => $"  [{c.RelevanceScore:F2}] {c.FilePath}\r\n      {c.Content}")) +
            $"\r\nTotal tokens used (approx): {ContextAssembler.TotalTokens(assembled)}");
    }

    // ─── Stage 04 demo — ReAct agent loop with a mock language model ────────

    private async Task<string> RunAgentLoopDemoAsync()
    {
        // The mock language model produces a deterministic sequence of three
        // responses: thought, tool call, then task complete. The user sees the
        // exact same flow a real LLM would drive.
        var mockLlm = new MockLlmClient(
        [
            new LlmResponse("Looking up the requested file...", null, null),
            new LlmResponse("",
                            ToolCallName:      "read_file",
                            ToolCallArguments: new Dictionary<string, object> { ["path"] = Path.Combine(_projectRoot, "AGENTS.md") }),
            new LlmResponse("TASK_COMPLETE:Read the file successfully.", null, null),
        ]);

        // Wire up a single read-only tool so the loop has something safe to invoke.
        var permissionGate = new PermissionGate(_projectRoot);
        var toolRegistry   = new ToolRegistry(permissionGate);
        toolRegistry.Register(
            new ToolDefinition("read_file", "Reads the named file from disk", new()),
            args => Task.FromResult(OperationResult<string>.Ok(
                $"[fake content of {args.GetValueOrDefault("path")}]")));

        var loop = new AgentLoop(mockLlm, toolRegistry);
        var initialState = new AgentState
        {
            CurrentTask = "Read AGENTS.md",
        };
        var result = await loop.RunAsync(initialState);

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        var loopResult = result.Value!;
        return $"Loop completed: {loopResult.IsComplete}\r\n" +
               $"Final response: {loopResult.FinalResponse}\r\n\r\n" +
               $"Conversation log:\r\n" +
               string.Join("\r\n", loopResult.FinalState.MessageHistory
                   .Select((m, i) => $"  {i:D2} [{m.Role}] {m.Content}"));
    }

    // ─── Stage 05 demo — tool registry + permission gate ────────────────────

    private Task<string> RunToolSystemDemoAsync()
    {
        var gate = new PermissionGate(_projectRoot);

        // Three sample probes covering all three permission outcomes.
        var probes = new[]
        {
            ("read_file",   new Dictionary<string, object> { ["path"] = Path.Combine(_projectRoot, "AGENTS.md") }),
            ("edit_file",   new Dictionary<string, object> { ["path"] = Path.Combine(_projectRoot, "AGENTS.md") }),
            ("read_file",   new Dictionary<string, object> { ["path"] = "C:\\Windows\\System32\\hosts" }),
            ("run_command", new Dictionary<string, object> { ["cmd"]  = "rm -rf /"  }),
        };

        var lines = new List<string>();
        foreach (var (name, args) in probes)
        {
            var decision = gate.Check(name, args);
            string text = decision.IsSuccess ? decision.Value.ToString() : decision.ErrorMessage;
            string pathArg = args.TryGetValue("path", out var p) ? p.ToString() ?? string.Empty : string.Empty;
            lines.Add($"  {name,-12}  path={pathArg,-50}  → {text}");
        }

        return Task.FromResult(
            "Permission gate decisions:\r\n" + string.Join("\r\n", lines));
    }

    // ─── Stage 06 demo — file-change applier (in-memory ICodeWriter) ────────

    private async Task<string> RunFileChangeApplierDemoAsync()
    {
        var reader = new InMemoryCodeReader();
        reader.Set("greet.cs", "public string Greet() => \"Hello, world!\";");

        var writer  = new InMemoryCodeWriter();
        var applier = new FileChangeApplier(reader, writer);

        var result = await applier.ApplyPatchAsync(
            filePath:        "greet.cs",
            searchText:      "Hello, world!",
            replacementText: "Hello, AgentInfrastructure!");

        if (!result.IsSuccess)
            return $"Failed: {result.ErrorMessage} ({result.ErrorCode})";

        return $"Patch applied successfully.\r\nNew content:\r\n  {writer.Get("greet.cs")}";
    }

    // ─── Stage 07 demo — session state store using an in-memory ISessionStorage ──

    private async Task<string> RunSessionStateStoreDemoAsync()
    {
        var storage = new InMemorySessionStorage();

        var state = new AgentState
        {
            CurrentTask     = "Explain the agent loop",
            MessageHistory  = [new ConversationMessage("user", "What does the loop do?")],
            IterationCount  = 1,
        };

        var saveResult = await storage.SaveAsync("session-001", state);
        if (!saveResult.IsSuccess) return saveResult.ErrorMessage;

        var loadResult = await storage.LoadAsync("session-001");
        if (!loadResult.IsSuccess) return loadResult.ErrorMessage;

        var sessionList = await storage.ListSessionsAsync();

        return $"Saved task = '{state.CurrentTask}'\r\n" +
               $"Loaded task = '{loadResult.Value!.CurrentTask}'\r\n" +
               $"Loaded message count = {loadResult.Value!.MessageHistory.Count}\r\n" +
               $"All sessions: {string.Join(", ", sessionList.Value ?? [])}";
    }

    // ─── Helper: drill into a nested merged-config dictionary ──────────────

    private static string DigInto(IReadOnlyDictionary<string, object> root, params string[] path)
    {
        object? current = root;
        foreach (var key in path)
        {
            if (current is IReadOnlyDictionary<string, object> dictionary
                && dictionary.TryGetValue(key, out var next))
            {
                current = next;
            }
            else
            {
                return "<missing>";
            }
        }
        return current?.ToString() ?? "<null>";
    }
}

// ─── Demo-only fabric implementations ───────────────────────────────────────

// In-memory IConfigurationProvider for the configuration-loader demo.
internal sealed class InMemoryConfigurationProvider : IConfigurationProvider
{
    private readonly Dictionary<string, string> _json = [];
    private readonly Dictionary<string, string> _text = [];

    public void SetJson(string path, string value) => _json[path] = value;
    public void SetText(string path, string value) => _text[path] = value;

    public Task<OperationResult<Dictionary<string, object>>> ReadJsonAsync(string path)
    {
        if (!_json.TryGetValue(path, out var json))
            return Task.FromResult(OperationResult<Dictionary<string, object>>.Fail(
                $"No JSON at '{path}'", "NOT_FOUND"));

        var doc = System.Text.Json.JsonDocument.Parse(json);
        return Task.FromResult(OperationResult<Dictionary<string, object>>.Ok(
            ConvertJsonElement(doc.RootElement)));
    }

    public Task<OperationResult<string>> ReadTextAsync(string path)
        => Task.FromResult(_text.TryGetValue(path, out var text)
            ? OperationResult<string>.Ok(text)
            : OperationResult<string>.Fail($"No text at '{path}'", "NOT_FOUND"));

    private static Dictionary<string, object> ConvertJsonElement(System.Text.Json.JsonElement element)
    {
        var dict = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject())
            dict[property.Name] = ConvertValue(property.Value);
        return dict;
    }

    private static object ConvertValue(System.Text.Json.JsonElement element) => element.ValueKind switch
    {
        System.Text.Json.JsonValueKind.Object => ConvertJsonElement(element),
        System.Text.Json.JsonValueKind.Number => element.GetDouble(),
        System.Text.Json.JsonValueKind.True   => true,
        System.Text.Json.JsonValueKind.False  => false,
        System.Text.Json.JsonValueKind.Null   => null!,
        _                                     => element.GetString() ?? string.Empty,
    };
}

// Mock ILlmClient that returns a fixed sequence of canned responses.
internal sealed class MockLlmClient : ILlmClient
{
    private readonly Queue<LlmResponse> _responses;

    public MockLlmClient(IEnumerable<LlmResponse> responses)
    {
        _responses = new Queue<LlmResponse>(responses);
    }

    public Task<OperationResult<LlmResponse>> SendAsync(
        IReadOnlyList<ConversationMessage> messageHistory,
        IReadOnlyList<ToolDefinition>?     availableTools,
        CancellationToken                  cancellationToken)
    {
        if (_responses.Count == 0)
            return Task.FromResult(OperationResult<LlmResponse>.Ok(
                new LlmResponse("TASK_COMPLETE:no more responses", null, null)));

        return Task.FromResult(OperationResult<LlmResponse>.Ok(_responses.Dequeue()));
    }
}

// In-memory ICodeReader so demos can run without disk I/O.
internal sealed class InMemoryCodeReader : ICodeReader
{
    private readonly Dictionary<string, string> _files = [];

    public void Set(string path, string content) => _files[path] = content;

    public Task<OperationResult<string>> ReadFileAsync(string path)
        => Task.FromResult(_files.TryGetValue(path, out var content)
            ? OperationResult<string>.Ok(content)
            : OperationResult<string>.Fail($"File '{path}' not found", "NOT_FOUND"));

    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string directory, string pattern)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)_files.Keys.ToList()));
}

// In-memory ICodeWriter so demos can run without disk I/O.
internal sealed class InMemoryCodeWriter : ICodeWriter
{
    private readonly Dictionary<string, string> _files = [];

    public string? Get(string path) => _files.GetValueOrDefault(path);

    public Task<OperationResult<bool>> WriteFileAsync(string path, string content)
    {
        _files[path] = content;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }
}

// In-memory ISessionStorage for the session-store demo.
internal sealed class InMemorySessionStorage : ISessionStorage
{
    private readonly Dictionary<string, AgentState> _sessions = [];

    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        _sessions[sessionId] = state;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(_sessions.TryGetValue(sessionId, out var state)
            ? OperationResult<AgentState>.Ok(state)
            : OperationResult<AgentState>.Fail($"Session '{sessionId}' not found", "NOT_FOUND"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)_sessions.Keys.ToList()));
}
