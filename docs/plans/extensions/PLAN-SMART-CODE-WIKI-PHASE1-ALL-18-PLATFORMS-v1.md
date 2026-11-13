# PLAN — XIIGen Smart Code Wiki Phase 1: Per-Platform Implementation Analysis
# Version: 1.0 — 2026-05-14
# Session type: ARCHITECT
# Scope: All 18 supported desktop platforms
#        (19 platforms from the Adaptation Report minus Zed — WASM sandbox blocks subprocess)
# Discipline: Every claim is platform-specific, not generic.
#             Every Gate B is a concrete, runnable verification.

---

## 2026-05-24 Architecture Alignment Addendum

This document remains useful for platform-specific plugin mechanics, but it is
not sufficient as the product readiness plan for Visual Studio and Visual Studio
Code.

Current product target:

```text
XIIGen must provide a smart code wiki library that can serve Visual Studio and
Visual Studio Code plugins while understanding a real project as one system:
Angular client + .NET server + SQL stored procedures.
```

Therefore every platform section below is only a platform adapter plan. The
shared capability must come from the same project wiki contract used by
`42.ProjectWikiDomain` and `43.LearningLocalWikiData`.

### Shared library dependency

All platforms must use a shared XIIGen Smart Code Wiki client contract instead
of creating their own answer shapes:

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

Visual Studio and Visual Studio Code are the first product targets:

```text
https://github.com/lubochka/xiigen_visual_studio
https://github.com/lubochka/xiigen_visual_code
```

### Required readiness states

No platform may treat `PROMOTED` alone as full readiness. The UI must expose the
more precise state:

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

`advanced_wiki_ready` requires:

```text
registry accepted units have manifest_ref, locator_key and remote_unit_id
import/export validation has passed
system map links Angular + .NET + SQL when the project uses those technologies
advanced wiki question suite exists
orchestrator prompt planning suite exists
no blocker is present in readiness report
```

### Multi-root system requirement

For VS Code family platforms, indexing only the first workspace folder is not
valid for XIIGen smart wiki readiness. The adapter must support all workspace
folders and pass them into the shared wiki layer.

For Visual Studio, the open solution is not always the whole business system.
The adapter must support linked roots for:

```text
Angular client
.NET server
SQL stored procedure/database evidence
```

### Timeout rule

Timeouts in platform adapters are UI responsiveness limits only. They are not
project stop gates and cannot produce a readiness claim. Long work must be
background, resumable, or persisted. A timeout may return `indexing`,
`training_missing`, or `blocked_with_gaps`, but never `advanced_wiki_ready`.

### Cross-stack plugin readiness gate

Before calling Visual Studio or Visual Studio Code support ready, the adapter
must show:

```text
basic wiki map works
readiness state is displayed
registry/import-export gaps are displayed
multi-root or linked project configuration is supported
Angular + .NET + SQL system map is displayed when evidence exists
advanced prompt planning suite status is displayed
context injection or copy uses evidence-bound wiki context
```

This addendum supersedes any older platform-specific statement that makes a
static single-project wiki enough for current product readiness.

## Why This Document Exists

The previous Phase 1 addition treated all platforms as variations of VS Code.
That is wrong. Eighteen platforms have eighteen different plugin runtimes,
seventeen different user code languages, and three fundamentally different
relationships with AI assistants. This document corrects that.

Before writing a single line of Phase 1 code, the team must know:

- What code does the wiki actually analyse on this platform?
- How does wiki-context-hook.exe get called from this plugin's runtime?
- Does this platform have an AI session to inject context into?
- How does the developer see the code relevance map in this platform's UI?

These four questions are answered for every platform below.

---

## Shared Foundation Before Any Platform Work

### The language analysis tier decision

`41.SmartCodeWiki/Builder/FileWikiAnalyzer.cs` must support multiple languages.
The XIIGen AI analysis is language-agnostic — it understands any structured
outline of class names, function names, doc strings, and imports. What varies
per language is the extraction step that produces that outline.

Three extraction tiers:

**Tier A — Full syntax tree (Roslyn)**
Used for: C# (.cs) files.
Already available in `13.CodeOptimizer`. Gives the richest structural data:
interfaces, constructor injection, method signatures with return types.

**Tier B — Syntax-aware regex parser**
Used for: Python (.py), TypeScript (.ts, .js), Java (.java), Ruby (.rb).
A .NET implementation that reads these files, finds class/function/module
declarations and their leading doc comments. Does not require a Python/Node/JVM
runtime — pure .NET string processing.
Produces: `{ "kind": "class", "name": "UserService", "methods": [...], "docComment": "..." }`
Accuracy: 90–95% — misses complex multiline signatures, catches all named declarations.

**Tier C — Comment + first-line extraction**
Used for: C++ (.cpp, .h), Lua (.lua), Ruby Script (.rb if Tier B is unavailable),
ExtendScript (.jsx), GhPython scripts (.py in Grasshopper context).
Extracts: block comments above declarations, first non-blank line after a blank line
block. Feeds the AI with enough to generate meaningful summaries.
Accuracy: 70–80% — enough for AIAnalysis to produce useful `aiSummary`.

**Critical rule:** The AIAnalysis step (running through XIIGen's
`AdaptiveLogicalOrchestrator`) runs on whatever the extractor produces.
Even Tier C output gives the AI enough to explain what a file does.
The wiki quality improves with tier, but it never fails entirely.

### The hook invocation models

Six invocation patterns exist across the 18 platforms:

| Model | How it works | Platforms |
|---|---|---|
| **A — Node.js spawn** | `child_process.spawn('wiki-context-hook.exe', [], { stdio: 'pipe' })` | VS Code, Cursor, Windsurf, Open VSX, Obsidian, Adobe CEP, Raycast |
| **B — .NET direct** | Reference `41.SmartCodeWiki.dll` directly — no subprocess needed | Visual Studio, Unity, Autodesk, Rhino, ArcGIS |
| **C — Python subprocess** | `subprocess.Popen(['wiki-context-hook.exe'], stdin=PIPE, stdout=PIPE)` | Blender, QGIS, OBS |
| **D — Java ProcessBuilder** | `new ProcessBuilder("wiki-context-hook.exe").start()` | JetBrains, Eclipse |
| **E — Ruby Open3** | `Open3.popen3('wiki-context-hook.exe')` | SketchUp |
| **F — C++ CreateProcess** | `CreateProcessW(L"wiki-context-hook.exe", ...)` | Unreal |

Models B through F use the same `wiki-context-hook.exe` binary.
The binary reads HOOK_PROMPT from environment, returns JSON to stdout.
No platform-specific logic inside the binary.

---

## Platform 1 — VS Code

**Code analysed:** The user's active workspace — any language. TypeScript, C#,
Python, Java, Go, Rust, PHP. The wiki builder scans `**/*.ts`, `**/*.cs`,
`**/*.py`, `**/*.java` using Tier A for `.cs`, Tier B for the rest.

**Hook invocation:** Model A — Node.js spawn. The VS Code extension calls
`child_process.spawn` on `wiki-context-hook.exe` bundled inside the `.vsix` or
downloaded from xiigen.com on first activation (installer pattern from Section 2
of the Adaptation Report).

