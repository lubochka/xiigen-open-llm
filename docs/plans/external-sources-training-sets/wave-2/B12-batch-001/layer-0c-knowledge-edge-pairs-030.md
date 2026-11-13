# Layer 0-C — Knowledge Edge Pairs (030)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-030-001 — Idempotent: fires API_CONTRACT unconditionally (API contract term)

```
source_id: xiigen-b12-0c-030-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the idempotent payment submission API contract with idempotency key requirements."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R18: "idempotent" → API_CONTRACT unconditional ✓
    Rule B12-R18: "idempotency" → API_CONTRACT (same root term) ✓
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key edge: "idempotent" is a core API contract concept. It fires API_CONTRACT regardless of domain.
  "idempotency key" → "idempotency" fires API_CONTRACT + "key" is a modifier (no independent predicate).
  Boundary: "idempotent operation" without API context → may not fire API_CONTRACT.
            "idempotent API" or "idempotency key" → always fires API_CONTRACT.

---

## B12-0C-030-002 — Versioned endpoint: API_CONTRACT fires; "upgrade" does NOT fire IMPLEMENT

```
source_id: xiigen-b12-0c-030-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the versioned API endpoint contract for the customer service v2 release."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" + "endpoint" + "contract" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Boundary: "v2 release" does NOT fire IMPLEMENT. "release" is a publication verb, not construction.
  "versioned" is a modifier on the contract — does not change the predicate.
  Compare: "build the v2 API" → IMPLEMENT fires | "define the v2 contract" → IMPLEMENT does NOT fire.

---

## B12-0C-030-003 — Audit trail authentication: SECURITY always fires with "authentication"

```
source_id: xiigen-b12-0c-030-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Expose the audit trail API endpoint with authentication requirements for the compliance team."

B:
  CompiledPredicateIds: ["API_CONTRACT", "SECURITY"]
  Rule applications:
    Rule B12-R17c: "expose" + "api" + "endpoint" → API_CONTRACT ✓
    Rule B12-R06: "authentication" → SECURITY unconditional ✓
  v6-02 risk: classifier may suppress SECURITY thinking this is an API exposure context (admin/compliance context).
  Correct: SECURITY fires unconditionally whenever "authentication" is present. No context suppresses it.
  Key teaching: Even in an exposure/release context, "authentication" always fires SECURITY.
  DecisionCount = 2: two predicates = two routing decisions (3.3 + 3.4).
