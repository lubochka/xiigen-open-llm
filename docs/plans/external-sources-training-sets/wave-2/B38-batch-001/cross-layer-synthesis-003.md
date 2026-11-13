# Cross-Layer Synthesis (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B38-CLS-003 — QA+DOCUMENT: Validate release criteria

Scenario: ReleaseCriteriaValidation
Input: "Review the deployment readiness criteria and validate the production environment is ready."
Full trace showing DOCUMENT fires on "criteria"; "deployment" is gap.

---

### LAYER 0-A: Compiler

Input vocabulary analysis:
- "Review" → QA
- "deployment" → GAP (rule 4: deployment operational context = gap)
- "readiness criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3 absolute)
- "validate" → QA
- "production" → context word
- "environment" → context word
- "ready" → context word

PredicateSet: ["QA", "DOCUMENT"]
CapabilityGapWarnings: ["deployment"]

Key compiler decisions:
- "deployment" evaluated separately from "criteria" — each word evaluated independently
- "deployment" is gap; "criteria" fires DOCUMENT; the compound "deployment readiness criteria" does not cancel either
- "Review" and "validate" both fire QA; consolidated to single QA predicate

---

### LAYER 0-B: Router

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow) — executed first
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (absolute rule 12)

DecisionCount: 2
TransitionType: no_contribution (IMPLEMENT absent)
CapabilityGapWarning: "deployment" is metadata; does not generate routing decision

---

### LAYER 0-C: Knowledge Edge

Knowledge edges:
- "deployment" fires no predicate (rule 4 knowledge edge: deployment = operational gap)
- "criteria" fires DOCUMENT (rule 3 knowledge edge: criteria → DOCUMENT always)
- Words evaluated independently: "deployment" + "criteria" → gap + DOCUMENT, not cancellation

---

### LAYER 0-D: Flow Factory

Factory creates two flows:
1. DocumentationFlow (deterministic_generic, Phase 2.1, order 1)
2. ReleaseGateFlow71 (deterministic_generic, Phase 7.1, order 2)

DOCUMENT flow artifact: deployment readiness criteria specification
QA flow evidence input: criteria_specification.md

---

### LAYER 1: Transition Orchestrator

Orchestration:
1. DocumentationFlow executes first (Phase 2.1):
   - Scope: deployment readiness criteria documentation
   - Output: readiness_criteria_specification.md
2. ReleaseGateFlow71 executes second (Phase 7.1 — QA last):
   - Input: readiness_criteria_specification.md
   - Output: release_gate_report.md

TransitionType: no_contribution
Gap warning on "deployment" does not affect orchestration flow

---

### LAYER 2: Nine-Node Flow (7.1)

N01: ["QA","DOCUMENT"] confirmed; gap warning: deployment
N02: 2.1 + 7.1 routing; DocumentationFlow first, ReleaseGateFlow71 second
N03-N07: criteria document produced; gate review executed
N09 Result:
- TransitionType: no_contribution
- Files: [readiness_criteria_specification.md, release_gate_report.md]
- FileCount: 2
- ResultState: readiness_check_complete
- CapabilityGapWarnings: ["deployment"]

---

### LAYER 3: Teaching Point

"deployment" is gap even in "deployment readiness criteria".
"criteria" still fires DOCUMENT despite the gap qualifier.
Words evaluated independently — gap qualifier does not cancel DOCUMENT trigger.
Two predicates → two routing decisions → two flows → two files.
TransitionType = no_contribution (IMPLEMENT absent).
