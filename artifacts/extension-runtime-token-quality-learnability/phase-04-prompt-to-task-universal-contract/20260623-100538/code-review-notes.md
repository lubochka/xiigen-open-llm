# Code review notes

## Scope

- `43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs`
- `Tests/PromptToTaskPhase04UniversalContractTests.cs`
- `tools/PromptToTaskPhase04Runner/Program.cs`

## Cycles

- Cycle 1: authority extraction. Finding: Found that quoted/attached/old/state authority needed explicit quarantine before signal extraction. Fix/result: Added evidence-only marker stripping and current_unquoted_user_instruction handling. Verdict: PASS
- Cycle 2: hidden deterministic shortcut audit. Finding: Raw prompt aliases are deterministic baseline signals. Fix/result: Kept the mechanism labelled BASELINE_NOT_SEMANTIC_QUALITY and semanticQualityClaimed=false. Verdict: PASS
- Cycle 3: source leakage audit. Finding: Provenance context keys could become accidental signal inputs. Fix/result: Skipped source_row_id, source_family, heldout family, pair/prompt/scenario ids, split role, and provenance keys. Verdict: PASS
- Cycle 4: evidence and lifecycle audit. Finding: Evidence needed to say common leg loaded but user adaptive training was not proven. Fix/result: Added lifecycle, leakage, negative authority, dashboard QA, and selected next work-order artifacts. Verdict: PASS

Hidden deterministic shortcut audit: PASS. The baseline signal aliases and nearest overlap are labelled internal/baseline, with semantic quality false.
Core boundary: PASS. PromptToTask logic stays in `43.SmartSkillsDomain`; host adapters were not edited.
Dashboard impact: PASS. Existing dashboard source-disjoint guard is parse-only evidence, not full UI/runtime readiness.
