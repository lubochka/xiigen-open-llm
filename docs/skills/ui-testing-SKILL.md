# ui-testing-SKILL.md — Desktop UI testing protocol

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
## v1.1: Windows Forms + WPF dashboard evidence boundaries

## When to load
Load before making any change to a .Desktop MainForm.cs or Program.cs file.
Load when a change to a library class could affect what a form displays.
Load when adding a new demo stage to any Desktop stage explorer.
Load when adding or reviewing a WPF dashboard, API lab, view model, command
handler, or UI evidence claim.

---

## The problem this skill solves

Xiigen used jest + @testing-library to test React components automatically.
Windows Forms and current WPF dashboard surfaces have no existing repository-wide
interactive component harness equivalent to browser testing.

The solution is a split responsibility:
- All business logic lives in the class library (tested by xUnit, never in the form)
- The form only wires controls to library calls and formats output
- The form is verified manually using the Desktop stage explorer

This split means the test suite can cover all logic without a UI harness.
The form is kept thin enough that manual verification catches all UI issues.

For WPF dashboard work, the same split applies:
- XAML and code-behind stay thin.
- View models expose command paths and state transitions.
- xUnit may automate the view model and the same handler methods that WPF
  controls bind to.
- A view-model or handler-level test is useful fallback evidence, but it is not
  full interactive WPF automation unless the test actually drives the running
  WPF window through the UI layer.

Never rename fallback evidence as stronger evidence. "View-model automation"
means the command path was exercised. "Full interactive WPF automation" means a
real WPF window/control path was invoked and observed. "Host runtime evidence"
means Claude, Cursor, Codex, or another host invoked its own entrypoint. These
are separate proof scopes.

"UX automation" is a fourth proof scope for user-facing surfaces. It proves
that the UI is understandable and operable for a human, not only that controls
can be clicked. A full UI automation pass does not automatically imply UX
automation unless the test also checks the human-facing surface.

---

## What belongs in the form (MainForm.cs)

Allowed in MainForm.cs:
- Control creation and layout (SplitContainer, ListBox, TextBox, Button, StatusStrip)
- `async Task RunSelectedAsync()` that calls the library and puts the result in `_outputBox`
- Demo sequences using in-memory fabric implementations
- Error display: `catch (Exception ex) { _outputBox.AppendText(ex.ToString()); }`

Not allowed in MainForm.cs:
- Business logic (any algorithm, any OperationResult creation)
- Any class that is not a Form, Control, or demo-only stub
- Any method longer than 30 lines (extract to library)

## WPF dashboard evidence standard

Use this standard for `ApiDashboardWindow`, `ApiDashboardViewModel`, or any
future WPF API dashboard:

```
□ XAML/code-behind contains binding/layout only
□ View model command methods are testable without launching the window
□ Tests drive the same command methods that buttons bind to
□ The report says "view-model automation" when only the view model was driven
□ The report says "WPF build" when only the WPF target compiled
□ The report does NOT say "full interactive WPF automation" unless the running
  WPF window/control path was actually invoked
□ The report does NOT say "Claude/Cursor/Codex host runtime" unless that host
  entrypoint was invoked
```

Accepted fallback for the current dashboard style:
- `dotnet test` proves view-model/catalog/runner behavior.
- `dotnet build -f net9.0-windows` proves WPF project compilation.
- A dashboard-scoped QA script may prove documentation, catalog, and evidence
  boundary wording.

Rejected overclaim:
- "The handler/view-model test passed, therefore the WPF UI is fully automated."
- "The dashboard called core, therefore Claude/Cursor/Codex runtime works."
- "The XAML exists, therefore the user journey is end-to-end proven."

## Dual-Path Same-Scenario UI Proof

When a plan or work order asks for visual evidence for Cursor, Claude, or Codex
host behavior, visual evidence means the same scenario twice:

- Path A: automated pre-host/core/adapter execution.
- Path B: real local host UI execution in Cursor, Claude, or Codex.

The automated path and real host UI path must use the same user prompt/task, the
same expected behavior, the same selected skills/plan or response contract, and
a result comparison. A marker or correlation id may connect traces, but it is
not proof by itself.

Rejected weak visual evidence:
- JSON/log/marker screenshot is not UI proof.
- Opening a JSON/log/result file in the app is not UI proof.
- Showing only a marker is not UI proof.
- Screenshotting a debug artifact is not UI proof.
- Proving only that Cursor/Claude/Codex opened is not UI proof.
- Handler-level proof must not be reported as UI proof.

