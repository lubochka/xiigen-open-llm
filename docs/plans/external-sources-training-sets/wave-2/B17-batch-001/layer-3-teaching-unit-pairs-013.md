# Layer 3 — Teaching Unit Pairs (013)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: teaching-unit | focus: Stripe payment connector

---

## B17-TU-013-001 — "integrate Stripe" → API_CONTRACT only; "build Stripe connector" → IMPLEMENT + API_CONTRACT

**A:** What is the difference between "Integrate the Stripe API" and "Build the Stripe payment connector"?

**B:**
- "Integrate the Stripe API" → ["API_CONTRACT"] → sub-phase 3.3 → documentation (no connector code)
- "Build the Stripe payment connector" → ["IMPLEMENT", "API_CONTRACT"] → sub-phase 4.3 + 3.3 → connector code + API contract

The single word "integrate" (vs "build") determines whether connector code is produced. "integrate" fires API_CONTRACT only. "build" fires IMPLEMENT (which activates IntegrationConnectorFlow43 and produces code).

---

## B17-TU-013-002 — Stripe "idempotency token" fires SECURITY (Rule 6)

**A:** In "Build the Stripe connector with idempotency token handling", does "idempotency token" fire SECURITY?

**B:** Yes. Rule 6 is unconditional: "token" fires SECURITY regardless of token type. An idempotency token is not a security credential, but the word "token" still fires SECURITY. The predicate system fires on the word "token", not on whether the token is a security mechanism.

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Teaching point:** The SECURITY predicate is not semantic (it does not reason about whether the token is security-related). It is lexical (it fires on the word "token"). This is by design — every token is tracked as a potential security concern.

---

## B17-TU-013-003 — Stripe "refactor" → refactoring TransitionType

**A:** "Refactor the Stripe payment connector to use the Stripe API v2024-06-20 webhook schema." What predicates fire? What is TransitionType?

**B:**
- "Refactor" → IMPLEMENT (Rule 14)
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5; reinforces)
- "API" → API_CONTRACT (reinforces)
- **CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
- **TransitionType:** refactoring (refactor verb is present — overrides feature_addition)
- **SubPhase:** 4.3 + 3.3

**Rule 14:** "refactor" fires IMPLEMENT AND sets TransitionType to refactoring. The predicate set and routing are identical to a feature_addition build — only the TransitionType metadata changes.

---

## B17-TU-013-004 — Stripe PCI-DSS guard: business rule enforced at N08

**A:** The Stripe connector has a PCI-DSS (Payment Card Industry — Data Security Standard) guard that prevents storing raw card data. Where in the nine-node flow is this enforced?

**B:** The PCI guard is a business rule (BR-STR-002) enforced in N04 (DomainProcessorUnit) as a specification requirement and verified by N08 (ArbiterUnit) as a Gate B condition. The Arbiter explicitly checks that ChargeRequest does not accept CardNumber, CVV (Card Verification Value), or raw PAN (Primary Account Number) — only PaymentMethodId is accepted.

The nine-node flow encodes business rules at N04 and verifies them at N08. The PCI guard is not a predicate — it is a domain-specific constraint on the generated connector.
