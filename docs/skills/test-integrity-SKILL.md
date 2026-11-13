# test-integrity-SKILL.md — xUnit test correctness guard

## Учебный тест и реальная приёмка — правило Любы от 3 ноября 2026

Для аудита/ремонта учебного теста и переноса обязателен [разбор причины и нативная приёмка](../plans/replanning-possible-diffs/experimets/45-learning-claim-and-native-acceptance-contract-2026-11-03.md). Архитектор не исправляет отсутствие нужных уроков ручным семантическим исключением в продукте и не объявляет опасный наблюдённый ответ несуществующим. Он отделяет дефект постановки от дефекта контракта, назначает отдельный опыт с TRAIN/EXAM и сохраняет исходное наблюдение.

При назначенном переносе, аудите кода/QA, приёмке или ремонте тестов исполнитель обязан **действительно собрать затронутые проекты и тестовый проект, запустить проверки на финальном исходнике и проверить discovery**. Обоснованная адаптация тестов к уже утверждённому договору входит в это поручение; отдельное одобрение каждой правки assertion не требуется. Нельзя подгонять expected под дефект продукта. UNRUN не закрывает приёмку; при отказе передать архитектору команду, версию, журнал, expected/actual и следующий шаг. Слово «аудит» не включает READ_ONLY_RESEARCH автоматически. Прямое ограничение «только поиск/документы, не запускать» действует только в своей задаче. [Навыки по этапам, свежие результаты и примеры46](../plans/replanning-possible-diffs/experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md) обязательны вместе с45; не использовать старую DLL/TRX и не завершать поручение вместо ожидания.

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

## Fresh Execution / Method / Humility Test Hook

Test integrity must not accept stale or method-skipped evidence:
- `REPEATED_CURRENT_COMMAND_IS_FRESH_EXECUTION_GATE`: repeated current command is a fresh execution trigger; old artifacts are baseline evidence, not completion.
- `TOO_FAST_HIGH_PASS_SUSPICION_GATE`: too-fast 50-pass completion is suspicious until per-pass sub-agent evidence is accepted.
- `LARGE_VOLUME_MODEL_HUMILITY_AND_FOCUS_GATE`: large-volume model humility requires bounded slices and focus ledger before completion.
- `EXPLICIT_METHOD_OVERRIDE_GATE`: Luba's method/model/reasoning/count/folders/target/sub-agent route is binding; old tests or package checks cannot substitute for the requested method.
- `FINAL_ANSWER_FRESHNESS_GATE`: final/status must answer the latest command and state freshly executed this turn vs only verified.
- `CURRENT_REPEATED_PACKAGE_COMMAND_GOVERNANCE_ADDENDUM`: test integrity must reject repeated extension-development package closure by old artifact verification instead of a fresh execution chain; too-fast N-pass/status-package claims without ordered packets/records; missing large-volume model humility focus ledger with exact current task, not-the-task, files, pass/review counts, active worker, remaining actions, and latest-command freshness; parallel workers when Luba flags parallel/too-fast or requires exact-count sub-agent work unless her current unquoted message authorizes parallelism; interrupted or zero-work spawned-worker packets using `NONE` instead of `subagent_id: UNKNOWN_IN_SUBAGENT` when platform id is hidden; parent local sidecar work during waiting; missing human-readable phase counts before execution continuation; and final/status without latest-command, fresh-vs-old execution, actual packets/records, no parent self-review counted, and no product-readiness overclaim.
- `COMMAND_SCOPE_EXACTNESS_GATE`: test integrity must reject current-task command violations. Current `allowed_commands` and `forbidden_commands` are binding; read-only convenience commands, alternate git diff modes, external AI/web, `codex.exe`, nested sub-agents, branch/git mutations, or shell variants outside the current allowed list are violations unless Luba's current unquoted instruction explicitly authorizes them. Evidence from an out-of-scope command is quarantine evidence only and cannot support completion.

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
## v1.0: translated from xiigen test-integrity (NestJS→C#/xUnit)

## When to load
Load when a test fails after a guard condition is changed.
Load when a stub is modified (not just created new) to make a test pass.
Load when a phase ends with "all tests pass" and at least one test was modified.
Load when test count increases by more than the number of new behaviours introduced.

---

## Core principle

A passing test confirms an assertion holds. It does not confirm that the
code path you care about actually executed.

Tests are allowed as fallback evidence when the real UI, host, or runtime path
is not available in the current slice. That fallback must be labelled with its
exact scope. A view-model test, handler-level test, static QA script, or
contract test must not hide a missing real project, missing host entrypoint,
missing WPF interaction, or missing trained runtime evidence.

These two things come apart whenever:
- A stub is changed to skip an OperationResult check (the guard is never reached)
- Two conditions are stacked, making a downstream branch unreachable
- A test adds assertions but the stubs bypass the real interface contract
- An IsSuccess guard exists in one service but is missing in its sibling

When they come apart, test count goes up and everything looks green — but a
code path is silently unguarded.

---

## The four failure modes