Event mapping:
```typescript
// Extension activation
vscode.workspace.onDidSaveTextDocument(() => wikiBuilder.markFileStale(doc.uri));

// On every chat send (GitHub Copilot Chat, inline chat, Ask Claude)
vscode.lm.onDidChangeChatModels(() => { /* hook fires here */ });
// Correct hook: vscode.commands.registerCommand via ChatParticipant API
// wiki-context-hook injects via ChatRequest.references array
```

Injection point: VS Code 1.90+ `vscode.chat.createChatParticipant` API allows
a registered participant to prepend context to any chat request. The wiki
hook registers as an implicit participant that adds `ChatReferencesContext`
containing the formatted wiki block before the user's message reaches Copilot
or Claude.

**AI session presence:** Full — GitHub Copilot Chat, Claude.ai extension,
Codeium chat, Continue.dev. All use the same `vscode.chat` API. The wiki
context appears as a reference attachment the AI sees before the user's question.

**Mode 3 UI:** Side panel using `vscode.WebviewViewProvider`. Developer opens
"XIIGen: Code Map" from the Activity Bar. Searches interactively. Results show
as a ranked list with file:line links that open the file on click.

**Specific challenges:**
- Binary bundling: the `.vsix` cannot include a self-contained executable
  larger than ~10MB without slowing install. Use the installer pattern —
  check for `~/.xiigen/wiki-context-hook.exe`, redirect to download if absent.
- Multi-root workspaces: the wiki must index all workspace folders, not just
  the first one. `vscode.workspace.workspaceFolders` returns all.

**Gate B:**
```bash
# Extension installed, workspace opened
code --install-extension xiigen-smart-wiki-0.1.0.vsix
# Open a TypeScript project, run command
code --command "xiigen.buildWiki"
# Verify
ls ~/.xiigen/smart-wiki/*.json | wc -l   # must be > 0
```

---

## Platform 2 — Cursor (VS Code fork)

**Code analysed:** Identical to VS Code — same workspace, same language mix.

**Hook invocation:** Model A — identical to VS Code. Same `.vsix`, same binary.
Zero incremental cost from VS Code.

**AI session presence:** Full, and stronger than VS Code. Cursor's AI is
the primary interaction model (not an extension). The injection point differs:
Cursor exposes `beforeSubmitPrompt` hook which receives the full prompt before
sending to the model. The wiki hook fires here and returns
`{ "systemPromptAddition": "..." }` — the most direct injection path of all
platforms.

```json
// .cursor/hooks.json
{
  "version": 1,
  "hooks": {
    "sessionStart": [{ "command": "wiki-context-hook.exe --build-if-needed" }],
    "beforeSubmitPrompt": [{ "command": "wiki-context-hook.exe", "timeout": 500 }]
  }
}
```

**Mode 3 UI:** Same WebView panel as VS Code — free ride on the VS Code
extension. Cursor renders VS Code WebViews identically.

**Specific challenges:** None beyond VS Code. The `beforeSubmitPrompt` hook
actually provides a cleaner injection path than VS Code's ChatParticipant API.

**Gate B:**
```bash
# Same as VS Code — the .vsix is identical
# Additional verification for Cursor hook:
HOOK_PROMPT="tenant isolation bug" ./bin/wiki-context-hook.exe
# stdout must contain systemPromptAddition with non-empty string
```

---

## Platform 3 — Windsurf (Codeium IDE)

**Code analysed:** Same as VS Code.

**Hook invocation:** Model A — same `.vsix` distributed via Open VSX Registry.
Windsurf reads extensions from Open VSX. Publishing to Open VSX covers Windsurf
automatically.

**AI session presence:** Full — Codeium's AI is built-in. Windsurf uses the
Open VSX compatible extension API. The ChatParticipant API injection path from
VS Code applies here.

**Mode 3 UI:** Same WebView panel.

**Specific challenges:** Windsurf's AI context injection API may diverge from
VS Code's ChatParticipant API over time as Codeium builds proprietary context
features. Monitor Windsurf changelog. Current state (May 2026): VS Code
compatible extension API is stable.

**Gate B:** Same as VS Code — the binary and extension are identical.

---

## Platform 4 — Open VSX / VSCodium

**Code analysed:** Same as VS Code.

**Hook invocation:** Model A — same `.vsix`. Publishing to Open VSX Registry is
one `ovsx publish` command that covers VSCodium, Theia, Gitpod, and all
Eclipse Che-based environments.

**AI session presence:** Variable. VSCodium users deliberately avoid telemetry
and Microsoft services — many run Continue.dev with local Ollama, or use
direct API-keyed Claude/OpenAI integrations. The wiki context injection via
ChatParticipant API works for Continue.dev. For users with no AI extensions,
only Mode 3 (code map) delivers value.

**Mode 3 UI:** Same WebView panel — this is the primary value for VSCodium
users who avoid cloud AI.

**Specific challenges:** Telemetry must be explicitly OFF in the plugin. VSCodium
users will reject any plugin that phones home. The wiki analysis runs entirely
locally — no data leaves the machine. This must be clearly documented.

**Gate B:** Same as VS Code via Open VSX registry URL.

---

## Platform 5 — JetBrains IDEs / Rider

**Code analysed:** Depends on the IDE. Rider: C# (.cs) — Tier A analysis.
IntelliJ IDEA: Java/Kotlin (.java, .kt) — Tier B analysis. PyCharm: Python (.py)
— Tier B analysis. WebStorm: TypeScript/JavaScript — Tier B analysis.
GoLand: Go (.go) — Tier B analysis.

The wiki builder detects the primary language by scanning file extensions in
the project root and using the dominant type.

**Hook invocation:** Model D — Java `ProcessBuilder`. The IntelliJ Platform
`GeneralCommandLine` API is the canonical way to run external processes:

```kotlin
// Kotlin plugin code
val cmd = GeneralCommandLine()
  .withExePath(wikiHookPath)
  .withEnvironment("HOOK_PROMPT", promptText)
  .withEnvironment("XIIGEN_PROJECT_ROOT", project.basePath ?: "")
val handler = CapturingProcessHandler(cmd)
val output = handler.runProcess(500) // 500ms timeout
val result = output.stdout  // JSON string
```

**AI session presence:** JetBrains AI Assistant (paid subscription) is
the primary AI tool. It has a `AiAssistant` API that plugins can interact with.
The wiki context injection happens via `AiAssistantService.setCustomContext()` —
a string that prepends to every AI Assistant query in the current session.
This is the correct injection point, not chat message interception.

For users without JetBrains AI Assistant: the Mode 3 panel still delivers
full value (code map navigation without AI).

**Mode 3 UI:** IntelliJ Platform `ToolWindowFactory` creates a side panel.
The `JBScrollPane` + `JBList` pattern shows ranked results as a native
IntelliJ list. Double-click opens the file in the editor at the correct line.

```kotlin
class WikiMapToolWindow : ToolWindowFactory {
  override fun createToolWindowContent(project: Project, toolWindow: ToolWindow) {
    val panel = WikiMapPanel(project)  // JPanel with search field + JBList
    toolWindow.contentManager.addContent(
      toolWindow.contentManager.factory.createContent(panel, "Code Map", false))
  }
}
```

**Specific challenges:**
- JetBrains requires compatibility updates per IDE version (5–14 day review).
  Use the `until-build = "*"` approach to avoid blocking updates.
