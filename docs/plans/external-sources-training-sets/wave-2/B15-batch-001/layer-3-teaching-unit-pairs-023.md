# Layer 3 — Teaching Unit Pairs (023)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-023-A — "architect" verb fires ARCHITECTURE

A: "Architect the tax calculation engine and implement the rate lookup." How many predicates?
B: Two: ["ARCHITECTURE", "IMPLEMENT"]. "architect" (verb) → ARCHITECTURE fires. "implement" → IMPLEMENT fires. Both predicates fire independently — ARCHITECTURE does not absorb IMPLEMENT.

## TU-023-B — "tax model" fires ARCHITECTURE

A: "Build the tax service and define the tax model for multi-jurisdiction compliance." How many predicates?
B: Three: ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]. "build" → IMPLEMENT; "tax model" → ARCHITECTURE fires on "model"; "define" → DOCUMENT fires. Three independent predicates, three routing decisions.

## TU-023-C — "exemption categories" alone does not fire ARCHITECTURE

A: "Implement the tax exemption category lookup service." Does ARCHITECTURE fire?
B: No. "categories" alone does not fire ARCHITECTURE. Only "category model" or "exemption model" would fire ARCHITECTURE (because "model" is the keyword). Plain domain nouns without "model" / "bounded context" / "architecture" do not trigger ARCHITECTURE.

## TU-023-D — Six-predicate TaxService: full pipeline

A: "Design tax domain model, define tax API contracts, build tax service, secure tax data endpoints, write tax calculation tests, document the tax rules." How many predicates and what order?
B: Six predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]. Ordering: ArchitectureFlow31 → APIContractFlow33 → SecurityAuditFlow → ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21. QA absent from this request.

## TU-023-E — TaxService: OperationResult<T> on tax calculation

A: IsTaxExempt(CustomerId, ProductId) returns bool — is this acceptable?
B: No. Rule 1: every public method returns OperationResult<T>. Correct signature: IsTaxExempt(CustomerId, ProductId) → OperationResult<bool>. Even for methods that return a simple boolean result, the OperationResult<T> wrapper is required to convey failure mode (e.g., jurisdiction lookup failed, database unavailable).
