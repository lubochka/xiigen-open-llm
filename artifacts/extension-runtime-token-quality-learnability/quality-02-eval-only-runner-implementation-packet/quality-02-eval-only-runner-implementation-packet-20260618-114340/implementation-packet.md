# QUALITY-02 Eval-only Runner Implementation Packet

Generated at local time: 2026-06-18T11:43:40+03:00

Sub-agent: bounded-worker-quality-02-implementation-packet-20260618-114340

Requested model: `gpt-5.5`

Requested reasoning effort: `xhigh`

Runtime model metadata: unavailable in this worker packet. No concrete contradictory runtime metadata was visible, so this packet is counted from the work-order request with `mode_verified_from_work_order=true`.

## Scope

This packet prepares the next implementation slice for the QUALITY-02 eval-only runner. It does not implement source code, run eval, run host runtime, call external AI, start training, mutate checkpoints, mutate locators, or open any readiness claim.

The next worker must implement runner code only after parent review and commit of this packet. The implementation must keep core scoring and validation inside `llm_mvp_core`, with any command-line or script surface acting as a thin adapter.

## Loaded Rules And Evidence

Loaded governance and guide files:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/HOW-TO-USE-SKILLS.md`
- `docs/skills/output-readability-gate-SKILL.md`
- `docs/skills/phase-completion-packager-SKILL.md`
- `docs/skills/session-execution-log-SKILL.md`
- `docs/skills/skill-injection-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/test-integrity-SKILL.md`
- `docs/skills/verification-before-completion-SKILL.md`

Accepted input evidence for this packet:

- QUALITY-01 prompt pack and future-runner input contract exist in `artifacts/extension-runtime-token-quality-learnability/quality-01-eval-prompt-pack-rubric/quality-01-eval-prompt-pack-rubric-20260618-093007/`.
- QUALITY-02 scorer contract exists in `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/scorer-contract.json`.
- QUALITY-02 score-output schema exists in `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/score-output-schema.json`.
- Artifact dry-run validation exists in `artifacts/extension-runtime-token-quality-learnability/quality-02-artifact-runner-dry-run-validation/quality-02-artifact-runner-dry-run-validation-20260618-103305/artifact-dry-run-join-report.json`.
- Dry-run preflight reports 12 scenarios, 54 pair rows, 7 user-adaptive eval-only scenarios, 21 user-adaptive eval-only pair rows, 5 evidence-only scenarios, 33 evidence-only pair rows, and all claim locks false.
- Existing core quality scorer is `43.SmartSkillsDomain/ExtensionQualityScoring.cs`; existing tests include `Tests/ExtensionQualityScoringTests.cs`.
- Existing token runner harness is `tools/token-quality-runner.ps1`, but that script is token-specific and must not become the core quality runner authority.

## Human-readable Board

Всего: 10 крупных блоков плана; в текущем QUALITY-02 packet-срезе 4 задачи.

Сделано: 4/4 в этом packet-срезе: загружены правила и skills/guides; приняты входные QUALITY-01/QUALITY-02 evidence-only артефакты; подготовлен implementation packet; state обновляется на parent-review boundary. По большому плану продуктовые runtime/token/quality/learnability блоки еще не закрыты как готовые.

Сейчас выполняется: QUALITY-02 eval-only runner implementation packet, владелец - bounded worker sub-agent `bounded-worker-quality-02-implementation-packet-20260618-114340`.

Осталось: 2 ближайших шага - parent review/commit этого packet-среза, затем bounded code implementation slice `QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-NO-EVAL-EXECUTION`. По полному плану остаются runtime proof, thin adapters, live host proof, token before/after, quality improvement, learnability/DPO/lifecycle, rollback/locator safety, docs/QA and final readiness review.

Тип текущего результата: artifact/evidence-only/implementation-packet.

Принятые доказательства: есть QUALITY-01 prompt pack/rubric contract, QUALITY-02 scorer contract, score-output schema, dry-run join/preflight evidence, and no-overclaim firewall. Эти доказательства показывают только, что будущий runner можно реализовать against a checked static contract.

Закрытые claims и почему: token savings закрыт, потому что token runner and same-pair quality non-regression не выполнялись; quality improvement закрыт, потому что scores are not produced; semantic quality and semantic readiness закрыты, потому что held-out/ablation/no-leakage/no-memorization controls did not run; learnability закрыт, потому что DPO training, fresh-load, continue-training, backup/export/import/share and rollback did not run; live host readiness закрыт, потому что Claude/Cursor/Codex runtime was not launched; common readiness закрыт, потому что common source/license/material gate remains blocked; strict QUALITY-03 закрыт, потому что runner implementation and actual scoring are still missing; training readiness, target promotion, locator update and final readiness закрыты, потому что no checkpoint/model/locator/runtime mutation or final review evidence exists.

Открытые разрывы: source-code runner not implemented; exact code review not run; targeted tests not added; dashboard QA impact not reviewed in code; actual eval on approved user-local rows not authorized; evidence-only rows must remain excluded; common rows remain blocked; no final readiness evidence exists.

Следующее действие: parent reviews and commits this packet, then launches bounded code worker for `QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-NO-EVAL-EXECUTION` with the work order below.

Execution paused for plan-review visibility: no. The plan-review visibility repair has already been committed and loaded by parent; runtime/eval/training remain closed because this is an implementation-packet slice, not because the visibility gate is still pending.

## Exact Next Implementation Work Order

Requested artifact/code type:

- Implement an eval-only quality runner in C# inside `llm_mvp_core`.
- The runner must validate QUALITY-01/QUALITY-02 contracts, read approved eval-only input rows, create score rows only for allowed user-adaptive eval-only rows, keep evidence-only and common-blocked rows out of scoring, and write human-readable and machine-readable reports.
- The runner may use deterministic rubric evaluation only as explicitly labelled eval-only evidence. It must not present deterministic scoring as trained model logic, semantic readiness, token savings, learnability, or final readiness.

Allowed write scope for the next code worker:

- Create `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunnerContracts.cs`.
- Create `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs`.
- Create `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`.
- Create `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Program.cs`.
- Update `LlmFromScratch.sln` only if the new tool project is added to the solution.
- Update `Tests/ExtensionEvalOnlyQualityRunnerTests.cs`.
- Update `Tests/LlmFromScratch.Tests.csproj` only if the test project needs a new reference for the tool project; prefer testing core library directly when possible.
- Update `43.SmartSkillsDomain.Dashboard/ApiDashboardCatalog.cs` and `Tests/ApiDashboardCatalogTests.cs` only if the runner becomes visible in the dashboard/API catalog. If no dashboard/UI surface changes, the worker must write an explicit "not applicable because no dashboard/API surface changed" row in its review packet.
- Update docs only if code behavior, command usage, report shape, dashboard/API behavior, or evidence scope changes. Minimum doc candidates: `docs/COMPONENT-INTERFACES.md`, `docs/SUBPROJECT-MAP.md`, and the active execution state.
- Write implementation evidence artifacts only under `artifacts/extension-runtime-token-quality-learnability/quality-02-eval-only-runner-implementation/`.

Forbidden write scope for the next code worker:

- Do not touch `artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/`.
- Do not mutate common training sets, user-local checkpoints, common checkpoints, manifests, locators, targets, rollback assets, or plugin/host runtime files.
- Do not create actual eval output over private or current QUALITY-01/QUALITY-02 production rows unless a fresh approval packet explicitly authorizes that eval run.
- Do not move scoring, routing, DPO, prompt-to-task, orchestration, skill-selection, trainable, or semantic authority into a PowerShell script, dashboard, Claude adapter, Cursor adapter, Codex adapter, or test-only project.

Expected files/classes for the next code worker:

- `ExtensionEvalOnlyQualityRunnerContracts` records:
  - `ExtensionEvalOnlyQualityRunnerOptions`
  - `ExtensionEvalOnlyQualityRunnerInputRow`
  - `ExtensionEvalOnlyQualityRunnerLayerInput`
  - `ExtensionEvalOnlyQualityRunnerOutputRow`
  - `ExtensionEvalOnlyQualityRunnerPairResult`
  - `ExtensionEvalOnlyQualityRunnerReport`
  - `ExtensionEvalOnlyQualityRunnerValidation`
  - `ExtensionEvalOnlyQualityRunnerClaimLocks`
- `ExtensionEvalOnlyQualityRunner` public API:
  - return `AgentInfrastructure.OperationResult<ExtensionEvalOnlyQualityRunnerReport>` from public runner methods;
  - read JSON/JSONL input using structured `System.Text.Json` APIs;
  - validate required fields from `future-runner-input-contract.json` and `scorer-contract.json`;
  - reject or skip `EVIDENCE_ONLY` rows as non-runnable context;
  - reject or skip `COMMON_BLOCKED` rows as common-training blocked context;
  - process only `USER_ADAPTIVE_EVAL_ONLY` rows when all pre-score gates are present;
  - require `approval_ref`, `user_local_root_ref`, `raw_private_input_ref`, `redacted_shared_evidence_ref`, `source_ref`, `doc_id`, `host_name`, `method`, `baseline_output_ref`, `adaptive_output_ref`, and mutation attestations;
  - read baseline/adaptive output refs only from approved local/redacted fixture paths in tests;
  - produce per-layer rows for `prompt_to_task`, `orchestration_reasoning`, `skill_selection`, and `final_usefulness`;
  - call `ExtensionQualityScoring.Score` or a narrowly adapted core scoring method for per-row verdicts;
  - always set token, quality-improvement, semantic-quality, learnability, live-host, common-readiness, strict-quality, training, promotion, locator and final-readiness claim fields to false unless a later approved scoring/execution phase proves them;
  - write JSON, JSONL and Markdown report outputs through core code, not through script-only formatting logic.
- `Quality02EvalOnlyRunner` console adapter:
  - parse arguments only;
  - call `ExtensionEvalOnlyQualityRunner`;
  - return nonzero exit code on validation failure;
  - avoid scoring logic in `Program.cs`.

Exact code-review scope for the next worker:

- Project coding standards: OperationResult return types for public runner methods, no business-logic throws, no null returns, no external ML frameworks, every production line commented according to project rules, and no direct concrete dependency pattern drift.
- Allowed tests and gate: targeted build/test only for this slice; no broad ritual `dotnet test` unless the active Gate B or current Luba instruction explicitly requires it.
- `llm_mvp_core` core boundary: scoring and validation logic must live in `43.SmartSkillsDomain`; the console/script/dashboard/host layer must be thin.
- Thin Claude/Cursor/Codex adapters: the runner must not add host adapter logic, host runtime claims, or host-side scoring. Any host name in rows is evidence metadata, not runtime proof.
- Hidden deterministic shortcut sweep: inspect for scorers, routers, lookups, counters, exact prompt/source replay, static fixtures, prompt hash shortcuts, first-row fallback, candidate-order shortcuts, filename/path/source-id authority, or static thresholds presented as trainable/semantic authority.
- Deterministic helper labels: rubric scoring and schema checks must be labelled `eval-only`, `helper`, `internal detail`, or `baseline`, and must not be final runtime/trainable authority.
- Documentation, QA and dashboard impact: if the runner is surfaced in dashboard/API, update catalog/tests and add a dashboard QA scenario with human API name, example input, expected output, negative case, raw/debug scope, and evidence boundary. If not surfaced, state why dashboard QA change is not applicable.
- No-overclaim review: every report must include "what this proves", "what this does not prove", and "next independent track".

Expected tests/checks for the next worker:

- JSON parser test rejects malformed JSONL with a human-readable error.
- Validation test rejects missing `approval_ref`, route, host, method, baseline/adaptive refs, privacy redaction status, or mutation attestations.
- Route test processes only `USER_ADAPTIVE_EVAL_ONLY` rows and excludes `EVIDENCE_ONLY` and `COMMON_BLOCKED` rows from scoring.
- Claim-lock test proves every forbidden claim field remains false in row, pair, report and Markdown outputs.
- Deterministic-label test proves rubric/scorer output is labelled eval-only and cannot become semantic/trainable authority.
- Output-shape test writes JSON, JSONL and Markdown with required counts and human-readable board.
- Pair comparison test proves adaptive rows cannot open quality improvement when any layer regresses or any reviewer note is missing.
- Source/privacy test proves raw private text is not copied into common repo artifacts and redacted refs are used for shared evidence.
- Dashboard/API test is required only if dashboard/catalog files change; otherwise add a review row saying no dashboard behavior changed.
- Targeted commands expected:
  - `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionEvalOnlyQualityRunner"`
  - `dotnet build 43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Quality02EvalOnlyRunner.csproj`
  - JSON parse checks for every generated report.
  - JSONL parse checks for every generated row file.
  - `git diff --check`