| Mode | Trigger | Symptom | Rule |
|------|---------|---------|------|
| FM-1: Stub-Fix Masking | Stub changed to skip IsSuccess check | Test passes, guard never runs | test-fix-or-code-fix |
| FM-2: Guard Stacking | Two conditions check the same input | Downstream branch unreachable | branch-reachability |
| FM-3: Coverage Inflation | Tests added but stubs bypass real contracts | Test count up, real path untested | coverage-vs-execution |
| FM-4: Sibling Guard Gap | Guard in ProjectIndexer, missing in IncrementalTrainer | Same input, inconsistent protection | sibling-guard-parity |
| FM-5: Test-Only Boundary Substitute | Test project links production files with `Compile Include` or source-link instead of referencing the real project | Tests pass, but requested project boundary is missing | project-boundary-evidence |
| FM-6: UI Automation Scope Inflation | View-model, handler, component, WPF build, static QA, script, or test-only adapter is reported as full UI e2e | Completion claim says UI is automated while the real window/control path never ran | ui-evidence-scope |
| FM-7: UX Automation Scope Inflation | Click-path UI automation is reported as UI/UX automation without checking human labels, accessible names/help text, examples, error clarity, raw/debug safety wording, layout/bounds, or primary-path jargon | The UI can be clicked but may still be confusing or inaccessible | ux-evidence-scope |
| FM-8: Pseudo-Trainable Pattern Passes Tests | Tests pass a scorer/selector/classifier/router/KNN/dictionary/pattern matcher and report semantic learning without proving it is inside an allowed `ILearningMachine`/`ILogical*` level with lifecycle gates | Deterministic or counter baselines can be accepted as trained semantic units | trainable-architecture-evidence |
| FM-9: Sub-Agent Firewall Test Substitute | Textual checks prove phrases exist somewhere, but do not prove parent avoided local sidecar work or that exact target files contain the firewall | Governance repair can be reported complete while the parent still used waiting time for substantive local work | subagent-firewall-evidence |
| FM-10: Full Extension Evidence Substitute | JSON, PowerShell, manifest, static validator, helper-call, dashboard-to-core, or handler-only evidence is reported as a full Claude/Cursor/Codex extension | Completion claims full host runtime without real host entrypoint, settings/debug, core API path, prompt/task/orchestration/skills path, adaptive learning, lifecycle, and QA evidence | full-extension-evidence |
| FM-11: Model Downgrade Evidence Substitute | A review, execution, or completion packet is counted even though concrete runtime/request metadata shows medium/high/low/default/lower-tier/unspecified settings without explicit current Luba approval | Lower-quality review evidence is accepted as if it had `requested_model=gpt-5.5`, `requested_reasoning_effort=xhigh`, and valid runtime metadata or `mode_verified_from_work_order=true` | model-reasoning-evidence |
| FM-12: Failed Verification Masked By Commit | `git diff --check`, tests, JSON/JSONL parse, schema checks, builds, or another required verification prints failure output or exits nonzero, then a wrapper still stages/commits/pushes | Commit success hides a red verification gate | verification-abort-boundary |
| FM-13: Glob Commit Group Miss | Commit staging uses a broad `glob` instead of exact `required_commit_group` paths from the reviewer/validation ledger | A required `validation artifact`, especially a dot/dash naming variant, is absent from `git diff --cached --name-only` and needs a follow-up commit | commit-group-staging-evidence |
| FM-14: Machine Log Visibility Substitute | QSG/STATE/JSON/log output is used as the user-facing status for long execution or 10-review admission | Luba cannot see the plan, stage, 10 records, accepted evidence, open gaps, or next action even though machine artifacts exist | human-readable-visibility-evidence |
| FM-15: Code Review Scope Substitute | A code slice is accepted from tests or QSG labels without an exact review scope over project standards, `llm_mvp_core` core boundary, thin host adapters, hidden deterministic shortcuts, and docs/QA/dashboard impact | A deterministic helper or host-side algorithm can pass as semantic/trainable product logic | code-review-completeness-evidence |
| FM-16: Dual-Path UI Proof Substitute | Visual evidence is satisfied by opening JSON/log/result files, showing a marker, screenshotting a debug artifact, proving only the host opened, or reporting handler-level proof as UI proof | The same scenario was not executed twice through the automated path and real host UI path | dual-path-same-scenario-ui-proof |
| FM-17: Host Session Auth Substitute | A host UI scenario runs while Cursor opens anonymously, the host is signed out, the wrong account is visible, or session state is unknown | Anonymous or wrong-session UI evidence is counted as host proof | host-session-auth-preflight |
| FM-18: Blind-Spot Scenario Substitute | A UI work order says to run an extension scenario without exact actions, expected UI state, evidence moments, qa/testing skills, branch handling, and clarity review | The executor must invent missing steps | no-blind-spot-ui-scenario-script |
| FM-19: Target Window Control Substitute | A UI scenario sends keyboard/mouse input without proving target-window input focus, treats global foreground alone as proof Cursor cannot be controlled, or treats failed typing/search as auth/session evidence without target-window verification | Multi-monitor Cursor control can be falsely blocked or keystrokes can be misclassified while Claude is visible on another monitor | multi-monitor-target-window-control-protocol |
| FM-20: Evidence Text Format Drift | Evidence JSON/MD/TXT/JSONL contains UTF-8 BOM, CRLF, trailing whitespace, or unparsed JSON, and parent cleanup fixes it later | Generated evidence is not commit-ready; cleanup hides a worker defect | evidence-text-format-integrity |
| FM-21: Unbounded Executor Scope Substitute | One executor/sub-agent receives or continues under "act until product ready", "continue until the whole plan is done", "do everything", "no stops until ready", or equivalent broad scope | Deprecated legacy product-readiness wording is treated as one worker's scope instead of using bounded dual-path scenario coverage packets | unbounded-executor-quarantine |
| FM-22: Code Implementation Claim Substitute | A task is claimed implemented from a plan, package, ledger, manifest, screenshot, helper, fixture, static check, or self-report without an implementation-location map and production call-chain proof | Review accepts artifacts instead of verifying that real code implements the user task | code-implementation-claim-evidence |