Required UI path evidence:
- screenshot, video, or screen capture of the actual host UI interaction;
- human-readable description of what the user did and what appeared;
- correlation between the UI result and automated Path A result;
- mismatch handling that marks the scenario not proven and creates the next
  smallest action instead of a readiness claim.

Status/reporting rule: every phase or step update must say which scenario is
being repeated through UI and how many paths are complete: zero of two,
automated only, UI only, or both joined. Use `Dual-Path Same-Scenario
Coverage`, not `product-ready count = 0 because full host UI not proven` or
equivalent old full-live-host wording. If Path B cannot complete, record the
exact local blocker, the next scenario/product fix, and the next bounded action.

Stop-loophole UI gate (2026-06-28): the old single chain `user in host UI ->
extension -> core -> answer in host UI` is optional bonus/live evidence only,
not the readiness definition. Required acceptance is same scenario Path A plus
Path B. A failed live host proof, impossible UI surface, missing screenshot, or
blocked auth/session/route is not a final stop; it creates the next bounded
sub-agent work order for auth recovery, scenario prep, host route repair,
tooling mitigation, or claim-scope downgrade unless a true unsafe or
missing-authority blocker is proven.

## Host Session Auth Preflight and Browser Login Recovery

Before any real host UI proof for Cursor, Claude, Codex, or another signed-in
host, the scenario must prove that the host is using the expected signed-in user
session. This preflight runs before the Dual-Path Same-Scenario UI Proof. If
Cursor opens anonymously, the host is logged out, the wrong account is visible,
or the session was lost, the UI scenario is not valid yet.

Required next action when auth is not ready: browser login recovery through the
host's normal sign-in flow. The agent must never capture or store passwords,
tokens, cookies, or secrets. If the browser requires human credential entry, the
agent records human-assisted login as required and resumes only after the user
completes it. The evidence packet must state: no secrets captured.

Auth preflight evidence requirements:
- screenshot or visible proof of signed-in/not-signed-in state, redacted if
  needed;
- preflight status: `SIGNED_IN_READY`, `LOGIN_RECOVERY_REQUIRED`,
  `HUMAN_LOGIN_REQUIRED`, or `AUTH_BLOCKED_WITH_EVIDENCE`;
- correlation from the auth preflight to the following same scenario twice run;
- human-readable status saying whether Cursor is signed in, anonymous, or needs
  browser login recovery before the scenario can count.

Only after signed-in state is visible and verified may the automated path and
real host UI path run and be joined.

## Multi-Monitor Target Window Control Protocol

Required literals:
- Multi-Monitor Target Window Control Protocol
- global foreground alone is not enough
- Claude on another monitor is not itself a blocker
- do not send scenario keystrokes unless target-window input focus is proven
- target-window input focus is proven
- TARGET_WINDOW_CONTROL_READY
- TARGET_WINDOW_CONTROL_PROTOCOL_MISSING
- TARGET_WINDOW_INPUT_NOT_VERIFIED
- TARGET_WINDOW_CONTROL_BLOCKED_WITH_EVIDENCE
- HOST_FOREGROUND_NOT_VERIFIED
- do not bypass by moving UI proof to the end

Before a host UI scenario sends keyboard or mouse input to Cursor, Claude, Codex,
or another desktop host, the scenario must prove that the intended target host
window can receive input. On multi-monitor desktops, another app being
foreground or on top on another screen does not automatically mean the target
host is unusable. global foreground alone is not enough. Claude on another
monitor is not itself a blocker.

Required target-window protocol:
- identify the intended host window by title, PID, handle if available, window
  bounds, and monitor bounds;
- record `monitor_count` and capture all-monitor overview or target-window crop
  if safe;
- do not use only global foreground window as proof that Cursor cannot be
  controlled;
- activate the target window by bounded methods: click title/content area inside
  target bounds, Win32/UI Automation activation by handle if available, then a
  work-order-approved host-control method;
- verify input landed in the target app by observing a Cursor-specific UI
  reaction, such as Command Palette opening in Cursor;
- do not type scenario content, account searches, sign-in text, command-palette
  text, or prompt text until target-window input focus is proven;
- continue only when status is `TARGET_WINDOW_CONTROL_READY`;
- record `TARGET_WINDOW_CONTROL_PROTOCOL_MISSING` when a scenario or work order
  lacks this protocol;
- record `TARGET_WINDOW_INPUT_NOT_VERIFIED` when the target window is found but
  the input focus probe does not produce a target-app UI reaction;
- record `TARGET_WINDOW_CONTROL_BLOCKED_WITH_EVIDENCE` when target-window
  activation/control fails after evidence is collected;
- record `HOST_FOREGROUND_NOT_VERIFIED` only if target-window activation/input
  verification fails after this multi-monitor protocol.

