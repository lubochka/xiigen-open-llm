# Layer 3 — Teaching Unit Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU017-001 — QA+ARCHITECTURE: infrastructure release design

A: "Design the infrastructure release gate architecture and QA the design."
B: ["ARCHITECTURE", "QA"] — "design" → ARCHITECTURE; "architecture" → ARCHITECTURE (dedup); "QA" → QA.
Reason: ARCHITECTURE fires on "design" verb and "architecture" noun. Deduplication yields one. TransitionType: documentation (IMPLEMENT absent).
CompiledPredicateIds: ["ARCHITECTURE", "QA"]

---

## TU017-002 — Gap: infrastructure provisioning

A: "Provision the Kubernetes cluster and deploy the Terraform infrastructure."
B: [] — "provision" → gap; "deploy infrastructure" → gap; "Kubernetes cluster" → gap.
Reason: Infrastructure provisioning vocabulary → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU017-003 — QA+ARCHITECTURE+IMPLEMENT: design + build + gate

A: "Design the release gate architecture, build the gate enforcement service, and QA approve."
B: ["ARCHITECTURE", "IMPLEMENT", "QA"] — "design" → ARCHITECTURE; "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA approve" → QA.
Reason: Three predicates. IMPLEMENT drives TransitionType: feature_addition.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "QA"]

---

## TU017-004 — QA+IMPLEMENT+DOCUMENT: IaC release pipeline

A: "Build the IaC pipeline release service, document the release criteria, and QA sign-off."
B: ["IMPLEMENT", "DOCUMENT", "QA"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "document" → DOCUMENT; "criteria" → DOCUMENT (dedup); "QA sign-off" → QA.
Reason: Three predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]

---

## TU017-005 — "IaC pipeline" alone is gap; "IaC pipeline service" fires IMPLEMENT

A: "Review the IaC pipeline." vs "Build the IaC pipeline service."
B1: [] — "IaC pipeline" alone → gap; "review" alone → gap.
B2: ["IMPLEMENT"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup).
Reason: "IaC pipeline" alone is gap vocabulary. Adding "service" fires IMPLEMENT via the "service" rule.
CompiledPredicateIds (B1): [] | CompiledPredicateIds (B2): ["IMPLEMENT"]
