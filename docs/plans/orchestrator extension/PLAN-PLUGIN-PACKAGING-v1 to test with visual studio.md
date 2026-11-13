# PLAN: Packaging llm_mvp_core Into Plugin Binaries
# Version: 1.0 — 2026-05-15
# Status: REFERENCE — architecture and build plan for wiki-context-hook.exe
#         and 42.ProjectWikiDomain.dll distribution.

---

## The Core Problem

`llm_mvp_core` contains 35+ projects. The hook binary needs only a small subset.
Shipping everything creates:
- Binary size: 50–200 MB (unacceptable for a .vsix sideload or VS Code extension)
- Startup time: slow cold start if unused assemblies are loaded
- Attack surface: unnecessary code in a binary that runs on developer machines

The solution is a **dependency slice** — a dedicated project that references
only the projects needed for the hook's specific mode, compiled with IL trimming
and single-file publish.

---

## Dependency Map: What Each Mode Needs

The hook has two runtime modes. They have completely different dependency chains.

```
Mode 3 — BM25 code map (offline, < 200ms):
  wiki-context-hook.exe
    → 42.ProjectWikiDomain (WikiContextRetriever, WikiContextFormatter)
        → 12.SearchEngine (BM25Scorer, InvertedIndex, StandardAnalyzer)
            → [no external NuGet packages]
        → System.Text.Json (BCL, always available)
  Total: ~3 projects from llm_mvp_core + BCL

Mode 2 — Semantic wiki answer (requires trained weights, < 500ms):
  wiki-context-hook.exe
    → 42.ProjectWikiDomain (ProjectLocalWikiOrchestrator, WikiContextRetriever)
        → 12.SearchEngine (same as Mode 3)
        → 08.GraphRag (HybridRetriever, Indexer, LocalSearch, GlobalSearch)
            → 05.Embeddings (EmbeddingModel)
                → 09.Inference (TextGenerator, Sampler, KvCache)
                    → 03.TransformerModel (TransformerBlock, Attention, etc.)
                        → 00.TensorCore (Tensor operations)
                        → 01.Tokenizer (BPETokenizer, etc.)
        → 06.VectorSearch (BruteForceIndex or HnswIndex)
        → 07.GraphEngine
        → 25.ModelServer (IModelRepository — loads weights from disk)
  Total: ~12 projects from llm_mvp_core + BCL

Mode 3 only needs 3 projects. Mode 2 needs 12.
These are separate binaries.
```

---

## Two Binaries, Not One

```
Deliverable 1: wiki-context-hook-mode3.exe   (~3 MB self-contained)
  Provides: --map query, --ensure-ready status check
  Requires: evidence-index.jsonl (from SESSION-02)
  No model weights needed
  Works before training completes

Deliverable 2: wiki-context-hook.exe          (~25–40 MB self-contained)
  Provides: Mode 2 inference + Mode 3 BM25 + --map + --ensure-ready
  Requires: model-manifest.json with status = PROMOTED
  Falls back to Mode 3 if weights not found
  Replaces mode3 binary after SESSION-04 completes
```

The VS Code extension and VS extension ship `wiki-context-hook-mode3.exe` first.
It replaces with `wiki-context-hook.exe` after the wiki is trained.

---

## Project Structure: WikiHookRunner

A dedicated project for each binary. These projects contain ONLY Program.cs
and a reference list. All logic stays in 42.ProjectWikiDomain.

```
llm_mvp_core/
  42.ProjectWikiDomain/
    42.ProjectWikiDomain.csproj
    ...all wiki domain classes...
  42.WikiHookRunner.Mode3/
    42.WikiHookRunner.Mode3.csproj    ← Mode 3 binary
    Program.cs
    publish-profiles/
      win-x64.pubxml
      linux-x64.pubxml
      osx-arm64.pubxml
  42.WikiHookRunner/
    42.WikiHookRunner.csproj          ← Full binary (Mode 2 + Mode 3)
    Program.cs
    publish-profiles/
      win-x64.pubxml
      linux-x64.pubxml
      osx-arm64.pubxml
```

