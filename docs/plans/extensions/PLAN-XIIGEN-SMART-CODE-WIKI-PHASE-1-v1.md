# Plan — XIIGen Smart Code Wiki: Phase 1
# Version: 1.0 — 2026-05-14
# Session type: ARCHITECT
# Applies to: PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-v3-CLAUDE.md
#             PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CURSOR-PORT-v2.md
#             PLAN-XIIGEN-TOKEN-OPTIMIZER-PLUGIN-CODEX-PORT-v2.md
#             XIIGen-Desktop-Plugin-Adaptation-Report-v2.md (Visual Studio section)
#
# Goal: Add Phase 1 to all AI coding tool plugins — a smart, code-based wiki that
# indexes the codebase statically and injects relevant context into every AI prompt.
# No AI generation at Phase 1. This is deterministic code analysis only.

---

## 2026-05-24 Architecture Alignment Addendum

This document describes the original Phase 1 foundation only. It is not enough
by itself to claim readiness for the Visual Studio and Visual Studio Code
products:

```text
https://github.com/lubochka/xiigen_visual_studio
https://github.com/lubochka/xiigen_visual_code
```

The product readiness target is larger than static wiki indexing. The libraries
for these repositories must support the current XIIGen project-local wiki goal:

```text
developer prompt
  -> route to project wiki capability
  -> resolve project system boundary
  -> plan wiki queries
  -> retrieve Angular client evidence
  -> retrieve .NET server evidence
  -> retrieve SQL stored procedure evidence
  -> compose an evidence-bound answer
  -> report missing evidence instead of guessing
```

For this reason, Phase 1 is now only the basic local indexing and UI foundation.
The plugin libraries are not ready until they also satisfy the shared contract
below.

### Shared library contract for Visual Studio and Visual Studio Code

Both `xiigen_visual_studio` and `xiigen_visual_code` must consume the same
project wiki protocol. They must not invent separate answer shapes.

Required shared contracts:

```text
XIIGen.SmartCodeWiki.Client
  WikiRequest
  WikiAnswer
  WikiReadinessStatus
  WikiRelevanceMap
  WikiContextSlice
  ProjectSystemMap
  ProjectWikiUnitManifest
  ProjectWikiImportExportStatus
```

Required protocol files consumed by both plugins:

```text
.xiigen/wiki-domain/evidence/evidence-index.jsonl
.xiigen/wiki-domain/model-manifest.json
.xiigen/wiki-domain/system-map.json
.xiigen/wiki-domain/wiki-readiness.json
```

Required learning/project asset sources in `llm_mvp_core`:

```text
42.ProjectWikiDomain
43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json
43.LearningLocalWikiData/validation/CROSS-STACK-SYSTEM-MAP.json
43.LearningLocalWikiData/validation/ADVANCED-WIKI-QUESTION-SUITE-v1.json
43.LearningLocalWikiData/validation/ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json
```

### Readiness status is not only PROMOTED

The old binary state `MOCK -> PROMOTED` is not enough.

Every plugin must surface one of these states:

```text
not_indexed
indexing
basic_wiki_ready
training_missing
import_export_missing
system_map_missing
advanced_prompt_suite_missing
advanced_wiki_ready
blocked_with_gaps
```

`advanced_wiki_ready` requires all of the following:

```text
model manifest exists
registry accepted unit has manifest_ref
registry accepted unit has locator_key
registry accepted unit has remote_unit_id
system map exists for Angular + .NET + SQL stored procedures when applicable
advanced wiki question suite exists
orchestrator prompt planning suite exists
latest readiness report contains no blocker
```

If any item is missing, the plugin may still show the basic wiki, but it must
not claim full smart wiki readiness.

### Multi-root and linked-project requirement

VS Code must not use only the first workspace folder. It must collect all
workspace folders and pass them to the wiki layer.

Visual Studio must not assume the `.sln` is the whole business system. It must
allow linked client and database roots so a .NET solution can be connected to:

