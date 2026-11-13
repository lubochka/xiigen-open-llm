# Row13.3c Core Selected-Skill Diagnosis

claim_scope: Row13.3c core selected-skill diagnosis only
evidence_scope: runtime request/response/core trace, host trace, service code, skill-selection contract/code, selected tests, common checkpoint manifest, nearby staged-candidate evidence
count_claimed: 0 tests/builds/UI reruns/git/model mutations
independent_evidence_count: 14 inspectable records
subagent_id_or_NONE: Row13.3c-bounded-sub-agent
downgrade_needed: no for diagnosis; yes for any readiness/test/build claim

## Verdict

`NEEDS_STAGED_CANDIDATE_INJECTION`

This is expected behavior under the current common-core priority rule, not proof of semantic readiness. The Row13.3c UI/core request did pass `RequestedSkills=["output-skepticism"]` and `AvailableSkills` containing `output-skepticism`, but `RequestedSkills` is not passed into `SkillSelectionMachine` as a hard constraint. The service uses it only as a fallback surface when the trained-state decision returns no selected skills.

Because `TargetLocator` was `null`, the UI path used the common/default asset state. The staged candidate that learned `output-skepticism` did not participate in this request.

## Answers

1. Runtime request/response/core trace:

- Method reaching core: `prompt-to-task`.
- `RequestedSkills`: `["output-skepticism"]`.
- `AvailableSkills`: `["output-skepticism","phase-completion-packager","verification-before-completion","plan-review"]`.
- `UserPromptOrTaskText`: `The user challenges whether a reported pass is real when the evidence only proves a boundary marker and does not prove the broader readiness claim.`
- `redactedInput`: present in runner/webview initial state, but not a top-level `XiigenExtensionRequest` field and not persisted in the core request JSON. The core request carried the redacted task meaning through `UserPromptOrTaskText`, plus host/checkset metadata.
- Core response selected: `["verification-before-completion"]`.
- Core trace says `core_asset_loader = XiigenExtensionApiService common asset root constructor`, `targetLocator = null`, and no training/external AI/host runtime claim.

2. `XiigenExtensionApiService` handling of `RequestedSkills`:

- At `XiigenExtensionApiService.cs:378-394`, `SkillSelectionRequest` is constructed from prompt, project root, `AvailableSkillIds`, host facts, and provenance. `RequestedSkills` is not included.
- At `XiigenExtensionApiService.cs:396-402`, if `DecideFromTrainedState` returns a selected skill, the service immediately returns that active decision even when `RequestedSkills` is present.
- At `XiigenExtensionApiService.cs:456-506`, `BuildRequestedSkillSurfaceFallbackDecision` intersects `RequestedSkills` with `AvailableSkills`, filters shortcut decoys, and marks it as fallback/no-semantic-authority/no-model-mutation evidence. That branch is only reached after no selected trained decision in the relevant paths.

Conclusion: `RequestedSkills` is a fallback/hint surface, not a hard constraint.

3. `SkillSelectionMachine` logic:

- `SkillSelectionContracts.cs:9-14` defines `SkillSelectionRequest` without `RequestedSkills`.
- `SkillSelectionMachine.cs:107-111` first calls `TryDecideWithSemanticUnitsOnly`.
- `SkillSelectionMachine.cs:173-177` prioritizes `TryDecideWithSkillSelection`.
- `SkillSelectionMachine.cs:954-978` returns the typed `SkillSelectionSkillUnit` decision when it has a decision.
- The runtime evidence reports that exact path: `SKILL_SELECTION_SKILL_UNIT_EXECUTED`, `SKILL_SELECTION_CHECKPOINT_SECTION`, confidence `0.85`, selected `verification-before-completion`.

So the machine did not "ignore" a field it received; it never received `RequestedSkills` in its contract.

4. Staged candidate participation:

- Runtime request has `TargetLocator: null`.
- Host trace has `targetLocator: null`.
- `XiigenExtensionApiService.cs:283-287` returns `_commonAssetLoadState` when `TargetLocator` is null.
- Core response check-set refs show common assets only: `common_asset_root = "."`, `skill_selection_checkpoint_ref = models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`, `skill_selection_checkpoint_sha256 = 737a5fad7c82851728a529dfc5890c22b4154cede7e639403a9e135d821e56ef`, `all_common_assets_loaded = true`.
- Nearby staged-candidate evidence exists and contains `output-skepticism`, for example `row13-1b-candidate-training-export-20260626T072146Z/staged-candidate/skill-selection-checkpoint.json`, and its scenario result says candidate selected `[output-skepticism]` where common selected `[verification-before-completion, plan-review]`.

Conclusion: the staged candidate did not participate in Row13.3c UI core request.

5. Exact next bounded work order:

Issue a staged-candidate injection/config work order. It should pass a `TargetLocator` or equivalent approved user-adaptive asset root into the Cursor UI/core request path and then rerun only the bounded Row13.3c core/UI scenario to prove the request loads the staged candidate and selects `output-skepticism`.

Do not train, promote, mutate checkpoints, or claim readiness in that work order. A separate core code fix is needed only if Luba decides that `RequestedSkills` must become a hard constraint over trained-state decisions. The evidence here supports staged candidate injection/config as the next necessary work, not DPO/model retraining and not a request-surface bug in the `RequestedSkills`/`AvailableSkills` pass-through.

## Evidence Records

- `row13-3c-selected-skill-trace-fix-20260626T091326Z/...positive_request-request-2026-06-26T09-14-31-666Z.json`: method, prompt, requested/available skills, `TargetLocator=null`.
- `row13-3c-selected-skill-trace-fix-20260626T091326Z/...positive_request-core-response-2026-06-26T09-14-31-666Z.json`: selected `verification-before-completion`, check-set refs, common checkpoint refs.
- `row13-3c-selected-skill-trace-fix-20260626T091326Z/...positive_request-core-trace-2026-06-26T09-14-31-666Z.json`: core endpoint and common asset loader evidence.
- `row13-3c-selected-skill-trace-fix-20260626T091326Z/...positive_request-host-trace-2026-06-26T09-14-31-666Z.json`: requested/available skills pass-through, no host selector/scorer, `targetLocator=null`.
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs:378-506`: requested skill fallback is conditional, not hard constraint.
- `43.SmartSkillsDomain/SkillSelectionContracts.cs:9-14`: `SkillSelectionRequest` lacks `RequestedSkills`.
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:107-111`, `173-177`, `954-978`: semantic unit decision priority.
- `models/skill-selection/skill-selection-v1/manifest.json`: common checkpoint sha and 65 profile common state.
- `row13-1b-candidate-training-export-20260626T072146Z/row13-1b-scenario-results.jsonl`: staged candidate selected `output-skepticism`; common selected `verification-before-completion, plan-review`.
