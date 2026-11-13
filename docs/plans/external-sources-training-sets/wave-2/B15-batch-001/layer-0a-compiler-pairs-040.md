# Layer 0-A — Compiler Pairs (040)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-040-001 — ComplianceService: IMPLEMENT from "build"

A: "Build a ComplianceService to enforce regulatory rules across all service transactions."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Confidence: 0.97
"build" → IMPLEMENT. "compliance", "regulatory" are domain nouns — do not fire predicates.

---

## B15-compiler-040-002 — ComplianceService: IMPLEMENT+SECURITY on "secure"

A: "Build ComplianceService and secure all audit log endpoints."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
"build" → IMPLEMENT. "secure" → SECURITY.

---

## B15-compiler-040-003 — ComplianceService: IMPLEMENT+ARCHITECTURE on "compliance model"

A: "Build ComplianceService with a compliance domain model for rule enforcement."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | TransitionType: feature_addition
"build" → IMPLEMENT. "compliance domain model" → ARCHITECTURE.

---

## B15-compiler-040-004 — ComplianceService: IMPLEMENT+DOCUMENT on "document"

A: "Build ComplianceService and document the regulatory rule catalog."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
"build" → IMPLEMENT. "document" → DOCUMENT.

---

## B15-compiler-040-005 — "enforce" does NOT fire SECURITY

"enforce" in the context of compliance rules does not fire SECURITY.
SECURITY fires from: secure, harden, protect, encrypt, authenticate.
"enforce compliance rules" → IMPLEMENT only. v6-01 risk: "enforce" → SECURITY incorrectly added.
