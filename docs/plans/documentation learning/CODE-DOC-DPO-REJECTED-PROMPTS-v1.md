# Code Documentation DPO Rejected Prompts
# Version: 1.0 - 2026-05-11

---

## Purpose

This document gives prompts for creating useful rejected sides for code
documentation DPO pairs.

Rejected sides must be plausible and instructive, not silly.

Good rejected sides are not "bad documentation". They are strong, readable,
almost-correct answers with one evidence-checkable defect. The model must learn
why that answer is unsafe, not merely that short or vague text is worse.

---

## Prompt 1 - Method-Level Rejected Side

```text
Create one rejected documentation answer for this C# method.

The rejected answer must be plausible to a junior developer but wrong in exactly
one primary way.

Choose one rejected type:
- generic_name_echo
- missing_side_effect
- dropped_error_path
- invented_business_purpose
- wrong_async_claim
- wrong_framework_lifecycle

The rejected answer must include:
- a natural documentation paragraph
- [WRONG: weakness - what cannot be learned - functional consequence.]

Do not make the rejected side identical to the chosen side.
Do not use a one-word or joke answer.
```

---

## Prompt 2 - Caller/Callee Rejected Side

```text
Create one rejected documentation answer for a caller method that has supplied
callee summaries.

The rejected answer must make exactly one caller/callee reasoning mistake:
- attributes callee behavior to the caller
- ignores an accepted callee summary
- uses a stale callee summary
- drops failure propagation
- claims a call path that is not present

End with a specific [WRONG: ...] annotation that names the consequence for
maintenance or refactoring.
```

---

## Prompt 3 - Framework Lifecycle Rejected Side

```text
Create one rejected documentation answer that confuses the legacy framework
lifecycle.

Allowed mistakes:
- WebForms Page_Load treated as MVC action
- HttpModule treated as controller middleware
- MVC action treated as background job
- WPF event handler treated as web request handler
- ASP.NET Core controller behavior projected onto ASP.NET Framework MVC

The rejected answer must look realistic and must include the specific
[WRONG: ...] annotation.
```

---

## Examples Of Bad Rejected Sides

Bad:

```text
This method does the opposite.
[WRONG: bad]
```

Why bad:
It teaches nothing about code documentation.

Bad:

```text
This method validates the user.
```

Why bad:
No explicit `[WRONG: ...]` consequence.

Bad:

```text
The method validates input, saves the entity, and returns the result.
[WRONG: copied chosen with minor wording.]
```

Why bad:
Too close to chosen and no meaningful defect.

---

## Runner Findings - 2026-05-15

The code documentation DPO runner found that negative examples fail when the
rejected side is either too weak or not trained through the answer tokens.

Observed failure modes:

- `incomplete_trace` as a short partial list: the rejected answer was obviously
  incomplete, so it did not teach a realistic maintenance mistake.
- `invented_callee` with a generic service name: the fake dependency was too
  generic and did not force source-evidence checking.
- different chosen/rejected labels in the tokenized answer prefix: the model
  could learn the label shape instead of the content defect.
- prompt tokens trained as answer tokens: long prompts diluted the actual
  chosen/rejected contrast.

Required runner behavior:

- use the same answer prefix for chosen and rejected;
- mask prompt tokens and train only answer tokens;
- persist `dpo-pair-diagnostics.jsonl` for every run;
- group diagnostics by rejected type before adding new rows;
- never publish a unit when held-out DPO accuracy is below `0.75`.

---

## Good Rejected Patterns For Code Trace DPO

### Pattern A - Incomplete Trace With One Wrong Value-Flow Fact

The rejected side should keep methods and call edges, then make one precise
value-flow mistake.

```text
methods: CampaignController.SaveCampaign, CampaignController.NormalizeCampaign,
CampaignController.PersistCampaign, CampaignRepository.StoreCampaign
call_edges: CampaignController.SaveCampaign->CampaignController.NormalizeCampaign,
CampaignController.SaveCampaign->CampaignController.PersistCampaign,
CampaignController.PersistCampaign->CampaignRepository.StoreCampaign
value_flows: CampaignController.SaveCampaign->CampaignController.NormalizeCampaign(value)->assigned,
CampaignController.SaveCampaign->CampaignController.PersistCampaign(id|normalized)->returned,
CampaignController.PersistCampaign->CampaignRepository.StoreCampaign(id|normalized)->side_effect
[WRONG: incomplete_trace keeps the method and call graph but marks the final repository transit as side_effect instead of returned, so the wiki cannot explain what Save returns to the caller.]
```

