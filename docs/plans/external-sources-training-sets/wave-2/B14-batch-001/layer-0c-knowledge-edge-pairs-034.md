# Layer 0-C — Knowledge Edge Pairs (034)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-034-001 — TerraformModules: "plan" and "apply" are always gap

Gap: "terraform plan" — "plan" does NOT fire ARCHITECTURE (not in design/architect/structure/blueprint/diagram set)
Gap: "terraform apply" — "apply" is operational action → gap
Gap: "terraform destroy" — "destroy" is operational → gap
Teaching: Only design/architect/structure/blueprint/diagram fire ARCHITECTURE. "plan" is not in this set and never fires ARCHITECTURE.

---

## B14-edge-034-002 — TerraformModules: module hierarchy fires ARCHITECTURE

ARCHITECTURE fires: "design the Terraform module hierarchy" — "hierarchy" = structural noun; "design" = ARCHITECTURE verb
ARCHITECTURE fires: "architect the module dependency structure" — "architect" + "structure" fires ARCHITECTURE
Gap: "list the Terraform module versions" (operational), "view the module registry" (operational)
Teaching: Structural nouns (hierarchy, structure, dependency graph) with design verbs fire ARCHITECTURE.

---

## B14-edge-034-003 — TerraformModules: IAM boundary between IMPLEMENT and SECURITY

IMPLEMENT fires: "write the IAM policy modules" — code artifact noun (policy modules = IaC artifact) → IMPLEMENT
SECURITY fires: "enforce encryption at rest" — "encryption at rest" is listed SECURITY compound phrase
Both fire from same sentence: "write the IAM policy modules and enforce encryption at rest" → IMPLEMENT + SECURITY
Teaching: Code artifact creation and security enforcement in same sentence fire both predicates independently.

---

## B14-edge-034-004 — TerraformModules: DOCUMENT vs IMPLEMENT for "write"

DOCUMENT fires: "write the Terraform module API reference" — "API reference" = documentary artifact → DOCUMENT
IMPLEMENT fires: "write the Terraform networking module" — "networking module" = code artifact → IMPLEMENT
Teaching: Same verb "write" produces different predicates based on the artifact noun: documentary artifact → DOCUMENT, code artifact → IMPLEMENT.