Required evidence fields:
- `monitor_count`;
- `target_window_title`;
- `target_pid`;
- `target_window_handle_if_available`;
- `target_window_bounds`;
- `target_monitor_id_or_bounds`;
- `activation_method_used`;
- `input_focus_probe`;
- `cursor_ui_reaction_observed`;
- `status`.

Anti-loopholes:
- do not bypass by moving UI proof to the end;
- do not replace real UI control with JSON/log screenshot;
- do not require human foregrounding before trying target-window/multi-monitor
  control;
- do not claim auth/session result until target-window input focus is proven.

## No-Blind-Spot UI Scenario Script

Before any host UI scenario is executed, the work order must contain a
No-Blind-Spot UI Scenario Script. "Run the extension scenario" is not a valid
scenario. agent judgment is not a scenario step.

Each UI scenario script must include:
- scenario id and purpose in human language;
- required skills/guides to load, including qa/testing skills;
- exact app launch and preflight steps;
- Multi-Monitor Target Window Control Protocol before any host keyboard or mouse input;
- exact auth/session steps, including browser login recovery branches;
- exact clicks, typing, menu commands, and keyboard commands where known;
- exact clicks/typing/expected UI state;
- exact input prompt/task;
- exact expected UI text or visual state;
- exact screenshot/video moments;
- exact comparison against automated Path A;
- exact pass/fail/blocked statuses;
- exact next smallest action for each blocked branch;
- forbidden substitutions: JSON/log/marker screenshot, handler-only proof, and
  debug file display.

If exact selectors, labels, or commands are unknown, discovery must be written
as an explicit scenario step with expected evidence. It cannot be left as
free-form agent judgment.

## Scenario Prep Review Must Pass Before Execution

Scenario prep NEEDS_FIX blocks executor work order. A host UI/runtime executor
may not receive a run command while the same scenario prep review is `NEEDS_FIX`.
plan not closed/readable/exact is repair loop, not execution and not stop.

Before a UI/runtime scenario can be executed, the prep packet must be
human-readable Russian scenario prep and must include:
- exact phase/step meaning;
- app/window target;
- login/session preflight;
- exact clicks/typing/expected visible state;
- screenshots or video moments to capture;
- fallback if two monitors/windows overlap or target focus is unclear;
- no secrets captured;
- exact evidence filenames.

Automated Path A prep must include exact commands or exact callable script
entrypoints, expected output, expected failure/blocker wording, and exact
evidence filenames. Vague commands such as "test host UI", "run scenario",
"run UI", or "continue scenario" are invalid work orders until rewritten as
concrete steps and independently reviewed as PASS.

UI/runtime scenario work orders must not use huge inline shell scripts. If a
PowerShell/Node command would exceed 20 nonblank lines, 4000 characters, or
include more than one function/class, prep must create a scoped `script-file
runner` in the evidence/run folder and execute that file by path. The prep must
name the runner file, expected stdout/stderr files, exit-code handling, and
exact evidence filenames.

Windows error 206, oversized inline shell command, or tool-command construction
failure is not a host/UI product blocker. After such a timeout/failure, the next
step is preflight-only script-file prep/execution, not another huge inline
command retry. Node REPL use must avoid `Node REPL top-level redeclaration` by
using `var`, scoped blocks, or fresh names; a declaration collision is a
work-order/tooling failure, not product evidence.

Required scenario prep literals: `Scenario prep NEEDS_FIX blocks executor work order`; `human-readable Russian scenario prep`; `exact clicks/typing/expected visible state`; `plan not closed/readable/exact is repair loop`.

## 50-Pass Scenario Clarity Review

Before execution, every No-Blind-Spot UI Scenario Script must pass 50-Pass
Scenario Clarity Review. The review may be 50 passes or a ledger of 50 explicit
review checks. Each pass/check asks whether an agent could still make a
judgment call not described by the scenario. Any gap requires fixing the
scenario before execution. Completion requires a human-readable 50-pass ledger,
not just a statement that the scenario was reviewed.

## Plan-Not-Ready Is Repair Loop, Not Stop

plan not ready is not a stop. If a UI scenario script, auth preflight, dual-path
definition, or 50-pass clarity ledger is missing, do not execute an unready plan
and do not stop as a final answer. return to architecture/planning repair loop,
load the required skills/context, repair the exact active scenario or plan
through bounded subagents when required, rerun the clarity/review gate, and
resume the nearest authorized execution only after plan-ready criteria pass.

## UI/dashboard final completion lock

When Luba asked for e2e or QA automation on a UI/dashboard task, final
completion is blocked while full-interactive UI automation is open.

