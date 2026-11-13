# Layer 0-A — Compiler Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-010-001 — IMPLEMENT + API_CONTRACT + DOCUMENT + QA

**Input:**
"Build the DocuSign e-signature connector, document the signing workflow API interface, and QA the end-to-end signing flow."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "document" → DOCUMENT (Rule 12)
- "API interface" → API_CONTRACT (reinforces)
- "QA" → QA (Rule 21: QA fires only on "QA" abbreviation)
- Integration vocabulary: "connector", "API" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "QA"]
**SubPhase:** 4.3, 3.3, 2.1, 7.x (QAGatingFlow73)
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** QA is always last in canonical ordering (Rule 21). QA fires only on "QA" abbreviation — "quality assurance" written out does NOT fire QA.

---

## B17-0A-010-002 — QA abbreviation vs "quality assurance" full form

**Input A:** "Build the connector and QA the output." → QA fires → ["IMPLEMENT", "API_CONTRACT", "QA"]
**Input B:** "Build the connector and run quality assurance." → QA does NOT fire → ["IMPLEMENT", "API_CONTRACT"]

**Rule 21:** QA fires only on the abbreviation "QA". The full phrase "quality assurance" does not trigger QA.

**TransitionType comparison:**
- Input A (with QA): feature_addition (IMPLEMENT present, no refactor)
- Input B (without QA): feature_addition (same, QA absence doesn't change TransitionType)

---

## B17-0A-010-003 — QA alone → no contribution TransitionType

**Input:**
"QA the DocuSign signing flow end-to-end."

**Compiler analysis:**
- "QA" → QA (Rule 21)
- No build/implement/wire/connect verb → IMPLEMENT does not fire
- No connector/webhook/API/integration vocabulary → API_CONTRACT does not fire
- QA alone → no contribution TransitionType (Rule 21)

**CompiledPredicateIds:** ["QA"]
**SubPhase:** 7.x (QAGatingFlow73)
**TransitionType:** no contribution
**DomainId:** software-development

---

## B17-0A-010-004 — DOCUMENT vs QA distinction

**Input A:** "Document the DocuSign API and QA the signing flow."
→ ["DOCUMENT", "QA"] — two predicates, no IMPLEMENT
→ TransitionType: no contribution (QA presence doesn't elevate; DOCUMENT alone = documentation but QA overrides to no contribution when it's the highest-priority)

**Correction note:** With DOCUMENT + QA, no IMPLEMENT present → TransitionType: documentation (DOCUMENT is present and is higher-priority than QA's no-contribution).

**Final:**
**CompiledPredicateIds:** ["DOCUMENT", "QA"]
**SubPhase:** 2.1 (DOCUMENT), 7.x (QA)
**TransitionType:** documentation (DOCUMENT > no contribution in hierarchy)
**DomainId:** software-development