### 42.WikiHookRunner.Mode3.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>wiki-context-hook-mode3</AssemblyName>
    <RootNamespace>WikiHookRunner</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Optimize>true</Optimize>

    <!-- Single-file publish -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>

    <!-- IL trimming — removes unused code from BCL and referenced assemblies -->
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>

    <!-- ReadyToRun — pre-compiles to native code for faster startup -->
    <PublishReadyToRun>true</PublishReadyToRun>

    <!-- Compression — reduces binary size ~30% -->
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>

    <!-- Suppress trimmer warnings for code we know is safe -->
    <TrimmerRootDescriptor>trimmer-roots.xml</TrimmerRootDescriptor>
  </PropertyGroup>

  <!-- Only the projects needed for Mode 3 -->
  <ItemGroup>
    <ProjectReference Include="..\42.ProjectWikiDomain\42.ProjectWikiDomain.csproj"
                      TrimmerRootMark="true" />
    <!-- 12.SearchEngine is transitively included via 42.ProjectWikiDomain -->
  </ItemGroup>

  <!-- Exclude Mode 2 heavy chain explicitly if transitively pulled in -->
  <ItemGroup>
    <TrimmerRootAssembly Remove="TensorCore" />
    <TrimmerRootAssembly Remove="TransformerModel" />
    <TrimmerRootAssembly Remove="Inference" />
    <TrimmerRootAssembly Remove="Embeddings" />
    <TrimmerRootAssembly Remove="GraphRag" />
    <TrimmerRootAssembly Remove="VectorSearch" />
  </ItemGroup>
</Project>
```

### 42.WikiHookRunner.csproj (full Mode 2 + Mode 3)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <AssemblyName>wiki-context-hook</AssemblyName>
    <RootNamespace>WikiHookRunner</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Optimize>true</Optimize>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
    <PublishReadyToRun>true</PublishReadyToRun>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <TrimmerRootDescriptor>trimmer-roots.xml</TrimmerRootDescriptor>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\42.ProjectWikiDomain\42.ProjectWikiDomain.csproj" />
    <ProjectReference Include="..\08.GraphRag\GraphRag.csproj" />
    <ProjectReference Include="..\06.VectorSearch\VectorSearch.csproj" />
    <ProjectReference Include="..\12.SearchEngine\SearchEngine.csproj" />
    <ProjectReference Include="..\25.ModelServer\ModelServer.csproj" />
    <!-- TensorCore, TransformerModel, Inference, Embeddings, Tokenizer
         are all pulled in transitively through 08.GraphRag → 05.Embeddings
         → 09.Inference → 03.TransformerModel → 00.TensorCore, 01.Tokenizer -->
  </ItemGroup>
</Project>
```

---

## Program.cs — Minimal Entry Point

Both binaries share the same Program.cs structure. Mode 3 binary simply
returns empty/error for inference calls because those code paths are trimmed away.

