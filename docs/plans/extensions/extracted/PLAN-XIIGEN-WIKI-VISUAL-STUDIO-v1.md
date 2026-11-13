# Plan — XIIGen Smart Code Wiki: Visual Studio Extension
# Version: 1.0 — 2026-05-15
# Status: REFERENCE - DO NOT EXECUTE directly.
# Execution: use SESSION files once approved.
#
# This plan is fully standalone. It does not require any other plugin plan.
# Target: Visual Studio 2022 (17.0+). ICopilotContextProvider requires 17.12+.
# Architecture: VisualStudio.Extensibility (out-of-process, preferred for new extensions).

---

## Goal

Ship a Visual Studio extension that injects wiki context into every GitHub
Copilot prompt via `ICopilotContextProvider`, provides a tool window with
Browse / Ask / Map tabs, and works before the model is trained (Stage A)
and after (Stage B) with zero code changes between stages.

---

## Solution Structure

```
xiigen-vs-extension/
  xiigen-vs-extension.sln
  xiigen-vs-extension/
    xiigen-vs-extension.csproj     ← TargetFramework: net8.0-windows, PackageType: VsixPackage
    source.extension.vsixmanifest  ← extension metadata
    ExtensionEntrypoint.cs         ← VisualStudio.Extensibility entry point
    Services/
      WikiService.cs               ← IWikiOrchestrator via WikiOrchestratorFactory
      WikiOutputLogger.cs          ← logs to VS output window
    Commands/
      ShowWikiPanelCommand.cs      ← View → XIIGen → Code Wiki
      CopyWikiContextCommand.cs    ← copies wiki block to clipboard
    ToolWindows/
      CodeWikiToolWindow.cs        ← ToolWindowPane host
      CodeWikiControl.xaml         ← WPF: search + tabs (Browse/Ask/Map)
      CodeWikiControl.xaml.cs      ← code-behind
      CodeWikiViewModel.cs         ← INotifyPropertyChanged, async query
    CopilotIntegration/
      WikiCopilotContextProvider.cs ← ICopilotContextProvider (VS 17.12+)
      FallbackContextInjector.cs    ← clipboard fallback for VS < 17.12
    StatusBar/
      WikiStatusBarManager.cs      ← IVsStatusbar integration
```

---

## Stage A — Mock (Start Today, No Training Required)

### What Stage A requires

The only prerequisite is that `evidence-index.jsonl` exists:
```
C:\Projects\{project}\.xiigen\wiki-domain\evidence\evidence-index.jsonl
```
Created by SESSION-02. Does not require PROMOTED status.

Mode 3 (map/browse) is real BM25 from day 1.
Mode 2 (Copilot injection) returns mock answers via `MockWikiOrchestrator`.

### Step A.1 — Project file

```xml
<!-- xiigen-vs-extension.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <PackageType>VsixPackage</PackageType>
    <GeneratePkgDefFile>true</GeneratePkgDefFile>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- VisualStudio.Extensibility SDK -->
    <PackageReference Include="Microsoft.VisualStudio.Extensibility"
                      Version="17.12.*" />
    <PackageReference Include="Microsoft.VisualStudio.Extensibility.Build"
                      Version="17.12.*" />
    <!-- Wiki library (references 42.ProjectWikiDomain.dll) -->
    <Reference Include="42.ProjectWikiDomain">
      <HintPath>..\libs\42.ProjectWikiDomain.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Step A.2 — WikiService

`WikiService` is the only class that knows about `42.ProjectWikiDomain`.
All tool window and command code uses `WikiService`, never the domain
directly.

```csharp
// Services/WikiService.cs
using _42_ProjectWikiDomain;
using _42_ProjectWikiDomain.Contracts;

internal sealed class WikiService
{
    private readonly string _projectRoot;
    private readonly IWikiOrchestrator _orchestrator;
    private readonly WikiOutputLogger _logger;

