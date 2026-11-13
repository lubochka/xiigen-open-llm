# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-015-001 — Edge: "chain of thought" vs "implement chain-of-thought"

"chain of thought reasoning for the agent" → gap (AI vocabulary; no code verb)
"implement a chain-of-thought reasoning module" → IMPLEMENT (code verb; "module" = software artifact)
"use chain-of-thought prompting" → gap ("use" in AI context; no code predicate fires)

---

## KE-015-002 — Edge: "chain of thought" does not suppress ARCHITECTURE

"design a chain-of-thought reasoning architecture" → ARCHITECTURE ("design" + "architecture" = structural noun → ARCHITECTURE fires; "chain-of-thought" is AI context)
"architect the chain-of-thought pipeline" → ARCHITECTURE ("architect" = design verb + "pipeline" = structural noun)
Teaching: AI vocabulary in design requests does not suppress ARCHITECTURE. Design verb + structural noun → ARCHITECTURE regardless of AI object.

---

## KE-015-003 — Edge: "chain of thought" + "prompt injection" special case

"enable chain-of-thought reasoning while preventing prompt injection in user inputs" → ["SECURITY"]
Teaching: "prompt injection" fires SECURITY. "chain-of-thought" + "enable" = gap (skipped). Mixed gap+predicate: only SECURITY fires. Gap is silently skipped when any predicate fires.

---

## KE-015-004 — Edge: "CoT" abbreviation = "chain-of-thought"

"Enable CoT (chain-of-thought) reasoning in the response pipeline." → gap (abbreviation + expansion confirmed; same gap rule applies)
"Implement a CoT prompt formatter." → IMPLEMENT (code verb fires; "CoT formatter" = software artifact)
Teaching: Abbreviations follow the same rules as their expanded forms. CoT (chain-of-thought) → gap vocabulary.

---

## KE-015-005 — Edge: "chain of thought" as method vs "chain-of-thought service" as noun

"chain of thought is the reasoning method" → gap (observation; AI vocabulary)
"build the chain-of-thought reasoning service" → IMPLEMENT (code verb + "service" = software artifact)
Teaching: Noun phrase "chain-of-thought service" used as a software artifact → IMPLEMENT fires on "build". Method description without code verb → gap.
