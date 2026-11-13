# Layer 0-A — Compiler Pairs (050) — RedTeamExercise
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C050-001 — Red team exercise findings review → SECURITY

A: "Review the red team exercise findings for authentication bypass and credential harvesting vectors."
  "review" → SECURITY | "authentication bypass" → SECURITY | "credential harvesting" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C050-002 — "run a red team exercise" → gap (event planning)

A: "Run a red team exercise to test the incident response team's detection capabilities."
  "run" → gap (exercise execution) | "red team exercise" → gap (event noun)
  CompiledPredicateIds: [] → knowledge_edge

---

## C050-003 — Red team attack path audit → SECURITY + AUTHORIZATION

A: "Audit the attack paths identified in the red team exercise: privilege escalation via JWT claim forgery and IDOR in billing endpoints."
  "audit" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION | "IDOR" → SECURITY+AUTHORIZATION | "JWT" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C050-004 — Red team + DOCUMENT → SECURITY+DOCUMENT

A: "Audit the red team findings for persistence mechanisms and document a risk remediation roadmap."
  "audit" → SECURITY | "persistence mechanisms" = attack-vector context → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation

---

## C050-005 — Lateral movement scan → SECURITY

A: "Scan the network segmentation for lateral movement paths identified in the red team exercise."
  "scan" → SECURITY | "lateral movement" → SECURITY (attack vector vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
