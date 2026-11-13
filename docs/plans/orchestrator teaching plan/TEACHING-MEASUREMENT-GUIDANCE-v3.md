# Teaching Measurement Guidance

Version: 3.0 - 2026-05-05  
Replaces: `TEACHING-MEASUREMENT-GUIDANCE-v2.md`  
Authority: `ORCHESTRATOR-TEACHING-PLAN-v2.md`  
Grounded in: `AdaptiveLogicalOrchestrator.cs`, `MultiSubPhaseRouterUnit.cs`, `PromptRoutingInputCompilerUnit.cs`, `DefaultLogicalCapabilityTrainer.cs`

v3 changes from v2: row 4 accepted with correct routing confirmed; resource numbers updated to reflect Rank 2 cost; registry accumulation and pruning requirement added; row 4 warning resolved and replaced with confirmed result; cumulative state updated to 4 accepted rows, 14 units, 46 flows.

v3.1 clarification: older entries are older weight snapshots, not unrelated "dead" knowledge. Runtime uses the active snapshot. Teaching a repeated sub-phase must load the active checkpoint, continue training those weights, and save the next parent-linked version.

---

## What This Document Is

This document explains how to judge whether a teaching row actually improved the orchestrator machine, what the measurements mean, and what to do when the numbers reveal a problem.

The most important correction in v2 is this:

```text
Teaching must be cumulative.
The active teaching session persists one topology registry, flow registry, unit registry,
capability trainer state, and orchestrator state across rows.
Each invocation teaches exactly one next row, then re-tests 2-3 prior rows.
```

An isolated row run can be used only as a smoke test. It answers: "Can this row teach and retry itself?" It does not prove learning. Accepted teaching evidence must prove that row N runs with everything learned from rows 1..N-1 still present and usable, and that the newest 2-3 prior accepted rows still pass after row N grows the registry.

---

## Required Evidence Per Row

Every row must produce evidence for three layers:

```text
Layer 1 - Resources
  Inference time, teaching time, CPU, working-set delta, managed allocation,
  disk/checkpoint growth, branch count, registered flow count, registered unit count.

Layer 2 - Routing
  Compiler predicates, routed sub-phases, confidence values, branch count,
  and whether each branch is justified by the real human prompt.

Layer 3 - Verdict
  Pre-teaching verdict, teaching result, post-teaching verification verdict,
  knowledge-edge count before and after, and whether cumulative registry state improved.
```

A row succeeds only if all three layers are acceptable.

---

## Mandatory Cumulative Teaching Rule

The teaching runner must not create a fresh orchestrator per row.

Correct cycle:

```text
Start bundle
  create topology registry once
  create flow registry once
  create unit registry once
  create capability trainer once
  create adaptive orchestrator once
  save active teaching snapshot

For row 1
  load active teaching snapshot
  compile prompt
  route to sub-phases
  teach every routed branch
  register learned unit/flow into the same registries
  verify row 1 against the same registries
  save active teaching snapshot
  write row evidence

For row 2
  load active teaching snapshot from row 1
  compile prompt
  route to sub-phases
  teach every routed branch, including sub-phases already seen before
  register newer learned unit versions
  verify row 2 against cumulative registry state
  re-test row 1 against the grown registry
  save active teaching snapshot
  write row evidence

For row N
  load active teaching snapshot from row N-1
  teach only row N
  verify row N
  re-test the newest 2-3 previously accepted rows
  accept row N only if the new row passes and the regression window still passes
  save active teaching snapshot
```

This deliberately avoids an "N rows in one long push" teaching run. The machine must show progress after one row at a time, because the value being measured is whether the logical unit grows without forgetting recent accepted behavior.

Wrong cycle:

```text
For each row
  create a fresh orchestrator
  train missing flows
  retry same row
```

The wrong cycle proves only local self-repair. It does not prove continuous learning, because row 2 cannot benefit from row 1 and row 3 cannot benefit from rows 1 and 2.

---

## Already-Known Sub-Phase Rule

If row N routes to a sub-phase that already has a learned flow from an earlier row, the row must still create a new teaching event for that sub-phase.

Reason:

