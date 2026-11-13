# Layer 0-A — Compiler Pairs (055)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-055-001 — FalcoRuntime: ARCHITECTURE fires on Falco design vocabulary

"architect the runtime threat detection topology for the Kubernetes cluster" → ARCHITECTURE ("architect", "topology")
"design the Falco rule architecture and alert pipeline" → ARCHITECTURE ("design", "architecture", "pipeline" in infra context)
"structure the runtime security monitoring model" → ARCHITECTURE ("structure", "model")
"check Falco alerts" → gap ("check" is operational gap)

---

## B14-compiler-055-002 — FalcoRuntime: IMPLEMENT fires on Falco IaC artifacts

"implement the Falco DaemonSet manifest and rule configuration" → IMPLEMENT ("implement" + "manifest" + "configuration")
"write the Falco Helm chart values module" → IMPLEMENT ("write" + "module")
"scaffold the Falco rule file for syscall monitoring" → IMPLEMENT ("scaffold" + "rule file" = IaC artifact)

---

## B14-compiler-055-003 — FalcoRuntime: SECURITY fires on Falco security vocabulary

"enforce Falco runtime security rules across all node pools" → SECURITY ("enforce" in security context)
"harden runtime container security with kernel-level syscall monitoring" → SECURITY ("harden")
"configure TLS for Falco output webhook" → SECURITY ("TLS")

---

## B14-compiler-055-004 — FalcoRuntime: DOCUMENT compilation

"document the Falco rule policy ADR and threat model" → DOCUMENT ("document" + "ADR")
"write the Falco runtime security runbook" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]

---

## B14-compiler-055-005 — FalcoRuntime: gap vocabulary — runtime event operations

Confirmed gap verbs for Falco runtime operations (fire zero predicates):
  alert (receive alert), monitor (active monitoring), watch, tail (log tail), respond (incident response), mute, silence, acknowledge, escalate, page
  "tail Falco alerts" → gap | "respond to Falco alert" → gap | "mute the alert rule" → gap