```csharp
// 42.WikiHookRunner/Program.cs (same for both binaries)
using ProjectWikiDomain;
using ProjectWikiDomain.Contracts;
using System.Text.Json;

var projectRoot = Environment.GetEnvironmentVariable("XIIGEN_PROJECT_ROOT")
    ?? Directory.GetCurrentDirectory();

// Argument dispatch
if (args.Length > 0)
{
    return args[0] switch
    {
        "--map"          => await RunMapAsync(args, projectRoot),
        "--ensure-ready" => await RunEnsureReadyAsync(args, projectRoot),
        "--json"         => await RunEnsureReadyAsync(args, projectRoot, jsonOutput: true),
        "--version"      => PrintVersion(),
        _                => await RunHookAsync(projectRoot)
    };
}

return await RunHookAsync(projectRoot);

// ── Hook mode (called on every prompt) ──────────────────────────────────────
static async Task<int> RunHookAsync(string projectRoot)
{
    try
    {
        var orchestrator = WikiOrchestratorFactory.Create(projectRoot);
        var status = orchestrator.GetReadinessStatus(projectRoot);

        if (!status.IsReady)
        {
            // Always exit 0 — never block a prompt
            Console.WriteLine("{}");
            return 0;
        }

        var prompt = Environment.GetEnvironmentVariable("HOOK_PROMPT") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Console.WriteLine("{}");
            return 0;
        }

        var topN = int.TryParse(
            Environment.GetEnvironmentVariable("WIKI_TOP_N"), out var n) ? n : 5;
        var minConf = double.TryParse(
            Environment.GetEnvironmentVariable("WIKI_CONFIDENCE_MIN"),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var c) ? c : 0.65;

        var answer = await orchestrator.QueryAsync(
            prompt, projectRoot, new WikiQueryOptions(topN), CancellationToken.None);

        if (answer.Confidence < minConf)
        {
            Console.WriteLine("{}");
            return 0;
        }

        var formatted = WikiContextFormatter.Format(answer);
        var output = new { systemPromptAddition = formatted };
        Console.WriteLine(JsonSerializer.Serialize(output));
        return 0;
    }
    catch
    {
        // Never crash the developer's prompt
        Console.WriteLine("{}");
        return 0;
    }
}

// ── Map mode ─────────────────────────────────────────────────────────────────
static async Task<int> RunMapAsync(string[] args, string projectRoot)
{
    var query = args.Length > 1 ? args[1] : string.Empty;
    if (string.IsNullOrWhiteSpace(query))
    {
        Console.Error.WriteLine("Usage: wiki-context-hook --map <query>");
        return 1;
    }

    var orchestrator = WikiOrchestratorFactory.Create(projectRoot);
    var map = orchestrator.GetRelevanceMap(query, projectRoot, topN: 10);

    Console.WriteLine($"Query: \"{query}\"");
    Console.WriteLine();
    foreach (var e in map.Entries)
        Console.WriteLine($"  {e.Score * 10:F0}/10  {e.Name,-30} — {e.Summary}");

    if (map.DataFlows.Length > 0)
    {
        Console.WriteLine();
        Console.WriteLine("Data flows:");
        foreach (var f in map.DataFlows)
            Console.WriteLine($"  {f}");
    }
    return 0;
}

// ── Status mode ───────────────────────────────────────────────────────────────
static async Task<int> RunEnsureReadyAsync(string[] args, string projectRoot,
    bool jsonOutput = false)
{
    var orchestrator = WikiOrchestratorFactory.Create(projectRoot);
    var status = orchestrator.GetReadinessStatus(projectRoot);

    if (jsonOutput)
    {
        Console.WriteLine(JsonSerializer.Serialize(status));
        return status.IsReady ? 0 : 1;
    }

    Console.WriteLine($"XIIGen Wiki Status for: {projectRoot}");
    Console.WriteLine($"  Status:           {status.Status}");
    Console.WriteLine($"  Entries:          {status.EntryCount}");
    Console.WriteLine($"  Trained on:       {status.DocumentationVersion}");
    Console.WriteLine($"  Ready to serve:   {(status.IsReady ? "Yes" : "No")}");
    if (!status.IsReady)
        Console.WriteLine($"  Next action:      {status.NextAction}");
    return status.IsReady ? 0 : 1;
}

static int PrintVersion()
{
    var version = typeof(WikiOrchestratorFactory).Assembly.GetName().Version;
    Console.WriteLine($"wiki-context-hook {version}");
    return 0;
}
```

---

## trimmer-roots.xml — Preserve What Trimmer Would Remove

IL trimmer aggressively removes code. Some things must be explicitly preserved:

```xml
<!-- trimmer-roots.xml — same file used by both runner projects -->
<linker>
  <!-- Keep the full public surface of 42.ProjectWikiDomain -->
  <assembly fullname="42.ProjectWikiDomain" preserve="all" />

  <!-- Keep JSON serialization attributes (trimmer removes [JsonPropertyName] etc.) -->
  <assembly fullname="System.Text.Json">
    <type fullname="System.Text.Json.Serialization.*" preserve="all" />
  </assembly>

  <!-- Keep file I/O used by evidence-index reader -->
  <assembly fullname="System.IO.FileSystem" preserve="all" />

  <!-- Keep SHA256 for ConnectionId generation -->
  <assembly fullname="System.Security.Cryptography.Algorithms" preserve="all" />
</linker>
```

---

## Publish Profiles

### win-x64.pubxml (for VS extension and Windows VS Code)

```xml
<!-- publish-profiles/win-x64.pubxml -->
<Project>
  <PropertyGroup>
    <PublishProtocol>FileSystem</PublishProtocol>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>true</PublishTrimmed>
    <PublishReadyToRun>true</PublishReadyToRun>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishDir>..\..\dist\win-x64\</PublishDir>
    <Configuration>Release</Configuration>
  </PropertyGroup>
</Project>
```