```text
Existing capability means the machine has a prior belief.
The new row is new evidence.
Teaching must update that belief, not skip it.
```

Correct behavior:

```text
row 1 routes to 4.1 -> create 4.1-runtime-unit v1
row 2 routes to 4.1 -> load v1 checkpoint, continue training, save v2
ResolveLatest(4.1) now selects v2 if it is Probationary or Trusted.
```

Incorrect behavior:

```text
row 2 routes to 4.1
flow already exists
skip teaching
verify with row-0001 unit
```

That is reuse, not learning.

Also incorrect:

```text
row 2 routes to 4.1
create a completely separate row-0002 unit that does not start from row-0001 weights
```

That is accumulation, not continuous learning. The accepted target is a version chain:

```text
4.1-runtime-unit v1 -> learned from row 1
4.1-runtime-unit v2 -> starts from v1 weights, then learns row 2
4.1-runtime-unit v3 -> starts from v2 weights, then learns a later row
```

---

## Layer 1 - Resource Numbers

### Confirmed Cumulative Numbers

The first accepted cumulative run used one long-lived topology registry, flow registry, unit registry, capability trainer, and orchestrator across rows 1 and 2.

```text
Cycle run id: cumulative-20260505173527
Bundle: legacy-docs-skills-from-old-code
Mode: cumulative
```

Row 1 cumulative result:

```text
Row: CEXP-B0000-LDS-0001
Prompt: documentation on this project and skills for Claude Code
Predicates: ARCHITECTURE, DOCUMENT, IMPLEMENT, LEARNING
Branches: 2.1, 4.4, 4.1, 3.1, 7.5
Teaching elapsed: 3,298.4009 ms
Total row elapsed: 3,388.0747 ms
CPU: 12.68%
Working-set delta: 25.930 MB
Managed allocation delta: 336.730 MB
Registry before: 31 flows, 0 units
Registry after: 36 flows, 5 units
Registered this row: 5 flows, 5 learned units
Verification: PASS, 0 knowledge edges
Decision: ACCEPT_ROW
```

Row 2 cumulative result:

```text
Row: CEXP-B0000-LDS-0002
Prompt: Figma async/null issue with dynamic document
Predicates: IMPLEMENT, TEST, UI
Branches: 4.4, 4.1, 4.2, 5.4, 5.1
Teaching elapsed: 7,360.8703 ms
Total row elapsed: 7,401.3247 ms
CPU: 13.12%
Working-set delta: 11.961 MB
Managed allocation delta: 755.232 MB
Registry before: 36 flows, 5 units
Registry after: 41 flows, 10 units
Registered this row: 5 flows, 5 learned units
Verification: PASS, 0 knowledge edges
Decision: ACCEPT_ROW
```

Row 4 cumulative result (accepted version after routing fix):

```text
Row: CEXP-B0000-LDS-0004
Prompt: To my regret this didn't help Error - Figma frame export returned no data.
        Make sure the AI Design Canvas frame exists and try again.
Predicates: IMPLEMENT, TEST, UI
Branches: 5.1, 4.2, 4.4
Teaching elapsed: 2,887.9012 ms
Total row elapsed: 2,970.3787 ms
CPU: 12.89%
Working-set delta: 16.797 MB
Managed allocation delta: 299.498 MB
Registry before: 43 flows, 11 units
Registry after: 46 flows, 14 units
Registered this row: 3 flows (5.1, 4.2, 4.4), 3 learned units
Checkpoint bytes: 1,234,698
Verification: PASS, 0 knowledge edges
Regression: rows 1, 2, 3 all PASS against grown registry
Decision: ACCEPT_ROW
LoRA config used: Rank 2, Alpha 4
```

Row 5 cumulative result (first accepted checkpoint-continuation proof):

```text
Row: CEXP-B0000-LDS-0005
Prompt: Figma dynamic-page document loading / async execution-order issue
Predicates: IMPLEMENT, TEST, UI
Branches: 4.2, 5.4, 4.4, 5.1
Teaching elapsed: 7,748.0163 ms
Total row elapsed: 7,849.3363 ms
CPU: 13.06%
Working-set delta: 68.086 MB
Managed allocation delta: 802.116 MB
Registry before: 46 flows, 14 units
Registry after: 50 flows, 18 units
Registered this row: 4 flows (4.2, 5.4, 4.4, 5.1), 4 learned units
Verification: PASS, 0 knowledge edges
Regression: rows 2, 3, 4 all PASS against grown registry
Decision: ACCEPT_ROW
Checkpoint continuation: confirmed
```