    public WikiService(string solutionPath, WikiOutputLogger logger)
    {
        _projectRoot = solutionPath;
        _logger = logger;
        // Factory returns MockWikiOrchestrator until model-manifest.json
        // shows status == "PROMOTED", then returns ProjectLocalWikiOrchestrator.
        // Zero code change in WikiService when model becomes ready.
        _orchestrator = WikiOrchestratorFactory.Create(solutionPath);
        _logger.Log($"XIIGen Wiki: orchestrator type = {_orchestrator.GetType().Name}");
    }

    public WikiReadinessStatus GetStatus()
    {
        var status = _orchestrator.GetReadinessStatus(_projectRoot);
        _logger.Log($"XIIGen Wiki: status = {status.Status} · {status.EntryCount} entries");
        return status;
    }

    public async Task<WikiAnswer> AskAsync(string question, CancellationToken ct)
    {
        var answer = await _orchestrator.QueryAsync(
            question, _projectRoot, new WikiQueryOptions(TopN: 5), ct);
        _logger.Log(
            $"XIIGen Wiki: confidence={answer.Confidence:F2} " +
            $"evidenceIds={answer.EvidenceIds.Length} " +
            $"status={answer.DocumentationVersion}");
        return answer;
    }

    public WikiRelevanceMap GetMap(string query, int topN = 10)
        => _orchestrator.GetRelevanceMap(query, _projectRoot, topN);

    public static string ResolveSolutionPath()
    {
        // Resolve from DTE2 / VS automation; fallback to cwd
        try
        {
            var dte = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal
                .GetActiveObject("VisualStudio.DTE");
            var sln = dte.Solution?.FullName;
            if (!string.IsNullOrEmpty(sln))
                return Path.GetDirectoryName(sln) ?? Environment.CurrentDirectory;
        }
        catch { /* DTE not available — use cwd */ }
        return Environment.CurrentDirectory;
    }
}
```

**Gate B A.2:**
```csharp
// In test project:
var svc = new WikiService("C:/Projects/askapediaweb", new NullLogger());
var status = svc.GetStatus();
Assert.Equal("MOCK", status.Status);  // no PROMOTED yet → mock
Assert.True(status.EntryCount > 0);   // evidence-index exists
```

### Step A.3 — Tool Window (Browse / Ask / Map)

```csharp
// ToolWindows/CodeWikiToolWindow.cs
using Microsoft.VisualStudio.Extensibility.ToolWindows;

[ToolWindow(WindowAttribute.DialogFrame)]
internal class CodeWikiToolWindow : BaseToolWindow<CodeWikiToolWindow>
{
    public override string Title => "XIIGen Code Wiki";

    public override Type PaneType => typeof(CodeWikiControl);

    public override async Task<IRemoteUserControl> GetControlAsync(
        CancellationToken ct)
    {
        return new CodeWikiControl(
            WikiServiceProvider.Instance);  // singleton per solution
    }
}
```

```xaml
<!-- ToolWindows/CodeWikiControl.xaml -->
<UserControl xmlns:toolkit="clr-namespace:Microsoft.VisualStudio.Shell;assembly=Microsoft.VisualStudio.Shell.15.0">
  <DockPanel>
    <!-- Status -->
    <TextBlock DockPanel.Dock="Top"
               Text="{Binding StatusText}"
               Padding="4"/>

    <!-- Search -->
    <DockPanel DockPanel.Dock="Top">
      <Button Content="Find" Click="OnSearchClick"
              DockPanel.Dock="Right" Width="50"/>
      <TextBox Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}"
               KeyDown="OnSearchKeyDown"/>
    </DockPanel>

    <!-- Tab control -->
    <TabControl>
      <TabItem Header="Ask">
        <DockPanel>
          <TextBox Text="{Binding AskQuery}" DockPanel.Dock="Top"/>
          <Button Content="Ask" Click="OnAskClick" DockPanel.Dock="Top"/>
          <TextBlock Text="{Binding AskAnswer}" TextWrapping="Wrap"
                     Foreground="Gray" Padding="4"/>
        </DockPanel>
      </TabItem>
      <TabItem Header="Map">
        <ListBox ItemsSource="{Binding MapResults}"
                 MouseDoubleClick="OnResultDoubleClick">
          <ListBox.ItemTemplate>
            <DataTemplate>
              <StackPanel>
                <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                <TextBlock Text="{Binding Summary}" Foreground="Gray"/>
                <TextBlock Text="{Binding FilePath}" FontSize="10"/>
              </StackPanel>
            </DataTemplate>
          </ListBox.ItemTemplate>
        </ListBox>
      </TabItem>
      <TabItem Header="Browse">
        <TreeView ItemsSource="{Binding BrowseTree}"/>
      </TabItem>
    </TabControl>
  </DockPanel>