```text
Angular client project
.NET server project
SQL stored procedure source
```

### Time and timeout rule

Short UI calls may return cached or partial status quickly, but timeout values
are not project stop gates.

Long indexing, training, import/export validation, and system-map building must
run as background or resumable operations with persisted state. A timeout must
produce a status such as `indexing` or `blocked_with_gaps`, not a false
completion claim.

### Product readiness gate for VS and VS Code

The libraries for `xiigen_visual_studio` and `xiigen_visual_code` are ready only
when both plugins can:

```text
load the shared contract
read readiness state
show basic wiki results
show advanced readiness gaps
handle multi-root or linked projects
display Angular + .NET + SQL as one system when system-map evidence exists
inject or copy evidence-bound context
avoid claiming full readiness when only basic wiki exists
```

This addendum supersedes any earlier sentence that implies static Phase 1 alone
is enough for the current product goal.

## What Phase 1 Is — And What It Is Not

**Is:** A static code intelligence layer built from Roslyn analysis of the
open solution or workspace. It produces a structured, searchable snapshot of
the codebase — classes, methods, interfaces, data flow edges, dependency
relationships — and injects the most relevant slices into every AI prompt
before the model sees it.

**Is not:** AI tab completion. Not a co-pilot. Not code generation.

The mental model is a **code-aware wiki that travels with the prompt**. When a
developer types "why does UserService fail on tenant isolation", the wiki layer
silently retrieves the relevant class summary, its dependencies, its interface
contract, and its known data flow, and prepends that context to the prompt. The
AI receives a richer starting point and wastes no tokens re-reading files it
would otherwise have to be handed manually.

The value proposition before any AI improvement: a developer can open a panel
in Visual Studio, type a question, and get answers that are grounded in the
actual code structure — not hallucinated generalizations.

---

## Why This Comes Before The Other Hooks

The existing five hooks (Orchestration Router, Skill Selector, Context Injector,
Output Validator, Task Completion Guard) reduce token waste and improve focus. But
they operate on domain labels and skill files — they do not know the actual
codebase the developer is working in.

Phase 1 adds the missing foundation: a live, indexed representation of the
codebase that every subsequent hook can draw from. Hook 2 (Skill Selector) can
rank skills using real class names. Hook 3 (Context Injector) can include the
relevant interface contract, not just a generic snapshot. The wiki does not
replace the existing hooks — it makes all of them more precise.

Sequencing rule: Phase 1 wiki build runs at workspace open (once). Wiki context
hook fires before Hook 1 (Orchestration Router). All five existing hooks continue
unchanged.

---

## New Component: 41.CodeWiki

A new project in `llm_mvp_core` that builds and queries the code wiki.

```
llm_mvp_core/
  41.CodeWiki/
    CodeWiki.csproj
    Core/
      WikiEntry.cs              ← one entry per class, interface, enum
      DataFlowEdge.cs           ← directed dependency: source → target + kind
      WikiSnapshot.cs           ← full serializable snapshot
    Builder/
      SolutionWikiBuilder.cs    ← entry point: scan solution → WikiSnapshot
      RoslynFileScanner.cs      ← Roslyn walker: extract metadata per file
      DataFlowTracer.cs         ← trace constructor/field/method deps → edges
      WorkspaceFileScanner.cs   ← fallback for non-.NET projects: tree-sitter stub
    Index/
      WikiBm25Index.cs          ← BM25 index over wiki entry descriptions
                                   (reuses SearchEngine from 12.SearchEngine)
      WikiContextRetriever.cs   ← given a query string, return top-N entries
    Persistence/
      WikiSnapshotPersistence.cs ← write/read .xiigen/wiki/snapshot.json
      WikiChangeWatcher.cs       ← FileSystemWatcher: mark snapshot stale on save
    Output/
      WikiContextFormatter.cs    ← format retrieved entries as compact comment block
      WikiMermaidExporter.cs     ← export dependency graph as Mermaid diagram
```

