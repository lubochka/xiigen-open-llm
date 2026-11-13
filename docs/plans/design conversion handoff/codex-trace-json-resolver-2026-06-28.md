# TRACE_JSON_RESOLVER - 2026-06-28

claim_scope:
- Bounded trace JSON resolver packet for follow-up from `codex-preflight-resolution-sweep-2026-06-28.md`.
- Classified safe doc-only trace follow-up actions versus blocked trace generation, replay, mutation, import/export, publication, archive, cleanup, commit, and evidence mutation paths.
- Produced no trace JSON generation, no trace replay, no evidence mutation.

evidence_scope:
- Read `codex-preflight-resolution-sweep-2026-06-28.md`.
- Read handoff/index trace follow-up references from `codex-designconversion-handoff-index-2026-06-28.md` and `codex-designconversion-completion-handoff-2026-06-28.md`.
- Read trace-specific packet context from `codex-approval-ready-execution-packets-2026-06-28.md`.
- Read trace quarantine references from `codex-core-evidence-residue-resolver-2026-06-28.md` and `codex-core-evidence-dirt-manifest-2026-06-28.md`.
- Did not read, inspect, generate, modify, replay, import, export, redact, archive, delete, commit, stage, or publish any trace JSON contents.

count_claimed:
- resolver packet files created: 1
- trace JSON files inspected: 0
- trace JSON files generated: 0
- trace replays executed: 0
- evidence JSON files generated or mutated: 0
- repo/git/test/build/browser/runtime actions executed: 0

independent_evidence_count:
- 5 read-only document evidence sources used for trace routing.
- 0 trace-content evidence records.

subagent_id_or_NONE:
- TRACE_JSON_RESOLVER

downgrade_needed:
- Yes for any broader claim. This packet supports only doc-only resolver routing. It does not prove trace sensitivity, trace validity, replay correctness, import/export readiness, product readiness, clean repo, merge readiness, or safe publication.

Required boundary literals:
- `TRACE_JSON_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no trace JSON generation`
- `no trace replay`
- `no evidence mutation`

## Source Boundary

The preflight sweep keeps Packet Family 6 in conservative/default state:

- packet family: Root Trace JSON.
- default selector: `defer-sensitivity-review-required`.
- named targets:
  - `xiigen-codex-request.json`
  - `xiigen-codex-response.json`
  - `xiigen-codex-trace.json`
- default action: preserve as unresolved sensitivity decision.
- blocked actions: sensitivity inspection, redaction, archive, clean/delete, waive, stage, commit, push, or publish until exact selectors and policies are supplied.

The handoff/index references only a future `bounded-trace-json-worker` after sensitivity/no-secret-print policy and selected trace action are supplied.

## Safe Doc-Only Actions

These actions are safe under the current evidence boundary because they do not touch trace JSON contents, repos, runtime, or external systems:

| Action | Current status | Reason |
| --- | --- | --- |
| Document current trace decision state | Safe | This packet records that trace files remain blocked by preflight failure for non-default action. |
| List required selectors before future work | Safe | Selector lists do not require reading or mutating trace JSON contents. |
| Preserve no-op/defer default | Safe | No file content, repo state, runtime state, or external service is changed. |
| Draft future bounded work orders | Safe | Work orders are instructions only and remain blocked until Luba supplies missing selectors and approvals. |
| Re-state no-secret/no-content-print boundary | Safe | This reduces accidental disclosure risk without inspecting secrets. |

