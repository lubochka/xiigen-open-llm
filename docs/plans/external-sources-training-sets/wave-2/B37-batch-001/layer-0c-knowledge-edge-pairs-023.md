# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-023-001 — Mixed: managing + ARCHITECTURE false positive analysis

**Knowledge edge: managing vocabulary + "structure" pattern**

When a sentence contains B37 managing vocabulary AND "structure", ARCHITECTURE fires on "structure" and managing vocabulary becomes gap warnings.

**Pattern:** managing_vocab + structure → ["ARCHITECTURE"] (or + "define" → ["DOCUMENT", "ARCHITECTURE"])

| Input | Predicates | Gap warnings |
|-------|-----------|-------------|
| "Accountability structure" | [ARCHITECTURE] | accountability-001 |
| "Define accountability structure" | [DOCUMENT, ARCHITECTURE] | accountability-001 |
| "Governance structure for escalation" | [ARCHITECTURE] | escalation-001 |
| "Reporting structure for workstreams" | [ARCHITECTURE] | work-stream-001 |
| "Team structure for delivery" | [ARCHITECTURE] | manage-delivery-001 |

---

## B37-0C-023-002 — Managing + code predicate: IMPLEMENT disambiguation

**Knowledge edge: managing vocabulary does not prevent IMPLEMENT from firing**

When managing sentences contain IMPLEMENT trigger tokens (build, service, module, create, implement, write):

| Input | Predicates | Notes |
|-------|-----------|-------|
| "Build the tracking service." | [IMPLEMENT] | service + build fire IMPLEMENT; tracking → gap |
| "Implement the escalation service." | [IMPLEMENT] | implement + service fire IMPLEMENT; escalation → gap |
| "Manage the tracking service build." | [IMPLEMENT] | service fire IMPLEMENT; manage/tracking → gap |
| "Create the accountability module." | [IMPLEMENT] | create + module fire IMPLEMENT; accountability → gap |

IMPLEMENT fires on technical tokens regardless of managing vocabulary present. Managing vocabulary always becomes gap warnings.

---

## B37-0C-023-003 — Mixed: managing + ARCHITECTURE + IMPLEMENT

**Input:** "Build the governance structure service for tracking project accountability."

Knowledge edge:
- "Build" → IMPLEMENT fires (Rule 9)
- "governance" → not in predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "service" → IMPLEMENT fires (Rule 9)
- "tracking" → gap (Rule 2: track ≠ TEST)
- "project accountability" → gap

Result: ["IMPLEMENT", "ARCHITECTURE"]
DomainId "software-development"
Gap warnings: gap-project-managing-track-001, gap-project-managing-accountability-001

---

## B37-0C-023-004 — Disambiguation: "accountability" alone vs "accountability structure"

**Key distinction for B37 certification:**

| Input | Predicates | Reason |
|-------|-----------|--------|
| "Establish accountability." | [] | "accountability" not in any predicate lexicon |
| "Define accountability." | [DOCUMENT] | "define" fires DOCUMENT |
| "Map the accountability structure." | [ARCHITECTURE] | "structure" fires ARCHITECTURE |
| "Define the accountability structure." | [DOCUMENT, ARCHITECTURE] | both "define" and "structure" fire |

"Accountability" alone: zero predicates.
"Accountability" + "define": DOCUMENT only.
"Accountability" + "structure": ARCHITECTURE only (even without "define").
"Accountability" + "define" + "structure": DOCUMENT + ARCHITECTURE.

---

## B37-0C-023-005 — Five mixed managing + ARCHITECTURE test cases

1. "Define the governance structure for delivery." → [DOCUMENT, ARCHITECTURE]
2. "Map the org structure for the programme." → [ARCHITECTURE]
3. "Establish accountability for the delivery team." → []
4. "Design the RACI structure for escalation protocols." → [ARCHITECTURE]
5. "Clarify the reporting structure and track accountability." → [ARCHITECTURE]

Cases 1-2 and 4-5: ARCHITECTURE fires (structure/design tokens present).
Case 3: Zero predicates — "establish" and "accountability" fire nothing.
All cases: DomainId determined by predicate scan, not intent.
