# Layer 0-B — Router Pairs (034)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-034-001 — TerraformModules routing: IMPLEMENT → 4.1 (primary)

IMPLEMENT → 4.1 is primary route for Terraform module creation:
  "write Terraform modules" → 4.1 | "implement reusable networking modules" → 4.1
  Terraform modules as IaC code artifacts = BackendServiceFlow41
  IMPLEMENT always routes to 4.1 regardless of "Terraform" tool name

---

## B14-router-034-002 — TerraformModules routing: ARCHITECTURE → 3.5 concurrent

When ARCHITECTURE fires alongside IMPLEMENT for Terraform:
  "design the Terraform module hierarchy" → 3.5 (hierarchy = structural vocabulary)
  ARCHITECTURE (3.5) runs before IMPLEMENT (4.1) — design precedes code
  Sequential: 3.5 → 4.1; both deterministic_generic

---

## B14-router-034-003 — TerraformModules routing: SECURITY → 6.3 for IAM modules

SECURITY → 6.3 when IAM + encryption vocabulary present:
  "write IAM policy modules and enforce encryption at rest" → 4.1 (IMPLEMENT) + 6.3 (SECURITY)
  IAM in security binding context → SECURITY → 6.3 | code artifact → IMPLEMENT → 4.1
  Three concurrent flows if ARCHITECTURE also fires: 3.5 → 6.3 → 4.1

---

## B14-router-034-004 — TerraformModules routing: "plan" is never ARCHITECTURE

"terraform plan" → zero predicates:
  "plan" does NOT fire ARCHITECTURE → no 3.5 routing
  "terraform apply" → zero predicates: operational action
  GapHandler receives signal for "plan" and "apply" vocabulary
  Only "design", "architect", "structure", "blueprint", "diagram" fire ARCHITECTURE
