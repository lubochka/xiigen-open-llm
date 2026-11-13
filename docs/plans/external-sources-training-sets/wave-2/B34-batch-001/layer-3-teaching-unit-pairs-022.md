# Layer 3 — Teaching Unit Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-022 — "Explain" boundary teaching unit

**Core rule**: "explain" = teaching vocabulary → ALWAYS gap, no matter the topic, context, or surrounding words.

**"Explain" is always gap — no exceptions for topic**:
| Request | Predicate | Why |
|---------|-----------|-----|
| "Explain the architecture" | [] gap | architecture topic ≠ ARCHITECTURE predicate |
| "Explain the security model" | [] gap | security topic ≠ SECURITY predicate |
| "Explain the test strategy" | [] gap | testing topic ≠ TEST predicate |
| "Explain the API integration" | [] gap | integration topic ≠ API_CONTRACT predicate |
| "Explain how to write tests" | [] gap | "how to" teaching = gap |
| "Explain how to build the service" | [] gap | "how to build" = teaching = gap |

**"Explain" alongside code verbs — code verb fires, explain is gap warning**:
| Request | Predicate | Why |
|---------|-----------|-----|
| "Explain and document it" | DOCUMENT | document fires; explain = gap warning |
| "Explain and implement it" | IMPLEMENT | implement fires; explain = gap warning |
| "Explain and write tests" | TEST | write tests fires; explain = gap warning |
| "Explain and integrate the API" | API_CONTRACT | integrate fires; explain = gap warning |

**"Explain" never fires DOCUMENT**:
v6 corruption pattern: explain → DOCUMENT. This is always wrong.
"Explain" ≠ DOCUMENT. DOCUMENT requires explicit write/create/produce/author verbs.
"Explain the documentation" → gap. "Write the documentation" → DOCUMENT.

**Gap ID**: gap-skills-guidance-train-001 (explain = teaching vocabulary).