</UserControl>
```

```csharp
// ToolWindows/CodeWikiViewModel.cs
internal sealed class CodeWikiViewModel : INotifyPropertyChanged
{
    private readonly WikiService _wikiService;
    private CancellationTokenSource _cts = new();

    public string StatusText { get; private set; } = "XIIGen Wiki: initializing...";
    public string SearchQuery { get; set; } = string.Empty;
    public string AskQuery { get; set; } = string.Empty;
    public string AskAnswer { get; private set; } = string.Empty;
    public ObservableCollection<MapEntry> MapResults { get; } = new();

    public CodeWikiViewModel(WikiService wikiService)
    {
        _wikiService = wikiService;
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var status = _wikiService.GetStatus();
        StatusText = $"XIIGen Wiki: {status.Status} · {status.EntryCount} entries";
        OnPropertyChanged(nameof(StatusText));
    }

    public void Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        var map = _wikiService.GetMap(query);
        MapResults.Clear();
        foreach (var e in map.Entries)
            MapResults.Add(new MapEntry(e.Name, e.Summary, e.FilePath, e.Score));
    }

    public async Task AskAsync(string question)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        AskAnswer = "Thinking...";
        OnPropertyChanged(nameof(AskAnswer));
        try
        {
            var answer = await _wikiService.AskAsync(question, _cts.Token);
            AskAnswer = answer.Confidence < 0.65
                ? $"[Low confidence: {answer.Confidence:F2}]\n{answer.Answer}"
                : answer.Answer;
        }
        catch (OperationCanceledException) { AskAnswer = string.Empty; }
        OnPropertyChanged(nameof(AskAnswer));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### Step A.4 — Commands

```csharp
// Commands/ShowWikiPanelCommand.cs
[VisualStudioContribution]
internal class ShowWikiPanelCommand : Command
{
    public override CommandConfiguration CommandConfiguration
        => new("Show XIIGen Code Wiki")
        {
            Placements = new[] { CommandPlacement.KnownPlacements.ViewMenuEnd }
        };

    public override Task ExecuteCommandAsync(
        IClientContext context, CancellationToken ct)
        => Extensibility.Shell().ShowToolWindowAsync<CodeWikiToolWindow>(ct);
}
```

### Step A.5 — Copilot integration

**For VS 17.12+ (primary path):**

```csharp
// CopilotIntegration/WikiCopilotContextProvider.cs
[Export(typeof(ICopilotContextProvider))]
[method: ImportingConstructor]
internal sealed class WikiCopilotContextProvider(
    WikiService wikiService) : ICopilotContextProvider
{
    public async Task<string> GetContextAsync(
        string userPrompt, CancellationToken ct)
    {
        var status = wikiService.GetStatus();
        if (!status.IsReady) return string.Empty;

        var answer = await wikiService.AskAsync(userPrompt, ct);
        if (answer.Confidence < 0.65) return string.Empty;

        return WikiContextFormatter.Format(answer);
    }
}
```

**For VS < 17.12 (fallback):**

