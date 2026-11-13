# Layer 3 — Teaching Unit Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T022-001 — "zero-trust" fires SECURITY AND ARCHITECTURE together

A: "Design the zero-trust network topology." Predicates?
B: ["ARCHITECTURE", "SECURITY"]. "design" → ARCHITECTURE. "zero-trust network topology" → ARCHITECTURE via "topology" + SECURITY via "zero-trust". Both fire from the same phrase. Two flows: 3.5 + 6.3.

---

## T022-002 — "open port 443" is operational gap

A: "Open port 443 on the security group." Predicates?
B: Zero predicates. "open port" → operational firewall action, gap. "security group" → gap (platform resource). "port 443" → gap. Result: [] → knowledge_edge.

---

## T022-003 — "VPC network topology" fires ARCHITECTURE

A: "Design the VPC network topology." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "VPC network topology" → ARCHITECTURE via "topology" and "network" (in design context). "VPC" → gap but doesn't suppress.

---

## T022-004 — "NetworkPolicy deny-all" fires ARCHITECTURE via "boundary"

A: "Define the NetworkPolicy deny-all baseline for the production namespace." Predicates?
B: ["ARCHITECTURE"]. "define" → ARCHITECTURE. "NetworkPolicy deny-all baseline" → ARCHITECTURE via "boundary" (deny-all = boundary definition). Route to 3.5.

---

## T022-005 — ARCHITECTURE+IMPLEMENT+SECURITY for VPC security IaC

A: "Design the VPC security topology, implement the security group rules, and configure the TLS certificates." Predicates?
B: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]. "design" + "VPC security topology" → ARCHITECTURE. "implement" + "security group rules" → IMPLEMENT. "TLS certificates" → SECURITY ("tls", "certificate"). "configure" → IMPLEMENT. Three flows: 3.5 + 4.1 + 6.3. TransitionType: feature_addition.