---

## Quick routing table

| What just happened | Apply first | Then |
|-------------------|-------------|------|
| Test fails after guard tightened | test-fix-or-code-fix | branch-reachability |
| Stub changed to skip validation | test-fix-or-code-fix | coverage-vs-execution |
| Two guards check same input path | branch-reachability | coverage-vs-execution |
| Test count up but stubs bypass interface | coverage-vs-execution | branch-reachability |
| New project/dashboard/API lab tests pass through linked source files | project-boundary-evidence | coverage-vs-execution |
| UI/dashboard e2e or QA automation was requested, but only view-model/handler/static evidence exists | ui-evidence-scope | coverage-vs-execution |
| UI/UX automation was requested, but tests only click controls or assert internal state | ux-evidence-scope | coverage-vs-execution |
| Learned/semantic model tests pass, but the runtime authority is a scorer/selector/KNN/dictionary/pattern matcher | trainable-architecture-evidence | coverage-vs-execution |
| Sub-agent-only firewall repair claims done from broad grep or parent self-review | subagent-firewall-evidence | coverage-vs-execution |
| Claude/Cursor/Codex extension claim has only JSON/PowerShell/static/helper evidence | full-extension-evidence | coverage-vs-execution |
| Sub-agent plan/execution review packet lacks `requested_model=gpt-5.5`, `requested_reasoning_effort=xhigh`, and runtime metadata or `mode_verified_from_work_order=true` evidence | model-reasoning-evidence | coverage-vs-execution |
| Required verification failed before commit/push, but a later commit/push succeeded | verification-abort-boundary | coverage-vs-execution |
| Commit group missed a required validation artifact because staging relied on a broad glob | commit-group-staging-evidence | verification-abort-boundary |
| Luba cannot see the plan, 10 review records, current stage, or human-readable progress board | human-readable-visibility-evidence | coverage-vs-execution |
| Code slice accepted without exact standards/core-boundary/thin-adapter/determinism/docs review scope | code-review-completeness-evidence | coverage-vs-execution |
| Host visual evidence has only JSON/log/marker screenshot, host-open screenshot, debug artifact screenshot, or handler-level proof | dual-path-same-scenario-ui-proof | full-extension-evidence |
| Cursor opens anonymously, is logged out, has wrong account state, or needs browser login recovery before UI proof | host-session-auth-preflight | dual-path-same-scenario-ui-proof |
| Code/product/runtime/model claim lacks implementation location, file:line refs, production call chain, changed code files, tests/runtime commands, or independent verifier work order | code-implementation-claim-evidence | code-review-completeness-evidence |
| Cursor/Claude/Codex target-window input focus is unknown before keyboard/mouse input | multi-monitor-target-window-control-protocol | no-blind-spot-ui-scenario-script |
| UI scenario leaves exact clicks/typing/expected UI state or qa/testing skills to the agent | no-blind-spot-ui-scenario-script | host-session-auth-preflight |
| Evidence files need parent format-only repair for BOM, CRLF, trailing whitespace, or JSON parse | evidence-text-format-integrity | verification-abort-boundary |
| Executor/sub-agent was launched with an "until product ready / until plan complete / do everything" scope | unbounded-executor-quarantine | human-readable-visibility-evidence |
| Sub-agent has run more than 10 minutes without a protocol/evidence packet | unbounded-executor-quarantine | subagent-firewall-evidence |
| Guard in one service, missing in sibling | branch-reachability (sibling audit) | — |
| "All tests pass" but a guard interaction exists | branch-reachability | coverage-vs-execution |
| Test uses hand-crafted fake data | contract-driven-testing | — |

---

## Rule: test-fix-or-code-fix

Before changing any stub or assertion to make a test pass, answer:

