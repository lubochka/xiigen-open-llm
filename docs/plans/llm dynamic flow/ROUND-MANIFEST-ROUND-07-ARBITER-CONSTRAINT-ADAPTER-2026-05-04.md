# Round 07 Manifest - Arbiter Constraint Adapter Generalization

Date: 2026-05-04

Code repository: `C:\Projects\llm_mvp_core`

Artifact zip:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\round-zips\round-07-arbiter-constraint-adapter-corrected-2026-05-04.zip`

## Purpose

Close the remaining named technical debt from the projects 21-23 transfer: move `ExternalAiResultAdapter` from one hardcoded duplicate-account check to named arbiter constraint rules.

## Changed Files

1. `C:\Projects\llm_mvp_core\22.AgentAlgorithms\Pipeline\InternalFirstAiSupport.cs`
   - Replaced the direct `ContradictsLockedDuplicateRule` call with `FindArbiterRejection`.
   - Added a named arbiter rule table keyed by constraint name.
   - Preserved `identity_consistency` behavior for repeated provider callbacks that would create duplicate accounts.
   - Added `token_safety` behavior for unsafe token exposure through local storage, query strings, URL parameters, or non-httpOnly cookies.
   - Unknown constraint names do not reject by default.
   - Added plain-language comments across the production logic so the file satisfies the project DNA rule before the round is considered complete.

2. `C:\Projects\llm_mvp_core\Tests\AgentAlgorithmsTests.cs`
   - Added `ExternalAiResultAdapter_IdentityConsistencyConstraint_RejectsDuplicateAccount`.
   - Added `ExternalAiResultAdapter_TokenSafetyConstraint_RejectsUnsafeTokenStorage`.
   - Added `ExternalAiResultAdapter_UnknownConstraint_DoesNotReject`.

3. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\PROJECTS-21-23-TESTING-GUIDANCE-v1.md`
   - Added the post-Round 07 implementation note for the named arbiter rule table.
   - Added the token-safety and unknown-constraint adapter tests to the guidance.

4. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md`
   - Added the Round 07 validation note for the adapter return path.

5. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\WHAT-TO-DO-WHAT-NOT-TO-DO-v2.md`
   - Replaced the old hardcoded-adapter guidance with the named-rule-table guidance.
   - Added the rule that passing tests are not enough when project DNA comment requirements are unmet.

6. `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\WHAT-TO-DO-WHAT-NOT-TO-DO-v1.md`
   - Updated the historical guidance copy so it no longer claims the adapter is currently hardcoded.

## Included Reference File

`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`

This is included so a future session can repeat the same round packaging pattern.

## Verification

Fresh baseline before edits:

```powershell
dotnet test
```

Result: `982/982 passed`

Focused adapter tests:

```powershell
dotnet test --filter "ExternalAiResultAdapter"
```

Result: `4/4 passed`

Full suite after edits:

```powershell
dotnet test
```

Result: `985/985 passed`

Corrective verification after the comment and documentation fix:

```powershell
dotnet test --filter "ExternalAiResultAdapter"
dotnet test
```

Result: `4/4 passed`, then `985/985 passed`

## Design Meaning

External AI results are still advisory. They are adapted into local learning data only after named arbiter constraints are checked. The adapter now has a small extension point for additional arbiter rules while preserving the authority rule: an arbiter rejection wins over an external AI answer.

## Remaining Notes

The rule table is intentionally small. Future arbiter constraints can be added as named entries with focused tests, without changing the external AI approval package shape.

The first Round 07 zip was not final because the production file did not yet satisfy the plain-language comment discipline. The recreated zip supersedes it.
