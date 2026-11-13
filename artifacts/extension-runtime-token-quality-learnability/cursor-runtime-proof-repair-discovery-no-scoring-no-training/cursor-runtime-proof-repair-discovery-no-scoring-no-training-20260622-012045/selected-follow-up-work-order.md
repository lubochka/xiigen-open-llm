# Selected Follow-Up Work Order

Selected next work order: DOCS-QA-CURSOR-RUNTIME-PROOF-RETRY-EXECUTION-NO-SCORING-NO-TRAINING-01

Reason: repair/discovery found and created a deterministic Cursor command-level invocation path. The next safe action is a retry execution, not a stop.

Scope for retry:

- Launch local Cursor/Extension Host with `--extensionDevelopmentPath` pointing to `C:\Users\lubao\Documents\xiigen\extensions reasearch\cursor-xiigen-smart-skills`.
- Set `PHASE40_CURSOR_RUNTIME_EVIDENCE_DIR=<NEXT_RUN_ROOT>\runtime-evidence`.
- Activate extension id `xiigen.cursor-xiigen-smart-skills`.
- Invoke command `xiigenSmartSkills.runtimeProof.invoke` for positive and negative redacted scenarios exactly as described in `invocation-path-contract.json`.
- Produce fresh runtime proof artifacts under the retry work order's own timestamped leaf.

Forbidden in retry: scoring/eval/token savings measurement, training, DPO, checkpoint/model/locator/target mutation, Claude runtime proof, Codex runtime proof, external AI/web/model calls, broad private corpus reads and git commands unless a separate parent boundary authorizes git.