```
Diagnosis question: WHY is this path failing?

Option A: The code is wrong — the stub is strict and correctly rejecting bad output.
  → Fix the production code. Keep the stub strict.

Option B: The stub is too strict for a valid code path.
  → Update the stub to match the real interface contract. Do not weaken it.

Option C: The assertion tests the wrong thing.
  → Correct it against an independently approved contract within the assigned
    repair; record old/new expectations and preserve a discriminating control.
    A change to the product requirement itself needs its own decision.
```

```
Anti-pattern:
  Test fails because OperationResult.IsSuccess=false
  → Stub changed to always return IsSuccess=true
  → Test passes
  → IsSuccess guard in production code now unreachable

Correct:
  Diagnose WHY IsSuccess=false is returned
  Fix the production code to return IsSuccess=true on the happy path
  Keep the stub strict — it should still return IsSuccess=false for the error path
```

---

## Rule: branch-reachability

After adding or changing a guard condition, verify that every branch has
at least one test that reaches it.

```csharp
// Example: stacked guards make the second branch unreachable
if (!input.IsSuccess) return Fail("no input");        // guard 1
if (!input.Value!.Domain.HasValue) return Fail("no domain"); // guard 2 — can this be reached?

// Checklist:
// ☐ Is there a test that provides a valid input.IsSuccess=true but no Domain?
// ☐ If guard 1 rejects everything that reaches guard 2, guard 2 is dead code
```

Sibling audit: after adding a guard to any service, check every sibling
service that processes the same input type:

```
Subproject sibling pairs:
  ProjectIndexer ↔ DomainDatasetCache      (both process TrainingExample inputs)
  IncrementalTrainer ↔ DomainModelRegistry (both consume domain strings)
  FeedbackRouter ACCEPTED ↔ REJECTED paths (both use FeedbackSignal routing)
```

---

## Rule: coverage-vs-execution

Count tests that execute real code paths, not tests that merely pass.

```
Checklist:
☐ Does the test exercise the real ICodeReader.ListFilesAsync call, or a stub that returns []?
☐ Does the test exercise the real ISemanticCompiler29.Compile call, or a stub with fixed domain?
☐ Is there at least one test per service that uses a non-trivial stub (returns real-shaped data)?
☐ Does the test assert on the specific output field the behaviour change affects?
☐ After FM-3 is suspected: count tests that set up real-shaped input vs tests that use empty stubs
```

---

## Rule: contract-driven-testing

Use real domain types as test inputs — not synthetic hand-crafted dictionaries.

```csharp
// Wrong — synthetic fixture
var example = new TrainingExample("", "", "", 0, DateTimeOffset.MinValue);

// Correct — real-shaped fixture matching actual domain contract
var example = new TrainingExample(
    "fix null reference",            // real prompt
    "if (x is null) return;",        // real response
    "BUG",                           // real domain from D-04
    1.0,                             // real weight (D-09 formula baseline)
    DateTimeOffset.UtcNow);
```

---

## Rule: project-boundary-evidence

For a new project, dashboard, API lab, plugin surface, or host integration,
tests must exercise the artifact boundary requested by the work order.

Checklist:

```
☐ The implementation project file exists in the requested folder
☐ The test project uses a real ProjectReference, package reference, or approved runtime boundary
☐ The test project does not use Compile Include / Link to pull production files as the main path
☐ A project build or runtime command proves the real project boundary compiles or loads
☐ The report states whether this is unit, integration, view-model, full UI, host, or trained evidence
```

If the real host/UI path is not available, write that as an honest gap and keep
the fallback evidence narrow. Do not rename fallback evidence as end-to-end
runtime proof.

## Rule: ui-evidence-scope

When Luba or the plan asks for UI e2e or QA automation, classify UI evidence
before accepting a completion claim:

```text
full_interactive_ui:
  Drives and observes the running UI window/control/host entrypoint.

intermediate_ui:
  View-model, handler-level, component, static QA, WPF build, script parity, or
  test-only adapter evidence.
```

Intermediate evidence is useful, but it cannot close a final UI e2e row. If the
real UI automation path is unavailable, add the honest intermediate artifact,
record the remaining full UI gap, move product-decision/runtime-environment
work to an end queue when needed, and continue independent plan-derived work.

## Rule: ux-evidence-scope

When UI/UX automation is requested, classify UX evidence separately from UI
execution evidence:

```text
ux_automation:
  Drives the real UI surface and asserts human-readable labels, accessible
  names/help text, examples, visible errors, raw/debug safety wording,
  copy/export/history clarity, layout/bounds, and no internal jargon on the
  primary path.

click_only_ui:
  Selects, types, clicks, and observes values without checking whether a human
  can understand the surface.
```

Click-only UI evidence is useful, but it cannot close a UI/UX row. A test that
uses automation peers directly must still assert `AutomationProperties.Name`,
`AutomationProperties.HelpText`, visible help text/tooltips, and human wording
for the primary path.

## Rule: dual-path-same-scenario-ui-proof

This is the Dual-Path Same-Scenario UI Proof rule.

