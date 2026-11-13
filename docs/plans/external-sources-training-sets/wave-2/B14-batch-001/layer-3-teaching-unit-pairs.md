# Layer 3 — Teaching Unit Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B14B1-WDA-teach-001 — ARCHITECTURE routes to 3.5 for infra context vs 3.1 for app context

A: "Design the system architecture for the order service" vs "Design the cloud infrastructure architecture."
   Both fire ARCHITECTURE. Do they both route to 3.1?
B: No. "Design the system architecture for the order service" → ARCHITECTURE → 3.1
   (System Architecture — application-level service boundary design).
   "Design the cloud infrastructure architecture" → ARCHITECTURE → 3.5
   (Infrastructure and DevOps Architecture — cloud/infra layer design).
   Sub-phase disambiguation: "cloud", "infrastructure", "deployment layer", "network",
   "service mesh" vocabulary in the compiled text signals 3.5. Application-level terms
   (service, domain, module) signal 3.1.

---

## Pair B14B1-WDA-teach-002 — cloud platform names are gap; action verbs still fire predicates

A: "Design the AWS infrastructure for the application." Does ARCHITECTURE fire?
B: Yes. ARCHITECTURE fires on "design" + "infrastructure". "AWS" is gap vocabulary (cloud
provider name not in any predicate lexicon). Gap vocabulary in the sentence produces
CapabilityGapWarnings but does not suppress predicates that fire on action verbs and
structural nouns. CompiledPredicateIds: ["ARCHITECTURE"]. Routing: 3.5.
CapabilityGapWarnings: gap-devops-cloud-001.

---

## Pair B14B1-WDA-teach-003 — "scale" is gap; "design a scaling architecture" fires ARCHITECTURE

A: "Scale the API service." vs "Design the horizontal scaling architecture for the API service."
B: "Scale the API service" → zero predicates → knowledge_edge. "scale" has no predicate.
   "Design the horizontal scaling architecture for the API service" → ARCHITECTURE fires on
   "design" + "architecture". "horizontal scaling" in context of architectural design fires
   ARCHITECTURE via the design action. The difference: the action verb "scale" (gap) vs
   "design the architecture" (ARCHITECTURE terms).

---

## Pair B14B1-WDA-teach-004 — "document + diagram" → documentation TransitionType, not feature_addition

A: "Document the infrastructure architecture and create a diagram." TransitionType?
B: documentation. The primary action verbs are "document" and "diagram" — both produce
documentary artifacts from existing architectural decisions. TransitionType is determined
by whether net-new artifacts are being designed (feature_addition) or existing decisions
are being recorded (documentation). "Design the architecture" → feature_addition (creating).
"Document the architecture" → documentation (recording).

---

## Pair B14B1-WDA-teach-005 — "infrastructure" as a noun fires ARCHITECTURE independently

A: "Write the Terraform code for the networking infrastructure." What predicates fire?
B: IMPLEMENT fires on "write" + "code". ARCHITECTURE fires on "infrastructure" (structural noun).
"Terraform" is gap. "networking infrastructure" → "infrastructure" fires ARCHITECTURE.
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]. Routing: IMPLEMENT → 4.1, ARCHITECTURE → 3.5.
"Infrastructure" as a structural noun fires ARCHITECTURE independently of any design action verb.
