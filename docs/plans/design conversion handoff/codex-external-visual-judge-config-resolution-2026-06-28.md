# Codex External Visual Judge Config Resolution - 2026-06-28

claim_scope:
- Created this bounded external visual judge config resolution packet only.
- Used the allowed source packets `codex-external-blocker-fallback-sweep-2026-06-28.md` and `codex-preflight-resolution-sweep-2026-06-28.md`.
- Did not call external AI, run browser/runtime/tests/builds, inspect or print secrets, mutate git, stage, commit, push, create branches, or mutate PRs.
- This packet is a decision/config packet, not external AI visual judge proof, not source-design fidelity proof, not product completion, not merge readiness, and not clean-repo proof.

evidence_scope:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-external-blocker-fallback-sweep-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-preflight-resolution-sweep-2026-06-28.md`
- Handoff folder codex-packet name listing used only to confirm context and target naming.

count_claimed:
- 1 config resolution packet created.
- 2 allowed source packet contents read.
- 0 external AI calls.
- 0 secret values read.
- 0 tests/builds/browser/runtime probes.
- 0 git actions.

independent_evidence_count:
- 3 current-turn evidence records: handoff codex-packet name listing, external blocker fallback sweep readback, and preflight resolution sweep readback.

subagent_id_or_NONE: BOUNDED_EXTERNAL_VISUAL_JUDGE_CONFIG_RESOLUTION_PACKET_WORKER

downgrade_needed:
- No for the narrow claim that this packet was created from the allowed source packets.
- Yes for external visual judge readiness: provider/account/model/endpoint/secret-variable/data-sharing/rubric/threshold decisions are still unresolved.
- Yes for any product, runtime, source-design fidelity, visual fidelity, merge readiness, or external AI success claim.

## Required Boundary Literals

- `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `EXTERNAL_BLOCKER_FALLBACK_SWEEP`

## Minimal Safe Default

Recommended minimal safe default:

`NO_EXTERNAL_AI_CALL_UNTIL_LUBA_PROVIDES_PROVIDER_SECRET_ENV_VAR_NAME_AND_DATA_SHARING_APPROVAL`

Meaning:
- Do not call Claude, OpenAI, Gemini, or any other external AI visual judge.
- Do not ask the parent to expose secret values.
- Do not read, print, copy, infer, or store credential values.
- Use placeholder variable names only until Luba supplies the approved provider, approved secret environment variable name, and explicit data-sharing approval.
- Keep Packet Group 7 blocked as `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`.
- If a future local/offline scaffold path is selected, keep it separately blocked as `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` until exact local artifact/rubric/threshold/command decisions exist.

## Current Packet Classification

External visual judge path:

`PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`

Reason:
- No approved provider is selected.
- No approved account/project/workspace/tenant alias is selected.
- No approved model is selected.
- No approved endpoint/base URL or endpoint alias is selected.
- No approved credential environment variable name is selected.
- No data-sharing approval names which artifact classes may leave the machine.

Local/offline or waiver path:

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

Reason:
- The fallback sweep found only a no-secret blocked stub and deterministic metadata-only comparison scaffolds.
- That local path does not prove external AI visual judge success or source-design fidelity.
- A future local/offline run still needs exact artifact set, design references or waiver, rubric, threshold, output path, retention policy, budget/time limits, and command/test permission.

## Unresolved Provider/Account/Model/Endpoint/Secret Decisions

| Field | Current value | Decision needed |
| --- | --- | --- |
| Provider family | `<UNRESOLVED_PROVIDER>` | Choose `Claude`, `OpenAI`, `Gemini`, another approved provider, `local/offline`, or `waiver`. |
| Account/project/workspace/tenant | `<UNRESOLVED_ACCOUNT_OR_PROJECT_ALIAS>` | Provide a non-secret account, project, workspace, or tenant alias. |
| Model | `<UNRESOLVED_MODEL_ID>` | Provide an exact model id approved for visual/image comparison. |
| Endpoint/base URL | `<UNRESOLVED_ENDPOINT_ALIAS_OR_BASE_URL>` | Provide endpoint alias or base URL without credentials. |
| Secret env var name | `<UNRESOLVED_SECRET_ENV_VAR_NAME>` | Provide the environment variable name only, never the secret value. |
| Credential presence check | `<UNRESOLVED_CREDENTIAL_PRESENCE_CHECK_POLICY>` | Decide whether a future worker may check presence only, without printing the value. |
| External call permission | `<UNRESOLVED_EXTERNAL_CALL_PERMISSION>` | Decide whether a future worker may make the external AI request after config is supplied. |

Provider-specific placeholder examples:

