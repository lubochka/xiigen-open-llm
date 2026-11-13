# Document Examination To Training Sets
# Transition orchestrator edition
# Version: 5.0 - 2026-05-05
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v4.md

---

## Purpose

This method examines documents for the corrected first orchestration step:

```text
human prompt -> LogicalTransitionOrchestrator -> selected A/B/X transition
```

The transition is non-deterministic. Examination must therefore preserve
candidate transitions, not only the selected one.

---

## Required Candidate Set

For each prompt-level row, the B side contains:

```yaml
candidates:
  - transition_type:
    A:
    B:
    X:
    confidence:
    status:
    reason:
selected_transition:
clarification:
arbiters:
evidence_ids:
```

At least one selected candidate and one rejected candidate are required unless
the prompt is a clarification-only case.

---

## Candidate Examination Questions

Ask:

```text
What transition type does the prompt most likely request?
What other transition types are plausible?
Why are the non-selected candidates rejected?
Does any candidate require clarification instead of selection?
Does any candidate invent source authority?
Does any candidate hide a rejected intent?
```

---

## Correct B Shape Example

```yaml
candidates:
  - transition_type: feature_addition
    A: ["API lacks requested tenant-aware authentication lifecycle."]
    B: ["API has tenant-aware authentication with release-blocking evidence."]
    X: ["design, implement, test, review, and automate tenant-authentication controls."]
    confidence: 0.91
    status: selected
    reason: "Prompt says add multi-tenant authentication and asks for full lifecycle evidence."
  - transition_type: repair
    A: ["Existing authentication is unsafe."]
    B: ["Existing authentication is repaired."]
    X: ["fix unsafe behavior."]
    confidence: 0.55
    status: rejected
    reason: "Prompt does not describe an existing failed implementation."
selected_transition: feature_addition
clarification:
  needed: false
```

---

## Failure Modes

### TO-1 - Single-candidate shortcut

The B side contains only one candidate.

Correction: add plausible rejected candidates, or mark the row as
clarification-only if no safe selection exists.

### TO-2 - Deterministic type switch

The B side acts as if one keyword always determines the transition type.

Correction: include candidate confidence, source evidence, and rejection reasons.

### TO-3 - Lost rejected candidate

A plausible non-selected transition disappears from evidence.

Correction: preserve it with status `rejected` and a reason.

### TO-4 - Clarification suppressed

Candidates are too close or imply different downstream work, but the B selects
one anyway.

Correction: output clarification.

---

## Last Updated

2026-05-05.
