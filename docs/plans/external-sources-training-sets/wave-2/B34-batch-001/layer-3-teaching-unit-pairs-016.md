# Layer 3 — Teaching Unit Pairs (016)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-016 — "Workshop" facilitation teaching unit

**Core rule**: "workshop" as event/facilitation noun → always gap. The topic of a workshop never fires its associated predicate.

**Workshop topic ≠ predicate**:
| Workshop topic | Would-be predicate | Actual result | Why |
|---------------|-------------------|---------------|-----|
| "workshop on unit testing" | TEST | gap | facilitation ≠ writing tests |
| "workshop on architecture" | ARCHITECTURE | gap | facilitation ≠ designing architecture |
| "workshop on API design" | API_CONTRACT | gap | facilitation ≠ integrating |
| "workshop on security" | SECURITY | gap | facilitation ≠ hardening |
| "workshop on clean code" | (none) | gap | facilitation → gap always |

**Workshop nouns: content vs software**:
| Noun type | Example | Verdict |
|-----------|---------|---------|
| Curriculum noun | "design workshop curriculum" | gap |
| Platform/system noun | "build workshop management platform" | IMPLEMENT |
| Documentation noun | "write workshop agenda" | DOCUMENT |

**Gap ID**: gap-skills-guidance-train-001 (workshop = training/facilitation event).
