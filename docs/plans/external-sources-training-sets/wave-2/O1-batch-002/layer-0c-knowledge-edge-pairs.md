# Layer 0-C — Knowledge Edge Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-kedge-001 — "client portal is down" → knowledge_edge (no lexicon match)

A: "the client portal is down"

B: Result: knowledge_edge
   Reasoning: "client portal" is colloquial infrastructure complaint. "client" alone is not in any predicate lexicon. "portal" is not in any predicate lexicon. "down" is not in any predicate lexicon. Zero predicates → knowledge_edge. This is the canonical no-signal boundary case for O1b.

---

## O1b-kedge-002 — "oauth is not working" → SECURITY fires (NOT knowledge_edge — critical teaching point)

A: "oauth is not working"

B: Result: VALID ROUTING (not knowledge_edge)
   Predicates: ["SECURITY"] | TransitionType: feature_addition
   Reasoning: CRITICAL TEACHING POINT — "oauth" appears explicitly in the SECURITY predicate lexicon. Even though "is not working" is colloquial, the technical term "oauth" fires SECURITY. This prompt IS routable to sub-phase 3.4. Do NOT classify as knowledge_edge when a lexicon keyword is present anywhere in the prompt.
