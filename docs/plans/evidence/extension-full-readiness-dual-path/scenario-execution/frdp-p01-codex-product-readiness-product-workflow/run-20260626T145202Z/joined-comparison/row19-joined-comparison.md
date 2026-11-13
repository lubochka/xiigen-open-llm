# Row19 joined same-scenario comparison

claim_scope: Row19 joined comparison of existing Path A adapter/core evidence vs existing Path B Codex UI evidence only.
evidence_scope: Read-only comparison of allowed Row19 run folder `run-20260626T145202Z`; no adapter/UI rerun, no code edit, no git, no training/eval/checkpoint/locator mutation.
count_claimed: 1 joined comparison pass package; 2 joined evidence files created.
independent_evidence_count: Path A request/trace/validation/report/command output plus targeted text search; Path B validation/report/readback/inventory plus targeted text search and 4 visual screenshot inspections.
subagent_id_or_NONE: bounded-sub-agent-comparator-ROW19
downgrade_needed: yes - accepted only as scoped workflow-boundary evidence; not product readiness.

## Verdict

`ROW19_JOINED_COMPARISON_PASS_PACKAGE_ROW19`

Row19 is scoped accepted for same-scenario Codex workflow-boundary comparison. The evidence supports that Path A and Path B refer to the same Row19 scenario/correlation and preserve the same non-mutating product-workflow request boundary. This is not a product readiness claim, not host readiness, not semantic-quality proof, not token-savings proof, not learnability proof, and not checkpoint/locator promotion evidence.

## Evidence Counts

| Count | Value |
|---|---:|
| Path A direct files read | 5 |
| Path A targeted text searches | 1 |
| Path B direct files read | 5 |
| Path B screenshots visually inspected | 4 |
| Path B targeted text searches | 1 |
| Adapter reruns by this comparator | 0 |
| UI reruns by this comparator | 0 |
| Code edits by this comparator | 0 |
| Git actions by this comparator | 0 |
| Training/eval/checkpoint/locator mutations by this comparator | 0 |
| Joined evidence files created | 2 |

## Criteria Results

| # | Criterion | Result | Evidence |
|---:|---|---|---|
| 1 | Same prompt/correlation id | PASS scoped | Path A request/trace/validation use scenario `frdp-p01-codex-product-readiness-product-workflow`, correlation `ROW19-CODEX-PRODUCT-WORKFLOW-001`, prompt hash `858fad70c310395640bbf0c0ab4aa04c78da2a03b3eb2684db4a6fc2e61996eb`. Path B prompt-before-submit screenshot visually shows Row19 prompt/scenario/correlation, and Path B UIA/readback contains the same scenario and correlation. |
| 2 | Path A adapter/core boundary | PASS | Path A command output reports exit code `0`. Trace reports endpoint `xiigen-extension-core-api`, call target `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`, `response_success=true`, `external_ai_used=false`, `training_started=false`, and claim scope limited to core-owned endpoint invocation. Validation reports prompt hash consistency pass and selected skill `xiigen-smart-skills`. |
| 3 | Path B visible Codex UI response | PASS scoped | Path B validation says active foreground Codex thread `Prepare Row19 product workflow`, same Row19 thread visible, route not lost, response captured. Screenshots show the prompt before submit and later foreground thread with `core_root`, mutation flags, external AI marker, and non-claims. Full `final_user_visible_response` is captured by UIA/readback with `offscreen=false`, not fully by screenshot. |
| 4 | Output contract comparison | PASS scoped | Path A proves installed-plugin adapter/core boundary and selected `xiigen-smart-skills`; it does not prove `client-adaptive-learning-loop` and marks it `not_proven`. Path B visible answer says `client-adaptive-learning-loop` is planned/expected, not a proven runtime-selected skill, and says files were not changed. This is a surface difference, not a blocking mismatch. |
| 5 | No mutation/training/eval/promotion/locator/git | PASS scoped | Path A trace/validation report no training, no external AI, no checkpoint/locator promotion/update, no git/code edit evidence. Path B validation/readback report no git, no code edits, no product mutation, no training/eval/DPO/checkpoint/locator mutation, and no product readiness claim. This comparator did not rerun or mutate anything. |
| 6 | Row19 verdict | PASS scoped | Same-scenario workflow-boundary evidence is sufficient for scoped Row19 package, with explicit non-claims. |

## Matches

- Same scenario id: `frdp-p01-codex-product-readiness-product-workflow`.
- Same correlation id: `ROW19-CODEX-PRODUCT-WORKFLOW-001`.
- Same user-level intent: non-mutating Row19 Codex product workflow for XIIGen Smart Skills check.
- Same non-claim boundary: no token savings, no semantic quality improvement, no learnability, no host/product readiness, no checkpoint promotion, no locator update.
- Both paths preserve non-mutation boundaries for training/eval/DPO/checkpoint/locator/git.
- Both paths keep `client-adaptive-learning-loop` below proven-runtime-selection status.

## Mismatches And Not-Proven Fields

- Path A selected `xiigen-smart-skills`; Path B user-visible response discusses `client-adaptive-learning-loop` as planned/expected. This is acceptable because Path A is adapter/core plugin-surface evidence and Path B is user-visible Codex routing text.
- Path A explicitly reports `client-adaptive-learning-loop: not_proven`.
- Path B does not prove adapter/core invocation; it explicitly reports `adapter_core_invoked=false` for the UI answer.
- Path B final response is captured through UIA/readback from the active foreground thread; screenshots show the same thread and nearby response markers but not the complete final response line.
- Product readiness remains `not_proven`.

## Narrow Human Claim

Row19 has a scoped same-scenario pass package: Path A proves the installed Codex XIIGen adapter/core boundary reached exit `0` with consistent prompt hash and selected `xiigen-smart-skills`; Path B proves the same Row19 Codex UI thread produced/captured a non-mutating user-visible response with the same scenario/correlation. The evidence supports workflow-boundary comparison only. It does not prove product readiness, host readiness, semantic quality, token savings, learnability, training, checkpoint mutation/promotion, locator update, or git/code changes.

## Next Packaging Work Order

Package `joined-comparison/row19-joined-comparison.md` and `joined-comparison/row19-joined-comparison.json` with the existing Path A and Path B artifacts as Row19 scoped workflow evidence. Keep the package label as scoped accepted, not product-ready. If a stricter UI proof gate is later required, the next bounded UI work order should capture a full screenshot-visible `final_user_visible_response` line and/or an explicit Path B prompt hash, but that is hardening rather than a blocker for this scoped joined comparison.
