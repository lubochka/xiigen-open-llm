# Layer 0-C — Knowledge Edge Pairs (048)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-048-001 — ARCHITECTURE+SECURITY: "harden" always fires SECURITY

"harden" verb: fires SECURITY in ANY infrastructure context without requiring an additional security noun.
  "harden the Kubernetes cluster" → SECURITY | "harden the ingress" → SECURITY | "harden the API server" → SECURITY
  Contrast: "tighten" does NOT fire SECURITY ("tighten" is not in B14 SECURITY vocabulary)
  "secure" (adjective): "secure communication" → SECURITY (via communication + TLS context); "secure" alone as adjective does not fire

---

## B14-edge-048-002 — ARCHITECTURE+SECURITY: "enforce" is context-dependent

"enforce" verb fires SECURITY when followed by security vocabulary:
  "enforce TLS" → SECURITY | "enforce zero-trust" → SECURITY | "enforce mTLS" → SECURITY | "enforce encryption" → SECURITY
"enforce" verb fires AUTHORIZATION when followed by access-policy vocabulary:
  "enforce permission boundary" → AUTHORIZATION (permission boundary is AUTHORIZATION vocabulary) AND SECURITY (compound phrase)
  → Dual-predicate: "enforce permission boundary enforcement" fires both SECURITY and AUTHORIZATION
"enforce" without following vocabulary → ambiguous → resolved by the noun that follows

---

## B14-edge-048-003 — ARCHITECTURE+SECURITY: negation rule

Negated action verbs do NOT fire their predicate:
  "without implementing IaC" → IMPLEMENT does not fire
  "no TLS enforcement" → SECURITY does not fire (negation suppresses)
  "avoid encryption at rest" → SECURITY does not fire
  BUT: SECURITY compound phrases in positive assertions always fire: "zero-trust policy" in a positive description fires SECURITY.

---

## B14-edge-048-004 — ARCHITECTURE+SECURITY: "secure" vs "harden" vs "enforce"

"secure" (verb): rarely fires SECURITY directly in B14; requires security noun following it
  "secure the API" → context-dependent; "secure the API with TLS" → SECURITY fires from "TLS"
"harden" (verb): always fires SECURITY directly, no noun required
"enforce" (verb): fires SECURITY with security noun; fires AUTHORIZATION with access noun
Rule: "harden" is the most reliable standalone SECURITY trigger verb in B14.