Row 5 is the first row where the implementation proves weight continuation rather
than only registry accumulation. Every row-5 generated capability manifest contains:

```text
ParentUnitId: present
ParentPolicyCheckpointPath: present
TrainingStepCount: 2
Version: 1.0.1
```

Human meaning: the row-5 units did not start from fresh random weights. They loaded
the active row-2 or row-4 checkpoint for the same sub-phase, ran the next DPO step,
and saved the next checkpoint version.

Important: row 4 had a rejected first attempt (run id ending 181329) that routed to
sub-phase 1.1 only with UI predicate alone — wrong for a runtime export failure.
After the compiler/router fix for "export", "frame", "canvas", and "no data" vocabulary,
the second attempt (run id ending 181747) routed correctly to 5.1, 4.2, 4.4.
The rejected attempt is preserved in the ledger as a false-route audit record.
The 1.1 capability it registered was removed from the active snapshot before row 4 rerun.

Most important cumulative proof across all four rows:

```text
Row 1 taught sub-phases 2.1, 3.1, 4.1, 4.4, 7.5.
Row 2 also touched 4.1 and 4.4 — registered newer versions, not skipped.
Row 4 also touched 4.4 — registered row-4 version.
Regression on row 1 after row 4: selected generated-capability-4-4-row-0004,
not row-0001. ResolveLatest correctly selects the newest version.
The machine learned from all four rows and each prior row remained stable.
```

This proves the corrected cycle is cumulative learning rather than isolated self-repair.

Important limitation of the row-1-through-row-4 evidence: it proves cumulative
registry selection and regression stability. It does not by itself prove that
the row-4 model weights started from the row-2 or row-1 weights. That stricter
proof requires parent checkpoint metadata and a trainer path that loads the
active checkpoint before running the next DPO step.

### Cumulative Budget Warning

A follow-up cumulative run through rows 1..3 produced the same routing and registry-carry-forward behavior, but row 2 crossed the 10-second row budget:

```text
Cycle run id: cumulative-20260505173713
Row 1: 6,013.9270 ms, PASS, 5 learned units registered
Row 2: 12,573.8179 ms, PASS, 5 learned units registered
Row 3: 1,311.2407 ms, PASS, 1 learned unit registered
```

Human meaning:

```text
The cumulative learning behavior is correct.
The resource gate is not stable enough yet for repeated multi-branch rows.
Row 2 must be treated as REVIEW_ROW when it exceeds 10,000 ms, even if verdict is PASS.
```

Runner requirement:

```text
decision = ACCEPT_ROW only when:
  verification verdict is PASS
  knowledge_edge_count is 0
  row elapsed <= 10,000 ms
  registry_after learned_unit_count > registry_before learned_unit_count
```

If a row passes logically but exceeds the resource budget, the row is a useful learning observation but not an accepted teaching row. The next action is to reduce branch count, split the row, or reduce training cost, then rerun from the last accepted cumulative row.

### Superseded Smoke-Test Numbers

The first two real rows from `legacy-docs-skills-from-old-code` were initially measured in isolated-row mode. These numbers are useful as smoke-test baselines only, not accepted cumulative teaching evidence.

```text
Row 1 after predicate + anchor fixes:
  Prompt: documentation on this project and skills for Claude Code
  Branches: 5
  Verdict after retry: PASS
  Elapsed: 9,926.4751 ms
  CPU: 12.08%
  Working-set delta: 55.777 MB
  Managed allocation delta: 521.320 MB
  Checkpoint bytes: 627

Row 2 after document disambiguation:
  Prompt: Figma async/null issue with dynamic document
  Branches: 5
  Verdict after retry: PASS
  Elapsed: 9,582.4083 ms
  CPU: 12.31%
  Working-set delta: 71.012 MB
  Managed allocation delta: 957.215 MB
  Checkpoint bytes: 1,510
```

