# Layer 0-C — Knowledge Edge Pairs (039)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-039-001 — ReservedCapacity: gap vocabulary — financial capacity operations

Confirmed gap vocabulary for reserved capacity financial operations (zero predicates):
  Action verbs: purchase, buy, renew, commit (financial), exchange, sell (marketplace), modify (reservation modification)
  Financial nouns: RI (Reserved Instance), savings plan, committed use discount, convertible RI, standard RI, upfront payment, partial upfront, no upfront
  None fire any B14 predicate.

---

## B14-edge-039-002 — ReservedCapacity: AUTHORIZATION boundary — capacity admin roles

AUTHORIZATION fires for these reserved-capacity access control nouns:
  "capacity-admin role", "billing IAM role", "reserved instance management permission", "capacity reservation purchase role"
  "grant the billing IAM role" → AUTHORIZATION | "authorize the capacity-admin role" → AUTHORIZATION
  "purchase the reserved instances" → gap ("purchase" is financial gap)

---

## B14-edge-039-003 — ReservedCapacity: disambiguation — "commit" as financial vs design verb

"commit" (financial): "commit to a 3-year savings plan" → gap (financial commitment)
"commit" (git): "commit the Terraform code" → gap (GitOps operational)
"commit" (design): not a B14 design verb; "commit" does not fire ARCHITECTURE
Rule: "commit" in all its B14 contexts is gap. It does not fire any predicate.

---

## B14-edge-039-004 — ReservedCapacity: ARCHITECTURE boundary — capacity design vocabulary

ARCHITECTURE fires only when capacity vocabulary is paired with design verbs:
  "architect the reserved capacity model" → ARCHITECTURE | "design the capacity reservation topology" → ARCHITECTURE
  "reserved capacity planning" alone → gap | "capacity model" alone → gap
  The design verb must be present.
