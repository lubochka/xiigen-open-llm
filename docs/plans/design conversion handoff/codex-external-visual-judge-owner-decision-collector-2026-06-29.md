# EXTERNAL_VISUAL_JUDGE_OWNER_DECISION_COLLECTOR - 2026-06-29

claim_scope:
- Created this bounded doc-only owner-decision collector for the external visual AI judge path.
- Used existing handoff reports for external visual judge configuration, local/offline scaffold boundaries, waiver state, runtime/design proof routing, and proof-task queue routing.
- Did not call external AI, read/use secrets, run runtime/browser/tests/builds, mutate git/PRs, spawn nested agents, use codex.exe, or touch claude-* files.
- This collector is not external visual judge proof, not visual fidelity proof, not source-design fidelity proof, not runtime/browser proof, not product readiness, and not merge readiness.

evidence_scope:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-external-visual-judge-config-resolution-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-visual-judge-config-checklist-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-local-offline-visual-scaffold-packet-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-runtime-design-proof-resolver-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-proof-task-scenario-queue-2026-06-29.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-packet7-waiver-record-2026-06-28.md`

count_claimed:
- 1 collector file created.
- 6 source reports read.
- 0 external AI calls.
- 0 secret values read, printed, copied, stored, inferred, or checked.
- 0 runtime/browser/test/build commands.
- 0 git/PR/branch actions.

independent_evidence_count:
- 8 current-turn records expected for this collector: six source report readbacks, target nonexistence check before creation, and post-write literal/readback verification.

subagent_id_or_NONE:
- delegated-bounded-sub-agent-executor-current-session

downgrade_needed:
- No for the narrow claim that this owner-decision collector was created.
- Yes for any claim that external visual judging, visual fidelity, source-design fidelity, runtime/browser behavior, product readiness, or merge readiness is proven.

## Safe Default

Safe default:

`no external AI call`

Meaning:
- No Claude, OpenAI, Gemini, or other provider call may happen until Luba supplies the decisions below.
- No screenshots, rendered HTML/CSS, DOM text, source design references, prompts, logs, metadata, or diffs may be sent to an external provider until data sharing is explicitly approved.
- No credential value may be requested, printed, copied, stored, inferred, or captured.
- A future worker may use only the approved `secret env var name`, never the secret value.
- Current state remains decision collection, not execution.

Required safety literal:

`no secrets captured`

## What Luba Must Choose

This is the owner decision set needed before a real external visual AI judge path can move beyond documentation.

Required literal:

`provider/account/model/endpoint/secret env var name`

| Decision | Luba must provide | Safe boundary |
| --- | --- | --- |
| Provider | Choose Claude, OpenAI, Gemini, another approved provider, local/offline, or waiver. | Provider choice only; no credential value. |
| Account/project | Name the approved account, organization, project, workspace, tenant, or non-secret alias. | Alias only; no login, password, token, cookie, or secret. |
| Model | Name the exact visual/image-capable model id to use. | Model id only; no secret. |
| Endpoint | Name endpoint alias or base URL without credentials. | No credential embedded in URL. |
| Secret env var name | Name the environment variable that will hold the credential. | Variable name only, never value. |
| Credential presence check | Decide whether a future worker may check that the named env var exists. | Presence-only check; value must not be printed or saved. |
| External-call permission | Decide whether a future worker may call the selected provider after preflight passes. | No call unless explicitly approved after decisions. |
| Data sharing approval | Name which artifact classes may leave the machine. | Must list allowed and excluded classes. |
| Artifact set | Name exact artifact paths allowed as judge inputs. | No path expansion by worker judgment. |
| Design references | Name exact design reference paths or explicitly waive source-reference comparison. | No provider account/session capture. |
| Rubric | Provide scoring categories and forbidden claims. | Rubric text or file path only. |
| Thresholds | Provide pass/fail score threshold, rounding, tie handling, and failure consequence. | Threshold only; no account data. |
| Cost cap | Provide maximum calls, tokens, dollars, timeout, and rate cap. | Stop when cap is reached. |
| Retry policy | Provide retry count, retry conditions, timeout, and backoff. | No unbounded retry. |
| Retention policy | Decide local and provider retention for prompts, responses, images, and logs. | Redaction required; no secret retention. |
| Output evidence path | Name the exact future report path. | Future worker writes only approved evidence path. |

## Provider Decision Rows

These are decision placeholders, not verified configured providers.

| Provider option | Account/project decision | Model decision | Endpoint decision | Secret env var name decision | Status now |
| --- | --- | --- | --- | --- | --- |
| Claude | `<CLAUDE_ACCOUNT_OR_WORKSPACE_ALIAS>` | `<CLAUDE_VISION_MODEL_ID>` | `<CLAUDE_ENDPOINT_ALIAS_OR_BASE_URL>` | `<CLAUDE_API_KEY_ENV_VAR_NAME>` | Not configured here. |
| OpenAI | `<OPENAI_PROJECT_OR_ORG_ALIAS>` | `<OPENAI_VISION_MODEL_ID>` | `<OPENAI_ENDPOINT_ALIAS_OR_BASE_URL>` | `<OPENAI_API_KEY_ENV_VAR_NAME>` | Not configured here. |
| Gemini | `<GEMINI_PROJECT_ALIAS>` | `<GEMINI_VISION_MODEL_ID>` | `<GEMINI_ENDPOINT_ALIAS_OR_BASE_URL>` | `<GEMINI_API_KEY_ENV_VAR_NAME>` | Not configured here. |
| Other approved provider | `<OTHER_PROVIDER_ACCOUNT_ALIAS>` | `<OTHER_PROVIDER_MODEL_ID>` | `<OTHER_PROVIDER_ENDPOINT_ALIAS_OR_BASE_URL>` | `<OTHER_PROVIDER_SECRET_ENV_VAR_NAME>` | Not configured here. |
| Local/offline | `<LOCAL_PROVIDER_OR_SCORER_ALIAS>` | `<LOCAL_SCORER_OR_STUB_ID>` | `<LOCAL_ENTRYPOINT>` | `<NO_SECRET_REQUIRED>` | Downgraded local scaffold only, if later approved. |
| Waiver | `<WAIVER_OWNER_DECISION>` | `<NOT_APPLICABLE>` | `<NOT_APPLICABLE>` | `<NO_SECRET_REQUIRED>` | Handoff accounting only; no visual proof. |

## Data Sharing Approval

Luba must explicitly approve what may be sent to the provider.

Candidate allowed artifact classes:
- screenshots;
- rendered HTML/CSS;
- DOM text;
- source design references;
- visual diffs;
- layout metadata;
- non-secret prompts;
- non-secret response summaries.

Candidate excluded artifact classes:
- secret values;
- tokens, cookies, passwords, bearer headers, private keys, session ids;
- credentials embedded in URLs;
- unapproved logs;
- unapproved traces;
- account/session screenshots;
- private data not required for visual comparison.

No artifact class is approved by default.

## Rubric And Thresholds

Luba must choose the rubric before any external judge execution.

Suggested rubric fields:
- layout and spacing fidelity;
- typography fidelity;
- color fidelity;
- component/content presence;
- responsive viewport behavior;
- interaction state if approved;
- source design reference match;
- visible regression severity.

Suggested threshold fields:
- pass score, for example `<minimum_score_out_of_100>`;
- per-category minimums, if any;
- critical-failure conditions;
- rounding policy;
- whether a single critical mismatch fails the run;
- whether the judge result is advisory or release-gating.

Forbidden upgrades:
- Do not claim product readiness from a visual judge result alone.
- Do not claim merge readiness from a visual judge result alone.
- Do not claim runtime/browser readiness unless a separate runtime/browser path actually ran.
- Do not claim source-design fidelity beyond the approved artifact/reference/rubric scope.

Required literal:

`visual fidelity proof is a task not a stop blocker`

## Cost, Retry, And Retention

Luba must choose operational limits before execution.

| Policy | Required choice |
| --- | --- |
| Cost cap | Maximum dollars or provider credits for one run. |
| Call cap | Maximum external calls and maximum image comparisons. |
| Token cap | Maximum input/output token budget if applicable. |
| Timeout | Per-call and whole-run timeout. |
| Retry count | Maximum retry attempts. |
| Retry condition | Which errors may be retried: timeout, 429, transient 5xx, provider-specific safe retry. |
| Backoff | Exact backoff or no-backoff policy. |
| Local retention | Whether local prompts/responses/images/logs may be saved, redacted, or deleted. |
| Provider retention | Whether provider retention/training/default policy is approved. |
| Redaction | What fields must be removed from evidence and logs. |

Default until chosen:
- max external calls: 0;
- max cost: 0;
- retry count: 0;
- retention: no external request/response exists;
- no provider upload;
- no external AI call.

## Artifact Set

Luba must choose exact inputs and outputs.

Required input decisions:
- rendered artifact path(s);
- screenshot path(s), if screenshots are approved;
- source design reference path(s), or explicit reference waiver;
- DOM/text path(s), if approved;
- metadata/diff path(s), if approved;
- rubric path or inline rubric;
- output evidence path.

Current safe state:
- No artifact path is approved for external upload by this collector.
- Existing saved-output/local scaffold reports may guide future choices, but they are not external visual judge proof.
- A local/offline metadata-only scaffold is not a substitute for full external visual judge proof unless Luba explicitly accepts that downgrade.

## Current Classification

External AI visual judge:

`PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`

Reason:
- Provider is not selected in this collector.
- Account/project is not selected.
- Model is not selected.
- Endpoint is not selected.
- Secret env var name is not selected.
- Data sharing approval is not selected.
- Artifact set, design references, rubric, thresholds, cost cap, retry policy, retention policy, and output evidence path are not selected.

Local/offline scaffold:

`PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

