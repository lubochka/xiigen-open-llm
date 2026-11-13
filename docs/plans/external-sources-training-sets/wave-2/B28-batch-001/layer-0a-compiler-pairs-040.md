# Layer 0-A — Compiler Pairs (040) — ApprovalGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-040-001 — "approval" alone is gap

A: "Wait for approval before proceeding to production."
  "wait for approval" → gap | "proceeding to production" → gap
  Teaching: "approval" alone is gap vocabulary in the release domain. Does not fire QA or any predicate.
  CompiledPredicateIds: []

---

## B28-compiler-040-002 — "approve" vs "QA approve": only "QA approve" fires

A: "Get sign-off approval from the release manager." vs "Get QA approve from the release manager."
  "sign-off approval" → gap | "QA approve" → QA
  Teaching: "approve" without "QA" prefix is gap. "QA approve" fires QA via the abbreviation prefix.
  CompiledPredicateIds for first: [] | CompiledPredicateIds for second: ["QA"]

---

## B28-compiler-040-003 — IMPLEMENT+QA for approval gate service

A: "Build the approval gate service and QA the gating logic."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "QA" → QA
  Teaching: Build+service → IMPLEMENT. QA fires on abbreviation. "approval gate" context is gap filler only.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-040-004 — AUTHORIZATION+QA for approval gate access

A: "Authorize the release manager role for approval gate bypass and QA the permission rules."
  "authorize" → AUTHORIZATION | "QA" → QA
  Teaching: "authorize" fires AUTHORIZATION. QA last. "approval gate bypass" → gap.
  CompiledPredicateIds: ["AUTHORIZATION", "QA"]

---

## B28-compiler-040-005 — "manual approval" and "automated approval" both gap

A: "Set up manual approval gates and automated approval thresholds."
  "set up" → gap | "manual approval" → gap | "automated approval" → gap | "thresholds" → gap
  Teaching: All approval configuration vocabulary is gap. No predicates fire.
  CompiledPredicateIds: []
