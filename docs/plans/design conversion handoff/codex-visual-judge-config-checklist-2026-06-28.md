# Codex Visual Judge Config Checklist - 2026-06-28

claim_scope:
- Created this bounded doc-only visual-judge configuration checklist.
- Used the packet execution sweep evidence for Packet Group 7 external AI visual judge.
- Performed a targeted provider-name search for `Claude`, `OpenAI`, and `Gemini` in handoff markdown before drafting.
- No external AI call was made.
- No secret value was read, requested, printed, stored, or inferred.
- This checklist is not product completion, not visual-fidelity proof, and not runtime proof.

evidence_scope:
- Source sweep file read: `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-packet-execution-sweep-2026-06-28.md`.
- Targeted provider-name search scope: markdown files under `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff`.
- The provider-name search returned no handoff markdown hits for `Claude`, `OpenAI`, or `Gemini` in this worker run.
- Claude/OpenAI/Gemini are therefore listed below as candidate provider decisions requested by the current work order, not as verified configured providers or accounts.

count_claimed:
- 1 checklist file created.
- 1 packet sweep file read.
- 1 targeted provider-name search run.
- 0 external AI calls.
- 0 secret reads.
- 0 git actions.

independent_evidence_count:
- 1 source packet readback.
- 1 provider-name search result.
- 1 target existence check before creation.

subagent_id_or_NONE: visual-judge-config-checklist-worker

downgrade_needed:
- No for the narrow claim that this checklist was created.
- Yes for external visual judging, visual-fidelity proof, product completion, provider/account availability, credential availability, and runtime proof.

Required literals:
- `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`
- `DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`

## Boundary

This is a safe configuration checklist only.

The packet execution sweep blocked Packet Group 7 with `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` because external visual judging was missing provider/account/credential inputs, data-sharing approval, design references if required, rubric, and threshold.

No external AI call was made. No local/offline visual judge was run. No browser, runtime, test, build, upload, PR mutation, stage, commit, push, cleanup, archive, restore, stash, or delete action was performed for this checklist.

Default rule until Luba supplies and authorizes a configured provider path:

`DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`

Meaning: if no external visual judge provider/account/credential convention is supplied, external visual judging remains waived as an unconfigured external service for handoff accounting only. This does not prove product completion, source-design fidelity, runtime readiness, visual accuracy, or merge readiness.

## Missing Owner Decisions

The future visual-judge worker needs exact owner decisions for every item below before any external provider can be called.

| Decision | Required owner input | No-secret boundary |
| --- | --- | --- |
| Provider family | Choose one configured path, such as Claude, OpenAI, Gemini, local/offline judge, or explicit waiver. | Provider choice only; no API key value. |
| Account/workspace | Identify approved account, project, workspace, or tenant label. | Label or alias only; no login, password, token, cookie, or account secret. |
| Endpoint/base URL | Provide approved endpoint alias or documented provider endpoint. | Endpoint metadata only; no credential in URL. |
| Model name | Provide exact model identifier allowed for visual judging. | Model id only; no secret. |
| Credential convention | Provide approved environment variable name or secret manager reference name. | Variable/reference name only; never provide the value. |
| Credential availability | Confirm whether the future worker may check for presence of the configured credential. | Presence check only; no echoing or printing secret contents. |
| Data-sharing approval | Approve or deny uploading screenshots, DOM text, design references, and rubric content to the chosen provider. | Must state allowed artifact classes; no secrets or private credentials. |
| Artifact set | Name exact screenshot/report/design-reference file paths allowed for judging. | Artifact paths only; worker must not expand scope without approval. |
| Design reference input | Provide exact reference image, Figma export, spec, or waive source-design comparison. | Reference artifact only; no account token or private session capture. |
| Rubric | Provide scoring categories, e.g. layout, spacing, typography, color, content presence, responsive behavior. | Rubric text only; no secrets. |
| Pass threshold | Provide numeric or categorical pass criteria and what happens on fail. | Threshold only; no external account data. |
| Retention/logging policy | Decide whether provider request/response metadata may be saved and where. | Metadata only; no secret payloads. |
| Retry/budget limit | Decide max attempts, timeout, and budget ceiling. | Operational limits only; no billing credential. |
| Waiver policy | Decide whether external judging is waived when provider is unconfigured. | Use `DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE` unless explicitly overridden. |