Why good:
It is realistic, complete-looking, and wrong in one concrete way.

### Pattern B - Invented Callee With Source-Evidence Absence

The rejected side may add a plausible business dependency, but it must explain
that the dependency is absent from the source trace.

```text
methods: CampaignController.SaveCampaign, CampaignController.NormalizeCampaign,
CampaignController.PersistCampaign, CampaignRepository.StoreCampaign,
CampaignPolicyService.ValidateCampaign
call_edges: CampaignController.SaveCampaign->CampaignController.NormalizeCampaign,
CampaignController.SaveCampaign->CampaignController.PersistCampaign,
CampaignController.PersistCampaign->CampaignRepository.StoreCampaign,
CampaignController.SaveCampaign->CampaignPolicyService.ValidateCampaign
value_flows: CampaignController.SaveCampaign->CampaignController.NormalizeCampaign(value)->assigned,
CampaignController.SaveCampaign->CampaignController.PersistCampaign(id|normalized)->returned,
CampaignController.PersistCampaign->CampaignRepository.StoreCampaign(id|normalized)->returned,
CampaignController.SaveCampaign->CampaignPolicyService.ValidateCampaign(id|normalized)->assigned
[WRONG: invented_callee is a plausible policy-validation dependency, but no such method exists in the source trace; teaching it would make the wiki route field-impact work through a fake service.]
```

Why good:
The rejected answer is not silly. It tests whether the model respects source
evidence instead of accepting a plausible architectural story.

---

## Negative DTO Generator Findings - 2026-05-15

The first real DPO metric run for `NegativeDpoGenerator` exposed a confusing
rejected-shape problem:

- `missing_caller_context` scored `0.00` train accuracy and `0.00` held-out
  accuracy in the first run.
- `missing_callee_transit` also regressed when the rejected side used the same
  "callee" vocabulary as the correct side without a clear evidence contract.
- The bad pattern was not that the rejected text was too weak. It was that the
  rejected text was plausible in the wrong way and did not give the model a
  stable contrast between "complete documentation" and "one missing contract".

Fixed pattern:

- Do not put the rejected type in the synthetic symbol name, source path, or
  meaning text.
- Use realistic code-unit names such as `AccessPolicy.CanEditEntity` and
  `OrderWorkflow.ResolveOrder`, not `missing_caller_context.Unit0`.
- Put evidence-checkable contracts in both chosen and rejected:
  `location_contract`, `parameter_contract`, `return_contract`,
  `caller_context_contract`, `callee_transit_contract`.
- For caller/callee negatives, add a direct status field:
  `caller_path_status` or `dependency_path_status`.
- The rejected side must be almost complete and differ by one contract:
  `caller_path_missing`, `dependency_path_omitted`, `actual_parameters_missing`,
  `actual_return_missing`, or `source_path_and_line_missing`.
- For caller-context negatives, a pure omission is too weak and unstable.
  The best rejected side is an adjacent wrong-caller path: replace the actual
  controller caller with a plausible but unsupported background caller.

Important correction from the 2026-05-19 runner audit:

The run below is now an invalidated historical measurement. It was produced
before `ProjectWikiDpoMetricRunner` copied the initial policy weights into the
reference model. Because policy and reference were independently initialized,
the reward margin contained random-reference noise and must not be used for
promotion, "done", or quality claims. Keep it only as evidence that the old
runner could produce a false-positive pass.

Invalidated historical run:

```text
unit: negative-dpo-generator-unit
summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-negative-dpo-generator-batch-009-wrong-caller\dpo-runs\negative-dpo-generator-unit\negative-dpo-generator-batch-001-negative-dpo-generator-unit-20260519064526\project-wiki-dpo-metric-summary.json
diagnostics: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-negative-dpo-generator-batch-009-wrong-caller\dpo-runs\negative-dpo-generator-unit\negative-dpo-generator-batch-001-negative-dpo-generator-unit-20260519064526\dpo-pair-diagnostics.jsonl
train rows: 50
held-out rows: 50
final loss: 0.6789622843265534
final accuracy: 1.0
final reward margin: 0.028625794388353824
held-out loss: 0.6772080063819885
held-out accuracy: 1.0
held-out reward margin: 0.03218836709856987
average CPU percent: 12.016054396935374
total CPU time ms: 157859.375
peak working set MB: 84.67578125
elapsed ms: 225392.7104
```

Rejected-type diagnostics from that invalidated run:

```text
heldout missing_callee_transit: accuracy 1.0, margin 0.035470
heldout missing_caller_context: accuracy 1.0, margin 0.030099
heldout missing_location: accuracy 1.0, margin 0.043901
heldout missing_parameters: accuracy 1.0, margin 0.017675
heldout missing_return_contract: accuracy 1.0, margin 0.033796
```

Corrected runner status after copying policy weights into the frozen reference:

```text
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-dpo-runner-correction\PHASE-RESOURCE-REPORT.json
status: DATA_NEEDED
allowed claim: NegativeDpoGenerator is not promoted and not done.
best corrected held-out accuracy observed so far: 0.58
best corrected held-out reward margin observed so far: 0.0003848438209388405
runner correction: policy/reference now start from identical weights.
next action: continue rejected-shape remediation and trainer validation; do not reuse invalidated batch-009 metrics.
```

Superseded status from the early 2026-05-19 negative-DPO remediation phase:

```text
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-negative-dpo-remediation\PHASE-RESOURCE-REPORT.json
status: DATA_NEEDED
allowed claim: NegativeDpoGenerator is not promoted and not done.
current latest mixed batch: negative-dpo-generator-batch-034-mixed-restored-best-shape
latest mixed held-out accuracy: 0.47999998927116394
latest mixed held-out reward margin: 0.00017975230002775788
latest mixed final loss: 0.693071266412735
latest mixed average CPU percent: 12.555699621324065
latest mixed total CPU time ms: 131843.75
latest mixed peak RAM MB: 101.3125
latest mixed elapsed ms: 178188.5585
best observed continuation held-out accuracy: 0.8999999761581421
best observed continuation held-out reward margin: 0.002504566218703985
confirmation result: not stable; later mixed and confirmation runs fell below 0.75.
```

Current accepted status from the 2026-05-19 negative-DPO remediation v2 phase:

```text
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-negative-dpo-remediation-v2\PHASE-RESOURCE-REPORT.json
status: PROMOTED for NegativeDpoGenerator only.
allowed claim: NegativeDpoGenerator published from explicit promotion-confirmation batch 051.
whole wiki claim: not done; DocumentationGenerator, WikiQueryPlanningFlow, WikiContextRoutingUnit, project-local answer quality, and question-bank training remain open.
promotion manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-negative-dpo-generator-sequence-047-rich-evidence-shape\negative-dpo-generator-unit\negative-dpo-generator-20260519153042\generated_capability_manifest.json
promotion held-out accuracy: 0.980000019
promotion held-out reward margin: 0.003840186
promotion final loss: 0.691329753
promotion average CPU percent: 12.734612
promotion total CPU time ms: 25296.875
promotion peak RAM MB: 81.199219
promotion elapsed ms: 62500.289
```

Current rejected-type diagnostics from the promotion-confirmation batch:

```text
missing_callee_transit: held-out accuracy 1.0, reward margin 0.005602293
missing_caller_context: held-out accuracy 1.0, reward margin 0.003661566
missing_location: held-out accuracy 0.9, reward margin 0.001554184
missing_parameters: held-out accuracy 1.0, reward margin 0.003616648
missing_return_contract: held-out accuracy 1.0, reward margin 0.004766245
```

The key training lesson: isolated focus repetition did not fix
`missing_parameters`. It stayed at 0.70 in batch048. The fix was to make the
parameter negative DPO shape more architectural:

- chosen side adds `parameter_verification_status: actual_input_values_present_in_code_trace`;
- `missing_parameters` appears across several realistic code roles, not only
  `RequestMapper.MapRequest`;
