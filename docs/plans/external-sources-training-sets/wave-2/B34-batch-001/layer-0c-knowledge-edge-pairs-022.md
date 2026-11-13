# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-022-001 — "explain" boundary cases: always gap, no exceptions

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Explain the architecture" | [] gap | explain = teaching → gap |
| "Explain how to build the service" | [] gap | explain + infinitive = gap |
| "Explain and document" | DOCUMENT | document fires; explain = gap warning |
| "Explain and implement" | IMPLEMENT | implement fires; explain = gap warning |
| "Explain the security model" | [] gap | security topic ≠ SECURITY predicate |
| "Explain the test strategy" | [] gap | testing topic ≠ TEST predicate |
| "Explain and write tests" | TEST | write tests fires; explain = gap warning |
| "Explain the integration" | [] gap | integration topic ≠ API_CONTRACT; no "integrate" verb |
| "Explain and integrate" | API_CONTRACT | integrate fires; explain = gap warning |

---

## KE-022-002 — "explain" vs "document" disambiguation

"explain" = oral or conceptual teaching → always gap
"document" = written artifact creation → DOCUMENT fires
"Explain the API design" → gap
"Document the API design" → DOCUMENT
"Explain and write up the API design" → DOCUMENT ("write up" = DOCUMENT trigger)
"Explain it verbally then document it in Confluence" → DOCUMENT fires; explain = gap warning

---

## KE-022-003 — "explain" never fires DOCUMENT — critical rule

v6 corruption pattern: "Explain the onboarding process" → Predicates: ["DOCUMENT"]
Correct: Predicates: [] → gap
Rule: explain ≠ DOCUMENT. DOCUMENT fires on write/create/produce/author + documentation artifact. Explain has no predicate equivalent, ever.
DOCUMENT also does NOT fire on: tutorial, walkthrough, guide (noun), onboarding, learning path — these are gap vocabulary. DOCUMENT fires on explicit documentation action verbs only.
