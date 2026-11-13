# Layer 0-D — Flow Factory Pairs (007)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-007-001 — Full seven-predicate factory map in vibe context

**Predicates:** `["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]`

**Source request:** "Quick vibe session: architect the auth layer, integrate the OAuth provider, build the login service, secure the tokens, write unit tests, document the flow, and QA the implementation."

**Gap warnings:** gap-vibe-coding-style-001

**Factory map:**

| Predicate | Factory | Notes |
|---|---|---|
| ARCHITECTURE | ArchitectureFlow31 | First — other flows may depend on it |
| API_CONTRACT | APIContractFlow | Parallel with ARCHITECTURE |
| IMPLEMENT | BackendServiceFlow41 | After ARCHITECTURE available |
| SECURITY | SecurityFlow | After IMPLEMENT available |
| TEST | TestFlow | After IMPLEMENT available |
| DOCUMENT | DocumentFlow | Parallel with TEST |
| QA | QAFlow | Last — after all others |

**GapMetadata:** {vibe-style: true} applied to all 7 factories

---

## B32-0D-007-002 — Seven-factory execution order

```
Phase 1 (parallel): ArchitectureFlow31, APIContractFlow
Phase 2 (after Phase 1): BackendServiceFlow41
Phase 3 (parallel, after Phase 2): SecurityFlow, TestFlow, DocumentFlow
Phase 4 (after Phase 3): QAFlow
```

Gap framing does not change this ordering. "Vibe session" framing → gap warning only.

---

## B32-0D-007-003 — Gap metadata propagation across all 7 factories

**All seven factories receive the same gap metadata:**

```
gapMetadata: {
  "gap-vibe-coding-style-001": {
    trigger: "vibe session",
    vibeStyleIndicator: true,
    informalModeFlag: true
  }
}
```

Each factory can choose to respond to this metadata (e.g., by generating lighter-weight artifacts) or ignore it and proceed with standard output.

---

## B32-0D-007-004 — Factory map completeness check

**All registered factories (7 total):**

| Factory | Predicate | Registered? |
|---|---|---|
| ArchitectureFlow31 | ARCHITECTURE | ✓ yes |
| APIContractFlow | API_CONTRACT | ✓ yes |
| BackendServiceFlow41 | IMPLEMENT | ✓ yes |
| SecurityFlow | SECURITY | ✓ yes |
| TestFlow | TEST | ✓ yes |
| DocumentFlow | DOCUMENT | ✓ yes |
| QAFlow | QA | ✓ yes |

There is no VibeCodingFlow. There is no ExploratoryFlow. These are not registered factories.

---