Reason:
- A local/offline path needs exact command or script entrypoint, exact artifact set, pass/fail boundary, output evidence path, and execution permission.
- It remains downgraded as deterministic or metadata-only unless a stronger approved proof path is executed.

Waiver:

`DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`

Reason:
- Waiver can record that the external service is unconfigured for handoff accounting.
- Waiver does not prove external visual judge success or visual fidelity.

Required literals:

`Proof readiness items are tasks, not stop blockers`

`visual fidelity proof is a task not a stop blocker`

## Future Bounded Worker Order After Decisions

Use this only after Luba supplies the owner choices above.

Worker:
- `bounded-external-visual-judge-preflight-worker`

Allowed action:
- Read this collector and the approved owner decision packet/config.
- Verify non-secret completeness: provider, account/project, model, endpoint, secret env var name, data sharing approval, artifact set, rubric, thresholds, cost cap, retry/retention policy, and output path.
- If Luba explicitly approved it, check that the named credential environment variable exists without printing or storing its value.
- Verify only the exact approved artifact/design-reference paths.
- Write only the approved preflight report path.

Forbidden:
- no external AI call;
- no secrets captured;
- no secret value output;
- no runtime/browser/tests/builds;
- no git/PR/branch mutation;
- no artifact upload;
- no provider/account/model guessing.

