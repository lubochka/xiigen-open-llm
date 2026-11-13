# Review/Fix Pass 06 - Local Learnability

Requested floor: 10 review/fix passes. This is pass 6 only.

Reviewer: Codex review_fix_pass_06.

Scope inspected:

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- Section 4.4 Correction -> DPO -> local learning algorithm
- Section 4.5 Target/rollback algorithm
- Section 6 LEARN phases
- Section 7 Required artifacts
- Section 8 review/fix ledger row 6
- Section 9 final acceptance matrix
- Section 11 no-overclaim firewall
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` row 6

Loaded rules and gates:

- `AGENTS.md` OP-62: a pass requires an inspectable evidence packet, not only a ledger row.
- `docs/skills/plan-review-SKILL.md` Review depth floor: ledger rows are an index, not evidence.
- `docs/skills/claim-verification-SKILL.md`: claims must be tied to inspected artifact evidence.

## Finding

Verdict before fix: FAIL.

The plan had a useful learnability sketch, but it was too generic for pass 6. Section 4.4 said DPO, continue-training, fresh-load, held-out and ablation were required, but it did not force:

- the full AI output/correction -> DPO -> privacy/source/license -> user-local adaptive store -> approval packet -> continue-training -> fresh-load -> held-out/ablation -> rollback chain;
- strict user-local/private leg versus common repo leg separation;
- backup/export/import/share evidence for the user-local adaptive store;
- continue-training from an existing checkpoint, with retrain-from-zero forbidden unless explicitly labelled cold-start;
- source-disjoint held-out construction and ablation/no-memorization controls;
- separate learnability evidence for `PromptToTaskMachine`, `OrchestrationReasoningMachine`, and `SkillSelectionMachine`.

The review ledger also had row 6 as a short `PASS` without `review-pass-06.md/json`, so it did not satisfy OP-62.

## Fix Applied

Updated `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`:

- added Section 4.4.1 `Local learnability hard gate`;
- added the full lifecycle chain from correction/DPO through rollback;
- added privacy/source/license routing between user-local store and common repo training;
- required user-local adaptive store manifest plus backup/export/import/share and restore probe;
- required continue-training to load the existing checkpoint for each machine and record base/target/rollback sha;
- forbade retrain-from-zero unless the approval packet labels it as cold-start;
- required fresh-load through the public machine boundary, not file-open proof;
- required source-disjoint held-out rows and ablation controls for exact prompt, path/title/source-id, nearest-neighbour, no-trained-state, shuffled chosen/rejected, and user-local disabled/enabled comparison;
- added a machine-specific evidence table for `PromptToTaskMachine`, `OrchestrationReasoningMachine`, and `SkillSelectionMachine`;
- strengthened LEARN phases, required artifacts, final acceptance matrix, no-overclaim firewall, and review row 6.

Updated `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` row 6 to point at this evidence packet with `ledger_rows_are_evidence=false`.

## Recheck

After the fix, pass 6 has an inspectable evidence packet and the plan now requires local learnability proof per machine, not a generic local-model row.

Remaining boundary: this is still plan/review evidence only. No product code, runtime, training, tests, checkpoint mutation, commit, or push was run.

Verdict after fix: PASS_WITH_FIX for pass 6 only.
