# Layer 0-C — Knowledge Edge Pairs (032)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-032-001 — OAuth scope: SECURITY fires from "oauth" AND from "authentication" independently

```
source_id: xiigen-b12-0c-032-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the OAuth scope contract for the customer data API and document the authentication requirements."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  Rule applications:
    Rule B12-R06: "oauth" → SECURITY (authentication protocol trigger) ✓
    Rule B12-R06: "authentication" → SECURITY unconditional ✓
    Rule B12-R01: "contract" + "api" → API_CONTRACT ✓
    Rule B12-R04: "define"/"document" → DOCUMENT ✓
  Key: Both "oauth" and "authentication" independently trigger SECURITY. They result in ONE SECURITY
  predicate in CompiledPredicateIds (not two — SECURITY fires once regardless of how many triggers).
  DecisionCount reflects predicates, not triggers: three predicates = DecisionCount 3.

---

## B12-0C-032-002 — API key management: "contract" fires regardless of "management" modifier

```
source_id: xiigen-b12-0c-032-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the API key management contract for the developer portal."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional ("management contract" — modifier irrelevant) ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key: "management contract" fires API_CONTRACT via "contract". The modifier "key management" does
  not suppress or change the predicate. All contract modifiers are accepted.

---

## B12-0C-032-003 — Multi-version API: "multi-version" is modifier; API_CONTRACT fires via "api" + "contract"

```
source_id: xiigen-b12-0c-032-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the multi-version API contract strategy for the payments REST service."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Rule applications:
    Rule B12-R01: "api" → API_CONTRACT ✓
    Rule B12-R01: "contract" → API_CONTRACT unconditional ✓
    Rule B12-R01: "rest" → API_CONTRACT ✓
    Rule B12-R04: "define" → DOCUMENT ✓
  Key: "multi-version" is a contract modifier. It does not change the predicate outcome.
  "strategy" is a design word but does NOT fire ARCHITECTURE because "api" and "contract" are present —
  in B12, "strategy" alone (without "design" verb) does NOT fire ARCHITECTURE.
  Boundary: "design the multi-version API strategy" → would fire ARCHITECTURE (design + API = dual-fire).