DPO/source-quality/training/eval boundaries for the next worker:

- Allowed now: code implementation, targeted unit/integration tests, synthetic fixture runner invocation, schema validation, dry-run output generation from synthetic test data, and review artifacts.
- Closed now: actual eval over current private/user rows, external AI/model calls, host runtime, DPO creation, training, continue-training, checkpoint mutation, target promotion, locator update, rollback mutation, common training, common source ingestion, and final readiness.
- Source-quality requirement: the runner must preserve `source_ref`, `doc_id`, privacy class, redaction status, and route classification. It must not promote private/project/stack-specific evidence into common training.
- Eval-only requirement: if the runner is invoked in tests, reports must say the evidence is test-only or synthetic-fixture eval-only and does not prove product quality.

Local model lifecycle boundaries future code must prove:

- The runner must record that common checkpoints, user checkpoints, manifests, locators and targets were not mutated.
- Any later training phase must separately prove user/cache load first, repo/common fallback second, backup/export/import/share, load-without-retrain, continue-training from an existing checkpoint, fresh-load, held-out/source-disjoint eval, ablation/no-memorization, rollback, and approved target/locator promotion.
- Saved checkpoint existence alone must remain insufficient.
- Eval-only score rows must not be reused as training, rollback, promotion, common readiness, semantic readiness, or final readiness evidence.

Git and authority boundary for the next worker:

- `git_authority=forbidden`
- `allowed_git_commands=NONE`
- `commit_push_authorized=false`
- `parent_will_perform_commit_push_after_review=true`
- `may_derive_next_phase=false`
- `must_stop_after_report=true`
- Parent owns review and commit/push after accepting the returned evidence.

## What This Packet Proves

- The next implementation slice has an exact work order and review scope.
- The expected implementation files and tests are named.
- The runner must keep deterministic rubric logic labelled eval-only.
- The current packet did not run eval, host runtime, training, model calls, checkpoint mutation, target promotion, locator update, or source code implementation.

## What This Packet Does Not Prove

- It does not prove runner code exists.
- It does not prove quality improvement.
- It does not prove token savings.
- It does not prove semantic quality, learnability, common readiness, live host readiness, or final readiness.
- It does not authorize actual eval execution.

## Parent Review Checklist

- Confirm only packet artifacts and execution state changed.
- Confirm no source code files changed in this packet.
- Confirm the next work order keeps core logic in `llm_mvp_core`.
- Confirm all forbidden product claims remain closed.
- Commit/push this packet if accepted, then launch the bounded code worker.
