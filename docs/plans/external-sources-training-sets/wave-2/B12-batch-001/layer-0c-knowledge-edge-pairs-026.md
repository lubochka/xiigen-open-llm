# Layer 0-C — Knowledge Edge Pairs (026)
# Batch: B12-batch-001 | Scenario: B12 — API contracts and integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B12-0C-026-001 — gRPC: "connect" is integration verb — API_CONTRACT fires, IMPLEMENT does NOT

```
source_id: xiigen-b12-0c-026-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Connect the gRPC billing service to the payment orchestrator service."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17b: "connect" → API_CONTRACT (integration verb in API context) ✓
    Rule B12-R01: "grpc" → API_CONTRACT ✓
  Boundary: "connect" is NOT IMPLEMENT. Connect = integration (how two services talk). Build = construction (creating code).
  Edge: connect, wire up, integrate, expose, publish → API_CONTRACT only; implement, build, create, write → IMPLEMENT + API_CONTRACT

---

## B12-0C-026-002 — gRPC "publish endpoint": API_CONTRACT fires, IMPLEMENT does NOT

```
source_id: xiigen-b12-0c-026-002
pair_kind: knowledge_edge
```

A:
  CompiledText: "Publish the gRPC endpoint for the billing service to the internal service registry."

B:
  CompiledPredicateIds: ["API_CONTRACT"]
  Rule applications:
    Rule B12-R17c: "publish endpoint" → API_CONTRACT ✓
    Rule B12-R01: "grpc" → API_CONTRACT ✓
  Boundary: "publish" is a release verb. It fires API_CONTRACT but NOT IMPLEMENT.
  Common corruption (v6-01): treating "publish" as IMPLEMENT because publication implies some action.
  Correct: publish → API_CONTRACT only; no IMPLEMENT.

---

## B12-0C-026-003 — gRPC implement: both IMPLEMENT and API_CONTRACT fire simultaneously

```
source_id: xiigen-b12-0c-026-003
pair_kind: knowledge_edge
```

A:
  CompiledText: "Implement the gRPC service for the billing module and create the protobuf message types."

B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Rule applications:
    Rule B12-R08: "implement"/"create" → IMPLEMENT ✓
    Rule B12-R01: "grpc" → API_CONTRACT ✓
    Rule B12-R10: IMPLEMENT present → TransitionType = feature_addition ✓
  Edge: When a construction verb (implement, build, create) is present alongside a gRPC trigger, BOTH
  IMPLEMENT and API_CONTRACT fire. Neither suppresses the other.
  TransitionType moves to feature_addition.
