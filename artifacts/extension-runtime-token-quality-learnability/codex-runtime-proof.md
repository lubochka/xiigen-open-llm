# RUNTIME-04 Codex Runtime Proof

## Verdict

Classification: `IN_PROGRESS_NARROW_EVIDENCE_ONLY`

This proves a fresh Codex `exec` runtime process loaded the Codex environment and, from inside that runtime, invoked the XIIGen Codex thin adapter source entrypoint for a bounded smoke scenario. It does not prove full Codex plugin/chat UI invocation, token savings, quality improvement, learnability, training, checkpoint mutation, rollback, or full product readiness.

## Command And Target

- Phase: `RUNTIME-04`
- Sub-agent: `RUNTIME-04`
- Scenario id: `RUNTIME-04-CODEX-EXEC-SKILL-SELECTION-SMOKE-20260616-215139`
- Correlation id: `RUNTIME04-CODEX-20260616-215139`
- Negative correlation id: `RUNTIME04-CODEX-20260616-215139-NEGATIVE`
- Host/app version: `codex-cli 0.140.0-alpha.2`
- Target type: `codex_exec`
- Why this target: local `codex plugin` help exposes marketplace management only, and `codex plugin list` does not show `codex-xiigen-smart-skills` as an installed/enabled plugin. The available approved runtime path is therefore Codex CLI `exec`, with the XIIGen source entrypoint invoked from inside the Codex runtime.
- Command/entrypoint invoked: `codex.exe exec --json --dangerously-bypass-approvals-and-sandbox -C C:\Projects\codex-xiigen-smart-skills --add-dir C:\Projects\llm_mvp_core -o <run>\codex-last-message.txt -`
- Nested adapter entrypoint: `node src/codex-entrypoint.js --method skill-selection --debug-enabled --correlation-id RUNTIME04-CODEX-20260616-215139 ...`
- Timestamp: `2026-06-16T21:51:39+03:00` launch; core trace completed `2026-06-16T18:56:11.6052697Z`.

## Input And Output Evidence

- Raw private input ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\positive\request.json`
- Raw private input status: synthetic redacted smoke prompt only; no private corpus, secret, or user data was used.
- Redacted input text: `RUNTIME-04 redacted Codex exec smoke: choose skills for a small planning request.`
- Raw output ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\positive\response.json`
- Redacted visible output ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\positive\visible-answer.txt`
- Codex exec JSONL transcript: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\codex-exec.stdout.jsonl`
- Codex exec stderr log: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\codex-exec.stderr.log`
- Fresh nested summary: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\runtime04-nested-summary.json`

Visible adapter answer:

```text
Core xiigen boundary executed. Semantic quality is not claimed.
xiigen skill-selection :
input: {"method":"skill-selection","hostName":"Codex","hostType":"codex","correlationId":"RUNTIME04-CODEX-20260616-215139","userPromptSha256":"E6937DA5C3BA352EEB979B93EC4984027BC5EDA909E359D1A6B3E3BB0729CAD4","userPromptOrTaskText":"[redacted]","requestedSkills":["planning"],"availableSkillCount":3,"hostFactKeys":["runtime_phase","target_type"],"checkSetInputKeys":["full_plugin_ui_evidence","proof_scope"],"provenanceKeys":["entrypoint","host","package","runner","source"]}
output: {"success":true,"status":"core_boundary_partial_no_semantic_quality_claim","selectedSkills":["plan-review"],"semanticQualityClaimed":"false","runtimeExtensionClaimed":false}
```

Visible Codex final answer status: unavailable. The Codex exec process produced runtime artifacts and a transcript, but the outer command timed out after 300 seconds before `codex-last-message.txt` was written. This is why the classification remains narrow evidence only.

## Debug Trace

- Debug marker: `xiigen skill-selection :`
- Debug trace ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\positive\trace.json`
- Redaction result: debug input and output are redacted in the visible display; `raw_private_prompt_persisted=false`, `contains_secrets=false`.

## Negative Path

Controlled negative/error-path invocation used a missing core script:

- Negative request ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\negative\request.json`
- Negative stderr ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\negative\entrypoint.stderr.log`
- Negative visible answer ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\negative\visible-answer.txt`
- Negative exit code: `4294770688`
- Negative response/trace status: intentionally absent because PowerShell rejected the missing `-File` target before the core script could run.

Error text:

```text
The argument 'C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\negative\missing-core-api.ps1' to the -File parameter does not exist.
```

## Core Boundary

Positive trace proves the core boundary handled the request:

```text
SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest
```

Trace ref: `C:\Projects\codex-xiigen-smart-skills\reports\RUNTIME-04-codex\20260616-215139\positive\trace.json`

## What This Proves

- A fresh Codex `exec` runtime was launched for RUNTIME-04.
- The Codex runtime executed commands inside `C:\Projects\codex-xiigen-smart-skills`.
- The XIIGen Codex thin adapter source entrypoint created a `XiigenExtensionRequest`.
- `RequestedSkills=["planning"]` and `AvailableSkills=["architect","planning","plan-review"]` reached the request after the parser repair.
- `XiigenExtensionApiService.ProcessHostExtensionRequest` handled the positive request.
- A visible redacted XIIGen answer and debug marker were produced.
- A controlled missing-core negative path was invoked and recorded.

## What This Does Not Prove

- Full Codex plugin/chat UI invocation.
- A clean Codex final visible answer file via `--output-last-message`.
- Token savings.
- Quality improvement.
- Semantic quality.
- Local learnability, DPO, training, fresh-load, checkpoint mutation, rollback, target promotion, or common model readiness.
- Full cross-host product readiness.

## Next Independent Track

- `RUNTIME-04-RERUN`: run a tighter Codex runtime proof that either obtains a clean `codex-last-message.txt` visible final answer or uses a supported installed Codex plugin/app invocation path if one becomes available. Keep the same no-overclaim firewall and do not use package smoke, dashboard/API, or direct adapter evidence as a full plugin/chat UI substitute.