Human meaning:

```text
The row-level teach/register/retry mechanism works.
The resource cost for five branches is just under the 10-second row budget.
But these runs do not prove cumulative learning because the runner restarted state.
```

### Resource Thresholds

| Number | Threshold | Meaning | Action |
|---|---:|---|---|
| Inference elapsed | > 200 ms | Runtime routing is getting too slow | Inspect HNSW/search/tokenization cost |
| Teaching elapsed | > 10,000 ms for one row | Row exceeds budget | First reduce branch count; if still high, reduce model/training work |
| Teaching elapsed trend | increasing > 500 ms per row | Cumulative registry/model cost is growing too fast | Check duplicate flow/model registration and branch fan-out |
| Working-set delta per row | > 200 MB | Retained memory risk | Stop teaching; inspect model/tensor retention |
| Working-set after GC | monotonically increasing | Cumulative leak or registry/model duplication | Force post-row GC measurement and inspect generated flow/unit storage |
| Managed allocation delta | high but working set stable | Temporary tensor churn | Watch, but do not stop on this alone |
| Adapter/checkpoint bytes | unexpected large jump | Capability artifact growing too fast | Inspect registered unit/flow count and checkpoint path |

Important correction:

```text
Do not blindly say "reduce LoRA Rank from 4 to 2" unless the current trainer
actually uses Rank 4.
```

The current default capability trainer already uses a tiny Rank 2 LoRA config. If teaching still exceeds the 10-second budget at Rank 2, the next actions are branch-count reduction, row splitting, or reducing the tiny model/training path further.

---

## Layer 2 - Routing Numbers

### What Fixed So Far

Three routing fixes have already been applied and confirmed by row-level smoke tests:

```text
1. Predicate lexicon expansion
   Added missing LEARNING vocabulary and old/current codebase vocabulary.
   Result: skills-library prompts can reach 7.5.

2. Anchor filtering
   Broad predicates such as ARCHITECTURE and IMPLEMENT no longer select every
   sibling sub-phase unless the prompt has anchor vocabulary.
   Result: row 1 dropped from 9 branches to 5.

3. Document disambiguation
   The word "document" alone no longer triggers DOCUMENT when used as a runtime
   code entity such as "dynamic document".
   Result: row 2 dropped from 7 branches to 5.
```

### Routing Failure Modes

| Signal | Meaning | Resolution |
|---|---|---|
| Too few branches | Under-routing; compiler missed predicates | Add predicate vocabulary or source-specific synonyms |
| Too many branches | Over-routing; predicate family too broad | Tighten anchor terms or relevance floor |
| Wrong phase family | Predicate mismatch | Fix compiler lexicon before continuing rows |
| Correct predicates, wrong sub-phases | Sub-phase anchor vocabulary too weak | Add anchor terms or teaching rows for sibling distinction |
| PASS with wrong branch count | False success | Treat row as partial; fix routing before accepting it |

### Expected Routing From First Rows

Row 1 expected/correct after fixes:

```text
Prompt: I need a documentation on this project and skills for claude code
Predicates: ARCHITECTURE, DOCUMENT, IMPLEMENT, LEARNING
Branches:
  2.1 Business Capability Definition
  3.1 Architecture and Component Topology
  4.1 Backend Services
  4.4 Refactoring and Code Quality
  7.5 Learning Set and Model Feedback
```

Row 2 expected/correct after fixes:

```text
Prompt: Figma async/null issue with dynamic document
Predicates: IMPLEMENT, TEST, UI
Branches:
  4.1 Backend Services
  4.2 Frontend and Client Integration
  4.4 Refactoring and Code Quality
  5.1 Unit Test Design
  5.4 Regression and Compatibility Testing
```

### Row 4 Confirmed Routing

The first attempt at row 4 produced a false route to 1.1 (Design System and Tokens)
using only the UI predicate. This was wrong: a Figma export runtime failure is not
design-token work. The compiler vocabulary for "export", "frame", "canvas", "no data",
and "plugin" was missing IMPLEMENT and TEST signal.

After the vocabulary fix, the accepted row 4 routing is:

