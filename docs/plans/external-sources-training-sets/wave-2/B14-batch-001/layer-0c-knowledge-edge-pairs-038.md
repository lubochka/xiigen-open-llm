# Layer 0-C — Knowledge Edge Pairs (038)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-038-001 — SpotInstances: gap vocabulary — spot runtime operations

Confirmed gap vocabulary for spot instance operations (zero predicates):
  Action verbs: interrupt (handle interruption), preempt, reclaim, terminate (spot instance), request (spot), bid, refresh (spot pool)
  Spot-specific nouns: spot price, preemption notice, interruption notice, spot capacity pool, bid price
  None of the above fire any B14 predicate.

---

## B14-edge-038-002 — SpotInstances: SECURITY boundary — IMDSv2 enforcement

SECURITY fires for these spot-related security compound phrases:
  "enforce IMDSv2 (Instance Metadata Service v2)" → SECURITY (enforce + metadata service credential security)
  "restrict instance metadata access" → SECURITY + AUTHORIZATION (dual-predicate: security + access restriction)
  "encrypt spot root volumes at rest" → SECURITY
  "rotate spot instance credentials" → SECURITY (rotate + credentials)

---

## B14-edge-038-003 — SpotInstances: IMPLEMENT boundary — spot IaC artifacts

IMPLEMENT fires for these spot-related IaC artifact nouns:
  spot node group module, spot fleet configuration, mixed instance policy configuration, spot interruption handler script, launch template (IaC context), node pool configuration
  "configure spot pools" alone without IaC-artifact context → gap

---

## B14-edge-038-004 — SpotInstances: disambiguation — "interrupt" vs "design the interruption handler"

"handle spot interruption" → zero predicates (gap verb + gap noun)
"design the spot interruption handler" → ARCHITECTURE ("design" = ARCHITECTURE trigger; "handler" is a design artifact here)
"implement the spot interruption handler script" → IMPLEMENT ("implement" + "script")
Rule: The trigger verb determines the predicate; the noun "interruption handler" itself does not fire any predicate.