| Provider option | Account/project placeholder | Model placeholder | Endpoint placeholder | Secret env var name placeholder |
| --- | --- | --- | --- | --- |
| Claude | `<CLAUDE_ACCOUNT_OR_WORKSPACE_ALIAS>` | `<CLAUDE_VISION_MODEL_ID>` | `<CLAUDE_ENDPOINT_ALIAS_OR_BASE_URL>` | `<CLAUDE_API_KEY_ENV_VAR_NAME>` |
| OpenAI | `<OPENAI_PROJECT_OR_ORG_ALIAS>` | `<OPENAI_VISION_MODEL_ID>` | `<OPENAI_ENDPOINT_ALIAS_OR_BASE_URL>` | `<OPENAI_API_KEY_ENV_VAR_NAME>` |
| Gemini | `<GEMINI_PROJECT_ALIAS>` | `<GEMINI_VISION_MODEL_ID>` | `<GEMINI_ENDPOINT_ALIAS_OR_BASE_URL>` | `<GEMINI_API_KEY_ENV_VAR_NAME>` |
| Other approved AI | `<OTHER_PROVIDER_ACCOUNT_ALIAS>` | `<OTHER_PROVIDER_MODEL_ID>` | `<OTHER_PROVIDER_ENDPOINT_ALIAS_OR_BASE_URL>` | `<OTHER_PROVIDER_SECRET_ENV_VAR_NAME>` |
| Local/offline | `<LOCAL_PROVIDER_ALIAS_OR_WAIVER>` | `<LOCAL_SCORER_OR_STUB_ID>` | `<LOCAL_ENTRYPOINT>` | `<NO_SECRET_REQUIRED>` |

## Unresolved Policy Decisions

| Policy field | Current value | Decision needed |
| --- | --- | --- |
| Data-sharing permission | `<UNRESOLVED_DATA_SHARING_APPROVAL>` | Name which artifact classes may be sent externally: screenshots, rendered HTML/CSS, DOM text, design references, metadata, diffs, prompts, logs. |
| Artifact set | `<UNRESOLVED_ARTIFACT_SET>` | Provide exact allowed input artifact paths and exact excluded artifact classes. |
| Design references | `<UNRESOLVED_DESIGN_REFERENCES>` | Provide exact source-design reference paths, URLs, or an explicit source-reference waiver. |
| Rubric | `<UNRESOLVED_RUBRIC>` | Provide comparison criteria and forbidden claims. |
| Pass threshold | `<UNRESOLVED_PASS_THRESHOLD>` | Provide exact numeric or categorical threshold and rounding policy. |
| Output evidence path | `<UNRESOLVED_OUTPUT_EVIDENCE_PATH>` | Provide exact output report path for future worker evidence. |
| Retention policy | `<UNRESOLVED_RETENTION_POLICY>` | Decide whether prompts/responses/images may be retained locally or externally, and for how long. |
| Retry policy | `<UNRESOLVED_RETRY_POLICY>` | Provide retry limit, retry conditions, and backoff/timeout behavior. |
| Budget limit | `<UNRESOLVED_BUDGET_LIMIT>` | Provide max calls/tokens/cost/time before stop. |
| Rate/cost cap | `<UNRESOLVED_RATE_COST_CAP>` | Provide per-run cap and provider quota boundary. |
| Logging/redaction | `<UNRESOLVED_LOGGING_REDACTION_POLICY>` | Decide what may be logged and require no secret values in logs. |
| Failure policy | `<UNRESOLVED_FAILURE_POLICY>` | Decide whether failure records blocker, waiver, retry, or local fallback routing. |

## Candidate Config Template

Use this as a decision template only. It contains no secrets.

```yaml
external_visual_judge:
  provider_family: <Claude | OpenAI | Gemini | other approved provider | local/offline | waiver>
  account_or_project_alias: <approved non-secret alias>
  endpoint_alias_or_base_url: <approved endpoint name or URL without credentials>
  model_id: <approved visual judge model id>
  secret_env_var_name: <approved environment variable name only>
  credential_presence_check_allowed: <true|false>
  external_call_allowed: <true|false>
  data_sharing_approval:
    allowed_artifact_classes:
      - <screenshots | rendered_html_css | dom_text | design_references | metadata | diffs | other>
    excluded_artifact_classes:
      - <secrets | credentials | private logs | unapproved traces | other>
  artifact_set:
    input_paths:
      - <exact approved artifact path>
    design_reference_paths:
      - <exact approved design reference path or WAIVED_BY_LUBA>
    output_evidence_path: <exact future report path>
  rubric:
    source: <inline rubric or exact rubric path>
    pass_threshold: <exact threshold>
    forbidden_claims:
      - product completion unless separately proven
      - merge readiness unless separately proven
      - runtime readiness unless separately proven
      - source-design fidelity beyond approved artifact/reference scope
  retention:
    local_prompt_response_retention: <none | redacted | full approved>
    provider_retention_allowed: <true|false|provider-default-approved>
    delete_after_days: <number or not-applicable>
  retry_budget_rate:
    retry_limit: <integer>
    timeout_seconds: <integer>
    max_external_calls: <integer>
    max_cost_usd: <number>
    rate_limit_policy: <exact cap or provider default approved>
  failure_policy:
    on_missing_secret_or_account: PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT
    on_preflight_failure: PACKET_BLOCKED_BY_PREFLIGHT_FAILURE
    on_provider_error: <retry | block | waive | local/offline fallback>
```