### WikiEntry — one record per class/interface/enum

```csharp
public sealed record WikiEntry(
    string Id,                    // "MyNamespace.UserService"
    string Kind,                  // "class" | "interface" | "enum" | "record"
    string Name,                  // "UserService"
    string Namespace,
    string FilePath,
    int LineNumber,
    string Summary,               // XML doc comment <summary> or first comment line
    string[] ImplementedInterfaces,
    string[] BaseTypes,
    MethodSummary[] PublicMethods,
    string[] InjectedDependencies, // constructor param types
    string[] KnownCallers,         // types that reference this type
    DataFlowEdge[] OutboundEdges);
```

### DataFlowEdge — one directed relationship

```csharp
public sealed record DataFlowEdge(
    string SourceId,    // "MyNamespace.UserService"
    string TargetId,    // "MyNamespace.ISessionStorage"
    string Kind,        // "implements" | "injects" | "calls" | "returns" | "inherits"
    string Context);    // "constructor parameter" | "method return type" | etc.
```

### WikiSnapshot — what gets persisted

```csharp
public sealed record WikiSnapshot(
    string SolutionPath,
    DateTimeOffset BuiltAt,
    bool IsStale,          // set true by FileSystemWatcher on file save
    WikiEntry[] Entries,
    DataFlowEdge[] Edges);
```

---

## Build Process — What Roslyn Extracts

`SolutionWikiBuilder` walks every `.cs` file in the solution or workspace folder.
For each file it calls `RoslynFileScanner` which uses the Roslyn syntax tree
(no compilation required — syntax analysis only, fast, no build needed).

Per class/interface/enum, it extracts:

```
1. Full qualified name, kind, file path, line number
2. XML doc <summary> comment (first line if no XML doc)
3. Implemented interfaces (`:` list)
4. Base type
5. Public method signatures (name, parameters, return type, summary comment)
6. Constructor parameters → InjectedDependencies
   These become the primary data flow edges (injects)
7. Fields and properties that are interface types → additional injects
```

`DataFlowTracer` runs a second pass over the entries to fill KnownCallers:
for each entry, scan all other entries' InjectedDependencies for references
to this type's name or its interfaces. This gives the reverse graph
(who calls me) without requiring full compilation.

**No compilation required.** This is the key performance constraint: the wiki
must build in under 10 seconds for a solution with 50 files, and under 60 seconds
for a solution with 500 files. Syntax-only analysis meets this target.
Semantic analysis (full compilation) is Phase 2.

---

## Wiki Index and Retrieval

`WikiBm25Index` builds a BM25 index (reusing `12.SearchEngine.SearchEngine`) over
a text representation of each entry:

```
"{Kind} {Name} {Namespace} {Summary} {ImplementedInterfaces.Join} {PublicMethods.Select(m => m.Name).Join}"
```

`WikiContextRetriever.Retrieve(string query, int topN)`:

1. Run BM25 query over the index.
2. Also run exact name match — if the query contains a word that exactly matches
   an entry name, that entry is boosted to rank 1 regardless of BM25 score.
3. Return top-N entries sorted by combined score.
4. For each returned entry, include its immediate data flow edges (one hop only).

The output of retrieval is a `WikiContextSlice`:

```csharp
public sealed record WikiContextSlice(
    WikiEntry[] PrimaryEntries,
    DataFlowEdge[] RelevantEdges,
    string FormattedBlock);   // pre-formatted for injection
```

`WikiContextFormatter` produces the `FormattedBlock` as a compact, structured
comment that is readable by the AI and by the developer:

```
--- XIIGen Code Context ---
[UserService : IUserService]
  File: Services/UserService.cs:42
  Summary: Handles tenant-scoped user operations.
  Injects: ISessionStorage, IUserRepository, IPasswordHasherService
  Public methods: CreateAsync(CreateUserDto) → OperationResult<UserDto>
                  GetByIdAsync(string userId, string tenantId) → OperationResult<UserDto>
  Known callers: AuthController, UserMigrationService
[IUserService]
  File: Contracts/IUserService.cs:8
  Summary: Contract for tenant-scoped user management.
  Implemented by: UserService
Data flow: AuthController → injects → IUserService
           UserService → injects → ISessionStorage
--- End Code Context ---
```