Final completion requires the actual UI surface to be driven and observed:

```
□ The running WPF window/control path is invoked, not only the view model.
□ The automation selects an API, loads an example, enters or confirms text,
  runs the call, and observes result/warnings/raw JSON/copy or export behavior.
□ Negative UI cases are observed through the UI surface.
□ The report contains no open full-interactive UI automation gap.
```

Allowed intermediate fallback:
- view-model command-path tests;
- handler-level tests;
- component tests;
- WPF build;
- static QA;
- test-only adapter that calls the same production handler/core boundary.

Fallback evidence must be labelled with its exact scope. If full UI automation
is not feasible in the slice, add the narrow intermediate artifact, move
product-decision or runtime-environment-only work to the end queue, keep the
goal open, and continue any independent plan-derived work. Do not stop merely
because the real UI harness is unavailable, and do not claim full UI completion.

## UX automation standard

When Luba asks for UI/UX, dashboard, API lab, or human-testable UI quality,
final completion requires automated UX evidence in addition to click-path
evidence.

Required UX assertions:

```
□ Primary controls have human-readable labels, not only method ids or x:Name.
□ Primary controls expose AutomationProperties.Name and HelpText.
□ Buttons/toggles with non-obvious meaning have tooltips or visible help.
□ Examples explain what the user should type and what output to expect.
□ Empty/error states show a human-readable message in the UI.
□ Raw JSON and debug controls explain safety and evidence boundaries.
□ History/copy/export controls explain what is copied/exported and what is not.
□ Layout has stable minimum dimensions or bounds checks for the primary panels.
□ Primary path does not rely on internal jargon such as raw status keys.
```

Accepted UX automation evidence:
- a WPF/desktop/browser automation test that drives the real UI surface and
  asserts accessible names/help text, visible copy, examples, error wording,
  safety wording, and layout/bounds;
- a QA script that fails if the UX automation test or UX metadata disappears.

Rejected UX substitution:
- a click-path test that never checks what a user reads;
- a catalog metadata test with no running UI assertion;
- screenshots or docs with no repeatable automated check;
- a QA script that only checks file presence.

## Dashboard scenario ledger for new API behavior

When a plan adds or changes dashboard/API behavior, the QA automation must add
named scenarios for that new behavior. Reusing only old dashboard smoke tests is
not enough.

Each new dashboard scenario must record:

```text
scenario_id:
human_api_name:
technical_method_id:
example_input:
expected_visible_result:
expected_raw_or_debug_fields:
negative_case:
evidence_scope:
```

Required assertions:
- the API is shown with a human-readable name;
- the example says what the user can type and what result to expect;
- the run path uses the dashboard/API handler, not a disconnected fixture;
- the result, warning, error, raw JSON, and debug trace are readable;
- at least one negative/untrained/missing-state case is visible and honest;
- `evidence_scope` explicitly says whether the scenario proves dashboard/API,
  model quality, or host runtime. For dashboard-only tests it must say
  `dashboard_api_only_not_model_quality_not_host_runtime`.

---

## Form layout standard

All three Desktop stage explorers use the same layout. New forms must match it exactly.

```csharp
// Standard Desktop form layout
Text          = "Sub-project <N> — <Title> Stage Explorer";
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

// Left panel: list + run button
_stageList.Dock           = DockStyle.Fill;
_stageList.IntegralHeight = false;
_runButton.Text   = "Run selected stage";
_runButton.Dock   = DockStyle.Bottom;
_runButton.Height = 36;

// Right panel: monospaced output box
_outputBox.Dock       = DockStyle.Fill;
_outputBox.Multiline  = true;
_outputBox.ReadOnly   = true;
_outputBox.ScrollBars = ScrollBars.Both;
_outputBox.WordWrap   = false;
_outputBox.Font       = new System.Drawing.Font("Consolas", 9.5f);

// Status strip
_status.Items.Add(_statusLabel);
Controls.Add(split);
Controls.Add(_status);
```

Color scheme: do not set BackColor or ForeColor — use SystemColors defaults.
Fonts: Consolas 9.5f for output boxes only. System default for list and buttons.

---

## Program.cs standard

All Desktop projects use the same Program.cs entry point:

```csharp
using System.Windows.Forms;

namespace <Namespace>.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();  // DPI + visual styles
        Application.Run(new MainForm());
    }
}
```

`ApplicationConfiguration.Initialize()` is required — it enables DPI awareness and visual styles. Never replace it with manual calls to `Application.EnableVisualStyles()`.

---

## Demo stub pattern

When a MainForm demo needs to isolate from disk I/O or real LLM calls, it creates
a private inner class that implements the interface using in-memory data.