## Recommended Resolution Options

1. Conservative default: no external AI call.
   - Required Luba decision: approve `NO_EXTERNAL_AI_CALL_UNTIL_LUBA_PROVIDES_PROVIDER_SECRET_ENV_VAR_NAME_AND_DATA_SHARING_APPROVAL`.
   - Result: Packet Group 7 remains blocked or waived for handoff accounting only.
   - Claims allowed: no external AI call was made; no external AI judge proof exists.

2. True external AI visual judge.
   - Required Luba decisions: provider, account/project alias, model, endpoint, secret env var name, credential presence-check policy, data-sharing approval, artifact set, design references, rubric, threshold, output path, retention/retry/budget/rate/cost caps.
   - Result after future approved worker: external judge may be run if all preflight checks pass.
   - Claims allowed only after execution evidence: external call attempted/completed for the exact artifact set and rubric.

3. Local/offline scaffold.
   - Required Luba decisions: accept metadata-only/no-external boundary, exact commands or script entrypoints, exact artifacts, rubric or waiver, threshold, output report path.
   - Result after future approved worker: local scaffold evidence only.
   - Claims forbidden: external AI judge success, source-design fidelity, product completion.

4. Explicit waiver.
   - Required Luba decision: approve waiver language for unconfigured external visual judge.
   - Result: handoff records waiver only.
   - Claims forbidden: visual judge passed, source-design fidelity proven, product ready.

## Safe Follow-Up Work Orders

### Follow-Up A - Owner Decision Collection

Worker: `bounded-external-visual-judge-owner-decision-collector`

Allowed action:
- Produce a no-secrets decision checklist from this packet.
- Do not call external AI.
- Do not read or print credential values.
- Do not run tests/builds/browser/runtime or mutate git/PRs.

Required owner inputs:
- provider family;
- account/project/workspace alias;
- model id;
- endpoint alias or base URL;
- secret environment variable name only;
- credential presence-check permission;
- external-call permission;
- data-sharing approval;
- artifact set;
- design references or explicit waiver;
- rubric;
- threshold;
- output evidence path;
- retention, retry, budget, rate/cost caps.

Stop condition:
- Any missing provider/account/model/endpoint/secret-variable/data-sharing/rubric/threshold decision keeps the packet at `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`.

### Follow-Up B - External Visual Judge Preflight

Worker: `bounded-external-visual-judge-preflight-worker`

Allowed action after owner decisions:
- Verify only non-secret config completeness.
- If approved, check whether the named env var exists without printing its value.
- Verify artifact/design-reference path existence only for approved paths.
- Return a preflight report.

Forbidden:
- No external AI call unless a later work order explicitly authorizes execution.
- No secret value output.

Stop condition:
- Missing env var presence, missing artifact path, missing design reference, missing rubric, missing threshold, or missing data-sharing approval returns `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

### Follow-Up C - External Visual Judge Execution

Worker: `bounded-external-visual-judge-execution-worker`

Allowed action only after Follow-Up B passes:
- Execute exactly one approved external visual judge call set within the approved provider/model/endpoint/artifact/rubric/budget/rate/cost policy.
- Write only the approved evidence report path.
- Redact secrets and omit credential values.

Stop condition:
- Provider error, rate/cost cap, missing credential, unapproved artifact, or data-sharing mismatch stops execution and reports the exact blocker.

### Follow-Up D - Local/Offline Scaffold Evidence

Worker: `bounded-local-offline-visual-scaffold-worker`

Allowed action only if Luba selects local/offline:
- Run only approved local/offline commands or entrypoints.
- Record `external_ai_visual_judge_executed:false`.
- Record `ai_visual_judge_claimed:false`.
- Record `visual_fidelity_claimed:false` unless a separate visual fidelity path is approved and executed.

Stop condition:
- Missing exact commands, artifacts, rubric, threshold, or output path returns `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

### Follow-Up E - Waiver Recording

Worker: `bounded-packet7-waiver-record-worker`

Allowed action only if Luba selects waiver:
- Record the approved waiver in approved handoff paths.
- Preserve forbidden claims: no external AI judge proof, no source-design fidelity proof, no product completion, no merge readiness from waiver alone.

Stop condition:
- If Luba requires external visual judge proof, route back to Follow-Up A/B/C.

## Final Boundary

This packet resolves the decision surface, not the decisions themselves.

Remaining blockers:
- provider/account/model/endpoint/secret-variable name missing;
- data-sharing approval missing;
- artifact set missing;
- design references missing or not waived;
- rubric missing;
- threshold missing;
- output/retention/retry/budget/rate/cost policy missing;
- external AI execution permission missing.

Therefore the current safe state remains:

`PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`

and, for any attempted local/offline or execution path without complete preflight:

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

