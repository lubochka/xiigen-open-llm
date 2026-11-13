# Layer 3 — Teaching Unit Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T008-001 — "VPC" fires ARCHITECTURE in topology design

Rule: "VPC" (Virtual Private Cloud) in design/topology context → ARCHITECTURE
  "design the VPC peering topology" → ARCHITECTURE
  "create a VPC" → gap (operational command)
  Teaching: "VPC" fires ARCHITECTURE when inside a design-framing sentence. Operational VPC commands are gap.

---

## T008-002 — "set up" is gap for all predicates

Rule: "set up" → gap for ARCHITECTURE, IMPLEMENT, and all other predicates
  "set up Kubernetes" → gap | "set up the database" → gap
  Teaching: "set up" is universally gap. It is an operational configuration action, not a predicate trigger.

---

## T008-003 — mTLS fires SECURITY even in architecture context

Rule: "mTLS" (mutual TLS) → SECURITY even inside architecture design sentences
  "design the service mesh with mTLS policy" → ARCHITECTURE + SECURITY
  Teaching: Security vocabulary (mTLS, TLS, encryption) fires SECURITY independently of ARCHITECTURE context. Both predicates fire.

---

## T008-004 — "define" in structural context fires ARCHITECTURE

Rule: "define" + structural noun (boundary, topology, rules, strategy, policy, layer) → ARCHITECTURE
  "define the network boundary rules" → ARCHITECTURE
  "define the partitioning strategy" → ARCHITECTURE
  "define the requirements" → gap ("define" without structural noun is gap)
  Teaching: "define" fires ARCHITECTURE only with structural design nouns as objects.

---

## T008-005 — Interrogative architecture questions are gap

Rule: Questions about architecture without design imperative → gap
  "is our architecture scalable?" → gap
  "does the topology support high availability?" → gap
  Teaching: Architecture questions are gap. ARCHITECTURE requires an imperative design action.

