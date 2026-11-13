# Layer 0-C — Knowledge Edge Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review, sub-phase 6.3
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B26B1-WDA-edge-001 — "compliance" alone does not fire SECURITY

```
source_id: xiigen-b26-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Ensure the system is compliance-ready for the upcoming audit."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-security-compliance-readiness-001"
    gap_vocabulary: ["compliance-ready", "upcoming audit"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "ensure" — not in any predicate lexicon (ensure ≠ IMPLEMENT ≠ SECURITY)
    "compliance-ready" — "compliance" is gap; SECURITY fires on: audit, security,
      vulnerability, authentication, authorization, encryption, sanitization, injection.
      "compliance" alone does not fire SECURITY.
    "upcoming audit" — "audit" fires SECURITY when paired with security-action verbs.
      "upcoming audit" is a temporal noun phrase — audit as a future event, not an action.
    Result: zero predicates → knowledge_edge

  Boundary: "compliance" is gap vocabulary. "audit" fires SECURITY only as a verb
  (audit the system) not as a future-event noun (the upcoming audit).

---

## Pair B26B1-WDA-edge-002 — "pentest" is gap vocabulary

```
source_id: xiigen-b26-edge-002
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Run a pentest on the authentication service before the release."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-security-pentest-001"
    gap_vocabulary: ["pentest", "before the release"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "pentest" — not in SECURITY predicate lexicon. Penetration testing vocabulary
      (pentest, pen test, penetration test) is gap. SECURITY fires on: audit, vulnerability,
      authentication, authorization, encryption, injection, sanitization, access control.
    "run" — not in any predicate lexicon (run ≠ IMPLEMENT ≠ QA)
    "authentication service" — SECURITY fires on "authentication"? Yes, but the action
      verb "run a pentest" carries no predicate, so the contextual fire doesn't trigger.
    Correction: "authentication" fires SECURITY independently of the action verb.
    CompiledPredicateIds: ["SECURITY"] via "authentication service".

  Correction applied:
    CompiledPredicateIds: ["SECURITY"]
    SourceStatus: "ready (weak signal)"
    CapabilityGapWarnings:
      - "gap-security-pentest-001: 'pentest', 'run a pentest' are gap vocabulary"

  Teaching: Even with a gap action verb ("run a pentest"), "authentication" fires SECURITY
  as an independent security-domain noun. The predicate fires on security-domain vocabulary
  regardless of whether the action verb has a predicate.

---

## Pair B26B1-WDA-edge-003 — "GDPR" is gap vocabulary

```
source_id: xiigen-b26-edge-003
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Ensure GDPR compliance in the user data processing pipeline."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-security-regulatory-001"
    gap_vocabulary: ["GDPR", "GDPR compliance", "data processing pipeline"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "GDPR" (General Data Protection Regulation) — not in any predicate lexicon
    "ensure" — no predicate
    "compliance" — gap (as established in edge-001)
    "data processing pipeline" — "data processing" not in any predicate; "pipeline" alone insufficient
    Result: zero predicates → knowledge_edge

  Regulatory compliance vocabulary (GDPR, HIPAA, SOC2, PCI-DSS as acronyms) is gap.
  SECURITY fires on technical security actions, not on regulatory compliance labels.
