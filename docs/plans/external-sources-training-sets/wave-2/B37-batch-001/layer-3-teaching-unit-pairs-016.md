# Layer 3 — Teaching-Unit Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-016-001 — Teaching: decision log is gap vocabulary

**Concept:** "Decision log" records project decisions. It is B37 gap vocabulary.

Q: "Update the decision log." → predicates?
A: []. DomainId "project-management". knowledge_edge. SCL emitted.

Q: "Track open decisions." → predicates?
A: []. DomainId "project-management".

**Key distinction:** "log" does NOT fire DOCUMENT. "document" DOES fire DOCUMENT.

Q: "Log all decisions." → predicates?
A: []. "log" is NOT in DOCUMENT lexicon.

Q: "Document all decisions." → predicates?
A: [DOCUMENT]. "document" fires DOCUMENT.

---

## B37-L3-016-002 — Teaching: "log" vs "document" boundary

| Token | Predicate | Notes |
|-------|----------|-------|
| log | None | "log" not in DOCUMENT lexicon |
| document | DOCUMENT | DOCUMENT trigger |
| update the log | None | "update" not in DOCUMENT lexicon |
| create the log | IMPLEMENT | "create" fires IMPLEMENT |
| write to the log | IMPLEMENT | "write" fires IMPLEMENT (code context) |

---

## B37-L3-016-003 — Teaching: "architectural" fires ARCHITECTURE

**ADR (Architecture Decision Record) boundary:**
"Document the architectural decision" → [DOCUMENT, ARCHITECTURE]

- "document" fires DOCUMENT
- "architectural" fires ARCHITECTURE (adjective form of "architecture" — Rule 20)

This is a double false positive: the intent is recording a decision; both DOCUMENT and ARCHITECTURE fire because of trigger tokens.

---

## B37-L3-016-004 — Teaching: decision log false positive scenarios

| Trigger | Predicate | Gap warning |
|---------|----------|------------|
| document | DOCUMENT | manage-delivery-001 |
| architectural + document | [DOCUMENT, ARCHITECTURE] | manage-delivery-001 |
| review | QA | manage-delivery-001 |
| define | DOCUMENT | manage-delivery-001 |
| log (alone) | None | manage-delivery-001 |
| track | None | manage-delivery-001 + track-001 |

---

## B37-L3-016-005 — Teaching: decision log exam

1. "Update the decision log." → []
2. "Track open decisions." → []
3. "Log the key decision." → []
4. "Follow up on decisions." → []
5. "Review the decision log." → [QA]
6. "Document all decisions." → [DOCUMENT]
7. "Document architectural decisions." → [DOCUMENT, ARCHITECTURE]
8. "Define the decision process." → [DOCUMENT]
9. DomainId for question 2? → "project-management"
10. DomainId for question 7? → "software-development"
