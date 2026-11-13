# Layer 0-C — Knowledge Edge Pairs (037)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-037-001 — CostOptimization: gap vocabulary — financial and FinOps operations

Confirmed gap vocabulary for cost optimization financial operations (zero predicates):
  Action verbs: purchase, buy, renew, commit (financial), exchange, modify (reservation), activate, deactivate
  Financial nouns: savings plan, reserved instance, on-demand, spot (standalone), committed use discount, FinOps, billing alarm, cost explorer, budget alert (standalone), anomaly detection alert
  None of the above fire any B14 predicate.

---

## B14-edge-037-002 — CostOptimization: IMPLEMENT boundary — cost IaC artifacts

IMPLEMENT fires for cost-related IaC artifact nouns following a creation verb:
  budget alert module, cost allocation tag configuration, anomaly detection rule file, billing IAM policy module, cost anomaly detection configuration
  "implement cost allocation tags" → IMPLEMENT | "configure cost anomaly detection rules" → IMPLEMENT (configure + rule file)
  "view the cost dashboard" → gap ("view" is operational gap)

---

## B14-edge-037-003 — CostOptimization: ARCHITECTURE boundary — cost design vocabulary

ARCHITECTURE fires when cost vocabulary appears with design verbs:
  "design the cost allocation model" → ARCHITECTURE | "architect a cost-efficient topology" → ARCHITECTURE
  "cost optimization" alone → gap | "FinOps framework" alone → gap
  The cost noun must accompany a design verb to fire ARCHITECTURE.

---

## B14-edge-037-004 — CostOptimization: disambiguation — "tag" is context-dependent

"tag" noun: fires IMPLEMENT when in IaC-artifact context ("implement cost allocation tags" → IMPLEMENT)
"tag" as verb in kubectl context: "label the node" = operational gap; "annotate the resource" = operational gap
"tag" in git context: "git tag the release" = gap (GitOps operational)
