# Layer 0-A — Compiler Pairs (037)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-037-001 — CostOptimization: ARCHITECTURE fires on cost design vocabulary

"cost optimization" → ARCHITECTURE fires on "optimization" in design context + "cost architecture"
"design the cost allocation model" → ARCHITECTURE ("design", "model")
"architect a cost-efficient topology" → ARCHITECTURE ("architect", "topology")
"plan a FinOps framework" → gap ("plan" is gap; "FinOps" is gap)

---

## B14-compiler-037-002 — CostOptimization: IMPLEMENT fires on cost IaC vocabulary

"implement cost allocation tags" → IMPLEMENT ("implement" + "tags" as configuration artifact)
"write the Terraform budget alert modules" → IMPLEMENT ("write" + "modules")
"configure cost anomaly detection rules" → IMPLEMENT ("configure" in code-artifact context + "rules")
"set up billing alerts" → gap ("set up" is operational gap)

---

## B14-compiler-037-003 — CostOptimization: SECURITY fires on cost-related credential vocabulary

"enforce IAM cost boundary permissions" → SECURITY + AUTHORIZATION ("enforce" in access context; "permission boundary")
"restrict billing account access" → SECURITY + AUTHORIZATION ("restrict" in access policy context)
"encrypt cost data at rest" → SECURITY ("encrypt", "at rest")

---

## B14-compiler-037-004 — CostOptimization: DOCUMENT fires on cost documentation vocabulary

"document the cost optimization ADR" → DOCUMENT ("document" + "ADR")
"write the FinOps runbook" → DOCUMENT ("write" + "runbook")
"record cost allocation decisions" → DOCUMENT ("record" in documentation context)
CompiledPredicateIds: ["DOCUMENT"] for documentation verbs + documentary nouns

---

## B14-compiler-037-005 — CostOptimization: multi-predicate compilation

"Design the cost-efficient infrastructure topology, implement budget alert modules, and document the cost ADR."
  "design" → ARCHITECTURE | "topology" → ARCHITECTURE | "implement" → IMPLEMENT | "modules" → IMPLEMENT | "document" → DOCUMENT | "ADR" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  CapabilityGapWarnings: gap-devops-cloud-001 ("FinOps", "budget", "cost" labels are gap)