- Rider users have .NET already installed — wiki build is instant. For IntelliJ
  users, the .NET Core runtime may not be present. Must bundle or detect and
  download.
- The `ProcessBuilder` path must handle Windows, macOS, and Linux — three
  separate binary distributions needed in the plugin JAR.

**Gate B:**
```bash
# JetBrains plugin installed in Rider
# Open a C# project
# Run: XIIGen > Build Code Wiki (plugin action)
# Check:
ls $HOME/.xiigen/smart-wiki/ | wc -l  # must be > 0
# AI Assistant context check (manual): start a JetBrains AI conversation
# first assistant message should contain the wiki block
```

---

## Platform 6 — Visual Studio (Full IDE)

**Code analysed:** C# and VB.NET (.cs, .vb) — Tier A. C/C++ (.cpp, .h) —
Tier C. The project is a .sln-based workspace. `41.SmartCodeWiki` is referenced
directly as a .NET assembly — no subprocess needed.

**Hook invocation:** Model B — direct .NET assembly reference. The Visual Studio
extension references `41.SmartCodeWiki.dll` as a project reference in the `.csproj`.
`SmartWikiBuilder.BuildAsync(solutionPath)` is called directly from the extension's
event handler.

```csharp
// VS extension code (VisualStudio.Extensibility pattern)
[Command("XIIGen.BuildWiki")]
public async Task ExecuteCommandAsync(IClientContext context, CancellationToken ct)
{
    var solutionPath = await context.GetSolutionPathAsync(ct);
    await SmartWikiBuilder.BuildAsync(solutionPath, ct);
}

// Hook into GitHub Copilot context via ICopilotContextProvider (VS 17.12+)
public class WikiCopilotContextProvider : ICopilotContextProvider
{
    public async Task<string> GetContextAsync(string userPrompt, CancellationToken ct)
    {
        var slice = await WikiContextRetriever.RetrieveAsync(userPrompt, topN: 5, ct);
        return WikiContextFormatter.Format(slice);
    }
}
```

**AI session presence:** GitHub Copilot (via `ICopilotContextProvider` interface,
available in Visual Studio 17.12+). This is the cleanest injection path of all
platforms — Microsoft explicitly designed `ICopilotContextProvider` for extensions
that want to add custom context to Copilot. The wiki context registers as a named
provider and is automatically included in every Copilot inline chat request.

**Mode 3 UI:** `ToolWindowPane` with three tabs (Browse, Ask, Map) using WPF
controls inside Visual Studio's hosting surface. The Map tab renders Mermaid
via `WebView2` — available in VS 17.x. File links in the results open the file
in the VS editor via `IVsTextManager.NavigateToLineAndColumn`.

**Specific challenges:**
- `ICopilotContextProvider` requires Visual Studio 17.12 minimum. Users on older
  VS versions get Mode 3 only. Document the version requirement.
- The new `VisualStudio.Extensibility` model (out-of-process) is preferred for
  new VS extensions. The wiki integration runs out-of-process — natural fit.
- `41.SmartCodeWiki.dll` must target .NET Standard 2.0 or .NET 8 with multi-target
  to be referenced from both the VS extension and the standalone hook executable.

**Gate B:**
```powershell
# Visual Studio installed with our extension
# Open any C# solution
# View -> XIIGen -> Code Wiki
# Tool window opens — Browse tab shows namespace tree
# In Ask tab: type "tenant isolation" -> at least 1 result appears
```

---

## Platform 7 — Raycast

**Code analysed:** Raycast is a launcher, not an IDE. It has no project
workspace of its own. Phase 1 for Raycast works differently: the developer
runs a Raycast command, specifies a project path, and gets the code map for
that project. The wiki is stored per-project in `{projectPath}/.xiigen/smart-wiki/`.

This means Raycast Phase 1 is **Mode 3 only** — code map as a quick-lookup
tool. The developer types a query, selects a project, sees the relevance map.

**Hook invocation:** Model A — Node.js `child_process.exec`. Raycast extensions
run as Node.js TypeScript processes.

```typescript
// Raycast extension
import { exec } from 'child_process'

async function showCodeMap(query: string, projectPath: string) {
  const { stdout } = await exec(
    `"${wikiHookPath}" --map "${query}"`,
    { env: { ...process.env, XIIGEN_PROJECT_ROOT: projectPath } }
  )
  const map = JSON.parse(stdout)
  // Show in Raycast List
}
```

**AI session presence:** Raycast does not have a built-in AI coding session.
However, Raycast Pro has an AI chat feature. The wiki context can be injected
into Raycast AI queries via the `AI.ask()` API available in Raycast extensions:

```typescript
import { AI } from '@raycast/api'

const wikiContext = await getWikiContext(query)
const response = await AI.ask(
  `${wikiContext}\n\nQuestion: ${query}`,
  { model: AI.Model.OpenAI_GPT4o }
)
```

For users without Raycast Pro: Mode 3 (code map) is the full value proposition.

**Mode 3 UI:** Raycast `List` component with ranked entries. Each entry shows
the class name, relevance bar (via subtitle), and file path. `Action.OpenInEditor`
or `Action.ShowInFinder` opens the file.

```typescript
<List>
  {entries.map(entry => (
    <List.Item
      key={entry.id}
      title={entry.name}
      subtitle={`${entry.architecturalRole} · ${relevanceBar(entry.score)}`}
      accessories={[{ text: entry.filePath }]}
      actions={<ActionPanel>
        <Action.Open title="Open File" target={entry.filePath} />
      </ActionPanel>}
    />
  ))}
</List>
```

**Specific challenges:**
- Raycast is macOS only. The wiki-context-hook binary must be a macOS universal
  binary (arm64 + x64). Windows users cannot use this extension.
- The project path is not automatic — the developer must specify it or
  Raycast must detect it from recent Git repos. Use `~/.gitconfig` to find
  recent repos as default suggestions.
- Binary size: Raycast reviews extensions. Self-contained .NET 8 binary is
  ~15–25MB on macOS. This is within acceptable limits (yt-dlp binary in the
  proven extension is larger).

**Gate B:**
```bash
# Raycast extension installed
# Command: "XIIGen Code Map"
# Input: query "authorization tenant", project path = /Users/dev/myproject
# Expected: List of ranked components appears in Raycast window within 500ms
```

---

## Platform 8 — Obsidian

**Code analysed:** Obsidian is a note-taking tool. Two scenarios:

Scenario A — Developer uses Obsidian as a code knowledge base (common pattern):
vault contains `.md` notes about code, plus symlinked or directly stored `.cs`,
`.ts`, `.py` files. Wiki builder scans all code files in the vault.

Scenario B — Developer links Obsidian vault to a code project:
the plugin asks for a project path on first activation and builds the wiki for
that external directory. The vault stores only the resulting wiki `.md` files.

Both scenarios use the same `FileWikiAnalyzer` — the project path just differs.

**Hook invocation:** Model A — Node.js `child_process.spawn`. Obsidian plugins
run in an Electron/Node.js environment with full filesystem access.

