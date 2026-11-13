# Held-Out and Corruption Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-017-001 — HO: lessons-learned session → knowledge_edge

**Request:** "Run a lessons-learned retrospective with the delivery team."

**Expected output:**
- "Run" → not in any predicate lexicon
- "lessons-learned" → gap
- "retrospective" → not in any predicate lexicon
- "delivery team" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-manage-delivery-001

---

## B37-HO-017-002 — COR: "review" fires QA but intent check overrides (wrong)

**Request:** "Review lessons learned from the programme."

**v6 corrupt output:** Predicates: [] — QA suppressed because "reviewing lessons learned is a retrospective activity, not QA"

**Why this is wrong:** "review" fires QA regardless of context (Rule 19). QA is never suppressed based on intent.

**Correct output:** Predicates: ["QA"], DomainId: "software-development"

---

## B37-HO-017-003 — HO: track lessons-learned action items → knowledge_edge

**Request:** "Track all lessons-learned action items and ensure completion by workstream leads."

**Expected output:**
- "Track" → gap (Rule 2)
- "lessons-learned" → gap
- "action items" → gap
- "ensure" → not in predicate lexicon
- "workstream leads" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-track-001, gap-project-managing-action-items-001, gap-project-managing-work-stream-001, gap-project-managing-manage-delivery-001

---

## B37-HO-017-004 — COR: "validate" fires QA alongside "review" (correct — not corruption)

**Request:** "Review lessons learned and validate the improvement actions."

**Output:** Predicates: ["QA"] (one QA predicate from both "review" and "validate")

**This is CORRECT.** "review" and "validate" both fire QA. One QA predicate produced. DomainId "software-development".

**Contrast with false corruption:** A v6 would be claiming ["QA", "QA"] (two QA predicates) — predicates do not duplicate. One QA regardless of how many QA-firing tokens appear.

---

## B37-HO-017-005 — HO: lessons-learned certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-017-001 | "Run lessons-learned session" | [] | project-management |
| HO-017-002 | "Facilitate retrospective" | [] | project-management |
| HO-017-003 | "Track lessons-learned actions" | [] | project-management |
| HO-017-004 | "Review lessons learned" | [QA] | software-development |
| HO-017-005 | "Document lessons + stakeholders" | [DOCUMENT] | software-development |

All five must pass. Zero v6 corruptions permitted.
