# Layer 3 — Teaching Unit Pairs (016)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-016-A — "catalog architecture" fires ARCHITECTURE

A: "Design the catalog architecture and implement the product listing service." How many predicates?
B: Two: ["ARCHITECTURE", "IMPLEMENT"]. "architecture" is a direct ARCHITECTURE keyword — no additional phrase required. "design" reinforces the intent but "architecture" is the trigger.

## TU-016-B — ARCHITECTURE always before IMPLEMENT in ordering

A: When ARCHITECTURE and IMPLEMENT both fire, which flow executes first?
B: ArchitectureFlow31 (sub-phase 3.1) always executes before ImplementationFlow41 (sub-phase 4.1). The architecture output (domain model spec) feeds into the implementation phase as N01 input. This ordering is a hard rule — not configurable per request.

## TU-016-C — "product variants" alone does not fire ARCHITECTURE

A: "Build the catalog service to support product variants." Does ARCHITECTURE fire?
B: No. "variants" is a domain noun. ARCHITECTURE requires "model" / "bounded context" / "architecture" / "architect". "product variants" alone → only IMPLEMENT fires. However: "Build the catalog service and define the product variant model" → ARCHITECTURE fires on "variant model".

## TU-016-D — Four predicates fire: catalog full pipeline

A: "Design the catalog domain model, define the product API contracts, build the catalog service, and document the product endpoints." How many predicates and routing decisions?
B: Four predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "DOCUMENT"]. Four routing decisions: ArchitectureFlow31 (3.1), APIContractFlow33 (3.3), ImplementationFlow41 (4.1), DocumentationFlow21 (2.1). Ordering: 3.1 → 3.3 → 4.1 ‖ 2.1.

## TU-016-E — NearestWrongSubPhaseScore: 3.1 vs 3.3

A: When routing ARCHITECTURE to 3.1 and API_CONTRACT to 3.3 separately, what is the NearestWrongSubPhaseScore?
B: For ArchitectureFlow31 (3.1): NearestWrong = 3.3 (score: 0.38). For APIContractFlow33 (3.3): NearestWrong = 3.1 (score: 0.36). Both are confidently above routing threshold. They route independently — each to its own sub-phase.
