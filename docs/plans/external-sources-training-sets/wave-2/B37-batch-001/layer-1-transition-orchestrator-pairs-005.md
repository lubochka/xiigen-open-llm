# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-005-001 — ARCHITECTURE+DOCUMENT in governance context

**Input:** "Define the project governance structure and document the escalation criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation (parallel)
- ContextCarryForward: false
- Next states: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes

**Gap IDs forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-accountability-001 (governance)
- gap-project-managing-blockers-001 (if present in request)

**Orchestrator note:** Both flows are governance-related false positives in B37 context. ArchitectureFlow31 processes the structural design (software architecture), DocumentationFlow processes the documentation (specs/requirements). Neither addresses escalation criteria or governance.

---

## B37-L1-005-002 — ARCHITECTURE+DOCUMENT: RACI structure context

**Input:** "Define the RACI structure and specify the accountability requirements."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation
- Next states: 3.1 + 2.1
- DomainId: "software-development"
- Gap IDs: gap-project-managing-accountability-001

---

## B37-L1-005-003 — ARCHITECTURE alone: governance structure without DOCUMENT trigger

**Input:** "Establish the delivery governance structure."

**Compiler output:** ["ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: architecture_design
- Next state: ArchitectureFlow31 (3.1) only
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-accountability-001

---

## B37-L1-005-004 — ARCHITECTURE+DOCUMENT+QA: review governance structure

**Input:** "Review the governance structure and document the accountability criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT", "QA"]

**Transition orchestrator:**
- TransitionType: architecture_design + documentation + no_contribution (QA)
- Note: QA does not have a standard TransitionType name; it is a sub-phase, not a transition type
- Next states: ArchitectureFlow31 (3.1) + DocumentationFlow (2.1) + QA sub-phase
- DomainId: "software-development"
- Parallel eligible: ArchitectureFlow31 + DocumentationFlow parallel; QA sub-phase can be concurrent

---

## B37-L1-005-005 — architecture_design TransitionType rules

**TransitionType = "architecture_design" when:**
- ARCHITECTURE predicate fired
- Trigger tokens: structure, architecture, design
- Prior context: any
- Next state: ArchitectureFlow31

**False positive advisory for B37:**
"architecture_design" TransitionType in B37 context indicates a false positive. The request intent is organizational governance. The correct TransitionType is assigned by predicate (ARCHITECTURE), not by intent.

**Compound types:**
- "architecture_design + documentation" — ARCHITECTURE + DOCUMENT
- "architecture_design + documentation + no_contribution" — ARCHITECTURE + DOCUMENT + QA
- "architecture_design + feature_addition" — ARCHITECTURE + IMPLEMENT