This block is under 400 tokens for two entries with edges. It gives the AI
precise structural knowledge without requiring it to read the files.

---

## New Hook 0 — WikiContextHook

This is the new Phase 1 hook. It fires before all five existing hooks.

### Position in the hook sequence

```
(NEW) Hook 0: WikiContextHook     ← UserPromptSubmit / beforeSubmitPrompt
Hook 1:       OrchestrationRouter ← reads hook-state.json written by Hook 0
Hook 2:       SkillSelector       ← reads hook-state.json
Hook 3:       ContextInjector     ← now reads wiki slice from hook-state.json
Hook 4:       OutputValidator     ← unchanged
Hook 5:       TaskCompletionGuard ← unchanged
```

### What Hook 0 does

```
1. Read .xiigen/wiki/snapshot.json
   If absent or stale: trigger SolutionWikiBuilder.BuildAsync() synchronously
   (first run: ~5–30s; subsequent: <500ms if snapshot is fresh)

2. Extract query from HOOK_PROMPT environment variable

3. Call WikiContextRetriever.Retrieve(query, topN: 5)

4. Write to .xiigen/hook-state.json:
   {
     "domain": null,              ← filled by Hook 1
     "subPhases": [],             ← filled by Hook 1
     "wikiSlice": {
       "entries": [...],          ← for Hook 3 to use
       "formattedBlock": "..."    ← injected into system prompt
     },
     "ts": 1746600000000
   }

5. Write formattedBlock to stdout as additional system prompt injection:
   { "systemPromptAddition": "<formattedBlock>" }
```

### Hook 0 in each platform

**Claude Code (`hooks.json`):**

```json
{
  "hooks": {
    "UserPromptSubmit": [
      { "command": "bin/wiki-context-hook.exe", "timeout": 30000 },
      { "command": "bin/orchestration-router.exe" },
      { "command": "bin/skill-selector.exe" }
    ]
  }
}
```

**Cursor (`.cursor/hooks.json`):**

```json
{
  "version": 1,
  "hooks": {
    "beforeSubmitPrompt": [
      { "command": "bin/wiki-context-hook.exe" },
      { "command": "bin/orchestration-router.exe" }
    ]
  }
}
```

**Codex (`plugin.json`):**

```json
{
  "hooks": {
    "before_prompt": [
      { "exec": "bin/wiki-context-hook.exe" },
      { "exec": "bin/orchestration-router.exe" }
    ]
  }
}
```

### New exe: wiki-context-hook.exe

```
41.CodeWiki/HookRunner/Program.cs

Inputs:
  env HOOK_PROMPT            — the developer's prompt text
  env XIIGEN_SOLUTION_PATH   — path to .sln file or workspace root
                               (defaults to current working directory)
  env WIKI_TOP_N             — how many entries to retrieve (default: 5, Pro: 10)

Steps:
  1. Load or build wiki snapshot
  2. Retrieve top-N entries matching the prompt
  3. Format the block
  4. Write to .xiigen/hook-state.json (wikiSlice field)
  5. Print JSON to stdout: { "systemPromptAddition": "..." }

Timing target:
  Cold start (wiki absent): ≤30s (acceptable as one-time build)
  Warm (wiki present, fresh): ≤500ms
  Stale refresh (file changed): ≤5s (incremental, only changed files)
```

### Free vs Pro limits

| Feature | Free | Pro |
|---|---|---|
| Entries retrieved per prompt | Top 3 | Top 10 |
| Snapshot refresh on file save | Manual trigger | Automatic (FileSystemWatcher) |
| Data flow edges included | Direct only (1 hop) | Transitive (2 hops) |
| Mermaid diagram export | No | Yes |
| Multi-project solutions | Single project | Full solution |

