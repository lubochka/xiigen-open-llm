# Held-Out and Corruption Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-012-A — Held-out: "autonomous agent" full five-predicate

A: "Design the autonomous agent architecture, build the decision engine, integrate with the workflow API, secure the agent inputs, and QA the release."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "QA"]
Orchestration: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → QAGatingFlow73
TransitionType: feature_addition | QA sub-phase: 7.1 (release gate)
Phrasing-B: Five-predicate pipeline — "autonomous agent" in description does not reduce predicates. All five verbs fire.

---

## HC-012-B — Held-out: "agent skill" gap vs IMPLEMENT distinction

A (gap): "Give the agent new coding skills." → Predicates: [] → GapHandler → gap-ai-agent-skill-001
A (IMPLEMENT): "Create a coding skill plugin for the agent." → Predicates: ["IMPLEMENT"] → ImplementationFlow41
Distinction: "give skills" = capability-grant vocabulary (gap). "create a [noun] plugin" = code-action (IMPLEMENT).

---

## HC-012-C — Corruption v6-02: Full gap vocabulary incorrectly fires predicates

v6 corrupt list of incorrect mappings:
- "adapt" → IMPLEMENT ✗ (gap)
- "orchestrate" → ARCHITECTURE ✗ (gap)
- "fine-tune" → IMPLEMENT ✗ (gap)
- "train" (ML) → IMPLEMENT ✗ (gap)
- "evaluate" (AI quality) → TEST ✗ (gap)
All five are v6-02 corruptions (gap suppression). All correct outputs: Predicates: [] → GapHandler.

---

## HC-012-D — Held-out: "multi-agent" observation → gap

A: "The multi-agent system is not coordinating tasks efficiently."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: Observation statement — no action verb; AI coordination vocabulary → gap.
Phrasing-B: Gap — observation statements always yield zero predicates regardless of AI complexity.
