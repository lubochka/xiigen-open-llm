# Layer 0-C — Knowledge Edge Pairs (055)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-055-001 — FalcoRuntime: gap vocabulary — Falco alert operations

Confirmed gap vocabulary for Falco runtime alert operations (zero predicates):
  Action verbs: alert (receive), monitor, watch, tail (log tail), respond (incident), mute, silence, acknowledge, escalate, page, suppress (alert), triage
  Alert nouns: alert rule output, syscall event, kernel event, falco alert JSON, alert webhook response

---

## B14-edge-055-002 — FalcoRuntime: IMPLEMENT boundary — Falco rule files are IaC artifacts

Falco rule files and DaemonSet configurations are IaC artifacts:
  "implement the Falco DaemonSet manifest" → IMPLEMENT
  "write the Falco rule file for syscall monitoring" → IMPLEMENT ("write" + "rule file")
  "scaffold the Falco Helm chart values module" → IMPLEMENT
  "tail Falco alerts" → gap (operational log tailing)
  Rule: Falco rule files are "rule file" IaC artifacts per the extended IMPLEMENT artifact list (file 060).

---

## B14-edge-055-003 — FalcoRuntime: SECURITY boundary — Falco enforcement compound phrases

SECURITY fires for:
  "enforce Falco runtime security rules" → SECURITY ("enforce" + security context)
  "harden runtime container security with kernel-level monitoring" → SECURITY ("harden")
  "configure TLS for Falco output webhook" → SECURITY ("TLS")
  "mute Falco alert" → gap | "silence alert rule" → gap

---

## B14-edge-055-004 — FalcoRuntime: disambiguation — "syscall" monitoring design vs runtime

"design the syscall monitoring policy" → ARCHITECTURE ("design" + "policy" in design context)
"implement the syscall policy rule file" → IMPLEMENT ("implement" + "rule file")
"watch syscall events in real time" → gap ("watch" is operational gap)
"enforce syscall policy enforcement" → SECURITY ("enforce" + "policy enforcement" = security compound phrase)
