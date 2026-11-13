# Layer 0-A — Compiler Pairs (043)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-043-001 — AnonymizationService: IMPLEMENT from "build"

A: "Build an AnonymizationService to mask PII (Personally Identifiable Information) fields before export."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT.

---

## B15-compiler-043-002 — AnonymizationService: IMPLEMENT+SECURITY on "protect"

A: "Build AnonymizationService and protect the raw data access endpoint."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "protect" → SECURITY.

---

## B15-compiler-043-003 — AnonymizationService: IMPLEMENT+SECURITY on "encrypt"

A: "Build AnonymizationService and encrypt the PII mapping table."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "encrypt" → SECURITY. Single SECURITY entry (deduplication not needed — only one trigger here).

---

## B15-compiler-043-004 — AnonymizationService: IMPLEMENT+ARCHITECTURE on "anonymization model"

A: "Build AnonymizationService with an anonymization domain model covering all PII field categories."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "anonymization domain model" → ARCHITECTURE.

---

## B15-compiler-043-005 — IMPLEMENT+SECURITY deduplication when "protect" and "encrypt" both present

A: "Build AnonymizationService, protect the raw data endpoint, and encrypt the PII mapping table."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "protect" → SECURITY. "encrypt" → SECURITY. Deduplication → one SECURITY entry only.