## Candidate Provider Decision Rows

These rows are decision placeholders. This worker did not verify that any provider account exists or is configured.

| Candidate | Required before use | Status in this checklist |
| --- | --- | --- |
| Claude | Provider approval, account/workspace alias, endpoint/model, credential environment variable name, data-sharing approval, artifact/rubric/threshold inputs. | Not configured here; no account or secret verified. |
| OpenAI | Provider approval, project/account alias, endpoint/model, credential environment variable name, data-sharing approval, artifact/rubric/threshold inputs. | Not configured here; no account or secret verified. |
| Gemini | Provider approval, project/account alias, endpoint/model, credential environment variable name, data-sharing approval, artifact/rubric/threshold inputs. | Not configured here; no account or secret verified. |
| Local/offline judge | Tool/script path, install/runtime permission, exact command, input artifacts, rubric, threshold, output evidence path. | Not configured here; no tool discovery or runtime was run. |
| Waiver | Explicit owner waiver or default waiver for unconfigured external service. | Allowed only as handoff accounting; not product completion. |

## No-Secret Config Convention Fields

Future config may use fields like these, but values are intentionally omitted here.

```yaml
visual_judge:
  provider_family:
  account_alias:
  project_or_workspace_alias:
  endpoint_alias:
  model_id:
  credential_env_var_name:
  credential_presence_check_allowed:
  data_sharing_approval:
  allowed_artifact_paths:
  design_reference_paths:
  rubric_path_or_text:
  pass_threshold:
  retry_limit:
  timeout_seconds:
  budget_limit:
  retention_policy:
  save_request_metadata:
  save_response_metadata:
  output_evidence_path:
  unconfigured_service_policy: DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE
```

Rules for this convention:
- Store only names, aliases, paths, booleans, and policy labels.
- Do not store API keys, tokens, cookies, passwords, bearer headers, session IDs, private URLs containing credentials, or copied secret-manager values.
- Do not print credential values during verification.
- A future worker may only check whether an approved credential variable/reference exists if Luba explicitly authorizes that presence check.

## Exact Future Bounded Work Order

Use this only after Luba supplies or authorizes a provider and no-secret credential convention.

Worker: bounded-visual-judge-execution-worker

Allowed read-only scope:
- This checklist file.
- The approved visual proof artifacts named by Luba.
- The approved design reference artifacts named by Luba.
- The approved rubric/threshold file or text named by Luba.

Allowed secret boundary:
- Read only the approved credential environment variable name or secret reference name.
- If explicitly authorized, check credential presence without printing, copying, saving, or exposing the credential value.

Allowed external action:
- Call only the selected provider/model/endpoint approved by Luba.
- Upload only the approved artifact classes and paths.
- Save only the approved non-secret response/evidence metadata to the approved output path.

Required verification:
- Confirm selected provider, endpoint/model, allowed artifact paths, rubric, threshold, and output path.
- Confirm no secret values were printed or saved.
- Report pass/fail against the approved threshold.
- Preserve boundary if the service is unconfigured: `DEFAULT_WAIVE_EXTERNAL_UNCONFIGURED_SERVICE`.

Forbidden:
- No provider/account guessing.
- No secret discovery beyond approved presence check.
- No browser/runtime/test/build unless separately authorized.
- No product/source repo edits.
- No staging, commit, push, PR mutation, branch/ref mutation, cleanup, archive, restore, stash, or delete.

Stop condition:
- If provider, account, credential convention, data-sharing approval, artifacts, rubric, or threshold is missing, return `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` and do not call external AI.

## Final Boundary

This checklist is documentation only.

It does not complete Packet Group 7. It only converts the missing external visual judge inputs into an owner-decision checklist and future bounded work order.

No external AI call was made.

No git actions were performed.

must_stop_after_report=true
