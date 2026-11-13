# Layer 0-A — Compiler Pairs (037)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-037-001 — "escalation process" is zero-predicate vocabulary

**Input A:** "Run the escalation process for blocked deliverables."

Token scan:
- "Run" → not in predicate lexicon → gap
- "escalation process" → zero predicates → gap
- "blocked deliverables" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Document the escalation process for stakeholders."

Token scan:
- "Document" → DOCUMENT fires (Rule 8)
- "escalation process" → zero predicates → gap
- "stakeholders" → DOCUMENT fires (Rule 6)

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-037-002 — "escalation" vs "escalate" both zero-predicate

**Input A:** "Escalate delivery blockers to the programme board."
- "Escalate" → not in predicate lexicon → gap
- "delivery blockers" → gap
- "programme board" → zero predicates → gap
- Predicates: []

**Input B:** "Manage the escalation path for unresolved issues."
- "Manage" → gap
- "escalation path" → gap
- "unresolved issues" → gap
- Predicates: []

**Input C:** "Escalate blocker to stakeholders for resolution."
- "Escalate" → gap
- "blockers" → gap
- "stakeholders" → DOCUMENT fires
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"

---

## B37-0A-037-003 — "communication plan" triggers DOCUMENT

**Input A:** "Prepare the communication plan for the programme."

Token scan:
- "Prepare" → not in predicate lexicon → gap
- "communication plan" → "plan" alone → zero predicates → gap
- "programme" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Document the communication plan for stakeholders."

Token scan:
- "Document" → DOCUMENT fires
- "communication plan" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-037-004 — "project kickoff" is zero-predicate vocabulary

**Input:** "Facilitate the project kickoff session with the delivery team."

Token scan:
- "Facilitate" → not in predicate lexicon → gap
- "project kickoff" → zero predicates → gap
- "delivery team" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-manage-delivery-001
SCL: emitted

---

## B37-0A-037-005 — Round 3 vocabulary DomainId reference (037-048 opening)

**Zero-predicate DomainId "project-management" for new Round 3 terms:**

1. escalation process (alone) → project-management
2. escalation path (alone) → project-management
3. communication plan (alone, no DOCUMENT token) → project-management
4. project kickoff (alone) → project-management

**DomainId "software-development" immediately when:**
- "document" or "stakeholder" added → DOCUMENT fires
- "design" or "structure" added → ARCHITECTURE fires
- "build", "create", "implement", "service", "module" added → IMPLEMENT fires
