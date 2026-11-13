# Layer 0-A — Compiler Pairs (038)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-038-001 — SpotInstances: ARCHITECTURE fires on spot topology design

"architect a spot instance fleet topology" → ARCHITECTURE ("architect", "topology")
"design the spot interruption handler" → ARCHITECTURE ("design")
"structure spot and on-demand node groups" → ARCHITECTURE ("structure")
"use spot instances" → gap ("use" is operational gap; "spot instances" is gap)

---

## B14-compiler-038-002 — SpotInstances: IMPLEMENT fires on spot IaC artifacts

"implement the spot instance node group Terraform module" → IMPLEMENT ("implement" + "module")
"write the spot interruption handler script" → IMPLEMENT ("write" + "script")
"scaffold the mixed instance policy configuration" → IMPLEMENT ("scaffold" + "configuration")
"configure spot pools" → gap ("configure" without code artifact context is operational gap)

---

## B14-compiler-038-003 — SpotInstances: SECURITY fires on spot credential vocabulary

"enforce instance metadata service v2 (IMDSv2) on spot nodes" → SECURITY ("enforce" + credential/metadata context)
"restrict spot node IAM role permissions" → SECURITY + AUTHORIZATION
"encrypt spot instance root volumes at rest" → SECURITY ("encrypt", "at rest")

---

## B14-compiler-038-004 — SpotInstances: disambiguation — "interrupt" is not a predicate trigger

"handle spot interruption" → zero predicates ("interrupt", "handle" are operational gap verbs)
"respond to spot interruption notice" → zero predicates ("respond", "notice" are gap)
Contrast: "design the spot interruption handler" → ARCHITECTURE (design verb present)

---

## B14-compiler-038-005 — SpotInstances: DOCUMENT compilation

"document the spot instance strategy ADR" → DOCUMENT ("document" + "ADR")
"write the spot interruption runbook" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]
CapabilityGapWarnings: gap-devops-cloud-001 ("spot", "on-demand", "preemptible" are gap platform vocabulary)
