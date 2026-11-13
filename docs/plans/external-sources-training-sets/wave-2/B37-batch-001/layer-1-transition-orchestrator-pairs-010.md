# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-010-001 — TransitionType summary for B37: complete reference

**All TransitionType values used in B37:**

| TransitionType | Predicate(s) | Notes |
|---------------|-------------|-------|
| unknown | [] | Zero predicates; knowledge_edge; SCL emitted |
| documentation | [DOCUMENT] | "stakeholder" false positive most common |
| architecture_design | [ARCHITECTURE] | "structure" false positive most common |
| architecture_design + documentation | [ARCHITECTURE, DOCUMENT] | "define the X structure" pattern |
| feature_addition | [IMPLEMENT] | "build" in managing sentence |
| feature_addition + documentation | [IMPLEMENT, DOCUMENT] | Build + document |
| feature_addition + architecture_design | [IMPLEMENT, ARCHITECTURE] | Build + structure |
| api_contract | [API_CONTRACT] | "API dependencies" |
| api_contract + documentation | [API_CONTRACT, DOCUMENT] | API dep + document |
| no_contribution | [QA] | "review" or "validate" in managing sentence |
| no_contribution + documentation | [QA, DOCUMENT] | Review + criteria/requirements |
| test_contribution | [TEST] | "test" in estimation sentence |
| test_contribution + documentation | [TEST, DOCUMENT] | Test + stakeholder |
| test_contribution + no_contribution + documentation | [TEST, QA, DOCUMENT] | Full estimation test pattern |
| gap_interruption | [] (from within a flow) | Managing request interrupts active flow |

---

## B37-L1-010-002 — TransitionType "unknown" vs "gap_interruption"

**"unknown":** User has no active flow. Zero-predicate managing request arrives. No prior state.
**"gap_interruption":** User has an active flow. Zero-predicate managing request interrupts it. Prior state preserved.

Both result in knowledge_edge routing and SCL emission. The difference is in ContextCarryForward:
- "unknown" → ContextCarryForward: false
- "gap_interruption" → ContextCarryForward: true (prior flow preserved)

---

## B37-L1-010-003 — TransitionType determines ContextCarryForward

**ContextCarryForward rules:**

| TransitionType | ContextCarryForward |
|---------------|-------------------|
| unknown | false |
| gap_interruption | true |
| documentation | false (new flow) |
| architecture_design | false |
| feature_addition | false |
| api_contract | false |
| no_contribution | false |
| test_contribution | false |
| (any compound) | false |

Only gap_interruption carries forward context. All others start fresh flow state.

---

## B37-L1-010-004 — TransitionType for all five canonical B37 patterns

**Pattern 1:** "Track delivery blockers." → TransitionType: unknown, knowledge_edge
**Pattern 2:** "Update stakeholders on status." → TransitionType: documentation, 2.1
**Pattern 3:** "Define the accountability structure." → TransitionType: architecture_design + documentation, 3.1+2.1
**Pattern 4:** "Build the tracking module." → TransitionType: feature_addition, 4.1
**Pattern 5:** "Review the RAID log." → TransitionType: no_contribution, QA sub-phase

---

## B37-L1-010-005 — TransitionType + DomainId pairing invariant

**Invariant rule:**

"unknown" TransitionType → DomainId "project-management"
Any other TransitionType → DomainId "software-development"

This is the same rule as: zero predicates → "project-management"; any predicate → "software-development"

The TransitionType and DomainId always agree. If TransitionType is "documentation" but DomainId is "project-management" → error (v6-03 corruption variant).
