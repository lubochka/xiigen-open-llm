# Layer 0-A — Compiler Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-006-001 — IMPLEMENT + API_CONTRACT + DOCUMENT

**Input:**
"Build the Plaid banking connector and document the account linking and transaction retrieval interfaces."

**Compiler analysis:**
- "Build" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4)
- "document" → DOCUMENT (Rule 12: "document" fires DOCUMENT)
- "interfaces" → DOCUMENT reinforcer (documenting interfaces)
- Integration vocabulary: "connector", "banking" → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT)
**TransitionType:** feature_addition (Rule 16: IMPLEMENT present, no refactor verb)
**DomainId:** software-development

---

## B17-0A-006-002 — "document" fires DOCUMENT; "spec out" also fires DOCUMENT

**Input A:**
"Document the Plaid transaction API interface."
→ DOCUMENT only → 2.1

**Input B:**
"Spec out the Plaid transaction API interface."
→ DOCUMENT only (Rule 12: "spec out" / "spec" fires DOCUMENT) → 2.1

**Compiler analysis:**
Both "document" and "spec out" fire DOCUMENT. Neither fires IMPLEMENT or API_CONTRACT alone. Both route to sub-phase 2.1.

**CompiledPredicateIds (A):** ["DOCUMENT"]
**CompiledPredicateIds (B):** ["DOCUMENT"]
**SubPhase:** 2.1
**TransitionType:** documentation (DOCUMENT alone, no IMPLEMENT)
**DomainId:** software-development

---

## B17-0A-006-003 — "document the interfaces" with "build connector" → three predicates

**Input:**
"Build the PlaidBankingConnector, wire up the account-link and balance APIs, and document the integration interface."

**Compiler analysis:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "wire up" → API_CONTRACT (reinforces)
- "APIs" → API_CONTRACT (reinforces)
- "document" → DOCUMENT
- Integration vocabulary: "connector", "integration", "APIs" → 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**SubPhase:** 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT)
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-006-004 — DOCUMENT does not affect IMPLEMENT routing

**Input:**
"Document the PaymentProcessorService and its discount logic methods."

**Compiler analysis:**
- "Document" → DOCUMENT
- "service", "logic", "methods" → service vocabulary
- No build verb → IMPLEMENT does not fire
- No integration vocabulary
- DOCUMENT alone → 2.1

**CompiledPredicateIds:** ["DOCUMENT"]
**SubPhase:** 2.1
**TransitionType:** documentation
**DomainId:** software-development

**Teaching point:** DOCUMENT fires on "document" regardless of context. It routes to 2.1 independently of IMPLEMENT.
