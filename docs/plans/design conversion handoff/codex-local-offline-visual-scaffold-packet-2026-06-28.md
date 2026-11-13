# Codex Local Offline Visual Scaffold Packet - 2026-06-28

claim_scope:
- Created this bounded doc-only resolver packet for deterministic/no-secret local fallback planning for visual comparison readiness.
- Read only the two allowed prior packets in the same handoff folder:
  - `codex-external-blocker-fallback-sweep-2026-06-28.md`
  - `codex-preflight-resolution-sweep-2026-06-28.md`
- Checked that this target packet path did not already exist before creation.
- Did not inspect project source/tests in this turn.
- Did not run tests, builds, browser/runtime probes, external AI, git actions, staging, commit, push, PR mutation, branch mutation, cleanup, archive, delete, restore, or stash.
- Did not read, print, infer, verify, or use secrets.

evidence_scope:
- Prior packet `codex-external-blocker-fallback-sweep-2026-06-28.md` reported a no-secret blocked visual judge stub and deterministic local render/comparison scaffolds.
- Prior packet `codex-preflight-resolution-sweep-2026-06-28.md` reported Packet Family 8 as saved-output-boundary only and still blocked for full runtime/design proof without exact scenario commands, targets, thresholds, and artifact-write permissions.
- Current-turn target existence check returned absent before this file was created.
- This packet is a resolver/planning document only.

count_claimed:
- 1 resolver packet file created.
- 2 allowed source packet files read.
- 1 target existence check performed.
- 0 source/test files inspected in this turn.
- 0 external AI calls.
- 0 secret reads.
- 0 tests/builds/browser/runtime probes.
- 0 git actions.

independent_evidence_count:
- 3 current-turn records: two allowed packet readbacks and one target existence check.

subagent_id_or_NONE:
- LOCAL_OFFLINE_VISUAL_SCAFFOLD_PACKET

downgrade_needed:
- Yes for any broader claim. This packet supports only a deterministic/no-secret local fallback planning boundary.
- This is not external visual AI.
- This is not full visual-fidelity proof.
- This is not source-design fidelity proof.
- This is not runtime/browser proof.
- This is not product readiness, merge readiness, or clean-repo proof.

## Packet Purpose

This packet resolves a safe local/offline fallback route for visual comparison readiness when external visual AI is unavailable or unconfigured.

The fallback is a `deterministic metadata-only comparison scaffold`. It can support conservative accounting that a local no-secret comparison scaffold is selectable for future bounded work. It cannot prove visual fidelity, source-design fidelity, human-visible correctness, or product readiness.

## Required Blocker Literals

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

Use this when a future local/offline worker is missing any of:
- exact artifact paths;
- exact design-reference paths, if source-design comparison is required;
- exact local/offline scenario or test selector;
- exact output evidence path;
- exact pass/fail rule for metadata-only comparison;
- permission to run tests, builds, browser/runtime, or a scaffold harness;
- permission to write a new evidence report.

`PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`

Use this for the external visual AI path when provider/account/model/endpoint/credential-reference/data-sharing/rubric/threshold decisions remain missing.

`DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`

Conservative default for current handoff accounting: waive the unconfigured external service only. This waiver does not prove external AI visual judge success, visual fidelity, source-design fidelity, product readiness, runtime readiness, or merge readiness.

`deterministic metadata-only comparison scaffold`

Approved wording for the local/offline fallback boundary. The scaffold may compare deterministic metadata or markers. It must not be described as a visual AI judge, screenshot-fidelity proof, browser proof, or full design-conversion readiness proof.

## Local/Offline Fallback Classification

fallback_found: yes, limited.

Supported narrow claim:
- A future worker may route Packet Group 7 / visual comparison readiness to a local no-secret deterministic scaffold if Luba approves the exact artifact set, pass/fail boundary, command or script entrypoint, and evidence output path.

Forbidden upgraded claims:
- no external visual AI proof;
- no full visual-fidelity proof;
- no source-design fidelity proof;
- no browser/runtime proof;
- no product readiness;
- no merge readiness;
- no clean repository state;
- no `NO_REMAINING_BOUNDED_ACTIONS_FOR_FULL_CHAIN`.