```csharp
// CopilotIntegration/FallbackContextInjector.cs
// Registers a keyboard shortcut Alt+W that copies the wiki context block
// to the clipboard. Developer pastes it at the start of their Copilot prompt.
// This is the graceful degradation path.

[VisualStudioContribution]
internal class CopyWikiContextCommand : Command
{
    public override CommandConfiguration CommandConfiguration
        => new("Copy XIIGen Wiki Context")
        {
            Shortcuts = new[] { new CommandShortcut(ModifierKey.Alt, Key.W) }
        };

    public override async Task ExecuteCommandAsync(
        IClientContext context, CancellationToken ct)
    {
        var prompt = await GetCurrentEditorSelectionAsync(context, ct);
        var svc = WikiServiceProvider.Instance;
        var answer = await svc.AskAsync(prompt ?? "current context", ct);
        if (answer.Confidence > 0.0)
        {
            var block = WikiContextFormatter.Format(answer);
            Clipboard.SetText(block);
            await Extensibility.Shell().ShowPromptAsync(
                "XIIGen Wiki context copied to clipboard.", ct);
        }
    }
}
```

### Step A.6 — Status bar

```csharp
// StatusBar/WikiStatusBarManager.cs
internal sealed class WikiStatusBarManager
{
    private readonly IVsStatusbar _statusBar;
    private readonly WikiService _wikiService;
    private Timer? _timer;

    public WikiStatusBarManager(IVsStatusbar statusBar, WikiService wikiService)
    {
        _statusBar = statusBar;
        _wikiService = wikiService;
    }

    public void Start()
    {
        Refresh();
        _timer = new Timer(_ => Refresh(), null, TimeSpan.FromMinutes(1),
                           TimeSpan.FromMinutes(1));
    }

    private void Refresh()
    {
        var status = _wikiService.GetStatus();
        _statusBar.SetText(
            $"XIIGen Wiki: {status.Status} · {status.EntryCount}");
    }

    public void Stop() => _timer?.Dispose();
}
```

---

## Stage A: F5 Development Loop

```
1. Open xiigen-vs-extension.sln in Visual Studio 2022
2. Build → no errors (42.ProjectWikiDomain.dll must be in libs/)
3. Press F5
   → Experimental VS instance opens at %AppData%\Roaming\...\Exp\
   → Extension is loaded automatically

4. In experimental instance: File → Open → Folder → C:\Projects\askapediaweb

5. View → XIIGen Code Wiki
   → Tool window opens
   → Status text: "XIIGen Wiki: MOCK · N entries" (N > 0)
   → Ask tab: type "tenant isolation" → answer with [MOCK] prefix appears
   → Map tab: type "authorization" → results appear with file paths

6. Alt+W shortcut
   → Copies wiki context block (mock answer) to clipboard

7. Edit any .cs file in CodeWikiViewModel → Ctrl+Shift+F5 (restart experimental)
   → Changes are live immediately

8. Open Copilot Chat in experimental VS
   → Ask a question about the project
   → Check VS Output → XIIGen Debug channel:
     "Wiki context injected: confidence=0.50 (mock) evidence=2 ids"
```

## Stage A: Full Gate B Checklist

```
□ Build: dotnet build → 0 errors
□ Package: dotnet pack → .vsix created in bin/
□ F5 loop works: experimental VS opens, extension loads
□ Tool window opens via View menu
□ Status bar shows: "XIIGen Wiki: MOCK · N" (N > 0 if evidence-index exists)
□ Ask tab returns answer with [MOCK] prefix
□ Map tab returns ≥ 1 result for "authorization" in askapediaweb
□ Alt+W copies non-empty wiki block to clipboard
□ Output channel logs every invocation with confidence value
□ No crash when solution has no .xiigen/ folder (graceful empty state)
□ dotnet test --filter WikiService → all unit tests pass
```

---

## Stage B — Real Model (After SESSION-04 PROMOTED)

`WikiOrchestratorFactory.Create()` detects PROMOTED and returns the real
orchestrator. `WikiService` does not change. Tool window, commands, and
`WikiCopilotContextProvider` do not change.

