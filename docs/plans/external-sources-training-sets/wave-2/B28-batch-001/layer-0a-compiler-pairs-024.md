# Layer 0-A — Compiler Pairs (024) — Complete Predicate Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-024-001 — Complete QA trigger reference (release domain)

QA fires on (release domain):
  "QA" abbreviation in any position: QA sign-off, QA review, QA gate, QA checklist, QA criteria, QA approve, QA the [noun]
  Role compounds: QA lead, QA engineer, QA analyst, QA owner
  Gap forms in release: "go-live", "cutover", "rollout", "runbook", "deployment", "provisioning", "canary", "blue-green", "hotfix" (alone)
  "service" → IMPLEMENT (always, including in release review sentences)
  "criteria" → DOCUMENT (always, including "release criteria")
  "deployment" alone → gap; "build" → IMPLEMENT; "build provenance" → IMPLEMENT
  CompiledPredicateIds: reference

---

## B28-compiler-024-002 — "service" → IMPLEMENT in release context

A: "Review the deployment service and QA approve before release."
  "deployment service" → IMPLEMENT fires on "service" | "QA approve" → QA | "deployment" modifier → gap
  Teaching: "service" always fires IMPLEMENT even when preceded by "deployment" or "release."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-024-003 — "criteria" → DOCUMENT in release context

A: "Define the release criteria and QA sign-off on them."
  "release criteria" → DOCUMENT fires on "criteria" | "QA sign-off" → QA | "define" alone → DOCUMENT
  Teaching: "criteria" always fires DOCUMENT even when preceded by "release" or "acceptance."
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-024-004 — "build provenance" → IMPLEMENT

A: "Record build provenance and QA the artifact integrity before deployment."
  "build provenance" → IMPLEMENT (build artifact vocabulary) | "QA" → QA | "deployment" → gap
  Teaching: "build provenance" fires IMPLEMENT (build artifact term). Not gap.
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-024-005 — Pure gap vocabulary summary

A: "Plan the deployment rollout and set up the go-live checklist."
  "plan" alone → gap | "deployment rollout" → gap | "go-live checklist" → gap (pure gap vocabulary)
  Teaching: "go-live", "checklist" alone, "rollout", "cutover", "runbook", "deployment" alone → all gap.
  CompiledPredicateIds: [] → gap (knowledge_edge, not GapHandler/SCHEDULE_CAPABILITY_LEARNING)