Rules for demo stubs:
- Defined as `private sealed class` at the bottom of MainForm.cs
- Implement the interface exactly (no extra public members)
- Named `InMemory<Type>` for reader/writer stubs, `Mock<Type>` for behaviour stubs
- Never used outside the Demo form that defines them

```csharp
// Example: IAgentMessageBus stub for orchestrator demo
private sealed class MockAgentMessageBus : IAgentMessageBus
{
    public List<(string Agent, string Task)> Calls { get; } = [];

    public Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task)
    {
        Calls.Add((subAgentName, task));
        return Task.FromResult(OperationResult<string>.Ok($"[{subAgentName} done: {task}]"));
    }
}
```

---

## Manual verification checklist

Run this checklist after any change to a MainForm.cs file:

```
Before running the form:
☐ dotnet build LlmFromScratch.sln — 0 errors, 0 warnings
☐ dotnet test Tests/LlmFromScratch.Tests.csproj — 0 failures

Running the form manually:
☐ Launch: dotnet run --project <N>.<ProjectName>.Desktop/<ProjectName>.Desktop.csproj
☐ Form opens at correct size (1100×720), centred on screen
☐ Stage list is populated and the first item is pre-selected
☐ "Run selected stage" button is visible and enabled
☐ Click "Run selected stage" → status strip shows "Running <name>..."
☐ Output box fills with result text in Consolas 9.5f
☐ Status strip shows "Done." when complete
☐ Button re-enables after run
☐ Run every stage in the list once — no stage throws an unhandled exception
☐ Output for each stage contains meaningful non-empty text

After verifying:
☐ dotnet test — still 0 failures
```

---

## What replaces @testing-library queries

| @testing-library concept | This project equivalent |
|--------------------------|------------------------|
| `getByRole('button')` | `_runButton.Enabled` (checked after click) |
| `getByRole('listbox')` | `_stageList.SelectedIndex >= 0` |
| `getByRole('textbox')` | `_outputBox.Text.Length > 0` after run |
| `getByText('Done')` | `_statusLabel.Text == "Done."` |
| `fireEvent.click(button)` | `_runButton.PerformClick()` (in integration test) |
| `waitFor(() => expect(...))` | `await RunSelectedAsync()` in test (async completion) |
| `render(<Component />)` | `new MainForm()` — form construction as unit of test |

---

## xUnit form test pattern

For library-level verification of form construction (does not require visual display):

```csharp
// Tests that the form constructs without throwing
[Fact]
public void MainForm_Constructs_WithSevenStages()
{
    // The form is constructed on the test thread; no visual display needed
    using var form = new AgentInfrastructure.Desktop.MainForm();

    // Controls are initialised during the constructor
    // Only verify counts and properties — no visual checks
    Assert.Equal(7, form.StageCount);  // requires a public property
}
```

Note: Windows Forms controls require a single-threaded apartment (STA) thread.
Add `[STAThread]` to a test if any control raises a cross-thread exception.

---

## What to test in xUnit vs manually

| What changed | Test in xUnit | Test manually |
|-------------|--------------|---------------|
| Business logic method in library | ✅ Always — [Fact] in Tests/ | — |
| OperationResult chain in service | ✅ Always | — |
| Form layout (width, font, panel sizes) | — | ✅ Visual check |
| Demo stage produces non-empty output | — | ✅ Click "Run" in explorer |
| Status strip shows correct message | — | ✅ Watch during demo run |
| Error display on unexpected exception | — | ✅ Force an error and check output |

---

## Anti-patterns

### Anti-pattern 1: Business logic in MainForm
```
✗ private async Task<string> RunFeedbackRouterDemoAsync()
  {
      var input = _inputBox.Text;             // reading from control
      var result = new FeedbackRouter(...)    // new concrete — Rule 2 violation
          .Route(ParseFeedback(input));       // parsing logic in the form
      return result.Value!.Summary;
  }

✓ Business logic stays in the library class
  Form only formats and displays the pre-built library result
```

### Anti-pattern 2: Demo stub escapes MainForm.cs
```
✗ internal sealed class MockAgentMessageBus : IAgentMessageBus { ... }
  (defined at file scope, reused in test classes)

✓ private sealed class MockAgentMessageBus  (inside MainForm, unreachable outside)
  Test classes use their own stubs defined in the Tests/ project
```

### Anti-pattern 3: Non-standard layout
```
✗ Using a custom panel width of 250 instead of 380
  Using a different font than Consolas 9.5f
  Adding a second status strip

✓ Follow the layout standard exactly for every Desktop stage explorer
  All three explorers look identical — same chrome, same font, same layout
```