When visual evidence is required for Cursor, Claude, or Codex host behavior,
test integrity requires the same scenario twice:

```text
Path A:
  Automated pre-host/core/adapter execution.

Path B:
  Real local host UI execution in Cursor, Claude, or Codex.
```

The automated path and real host UI path must use the same user prompt/task,
same expected behavior, same selected skills/plan or response contract, and a
result comparison. The marker/correlation id is tracing only, not proof.

Hard fail substitutions:

```text
☐ JSON/log/marker screenshot is not UI proof
☐ Opening JSON/log/result files in the app is not UI proof
☐ Showing only a marker is not UI proof
☐ Screenshotting a debug artifact is not UI proof
☐ Proving only that Cursor/Claude/Codex opened is not UI proof
☐ Handler-level proof reported as UI proof is not UI proof
```

Required accepted evidence:

```text
☐ Screenshot/video/screen capture shows the actual host UI interaction
☐ Human-readable notes say what the user did and what appeared
☐ The UI result is correlated with the automated Path A result
☐ Mismatch or inability to run Path B is marked not proven
☐ The next smallest action is created instead of a readiness claim
☐ Phase/step status says which scenario is repeated through UI and whether
   zero of two, automated only, UI only, or both paths are complete
```

## Rule: host-session-auth-preflight

This is the Host Session Auth Preflight and Browser Login Recovery rule.

Before the real host UI path can count for Cursor, Claude, Codex, or another
signed-in host, test integrity requires visible proof of the expected signed-in
session. If Cursor opens anonymously, the host is logged out, the wrong account
is visible, or session state is unknown, the UI scenario is not valid yet.

Required recovery path:

```text
1. Record visible signed-in/not-signed-in evidence, redacted if needed.
2. If auth is not ready, run browser login recovery through the host's normal
   sign-in flow.
3. Never capture or store passwords, tokens, cookies, or secrets.
4. If the browser requires human credential entry, record HUMAN_LOGIN_REQUIRED
   and resume only after the user completes login.
5. Record no secrets captured.
6. Only then run the Dual-Path Same-Scenario UI Proof: same scenario twice,
   automated path and real host UI path.
```

Required accepted evidence:

```text
check: Screenshot or visible proof of signed-in/not-signed-in state exists
check: Preflight status is SIGNED_IN_READY, LOGIN_RECOVERY_REQUIRED,
       HUMAN_LOGIN_REQUIRED, or AUTH_BLOCKED_WITH_EVIDENCE
check: Auth preflight is correlated to the following UI scenario run
check: Report says whether Cursor is signed in, anonymous, or needs browser
       login recovery before the scenario can count
check: no secrets captured
```

## Rule: multi-monitor-target-window-control-protocol

This is the Multi-Monitor Target Window Control Protocol rule.

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

Before any host UI scenario sends keyboard or mouse input, test integrity
requires proof that the intended target host window can receive input. On
multi-monitor desktops, another app being foreground or on top on another screen
does not automatically mean the target host is unusable. global foreground alone
is not enough. Claude on another monitor is not itself a blocker.

Required recovery path:

```text
1. Identify the intended host window by title, PID, handle if available, bounds,
   and monitor bounds.
2. Record monitor_count and capture all-monitor overview or target-window crop
   if safe.
3. Do not use only global foreground window as proof that Cursor cannot be
   controlled.
4. Activate the target window by bounded methods: click title/content area inside
   target bounds, Win32/UI Automation activation by handle if available, then a
   work-order-approved host-control method.
5. Verify input landed in the target app by observing a Cursor-specific UI
   reaction, such as Command Palette opening in Cursor.
6. Continue only when status is TARGET_WINDOW_CONTROL_READY.
7. Use HOST_FOREGROUND_NOT_VERIFIED only after target-window activation/input
   verification fails under this protocol.
```

Required accepted evidence:

```text
check: monitor_count is recorded
check: target_window_title is recorded
check: target_pid is recorded
check: target_window_handle_if_available is recorded
check: target_window_bounds is recorded
check: target_monitor_id_or_bounds is recorded
check: activation_method_used is recorded
check: input_focus_probe is recorded
check: cursor_ui_reaction_observed is recorded
check: status is TARGET_WINDOW_CONTROL_READY before scenario input, or one of
       TARGET_WINDOW_CONTROL_PROTOCOL_MISSING, TARGET_WINDOW_INPUT_NOT_VERIFIED,
       TARGET_WINDOW_CONTROL_BLOCKED_WITH_EVIDENCE, or post-protocol
       HOST_FOREGROUND_NOT_VERIFIED
check: no auth/session/panel state is inferred until target-window input focus is proven
check: do not bypass by moving UI proof to the end
check: do not replace real UI control with JSON/log screenshot
check: do not require human foregrounding before trying target-window/multi-monitor control
```

## Rule: no-blind-spot-ui-scenario-script

This is the No-Blind-Spot UI Scenario Script rule. agent judgment is not a scenario step.

Before execution, every host UI scenario must be fully described. agent
judgment is not a scenario step. Test integrity rejects any scenario that lacks
the exact actions an executor should perform or the exact evidence that proves
the branch.

