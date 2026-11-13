# RUNTIME_BROWSER_COMMAND_DISCOVERY - 2026-06-29

claim_scope:
- Создан read-only discovery/doc packet для будущего runtime/browser proof в DesignToCode worktree.
- Обнаружены только candidate commands/entrypoints из project files, tests, scripts и handoff docs.
- `no runtime/browser execution`: runtime, browser, server, tests, builds, npm/dotnet execution, git/PR mutation, cleanup, external AI и secret reads не выполнялись.

evidence_scope:
- Worktree listing/read-only metadata: `C:\Projects\llm_mvp_core_DesignToCode_worktree`.
- Handoff docs: `codex-runtime-browser-scenario-prep-2026-06-29.md`, `codex-proof-task-scenario-queue-2026-06-29.md`.
- Project files: `LlmFromScratch.sln`, `44.DesignConversionDomain\44.DesignConversionDomain.csproj`, `Tests\LlmFromScratch.Tests.csproj`, `43.SmartSkillsDomain.Dashboard\43.SmartSkillsDomain.Dashboard.csproj`, `25.ModelServer\ModelServer.csproj`, `Tests.ApiDashboardWpf\Tests.ApiDashboardWpf.csproj`.
- Source/test/script files: `44.DesignConversionDomain\DesignConversionBrowserQaProviders.cs`, `Tests\DesignConversionBrowserQaProviderTests.cs`, `Tests\DesignConversionRuntimeFlowTests.cs`, `Tests\DesignConversionQaProviderTests.cs`, `43.SmartSkillsDomain.Dashboard\App.xaml`, `43.SmartSkillsDomain.Dashboard\App.xaml.cs`, `tools\api-dashboard-qa.ps1`, targeted `Select-String` over DesignConversion/dashboard/WPF files.
- Negative metadata findings from successful read-only searches: no `package.json`; no `launchSettings.json`; no root `global.json`; no root `Directory.*.props`.
- Two broad searches timed out and are not counted as complete evidence.

count_claimed:
- command discovery packets created: 1
- successful current-turn read/list/search outputs used: 31
- runtime/browser executions: 0
- tests/builds/server starts: 0
- npm/dotnet execution commands run: 0
- screenshots captured: 0
- secrets captured: 0
- git/PR/cleanup mutations: 0

independent_evidence_count:
- 31 successful current-turn read/list/search command outputs.

subagent_id_or_NONE:
- delegated-runtime-browser-command-discovery-current-session

downgrade_needed:
- Да. Evidence supports only static command/entrypoint discovery. It does not prove runtime readiness, browser readiness, WPF host readiness, URL availability, visual fidelity, clean repo, merge readiness, PR readiness, or product completion.

## Required literals

- `RUNTIME_BROWSER_COMMAND_DISCOVERY`
- `Work happens on DesignToCode in a separate folder or worktree`
- `no runtime/browser execution`
- `no secrets captured`
- `exact commands require executor validation`
- `Proof readiness items are tasks, not stop blockers`

## Human-readable Russian prep update

`Proof readiness items are tasks, not stop blockers`.

По-человечески: сейчас удалось обнаружить несколько вероятных входов для будущей проверки, но ни один из них не был запущен. Поэтому это не proof run. Это карта кандидатов, которые будущий executor сможет проверить только после отдельного разрешения Luba.

`Work happens on DesignToCode in a separate folder or worktree`.

Точный repo/worktree path:

- `C:\Projects\llm_mvp_core_DesignToCode_worktree`

Основной solution:

- `C:\Projects\llm_mvp_core_DesignToCode_worktree\LlmFromScratch.sln`

## What was discovered

### 1. No npm/browser web-app surface found

Read-only search found no `package.json` in the DesignToCode worktree. Therefore no `npm`, `yarn`, `pnpm`, Vite, Next, or Webpack script can be inferred from package metadata.

No `launchSettings.json` was found. Therefore no ASP.NET launch profile, `applicationUrl`, localhost URL, port, route, or health endpoint can be inferred from launch settings.

No root `global.json` or root `Directory.*.props` was found from the searched metadata.

### 2. DesignConversion browser proof appears to be a .NET browser-process adapter, not a hosted web URL

Likely project:

- `44.DesignConversionDomain\44.DesignConversionDomain.csproj`

Relevant source:

- `44.DesignConversionDomain\DesignConversionBrowserQaProviders.cs`

Relevant focused test file:

- `Tests\DesignConversionBrowserQaProviderTests.cs`

Static evidence from the source/test files:

- `LocalChromiumBrowserProcessAdapter` searches for Chromium-family executables.
- Environment variable names recognized by code: `DESIGN_CONVERSION_BROWSER_EXECUTABLE`, `PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH`, `CHROME_PATH`, `EDGE_PATH`.
- Default Windows executable candidates include Chrome and Edge paths under `C:\Program Files` and `C:\Program Files (x86)`.
- The adapter writes temporary `index.html`, launches browser headless, and captures `screenshot.png`.
- The adapter uses a `file:///.../index.html` URL, not a server URL.
- The real-browser test is `LocalChromiumProcessAdapter_CapturesRealScreenshotWhenRuntimeIsAvailableOrReportsBlocker`.
- If no browser executable is available, expected blocker code is `BROWSER_RUNTIME_UNAVAILABLE`.

