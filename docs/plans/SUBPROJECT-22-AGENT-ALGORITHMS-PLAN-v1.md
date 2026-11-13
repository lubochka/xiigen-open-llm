# Sub-Project 22 — Coding Agent Algorithm Library
## Source document: coding-agent-algorithms.jsx (React visualization of agent algorithms)
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core — sub-project 22  
**Language:** C# / .NET 8  
**UI framework:** Windows Forms (встроенный в .NET 8 фреймворк для Windows desktop UI)  
**External packages:** none — all algorithms implemented from scratch  
**This document does not touch XIIGen work.**

---

## What this sub-project implements

The `coding-agent-algorithms.jsx` file is a React component that teaches eight groups of algorithms used by AI coding agents. Each section of the component describes an algorithm with pseudocode, comparisons, and step-by-step breakdowns. This sub-project translates each algorithm group into a self-contained C# class, and wraps them in a Windows Forms application that acts as an interactive algorithm explorer — the same information the React component displays, but as a native Windows desktop application.

The eight algorithm groups from the JSX source map to C# modules as follows:

| Section id in JSX | Algorithm name | C# module |
|---|---|---|
| `pipeline` | General agent pipeline — 4 stages | `Pipeline/AgentPipeline.cs` |
| `codebase` | Two schools of code understanding (static map vs tool-based) | `Indexing/RepoMapBuilder.cs`, `Indexing/ToolBasedExplorer.cs` |
| `loop` | Agent Loop — ReAct pattern | `Loop/ReactAgentLoop.cs` |
| `context` | Context management — fitting into the context window | `Context/ContextWindowManager.cs` |
| `multiagent` | Multi-agent patterns — orchestrator plus specialized sub-agents | `MultiAgent/AgentOrchestrator.cs` |
| `permissions` | Permission model and safety | `Security/PermissionModel.cs` |
| `rules` | Rules files (AGENTS.md, CLAUDE.md) | `Rules/RulesFileParser.cs` |
| `complex` | Handling complex multi-step tasks | `Tasks/ComplexTaskDecomposer.cs` |

---

## Coding rules — same nine rules as all other sub-projects

All nine coding rules apply. Every public method returns `OperationResult<T>`. Every service extends `BaseService`. Every line has a comment.

---

## Abbreviation table

| Short form | Full form | Plain-language explanation |
|---|---|---|
| ReAct | Reasoning and Acting pattern (паттерн рассуждения и действия) | the agent alternates between thinking about what to do next and executing the chosen action |
| RepoMap | Repository symbol map (карта символов репозитория) | a pre-built index of all function and class names in the codebase, their signatures, and where they are called from |
| tree-sitter | Tree-sitter incremental parser (инкрементальный парсер tree-sitter) | a C library that parses source code into a syntax tree very fast, even for partial or broken code |
| Context window | Context window (контекстное окно модели) | the maximum amount of text the language model can process in one request |
| Context compaction | Context compaction (сжатие контекста) | when the conversation history grows too long, the agent summarizes old turns and discards verbatim history to make room for new content |
| Semantic search | Semantic search (семантический поиск) | finding documents that are similar in meaning to the query, not just those that share the same words |
| Symbol graph | Symbol graph (граф символов) | a directed graph where nodes are code symbols (functions, classes) and edges represent "calls", "imports", or "defines" relationships |
| Subagent | Subagent (субагент) | a separate agent instance launched by the main agent to handle a specific sub-task independently |
| Plan/Act mode | Plan/Act execution mode (режим планирования и выполнения) | the agent first reads all relevant code in read-only mode to build a plan, then switches to execution mode to apply changes |
| Todo list | Task tracking list (список задач агента) | a list maintained by the agent of what sub-tasks it has identified, which are done, and which remain |

---

## Folder structure

