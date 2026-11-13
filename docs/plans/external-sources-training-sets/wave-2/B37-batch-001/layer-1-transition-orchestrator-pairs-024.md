# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-024-001 — Complete TransitionType map for extended B37 vocabulary

**TransitionType reference for 013-024 vocabulary:**

| Input pattern | Predicates | TransitionType | DomainId |
|--------------|-----------|---------------|---------|
| Pure gap managing (all extended vocab) | [] | unknown | project-management |
| + "review" | [QA] | qa_review | software-development |
| + "define" | [DOCUMENT] | documentation | software-development |
| + "document" | [DOCUMENT] | documentation | software-development |
| + "stakeholder" | [DOCUMENT] | documentation | software-development |
| + "structure" | [ARCHITECTURE] | architecture_design | software-development |
| + "design" | [ARCHITECTURE] | architecture_design | software-development |
| + "define" + "structure" | [DOCUMENT, ARCHITECTURE] | documentation + architecture_design | software-development |
| + "build/create/implement/service/module" | [IMPLEMENT] | feature_addition | software-development |
| + API co-occurrence + "dependencies" | [API_CONTRACT] | api_contract | software-development |
| + API co-occurrence + "dependencies" + "service" | [API_CONTRACT, IMPLEMENT] | api_contract + feature_addition | software-development |

---

## B37-L1-024-002 — gap_interruption applies to extended vocabulary

**gap_interruption applies to all B37 gap vocabulary (base + extended):**

Scenario: User in active flow (any: DocumentationFlow, BackendServiceFlow41, etc.) issues a managing request using extended vocabulary.

Example: User in DocumentationFlow (2.1), then issues:
"Track change requests and escalate to the steering committee."

**Transition orchestrator:**
- TransitionType: gap_interruption
- ContextCarryForward: true (DocumentationFlow context preserved)
- Next state: knowledge_edge (gap handled)
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- After gap handling: DocumentationFlow context resumable

---

## B37-L1-024-003 — SCL emission: certified rule for extended vocabulary

**SCHEDULE_CAPABILITY_LEARNING emission rule (extended):**

Emitted:
- Zero predicates for any extended B37 vocabulary request
- TransitionType: unknown
- One SCL per request regardless of how many gap IDs fire

NOT emitted:
- Any single predicate fires (TEST, QA, DOCUMENT, ARCHITECTURE, IMPLEMENT, API_CONTRACT)
- Even if managing vocabulary is primary
- Even if multiple gap warnings fire alongside

This rule is identical for base vocabulary (002-012) and extended vocabulary (013-024).

---

## B37-L1-024-004 — Extended vocabulary certification test (10 questions)

Q1: "Maintain the risk log." → TransitionType?
A: unknown. DomainId: project-management. knowledge_edge.

Q2: "Review the risk log." → TransitionType?
A: qa_review. DomainId: software-development.

Q3: "Document change request + stakeholder." → TransitionType?
A: documentation. DomainId: software-development.

Q4: "Define the accountability structure." → TransitionType?
A: documentation + architecture_design. DomainId: software-development.

Q5: "Circulate the project charter." → TransitionType?
A: unknown. DomainId: project-management.

Q6: "Map API dependencies between services." → TransitionType?
A: api_contract + feature_addition. DomainId: software-development.

Q7: "Run lessons-learned session." → TransitionType?
A: unknown. DomainId: project-management.

Q8: "Map team dependencies." → TransitionType?
A: unknown. DomainId: project-management.

Q9: "Track change requests + escalate to stakeholders." → TransitionType?
A: documentation. DomainId: software-development (stakeholders fires DOCUMENT).

Q10: "Design the governance structure." → TransitionType?
A: architecture_design. DomainId: software-development.

---

## B37-L1-024-005 — Extended B37 orchestrator summary

1. All new vocabulary (risk log, change request, decision log, lessons learned, accountability, project charter) → TransitionType "unknown" when used alone
2. "structure" and "design" → architecture_design transition always
3. "stakeholder" → documentation transition always
4. "review" → qa_review transition always
5. DomainId "project-management" ↔ exactly zero predicates → exactly one
6. SCL emitted exactly once per zero-predicate request
7. ContextCarryForward "true" only for gap_interruption
8. Three v6 corruption types unchanged for extended vocabulary