Candidate command lane, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionBrowserQaProviderTests
```

Narrower candidate command for the real screenshot test, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionBrowserQaProviderTests.LocalChromiumProcessAdapter_CapturesRealScreenshotWhenRuntimeIsAvailableOrReportsBlocker
```

Candidate environment precondition, requiring executor validation and no secret capture:

```powershell
$env:DESIGN_CONVERSION_BROWSER_EXECUTABLE = '<absolute path to chrome.exe or msedge.exe if default discovery fails>'
```

Important boundary: this lane can prove only the .NET local Chromium screenshot adapter behavior under the recorded environment. It is not proof of a hosted web app, URL route, login flow, source-design fidelity, external visual judge, or full product readiness.

### 3. DesignConversion runtime flow has focused .NET tests but no browser or URL

Relevant focused test files:

- `Tests\DesignConversionRuntimeFlowTests.cs`
- `Tests\DesignConversionQaProviderTests.cs`
- other discovered `Tests\DesignConversion*.cs` files

Candidate command lane, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversion
```

Narrower candidate command for runtime flow scaffold, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionRuntimeFlowTests
```

Important boundary: this lane appears to be .NET unit/runtime-flow evidence, not browser proof. It should not be reported as live browser readiness unless the future worker also runs the approved browser adapter or approved browser scenario.

### 4. SmartSkills dashboard has WPF app/test/script surface, not browser URL surface

Likely project:

- `43.SmartSkillsDomain.Dashboard\43.SmartSkillsDomain.Dashboard.csproj`

Static evidence:

- Targets `net9.0;net9.0-windows`.
- WPF target uses `<OutputType>WinExe</OutputType>` and `<UseWPF>true</UseWPF>`.
- `App.xaml` has `StartupUri="ApiDashboardWindow.xaml"`.
- `ApiDashboardWindow.xaml`/`.cs` exist.
- `tools\api-dashboard-qa.ps1` is a QA script that reads files and checks dashboard/WPF/API evidence boundaries.
- `Tests.ApiDashboardWpf\Tests.ApiDashboardWpf.csproj` targets `net9.0-windows` with WPF enabled.
- `Tests.ApiDashboardWpf\ApiDashboardWindowInteractiveTests.cs` contains WPF control automation names such as `InputTextBox`, `RunButton`, PromptToTask/Orchestration/SkillSelection scenarios, and explicit false claims for host runtime/semantic quality.

Candidate WPF app command, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet run --project 43.SmartSkillsDomain.Dashboard\43.SmartSkillsDomain.Dashboard.csproj -f net9.0-windows
```

Candidate WPF interactive test command, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet test Tests.ApiDashboardWpf\Tests.ApiDashboardWpf.csproj
```

Candidate QA script command, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
powershell -ExecutionPolicy Bypass -File .\tools\api-dashboard-qa.ps1 -Json
```

Important boundary: this is dashboard/WPF evidence, not browser evidence. It has no discovered localhost URL or browser route. The existing script says dashboard evidence remains dashboard-to-core/WPF scope and does not prove Claude/Cursor/Codex host runtime, external AI, semantic quality, or trained-model proof.

### 5. ModelServer project exists but no server URL or launch profile was discovered

Likely project:

- `25.ModelServer\ModelServer.csproj`

Static evidence:

- Project targets `net9.0`.
- Project references `00.Core` and `18.CapabilityTraining`.
- No `launchSettings.json`, URL, port, route, or health endpoint was found in the searched metadata.

Candidate command if future executor chooses to validate ModelServer runtime, requiring executor validation:

```powershell
cd C:\Projects\llm_mvp_core_DesignToCode_worktree
dotnet run --project 25.ModelServer\ModelServer.csproj
```

Important boundary: this command is only a candidate because I did not find a documented URL/port/health endpoint or proof that this is the DesignConversion runtime/browser target.

## URL, port, route, health endpoint

Discovered exact URL/port:

- None.

Most specific browser target inferred from source:

- `file:///.../index.html` generated under `%TEMP%\llm_mvp_core\design-conversion-browser-qa\<guid>\index.html` by `LocalChromiumBrowserProcessAdapter`.

Most specific screenshot output inferred from source:

- `%TEMP%\llm_mvp_core\design-conversion-browser-qa\<guid>\screenshot.png`.

No hosted URL, localhost port, route, or health endpoint was discovered from package metadata, launch settings, handoff docs, or targeted source searches.

## Missing unknowns before execution

Future executor still needs exact decisions before running anything:

- Which lane is the approved proof target: DesignConversion browser adapter, DesignConversion runtime-flow tests, WPF dashboard, ModelServer, or another target.
- Whether `dotnet test` is allowed and which exact filter is approved.
- Whether browser screenshot capture is approved.
- Whether setting/checking `DESIGN_CONVERSION_BROWSER_EXECUTABLE`, `PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH`, `CHROME_PATH`, or `EDGE_PATH` is approved. Secret values must not be captured; these are executable path variables, not credentials.
- Exact evidence output path for transcripts and any screenshots.
- Exact screenshot approval, filenames, and retention policy.
- Exact pass/fail thresholds and downgrade rules.
- For WPF lane: whether visible desktop/window automation is approved.
- For hosted service lane: exact URL/port/health endpoint, because none was discovered.