---

## Visual Studio Plugin — Phase 1

The Visual Studio plugin adaptation (from the Desktop Adaptation Report, Wave 2)
gets Phase 1 as a dedicated tool window alongside the AI engines.

### What Phase 1 adds to the Visual Studio plugin

Phase 1 does not touch AI completion or the code editor directly. It adds:

**1. Code Wiki tool window (View → XIIGen → Code Wiki)**

A panel with three tabs:

- **Browse tab:** tree view of all indexed entries, grouped by namespace.
  Click any entry → see its summary, method list, injects, callers, data flow.
- **Ask tab:** a text field. Developer types a question about the code.
  The wiki retriever runs, the formatted context block is shown in a read-only
  panel. A "Send to AI" button appends this context + the question to a new
  Claude/Copilot chat window.
- **Flow tab:** Mermaid diagram of the dependency graph for the currently open
  file's classes, rendered inline using a WebView2 control.

**2. Solution Explorer integration**

Right-click a `.cs` file → "Add to Wiki context". This pins that file's entries
to the top of the next retrieval, regardless of BM25 score.

**3. Status bar indicator**

A small indicator in the VS status bar: "XIIGen Wiki: 247 entries | Fresh" or
"XIIGen Wiki: 247 entries | Stale — 3 files changed". Click to trigger rebuild.

### What Phase 1 does NOT add to Visual Studio

- No AI tab completion injection (that is Phase 2).
- No modification to the editor experience.
- No changes to IntelliSense.

The plugin at Phase 1 is a documentation and navigation tool. Developers can
use it without ever opening an AI prompt.

### Visual Studio adapter code additions

```
VisualStudioPlugin/
  CodeWiki/
    CodeWikiToolWindow.cs       ← ToolWindowPane subclass
    CodeWikiBrowseControl.xaml  ← TreeView for entry browsing
    CodeWikiAskControl.xaml     ← Ask + result panel
    CodeWikiFlowControl.xaml    ← WebView2 Mermaid renderer
    WikiStatusBarManager.cs     ← status bar indicator
    SolutionEventWatcher.cs     ← triggers rebuild on solution open/save
```

The tool window calls `41.CodeWiki` directly (same assembly — no subprocess
required, since the Visual Studio extension is already .NET). For VS Code,
JetBrains, and Cursor, the wiki logic runs as a subprocess (wiki-context-hook.exe).

---

## Integration with Existing Hook 3 (Context Injector)

The existing Context Injector (`hooks/context-injector.exe`) currently injects
a generic snapshot from `.claude/hook-state.json`. In Phase 1 that snapshot now
includes the `wikiSlice.formattedBlock`.

Context Injector change: read `hookState.wikiSlice.formattedBlock` if present,
and prepend it before the existing skill context. Total injected block structure:

```
--- XIIGen Code Context ---      ← from wiki (Phase 1)
[class summaries, data flow]
--- End Code Context ---

--- XIIGen Session Context ---   ← existing Hook 3 output
[domain, sub-phases, prior decisions]
--- End Session Context ---
```

No other changes to Hook 3.

---

## Phase 1 Implementation Sequence

### Step 1 — Build 41.CodeWiki core (Gate B: unit tests pass)

```
Deliverables:
  WikiEntry, DataFlowEdge, WikiSnapshot records
  RoslynFileScanner — extract entries from one .cs file
  DataFlowTracer — build edges from entries
  SolutionWikiBuilder — walk all files → WikiSnapshot
  WikiSnapshotPersistence — write/read snapshot.json
  WikiBm25Index + WikiContextRetriever
  WikiContextFormatter

Gate B:
  dotnet test --filter "CodeWiki" → all tests pass
  SolutionWikiBuilder.BuildAsync(llm_mvp_core path) completes in ≤60s
  WikiSnapshot.Entries.Length > 100 (verifiable with grep count)
```