## Candidate Future Commands

The following commands were identified in the prior fallback sweep packet. I did not independently inspect source/tests in this turn, so a future executor must revalidate project paths and test names before execution.

These commands are future-only and require explicit approval to run tests:

```text
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionVisualJudgeTests.NoConfiguredVisualJudge_ReturnsExplicitBlockerWithoutExternalCall --no-restore
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionQaProviderTests --no-restore
```

Commands needing future artifact/path selection:
- Any command or harness that compares saved output artifacts against design references.
- Any command or harness that writes a local/offline scaffold evidence report.
- Any browser/runtime screenshot command.
- Any external visual AI judge command.

## Future Local/Offline Work Order Template

Worker:
- `bounded-local-offline-visual-scaffold-worker`

Allowed intent:
- Revalidate the local no-secret visual judge blocker test and deterministic QA provider tests, or run an explicitly approved local/offline metadata-only comparison harness.

Required before execution:
- exact artifact paths;
- exact output evidence report path;
- exact pass/fail rule for metadata-only comparison;
- confirmation that no external AI, secrets, browser/runtime, PR mutation, branch mutation, staging, commit, or push is allowed;
- explicit permission to run tests or the named harness;
- explicit permission to write the named evidence report.

Required output markers:
- `external_ai_visual_judge_executed:false`
- `ai_visual_judge_claimed:false`
- `visual_fidelity_claimed:false`, unless a separate approved visual-fidelity path is actually executed and proven;
- `hidden_determinism_boundary:metadata_only_comparison`
- `deterministic metadata-only comparison scaffold`

Stop conditions:
- Return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` if artifact paths, output path, commands, thresholds, or write/test permissions are missing.
- Return `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` if Luba requests real external visual AI proof without supplying provider/account/model/endpoint/credential-reference/data-sharing/rubric/threshold decisions.
- Return a downgraded result if only metadata markers are checked.

## Future External Visual AI Work Order Boundary

This packet does not configure or invoke external visual AI.

Smallest required Luba decisions for external visual AI:
- provider family;
- account/project/workspace/tenant alias;
- endpoint alias or base URL without credentials;
- model id;
- credential environment variable name or secret-reference name, without value;
- whether presence-only credential check is allowed;
- data-sharing approval and artifact classes allowed to leave the machine;
- exact artifact paths;
- exact design-reference paths;
- rubric;
- pass threshold;
- output evidence path;
- retention/logging policy;
- retry, timeout, and budget limits.

Until those are supplied, the external path remains `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`.

## Safe Next-Action Queue

1. If Luba wants the local/offline route, create a bounded worker order to revalidate the candidate test commands and/or selected harness paths, then write only the approved evidence report.
2. If Luba wants only conservative handoff accounting, record `DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE` in the approved handoff target and keep all visual-readiness claims downgraded.
3. If Luba wants true external/source-fidelity visual judging, first run a bounded config-resolution packet worker that collects names only, never secret values.
4. If Luba wants browser/runtime visual proof, create a separate Packet Family 8 scenario with exact commands, ports/windows, clicks/typing/expected visible state, evidence filenames, and thresholds.

## Smallest Luba Decisions Still Needed

For local/offline deterministic scaffold:
- Approve or reject metadata-only comparison as sufficient for Packet Group 7 fallback accounting.
- Name exact input artifact paths.
- Name exact output report path.
- Approve the future command/harness to run, or require future artifact/path selection first.
- Define the pass/fail rule or confirm the result must stay informational only.

For conservative waiver:
- Approve or reject `DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`.
- Name the exact handoff/index/report file where the waiver should be recorded.

For external visual AI:
- Supply the provider/account/model/endpoint/credential-reference/data-sharing/rubric/threshold decisions listed above.

## Final Boundary

This packet is doc-only. It creates a local/offline resolver path for future bounded work, but it does not execute that path.

not_external_visual_ai: true

not_full_visual_fidelity_proof: true

not_product_readiness: true

not_runtime_or_browser_proof: true

no_git_actions: true

no_external_ai_call: true

no_secret_values_read_or_printed: true

