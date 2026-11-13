# Cross-Layer Synthesis (007)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-007 — Embedded API_CONTRACT: "Walk through integrating the SSO"

**Request**: "Walk through integrating the Okta SSO (Single Sign-On) provider with our developer portal authentication system."

### Layer 0-A (Compiler)
Tokens: walk through → guidance vocabulary (gap candidate); integrating → embedded API action verb; Okta SSO provider → external system (API contract target); developer portal → platform (integration target).
Embedded verb analysis: "integrating" = integrate verb → API_CONTRACT fires (integrate always fires in any frame).
Predicate set = ["API_CONTRACT"]
CapabilityGapWarning: gap-skills-guidance-mentor-001 (walk through = guidance vocabulary)

### Layer 0-B (Router)
Predicates = ["API_CONTRACT"] → APIContractFlow33
Gap warning appended; GapHandler NOT invoked.

### Layer 0-C (Knowledge Edge)
Integrate-always-fires rule: "integrate/integrating" fires API_CONTRACT in every context including guidance frames. "Walk through integrating" is not an exception. The sole exception is "teach to integrate" (infinitive learning target).
"Walk through" is guidance vocabulary → gap warning. "Integrating" → API_CONTRACT.

### Layer 0-D (Flow Factory)
Predicate ["API_CONTRACT"] → APIContractFlow33 selected.
TransitionType: documentation.
CapabilityGapWarning: gap-skills-guidance-mentor-001.

### Layer 1 (Transition Orchestrator)
APIContractFlow33 executes.
Gap warning appended.
SCHEDULE_CAPABILITY_LEARNING NOT emitted.

### Layer 2 (Flow Execution)
APIContractFlow33: Okta SSO integration with developer portal — OAuth2 (Open Authorization 2.0) flow, token exchange, session management.
Verdict: API_CONTRACT_APPROVED

### Layer 3 (Teaching)
Key rule: "integrate" always fires API_CONTRACT — even inside a walk-through guidance frame. "Walk through" becomes a gap warning, not a gap log. The integration action is structural and must be routed regardless of educational framing.
Compare: "walk through the API" (no integrate verb) → gap. "Walk through integrating the API" → API_CONTRACT fires.

**TransitionType**: documentation | **Gap warning**: gap-skills-guidance-mentor-001 | **SCHEDULE_CAPABILITY_LEARNING**: NOT emitted
