# Layer 0-A — Compiler Pairs (039)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-039-001 — ReservedCapacity: ARCHITECTURE fires on capacity design vocabulary

"architect the reserved capacity model for compute tiers" → ARCHITECTURE ("architect", "model")
"design a capacity reservation topology" → ARCHITECTURE ("design", "topology")
"structure reserved vs on-demand allocation boundaries" → ARCHITECTURE ("structure", "boundaries")
"purchase reserved instances" → gap ("purchase" is financial gap; "reserved instances" is gap)

---

## B14-compiler-039-002 — ReservedCapacity: IMPLEMENT fires on capacity IaC artifacts

"implement the capacity reservation Terraform module" → IMPLEMENT ("implement" + "module")
"write the savings plan configuration script" → IMPLEMENT ("write" + "script")
"build the reserved node group manifest" → IMPLEMENT ("build" + "manifest")

---

## B14-compiler-039-003 — ReservedCapacity: AUTHORIZATION fires on capacity access control

"authorize the capacity reservation purchase role" → AUTHORIZATION ("authorize" + "role")
"grant billing IAM role for reserved instance management" → AUTHORIZATION ("grant" + "IAM role")
"define ClusterRole binding for capacity admin" → AUTHORIZATION ("ClusterRole", "binding")

---

## B14-compiler-039-004 — ReservedCapacity: gap vocabulary — financial operations

Confirmed gap verbs/nouns for reserved capacity financial operations:
  purchase, buy, renew, commit (financial), sell (marketplace), exchange, modify (reservation modification)
  "purchase RI (Reserved Instance)" → gap | "buy savings plan" → gap | "renew reservation" → gap
  None of these fire any B14 predicate.

---

## B14-compiler-039-005 — ReservedCapacity: DOCUMENT compilation

"document the reserved capacity ADR and savings model" → DOCUMENT ("document" + "ADR")
"write the capacity planning runbook" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]
CapabilityGapWarnings: gap-devops-cloud-001 ("reserved instances", "savings plan", "committed use" are gap)
