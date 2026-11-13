# Held-Out and Corruption Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "structure" + "infrastructure" → ARCHITECTURE at 3.5

A: "Structure the Kubernetes networking infrastructure and define the service communication patterns."
Expected: ARCHITECTURE fires on "structure" + "infrastructure" + "patterns" (architecture pattern term).
"Kubernetes" is gap. "service communication" → ARCHITECTURE via "patterns" and structural context.
CompiledPredicateIds: ["ARCHITECTURE"]. Routing: 3.5 (infra context).
CapabilityGapWarnings: gap-devops-cloud-001 (Kubernetes).

---

## HO-002 — "blueprint" fires ARCHITECTURE; "write the blueprint" also fires IMPLEMENT

A: "Write the infrastructure blueprint for the containerized deployment."
Expected: IMPLEMENT fires on "write". ARCHITECTURE fires on "blueprint" + "infrastructure".
"containerized" is gap. CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"].
Routing: IMPLEMENT → 4.1, ARCHITECTURE → 3.5. TransitionType: feature_addition.

---

## HO-003 — pure cloud platform vocabulary: knowledge_edge

A: "Migrate the workloads to Azure Kubernetes Service."
Expected: zero predicates → knowledge_edge. "migrate" fires IMPLEMENT? No: "migrate"
in a data migration context fires IMPLEMENT (4.4), but "migrate workloads" to a cloud
platform is DevOps migration — "workloads" is not in IMPLEMENT lexicon as a data migration
noun. Actually: "migrate" → IMPLEMENT fires (migrate is in IMPLEMENT lexicon at 4.4).
Correction: CompiledPredicateIds: ["IMPLEMENT"] via "migrate". Routing: 4.4.
"Azure Kubernetes Service" and "workloads" → gap. CapabilityGapWarnings: gap-devops-cloud-001.

---

## COR-001 — v6-01: ARCHITECTURE invented for "scale the service"

CORRUPT: "Scale the order service to handle peak traffic." → CompiledPredicateIds: ["ARCHITECTURE"].
Why corrupt: "scale" is not in the ARCHITECTURE predicate lexicon. The corrupt model
inferred that scaling involves architectural decisions. But predicates fire on lexicon
terms — "scale" is gap vocabulary. Correct: CompiledPredicateIds: [] → knowledge_edge.
Gap ID: gap-devops-scaling-001.

---

## COR-002 — v6-01: IMPLEMENT invented for cloud platform setup

CORRUPT: "Set up the AWS ECS cluster for the microservices." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "set up" is not in the IMPLEMENT lexicon. AWS, ECS, cluster are all gap.
The corrupt model treated "set up infrastructure" as implementation work. Correct:
CompiledPredicateIds: [] → knowledge_edge. To fire IMPLEMENT: "Build the ECS deployment config."

---

## COR-003 — v6-03: ARCHITECTURE at 3.5 and 3.1 collapsed to single routing

CORRUPT: Request with both infra vocabulary (infrastructure, layer) and app vocabulary
(service, domain) → single ARCHITECTURE routing to 3.1 only.
Why corrupt: ARCHITECTURE fires once but sub-phase disambiguation should produce 3.5
for the infra dimension. The corrupt model defaulted all ARCHITECTURE to 3.1. Correct:
When compiled text contains both infra vocabulary (infrastructure, deployment layer, network)
AND app vocabulary, the dominant context (higher density terms) determines sub-phase.
Dense infra vocabulary → 3.5; dense app vocabulary → 3.1.