### linux-x64.pubxml (for VS Code on Linux / WSL)

```xml
<Project>
  <PropertyGroup>
    <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>true</PublishTrimmed>
    <PublishReadyToRun>true</PublishReadyToRun>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishDir>..\..\dist\linux-x64\</PublishDir>
    <Configuration>Release</Configuration>
  </PropertyGroup>
</Project>
```

### osx-arm64.pubxml (for VS Code on Apple Silicon)

```xml
<Project>
  <PropertyGroup>
    <RuntimeIdentifier>osx-arm64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <PublishTrimmed>true</PublishTrimmed>
    <!-- ReadyToRun not supported for cross-compile — use on native Mac build agent -->
    <PublishReadyToRun>false</PublishReadyToRun>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishDir>..\..\dist\osx-arm64\</PublishDir>
    <Configuration>Release</Configuration>
  </PropertyGroup>
</Project>
```

---

## Build Commands

```powershell
# ── Build Mode 3 binary (all platforms) ────────────────────────────────────
cd C:\Projects\llm_mvp_core\42.WikiHookRunner.Mode3

dotnet publish -p:PublishProfile=win-x64     # → dist/win-x64/wiki-context-hook-mode3.exe
dotnet publish -p:PublishProfile=linux-x64   # → dist/linux-x64/wiki-context-hook-mode3
dotnet publish -p:PublishProfile=osx-arm64   # → dist/osx-arm64/wiki-context-hook-mode3

# ── Build full binary (all platforms) ──────────────────────────────────────
cd C:\Projects\llm_mvp_core\42.WikiHookRunner

dotnet publish -p:PublishProfile=win-x64     # → dist/win-x64/wiki-context-hook.exe
dotnet publish -p:PublishProfile=linux-x64   # → dist/linux-x64/wiki-context-hook
dotnet publish -p:PublishProfile=osx-arm64   # → dist/osx-arm64/wiki-context-hook

# ── Measure actual sizes ───────────────────────────────────────────────────
Get-ChildItem .\dist -Recurse -Filter "wiki-context-hook*" |
  Select-Object Name, @{N='MB';E={[math]::Round($_.Length/1MB,1)}}
```

---

## Expected Binary Sizes

| Binary | Mode | Platform | Target size | Max acceptable |
|---|---|---|---|---|
| `wiki-context-hook-mode3.exe` | BM25 only | win-x64 | 3–5 MB | 8 MB |
| `wiki-context-hook-mode3` | BM25 only | linux-x64 | 3–5 MB | 8 MB |
| `wiki-context-hook-mode3` | BM25 only | osx-arm64 | 3–5 MB | 8 MB |
| `wiki-context-hook.exe` | Full (Mode 2+3) | win-x64 | 20–35 MB | 50 MB |
| `wiki-context-hook` | Full (Mode 2+3) | linux-x64 | 20–35 MB | 50 MB |
| `wiki-context-hook` | Full (Mode 2+3) | osx-arm64 | 20–35 MB | 50 MB |

If Mode 3 binary exceeds 8 MB: trimmer roots are too broad. Inspect with
`dotnet-trace` or `ILSpy` to find what is being included unexpectedly.

If Mode 2 binary exceeds 50 MB: TensorCore or Tokenizer are pulling in
unexpected transitive dependencies. Run `dotnet publish --verbosity detailed`
and look for assemblies in the output directory before compression.

---

## Size Verification Step

After every publish, run this check before shipping:

```powershell
# Gate: Mode 3 binary must be under 8 MB
$mode3 = (Get-Item ".\dist\win-x64\wiki-context-hook-mode3.exe").Length
if ($mode3 -gt 8MB) {
    Write-Error "Mode 3 binary too large: $([math]::Round($mode3/1MB,1)) MB. Check trimmer-roots.xml."
    exit 1
}

# Gate: Full binary must be under 50 MB
$full = (Get-Item ".\dist\win-x64\wiki-context-hook.exe").Length
if ($full -gt 50MB) {
    Write-Error "Full binary too large: $([math]::Round($full/1MB,1)) MB. Inspect transitive deps."
    exit 1
}

Write-Host "Mode 3: $([math]::Round($mode3/1MB,1)) MB — OK"
Write-Host "Full:   $([math]::Round($full/1MB,1)) MB — OK"
```

