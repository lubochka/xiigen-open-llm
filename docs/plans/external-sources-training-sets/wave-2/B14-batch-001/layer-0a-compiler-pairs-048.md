# Layer 0-A — Compiler Pairs (048)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-048-001 — ARCHITECTURE+SECURITY combo: compound predicate compilation

"Design the zero-trust network topology and enforce mTLS across all service communication paths."
  "design" → ARCHITECTURE | "topology" → ARCHITECTURE | "zero-trust" → SECURITY | "enforce" → SECURITY (enforcement verb) | "mTLS" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  CapabilityGapWarnings: none (no gap vocabulary in this request)

---

## B14-compiler-048-002 — ARCHITECTURE+SECURITY combo: additional compound examples

"Architect the secrets management layer and enforce certificate rotation policies."
  "architect" → ARCHITECTURE | "layer" → ARCHITECTURE (in design context) | "enforce" + "certificate rotation" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]

"Structure the network boundary model and encrypt all data in transit."
  "structure" → ARCHITECTURE | "boundary" + "model" → ARCHITECTURE | "encrypt" + "in transit" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]

---

## B14-compiler-048-003 — ARCHITECTURE+SECURITY combo: AUTHORIZATION fires alongside

"Design the IAM trust policy architecture and enforce permission boundaries."
  "design" → ARCHITECTURE | "trust policy" → AUTHORIZATION ("trust policy" in policy document context) | "enforce" + "permission boundaries" → SECURITY + AUTHORIZATION ("permission boundary enforcement" is a SECURITY compound phrase; "permission boundary" is also AUTHORIZATION)
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "AUTHORIZATION"]

---

## B14-compiler-048-004 — ARCHITECTURE+SECURITY combo: "harden" as SECURITY trigger

"Harden the Kubernetes cluster infrastructure and design the hardened topology."
  "harden" → SECURITY ("harden" is a B14 SECURITY predicate verb) | "design" → ARCHITECTURE | "topology" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Note: "harden" alone fires SECURITY even without a following encryption/key noun.

---

## B14-compiler-048-005 — ARCHITECTURE+SECURITY combo: disambiguation from IMPLEMENT

"Design and harden the service mesh topology without implementing IaC."
  "design" → ARCHITECTURE | "harden" → SECURITY | "topology" → ARCHITECTURE
  "implementing IaC" — the negative context (without) does NOT fire IMPLEMENT; negation suppresses the verb
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Rule: negated action verbs ("without implementing", "no IMPLEMENT required") do not fire the predicate.