Safe doc-only result:
- `TRACE_JSON_DOC_ONLY_DEFER_SENSITIVITY_REQUIRED`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` remains active for every non-doc trace action.
- no trace JSON generation.
- no trace replay.
- no evidence mutation.

## Blocked Trace Actions

All actions below remain blocked until exact selectors, safety policy, and write/run authority are supplied:

| Action family | Status | Missing selector or approval |
| --- | --- | --- |
| Trace content inspection | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | sensitivity inspection approval, no-secret-print rule, exact file set, redaction/report policy. |
| Trace JSON generation | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | generator command or script, source inputs, destination path, schema/version, redaction policy, artifact-write permission. |
| Trace replay | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | replay target, runtime/service permission, exact command, expected outputs, no-network/no-secret policy, evidence destination. |
| Trace import | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | import destination, schema compatibility rule, overwrite/merge behavior, backup policy, validation command. |
| Trace export | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | export destination, redaction policy, allowed fields, publication/sharing boundary, checksum/manifest requirement. |
| Trace redaction | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | sensitive-field rules, allowed replacement format, destination file policy, review/verification method. |
| Trace archive outside repo | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | archive path/account, redaction policy, checksum manifest policy, source retention/delete policy. |
| Trace cleanup/delete | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | archive/backup prerequisite, exact delete list, restore path, post-delete verification. |
| Trace commit/stage/push | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | sensitivity pass, exact files, commit message, branch/PR policy, push approval. |
| Evidence JSON generation or mutation | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | explicit evidence artifact path, schema, writer authority, non-secret content policy, verification command. |

Current packet decision:
- Trace files remain quarantine evidence candidates.
- no trace JSON generation is allowed by this packet.
- no trace replay is allowed by this packet.
- no evidence mutation is allowed by this packet.

## Decisions Required Before Trace Generation

Before any future worker may generate trace JSON, Luba must provide all of:

1. Exact trace purpose: diagnostic, replay fixture, regression evidence, audit archive, import/export bridge, or other named purpose.
2. Exact source inputs: whether generation may use existing request/response/trace files, runtime logs, saved output evidence, product runtime, or only synthetic fixtures.
3. Exact generator entrypoint: command, script path, or callable function, including working directory and expected output filenames.
4. Exact output destination: repo path or outside-repo archive path, plus whether new JSON evidence files may be created.
5. Schema/version selector: expected trace JSON schema, required fields, optional fields, and validation method.
6. Sensitivity policy: whether secrets/private prompts/private responses/account identifiers/local paths may appear, how to redact them, and who reviews the result.
7. No-secret-print rule: what may be reported in chat and what must remain only in local redacted reports.
8. Mutation policy: whether output files may overwrite existing files, create siblings, or only write to a fresh approved directory.
9. Verification policy: exact read-only or write-producing checks allowed after generation.

Without those decisions:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- no trace JSON generation.

## Decisions Required Before Trace Replay

Before any future worker may replay traces, Luba must provide all of:

1. Exact replay trace file set and whether existing `xiigen-codex-*.json` contents may be inspected.
2. Replay target: local script, adapter, service, extension runtime, browser path, or external system.
3. Runtime permission: whether tests/builds/browser/runtime/service start are allowed.
4. Network policy: offline-only, localhost-only, or named external endpoint.
5. Secret policy: whether credentials are required; if so, the work must stop and request a secure credential path rather than printing or guessing secrets.
6. Expected behavior: exact success/failure output, logs, status codes, UI state, or validation record.
7. Evidence destination: exact allowed files/directories and whether any JSON evidence may be written.
8. Replay safety: whether replay may mutate repo, database, service state, cache, user-data, or external account state.
9. Stop conditions: schema mismatch, missing files, suspected secrets, unexpected network call, runtime failure, or output path collision.

Without those decisions:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- no trace replay.

## Future Bounded Work Orders

### Future Work Order A - Trace Sensitivity Triage

Worker: `bounded-trace-json-sensitivity-worker`

Allowed only after Luba supplies sensitivity inspection approval and no-secret-print policy.

Scope:
- Inspect exactly selected trace files.
- Do not print secrets or private contents.
- Produce a redacted Markdown sensitivity report only if a write path is explicitly approved.
- Recommend one of: commit, archive, redact, cleanup, waive, or defer.

Still blocked:
- redaction writes, archive, cleanup/delete, stage, commit, push, replay, import/export, external upload.

### Future Work Order B - Trace Archive Or Cleanup

Worker: `bounded-trace-json-archive-cleanup-worker`

Allowed only after Luba supplies:
- exact selected trace files;
- archive destination or cleanup/delete approval;
- backup/restore policy;
- redaction policy;
- post-action verification command.

Scope:
- Copy or delete only approved files.
- Return checksums/status only if authorized.
- Do not replay or regenerate traces.

### Future Work Order C - Trace Generation

Worker: `bounded-trace-json-generation-worker`

Allowed only after Luba supplies all generation decisions listed above and explicit artifact-write permission.

Scope:
- Run exactly approved generator entrypoint.
- Write only exact approved output file(s).
- Validate only with approved command.
- Return redacted summary and exact file paths.

Blocked until then:
- no trace JSON generation.
- no evidence mutation.

### Future Work Order D - Trace Replay

Worker: `bounded-trace-json-replay-worker`

Allowed only after Luba supplies all replay decisions listed above and explicit runtime/test/browser/service permissions if needed.

Scope:
- Re-read metadata for selected trace files.
- Replay exactly selected trace through exactly selected target.
- Write only approved evidence artifacts.
- Stop on suspected secrets, schema mismatch, unexpected mutation, or output path collision.

Blocked until then:
- no trace replay.
- no evidence mutation.

### Future Work Order E - Trace Import/Export

Worker: `bounded-trace-json-import-export-worker`

Allowed only after Luba supplies:
- import/export direction;
- source and destination paths;
- schema compatibility rule;
- redaction/export field policy;
- overwrite/merge behavior;
- validation command;
- publication or no-publication boundary.

Scope:
- Transform only approved trace artifacts.
- Do not call external services unless explicitly approved.
- Do not commit, push, or publish unless separately approved.

## Resolver Packet Decision

Decision:
- Keep trace JSON follow-up as doc-only defer.
- Preserve current quarantine/sensitivity boundary.
- Require future exact selectors before any trace generation, replay, import/export, evidence mutation, archive, cleanup, stage, commit, push, or publication.

Status:
- `TRACE_JSON_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- no trace JSON generation
- no trace replay
- no evidence mutation

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-trace-json-resolver-2026-06-28.md`

verification:
- Created this Markdown resolver packet only.
- Did not run tests, builds, browser checks, runtime probes, git mutations, PR mutations, external AI, trace generation, trace replay, trace content inspection, or JSON evidence mutation.