```
22.AgentAlgorithms/               ← class library
├── Fabric/
│   ├── ISymbolGraphProvider.cs     — provides access to a codebase's symbol graph
│   ├── ISemanticSearchProvider.cs  — runs semantic search over a codebase index
│   └── IAgentMessageBus.cs         — sends messages between orchestrator and sub-agents
├── Pipeline/
│   └── AgentPipeline.cs            — the four-stage pipeline shared by all agents
├── Indexing/
│   ├── RepoMapBuilder.cs           — builds a static symbol map using Roslyn
│   └── ToolBasedExplorer.cs        — models tool-based dynamic exploration (no pre-index)
├── Loop/
│   └── ReactAgentLoop.cs           — ReAct pattern: reason → act → observe → repeat
├── Context/
│   ├── ContextWindowManager.cs     — assembles context within the token budget
│   ├── SemanticRanker.cs           — ranks context chunks by relevance to the query
│   └── ContextCompactor.cs         — summarizes and prunes old history when token limit approaches
├── MultiAgent/
│   ├── AgentOrchestrator.cs        — coordinates multiple specialized sub-agents
│   └── SubAgentDefinition.cs       — describes a sub-agent's name, role, and allowed tools
├── Security/
│   └── PermissionModel.cs          — rule-based permission checking for tool calls
├── Rules/
│   └── RulesFileParser.cs          — parses AGENTS.md / CLAUDE.md instruction files
├── Tasks/
│   ├── ComplexTaskDecomposer.cs    — breaks a multi-step task into a todo list
│   └── TodoList.cs                 — tracks task items and their completion status
└── Core/
    ├── BaseService.cs
    ├── OperationResult.cs
    └── StandardEvent.cs

22.AgentAlgorithms.Desktop/       ← Windows Forms interactive explorer
├── Program.cs
├── MainForm.cs                     — sidebar with 8 algorithm sections, detail panel on the right
├── AlgorithmDetailPanel.cs         — shows description, pseudocode, and a "Run Demo" button
└── DemoRunnerForm.cs               — runs the algorithm on a sample input and shows the output
```

---

## Phase 0 — Fabric interfaces (0.5 day)

```csharp
// ISymbolGraphProvider gives the algorithm classes access to a symbol graph
// without knowing whether the graph is stored in memory, on disk, or in a database.
public interface ISymbolGraphProvider
{
    // Return all symbols (functions, classes, files) in the graph
    Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync();

    // Return all symbols that are directly referenced by the given symbol
    Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId);
}

// IAgentMessageBus allows the orchestrator to send tasks to sub-agents
// and receive their results, without knowing how the sub-agents are implemented.
public interface IAgentMessageBus
{
    // Assign a task to a named sub-agent and wait for its result
    Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task);
}
```

Phase exit: `dotnet build` exits 0.

---

## Phase 1 — Four-stage agent pipeline (1 day)

**What this section of the JSX describes:** all agents (OpenCode, Aider, Cline, Claude Code) follow the same four-stage pipeline regardless of their internal differences: (1) index the codebase, (2) assemble context, (3) reason with the language model, (4) apply changes.

What to write: `Pipeline/AgentPipeline.cs`

```csharp
// AgentPipeline models the four-stage pipeline shared by all AI coding agents.
// Stage 1: index the codebase — build a searchable map of all code
// Stage 2: assemble context — select the most relevant code for the current task
// Stage 3: language model reasoning — send context + task to the model and get a plan
// Stage 4: apply changes — write the model's suggested edits back to the files
//
// This class does not implement each stage fully — it defines the contract
// and wires the stages together, delegating to specialized services.
public class AgentPipeline : BaseService
{
    // The service that indexes the codebase (stage 1)
    private readonly ISymbolGraphProvider symbolGraphProvider;

    // The service that selects relevant context (stage 2)
    private readonly ISemanticSearchProvider semanticSearchProvider;

    // Run all four stages for a given user task
    // task — the user's request in natural language (e.g. "add error handling to the login function")
    // Returns a summary of what was done (which files were changed and why)
    public async Task<OperationResult<PipelineResult>> RunAsync(string task)
    {
        // --- Stage 1: ensure the codebase index is up to date ---
        // We do not re-index every time — only if the index is missing or stale
        Logger.LogInformation("Stage 1: checking codebase index");
        var symbolsResult = await symbolGraphProvider.GetAllSymbolsAsync();
        if (!symbolsResult.Success)
        {
            return OperationResult<PipelineResult>.Fail(
                $"Stage 1 failed — could not read the symbol graph: {symbolsResult.ErrorMessage}",
                "INDEX_UNAVAILABLE");
        }

        // --- Stage 2: select the most relevant code chunks for this task ---
        Logger.LogInformation("Stage 2: assembling context for task: {Task}", task);
        // ... semantic search + graph traversal ...

        // --- Stage 3: send task + context to the language model ---
        Logger.LogInformation("Stage 3: reasoning with language model");
        // ... LLM call ...

        // --- Stage 4: apply the model's suggested changes ---
        Logger.LogInformation("Stage 4: applying file changes");
        // ... patch application ...

        return OperationResult<PipelineResult>.Ok(new PipelineResult { /* ... */ });
    }
}
```