Required scenario fields:

```text
scenario_id_and_human_purpose:
required_skills_guides_including_qa/testing skills:
exact_app_launch_preflight_steps:
Multi-Monitor Target Window Control Protocol before host keyboard/mouse input:
exact_auth_session_steps_and_browser_login_recovery_branches:
exact clicks/typing/expected UI state:
exact_input_prompt_task:
exact_screenshot_video_moments:
exact_path_a_comparison:
exact_pass_fail_blocked_statuses:
exact_next_smallest_action_per_blocked_branch:
forbidden_substitutions: JSON/log/marker screenshot, handler-only proof, debug file display
```

If an exact selector or UI label is unknown, the scenario must include a
discovery step with required evidence and branch handling. It cannot say "agent
decides", "inspect as needed", or "run the extension scenario".

## Rule: 50-pass-scenario-clarity-review

This is the 50-Pass Scenario Clarity Review rule.

Before UI execution, the scenario must have 50 explicit review checks or 50
passes. Each pass asks whether an agent could still make a judgment call not
described by the scenario. Any gap requires scenario repair and re-review.
"Reviewed" without a human-readable 50-pass ledger is not evidence.

## Rule: plan-not-ready-repair-loop

This is the Plan-Not-Ready Is Repair Loop, Not Stop rule.

plan not ready is not a stop. When a scenario/plan/test gap is found, do not execute an unready plan and do not convert the finding into a final pause.
return to architecture/planning repair loop, load required skills/context,
repair the exact active scenario/plan/skills/guides through bounded subagents
when required, run the required clarity/review passes, and resume only after
plan-ready criteria pass.

## Rule: subagent-firewall-evidence

Textual tests for sub-agent-only governance repair must prove the exact
boundary, not just that useful words exist in the repository.

Checklist:

```text
☐ Exact target files were checked, not only broad `rg docs/skills`
☐ Each exact file contains the Parent Architect Subagent-Only Firewall
☐ Each exact file forbids parent local sidecar work while waiting
☐ Each exact file says read-only verification happens after packet return
☐ Each exact file disables the no local non-overlapping work escape
☐ Each exact file says sub-agent waiting is not a stop
☐ The packet names the sub-agent id and exact changed files
☐ The report states parent did not read/analyze governed files before packet return
☐ The report does not use broad `rg docs/skills` as completion evidence
```

Passing textual checks do not prove the parent respected the boundary unless
the authorship and timing rows are present. If those rows are missing, the
correct result is a governance/test-integrity failure, not a completed repair.

Parent Architect Subagent-Only Firewall compatibility: tests and textual checks
must prove no parent local sidecar work happened, read-only verification was
after a sub-agent packet returns, the no local non-overlapping work escape was
disabled, and sub-agent waiting is not a stop. Any final/stop before the
sub-agent-required deliverable is complete is a test-integrity failure unless
GOLD GATE explicitly asked for answer-only or stop.

## Rule: full-extension-evidence

For Claude, Cursor, and Codex, tests and QA evidence must match the claimed
extension level.

Checklist:

```text
☐ Real host entrypoint evidence exists for each host
☐ Settings/debug surface is exercised or explicitly scoped as handler-only
☐ Target-locator/core API path into llm_mvp_core is proven
☐ Prompt-to-task, task-to-orchestration, and task/plan-to-skills paths are covered
☐ Local adaptive learning path is covered
☐ Export/import/backup/continue-training evidence exists
☐ QA is live-host evidence or explicitly labelled handler evidence
☐ JSON/PowerShell/manifest/static/helper/dashboard evidence is not renamed as full extension runtime
```

If any row is missing, the correct status is not full extension complete. It is
`IN_PROGRESS_NEXT_STEP_KNOWN` with the first missing host/runtime evidence row.

## Rule: model-reasoning-evidence

Plan review, execution review, code review, and governance repair packets count
only when they include required model/reasoning evidence:

```text
requested_model = gpt-5.5
requested_reasoning_effort = xhigh
runtime_model_or_unavailable:
runtime_reasoning_effort_or_unavailable:
mode_verified_from_work_order:
downgrade_authority = explicit current Luba quote only
downgrade_warning_or_NONE:
state_log_record_ref:
```

Medium/high/low/default/lower-tier/unspecified packets without a current Luba
downgrade quote are governance violations. They must emit a user-visible
warning, record state/log evidence, reload the Model/Reasoning Configuration
Gate, and cannot count toward the 10 Review Cycle Gate or Final Execution
Review Gate.

Generic system/developer identity text such as "GPT-5" is not downgrade
evidence by itself. If exact runtime metadata is unavailable but the work order
explicitly requests `gpt-5.5` / `xhigh`, the packet can be evaluated with
`mode_verified_from_work_order=true` unless a concrete contradictory
runtime/request value is visible. A false downgrade stop based only on generic
identity text is a premature stop loophole.

## Rule: commit-group-staging-evidence

