# Row27 independent review

Verdict: NEEDS_FIX

Review scope: target Row27 execution folder only, plus exact production artifact paths named in rollback/preflight for hash verification. I did not run product/runtime/UI/tests/eval/heldout/training. I did not commit or push.

## Honesty guard

claim_scope: independent Row27 execution packet review
evidence_scope: target folder JSON/MD files; exact rollback/preflight-named production artifact hashes only
count_claimed: 7 required checks evaluated, 1 packet-quality finding
independent_evidence_count: 8 command outputs
subagent_id_or_NONE: Beauvoir the 2nd / 019f05cd-53a3-74d3-ab36-d6df7b7aef57
downgrade_needed: yes; core required checks passed, but packet markdown report has unresolved placeholders

## Required checks

1. All JSON in target folder parses: PASS.
   Parsed 9 JSON files with ConvertFrom-Json: checkpoint-readback.json, preflight.json, reload-result.json, rollback-proof.json, row27-execution-report.json, row27-ui-blocker.json, temp-checkpoint.json, temp-locator.json, temp-locator-readback.json.

2. temp-checkpoint hash equals reload/readback observed hash: PASS.
   Actual temp-checkpoint SHA-256: bf7d3b7bea07d9e422c2eee7a7cace7017ea5cad3316534e73d9997e37e9ee33.
   checkpoint-readback, temp-locator, temp-locator-readback, reload expected, and reload observed all record the same hash.

3. temp-locator points to temp-checkpoint path: PASS.
   temp-locator target path equals preflight temp_checkpoint_path:
   docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332/temp-checkpoint.json.

4. rollback-proof has before/after hashes and they match for named production artifacts: PASS.
   Recomputed exact named artifact hashes and confirmed actual == before == after for:
   - artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/p17e-manifest-locator-map.json
   - artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-checkpoint.json
   - artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-manifest.json

5. git diff --check -- <target folder>: PASS.
   Command exited 0 with no output.

6. Claims locked false: PASS.
   row27-execution-report.json has false for product_readiness, real_promotion, production_locator_update, semantic_quality, training, token_savings, host_ui_readiness.
   Related temp/UI files also keep product/readiness/promotion/semantic/training claims false where present.

7. UI path blocker and no fake UI pass: PASS with scope note.
   row27-ui-blocker.json says status=BLOCKED_FOR_UI_ONLY, host_ui_route_found=false, ui_execution_ran=false.
   row27-execution-report.json says exact_codex_host_route_found=false and ui_execution_ran=false.
   I did not search outside the target folder for a host route because the allowed-read scope did not permit it. The evidence supports only: the target packet declares no exact host route and does not fake a UI pass.

## Finding

NEEDS_FIX: row27-execution-report.md contains unresolved PowerShell placeholders in evidence lines.

Evidence:
- line 7 contains $(System.Collections.Specialized.OrderedDictionary.files.temp_checkpoint)
- line 8 contains $tempCheckpointHash
- line 9 contains $(System.Collections.Specialized.OrderedDictionary.files.temp_locator)
- line 10 contains unresolved expected/observed placeholder expressions

Impact: JSON evidence is internally consistent and independently verified, but the human-readable markdown execution report is not a clean evidence artifact. It should be regenerated or corrected from the verified JSON values before the packet is accepted as PASS.

## Files changed by this review

- docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332/review-20260627-002113/row27-review.md
- docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332/review-20260627-002113/row27-review.json

## Commands run

- Get-ChildItem -LiteralPath <target folder> -Force
- Get-ChildItem -LiteralPath <target folder> -Recurse -Filter *.json -File
- ConvertFrom-Json over all target-folder JSON files
- git diff --check -- docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-20260627-001332
- Get-FileHash SHA256 for temp-checkpoint.json
- Get-FileHash SHA256 for the three rollback/preflight-named production artifacts
- Select-String for unresolved placeholders in row27-execution-report.md