Tests:
- Run pipeline with a mock for all four stages; verify all four stages are called in order
- Verify that if stage 1 fails, stages 2–4 are not called

---

## Phase 2 — Two indexing strategies (2 days)

**What the JSX describes:** there are two schools of code understanding. The static approach (RepoMap, used by Aider) pre-builds a symbol map at startup. The dynamic approach (tool-based, used by OpenCode and Claude Code) does not pre-index — the agent explores the codebase on demand using tools.

**Sub-phase 2a — RepoMap builder (1 day)**

What to write: `Indexing/RepoMapBuilder.cs`

Uses Roslyn to walk all `.cs` files and produce a compact map: function names, parameter types, return types, and call sites — but not the function bodies. The result fits in a small number of tokens and gives the language model a structural overview without sending all the code.

```csharp
// RepoMapBuilder builds a compact structural map of a C# codebase.
// The map contains function signatures and their call relationships —
// but NOT the full function bodies, to keep the token count low.
// This is the static indexing approach described in the JSX as "RepoMap (Aider)".
public class RepoMapBuilder : BaseService
{
    // Build the map for all .cs files under the given directory
    // projectDirectory — the root directory of the C# project to analyze
    // Returns a list of symbol summaries, each as a dictionary with:
    //   "name" → symbol name, "signature" → parameter list and return type,
    //   "callers" → list of other symbols that call this one,
    //   "pageRankScore" → importance score (widely-called symbols score higher)
    public async Task<OperationResult<List<Dictionary<string, object>>>> BuildAsync(
        string projectDirectory)
    { ... }
}
```

**Sub-phase 2b — Tool-based explorer (1 day)**

What to write: `Indexing/ToolBasedExplorer.cs`

Models the dynamic exploration approach: no pre-built index. The explorer exposes the same tools that an agent would use at runtime (read file, search code, list directory) and records which files and symbols the agent visited during a session. This record can be replayed for testing.

Tests:
- Build RepoMap for a 5-file test project; verify a widely-called function has a higher score than one called from only one place
- Verify the RepoMap does not include function bodies, only signatures

---

## Phase 3 — ReAct agent loop (1.5 days)

**What the JSX describes:** the pseudocode for the ReAct pattern (паттерн рассуждения и действия — агент чередует фазу рассуждения с фазой выполнения действия):

```
function agentLoop(task, context):
  history = [systemPrompt, userTask]
  LOOP:
    response = LLM.call(history + availableTools)
    if response is text → append to history, continue
    if response is tool_call → execute, append result, continue
    if response is task_complete → return result
    if tokens > limit → compact history
    if iterations > max → return partial
```

What to write: `Loop/ReactAgentLoop.cs`

The C# implementation follows the pseudocode exactly. Key difference: all side effects (calling the language model, executing tools) go through injected interfaces so the loop is fully testable without a real language model.

Tests:
- Run loop with a mock that returns `task_complete` on the first call; verify exactly one language model call was made
- Run loop with a mock that always returns a tool call; verify the loop stops after `maximumIterations` and returns a partial result
- Run loop until history exceeds the compaction threshold; verify `ContextCompactor.CompactAsync` was called

---

## Phase 4 — Context window management (2 days)

**What the JSX describes:** five strategies for fitting the codebase into the context window:
1. Semantic search plus ranking (find relevant chunks, rank by similarity)
2. Symbol graph traversal (expand from seed symbols to neighbors)
3. Context compaction (summarize old history to free up space)
4. Priority-based trimming (keep the most important chunks when space runs out)
5. Lazy loading (tool-based: only load what the agent explicitly requests)

What to write: `Context/ContextWindowManager.cs`, `Context/SemanticRanker.cs`, `Context/ContextCompactor.cs`

`SemanticRanker` takes a list of text chunks and a query; returns the chunks sorted by relevance score (using cosine similarity against the query embedding, via `ISemanticSearchProvider`).

