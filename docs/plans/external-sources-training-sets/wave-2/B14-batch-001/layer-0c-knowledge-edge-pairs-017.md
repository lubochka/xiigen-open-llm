# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-017-001 — "scalability" fires ARCHITECTURE

A: "Design the scalability architecture for the real-time data processing platform."
  "design" → ARCHITECTURE | "scalability architecture" → ARCHITECTURE ("scalability", "architecture") | "real-time data processing platform" → ARCHITECTURE ("platform")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "scalability" is an ARCHITECTURE trigger noun. "platform" fires ARCHITECTURE.

---

## B14-edge-017-002 — "capacity" planning without design framing is gap

A: "Calculate the capacity needed for the holiday traffic peak."
  "calculate" → gap | "capacity needed" → gap (without design/architect framing) | "holiday traffic peak" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-scaling-001 — "capacity" as a metric calculation without architecture framing is gap.
  Teaching: "define the capacity model" → ARCHITECTURE. "calculate capacity" → gap.

---

## B14-edge-017-003 — "HPA" design fires ARCHITECTURE

A: "Design the HPA (Horizontal Pod Autoscaler) configuration strategy — CPU-based scaling, custom metrics, and cooldown periods."
  "design" → ARCHITECTURE | "HPA configuration strategy" → ARCHITECTURE ("strategy") | "CPU-based scaling" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "design" + "strategy" fires ARCHITECTURE. "HPA" → gap (platform tool), but does not suppress ARCHITECTURE.

---

## B14-edge-017-004 — "provision new servers" is gap

A: "Provision three new EC2 instances for the web tier."
  "provision" → gap (operational provisioning without IaC scripting context) | "EC2 instances" → gap | "web tier" → ARCHITECTURE? — "tier" without design context does not fire alone
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "provision" as direct operational action is gap.
  Teaching: "provision" fires IMPLEMENT only when in IaC scripting context: "write the Terraform module to provision EC2 instances."