### Stage B Gate B Checklist

```
□ model-manifest.json: status == "PROMOTED"
□ Status bar: "XIIGen Wiki: PROMOTED · 247" (no "MOCK")
□ Ask tab: answer has no [MOCK] prefix, evidence IDs are real symbols
□ Confidence logged in output: > 0.65
□ ICopilotContextProvider verified:
     Open Copilot Chat in VS
     Type any question about the project
     Open output channel "XIIGen Debug"
     Should show: "Wiki context injected: confidence=0.87 evidence=2 ids"
     In Copilot Chat, Copilot's answer should reference specific classes
     (sign that it received the wiki context block)
□ Regression tests: dotnet test --filter WikiRegression → all pass
□ Bump extension version to 1.0 in source.extension.vsixmanifest
□ Upload to Visual Studio Marketplace:
     vsix-publisher publish
       --publisher xiigen
       --extensionId xiigen-wiki
       --version 1.0.0
       --vsixPath ./bin/xiigen-wiki-1.0.0.vsix
```

---

## Verifying ICopilotContextProvider Works (Stage B Only)

The provider fires on every Copilot Chat message. To verify it actually
receives the wiki context:

```csharp
// Add to WikiCopilotContextProvider.GetContextAsync() for debugging:
Debug.WriteLine($"[XIIGen] GetContextAsync called for: {userPrompt[..Math.Min(50, userPrompt.Length)]}");
Debug.WriteLine($"[XIIGen] Returning context: {result?.Length ?? 0} chars");

// In Output window (Debug category), you should see these lines
// when sending a message in Copilot Chat.
// If you do NOT see them: ICopilotContextProvider is not wired correctly.
// Check: export attribute, MEF composition, VS version >= 17.12
```

---

## VS Version Compatibility Table

| VS Version | ICopilotContextProvider | Tool Window | Mode 3 Map | Fallback |
|---|---|---|---|---|
| 17.12+ | ✅ Auto-injects | ✅ | ✅ | N/A |
| 17.0–17.11 | ❌ | ✅ | ✅ | Alt+W copy |
| < 17.0 | ❌ | ❌ | ❌ | Not supported |

Always check VS version at startup:

```csharp
// ExtensionEntrypoint.cs
var vsVersion = new Version(
    (int)Extensibility.GetVSVersionAsync().GetAwaiter().GetResult().Major,
    (int)Extensibility.GetVSVersionAsync().GetAwaiter().GetResult().Minor);

var hasCopilotContextProvider = vsVersion >= new Version(17, 12);
WikiServiceProvider.HasCopilotIntegration = hasCopilotContextProvider;
```

---

## Development Loop Summary

```
Day 1–3 (Stage A setup):
  □ Create extension project with VisualStudio.Extensibility template
  □ Add 42.ProjectWikiDomain.dll reference to libs/
  □ WikiService + unit test (mock orchestrator, no solution needed)
  □ Empty tool window: opens via View menu, shows "initializing"
  □ Status bar item: shows "XIIGen Wiki: MOCK · 0"

Day 3–6 (Stage A features):
  □ BrowseTab: reads evidence-index.jsonl, shows namespace tree
  □ MapTab: calls --map via WikiService.GetMap(), shows sorted results
  □ AskTab: calls WikiService.AskAsync(), shows mock answer
  □ Output channel logging

Day 6–7 (Stage A Copilot):
  □ WikiCopilotContextProvider registered (VS 17.12+ only)
  □ FallbackContextInjector Alt+W command (VS < 17.12)
  □ Verify output channel shows injection logs

Day 7 (Stage A Gate B):
  □ All Gate B checks pass
  □ F5 loop works end-to-end with mock

When SESSION-04 PROMOTED (Stage B):
  □ Run regression tests
  □ Verify Copilot Chat receives real wiki context
  □ Upload to VS Marketplace
```