```text
Prompt: Figma frame export returned no data. Make sure the AI Design Canvas frame exists.
Predicates: IMPLEMENT, TEST, UI
Branches:
  5.1 Unit Test Design        (test coverage for the export path)
  4.2 Frontend and Client Integration (UI integration fix)
  4.4 Refactoring and Code Quality   (null/error handling cleanup)
Branch count: 3
```

This is correct. A runtime API failure in a UI plugin routes to testing, frontend
integration, and code quality — not to design tokens.

The pattern that triggered the false route: the word "canvas" matched UI vocabulary
strongly enough to dominate the routing when no IMPLEMENT or TEST terms were present.
The fix adds "export failure", "returned no data", and "frame exists" to IMPLEMENT
and TEST anchor vocabulary so runtime error prompts are not collapsed into UI-only routing.

---

## Layer 3 - Verdict Numbers

### Verdict Order

```text
REJECT
REPROCESS
KNOWLEDGE_EDGE
PASS_WITH_GAPS
PASS_WITH_CONCERNS / CONCERN
PASS
```

When multiple branches run, the highest-severity verdict wins.

### Correct Successful Teaching Pattern

For a new or updated branch:

```text
Before teaching:
  KNOWLEDGE_EDGE or existing lower-confidence learned result

Teaching:
  new row evidence creates a newer learned unit/flow version

Verification:
  ResolveLatest selects the newest Probationary-or-better unit
  verdict becomes PASS or improves according to the row oracle
```

For cumulative learning, it is not enough that the row passes. Evidence must show:

```text
previous_registered_unit_count
new_registered_unit_count
previous_registered_flow_count
new_registered_flow_count
selected learned unit id/version
whether the selected unit came from current row or earlier row
```

### Verdict Failure Modes

| Signal | Meaning | Action |
|---|---|---|
| KNOWLEDGE_EDGE after teaching | Trainer did not register usable capability | Inspect training result and flow registry |
| PASS but selected old unit for a taught branch | Row was not actually learned | Register newer unit version for same sub-phase |
| PASS but branch count wrong | Routing false success | Fix routing before accepting row |
| PASS_WITH_GAPS persists | Arbiter/coverage gap | Add arbiter pairs or flow-node pairs |
| Verdict degrades after new row | New training evidence introduced bad behavior | Compare selected unit version and row correction pair |

---

## Accepted Row Evidence Schema

Each cumulative row summary must include at least:

```json
{
  "row_id": "CEXP-B0000-LDS-0004",
  "cycle_mode": "cumulative",
  "prompt": "...",
  "compiler_predicates": ["IMPLEMENT", "TEST", "UI"],
  "router_sub_phases": ["4.2", "4.4", "5.4"],
  "branch_count": 3,
  "resources": {
    "inference_elapsed_ms": 0,
    "teaching_elapsed_ms": 0,
    "cpu_percent": 0,
    "working_set_delta_mb": 0,
    "managed_allocation_delta_mb": 0,
    "disk_delta_mb": 0,
    "checkpoint_bytes": 0
  },
  "registry_before": {
    "flow_count": 0,
    "unit_count": 0
  },
  "registry_after": {
    "flow_count": 0,
    "unit_count": 0
  },
  "registered_this_row": {
    "flow_ids": [],
    "unit_ids": [],
    "sub_phase_ids": []
  },
  "verification": {
    "overall_verdict": "PASS",
    "knowledge_edge_count": 0,
    "selected_unit_ids": []
  },
  "decision": "ACCEPT_ROW"
}
```

If `cycle_mode` is not `cumulative`, the row cannot be accepted as learning evidence.

---

## Registry Version Chains and Active Snapshots

### What accumulates today

Each row that touches a previously-seen sub-phase registers a new learned unit
version without removing the old one. After four rows, the snapshot for
sub-phase 4.4 contains three unit entries: from row 1, row 2, and row 4.
`ResolveLatest` always selects the highest `RegistrationOrder` non-Rejected entry,
so only the newest version is used at query time.

That behavior proves cumulative registry selection, but by itself it is not enough.
The trainer must also make the new checkpoint start from the previous active
checkpoint. Otherwise the machine is only replacing one one-row specialist with
another one-row specialist.