Before accepting a commit-boundary test or completion claim, compare the exact
`required_commit_group` from the reviewer/validation ledger to
`git diff --cached --name-only`. Broad `glob` patterns may supplement the set
but cannot be the evidence that required files were staged. Required
`validation artifact` paths with dot/dash naming variants must be listed
explicitly. If any required artifact is missing, the correct result is a hard
fail before commit. A follow-up commit for the missed validation artifact is a
commit-boundary defect to repair, not a normal passing boundary.

## Rule: evidence-text-format-integrity

Runtime and evidence-writing sub-agents must produce JSON, JSONL, MD, TXT, and
other text evidence in commit-ready format: UTF-8 without BOM, LF line endings,
and no trailing whitespace. JSON and JSONL evidence must parse before a packet
can count.

The packet must list the exact changed evidence text files and include a passing
`git diff --check -- <changed evidence files>` result. For generated,
untracked, or no-index files, use an explicit equivalent scan for BOM, CRLF,
trailing whitespace, and JSON/JSONL parse where applicable. Repeated parent
format-only repair is a test-integrity finding against the generating worker;
do not count the packet clean until the worker fixes or re-runs the evidence
format gate.

## Rule: unbounded-executor-quarantine

Tests and QA evidence must reject any output produced by one executor/sub-agent
under an unbounded scope such as "act until product ready", "continue until the
whole plan is done", "do everything", or "no stops until ready". Deprecated
legacy `product readiness` wording must not be used as a test work-order scope.
Use bounded `Dual-Path Same-Scenario Coverage` packets after tests, reviews,
and packaging.

When the unbounded executor has an id, the parent must interrupt/close it before
accepting more output. When the id is unavailable, quarantine all outputs from
that executor as evidence-only and use a bounded replacement sub-agent for the
next scenario, slice, repair, or review packet. This is not a stop for the chat.

A sub-agent running over 10 minutes without a protocol/evidence packet is
over-scoped. Request or interrupt for the packet, close if needed, split the
scope, and continue with the next bounded work order. A valid packet must start
in Russian/plain human language with total phases/steps,
done/in-progress/remaining, what the phase solves, what the current step solves,
what evidence exists, what is not proven, and the next bounded action before
machine labels or paths. Every bounded test/QA work order must set
`must_stop_after_report=true`.

## Rule: human-readable-visibility-evidence

Machine evidence does not prove user-visible protocol compliance.

Checklist:

```text
☐ The status includes a Russian/plain human-readable execution board
☐ Total task count, completed count and names, current task and owner/sub-agent, remaining count and names are visible
☐ Current result type is visible: code, artifact, review, policy, evidence-only, or commit boundary
☐ Accepted evidence, open gaps, claims still closed with reasons, and next bounded action are visible
☐ Execution paused for plan-review visibility is stated yes/no
☐ If 10 reviews are required, records 1..10 are visible with reviewer role/id, requested model/effort, verdict, findings, repair action, affected files/sections, and plan/skills changed
☐ QSG/STATE/JSON/internal labels are translated into what was checked, result, what was fixed, and what remains
☐ Sub-agent work is visible with agent id/nickname, task, status, key result, and parent review disposition
```

If any row is missing, the correct result is visibility/protocol repair before
execution resumes, not a final stop and not a full completion claim.

## Rule: code-review-completeness-evidence

Passing tests or a green machine label do not prove that the code review covered
the correct boundary.

Checklist:

```text
☐ Exact files/sections/hosts/claims inspected are named
☐ Project coding standards and allowed tests/Gate B are checked
☐ Core logic that belongs in `llm_mvp_core` is not in a host adapter
☐ Claude/Cursor/Codex adapters remain thin
☐ Hidden deterministic shortcut sweep was run
☐ Deterministic helpers are labelled eval-only/helper/internal baseline when present
☐ Deterministic helpers are not runtime/trainable authority
☐ Docs/QA/dashboard/user-visible status impact is checked when applicable
```

If any row is missing, the code slice remains
`IN_PROGRESS_NEXT_STEP_KNOWN` with the first missing review row as the next
action.

---

## Anti-patterns

### Anti-pattern 1: Stub-fix masking
```
✗ Test fails because IncrementalTrainer returns EMPTY_EXAMPLES
  → StubIncrementalTrainer changed to return Ok("adapters/fake.lora")
  → Test passes
  → EMPTY_EXAMPLES guard now never reached in production

✓ Diagnose WHY the EMPTY_EXAMPLES error fires
  Provide a non-empty example list in the test setup
  Keep the stub strict — it should still return EMPTY_EXAMPLES for empty inputs
```

### Anti-pattern 2: Guard stacking
```
✗ FeedbackRouter.Route checks FeedbackSignal.Accepted
  Then checks UserCorrection is not null
  Both guard the same input path — second guard never reached for Accepted signal

✓ Run branch-reachability for the combined guard chain
  Ensure each branch has at least one test input that reaches it
```