`exact commands require executor validation`.

## Candidate command summary

These commands were not run. They are candidate commands only.

| Lane | Candidate command | Evidence basis | Claim boundary |
| --- | --- | --- | --- |
| DesignConversion browser adapter | `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionBrowserQaProviderTests` | `Tests\DesignConversionBrowserQaProviderTests.cs`; `44.DesignConversionDomain\DesignConversionBrowserQaProviders.cs` | Focused .NET browser adapter test; may capture headless screenshot if browser exists and execution is authorized. |
| DesignConversion real screenshot only | `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionBrowserQaProviderTests.LocalChromiumProcessAdapter_CapturesRealScreenshotWhenRuntimeIsAvailableOrReportsBlocker` | Specific test method name in `DesignConversionBrowserQaProviderTests.cs` | Narrow browser runtime availability/screenshot check or explicit blocker. |
| DesignConversion runtime flow | `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionRuntimeFlowTests` | `Tests\DesignConversionRuntimeFlowTests.cs` | Runtime-flow unit evidence only; not browser proof. |
| All DesignConversion tests | `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversion` | `Tests\DesignConversion*.cs` files and test project references | Broader .NET test surface; may include non-browser unit tests. |
| WPF dashboard app | `dotnet run --project 43.SmartSkillsDomain.Dashboard\43.SmartSkillsDomain.Dashboard.csproj -f net9.0-windows` | Dashboard csproj and `App.xaml` StartupUri | WPF app/window launch candidate; not browser URL proof. |
| WPF dashboard tests | `dotnet test Tests.ApiDashboardWpf\Tests.ApiDashboardWpf.csproj` | WPF test csproj and interactive test names | WPF control automation; not browser URL proof. |
| Dashboard QA script | `powershell -ExecutionPolicy Bypass -File .\tools\api-dashboard-qa.ps1 -Json` | Script source read-only inspection | Static/scripted dashboard QA boundary check; not runtime/browser proof unless future authority says this script is enough for a narrower claim. |
| ModelServer | `dotnet run --project 25.ModelServer\ModelServer.csproj` | ModelServer csproj exists in solution | Candidate only; no discovered URL/port/health endpoint or DesignConversion tie-in. |

## Next bounded worker order

Next bounded worker after Luba permits actual execution:

`bounded-runtime-browser-execution-worker`

Scope:

- Worktree: `C:\Projects\llm_mvp_core_DesignToCode_worktree`.
- Start from this discovery packet and select exactly one approved lane.
- Run only the exact command(s) approved by Luba.
- If lane is DesignConversion browser adapter, first validate browser executable availability without printing secrets; if default discovery fails, use only an approved browser executable path variable.
- Record cwd, exact command, timestamps, stdout/stderr transcript, exit code, and downgrade line.
- If screenshot capture is approved and actually occurs, record exact screenshot URI/path and sha256 from the test/output where available.
- If no browser executable is available, report `BROWSER_RUNTIME_UNAVAILABLE` as an environment blocker and do not claim browser proof.
- If WPF lane is selected, state that it is WPF/dashboard evidence, not browser URL proof.
- Do not run npm, external AI, git/PR mutations, cleanup, training, package publish, or secret reads.

Stop rules:

- Missing approval for command execution.
- Missing exact lane selection.
- Missing browser executable for browser lane.
- Missing URL/port if a hosted server/browser route is selected.
- Any secret exposure risk.
- Any command outside approved list.
- Any requested claim wider than actual evidence.

## Files changed

- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-runtime-browser-command-discovery-2026-06-29.md`

## Verification

Post-write verification should be readback/literal-only for this file.

Expected verification literals:

- `RUNTIME_BROWSER_COMMAND_DISCOVERY`
- `Work happens on DesignToCode in a separate folder or worktree`
- `no runtime/browser execution`
- `no secrets captured`
- `exact commands require executor validation`
- `Proof readiness items are tasks, not stop blockers`

No runtime/browser/test/build/server/npm/dotnet/git/PR/cleanup/external-AI/secret verification is implied by this packet.

## Class C yes-no

Class C yes-no: no.

Reason: this packet did not create a branch from absent branch/source authority and did not mutate repo state. It is read-only discovery plus one allowed doc output.

## Final boundary

This is a command discovery packet, not a proof run.

The evidence supports only:

- likely .NET/WPF command candidates were discovered;
- no npm package scripts or ASP.NET launch URL/port were discovered;
- DesignConversion browser proof likely uses a Chromium-family executable against temp `file:///` HTML, not a hosted browser URL;
- WPF dashboard paths are candidate WPF/dashboard evidence, not browser URL proof;
- `no runtime/browser execution`;
- `no secrets captured`;
- `exact commands require executor validation`.
