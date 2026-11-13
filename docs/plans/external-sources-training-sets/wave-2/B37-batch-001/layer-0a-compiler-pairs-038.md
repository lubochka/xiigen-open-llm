# Layer 0-A — Compiler Pairs (038)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-038-001 — "steering committee" is zero-predicate vocabulary

**Input A:** "Present the programme status to the steering committee."

Token scan:
- "Present" → not in predicate lexicon → gap
- "programme status" → zero predicates → gap
- "steering committee" → zero predicates (Rule: committee names ≠ stakeholders) → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Present status to stakeholders and the steering committee."
- "Present" → gap
- "status" → gap
- "stakeholders" → DOCUMENT fires
- "steering committee" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-038-002 — "change control board" is zero-predicate vocabulary

**Input A:** "Submit the change request to the change control board."

Token scan:
- "Submit" → not in predicate lexicon → gap
- "change request" → zero predicates → gap
- "change control board" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Document the change request criteria for the change control board."
- "Document" → DOCUMENT fires
- "change request" → gap
- "criteria" → DOCUMENT fires
- "change control board" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-038-003 — "scope creep" is zero-predicate vocabulary

**Input A:** "Flag scope creep on the delivery workstream."

Token scan:
- "Flag" → not in predicate lexicon → gap
- "scope creep" → zero predicates → gap
- "delivery workstream" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Define scope boundaries to prevent scope creep."
- "Define" → DOCUMENT fires
- "scope boundaries" → "boundaries" alone → gap; but "define" already fired
- "scope creep" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-038-004 — "risk mitigation" is zero-predicate vocabulary

**Input A:** "Manage risk mitigation across the delivery programme."
- "Manage" → gap
- "risk mitigation" → gap
- "delivery programme" → gap
- Predicates: []

**Input B:** "Implement the risk mitigation strategy."
- "Implement" → IMPLEMENT fires
- "risk mitigation" → gap
- "strategy" → alone → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-038-005 — "quality gate" fires QA only when QA token present

**Input A:** "Pass the quality gate before delivery."
- "Pass" → not in predicate lexicon → gap
- "quality gate" → zero predicates (Rule: "quality" alone ≠ QA) → gap
- "delivery" → gap
- Predicates: []

**Input B:** "Review the quality gate criteria."
- "Review" → QA fires
- "quality gate" → gap
- "criteria" → DOCUMENT fires

Predicates: ["QA", "DOCUMENT"]
DomainId: "software-development"
