# Layer 0-D — Flow Factory Pairs (033)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-033-001 — "describe to team" + IMPLEMENT factory (new vocabulary)

A: "Describe the feature flag architecture to the team and implement the feature flag service."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "describe to team" = gap warning (describe = explanation vocabulary); "implement feature flag service" = IMPLEMENT → ImplementationFlow41.

---

## FF-033-002 — "illustrate" + ARCHITECTURE factory (new vocabulary)

A: "Illustrate the domain boundaries on the whiteboard and architect the bounded context model."
Factory: ArchitectureFlow31 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "illustrate on whiteboard" = gap warning (illustrate = explanation vocabulary); "architect bounded context model" = ARCHITECTURE → ArchitectureFlow31.

---

## FF-033-003 — "hand over" + DOCUMENT factory (new vocabulary)

A: "Hand over the codebase to the new team and write the comprehensive handoff documentation."
Factory: DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "hand over codebase" = gap warning (hand over = knowledge transfer vocabulary); "write handoff documentation" = DOCUMENT → DocumentationFlow.

---

## FF-033-004 — "get a review from" + SECURITY factory (new vocabulary)

A: "Get a review from the security guild on the OAuth2 (OAuth 2.0) implementation and harden the token validation logic."
Factory: SecurityAuditFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "get a review from security guild" = gap warning; "harden token validation" = SECURITY → SecurityAuditFlow.

---

## FF-033-005 — "check in with" + API_CONTRACT factory (new vocabulary)

A: "Check in with the API (Application Programming Interface) governance team on the design and integrate the v3 contract endpoints."
Factory: APIContractFlow33 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "check in with governance team" = gap warning; "integrate v3 contract endpoints" = API_CONTRACT → APIContractFlow33.