- rejected side remains plausible and near-correct but substitutes guessed
  input contracts that are missing from the code trace.

Promotion for future negative-DPO units remains blocked until all of these are
true:

```text
1. policy and reference initialization are persisted in the runner artifact.
2. continued runs load the policy checkpoint and the original reference checkpoint.
3. a mixed batch stays above the unit threshold.
4. a second mixed confirmation batch stays above the threshold.
5. per-rejected-type diagnostics no longer show a hidden weak type below threshold.
6. publication is attempted only by an explicit promotion-confirmation batch,
   not by ordinary mixed-confirmation or replay runs.
```

If a focused batch scores well but the next mixed batch falls below threshold,
that focused batch is useful remediation evidence, not a promotion candidate.

If an ordinary mixed-confirmation batch passes, it still must not publish. Only
an explicit promotion-confirmation batch may publish weights.

### Pattern C - Missing Caller Context With One Wrong Caller Path

Chosen:

```text
documentation_verdict: complete_unit_documentation.
AccessPolicy.CanEditEntity is located at Project/Services/access-policy/Unit.cs:10.
location_contract: source_path_and_line_present.
Meaning: Checks whether a user can edit an entity before the controller continues the save workflow.
parameter_contract: actual_parameters: userId:int, entityId:int.
return_contract: actual_return: bool allowed.
caller_context_contract: actual_callers: EntityController.Post, EntityController.Put.
caller_path_status: caller_path_complete.
callee_transit_contract: none recorded.
dependency_path_status: no_lower_level_dependency.
Evidence: Project:access-policy:Unit0.
```

Rejected:

```text
documentation_verdict: incomplete_unit_documentation.
AccessPolicy.CanEditEntity is located at Project/Services/access-policy/Unit.cs:10.
location_contract: source_path_and_line_present.
Meaning: Checks whether a user can edit an entity before the controller continues the save workflow.
parameter_contract: actual_parameters: userId:int, entityId:int.
return_contract: actual_return: bool allowed.
caller_context_contract: wrong_callers: NightlyPermissionSync.Run, BackgroundAccessJob.Execute.
caller_path_status: caller_path_wrong.
callee_transit_contract: none recorded.
[WRONG: missing_caller_context - this replaces the actual upstream controller workflow with an unsupported background workflow, so the wiki would route impact analysis through the wrong caller path.]
```

Why good:
It keeps the meaning, location, parameters, and return contract intact. The only
defect is a plausible but unsupported upstream caller path. This trained better
than a pure omission because the rejected answer remains realistic while still
being evidence-checkably wrong.

### Pattern D - Missing Callee Transit With One Dependency Omission

Chosen:

```text
documentation_verdict: complete_unit_documentation.
OrderWorkflow.ResolveOrder is located at Project/Services/order-workflow/Unit.cs:10.
location_contract: source_path_and_line_present.
Meaning: Loads the order aggregate and returns the resolved order summary for the request workflow.
parameter_contract: actual_parameters: orderId:int.
return_contract: actual_return: OrderSummary resolvedOrder.
caller_context_contract: none recorded.
callee_transit_contract: actual_callees: OrderRepository.LoadOrder, OrderPolicy.ApplyVisibility.
dependency_path_status: dependency_path_complete.
Evidence: Project:order-workflow:Unit0.
```

Rejected:

```text
documentation_verdict: incomplete_unit_documentation.
OrderWorkflow.ResolveOrder is located at Project/Services/order-workflow/Unit.cs:10.
location_contract: source_path_and_line_present.
Meaning: Loads the order aggregate and returns the resolved order summary for the request workflow.
parameter_contract: actual_parameters: orderId:int.
return_contract: actual_return: OrderSummary resolvedOrder.
caller_context_contract: none recorded.
callee_transit_contract: dependency_path_omitted.
called_dependencies: not documented.
[WRONG: missing_callee_transit - the source calls lower-level dependencies, but this answer hides them, so caller-level documentation cannot propagate dependency meaning.]
```

Why good:
It avoids a vague "document alone" statement. The rejected side is realistic but
fails exactly one dependency-transit contract.