`ContextCompactor` takes a long message history and produces a shorter version by keeping only: the system instructions, the most recent N turns verbatim, and a one-paragraph summary of everything older.

```csharp
// ContextCompactor reduces the size of the message history when it grows too large.
// It preserves the most recent turns in full and summarizes older turns into a short paragraph.
// This is called "context compaction" (сжатие контекста) in the JSX source.
public class ContextCompactor : BaseService
{
    // How many of the most recent message turns to always keep verbatim (не сжимая)
    private const int RecentTurnsToPreserve = 10;

    // Compact a message history that has grown too long
    // messageHistory — the full conversation history
    // summaryGenerator — a function that summarizes a list of messages into one paragraph
    //   (in production this calls the language model; in tests it returns a fixed string)
    public async Task<OperationResult<List<ConversationMessage>>> CompactAsync(
        List<ConversationMessage> messageHistory,
        Func<List<ConversationMessage>, Task<string>> summaryGenerator)
    {
        // Separate the recent turns (keep verbatim) from the older turns (will be summarized)
        int olderTurnCount = Math.Max(0, messageHistory.Count - RecentTurnsToPreserve);
        var olderTurns = messageHistory.Take(olderTurnCount).ToList();
        var recentTurns = messageHistory.Skip(olderTurnCount).ToList();

        // If there is nothing old enough to summarize, return the history unchanged
        if (olderTurns.Count == 0)
        {
            return OperationResult<List<ConversationMessage>>.Ok(messageHistory);
        }

        // Ask the summary generator to produce a one-paragraph summary of the older turns
        string summary = await summaryGenerator(olderTurns);

        // Replace all old turns with a single "summary" message
        var summaryMessage = new ConversationMessage
        {
            Role = "system",
            Content = $"[Summary of earlier conversation: {summary}]"
        };

        // Return the compacted history: summary message followed by the recent turns verbatim
        var compactedHistory = new List<ConversationMessage> { summaryMessage };
        compactedHistory.AddRange(recentTurns);
        return OperationResult<List<ConversationMessage>>.Ok(compactedHistory);
    }
}
```

Tests:
- Compact a 30-message history with `RecentTurnsToPreserve = 10`; verify output has 11 messages (1 summary + 10 recent)
- Verify summary content appears as the first message
- Verify history with fewer than `RecentTurnsToPreserve` messages is returned unchanged

---

## Phase 5 — Multi-agent orchestrator (1.5 days)

**What the JSX describes:** the orchestrator pattern — a main agent that breaks a complex task into sub-tasks and delegates each sub-task to a specialized sub-agent. Sub-agents have narrower tool access than the main agent.

What to write: `MultiAgent/AgentOrchestrator.cs`, `MultiAgent/SubAgentDefinition.cs`

```csharp
// AgentOrchestrator coordinates multiple sub-agents to complete a complex task.
// It breaks the main task into sub-tasks, assigns each to the right sub-agent,
// and combines their results into the final answer.
// This mirrors the multi-agent pattern described in the JSX section "multiagent".
public class AgentOrchestrator : BaseService
{
    // The registered sub-agents, keyed by their role name
    private readonly Dictionary<string, SubAgentDefinition> registeredSubAgents;

    // The message bus for sending tasks to sub-agents and receiving results
    private readonly IAgentMessageBus messageBus;

    // Run a complex task by decomposing it into sub-tasks and delegating them
    // task — the high-level task to accomplish
    // Returns the combined result from all sub-agents
    public async Task<OperationResult<string>> RunAsync(string task)
    {
        // Decompose the task into a list of sub-tasks, each tagged with a required sub-agent role
        var decompositionResult = ComplexTaskDecomposer.Decompose(task);
        if (!decompositionResult.Success)
        {
            return OperationResult<string>.Fail(decompositionResult.ErrorMessage, "DECOMPOSITION_FAILED");
        }

        var subTaskResults = new List<string>();

        // Execute each sub-task in order, collecting results
        foreach (var subTask in decompositionResult.Value!)
        {
            // Find the sub-agent whose role matches this sub-task
            if (!registeredSubAgents.TryGetValue(subTask.RequiredRole, out var subAgent))
            {
                return OperationResult<string>.Fail(
                    $"No sub-agent registered for role '{subTask.RequiredRole}'.",
                    "MISSING_SUBAGENT");
            }

            // Assign the sub-task to the matching sub-agent and wait for the result
            var subTaskResult = await messageBus.AssignTaskAsync(subAgent.Name, subTask.Description);
            if (!subTaskResult.Success)
            {
                return OperationResult<string>.Fail(
                    $"Sub-agent '{subAgent.Name}' failed: {subTaskResult.ErrorMessage}",
                    "SUBAGENT_FAILED");
            }

            // Collect the result for later combination
            subTaskResults.Add(subTaskResult.Value!);
        }

        // Combine all sub-task results into a single response
        string combinedResult = string.Join("\n\n", subTaskResults);
        return OperationResult<string>.Ok(combinedResult);
    }
}
```