---

## Startup Time Verification

```powershell
# Cold start must be < 50ms for Mode 3 (no weights to load)
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$env:HOOK_PROMPT = "test"
$env:XIIGEN_PROJECT_ROOT = "C:\Projects\noproject"
.\dist\win-x64\wiki-context-hook-mode3.exe | Out-Null
$sw.Stop()
Write-Host "Mode 3 cold start: $($sw.ElapsedMilliseconds) ms"

# Target: < 50ms. If > 200ms: ReadyToRun compilation is not working.
# Fix: ensure PublishReadyToRun=true in the publish profile.
```

---

## What NOT To Include

These projects must never appear in the plugin binary:

```
DO NOT INCLUDE:
  04.TrainingLoop     ← DPO training loop (only runs server-side)
  10.LoRA             ← LoRA adapter training
  11.DPO              ← DPO pair processing
  02.DatasetPipeline  ← dataset preparation
  14.ContinuousLearning
  14.MachineLearning
  16.AutonomousLearning
  InternalTrainReadyDpoRunner ← the training runner itself
  any *Tests* project
  any *Experiments* project

CHECK: after publish, scan the output directory for these assembly names.
If any are present, the csproj has an unexpected transitive reference.
```

```powershell
# Verification: none of the training assemblies must appear
$forbidden = @("TrainingLoop","LoRA","DPO","DatasetPipeline",
               "ContinuousLearning","MachineLearning","AutonomousLearning")

# With single-file publish, assemblies are embedded — use ILSpy or:
dotnet publish --no-build -r win-x64 --no-self-contained 2>&1 |
  Select-String "Copying" |
  ForEach-Object {
    $name = ($_ -split '\\')[-1]
    foreach ($f in $forbidden) {
      if ($name -like "*$f*") {
        Write-Error "Forbidden assembly included: $name"
      }
    }
  }
```

---

## Distribution Into IDE Plugins

### VS Code (.vsix)

```
xiigen-vscode-extension/
  bin/
    win-x64/
      wiki-context-hook-mode3.exe      ← shipped in Stage A
      wiki-context-hook.exe            ← shipped in Stage B (replaces mode3)
    linux-x64/
      wiki-context-hook-mode3
      wiki-context-hook
    osx-arm64/
      wiki-context-hook-mode3
      wiki-context-hook
```

The extension selects the correct binary at runtime:

```typescript
// hookRunner.ts
function getHookBinaryPath(): string {
    const platform = process.platform           // 'win32' | 'linux' | 'darwin'
    const arch     = process.arch               // 'x64' | 'arm64'
    const rid      = platform === 'win32'  ? 'win-x64'
                   : platform === 'darwin' ? `osx-${arch}`
                   : `linux-${arch}`

    const mode3 = path.join(__dirname, '..', 'bin', rid, 'wiki-context-hook-mode3')
    const full  = path.join(__dirname, '..', 'bin', rid, 'wiki-context-hook')

    // Use full binary if available (Stage B), otherwise mode3 (Stage A)
    return require('fs').existsSync(full + (rid.startsWith('win') ? '.exe' : ''))
        ? full + (rid.startsWith('win') ? '.exe' : '')
        : mode3 + (rid.startsWith('win') ? '.exe' : '')
}
```

### Visual Studio (.vsix)

The VS extension ships only the Windows binaries (VS is Windows-only):

```
xiigen-vs-extension/
  libs/
    42.ProjectWikiDomain.dll    ← referenced directly (not single-file)
    win-x64/
      wiki-context-hook-mode3.exe
      wiki-context-hook.exe
```

For the VS extension using the DLL directly (not the exe), the DLL must be
built separately without single-file publish:

```powershell
# 42.ProjectWikiDomain.dll for VS extension (not self-contained, not single-file)
cd C:\Projects\llm_mvp_core\42.ProjectWikiDomain
dotnet build -c Release -r win-x64 --no-self-contained
# → 42.ProjectWikiDomain/bin/Release/net9.0/win-x64/42.ProjectWikiDomain.dll

# Copy to VS extension libs/
Copy-Item ".\bin\Release\net9.0\win-x64\42.ProjectWikiDomain.dll" `
          "..\xiigen-vs-extension\libs\"