### Step 2 — Build wiki-context-hook.exe (Gate B: hook fires in Claude Code)

```
Deliverables:
  41.CodeWiki/HookRunner/Program.cs
  hooks.json updated: WikiContextHook first in UserPromptSubmit chain
  hook-state.json schema extended with wikiSlice field

Gate B:
  claude --plugin-dir ./xiigen-token-optimizer (on a real .NET project)
  Send prompt: "why does UserService fail on tenant isolation"
  cat .xiigen/hook-state.json | jq '.wikiSlice.entries | length' → ≥ 1
  cat .xiigen/hook-state.json | jq '.wikiSlice.formattedBlock' → non-empty string
```

### Step 3 — Port to Cursor and Codex (Gate B: hook fires in both)

```
Deliverables:
  .cursor/hooks.json with wiki-context-hook first in beforeSubmitPrompt
  Codex plugin.json with wiki-context-hook first in before_prompt
  WikiContextHook exit path for Cursor: output JSON { "systemPromptAddition": "..." }

Gate B:
  Cursor: open project, send prompt, confirm hook fires via hook log
  Codex:  open project, send prompt, confirm hook fires via hook log
```

### Step 4 — Visual Studio tool window (Gate B: window opens and shows entries)

```
Deliverables:
  CodeWikiToolWindow, Browse/Ask/Flow controls
  WikiStatusBarManager
  SolutionEventWatcher

Gate B:
  Open VS → View → XIIGen → Code Wiki → tool window opens
  Browse tab shows ≥ 1 namespace node
  Ask tab: type "UserService" → result panel shows ≥ 1 entry with methods
  Status bar shows entry count and freshness
```

### Step 5 — Context Injector integration (Gate B: formatted block appears in prompt)

```
Deliverables:
  Updated context-injector.exe reads wikiSlice from hook-state.json

Gate B:
  dotnet test --filter "ContextInjector" → all pass (including new wiki slice test)
  End-to-end: Claude Code prompt → wiki block + session context both visible
  in injected system prompt (verify via hook log)
```

---

## What Phase 2 Will Add (not in scope here — for reference only)

- Semantic analysis (full compilation, method call graph) for deeper data flow.
- AI-generated summaries for classes that have no XML doc comments.
- Cross-file search: find all callers of a method across the whole solution.
- Visual Studio: inject wiki context automatically into Copilot Chat / Claude
  integration (requires VS extensibility API for AI chat panel injection).
- Support for TypeScript and Python workspaces via tree-sitter (replacing the
  stub in WorkspaceFileScanner).
- Wiki-based question answering endpoint (local HTTP service) so all adapters
  can query the wiki over HTTP instead of subprocess.

---

## Key Constraints

1. **No AI in Phase 1.** The wiki is built from static analysis only. Entry
   summaries come from XML doc comments or the first code comment above the
   declaration. If no comment exists, the summary is blank — never hallucinated.

2. **No compilation required.** Roslyn syntax tree analysis only. The wiki must
   work even if the solution has build errors, missing NuGet packages, or is
   mid-refactor.

3. **Snapshot must be incremental.** When a file is saved, only that file's
   entries are rebuilt. The full solution is not re-scanned.

4. **The wiki block must be compact.** The formatted block for top-5 entries
   must not exceed 600 tokens. Use abbreviations (type short names, not full
   qualified names) in the formatted output. Full qualified names are stored
   in the snapshot but not injected.

5. **The hook must not block the prompt.** If wiki-context-hook.exe fails or
   times out (30 second limit), Claude Code / Cursor / Codex must continue
   without the wiki context. The hook writes an error to the hook log and
   exits cleanly with no stdout output. The prompt proceeds normally.

6. **One binary, all three AI tools.** `wiki-context-hook.exe` is the same
   executable for Claude Code, Cursor, and Codex. Only the hook config file
   name and the output JSON shape differ per platform (already handled by the
   Cursor port pattern from the existing plan).
