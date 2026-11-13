# Sub-Project 23 — OSS Tools .NET Equivalent Library
## Source document: oss-plan-summary.md (VS Code Plugin / XIIGen OSS Evaluation Plan)
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core — sub-project 23  
**Language:** C# / .NET 8  
**UI framework:** Windows Forms (встроенный в .NET 8 фреймворк для Windows desktop)  
**External packages:** YamlDotNet (MIT — разбор YAML в C#); Microsoft.Data.Sqlite (MIT — хранение состояния); Microsoft.CodeAnalysis.CSharp (MIT — разбор C# кода через Roslyn). All three are MIT-licensed.  
**This document does not touch XIIGen work.**

---

## What this sub-project implements

The `oss-plan-summary.md` document evaluates eight open-source JavaScript/TypeScript/Python tools for use in a VS Code plugin. Each tool serves a specific role in the architecture. This sub-project provides the .NET equivalent of each tool as a C# class library — so the same algorithms are available inside a Windows desktop application without Node.js or Python.

The eight tools from the document and their .NET equivalents:

| Tool from OSS plan | Role in architecture | .NET equivalent | Already in solution? |
|---|---|---|---|
| `@modelcontextprotocol/sdk` | Machine Control Protocol transport | `McpTransport` — HTTP + named-pipe server | New code |
| `gray-matter` | YAML frontmatter parser for SKILL.md files | `FrontmatterParser` | New code |
| `vectra` | Local vector store | Already in sub-project 06 `IVectorStorage` | Reuse sub-project 06 |
| `graphology` | Graph data structure + PageRank | Already in sub-project 07 `IGraphStorage` | Reuse sub-project 07 |
| `ts-morph` | Abstract Syntax Tree traversal for TypeScript | `RoslynCodeAnalyzer` using Microsoft.CodeAnalysis | New code |
| `zod` + `instructor` | Schema validation + LLM retry loop | `SchemaValidator` + `StructuredLlmClient` | New code |
| `@langchain/langgraph` | State machine for agent loop | `AgentStateMachine` | New code |
| `nano-graphrag` | Graph Retrieval-Augmented Generation algorithm | Already in sub-project 08 `HybridRetriever` | Reuse sub-project 08 |

**Important:** sub-projects 06, 07, and 08 already implement vectra, graphology, and nano-graphrag equivalents. This sub-project does not duplicate them — it references them via their existing fabric interfaces and adds the four components that do not exist yet.

---

## Coding rules — all nine apply

Same rules as all other sub-projects. Every public method returns `OperationResult<T>`. Every service extends `BaseService`. Every line of code has a comment.

---

## Abbreviation table

| Short form | Full form | Plain-language explanation |
|---|---|---|
| Machine Control Protocol (MCP) | Machine Control Protocol (протокол управления инструментами) | a protocol that lets a language model request the execution of external tools and receive their results in a structured format |
| Named pipe | Named pipe (именованный канал межпроцессного взаимодействия) | a mechanism for two processes on the same machine to exchange data by writing to and reading from a shared named channel |
| Server-Sent Events (SSE) | Server-Sent Events (события отправляемые сервером) | a one-directional HTTP streaming protocol where the server pushes updates to the client without the client polling |
| YAML frontmatter | YAML frontmatter (метаданные в формате YAML в начале Markdown-файла) | a block of structured key-value data at the very top of a Markdown file, delimited by triple dashes, used to store metadata like title and description |
| Abstract Syntax Tree (AST) | Abstract Syntax Tree (абстрактное синтаксическое дерево) | a tree where each node represents a language construct (function declaration, if statement, variable assignment) |
| Roslyn | Microsoft C# Compiler Platform Roslyn (платформа компилятора C# от Microsoft) | an open-source library that exposes the C# compiler as an API, allowing programs to parse and analyze C# code |
| JSON Schema | JSON Schema (схема формата JSON) | a document that describes the expected structure, field names, and types of a JSON object |
| State machine | State machine (конечный автомат) | a system that is always in one of a finite set of named states and transitions between states based on defined rules |
| Checkpoint | State machine checkpoint (контрольная точка состояния автомата) | a saved snapshot of the state machine's current state, allowing it to be paused and resumed later |
| Discriminated union | Discriminated union (размеченное объединение) | a type that can hold one of several named variants, each with its own set of fields; C# uses `record` with a common base for this pattern |

---

## Folder structure

```
23.OssEquivalents/                ← class library
├── Fabric/
│   ├── ILlmClientWithSchema.cs   — calls LLM and validates the response against a schema
│   └── IToolExecutor.cs          — executes a named tool and returns its result
├── Mcp/
│   ├── McpTransport.cs           — MCP server over named pipe (Windows) or HTTP (cross-platform)
│   ├── McpToolDefinition.cs      — describes one tool: name, description, parameter schema
│   └── McpToolDispatcher.cs      — routes incoming tool calls to the right registered handler
├── Frontmatter/
│   ├── FrontmatterParser.cs      — parses YAML frontmatter from Markdown text
│   └── SkillFileLoader.cs        — loads SKILL.md files: metadata only at startup, full content on activation
├── CodeAnalysis/
│   ├── RoslynCodeAnalyzer.cs     — extracts functions, classes, call edges from C# source files
│   └── CallGraphBuilder.cs       — builds a directed call graph from Roslyn analysis output
├── Validation/
│   ├── SchemaValidator.cs        — validates a dictionary against a declared JSON schema
│   └── StructuredLlmClient.cs    — calls the LLM, validates response schema, retries on failure
├── StateMachine/
│   ├── AgentStateMachine.cs      — defines and runs a state machine for the agent loop
│   ├── AgentStateNode.cs         — one node in the state machine (an async function that transforms state)
│   └── StateCheckpointer.cs      — saves and restores state machine checkpoints to SQLite
└── Core/
    ├── BaseService.cs
    ├── OperationResult.cs
    └── StandardEvent.cs

23.OssEquivalents.Desktop/        ← Windows Forms host application
├── Program.cs
├── MainForm.cs                   — sidebar listing 8 tool equivalents; detail + demo panel on right
├── McpDemoForm.cs                — register tools and invoke them from a text input
├── SkillBrowserForm.cs           — browse SKILL.md files in a directory; show metadata and full content
└── StateMachineDemoForm.cs       — define a simple state machine and step through it visually
```

---

## Phase 0 — Fabric interfaces (0.5 day)

```csharp
// ILlmClientWithSchema extends the basic LLM client to require a validated structured response.
// The caller declares the expected output shape as a JSON Schema dictionary.
// If the model returns output that does not match the schema, the call is retried automatically.
// This is the .NET equivalent of the zod + instructor pattern from the OSS plan.
public interface ILlmClientWithSchema
{
    // Call the language model and return the response validated against the given schema
    // messageHistory — conversation history to send
    // expectedSchema — the JSON Schema describing the expected output format
    // maximumRetryCount — how many times to retry before giving up
    Task<OperationResult<Dictionary<string, object>>> SendWithSchemaAsync(
        List<ConversationMessage> messageHistory,
        Dictionary<string, object> expectedSchema,
        int maximumRetryCount = 3);
}

// IToolExecutor runs a named tool with given arguments and returns the result.
// The Machine Control Protocol dispatcher delegates to this interface.
public interface IToolExecutor
{
    // Execute the named tool with the given arguments
    // toolName — the registered name of the tool to run
    // arguments — the input parameters as a dictionary
    Task<OperationResult<Dictionary<string, object>>> ExecuteAsync(
        string toolName,
        Dictionary<string, object> arguments);
}
```

Phase exit: `dotnet build` exits 0.

---

## Phase 1 — Machine Control Protocol transport (2 days)

**What the OSS plan says about this tool:** `@modelcontextprotocol/sdk` is a TypeScript library that lets a language model request execution of external tools. We use it directly in TypeScript. For .NET, we implement the same protocol over Windows named pipes (именованный канал — механизм обмена данными между двумя процессами на одной машине через общий именованный канал).

**What Machine Control Protocol means in plain language:** it is an agreement between the language model host (e.g. Claude Code) and an external tool server. The model says "call tool X with arguments Y" using a specific message format. The server calls the tool and sends back the result in the same format. This allows a language model to use tools implemented in any language.

What to write: `Mcp/McpTransport.cs`, `Mcp/McpToolDefinition.cs`, `Mcp/McpToolDispatcher.cs`

```csharp
// McpTransport implements the Machine Control Protocol server.
// It listens on a Windows named pipe for incoming requests from a language model host.
// Each request is a JSON object describing a tool call; the response is a JSON object with the result.
public class McpTransport : BaseService
{
    // The name of the named pipe to listen on
    // Both the language model host and this server must agree on this name
    private readonly string pipeName;

    // The dispatcher that routes incoming tool calls to the correct handler
    private readonly McpToolDispatcher dispatcher;

    // Start listening for tool call requests on the named pipe
    // cancellationToken — signals when to stop listening (e.g. when the application closes)
    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        // Create a named pipe server — this will block until a client connects
        using var pipeServer = new System.IO.Pipes.NamedPipeServerStream(
            pipeName,
            System.IO.Pipes.PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            System.IO.Pipes.PipeTransmissionMode.Message);

        // Wait for a client (the language model host) to connect
        await pipeServer.WaitForConnectionAsync(cancellationToken);
        Logger.LogInformation("Language model host connected to named pipe '{PipeName}'", pipeName);

        // Read and process messages until the pipe is closed or cancellation is requested
        using var reader = new System.IO.StreamReader(pipeServer);
        using var writer = new System.IO.StreamWriter(pipeServer) { AutoFlush = true };

        while (!cancellationToken.IsCancellationRequested && pipeServer.IsConnected)
        {
            // Read one JSON message from the pipe
            var messageText = await reader.ReadLineAsync();
            if (messageText is null) break;

            // Dispatch the message to the right tool handler and get the result
            var responseResult = await dispatcher.DispatchAsync(messageText);

            // Write the result back to the pipe as a JSON message
            var responseText = responseResult.Success
                ? System.Text.Json.JsonSerializer.Serialize(responseResult.Value)
                : System.Text.Json.JsonSerializer.Serialize(new { error = responseResult.ErrorMessage });

            await writer.WriteLineAsync(responseText);
        }
    }
}
```

Tests:
- Start the transport on a test named pipe; connect from the same process as a client; send a tool call message; verify the dispatcher received it and the response was written back
- Register a tool that returns a fixed string; call it through the transport; verify the result

---

## Phase 2 — YAML frontmatter parser and SKILL.md loader (1 day)

**What the OSS plan says about this tool:** `gray-matter` is a 200-line Node.js library that parses the YAML block at the top of a Markdown file. In .NET, we implement the same logic using `YamlDotNet` (MIT) for the YAML portion and plain string parsing for the Markdown boundary detection.

**What YAML frontmatter means in plain language:** at the very top of a Markdown file, between two lines of three dashes (`---`), you can place structured data in YAML format. `gray-matter` splits the file into the structured part (the frontmatter) and the rest of the text (the content). The SKILL.md format uses this: the frontmatter holds the skill's name and description (loaded at startup), and the content holds the full instructions (loaded only when the skill is activated).

What to write: `Frontmatter/FrontmatterParser.cs`, `Frontmatter/SkillFileLoader.cs`

```csharp
// FrontmatterParser splits a Markdown file with YAML frontmatter into two parts:
// the structured metadata block and the plain text content.
// This is the .NET equivalent of the 'gray-matter' npm package.
public class FrontmatterParser : BaseService
{
    // Parse a Markdown file that may contain a YAML frontmatter block at the top
    // markdownText — the full text of the Markdown file as a string
    // Returns a tuple of:
    //   Metadata — the parsed YAML fields as a dictionary (e.g. name, description, version)
    //   Content  — the Markdown text after the frontmatter block
    public OperationResult<(Dictionary<string, object> Metadata, string Content)> Parse(
        string markdownText)
    {
        // A frontmatter block starts with "---" on the very first line and ends with another "---"
        // If the file does not start with "---", there is no frontmatter
        var lines = markdownText.Split('\n');

        if (lines.Length == 0 || lines[0].Trim() != "---")
        {
            // No frontmatter found — return an empty metadata dictionary and the full text as content
            return OperationResult<(Dictionary<string, object>, string)>.Ok(
                (new Dictionary<string, object>(), markdownText));
        }

        // Find the closing "---" line that ends the frontmatter block
        int closingLineIndex = -1;
        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            if (lines[lineIndex].Trim() == "---")
            {
                // Found the closing delimiter — everything between the two "---" lines is YAML
                closingLineIndex = lineIndex;
                break;
            }
        }

        // If there is an opening "---" but no closing "---", the file is malformed
        if (closingLineIndex == -1)
        {
            return OperationResult<(Dictionary<string, object>, string)>.Fail(
                "Frontmatter block opened with '---' but never closed with a second '---'.",
                "MALFORMED_FRONTMATTER");
        }

        // Extract the YAML text (everything between the two "---" lines)
        string yamlText = string.Join('\n', lines[1..closingLineIndex]);

        // Extract the content text (everything after the closing "---" line)
        string contentText = string.Join('\n', lines[(closingLineIndex + 1)..]).TrimStart();

        // Parse the YAML text into a dictionary using YamlDotNet
        var yamlDeserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
        var metadata = yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlText)
                       ?? new Dictionary<string, object>();

        return OperationResult<(Dictionary<string, object>, string)>.Ok((metadata, contentText));
    }
}
```

`SkillFileLoader.cs` loads all `.md` files from a directory. It parses only the frontmatter at startup (fast, small). The full content is loaded only when `LoadFullContentAsync(skillName)` is called.

Tests:
- Parse a Markdown file with a frontmatter block containing `name: my-skill` and `version: 1.0`; verify the metadata dictionary has those two keys
- Parse a file with no frontmatter; verify the metadata dictionary is empty and the content is the full file text
- Load a directory of three SKILL.md files; verify only three metadata entries exist at startup; call `LoadFullContentAsync` for one; verify only that one has content loaded

---

## Phase 3 — Roslyn code analyzer (2 days)

**What the OSS plan says about this tool:** `ts-morph` is a TypeScript wrapper over the TypeScript compiler API. It extracts function signatures, JSDoc comments, and call relationships. In .NET, the equivalent is Roslyn (Microsoft C# Compiler Platform — библиотека позволяющая разбирать и анализировать C# код как структуры данных), which provides the same capabilities for C# code.

What to write: `CodeAnalysis/RoslynCodeAnalyzer.cs`, `CodeAnalysis/CallGraphBuilder.cs`

`RoslynCodeAnalyzer` extracts for every C# source file:
- All class names and their XML documentation comments
- All method names, parameter lists with types, and return types
- All method invocations (to build call edges)
- All `using` directives (to build import edges)

The output is a list of `Dictionary<string, object>` entries, one per symbol, with keys: `"name"`, `"kind"` (Method/Class/Interface), `"signature"`, `"documentation"`, `"filePath"`, `"calledBy"` (list of caller names).

```csharp
// RoslynCodeAnalyzer extracts structural information from C# source files.
// It uses Roslyn (Microsoft's C# compiler as a library) to parse each file
// and extract names, signatures, documentation, and relationships.
// This is the .NET equivalent of ts-morph from the OSS plan.
public class RoslynCodeAnalyzer : BaseService
{
    // Analyze all .cs files under the given directory
    // Returns one dictionary per symbol (method or class) found
    public async Task<OperationResult<List<Dictionary<string, object>>>> AnalyzeDirectoryAsync(
        string directoryPath)
    {
        // Find all C# source files recursively under the given directory
        var sourceFilePaths = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

        var allSymbols = new List<Dictionary<string, object>>();

        // Process each source file independently
        foreach (var filePath in sourceFilePaths)
        {
            // Read the file text from disk
            var fileText = await File.ReadAllTextAsync(filePath);

            // Parse the file using Roslyn to produce a syntax tree
            // A syntax tree is a hierarchical structure representing every construct in the file
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(fileText);

            // Get the root node of the syntax tree (the compilation unit, i.e. the whole file)
            var rootNode = await syntaxTree.GetRootAsync();

            // Extract all class declarations from the tree
            var classDeclarations = rootNode
                .DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                // Build a dictionary describing this class
                allSymbols.Add(new Dictionary<string, object>
                {
                    // The simple name of the class (without namespace)
                    ["name"] = classDeclaration.Identifier.Text,
                    // The kind of symbol this is
                    ["kind"] = "Class",
                    // The full signature as it appears in the source
                    ["signature"] = classDeclaration.Identifier.Text,
                    // The XML documentation comment immediately above the class declaration, if any
                    ["documentation"] = ExtractXmlDocumentation(classDeclaration),
                    // The path of the file where this class is defined
                    ["filePath"] = filePath
                });
            }

            // ... similar extraction for methods ...
        }

        return OperationResult<List<Dictionary<string, object>>>.Ok(allSymbols);
    }

    // Extract XML documentation text from a syntax node's leading trivia (комментарии перед кодом)
    private static string ExtractXmlDocumentation(
        Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode syntaxNode)
    {
        // Documentation comments appear as "leading trivia" — text before the node
        var documentationLines = syntaxNode.GetLeadingTrivia()
            .Where(trivia => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(trivia => trivia.ToString().TrimStart('/', ' '));

        return string.Join(" ", documentationLines).Trim();
    }
}
```

Tests:
- Analyze a 3-class test file; verify the output contains 3 class entries with correct names
- Verify that XML documentation comments are extracted correctly
- Verify that a method calling another method produces a call edge

---

## Phase 4 — Schema validator and structured language model client (2 days)

**What the OSS plan says:** `zod` defines schemas; `instructor` wraps LLM calls to guarantee the response matches the schema. The retry loop sends the validation error back to the model as a correction message. This is the "arbiter pattern".

**In .NET:** `SchemaValidator` checks a `Dictionary<string, object>` against a declared schema. `StructuredLlmClient` wraps `ILlmClient`, calls the model, validates the result, and retries with an error correction message if validation fails.

```csharp
// StructuredLlmClient calls the language model and validates that the response
// matches the expected structure. If it does not match, it sends the error back
// to the model as a correction request and tries again.
// This is the .NET equivalent of 'instructor' from the OSS plan.
public class StructuredLlmClient : BaseService
{
    // The underlying language model client that sends and receives messages
    private readonly ILlmClient rawLlmClient;

    // The validator that checks whether a response dictionary matches a schema
    private readonly SchemaValidator schemaValidator;

    // Call the language model and return a validated structured response
    // messageHistory — conversation history to send to the model
    // expectedSchema — a dictionary describing required fields and their types
    //   (e.g. { "skill_name": "string", "confidence": "number" })
    // maximumRetryCount — how many times to retry before giving up and returning failure
    public async Task<OperationResult<Dictionary<string, object>>> SendAsync(
        List<ConversationMessage> messageHistory,
        Dictionary<string, object> expectedSchema,
        int maximumRetryCount = 3)
    {
        // Keep track of how many attempts have been made
        int attemptCount = 0;

        // Add a system instruction to the conversation explaining the expected output format
        var enrichedHistory = new List<ConversationMessage>(messageHistory);
        enrichedHistory.Insert(0, new ConversationMessage
        {
            Role = "system",
            Content = $"You must respond with a JSON object that matches this schema: " +
                      $"{System.Text.Json.JsonSerializer.Serialize(expectedSchema)}"
        });

        while (attemptCount < maximumRetryCount)
        {
            // Call the language model and get the raw response text
            var rawResult = await rawLlmClient.SendAsync(enrichedHistory, tools: null, CancellationToken.None);
            if (!rawResult.Success)
            {
                // The language model call itself failed — do not retry, return the error
                return OperationResult<Dictionary<string, object>>.Fail(rawResult.ErrorMessage, "LLM_CALL_FAILED");
            }

            // Attempt to parse the response text as a JSON dictionary
            Dictionary<string, object>? parsedResponse = null;
            try
            {
                parsedResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    rawResult.Value!.ResponseText);
            }
            catch (System.Text.Json.JsonException)
            {
                // The response was not valid JSON — treat this as a validation failure
            }

            // Validate the parsed response against the expected schema
            if (parsedResponse is not null)
            {
                var validationResult = schemaValidator.Validate(parsedResponse, expectedSchema);
                if (validationResult.Success)
                {
                    // The response matches the schema — return it
                    return OperationResult<Dictionary<string, object>>.Ok(parsedResponse);
                }

                // The response did not match — add the error as a correction message and retry
                enrichedHistory.Add(new ConversationMessage { Role = "assistant", Content = rawResult.Value!.ResponseText });
                enrichedHistory.Add(new ConversationMessage
                {
                    Role = "user",
                    Content = $"Your response did not match the required format. " +
                              $"Validation error: {validationResult.ErrorMessage}. " +
                              $"Please respond again with a valid JSON object."
                });
            }

            // Increment the attempt counter before the next try
            attemptCount++;
        }

        // All retries exhausted — return failure
        return OperationResult<Dictionary<string, object>>.Fail(
            $"Language model did not produce a valid response after {maximumRetryCount} attempts.",
            "SCHEMA_VALIDATION_EXHAUSTED");
    }
}
```

Tests:
- Call with a mock that returns valid JSON on the first attempt; verify exactly one call was made
- Call with a mock that returns invalid JSON twice then valid JSON; verify three calls were made and the result is valid
- Call with a mock that always returns invalid JSON; verify failure after `maximumRetryCount` attempts

---

## Phase 5 — Agent state machine (2 days)

**What the OSS plan says about LangGraph:** a state machine where each node is an async function that receives the current state and returns a state delta. Edges define transitions; conditional edges define branching. Checkpoints save the state to SQLite so the machine can be resumed.

**In .NET:** `AgentStateMachine` is a hand-rolled state machine that follows the same model. No external state machine library is needed — the pattern is simple enough to implement directly.

```csharp
// AgentStateMachine runs an agent as an explicit state machine.
// Each state is identified by a string name.
// Transitions are defined as a dictionary from (current state, condition) to next state.
// The state machine saves a checkpoint after every transition so it can be resumed.
//
// This is the .NET equivalent of @langchain/langgraph from the OSS plan.
public class AgentStateMachine : BaseService
{
    // All registered state node functions, keyed by state name
    // Each function receives the current agent state and returns the updated state
    private readonly Dictionary<string, Func<Dictionary<string, object>,
                                              Task<Dictionary<string, object>>>> stateNodes = new();

    // All registered transitions: from state name → list of (condition, next state name) pairs
    // A condition is a function that looks at the state and returns true if this edge should be followed
    private readonly Dictionary<string, List<(Func<Dictionary<string, object>, bool> Condition, string NextState)>> transitions = new();

    // The checkpointer that saves and restores state between transitions
    private readonly StateCheckpointer checkpointer;

    // Register a state node function
    // stateName — the name of the state this function handles
    // nodeFunction — the async function that runs when the machine enters this state
    public void AddNode(string stateName,
                        Func<Dictionary<string, object>, Task<Dictionary<string, object>>> nodeFunction)
    {
        // Store the function under its state name so the run loop can call it
        stateNodes[stateName] = nodeFunction;
    }

    // Add a conditional transition from one state to another
    // fromState — the state we are leaving
    // condition — a function that returns true when this transition should fire
    // toState — the state to enter when the condition is true
    public void AddConditionalEdge(string fromState,
                                   Func<Dictionary<string, object>, bool> condition,
                                   string toState)
    {
        // Add the condition-destination pair to the list of transitions for this state
        if (!transitions.ContainsKey(fromState))
            transitions[fromState] = new List<(Func<Dictionary<string, object>, bool>, string)>();

        transitions[fromState].Add((condition, toState));
    }

    // Run the state machine from an initial state until it reaches a terminal state
    // initialState — the starting state dictionary (contains the task, message history, etc.)
    // startingStateName — the name of the first state to enter
    // terminalStateName — the state whose name signals that the run is complete
    public async Task<OperationResult<Dictionary<string, object>>> RunAsync(
        Dictionary<string, object> initialState,
        string startingStateName,
        string terminalStateName)
    {
        // Begin with the initial state
        var currentState = initialState;
        var currentStateName = startingStateName;

        // Keep running until we reach the terminal state or an error occurs
        while (currentStateName != terminalStateName)
        {
            // Look up the function registered for the current state
            if (!stateNodes.TryGetValue(currentStateName, out var nodeFunction))
            {
                return OperationResult<Dictionary<string, object>>.Fail(
                    $"No node function registered for state '{currentStateName}'.",
                    "UNKNOWN_STATE");
            }

            // Execute the state function to produce the updated state
            var updatedState = await nodeFunction(currentState);

            // Save a checkpoint after this transition so we can resume if needed
            await checkpointer.SaveAsync(currentStateName, updatedState);

            // Find which transition condition is satisfied by the updated state
            string? nextStateName = null;
            if (transitions.TryGetValue(currentStateName, out var conditionalEdges))
            {
                foreach (var (condition, targetState) in conditionalEdges)
                {
                    if (condition(updatedState))
                    {
                        // This condition matched — follow this edge
                        nextStateName = targetState;
                        break;
                    }
                }
            }

            // If no conditional edge matched, there is no path forward — report an error
            if (nextStateName is null)
            {
                return OperationResult<Dictionary<string, object>>.Fail(
                    $"No transition condition matched from state '{currentStateName}'.",
                    "NO_TRANSITION");
            }

            // Move to the next state
            currentState = updatedState;
            currentStateName = nextStateName;
        }

        // We have reached the terminal state — return the final state
        return OperationResult<Dictionary<string, object>>.Ok(currentState);
    }
}
```

Tests:
- Define a 3-node machine (analyze → execute → respond); run it with mock functions; verify all three nodes were called in order
- Verify the checkpointer saved a snapshot after each transition
- Reload a checkpoint mid-run; verify the machine continues from the saved point

---

## Phase 6 — Windows Forms demo host (1.5 days)

Three separate demo windows, each accessible from the main form's menu:

**Machine Control Protocol demo** (`McpDemoForm`): displays a list of registered tools on the left; a JSON input area in the center; a "Send" button that calls the tool through the transport and shows the result.

**SKILL.md browser** (`SkillBrowserForm`): shows a directory picker; lists all discovered SKILL.md files with their metadata (name, description); clicking a skill loads and shows its full content in a text area on the right.

**State machine visualizer** (`StateMachineDemoForm`): shows a pre-defined 4-node agent loop as a list of state names; a "Step" button runs one transition and highlights which state is now active; the state dictionary contents are shown in a panel below.

Phase exit: all three demo forms open without errors and produce visible output.

---

## Plan Review Checks

**Goal delivery:** all 8 tools from `oss-plan-summary.md` are accounted for — 4 implemented in this sub-project, 3 reused from existing sub-projects, 1 (nano-graphrag equivalent) reused from sub-project 08. ✓  
**Authentication check:** no HTTP controllers. Machine Control Protocol transport uses named pipes, not HTTP. No auth declaration needed. ✓  
**Fabric-first check:** `ILlmClientWithSchema`, `IToolExecutor` injected — no concrete classes instantiated inside services. ✓  
**Count accuracy:** 8 tools → 5 implementation phases (phases 1–5) + 1 UI phase. ✓

---

## Summary table

| Phase | What is built | OSS equivalent | Days |
|---|---|---|---|
| 0 | Fabric interfaces | — | 0.5 |
| 1 | Machine Control Protocol transport | `@modelcontextprotocol/sdk` | 2 |
| 2 | YAML frontmatter parser + SKILL.md loader | `gray-matter` | 1 |
| 3 | Roslyn code analyzer | `ts-morph` | 2 |
| 4 | Schema validator + structured language model client | `zod` + `instructor` | 2 |
| 5 | Agent state machine + checkpointer | `@langchain/langgraph` | 2 |
| 6 | Windows Forms demo host (3 demo windows) | — | 1.5 |
| Reused | Vector store | `vectra` → sub-project 06 | 0 |
| Reused | Graph + PageRank | `graphology` → sub-project 07 | 0 |
| Reused | Graph retrieval-augmented generation | `nano-graphrag` → sub-project 08 | 0 |
| **Total** | | | **11 days** |

---

## Dependency map

```
23.OssEquivalents
  depends on (for reused components):
    06.VectorSearch          — IVectorStorage (vectra equivalent)
    07.GraphEngine           — IGraphStorage, PageRankCalculator (graphology equivalent)
    08.GraphRag              — HybridRetriever (nano-graphrag equivalent)
  new external packages:
    YamlDotNet               — YAML parsing in FrontmatterParser
    Microsoft.Data.Sqlite    — checkpoint storage in StateCheckpointer
    Microsoft.CodeAnalysis.CSharp — code parsing in RoslynCodeAnalyzer
```

---

*Plan v1.0 — sub-project 23, source: oss-plan-summary.md. Does not touch XIIGen or sub-projects 01–22.*