Tests:
- Orchestrate a 2-sub-task scenario with mock sub-agents; verify both sub-agents received their tasks
- Verify failure when a required sub-agent is not registered

---

## Phase 6 — Permission model, rules parser, task decomposer (2 days)

**Three classes, one day each:**

`PermissionModel.cs` — rule-based check: each tool has an associated permission level (`ReadOnly`, `WriteWithApproval`, `AlwaysDenied`). The model reads the level and returns a typed decision.

`RulesFileParser.cs` — parses AGENTS.md and CLAUDE.md files. Extracts: the preamble (instructions before any heading), named sections (each heading becomes a section name), and any `@allow`/`@deny` lines that define tool permissions.

```csharp
// RulesFileParser reads instruction files (AGENTS.md or CLAUDE.md).
// These are plain Markdown files where each heading introduces a section of instructions.
// The parser extracts the sections so the agent can present them to the language model
// in a structured way rather than as a raw wall of text.
public class RulesFileParser : BaseService
{
    // Parse an instruction file and return its contents as named sections
    // fileContent — the raw text of the AGENTS.md or CLAUDE.md file
    // Returns a dictionary from section name to section body text
    public OperationResult<Dictionary<string, string>> Parse(string fileContent)
    { ... }
}
```

`ComplexTaskDecomposer.cs` — given a multi-step task description, breaks it into a list of `SubTask` records, each with: a description, a required sub-agent role, and an estimated complexity (simple/medium/complex). Uses a simple heuristic (number of "and", "then", "also" conjunctions) to decide whether decomposition is needed.

Tests:
- Parse an AGENTS.md with three sections; verify three keys in the output dictionary
- Decompose `"read the auth module and then refactor the login function"` into two sub-tasks
- Verify `PermissionModel` returns `WriteWithApproval` for the `edit_file` tool

---

## Phase 7 — Windows Forms interactive explorer (1.5 days)

The desktop application lets users explore all eight algorithm sections, run each one on a sample input, and see the output.

Layout:
- Left panel: a list of the eight algorithm names (one row per JSX section)
- Right panel: two tabs — "Description" (text explaining what the algorithm does) and "Demo" (a "Run" button with an output area below it)

Each algorithm section has a hardcoded demo input (a small string or a small list of mock messages) and the "Run" button calls the corresponding C# class and displays the result.

This is a learning tool — its purpose is to make the algorithms concrete and observable, the same role the JSX component plays in a browser.

Phase exit: all eight sections are selectable; the "Run" button produces non-empty output for each section.

---

## Plan Review Checks

**Goal delivery:** all 8 JSX sections (pipeline, codebase, loop, context, multiagent, permissions, rules, complex) are covered by implementation phases. ✓  
**Authentication check:** no HTTP controllers. No auth declaration needed. ✓  
**Count accuracy:** 8 algorithm groups → 7 implementation phases (phases 2a and 2b are sub-phases of phase 2). ✓

---

## Summary table

| Phase | What is built | Days |
|---|---|---|
| 0 | Fabric interfaces | 0.5 |
| 1 | Four-stage pipeline | 1 |
| 2 | Two indexing strategies (RepoMap + tool-based) | 2 |
| 3 | ReAct agent loop | 1.5 |
| 4 | Context window management + compaction | 2 |
| 5 | Multi-agent orchestrator | 1.5 |
| 6 | Permission model, rules parser, task decomposer | 2 |
| 7 | Windows Forms explorer UI | 1.5 |
| **Total** | | **12 days** |

---

*Plan v1.0 — sub-project 22, source: coding-agent-algorithms.jsx. Does not touch XIIGen or sub-projects 01–21.*