```typescript
// Obsidian plugin
import { spawn } from 'child_process'

async function getWikiContext(query: string): Promise<string> {
  return new Promise((resolve) => {
    const proc = spawn(wikiHookPath, [], {
      env: { ...process.env, HOOK_PROMPT: query, XIIGEN_PROJECT_ROOT: projectRoot },
      stdio: 'pipe'
    })
    let output = ''
    proc.stdout.on('data', d => output += d)
    proc.on('close', () => resolve(JSON.parse(output).systemPromptAddition))
  })
}
```

**AI session presence:** Obsidian does not have a built-in AI. However several
popular plugins (Copilot plugin, Smart Connections, Text Generator) provide AI
chat. Integration approach: when the developer uses the "Ask" command in the
wiki panel, the result is inserted into the current note as formatted Markdown,
and the developer's preferred AI plugin picks it up as note context.

This is the Obsidian-native approach — insert context into the note, let the
note become the AI input. Obsidian's paradigm is note-as-context.

**Mode 3 UI:** An Obsidian right-side panel (leaf view) with a search input
and a list of matching entries. Clicking an entry inserts a wiki block into the
current note:

```markdown
> [!xiigen-context] Code Context: UserService
> **Role:** Domain service. Guards tenant boundary.
> **Injects:** ISessionStorage, IUserRepository
> **Called by:** AuthController, UserMigrationService
> **Data flows:** AuthController → UserService → IUserRepository → DB
```

Obsidian renders callout blocks natively — this format is readable without any
custom renderer.

**Specific challenges:**
- Obsidian runs on Windows, macOS, and Linux. Three binary distributions needed.
- Obsidian mobile (iOS/Android) cannot run subprocesses. The wiki panel should
  gracefully degrade to "read-only" mode on mobile — showing previously built
  wiki from synced vault files.
- Obsidian vault sync (Obsidian Sync, Git) will sync `.xiigen/smart-wiki/*.json`
  files — this is actually desirable for teams sharing a vault.

**Gate B:**
```
# Plugin installed in Obsidian
# Open a vault with a linked code project
# Command palette: "XIIGen: Build Code Wiki"
# After build: command "XIIGen: Show Code Map"
# Type "authorization" in search
# At least 1 result appears in the panel
```

---

## Platform 9 — Blender

**Code analysed:** Python scripts and addons in the Blender project:
`**/*.py` files in `scripts/addons/`, `scripts/startup/`,
and any scripts linked to the current `.blend` file.
Tier B analysis — Python function/class/module extraction.

The wiki understands what each addon does, what Blender API classes it uses,
and what data flows through it (e.g., `bpy.types.Operator` subclasses,
`bpy.props` registrations, `register()`/`unregister()` lifecycle).

**Hook invocation:** Model C — Python `subprocess.Popen`. Blender addons run
inside Blender's bundled Python interpreter.

```python
import subprocess, json, os

def get_wiki_context(query: str) -> str:
    hook_path = get_wiki_hook_path()  # resolved from addon prefs
    result = subprocess.run(
        [hook_path],
        env={**os.environ, 'HOOK_PROMPT': query,
             'XIIGEN_PROJECT_ROOT': bpy.path.abspath('//')},
        capture_output=True, text=True, timeout=5
    )
    return json.loads(result.stdout).get('systemPromptAddition', '')
```

