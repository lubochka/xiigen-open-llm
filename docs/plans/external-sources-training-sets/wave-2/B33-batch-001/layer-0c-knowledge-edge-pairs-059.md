# Layer 0-C — Knowledge Edge Pairs (059)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-059-001 — "Need" is not a code verb

Rule: "need" is a requirement expression, not a code verb.
"We need an AI-powered knowledge graph connector" → [] → gap-ai-ml-model-001 (requirement statement; no code verb).
"We need to build an AI-powered knowledge graph connector" → IMPLEMENT ("build" fires, not "need").
"Need" never fires any predicate.

---

## KE-059-002 — Requirement statements always produce gap

Requirement statement forms (all produce gap when no code verb follows):
"We need X", "The system requires X", "X is required", "X should be implemented" (passive, no actor), "X needs to be done".
These are all requirement statements → gap.
Contrast: "Implement X", "Build X" → predicates fire.

---

## KE-059-003 — Multi-agent + training loop: gap-ai-ml-model-001 dominates

Rule: "Multi-agent" (round-3) + "training loop" (round-2) combined without code verb:
→ gap-ai-ml-model-001 (round-3 / model term dominates).
"Multi-agent systems with shared training loops" → [] → gap-ai-ml-model-001.

---

## KE-059-004 — ARCHITECTURE+IMPLEMENT: design then build multi-agent system

Pattern: "Design the [multi-agent] [architecture] and implement the [shared memory store]" → [ARCHITECTURE, IMPLEMENT].
"multi-agent system architecture" = structural design object (ARCHITECTURE fires).
"shared memory store" = software artifact (IMPLEMENT fires).
Canonical ordering: ARCHITECTURE before IMPLEMENT.

---

## KE-059-005 — DOCUMENT: fires on AI gap documentation requests

Rule: "document the complete AI/ML vocabulary gap boundary" → DOCUMENT.
"AI/ML vocabulary gap boundary" is the object of "document" → DOCUMENT fires.
Even though the subject matter is about gap detection, the verb "document" fires DOCUMENT.
Not a gap request: it's a documentation task about gaps.
