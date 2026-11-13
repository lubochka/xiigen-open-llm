# Layer 3 — Teaching-Unit Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-023-001 — Teaching: managing vocab + ARCHITECTURE (structure = ARCHITECTURE)

**Concept:** When managing vocabulary and "structure" appear, ARCHITECTURE fires on "structure" and managing vocabulary becomes gap warnings.

Q: "Define the accountability structure." → predicates?
A: [DOCUMENT, ARCHITECTURE]. "define" → DOCUMENT; "structure" → ARCHITECTURE.

Q: "Establish accountability." → predicates?
A: []. No "structure" → zero predicates.

---

## B37-L3-023-002 — Teaching: three ways ARCHITECTURE fires in managing context

1. "accountability structure" → ARCHITECTURE on "structure"
2. "governance structure" → ARCHITECTURE on "structure"
3. "design the accountability framework" → ARCHITECTURE on "design"

All three produce DomainId "software-development".

Non-firing equivalents: "accountability framework", "accountability model", "governance approach" → zero predicates.

---

## B37-L3-023-003 — Teaching: code predicates with managing vocabulary

**Concept:** IMPLEMENT fires on code tokens even when managing vocabulary surrounds them.

Q: "Build the escalation tracking service." → predicates?
A: [IMPLEMENT]. "build" and "service" fire IMPLEMENT. "escalation" and "tracking" → gap warnings.

Q: "Create the accountability module." → predicates?
A: [IMPLEMENT]. "create" and "module" fire IMPLEMENT. "accountability" → gap warning.

Managing vocabulary never prevents code predicates from firing.

---

## B37-L3-023-004 — Teaching: disambiguation between managing and code

The key rule for disambiguation:
1. Scan all tokens for predicates
2. If code tokens (build/service/module etc.) present → IMPLEMENT fires
3. If "structure" present → ARCHITECTURE fires
4. "accountability"/"escalation"/"risk log" etc. → gap warnings only
5. DomainId determined by predicate count (zero or non-zero)

The managing vocabulary NEVER adds predicates. It only contributes gap warnings.

---

## B37-L3-023-005 — Teaching: mixed managing + code exam

1. "Build the accountability tracking service." → [IMPLEMENT]
2. "Define the accountability structure." → [DOCUMENT, ARCHITECTURE]
3. "Track accountability across workstreams." → []
4. "Create the risk log template." → [IMPLEMENT]
5. "Maintain the risk log." → []
6. "Implement the decision log service." → [IMPLEMENT]
7. "Update the decision log." → []
8. "Review the accountability structure." → [QA, ARCHITECTURE]
9. DomainId for question 5? → "project-management"
10. DomainId for question 4? → "software-development"
