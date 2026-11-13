# Layer 2 — Capability Gap Extended Pairs (003)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-003 — DocumentationFlow nine-node: "Write developer onboarding guide"

Request: "Write the developer onboarding guide for new backend engineers."

### Flow: DocumentationFlow (DOCUMENT predicate)

**Node 1**: Parse. "write" + "guide" = DOCUMENT (write + documentation artifact).
**Node 2**: Predicate set = ["DOCUMENT"]. TransitionType: documentation.
**Node 3**: Scope: onboarding guide — sections: environment setup, codebase overview, coding standards, first PR checklist.
**Node 4**: Documentation structure: 4 sections; audience: new backend engineers; format: markdown.
**Node 5**: Arbiter: DocumentationArbiter
**Node 6**: Coverage: all onboarding touchpoints addressed; code standards section included; PR checklist included.
**Node 7**: DocumentationFlow GREEN — guide structure approved.
**Node 8**: Output: developer-onboarding-guide.md | 4 sections | audience: new backend engineers
**Node 9**: Verdict: DOCUMENT_APPROVED — onboarding guide ready for review. TransitionType: documentation.
