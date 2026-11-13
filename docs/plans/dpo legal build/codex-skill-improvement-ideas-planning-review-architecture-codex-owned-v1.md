# Codex-Owned Skill Improvement Ideas: Planning, Plan Review, Architecture

## Current Task Anchor

CURRENT TASK: collect Codex-owned ideas for improving planning, plan-review, and architecture skills based on the ILearningDynamicGraph planning pass and Claude benchmark comparison.

NOT THE TASK: do not edit Claude-authored/evidence-only files, do not implement product code, do not run training, tests, builds, git, or external model calls from this artifact.

EVIDENCE-ONLY: Claude attachment, `claude-*`, `dcm-*`, stale W2c overlays, invalid sub-agent packets, and the existing `codex-skill-improvement-ideas-planning-review-architecture.md` are evidence only.

## Ownership And Authority Ledger

| Artifact | Status | Rule |
|---|---|---|
| `codex-skill-improvement-ideas-planning-review-architecture-codex-owned-v1.md` | Codex-owned | editable by Codex in this planning library |
| `codex-skill-improvement-ideas-planning-review-architecture.md` | evidence-only | do not edit; master/BDR classify it as Claude-authored evidence |
| `codex-50-cycle-plan-review-supplement.md` | evidence-only for R20 | ledger rows do not count as cycles |

## Evidence-Only Sources Reviewed

- Claude attached benchmark text for plan-depth comparison.
- Valid returned review packets through cycle 37.
- Master/board/matrix honesty repairs around R20, coverage counts, and W2c false-claim quarantine.
- Legal/DPO, DynamicGraph, lifecycle, R15 reserve-row, and test-strategy synchronization findings.

## Planning Skill Improvement Ideas

1. Require an explicit `CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY` anchor before every plan subsection that derives scope from mixed current and benchmark material.
2. Add a `target_artifact_queue` block whenever the user names a directory plus ownership prefix, so foreign/parallel files cannot become active targets by similarity.
3. Add an `honest_count_contract` template for any numeric claim: `claimed_count`, `counted_unit`, `independent_evidence_count`, `invalid_attempt_count`, `pending_count`.
4. Require a `cross-artifact projection map` for every accepted benchmark idea: owner block, master row, board row, matrix row, test row, and final-question row when needed.
5. Separate `plan coverage` from `implementation authorization` in every final plan footer.
6. Require reserve rows to state whether they are in the baseline count, blocked, or activated by a named decision.
7. Add a duplicate-summary detector: if a plan has a body summary and a tail overlay, both must be updated or one must be quarantined.

## Plan Review Skill Improvement Ideas

1. Count only returned review packets, never spawned agents, ledger rows, supplement rows, or parent self-review.
2. Require every sub-agent packet to cite absolute target-library paths; packets that inspect the wrong workspace are `INVALID_PACKET_DO_NOT_COUNT`.
3. Add a stale-canon search bundle: `50/50`, `covered=15`, `partial=5`, `W2c`, `R20 covered`, `real_review_fix_pass_count=50`, `second_sweep_status=done`.
4. Add a contradiction class for "canonical footer overrides repaired body" and require footer readback after every plan repair.
5. Require accepted findings to name whether they need target-file edits, evidence-only quarantine, or final-question-only treatment.
6. Treat "no findings" packets as incomplete unless they include grep/search evidence and absolute files inspected.
7. Keep an invalid-packet ledger beside the valid review ledger so false cycle inflation is visible.

## Architecture Skill Improvement Ideas

1. Require DynamicGraph plans to name nodes, edges, topology builder, topology budget, topology signal, fan-out/fan-in, multi-output, async, and per-edge authority in one architecture reconstruction block.
2. Reject direct object-owned edge authority when the canon is registry/latest-wins by `AuthorityUnitId`.
3. Require edge-authority records to include `AuthorityUnitLevel`, `ParallelEligible`, `DeterministicFinalAuthority`, `BaselineUnitId`, `BaselineRole`, `ExcludedLeakageSignals`, `SourceProof`, `CutoverProof`, and `AblationProofRef`.
4. Require lifecycle proof fields in measurement/control artifacts: process id, restart id, entrypoint trace id, no replay cache proof, loaded hashes, manifest schema, source cutoff, path strategy, and rollback ref.
5. Require split-proof fields: train/eval source roots, source hashes, near-duplicate count, simhash/minhash method, and threshold.
6. Require no-signal auto-DPO behavior: accepted pair count after all gates is zero, stop reason is `no_signal`, no training or promotion happens, and previous checkpoint is retained.
7. Require open legal/product decisions in body text to be referenced as `decision_ref` or lock flags, with full questions only in the final questions section.

## Priority Backlog

| Priority | Improvement | Target Skill |
|---|---|---|
| P0 | Honest numeric-count contract for cycles, passes, files, and rows | plan review |
| P0 | Absolute target-path requirement for delegated review packets | plan review |
| P0 | Evidence-only quarantine gate for benchmark/foreign artifacts | planning |
| P1 | Cross-artifact projection map for accepted benchmark deltas | planning |
| P1 | DynamicGraph architecture reconstruction checklist | architecture |
| P1 | Lifecycle/split/no-signal proof-field checklist | architecture |
| P2 | Duplicate footer/body stale-canon detector | plan review |

## Proposed Patch Shape

- Planning skill: add `Authority/Target/Evidence Anchor`, `Cross-Artifact Projection Map`, and `Reserve Row Contract`.
- Plan-review skill: add `Returned Packet Count Gate`, `Invalid Packet Ledger`, and `Stale Canon Search Bundle`.
- Architecture skill: add `DynamicGraph Reconstruction Gate`, `Edge Authority Canon Gate`, and `Lifecycle/Split/No-Signal Proof Gate`.

## Safety Boundary

- This artifact is plan-only.
- It does not authorize implementation, tests, builds, training, git, or external model calls.
- It does not replace the ILearningDynamicGraph plan; it is an improvement backlog for future skill/guide patches.
- Any future skill patch must use a fresh active user instruction and must not edit evidence-only Claude artifacts.

## Questions for last plan section

None.
