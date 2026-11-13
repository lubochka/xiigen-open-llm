# Sub-Project 21 — AI Coding Agent Infrastructure Library
## Source document: agent-internals.md (OpenCode · Aider · Cline internals)
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core — sub-project 21  
**Language:** C# / .NET 8  
**UI framework:** Windows Forms (Форм для Windows — встроенный в .NET 8 фреймворк для создания графических окон, самый простой способ сделать desktop-приложение без XAML)  
**Why Windows Forms:** It is built into .NET 8 with no additional packages. Enable it with `<UseWindowsForms>true</UseWindowsForms>` in the project file. Event model is simple button-click handlers — no data binding, no XAML, no reactive streams.  
**External packages:** Microsoft.Data.Sqlite (хранение состояния в SQLite базе данных) for session persistence; Microsoft.CodeAnalysis.CSharp (Roslyn — компилятор C# используемый как библиотека для разбора кода) for code indexing. Both are MIT.  
**This document does not touch XIIGen work.**

---

## What this sub-project implements

The `agent-internals.md` document describes the internal architecture of three AI coding agents: OpenCode, Aider, and Cline. All three share seven common stages. This sub-project implements those seven stages as a reusable C# class library (`21.AgentInfrastructure`) plus a Windows Forms host application (`21.AgentInfrastructure.Desktop`) that lets you interact with and observe the running agent.

The seven stages from the source document map to C# as follows:

| Stage from document | C# component | File |
|---|---|---|
| Stage 01 — Initialization and configuration | ConfigurationLoader (загрузчик конфигурации) | `Config/ConfigurationLoader.cs` |
| Stage 02 — Codebase indexing | CodebaseIndexer (индексатор кодовой базы) | `Indexing/CodebaseIndexer.cs` |
| Stage 03 — Context assembly and token budget | ContextAssembler (сборщик контекста) | `Context/ContextAssembler.cs` |
| Stage 04 — Agent Loop (main execution cycle) | AgentLoop (главный цикл агента) | `Loop/AgentLoop.cs` |
| Stage 05 — Tool system and permissions | ToolRegistry and PermissionGate | `Tools/ToolRegistry.cs`, `Tools/PermissionGate.cs` |
| Stage 06 — Applying changes to files | FileChangeApplier (применение правок к файлам) | `Editing/FileChangeApplier.cs` |
| Stage 07 — State storage and persistence | SessionStateStore (хранилище состояния сессии) | `State/SessionStateStore.cs` |

---

## Coding rules (Genie DNA) — same as all other sub-projects

All nine coding rules from the main plan (v2.0) apply without exception. Key reminders:
- Every public method returns `OperationResult<T>` — no exceptions for business logic
- Every service class extends `BaseService`
- All fabric dependencies are injected — no `new ConcreteClass()` inside services
- Every line of code carries a plain-language comment
- No abbreviations without the full form on the same line

---

## Abbreviation table for this sub-project

| Short form | Full form | Plain-language explanation |
|---|---|---|
| LLM | Large Language Model (большая языковая модель) | a neural network trained on text that can answer questions, write code, and reason about problems |
| AST | Abstract Syntax Tree (абстрактное синтаксическое дерево) | a tree representation of source code where each node is a language construct (function, variable, loop) |
| ReAct | Reasoning and Acting pattern (паттерн рассуждения и действия) | a design pattern where the agent alternates between thinking about what to do next and actually doing it |
| SQLite | SQLite embedded database (встраиваемая база данных SQLite) | a file-based relational database that requires no server, runs inside the application process |
| Roslyn | Microsoft C# Compiler Platform (платформа компилятора Microsoft для C#) | an open-source library that lets you parse, analyze, and transform C# code as data structures |
| PageRank | PageRank graph ranking algorithm (алгоритм ранжирования узлов графа) | assigns a score to each node based on how many other important nodes point to it |
| Token budget | Token budget (бюджет токенов) | the maximum number of word-pieces the language model can receive in one request |
| Context window | Context window (контекстное окно) | the total amount of text (measured in tokens) that a language model can read at one time |
| Deep merge | Deep merge of configuration layers (глубокое объединение слоёв конфигурации) | combining two configuration dictionaries where nested keys are merged rather than overwritten |

---

## Folder structure

```
21.AgentInfrastructure/           ← class library (no UI code here)
├── Fabric/
│   ├── IConfigurationProvider.cs   — reads config from files or environment
│   ├── ICodeReader.cs              — reads source files from disk
│   ├── ILlmClient.cs               — sends prompts to and receives responses from the language model
│   ├── ISessionStorage.cs          — saves and loads session history
│   └── FileSystemCodeAccess.cs     — concrete implementation for disk access
├── Config/
│   └── ConfigurationLoader.cs      — stage 01: layered config with deep merge
├── Indexing/
│   ├── CodebaseIndexer.cs          — stage 02: walk repo, extract symbols, build index
│   └── SymbolGraphBuilder.cs       — build directed graph of function/class references
├── Context/
│   ├── ContextAssembler.cs         — stage 03: select and rank context within token budget
│   └── TokenCounter.cs             — count tokens in a string before sending to the model
├── Loop/
│   ├── AgentLoop.cs                — stage 04: ReAct cycle (reason → act → observe → repeat)
│   └── AgentState.cs               — current state of the loop (history, task, todo list)
├── Tools/
│   ├── ToolRegistry.cs             — stage 05: register and look up available tools by name
│   ├── PermissionGate.cs           — check whether a tool call is allowed in the current context
│   ├── ReadFileTool.cs             — tool: read a file from disk
│   ├── SearchCodeTool.cs           — tool: search for a pattern in the codebase
│   ├── ListDirectoryTool.cs        — tool: list files in a directory
│   └── RunCommandTool.cs           — tool: run a shell command (requires explicit permission)
├── Editing/
│   ├── FileChangeApplier.cs        — stage 06: apply search-and-replace patches to files
│   └── DiffGenerator.cs           — generate a human-readable diff for preview
├── State/
│   └── SessionStateStore.cs        — stage 07: save/load full session to SQLite
└── Core/
    ├── BaseService.cs
    ├── OperationResult.cs
    └── StandardEvent.cs

21.AgentInfrastructure.Desktop/   ← Windows Forms host application
├── Program.cs                      — application entry point
├── MainForm.cs                     — main window: chat input, response area, tool log
├── MainForm.Designer.cs            — auto-generated Windows Forms layout
├── IndexingProgressForm.cs         — progress window shown during codebase indexing
└── PermissionDialog.cs             — popup asking user to approve a tool call
```

---

## Phase 0 — Fabric interfaces (1 day)

All interfaces are defined first. No implementation code is written until all interfaces are agreed on.

```csharp
// ILlmClient hides which language model API is being called.
// The agent loop only knows "send a prompt, get a response" —
// it does not know whether the response comes from Claude, GPT, or a local model.
public interface ILlmClient
{
    // Send a list of messages (conversation history plus the current user message)
    // and receive the model's response text.
    // The response may contain tool call instructions embedded in it.
    Task<OperationResult<LlmResponse>> SendAsync(
        List<ConversationMessage> messageHistory,
        List<ToolDefinition> availableTools,
        CancellationToken cancellationToken);
}

// ISessionStorage saves and restores the full conversation state.
// The agent can be stopped and resumed without losing progress.
public interface ISessionStorage
{
    // Save the current agent state (message history, tool log, current task)
    Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state);

    // Load a previously saved state by its session identifier
    Task<OperationResult<AgentState>> LoadAsync(string sessionId);

    // Return a list of all session identifiers stored in the database
    Task<OperationResult<List<string>>> ListSessionsAsync();
}
```

Phase exit: `dotnet build` exits 0. All interface files compile.

---

## Phase 1 — Configuration loader — Stage 01 (1 day)

**What Stage 01 does:** reads configuration from multiple sources in priority order and merges them. Lower-priority sources are overwritten by higher-priority ones key-by-key. Nested keys are merged rather than overwritten.

```csharp
// ConfigurationLoader implements the layered configuration system described in agent-internals.md.
// It reads configuration from three sources in order of increasing priority:
//   1. The global user configuration file (~/.config/agent/agent.json)
//   2. The project configuration file (.agent/agent.json in the current directory)
//   3. The AGENTS.md or CLAUDE.md instruction file in the project root
// Each source is merged on top of the previous one using deep merge
// (глубокое объединение — вложенные словари объединяются ключ за ключом, а не перезаписываются целиком).
public class ConfigurationLoader : BaseService
{
    // The provider that reads raw file content from disk
    private readonly IConfigurationProvider configurationProvider;

    public ConfigurationLoader(IConfigurationProvider provider, ILogger logger, IOperationContext context)
        : base(logger, context)
    {
        // Store the file provider so we can read config files through the fabric interface
        configurationProvider = provider;
    }

    // Load and merge all configuration layers, returning the final merged configuration
    public async Task<OperationResult<Dictionary<string, object>>> LoadAsync(string projectRootPath)
    {
        // Start with an empty configuration — all defaults come from the merged layers
        var mergedConfiguration = new Dictionary<string, object>();

        // Layer 1: global user configuration — lowest priority, everything else can override it
        var globalConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "agent", "agent.json");

        var globalResult = await configurationProvider.ReadJsonAsync(globalConfigPath);
        if (globalResult.Success)
        {
            // Deep merge the global config into the empty base
            DeepMerge(mergedConfiguration, globalResult.Value!);
        }

        // Layer 2: project configuration — overrides global settings for this specific project
        var projectConfigPath = Path.Combine(projectRootPath, ".agent", "agent.json");
        var projectResult = await configurationProvider.ReadJsonAsync(projectConfigPath);
        if (projectResult.Success)
        {
            // Deep merge the project config on top of global — project wins on conflicts
            DeepMerge(mergedConfiguration, projectResult.Value!);
        }

        // Layer 3: instruction files (AGENTS.md or CLAUDE.md) — plain text instructions for the agent
        // These are injected as a "systemInstructions" key in the merged config
        var agentsInstructionPath = Path.Combine(projectRootPath, "AGENTS.md");
        var claudeInstructionPath = Path.Combine(projectRootPath, "CLAUDE.md");

        var instructionResult = await configurationProvider.ReadTextAsync(agentsInstructionPath);
        if (!instructionResult.Success)
        {
            // AGENTS.md not found — try CLAUDE.md as the fallback instruction file
            instructionResult = await configurationProvider.ReadTextAsync(claudeInstructionPath);
        }

        if (instructionResult.Success)
        {
            // Store the instruction text as a top-level key in the merged configuration
            mergedConfiguration["systemInstructions"] = instructionResult.Value!;
        }

        // Return the fully merged configuration
        return OperationResult<Dictionary<string, object>>.Ok(mergedConfiguration);
    }

    // Deep merge: for each key in the source dictionary, if both source and target have
    // a nested dictionary at that key, recursively merge them instead of overwriting.
    // For all other value types, the source value overwrites the target value.
    private static void DeepMerge(
        Dictionary<string, object> target,
        Dictionary<string, object> source)
    {
        foreach (var (key, sourceValue) in source)
        {
            // Check if both target and source have a nested dictionary at this key
            if (target.TryGetValue(key, out var targetValue)
                && targetValue is Dictionary<string, object> targetNested
                && sourceValue is Dictionary<string, object> sourceNested)
            {
                // Both sides are dictionaries — merge recursively instead of overwriting
                DeepMerge(targetNested, sourceNested);
            }
            else
            {
                // One or both sides is not a dictionary — source value wins
                target[key] = sourceValue;
            }
        }
    }
}
```

Tests: verify that a project config key overrides a global config key; verify that nested keys in the project config merge rather than overwrite the global nested keys.

---

## Phase 2 — Codebase indexer — Stage 02 (2 days)

Uses Roslyn (Microsoft C# Compiler Platform — компилятор C# как библиотека для разбора исходного кода) to extract:
- All class names, method names, and field names from every `.cs` file
- Call relationships: which method calls which other method
- Import relationships: which file imports which namespace

Builds a `SymbolGraph` (граф символов — структура данных где каждый символ кода это узел, а связи типа "вызывает" или "объявляет" это рёбра) stored in the existing `IGraphStorage` interface from sub-project 14. Runs PageRank on the resulting graph to rank symbols by importance (widely-called functions rank higher).

What to write: `CodebaseIndexer.cs`, `SymbolGraphBuilder.cs`

Key method:
- `IndexDirectoryAsync(string directoryPath, IProgress<int> progressReporter)` — walks all `.cs` files, extracts symbols via Roslyn, builds graph, computes PageRank, stores result via `IGraphStorage`

Tests:
- Index a 3-file test project; verify that a widely-called helper function has a higher PageRank score than a function called from only one place
- Verify progress reporting fires for each processed file

---

## Phase 3 — Context assembler — Stage 03 (1.5 days)

Selects which code snippets to include in the prompt, respecting the token budget (бюджет токенов — максимальное количество фрагментов слов которые модель может получить за один запрос).

Strategy mirrors the document description:
1. Semantic search in the vector index (sub-project 06 `IVectorStorage`) for the top K matching code chunks
2. Graph traversal (sub-project 07 `IGraphStorage`) to expand the result to neighboring symbols
3. Rank by PageRank score descending
4. Cut off when the token count would exceed the budget

What to write: `ContextAssembler.cs`, `TokenCounter.cs`

`TokenCounter` uses a simple word-count approximation (1 token ≈ 0.75 words) for estimation without calling the model.

Tests:
- Assemble context for a query; verify total token count is below the specified budget
- Verify that the most-relevant chunk (highest vector similarity) appears first in the assembled context

---

## Phase 4 — Agent Loop — Stage 04 (2 days)

Implements the ReAct pattern (паттерн рассуждения и действия — агент чередует фазу "думаю что делать дальше" и фазу "делаю выбранное действие"):

```
1. Send current state (history + tools) to the language model
2. If the model response is text → append to history, continue loop
3. If the model response contains a tool call → check permissions, execute tool, append result, continue loop
4. If the model response signals task complete → return final result
5. If token count exceeds compaction threshold → summarize old history, trim it, continue loop
6. If iteration count exceeds maximum → return partial result with explanation
```

What to write: `AgentLoop.cs`, `AgentState.cs`

`AgentState` is a plain record:
```csharp
// AgentState captures everything the agent needs to continue from where it left off.
// Storing this as a plain record makes it easy to serialize to SQLite and restore later.
public record AgentState
{
    // The full conversation history: all messages sent and received so far
    public List<ConversationMessage> MessageHistory { get; init; } = new();

    // The task the agent is currently working on (the original user request)
    public string CurrentTask { get; init; } = string.Empty;

    // How many loop iterations have run so far (used to detect runaway loops)
    public int IterationCount { get; init; }

    // The list of sub-tasks the agent has identified and their completion status
    public List<TodoItem> TodoList { get; init; } = new();
}
```

Tests:
- Run a minimal loop with a mock language model that immediately signals task complete; verify loop terminates after one iteration
- Run with a mock that always calls a tool; verify loop terminates after maximum iterations

---

## Phase 5 — Tool system — Stage 05 (1.5 days)

What to write: `ToolRegistry.cs`, `PermissionGate.cs`, and four tool implementations.

`PermissionGate` has three permission levels:
- Auto-approved: read-only tools (read file, list directory, search)
- Requires prompt approval: write tools (edit file)
- Always denied: destructive tools (delete, run arbitrary commands) unless explicitly unlocked

```csharp
// PermissionGate decides whether a tool call is allowed before it executes.
// It prevents the agent from taking actions the user has not authorized.
public class PermissionGate : BaseService
{
    // Check whether a specific tool call is permitted in the current context
    // toolName — the name of the tool being called (e.g. "read_file", "edit_file")
    // toolArguments — the arguments the agent wants to pass to the tool
    // Returns: Allowed (можно выполнять), RequiresApproval (нужно спросить пользователя), Denied (запрещено)
    public OperationResult<PermissionDecision> Check(
        string toolName,
        Dictionary<string, object> toolArguments)
    { ... }
}
```

Tests:
- Verify `read_file` is auto-approved
- Verify `edit_file` returns `RequiresApproval`
- Verify a tool attempting to write outside the project directory is denied

---

## Phase 6 — File change applier — Stage 06 (1 day)

Implements the search-and-replace patch format described in the document:

```csharp
// FileChangeApplier applies search-and-replace patches to source files.
// A patch describes an exact string to find and the string to replace it with.
// This is the most precise way to make targeted edits without rewriting the whole file.
public class FileChangeApplier : BaseService
{
    // Apply a single patch to a file
    // filePath — the file to modify
    // searchText — the exact text to find in the file (must match exactly, including whitespace)
    // replacementText — the text to substitute in place of the found text
    // Returns success if exactly one match was found and replaced;
    //   returns failure if zero matches or more than one match (ambiguous patch)
    public OperationResult<bool> ApplyPatch(
        string filePath,
        string searchText,
        string replacementText)
    { ... }
}
```

Tests:
- Apply a patch to a test file; verify the file content changed correctly
- Verify failure when `searchText` appears zero times in the file
- Verify failure when `searchText` appears more than once (ambiguous — the agent must be more specific)

---

## Phase 7 — Session state store — Stage 07 (1 day)

Persists the full `AgentState` to a SQLite database so sessions can be resumed.

Schema (two tables):
- `sessions` — one row per session: id, created_at, task description
- `messages` — one row per message: session_id, role, content, timestamp

Uses `Microsoft.Data.Sqlite` (MIT). No ORM — plain SQL with parameterized queries.

Tests:
- Save a state with 5 messages; reload it; verify message count and content match
- Verify that listing sessions returns the saved session identifier

---

## Phase 8 — Windows Forms desktop host (1.5 days)

What to write: `MainForm.cs`

The main window has four areas:
- A text box at the top for the user to type a task or question
- A "Run Agent" button that starts `AgentLoop`
- A read-only text area showing the agent's responses and reasoning
- A panel on the right showing a live log of tool calls (what tool, what arguments, what result)

When the agent calls a tool that requires approval (`RequiresApproval`), a modal `PermissionDialog` appears:
- Shows the tool name and the full arguments
- Two buttons: "Allow" and "Deny"
- The loop pauses until the user clicks one

Codebase indexing (which can take minutes) runs in a background thread and shows `IndexingProgressForm` — a progress bar window with a "files processed: X of Y" counter.

**No business logic in the Forms classes.** All logic lives in the library project. The Forms only call service methods and display results.

Phase exit: `dotnet build` exits 0. The desktop application starts, shows the main window, and accepts input.

---

## Plan Review Checks

**Goal delivery:** all 7 stages from `agent-internals.md` are covered — Stages 01 through 07 map to Phases 1 through 7. ✓  
**Authentication check:** no HTTP controllers produced. No auth declaration needed. ✓  
**Count accuracy:** 7 stages → 7 implementation phases → 7 test groups. ✓  
**Fabric-first check:** `ILlmClient`, `IConfigurationProvider`, `ISessionStorage`, `ICodeReader` — all dependencies injected, no concrete classes instantiated inside services. ✓

---

## Acceptance criteria

- All tests pass
- The Windows Forms application starts and shows the main window
- Running the agent on a small test repository (3–5 files) produces at least one tool call and one model response
- The session can be saved and reloaded — conversation history is preserved

## Estimated days: 13

---

*Plan v1.0 — sub-project 21, source: agent-internals.md. Does not touch XIIGen or sub-projects 01–20.*
