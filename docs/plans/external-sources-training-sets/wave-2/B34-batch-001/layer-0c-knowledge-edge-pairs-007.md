# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-007-001 — "integrate" + LMS → API_CONTRACT always

"Integrate the LMS with Slack" → API_CONTRACT
"Integrate the training platform with the SSO provider" → API_CONTRACT
"Integrate the onboarding tool with GitHub" → API_CONTRACT

"integrate" always fires API_CONTRACT regardless of the training/HR context of the objects.

---

## KE-007-002 — ARCHITECTURE in guidance context: structural noun required

"Design the learning platform architecture" → ARCHITECTURE (design + architecture/platform = structural noun)
"Architect the training system" → ARCHITECTURE (architect + system = structural noun)
"Design the training curriculum" → gap (curriculum = content noun, not structural artifact)
"Design a learning path" → gap (learning path = curriculum noun)

Rule: In B34 context, ARCHITECTURE fires on structural/software artifact nouns, not on content/curriculum nouns.

---

## KE-007-003 — "validate" in skills/guidance context

"Validate the developer's knowledge" → gap (validate knowledge = assessment vocabulary)
"Validate the input data in the training module" → SECURITY (validate + input data → security context)
"Write validation tests for the module" → TEST (write + test = TEST)

Rule: "validate" in HR/skill-assessment context → gap. "validate" in security/data context → SECURITY. "write validation tests" → TEST.

---

## KE-007-004 — DomainId is always software-development for B34 gaps

No domain exists for: education, training, HR, learning-management, mentoring
All B34 gap requests log DomainId: software-development
GapHandler canonical response: "Capability not available. Request logged for learning pipeline."
Event: SCHEDULE_CAPABILITY_LEARNING → AutonomousLearningPipeline (subproject 16)