**AI session presence:** Blender has no built-in AI coding assistant. However,
the Blender community actively uses AI for:
- Script generation (pasting code into Blender's scripting workspace)
- Addon documentation generation

For Phase 1, the integration is: in Blender's Scripting workspace, a side panel
shows the code map. The developer finds the relevant script, reads the AI-generated
summary, and uses that understanding when writing new code or asking an external AI.

Mode 2 (context injection) for Blender means: a "Copy Context" button puts the
wiki block into the clipboard. The developer pastes it into ChatGPT, Claude web,
or any external AI they use.

**Mode 3 UI:** Blender side panel in the Scripting workspace (N-panel):

```python
class XIIGEN_PT_CodeWiki(bpy.types.Panel):
    bl_label = "XIIGen Code Wiki"
    bl_space_type = 'TEXT_EDITOR'
    bl_region_type = 'UI'
    bl_category = 'XIIGen'

    def draw(self, context):
        layout = self.layout
        layout.prop(context.scene, "xiigen_query", text="Search")
        layout.operator("xiigen.search_wiki", text="Find")
        for entry in context.scene.xiigen_results:
            row = layout.row()
            row.label(text=f"{entry.name} — {entry.summary[:60]}")
            row.operator("xiigen.open_script", text="", icon='TEXT').path = entry.file
```

**Specific challenges:**
- Blender's bundled Python is a fixed version (3.11 in Blender 4.x). It can call
  subprocesses but the .NET Core runtime is separate. Must distribute a pre-built
  self-contained binary with no runtime dependency.
- Blender's official extensions platform (4.2+) requires signing and review.
  For initial testing, use the legacy addon installation path (zip install).
- The `//` path in Blender means "relative to the current .blend file." The wiki
  builder must resolve this before building.

**Gate B:**
```bash
# Blender addon installed (zip install, no marketplace required for testing)
# Open Blender with a project that has Python scripts
# Scripting workspace → N panel → XIIGen tab visible
# Enter "operator" in search → at least 1 result showing a bpy.types.Operator subclass
```

---

## Platform 10 — Unity Asset Store

**Code analysed:** C# MonoBehaviour scripts, Editor scripts, and ScriptableObject
definitions in the Unity project: `Assets/**/*.cs` files. Tier A analysis via
Roslyn — the richest possible output since Unity projects are pure C#.

The wiki understands MonoBehaviour lifecycle (`Start`, `Update`, `OnTriggerEnter`),
ScriptableObject definitions, Editor window scripts, and dependencies injected
via `[SerializeField]` or `GetComponent<T>()`.

**Hook invocation:** Model B — direct .NET assembly. Unity Editor plugins
(EditorWindow, MenuItem, AssetPostprocessor) run inside Unity's Mono/.NET runtime.
`41.SmartCodeWiki.dll` is added to `Assets/Plugins/Editor/` and called directly:

```csharp
// Unity Editor menu item
[MenuItem("XIIGen/Build Code Wiki")]
public static async void BuildWiki()
{
    var projectPath = Application.dataPath.Replace("/Assets", "");
    await SmartWikiBuilder.BuildAsync(projectPath);
    Debug.Log($"XIIGen: Wiki built at {projectPath}/.xiigen/smart-wiki/");
}
```

**AI session presence:** GitHub Copilot is available in Visual Studio and VS Code
used alongside Unity. When a developer writes a Unity script in VS Code or VS with
Copilot, the wiki context hook fires via the VS Code / VS extension — not the Unity
plugin. The Unity plugin's job is to build and display the wiki; context injection
for AI happens in the code editor.

Unity's own AI tools (Unity Muse, Unity Sentis) have separate APIs — not in scope
for Phase 1.

**Mode 3 UI:** Unity EditorWindow panel accessible via XIIGen menu:

```csharp
public class CodeWikiWindow : EditorWindow
{
    [MenuItem("XIIGen/Code Map")]
    public static void ShowWindow() => GetWindow<CodeWikiWindow>("XIIGen Code Map");

    void OnGUI()
    {
        _query = EditorGUILayout.TextField("Search", _query);
        if (GUILayout.Button("Find")) _results = WikiContextRetriever.Retrieve(_query, 5);
        foreach (var entry in _results)
        {
            EditorGUILayout.LabelField($"{entry.Name} — {entry.AiSummary[..60]}");
            if (GUILayout.Button("Open")) AssetDatabase.OpenAsset(
                AssetDatabase.LoadAssetAtPath<MonoScript>(entry.FilePath));
        }
    }
}
```

**Specific challenges:**
- Unity uses Mono (older .NET) in the Editor. `41.SmartCodeWiki.dll` must target
  .NET Standard 2.0 to be compatible with Unity's Mono runtime.
- Unity projects often have auto-generated code in `Library/` — exclude this
  directory from wiki analysis.
- The `Assets/Plugins/Editor/` placement is non-standard for Asset Store packages.
  Use `Tools/XIIGen/` menu convention for discoverability.

**Gate B:**
```
# Unity Editor with addon installed
# XIIGen menu → Build Code Wiki
# Console: "XIIGen: Wiki built — N entries"
# XIIGen menu → Code Map window opens
# Type "Player" → results include PlayerController or similar MonoBehaviour
```

---

## Platform 11 — Autodesk App Store (Revit/AutoCAD)

**Code analysed:** Two language contexts:

Revit/AutoCAD add-in C# code: `**/*.cs` files in the add-in project.
Tier A analysis — these are standard .NET assemblies.

Dynamo scripts (.dyn): JSON-based visual program files. Tier C — the wiki
extracts node names and descriptions from the JSON structure to describe
what the script computes.

AcroPy macros (.py in AutoCAD): Tier B Python analysis.

**Hook invocation:** Model B — direct .NET assembly. Revit and AutoCAD add-ins
are .NET DLLs loaded directly by the host application. `41.SmartCodeWiki.dll`
is referenced directly:

```csharp
// Revit IExternalCommand implementation
public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
{
    var addinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var projectRoot = Path.GetDirectoryName(addinPath);  // solution root
    SmartWikiBuilder.BuildAsync(projectRoot).GetAwaiter().GetResult();
    return Result.Succeeded;
}
```

**AI session presence:** Autodesk has AI tools (Autodesk AI, Forma) but they
target design, not code writing. Revit/AutoCAD developers who write C# add-ins
use Visual Studio — the wiki context injection happens through the VS extension,
not the Autodesk add-in.

The Autodesk add-in's primary Phase 1 value is Mode 3: architects and BIM
engineers can query "which script handles level assignment" and see the relevant
add-in code without opening Visual Studio.

**Mode 3 UI:** A WinForms panel (Revit's UI framework is WPF/WinForms) added
to the Revit ribbon:

```csharp
// Revit ribbon panel
ribbonPanel.AddItem(new PushButtonData("XIIGenCodeMap",
    "Code Map", Assembly.GetExecutingAssembly().Location, "XIIGen.CodeMapCommand"));

// CodeMapCommand opens a WinForms form with search + results list
public class CodeMapForm : Form
{
    // TextBox for query, ListBox for results, double-click opens file in VS
}
```

**Specific challenges:**
- Revit's .NET version constraints: Revit 2024+ uses .NET 8. Older versions use
  .NET Framework 4.8. `41.SmartCodeWiki.dll` needs multi-targeting.
- The Autodesk App Store review process requires installers (.msi or .exe).
  The wiki hook binary must be part of the installer, not downloaded at runtime.
- Dynamo script analysis (.dyn files) requires a custom parser for the JSON node
  structure. This is Phase 1.5 — for the initial release, index .cs files only.

**Gate B:**
```
# Revit with add-in installed
# Add-In tab → XIIGen → Build Wiki
# Add-In tab → XIIGen → Code Map
# Type "wall type" in search
# Results show C# methods that reference WallType or Wall API elements
```

---

## Platform 12 — Rhino / Grasshopper

**Code analysed:** Three code types exist in a Rhino/Grasshopper project:

C# Rhino plugins (.cs): Tier A analysis. These are standard .NET assemblies.
GhPython components (.py inside .gh files): extractable Python code in Grasshopper
component scripts — Tier B analysis after extraction from the .gh JSON.
RhinoScript macros (.rvb, .py): Tier B Python or Tier C VBScript.

**Hook invocation:** Model B — direct .NET assembly. Rhino plugins are .NET DLLs.

```csharp
// Rhino plugin command
public override Result RunCommand(RhinoDoc doc, RunMode mode)
{
    var projectPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    SmartWikiBuilder.BuildAsync(projectPath).GetAwaiter().GetResult();
    RhinoApp.WriteLine("XIIGen: Code wiki built.");
    return Result.Success;
}
```

**AI session presence:** Rhino/Grasshopper users who write code typically use
VS Code or Rider for editing — where the wiki context injection from those
platforms applies. Grasshopper has no built-in AI.

The primary Phase 1 value for Grasshopper is unique: a developer can query
"which component outputs a surface" and the wiki returns a map of all Grasshopper
components in the definition, showing their inputs, outputs, and purposes. This
is genuinely novel navigation for complex parametric definitions.

**Mode 3 UI:** Rhino command `XIIGenCodeMap` opens a modeless dialog (Eto.Forms,
which Rhino supports cross-platform — Windows and macOS):

```csharp
public class CodeMapDialog : Eto.Forms.Dialog
{
    // Eto TextBox + GridView
    // GridView shows: Component Name | Role | Inputs | Outputs | File
    // Double-click opens the component's source in VS Code via "code" CLI
}
```

**Specific challenges:**
- Extracting Python scripts from `.gh` files: Grasshopper files are binary JSON.
  The `GH_IO` library (comes with Rhino SDK) can deserialize `.gh` files and
  extract `GhPython` component scripts as strings. This requires a separate
  extractor step before Tier B analysis.
- The wiki for Grasshopper definitions should describe **component graphs**,
  not just files. Phase 1 does files only — component graph understanding is Phase 2.

**Gate B:**
```
# Rhino plugin installed
# Run command: XIIGenBuildWiki
# Rhino command prompt shows: "Wiki built: N entries"
# Run command: XIIGenCodeMap
# Dialog opens, type "mesh" → results show components/methods referencing Mesh types
```

---

## Platform 13 — Adobe CEP (After Effects, Premiere Pro)

**Code analysed:** ExtendScript files (.jsx) and UXP TypeScript files (.ts, .js)
in the CEP panel project. Also any JavaScript source files in the `src/` folder
of the extension. Tier B analysis — treat .jsx as JavaScript (same structure).

**Hook invocation:** Model A — Node.js `child_process.spawn`. CEP panels embed
a full Node.js runtime:

```javascript
// CEP panel JavaScript
const { spawn } = require('child_process')

function getWikiContext(query) {
  return new Promise((resolve) => {
    const env = { ...process.env,
      HOOK_PROMPT: query,
      XIIGEN_PROJECT_ROOT: new File($.fileName).parent.fsName }
    const proc = spawn(wikiHookPath, [], { env, stdio: 'pipe' })
    let out = ''
    proc.stdout.on('data', d => out += d)
    proc.on('close', () => resolve(JSON.parse(out).systemPromptAddition || ''))
  })
}
```

**AI session presence:** Adobe does not have a built-in AI coding assistant for
CEP/UXP. Mode 2 for Adobe means: a "Copy Context" button in the wiki panel
copies the formatted wiki block to the clipboard. The developer pastes it
into Claude web, ChatGPT, or wherever they generate code.

This is intentional — Adobe's environment is not a code editing environment.
Developers write Adobe scripts in VS Code or a separate editor, then test them
in After Effects. The wiki panel inside After Effects helps them understand
existing scripts without switching contexts.

**Mode 3 UI:** A panel in the CEP extension (HTML + CSS + JavaScript):

```html
<!-- panel/index.html -->
<div id="wiki-panel">
  <input type="text" id="query" placeholder="Search scripts...">
  <button onclick="search()">Find</button>
  <div id="results"></div>
  <button onclick="copyContext()">Copy Context for AI</button>
</div>
```

**Specific challenges:**
- UXP (Photoshop, Illustrator) does NOT support `child_process.spawn`. Only
  After Effects and Premiere Pro use CEP which has full Node.js. UXP plugins
  must communicate with a pre-installed local HTTP service. For Phase 1, limit
  to CEP-compatible apps (After Effects, Premiere Pro, InDesign).
- Adobe's review process for CEP panels takes several weeks. The installer
  pattern is required — the wiki hook binary cannot be bundled inside the
  extension package; it must be part of a separate installer.
- `$.fileName` in ExtendScript gives the path of the running script — useful
  for finding the project root of the current script being edited.

**Gate B:**
```
# CEP panel installed in After Effects (unsigned, developer mode)
# Window → Extensions → XIIGen Code Wiki
# Panel opens in After Effects
# Enter "render queue" in search
# Results show ExtendScript functions that reference app.project.renderQueue
```

---

## Platform 14 — SketchUp

**Code analysed:** Ruby extension files (.rb) in the SketchUp extension directory.
Tier B Ruby analysis — class, module, def keyword extraction with leading comments.

The wiki understands SketchUp Ruby API usage patterns:
`Sketchup::Model`, `Sketchup::Entities`, `UI::HtmlDialog`,
`Sketchup::Tool` subclasses, and Observer patterns.

**Hook invocation:** Model E — Ruby `Open3.popen3`:

```ruby
require 'open3'
require 'json'

def get_wiki_context(query)
  hook_path = File.join(File.dirname(__FILE__), 'bin', wiki_hook_binary)
  env = { 'HOOK_PROMPT' => query,
          'XIIGEN_PROJECT_ROOT' => File.dirname(__FILE__) }
  stdout, _stderr, _status = Open3.capture3(env, hook_path)
  JSON.parse(stdout)['systemPromptAddition'] rescue ''
end
```

**AI session presence:** SketchUp has no built-in AI coding assistant.
Mode 2 uses the clipboard pattern — "Copy Context" puts the wiki block in the
clipboard for use with external AI tools.

Unique Mode 2 opportunity: SketchUp's Ruby console is an interactive REPL.
The wiki panel can offer "Insert as Ruby comment" — the context block appears
as a comment above the cursor in the Ruby console, giving the developer
architectural context while scripting interactively.

**Mode 3 UI:** SketchUp `UI::WebDialog` or `UI::HtmlDialog` (preferred in newer
SketchUp versions) renders HTML — a simple search panel with results:

```ruby
dialog = UI::HtmlDialog.new({
  dialog_title: 'XIIGen Code Map',
  preferences_key: 'xiigen_code_map',
  width: 400, height: 600,
  resizable: true
})
dialog.set_file(File.join(File.dirname(__FILE__), 'ui', 'code_map.html'))
dialog.show
```

**Specific challenges:**
- Ruby's `Open3` is in the standard library — no additional dependencies.
  However, the .NET Core binary must be on the system PATH or referenced with
  a full path. Use the installer pattern — a Windows/macOS installer sets
  `~/.xiigen/` on PATH.
- SketchUp's Extension Warehouse requires a review and signing process.
  The installer pattern (binary installed separately from the extension) is
  required for review compliance.
- Ruby versions: SketchUp 2023+ bundles Ruby 3.x; older versions use Ruby 2.7.
  `Open3` is present in both — no compatibility issue.

**Gate B:**
```bash
# SketchUp extension installed (manual .rbz install for testing)
# Extensions → XIIGen → Code Map
# HtmlDialog opens
# Enter "group" in search → results show Ruby methods referencing Sketchup::Group
```

---

## Platform 15 — Unreal Engine / Fab Marketplace

**Code analysed:** C++ plugin source files (.cpp, .h) in `Source/` directories.
Tier C analysis — C++ header extraction: class declarations, method signatures,
`UCLASS()`, `UFUNCTION()`, `UPROPERTY()` macros, and doc comments (`/** */`).

Unreal Blueprints (.uasset) are binary — not analysed directly in Phase 1.
A dedicated Blueprint text export pass is Phase 2.

**Hook invocation:** Model F — C++ `CreateProcessW`. Unreal plugins are C++ DLLs
loaded by the Unreal Editor. The `FPlatformProcess::CreateProc` wrapper provides
cross-platform process launching:

```cpp
// Unreal Editor module code
void FXIIGenCodeWikiModule::BuildWiki()
{
    FString HookPath = FPaths::Combine(FPaths::ProjectDir(), TEXT("Binaries/XIIGen/wiki-context-hook.exe"));
    FString Args = TEXT("--build");
    FString EnvVars = FString::Printf(TEXT("XIIGEN_PROJECT_ROOT=%s"), *FPaths::ProjectDir());

    FProcHandle Proc = FPlatformProcess::CreateProc(
        *HookPath, *Args, true, false, false, nullptr, 0, nullptr, nullptr);
    FPlatformProcess::WaitForProc(Proc);
    FPlatformProcess::CloseProc(Proc);
}
```

**AI session presence:** Unreal developers typically write code in Visual Studio
or VS Code — where Phase 1 wiki context injection from those platforms applies.
Unreal Editor itself has no built-in AI coding assistant in the base editor.

Epic's AI tools (Verse AI assist) are for Blueprint/Verse, not C++ add-in code.

Mode 2 for Unreal: clipboard pattern — "Copy Context" puts the block on the
clipboard for use in VS Code, VS, or Claude web.

**Mode 3 UI:** Unreal Editor `SWindow` (Slate UI) with search:

```cpp
TSharedRef<SDockTab> FXIIGenCodeWikiModule::SpawnCodeMapTab(const FSpawnTabArgs& Args)
{
    return SNew(SDockTab).TabRole(ETabRole::NomadTab)
    [
        SNew(SVerticalBox)
        + SVerticalBox::Slot().AutoHeight()
        [ SNew(SEditableTextBox).OnTextCommitted(this, &FXIIGenCodeWikiModule::OnQueryCommitted) ]
        + SVerticalBox::Slot()
        [ SNew(SListView<TSharedPtr<FWikiEntry>>).ListItemsSource(&Results) ]
    ];
}
```

**Specific challenges:**
- C++ build compatibility: Unreal plugins must be recompiled per engine version.
  The wiki module must ship with source, not binaries, for Fab marketplace compliance.
- The .NET Core binary is a foreign runtime in a C++ environment. It must be
  distributed as a self-contained executable with no .NET install required.
- C++ header analysis (Tier C) misses template specializations and complex macros.
  For `UCLASS()` and `UFUNCTION()` — these are simple enough that Tier C regex
  handles them correctly.

**Gate B:**
```
# Unreal Editor with plugin compiled
# Window → XIIGen → Code Map
# Dock tab opens
# Type "character movement" in search
# Results show C++ headers referencing UCharacterMovementComponent or similar
```

---

## Platform 16 — OBS Studio

**Code analysed:** OBS has two script types. Python scripts (.py) for OBS
automation via `obspython` module — Tier B analysis. Lua scripts (.lua) for
simpler automation — Tier C analysis.

The wiki understands OBS API patterns: `obs_source_t*` references in Python,
`obslua` module usage, scene/source management functions, hotkey handlers.

**Hook invocation:** Model C — Python `subprocess.run`. OBS Python scripts run
inside OBS's bundled Python interpreter (CPython, same subprocess capability
as Blender):

```python
import obspython as obs
import subprocess, json, os

def get_wiki_context(query):
    hook_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                             'bin', wiki_hook_binary())
    result = subprocess.run(
        [hook_path],
        env={**os.environ, 'HOOK_PROMPT': query,
             'XIIGEN_PROJECT_ROOT': obs.obs_frontend_get_profile_config_path()},
        capture_output=True, text=True, timeout=5
    )
    return json.loads(result.stdout).get('systemPromptAddition', '')
```

**AI session presence:** OBS has no built-in AI coding assistant. Mode 2 uses
the clipboard pattern. The primary value is Mode 3: OBS power users who maintain
complex Python automation scripts can navigate them via the code map panel.

**Mode 3 UI:** OBS Python scripts can display a Qt dialog (OBS uses Qt):

```python
def script_load(settings):
    global dialog
    dialog = obs.QDialog()  # OBS exposes Qt bindings
    # Build search UI using Qt widgets
```

Alternative (simpler): OBS scripts can show a simple dockable browser source
pointing to a local HTML page served by the wiki hook's built-in HTTP server.
This avoids Qt binding complexity.

**Specific challenges:**
- OBS Python scripts cannot easily create persistent UI panels — only settings
  dialogs (`script_properties`). A dockable panel requires a C++ OBS plugin, not
  a Python script. For Phase 1: use a properties-based UI (a textbox for query
  in the script properties dialog) rather than a rich panel.
- OBS has no formal marketplace. Distribution is via GitHub or community forums.
  No review process means faster iteration.
- Lua scripts in OBS are simpler — Tier C analysis is sufficient. Many OBS users
  write minimal Lua scripts that the wiki can summarise with just function names
  and comments.

**Gate B:**
```
# OBS script installed (Tools → Scripts → + → add script)
# In Script Properties: enter "scene" in Query field, click "Find"
# Console (Log) shows: wiki search results for "scene"
# (Rich UI deferred to Phase 2 C++ plugin)
```

---

## Platform 17 — Eclipse IDE

**Code analysed:** Java source files (.java) primarily. Also Kotlin (.kt) if
the project uses it. Tier B analysis — Java class/interface/method extraction
from source text.

**Hook invocation:** Model D — Java `ProcessBuilder`. Eclipse plugins (OSGi bundles)
run inside the JVM:

```java
// Eclipse plugin
public class BuildWikiHandler extends AbstractHandler {
    @Override
    public Object execute(ExecutionEvent event) {
        String projectRoot = getProjectRoot(event);
        ProcessBuilder pb = new ProcessBuilder(getWikiHookPath(), "--build");
        pb.environment().put("XIIGEN_PROJECT_ROOT", projectRoot);
        pb.redirectErrorStream(true);
        try {
            Process p = pb.start();
            p.waitFor(60, TimeUnit.SECONDS);
        } catch (Exception e) {
            // log error, continue
        }
        return null;
    }
}
```

**AI session presence:** Eclipse has GitHub Copilot extension support (via the
Eclipse Copilot plugin). The injection approach: the Eclipse plugin hooks into
the Copilot completion request via an `ICompletionListener` interceptor, prepending
the wiki context block to the system prompt. This is less direct than VS Code's
`ChatParticipant` API — Eclipse's Copilot integration is more opaque.

Practical approach for Phase 1: inject via a template variable. Eclipse has a
code template feature (`Java > Code Style > Code Templates`) — the wiki context
can be injected as a special template that developers add to their Copilot prompts.
Not automatic, but functional.

**Mode 3 UI:** Eclipse `ViewPart` in the Java perspective sidebar:

```java
public class CodeMapView extends ViewPart {
    @Override
    public void createPartControl(Composite parent) {
        Text searchBox = new Text(parent, SWT.SINGLE | SWT.BORDER);
        Table resultTable = new Table(parent, SWT.V_SCROLL | SWT.FULL_SELECTION);
        // Wire up search to wiki retriever
    }
}
```

**Specific challenges:**
- Eclipse audience is shrinking (flat growth per the Adaptation Report). This
  platform should be Wave 9 — after JetBrains validates the JVM approach.
  The JVM invocation code (`ProcessBuilder`) from JetBrains is directly reusable.
- Eclipse's OSGi dependency management is complex. The wiki hook binary path
  must be resolved relative to the plugin bundle, not the workspace.
- .NET Core on Linux (common for Eclipse users): the binary must be a Linux
  self-contained build in addition to Windows and macOS.

**Gate B:**
```
# Eclipse with plugin installed
# Window → Show View → XIIGen Code Map
# View panel appears in Eclipse IDE
# Enter "service" in search → results show Java service classes in project
```

---

## Platform 18 — QGIS

**Code analysed:** Python processing scripts (.py) in the QGIS Python plugins
directory and in the project's script library. Tier B analysis — Python
class/function/module extraction. The wiki understands QGIS API patterns:
`QgsProcessingAlgorithm` subclasses, `QgsFeature` manipulation,
`QgsVectorLayer` usage, parameter type definitions.

**Hook invocation:** Model C — Python `subprocess.Popen`. QGIS plugins run
inside QGIS's bundled Python (PyQGIS):

```python
from qgis.core import QgsApplication
import subprocess, json, os

def get_wiki_context(query):
    hook_path = get_wiki_hook_binary_path()
    result = subprocess.run(
        [hook_path],
        env={**os.environ,
             'HOOK_PROMPT': query,
             'XIIGEN_PROJECT_ROOT': QgsApplication.qgisSettingsDirPath()},
        capture_output=True, text=True, timeout=5
    )
    return json.loads(result.stdout).get('systemPromptAddition', '')
```

**AI session presence:** QGIS has no built-in AI coding assistant. Mode 2 uses
the clipboard pattern. The primary value is Mode 3: GIS analysts who maintain
complex Python processing algorithms can navigate them via the code map.

GIS professionals often work with large scripting libraries they did not write
themselves. The wiki's AI-generated summaries of spatial processing algorithms
are particularly useful here — "what does this algorithm do with the buffer
parameter?" can be answered without reading the full code.

**Mode 3 UI:** QGIS Plugin panel using PyQGIS Qt bindings. QGIS plugins can
create dockable panels:

```python
from qgis.PyQt.QtWidgets import QDockWidget, QLineEdit, QListWidget, QVBoxLayout

class CodeMapPanel(QDockWidget):
    def __init__(self, iface):
        super().__init__("XIIGen Code Map")
        self.iface = iface
        layout = QVBoxLayout()
        self.search = QLineEdit(placeholderText="Search scripts...")
        self.results = QListWidget()
        self.search.returnPressed.connect(self.do_search)
        layout.addWidget(self.search)
        layout.addWidget(self.results)
        # Add as dockable panel
        iface.addDockWidget(Qt.RightDockWidgetArea, self)
```

**Specific challenges:**
- QGIS Plugin Repository requires an OSGeo account and approval via `plugins.qgis.org`.
  The review focuses on functionality and code quality — no installer pattern issue.
- The .NET Core binary must be findable from within QGIS's Python environment.
  Use a fixed path (`~/.xiigen/wiki-context-hook`) resolved at plugin load time.
- PyQGIS's Python version is determined by QGIS's build — currently Python 3.9–3.12
  depending on the QGIS version. `subprocess.run` is standard in all of these.

**Gate B:**
```
# QGIS with plugin installed (Plugins → Manage → Install from zip for testing)
# View → Panels → XIIGen Code Map
# Panel appears in QGIS right dock
# Enter "buffer" in search → results show Python functions that call .buffer() on geometry
```

---

## Platform 19 — ArcGIS Pro

**Code analysed:** C# add-in files (.cs) in the ArcGIS Pro add-in project.
Tier A analysis — these are standard .NET assemblies. Also Python scripts (.py)
in ArcPy toolboxes — Tier B analysis.

**Hook invocation:** Model B — direct .NET assembly. ArcGIS Pro add-ins are
.NET DLLs loaded by ArcGIS Pro's .NET runtime. Reference `41.SmartCodeWiki.dll`
directly:

```csharp
// ArcGIS Pro button action
protected override async Task OnClick()
{
    var addinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    await SmartWikiBuilder.BuildAsync(addinPath);
    MessageBox.Show("XIIGen: Code wiki built.");
}
```

**AI session presence:** Esri has AI tools (ArcGIS AI) but they target GIS
analysis, not add-in code writing. ArcGIS Pro C# developers use Visual Studio
— where the VS extension handles wiki context injection for GitHub Copilot.

Mode 2 for ArcGIS: clipboard pattern. Mode 3 is the primary value.

**Mode 3 UI:** ArcGIS Pro uses WPF for all UI. A dockable pane using the
`DockPane` SDK pattern:

```csharp
// ArcGIS Pro SDK DockPane
public class CodeMapDockPane : DockPane
{
    private const string _dockPaneID = "XIIGen_CodeMapDockPane";

    protected override Task InitializeAsync()
    {
        // Initialize search binding
        return base.InitializeAsync();
    }
}
// XAML: SearchBox + ListBox with WikiEntry binding
```

**Specific challenges:**
- ArcGIS Pro SDK requires specific .NET 8 targeting. `41.SmartCodeWiki.dll`
  must target .NET 8 to be compatible.
- Esri's marketplace review is focused on professional users — clear documentation
  of what the wiki analyses and what data stays local is important for enterprise
  adoption.
- ArcPy script analysis requires detecting that a Python file uses ArcPy
  (`import arcpy` present) and tagging wiki entries with GIS-specific context
  (what layer it reads, what spatial reference it assumes).

**Gate B:**
```
# ArcGIS Pro with add-in installed
# Add-In tab → XIIGen → Code Map
# DockPane opens in ArcGIS Pro
# Enter "feature class" in search → results show C# methods referencing FeatureClass API
```

---

## Cross-Platform Development Order

Phase 1 should be built in waves that maximise code reuse:

### Wave 0 — Core library (before any platform work)

Build `41.SmartCodeWiki` with all four extraction tiers and all three modes.
This is platform-agnostic — no adapter code. Deliverable: a self-contained
`wiki-context-hook.exe` that passes all gate B tests when called from a terminal.

Duration: 3–4 weeks.
Gate B: `wiki-context-hook.exe --build` on llm_mvp_core → ≥ 100 entries.
`wiki-context-hook.exe --map "authorization"` → ≥ 3 results in ≤ 200ms.

### Wave 1 — VS Code family (1 build, 4 platforms)

Covers: VS Code, Cursor, Windsurf, Open VSX.
One `.vsix`, one publishing step, four platforms.
Duration: 2–3 weeks.

### Wave 2 — .NET-native IDEs (3 platforms, shared code)

Covers: Visual Studio, Unity, Autodesk, ArcGIS, Rhino.
All use Model B (direct .NET assembly). The `SmartWikiBuilder` and
`WikiContextRetriever` API is called identically from all five — only the
UI layer differs (WPF for VS and ArcGIS, EditorWindow for Unity,
IExternalCommand for Autodesk, Eto.Forms for Rhino).
Duration: 4–5 weeks.

### Wave 3 — JVM IDEs (2 platforms, shared code)

Covers: JetBrains, Eclipse.
Both use Model D (`ProcessBuilder`). The Kotlin plugin infrastructure from
JetBrains is reused for Eclipse with Java.
Duration: 3–4 weeks.

### Wave 4 — Python-based tools (3 platforms, shared code)

Covers: Blender, QGIS, OBS.
All use Model C (Python `subprocess.run`). The Python wrapper code is
identical — only the UI framework differs (bpy.types.Panel for Blender,
PyQGIS Qt for QGIS, script properties for OBS).
Duration: 3 weeks.

### Wave 5 — Node.js creative and productivity tools (3 platforms)

Covers: Raycast, Obsidian, Adobe CEP.
All use Model A (Node.js spawn). Similar to VS Code but different UI frameworks.
Duration: 3–4 weeks.

### Wave 6 — Single-language niche platforms (3 platforms)

Covers: SketchUp (Ruby/Open3), Unreal (C++/CreateProcess), combined with
remaining testing and marketplace submissions.
Duration: 4–5 weeks.

---

## Language Support Completion Matrix

| Platform | Primary code language | Extraction tier | Supported in Wave |
|---|---|---|---|
| VS Code | Multi-language | A (C#), B (TS/Python/Java/Go) | 1 |
| Cursor | Multi-language | A + B | 1 |
| Windsurf | Multi-language | A + B | 1 |
| Open VSX | Multi-language | A + B | 1 |
| Visual Studio | C#, C++ | A (C#), C (C++) | 2 |
| Unity | C# | A | 2 |
| Autodesk | C#, Python, Dynamo | A (C#), B (Python) | 2 |
| ArcGIS | C#, Python | A (C#), B (Python) | 2 |
| Rhino | C#, Python, Ruby | A (C#), B (Python), C (Ruby) | 2 |
| JetBrains | Java, Kotlin, Python, Go | B | 3 |
| Eclipse | Java | B | 3 |
| Blender | Python | B | 4 |
| QGIS | Python | B | 4 |
| OBS | Python, Lua | B (Python), C (Lua) | 4 |
| Raycast | TypeScript | B | 5 |
| Obsidian | TypeScript, any | B (TS) + A for .cs in vault | 5 |
| Adobe CEP | JavaScript, ExtendScript | B | 5 |
| SketchUp | Ruby | B | 6 |
| Unreal | C++, Blueprints | C (C++ only in Phase 1) | 6 |
