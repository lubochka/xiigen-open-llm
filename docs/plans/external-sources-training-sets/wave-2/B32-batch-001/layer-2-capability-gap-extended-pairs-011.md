# Layer 2 — Capability Gap Extended Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-011-001 — Seven-predicate vibe session: all predicates fire; only "vibe session" is gap

**Request:** "Quick vibe session: architect the auth layer, integrate the OAuth provider, build the login service, secure the tokens, write unit tests, document the flow, and QA the implementation."

**Layer 0-A trace:**
```
Token: "quick vibe session" → gap-vibe-coding-style-001
Token: "architect" → ARCHITECTURE fires ✓
Token: "auth layer" → domain noun
Token: "integrate" → API_CONTRACT fires ✓
Token: "OAuth provider" → domain noun
Token: "build" → IMPLEMENT fires ✓
Token: "login service" → domain noun
Token: "secure" → SECURITY fires ✓
Token: "tokens" → domain noun (in security context)
Token: "write unit tests" → TEST fires ✓
Token: "document" → DOCUMENT fires ✓
Token: "flow" → domain noun (in document context)
Token: "QA" → QA fires ✓
Token: "implementation" → domain noun

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
GapWarnings: [gap-vibe-coding-style-001]  ← only one gap warning
```

**Layer 2 trace:**
```
CapabilityGapType: style_qualifier_present
GapWarningType: vibe_style_framing
GapCount: 1 (only "vibe session" is gap; all actions are registered predicates)
PredicateCount: 7 (all registered predicates fire)
KeyInsight: maximal predicate coverage; minimal gap impact
```

**Final result:** [ARCHITECTURE, API_CONTRACT, IMPLEMENT, SECURITY, TEST, DOCUMENT, QA] → all 7 flows

---

## B32-L2-011-002 — Gap minimization in structured requests

**Pattern:** Even though this is a "vibe session", only one gap warning fires because the request contains rich, specific action vocabulary. The "vibe" label is the only unrecognized term.

**Rule:** Gap warning count is minimized when the request contains registered action verbs. The vibe framing only adds gaps; it doesn't change how many predicates fire.

| Request structure | Predicate count | Gap count |
|---|---|---|
| Only vibe vocabulary | 0 | high |
| Vibe label + one action verb | 1 | 1 |
| Vibe label + all 7 action verbs | 7 | 1 |
| Clean request, all 7 verbs | 7 | 0 |

---

## B32-L2-011-003 — Session completeness: all 7 predicates in vibe context

**When all 7 predicates fire in a vibe session:**
- All 7 flows are created
- All flows receive the vibe-style gap metadata
- Execution proceeds in standard ordered phases
- The "vibe session" label is captured in session metadata
- Session is marked: vibeStyleIndicator: true

**The session produces full output:**
- Architecture documents
- API contracts
- Service implementation code
- Security implementation
- Tests
- Documentation
- QA report

All outputs are fully produced regardless of the vibe framing. The vibe framing only adds a metadata note.

---