### Anti-pattern 3: Coverage inflation
```
✗ 5 new tests added for FeedbackRouter
  All use NullCodeWriterCL which always returns Ok(true)
  The FlushAsync error path never tested

✓ At least one test per service must provide a stub that returns Fail(...)
  Count tests that exercise error paths, not just happy-path tests
```

### Anti-pattern 4: Sibling guard gap
```
✗ IsSuccess guard added to ProjectIndexer after ICodeReader.ListFilesAsync
  IncrementalTrainer calls ICodeReader implicitly but has no guard
  Bug: IncrementalTrainer silently processes empty input lists

✓ After adding a guard to any service:
  Audit all siblings that process the same ICodeReader/ISemanticCompiler29 input
  Apply the same guard consistently
```

### Anti-pattern 5: View-model evidence reported as full UI automation
```
✗ ApiDashboardViewModel tests pass
  → final report says full WPF UI e2e is complete
  → no running WPF window/control path was driven

✓ Report view-model tests as intermediate command-path evidence
  Keep full-interactive WPF automation open until a real UI automation harness
  drives and observes the window/control path
```

### Anti-pattern 6: Commit masks failed verification
```
✗ git diff --check prints trailing whitespace failures
  → PowerShell wrapper continues to git commit and git push
  → final report treats the later commit success as a passed gate

✓ Check $LASTEXITCODE immediately after each required verification command
  Abort before stage/commit/push on nonzero
  Treat printed verification failure output as a hard fail
✓ Never run `git diff --cached --check; git commit; git push` as one
  semicolon chain; PowerShell can continue after the failed gate, so split or
  guard the commands and enter a fix-loop when the precheck fails
✓ Before any commit, run every pre-commit check as a hard gate: `git diff
  --check`, `git diff --cached --check`, JSON parse, text-format scan,
  screenshot privacy review, and required tests. commit is forbidden if a
  pre-commit gate prints warnings/errors. do not chain commit/push after a gate
  command unless exit code is checked and the wrapper aborts on nonzero.
✓ A committed bad evidence packet requires an immediate fix commit plus a
  governance repair; report the exact failed gate and repaired rule to Luba.
Required verification literals: commit is forbidden if a pre-commit gate prints warnings/errors; do not chain commit/push after a gate command unless exit code is checked.
```

### Anti-pattern 7: Deprecated product-ready count / promise-to-continue soft stop
```
✗ report uses "product-ready count" or equivalent as the primary status
  → says "product-ready count = 0 because full host UI not proven"
  → commit/push succeeds
  → final response says "next I will continue Claude UI prep"
  → no next sub-agent id, wait, review, or real blocker exists

✓ Apply `product_readiness_term_deprecated_use_scenario_coverage`
  `dual_path_same_scenario_coverage_gate`
  `no_product_ready_count_primary_status`
  `Dual-path same-scenario coverage is product readiness evidence`
  `full live host UI path is not the readiness definition unless explicitly re-approved by Luba`
  `same scenario Path A plus Path B`
  `NOT READY is a bounded sub-agent work queue, not a stop`
  `human-readable phase counts before execution continuation`
  Use `Dual-Path Same-Scenario Coverage`
  Report `scenario coverage: total / accepted / api_only / ui_only / blocked / not_started`
  Show current scenario Path A and Path B status
  State total scenarios, covered by Path A, covered by Path B, blocked Path B with reason, and remaining steps
  Treat unreachable extension/plugin UI route as blocked Path B with exact screen/state, next scenario/product fix, and next bounded action, not as the old product-readiness gate
✓ Apply `final_between_bounded_slices_forbidden`
  `final/status/report/commit-push/status-board is not a stop while authorized work remains`
  `status_board_counts_drive_continuation`
  `accidental_final_recovery_requires_subagent_repair_and_resume`
  `promise_to_continue_is_not_continuation`
  `commit/push boundary is not a stop`
  Check `next_subagent_started_or_real_blocker`
  Continue to the next bounded slice in commentary instead of final
```

Before a final/status packet is accepted, tests/review must answer:
`scenario_coverage_complete?`, `explicit_user_stop?`,
`active_requirements_complete?`, `status_board_remaining_count_zero?`,
`next_bounded_action_exists?`, `next_subagent_started_or_real_blocker?`.
False/false/false/false/true/false rejects the packet and requires bounded
continuation. A status board with `remaining_count > 0` and
`next_bounded_action_exists=true` is interim status only; commit/push of an
evidence packet or status board is not a completion boundary when remaining
groups exist.

A scenario is accepted only when the same scenario and same prompt are proven
through both `Path A` API/boundary evidence and `Path B` local-agent UI path
evidence in the real local host application, with screenshots, window
inventory, redaction log, and trace/correlation evidence when available.
Test-integrity review must reject any final/status packet that uses the missing
full live host UI path as the readiness definition. The supported narrow claim
is only the current dual-path scenario coverage state, and every NOT READY,
not-proven, blocked, or needs-fix finding must name the next bounded sub-agent
test/prep/mitigation task or a true external blocker with mitigation evidence.
