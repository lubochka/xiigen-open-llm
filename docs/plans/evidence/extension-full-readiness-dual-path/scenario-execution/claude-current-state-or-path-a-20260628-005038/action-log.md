# Action log

claim_scope: Claude scenario coverage current state plus corrected Path A direct Claude API/core boundary fallback.
evidence_scope: prep JSON read, existing Claude execution evidence searched, earlier adapter/core run reviewed as partial only, corrected direct core API rerun executed once, safe local window inventory retained.
count_claimed: 1 scenario, 1 corrected Path A rerun, 1 safe UI preflight.
independent_evidence_count: 4 evidence records: corrected request/response/trace, runner exit output, window-inventory, final checks.
subagent_id_or_NONE: bounded_subagent_current
downgrade_needed: no for direct Claude API/core boundary fallback; yes for live Claude host and Path B.

## Fix-loop steps

1. Parent review found the previous canonical `request.json` used `HostName=Codex` and `HostType=codex`, while only `HostFacts.host=Claude`.
2. I treated the previous state as `partial_fallback_only_not_accepted_for_clean_claude_path_a`.
3. I verified `tools/xiigen-extension-core-api.ps1` reads host fields directly from request JSON and writes them to trace.
4. I generated a corrected `request.json` from the Claude prep prompt using Node UTF-8 parsing, with `HostName=Claude` and `HostType=claude`.
5. I ran the local core API script directly against the corrected request.
6. Corrected rerun exit code was 0.
7. Corrected `response.json` reports success true and `core_boundary_executed=true`.
8. Corrected `trace.json` reports `host_name=Claude`, `host_type=claude`, `external_ai_used=false`, `training_started=false`, and `raw_private_prompt_persisted=false`.

## Existing Path B status

Path B remains blocked at route proof. The safe preflight found one candidate process `claude`, but no prompt was typed, no submit clicked, and no screenshots were created.

## Tool failures recorded from earlier packet creation

- UI preflight attempt 1 failed because `$PID` is a read-only PowerShell variable.
- UI preflight attempt 2 failed during candidate list serialization before writing output.
- UI preflight attempt 3 succeeded.
- First fix-loop request generation used an unnecessary PowerShell encoding repair and produced question marks in the prompt; I rejected that rerun and replaced it with Node UTF-8 request generation before accepting Path A.

## No-go actions

- No code edits.
- No plan/governance/skill edits.
- No commit.
- No push.
- No secrets typed or captured.