```

The VS extension csproj references this DLL:

```xml
<Reference Include="42.ProjectWikiDomain">
  <HintPath>..\libs\42.ProjectWikiDomain.dll</HintPath>
  <Private>true</Private>   <!-- copies to output directory -->
</Reference>
```

---

## CI/CD Build Pipeline

```yaml
# .github/workflows/plugin-binaries.yml
name: Build Plugin Binaries

on:
  push:
    paths:
      - '42.ProjectWikiDomain/**'
      - '42.WikiHookRunner/**'
      - '42.WikiHookRunner.Mode3/**'

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.x' }

      - name: Build Mode 3 (win-x64)
        run: dotnet publish 42.WikiHookRunner.Mode3 -p:PublishProfile=win-x64

      - name: Build Full (win-x64)
        run: dotnet publish 42.WikiHookRunner -p:PublishProfile=win-x64

      - name: Verify sizes
        shell: pwsh
        run: |
          $m3 = (Get-Item "dist/win-x64/wiki-context-hook-mode3.exe").Length
          $full = (Get-Item "dist/win-x64/wiki-context-hook.exe").Length
          if ($m3 -gt 8MB) { throw "Mode3 too large: $($m3/1MB) MB" }
          if ($full -gt 50MB) { throw "Full too large: $($full/1MB) MB" }
          Write-Host "Mode3: $([math]::Round($m3/1MB,1)) MB, Full: $([math]::Round($full/1MB,1)) MB"

      - name: Verify startup time
        shell: pwsh
        run: |
          $env:HOOK_PROMPT = "test"
          $env:XIIGEN_PROJECT_ROOT = "C:/noproject"
          $sw = [Diagnostics.Stopwatch]::StartNew()
          & "dist/win-x64/wiki-context-hook-mode3.exe"
          $sw.Stop()
          if ($sw.ElapsedMilliseconds -gt 200) { throw "Startup too slow: $($sw.ElapsedMilliseconds) ms" }

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.x' }
      - run: dotnet publish 42.WikiHookRunner.Mode3 -p:PublishProfile=linux-x64
      - run: dotnet publish 42.WikiHookRunner -p:PublishProfile=linux-x64

  build-mac:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.x' }
      - run: dotnet publish 42.WikiHookRunner.Mode3 -p:PublishProfile=osx-arm64
      - run: dotnet publish 42.WikiHookRunner -p:PublishProfile=osx-arm64
```

---

## Trimmer Warning Handling

IL trimming emits warnings for reflection-based code. Handle them explicitly:

```xml
<!-- In 42.ProjectWikiDomain.csproj: suppress known-safe reflection uses -->
<ItemGroup>
  <!-- JSON source generation eliminates reflection at runtime -->
  <TrimmerRootMark Include="System.Text.Json.JsonSerializer" />
</ItemGroup>

<!-- Use source-generated JSON serialization instead of runtime reflection -->
```

```csharp
// 42.ProjectWikiDomain/Json/WikiJsonContext.cs
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = false)]
[JsonSerializable(typeof(WikiAnswer))]
[JsonSerializable(typeof(WikiReadinessStatus))]
[JsonSerializable(typeof(WikiRelevanceMap))]
[JsonSerializable(typeof(WikiManifest))]
[JsonSerializable(typeof(EvidenceEntry[]))]
internal partial class WikiJsonContext : JsonSerializerContext { }
```

Source generation replaces `JsonSerializer.Deserialize<T>(string)` calls with
pre-compiled serializers — no reflection at runtime, no trimmer warnings,
faster deserialization.

---

## Native AOT (Future — Not Required For v1)

Native AOT produces a binary with no .NET runtime at all (< 5 MB for Mode 3,
< 15 MB for full). It is not needed for v1 because self-contained single-file
already meets the size targets. Add AOT in v2 if startup time becomes critical
or binary size targets tighten.

```xml
<!-- Future: add to csproj when ready -->
<PublishAot>true</PublishAot>
<!-- Requires: all code must be AOT-compatible (no dynamic code generation) -->
<!-- Requires: source-generated JSON (already done above) -->
<!-- Not compatible with: some reflection patterns in GraphRag/Embeddings -->
<!-- Fix: audit and fix those patterns before enabling -->
```