Stop condition:
- Missing provider, account/project, model, endpoint, secret env var name, data sharing approval, artifact set, rubric, threshold, cost cap, retry policy, retention policy, or output path returns `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` or `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

Follow-up after preflight PASS and explicit execution approval:

Worker:
- `bounded-external-visual-judge-execution-worker`

Allowed action:
- Execute only the approved provider/model/endpoint request set.
- Upload only approved artifact classes and exact paths.
- Stay within approved retry, retention, rate, token, and cost caps.
- Write only the approved evidence report.
- Redact secrets and record that no secret values were captured.

Execution stop condition:
- Missing credential, provider error, rate/cost cap, unapproved artifact, data-sharing mismatch, rubric mismatch, threshold mismatch, or output-path drift stops execution and records a downgraded result.

## Owner Decision Form

Luba can fill this form later. Values here are placeholders, not current approvals.

```yaml
external_visual_judge_owner_decisions:
  provider: <Claude | OpenAI | Gemini | other approved provider | local/offline | waiver>
  account_or_project_alias: <non-secret alias>
  model: <exact visual-capable model id>
  endpoint: <endpoint alias or base URL without credentials>
  secret_env_var_name: <environment variable name only>
  credential_presence_check_allowed: <true | false>
  external_ai_call_allowed: <true | false>
  data_sharing_approval:
    allowed_artifact_classes:
      - <screenshots | rendered_html_css | dom_text | design_references | metadata | diffs | other>
    excluded_artifact_classes:
      - <secrets | credentials | cookies | tokens | private_logs | unapproved_traces | other>
  artifact_set:
    rendered_artifacts:
      - <exact path>
    screenshots:
      - <exact path or none>
    design_references:
      - <exact path or WAIVED_BY_LUBA>
    rubric: <inline rubric or exact rubric path>
    output_evidence_path: <exact future report path>
  thresholds:
    pass_threshold: <exact numeric or categorical threshold>
    per_category_minimums: <exact values or none>
    rounding_policy: <exact policy>
    critical_failure_policy: <exact policy>
  cost_retry_retention:
    max_cost_usd: <number>
    max_external_calls: <integer>
    max_tokens: <integer or not applicable>
    timeout_seconds: <integer>
    retry_limit: <integer>
    retry_conditions: <exact conditions>
    retention_policy: <none | redacted local | provider default approved | other exact policy>
    redaction_policy: <exact policy>
```

## Final Boundary

This file is a human-readable decision collector only.

It records:
- what Luba must choose;
- why the safe default is no external AI call;
- why no secrets should be captured;
- what future bounded worker should do after decisions exist;
- why visual fidelity proof is a task, not a final stop.

Class C yes-no:
- yes, this is a Class C owner-decision item for external provider/account/model/endpoint/secret-env-var-name/data-sharing/rubric/threshold choices.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-external-visual-judge-owner-decision-collector-2026-06-29.md`

verification:
- Post-write verification should read this file and check the exact required literals.
- No external AI, no secrets, no runtime/browser/tests/builds, and no git/PR actions should be inferred from this document.