Correct model:

```text
4.4-runtime-debugging-unit
  active version: v3
  v3 checkpoint: starts from v2 weights, then learns row 4
  v2 checkpoint: starts from v1 weights, then learns row 2
  v1 checkpoint: starts from base weights, then learns row 1
```

After 74 rows in the first bundle, frequently used sub-phases such as 4.1, 4.4,
5.1, and 7.5 will each have many weight snapshots. That is useful for rollback
and audit, but normal runtime should only see the newest accepted version for
each logical unit family.

### The active/archive rule

After completing a bundle or at the end of any session, run an active/archive pass:

```text
For each logical unit family:
  keep active: the newest Probationary or Trusted version
  archive: older checkpoint versions in parent-chain order
  delete: nothing — archived versions are kept for audit and rollback
```

This does not throw learning away. The newer active checkpoint should already
contain the learning signal from older rows because it was trained forward from
the prior checkpoint. The archive keeps older weight values so the machine can
explain history or roll back if a newer version regresses.

### What to archive vs what to keep active

```text
Active snapshot: one active version per logical unit family
Archive:         prior weight snapshots per family, organized by parent version and row id
Ledger:          all row summaries preserved, never pruned
Checkpoints:     newest policy.bin and tokenizer.json active; older weight values in archive
```

### When to prune

Prune at the end of each bundle, before starting the next bundle. Do not prune
mid-bundle — the mid-bundle regression window needs access to the version history
to verify that `ResolveLatest` is selecting the right version.

### When to rollback

If row N's new weight snapshot for sub-phase X causes a regression on a prior
accepted row, mark the row N version as Rejected in the snapshot. `ResolveLatest`
will then fall back to the most recent non-Rejected weight snapshot, which was
the row N-1 version. The rollback does not require re-running training.

---

## Summary Decision Table

| Resources | Routing | Verdict / Registry | Decision |
|---|---|---|---|
| within limits | correct branches | new units registered and verification PASS | Accept row; continue |
| within limits | too narrow | PASS | Under-routing; fix lexicon, rerun row cumulatively |
| within limits | too wide | PASS | Over-routing; fix anchors, rerun row cumulatively |
| within limits | correct | PASS but no new unit for row evidence | Not teaching; fix cumulative trainer path |
| > 10s | correct | PASS | REVIEW_ROW; reduce branch count or training cost; rerun from last accepted row |
| working set > 200MB | any | any | Stop; inspect memory retention |
| cumulative registry not reused | any | any | Reject run; rerun bundle with one long-lived orchestrator |
| row hits known sub-phase and skips training | any | PASS | Reject row; new evidence must update capability |

---

## Current State

Four rows are accepted in cumulative mode from bundle `legacy-docs-skills-from-old-code`:

```text
Rows accepted cumulatively: 5
Sub-phases covered: 2.1, 3.1, 4.1, 4.2, 4.4, 5.1, 5.4, 7.5 (8 of 31)
Generated flows in active snapshot: 50 total (18 generated capability flows)
Learned units in active snapshot: 18 (parent-linked versions for repeated sub-phases from row 5 onward)
Knowledge edges after verification: 0 across all five rows
Regression window (rows 2-4 tested after row 5): all PASS
ResolveLatest selects newest version: confirmed
Checkpoint continuation from active parent: confirmed on row 5
LoRA Rank in use: 2 (reduced from 4 to stay within 10-second budget)
```

The cumulative registry loop is confirmed working. The next required standard
for accepted learning is stricter: when a sub-phase is taught again, the trainer
must load the active checkpoint and continue its weights before saving the next
version.

Next engineering improvement: checkpoint continuation, then active/archive cleanup.
The active snapshot currently holds multiple unit versions per sub-phase (three
versions of 4.4, two versions of 4.1, two versions of 5.1). The newest must be
the result of continuing from the prior checkpoint, not an unrelated replacement.

Next teaching action: continue from row 5 using the same cumulative runner. The
bundle has 70 remaining rows. Resource budget is stable at Rank 2.

---

Last updated: 2026-05-05
