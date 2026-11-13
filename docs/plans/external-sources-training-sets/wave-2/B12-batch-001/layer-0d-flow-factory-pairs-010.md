# Layer 0-D — Flow Factory Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B12-0D-010-001 — v6 factory corruption: wrong sub-phase (3.1 instead of 3.3 for API_CONTRACT)

```
source_id: xiigen-b12-0d-010-001
corruption_type: v6-factory
pair_kind: factory_corruption
```

Request: "Document the REST API endpoints and webhook contracts for the payment service."

Correct factory output:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SelectedFlows: [APIContractFlow33, DocumentationFlow21]
  FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]
  PrimarySubPhase: "3.3"

v6-factory-corrupt output:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]  (correctly detected)
  SelectedFlows: [ArchitectureFlow31, DocumentationFlow21]  ← WRONG
  FlowOrder: ["ArchitectureFlow31", "DocumentationFlow21"]  ← WRONG
  PrimarySubPhase: "3.1"  ← WRONG

Corruption: API_CONTRACT incorrectly mapped to ArchitectureFlow31 (3.1) instead of
APIContractFlow33 (3.3). The factory confused API contract design with system architecture.

Why this is wrong:
  API_CONTRACT → 3.3 (APIContractFlow33) — this is the B12 primary routing
  ARCHITECTURE → 3.1 (ArchitectureFlow31) — this only fires when ARCHITECTURE predicate fires
  API_CONTRACT does NOT route to 3.1 unless ARCHITECTURE also fires

Detection: If API_CONTRACT present but ArchitectureFlow31 selected without ARCHITECTURE predicate
in CompiledPredicateIds → factory routing corruption.

---

## B12-0D-010-002 — v6 factory corruption: DOCUMENT mapped to 3.3 instead of 2.1

```
source_id: xiigen-b12-0d-010-002
corruption_type: v6-factory
pair_kind: factory_corruption
```

Correct:
  DOCUMENT predicate → DocumentationFlow21 at sub-phase 2.1

Corrupt:
  DOCUMENT predicate → APIContractFlow33 at sub-phase 3.3
  (corruption: DOCUMENT absorbed into API_CONTRACT flow instead of routing to Phase 2)

Why this is wrong:
  DOCUMENT is a Phase 2 predicate. It generates a separate routing decision to Phase 2.1.
  DOCUMENT does NOT route to Phase 3.3.
  Even when API_CONTRACT is present and routes to 3.3, DOCUMENT independently routes to 2.1.

Example of corrupt factory output:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Corrupt FlowOrder: ["APIContractFlow33"]  (DOCUMENT absorbed into API_CONTRACT flow)
  Correct FlowOrder: ["APIContractFlow33", "DocumentationFlow21"]  (two separate flows)

---

## B12-0D-010-003 — v6 factory corruption: TransitionType wrong (feature_addition for documentation-only)

```
source_id: xiigen-b12-0d-010-003
corruption_type: v6-factory
pair_kind: factory_corruption
```

Request: "Define the gRPC service contract for the user service."
CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]

Correct factory output:
  TransitionType: "documentation"
  (IMPLEMENT not in CompiledPredicateIds → documentation)

v6-factory-corrupt output:
  TransitionType: "feature_addition"
  (factory incorrectly assigned feature_addition because "API contract creation sounds like building something")

Why this is wrong:
  TransitionType is determined by presence of IMPLEMENT predicate, not by the nature of the task.
  "Define the gRPC service contract" = documentation verb ("define") → DOCUMENT fires, IMPLEMENT does NOT
  No IMPLEMENT → TransitionType = documentation

Detection: If CompiledPredicateIds lacks IMPLEMENT but TransitionType is "feature_addition" → corruption.

---

## B12-0D-010-004 — Factory corruption summary for B12

```
source_id: xiigen-b12-0d-010-004
pair_kind: factory_reference
```

B12 factory corruption patterns and detection:

| Corruption | Symptom | Detection | Correct behavior |
|------------|---------|-----------|-----------------|
| API_CONTRACT → wrong sub-phase | ArchitectureFlow31 selected without ARCHITECTURE predicate | API_CONTRACT in predicates but ArchitectureFlow31 in flows without ARCHITECTURE predicate | API_CONTRACT → APIContractFlow33 at 3.3 |
| DOCUMENT absorbed into 3.3 flow | Only APIContractFlow33 in FlowOrder despite DOCUMENT predicate | DOCUMENT in predicates but DocumentationFlow21 not in FlowOrder | DOCUMENT → DocumentationFlow21 at 2.1 (separate flow) |
| Wrong TransitionType | feature_addition without IMPLEMENT predicate | feature_addition but IMPLEMENT absent from CompiledPredicateIds | TransitionType = documentation when IMPLEMENT absent |
| SECURITY flow missing | SecurityAuditFlow34 absent despite SECURITY predicate | SECURITY in predicates but SecurityAuditFlow34 not in FlowOrder | SECURITY → SecurityAuditFlow34 at 3.4 |
| Routing collapse (v6-03) | Single flow despite multiple predicates | N predicates but M<N flows | One flow per predicate |
