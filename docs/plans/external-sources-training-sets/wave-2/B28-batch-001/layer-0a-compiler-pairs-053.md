# Layer 0-A — Compiler Pairs (053) — DNSCutover
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-053-001 — "DNS cutover" is gap

A: "Execute the DNS cutover to point traffic to the new cluster."
  "execute DNS cutover" → gap | "point traffic" → gap | "new cluster" → gap
  Teaching: "DNS (Domain Name System) cutover" is deployment execution vocabulary → gap.
  CompiledPredicateIds: []

---

## B28-compiler-053-002 — IMPLEMENT+QA for DNS cutover service

A: "Build the DNS cutover automation service and QA the routing change."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Build+service → IMPLEMENT. QA fires on abbreviation. DNS context is gap filler.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-053-003 — "TTL" (Time To Live) and "DNS record" are gap

A: "Update TTL values and modify DNS records before the cutover."
  "update TTL values" → gap | "modify DNS records" → gap | "cutover" → gap
  Teaching: DNS configuration vocabulary is gap.
  CompiledPredicateIds: []

---

## B28-compiler-053-004 — TEST+QA for DNS cutover verification

A: "Verify the DNS resolution after cutover and QA the traffic routing."
  "verify" → TEST | "DNS resolution after cutover" → gap | "QA" → QA
  Teaching: "verify" fires TEST. "DNS resolution" is context. QA last.
  CompiledPredicateIds: ["TEST", "QA"]

---

## B28-compiler-053-005 — DOCUMENT+QA for DNS cutover runbook criteria

A: "Document the DNS cutover runbook criteria and QA the rollback steps."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "runbook" → gap | "QA" → QA
  Teaching: DOCUMENT via verb and noun. "runbook" is gap filler. QA last.
  CompiledPredicateIds: ["DOCUMENT", "QA"]
