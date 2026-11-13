# Codex Pass79 - Visual Judge Provider Boundary

## claim_scope

Pass79 hardened and tested the DesignConversion visual judge provider boundary
for provider-required behavior, redaction, test-provider labelling, tier/cost
stops, and Pass78-like render artifact inputs.

This is not proof that Claude, OpenAI, Gemini, or any other external visual AI
executed. This is not product readiness, training readiness, upload readiness,
or promotion readiness.

## evidence_scope

- Read Pass64 visual QA tier boundary evidence.
- Read Pass78 browser render QA evidence and its three-viewport PNG manifest
  shape.
- Read current visual judge, visual QA boundary policy, browser render manifest,
  internal service, contracts, and focused tests.
- Checked provider configuration presence by environment variable name only,
  without printing secret values.
- Ran focused local unit tests with in-memory fake/test providers only.
- Ran optional focused browser render QA tests against the existing local browser
  path.
- Ran no external AI provider or external network API call.

## provider_inventory

Provider variables checked for presence only:

```text
DESIGN_CONVERSION_VISUAL_JUDGE_ENDPOINT=false
DESIGN_CONVERSION_VISUAL_JUDGE_PROVIDER=false
DESIGN_CONVERSION_VISUAL_JUDGE_MODEL=false
DESIGN_CONVERSION_VISUAL_JUDGE_API_KEY=false
OPENAI_API_KEY=false
ANTHROPIC_API_KEY=false
GEMINI_API_KEY=false
GOOGLE_API_KEY=false
```

Provider status: `external_provider_not_configured`.

Required boundary: `provider_required`, `visual_ai_executed_false`.

## repair_performed

- Added literal evidence markers: `visual_judge_provider_boundary`,
  `visual_ai_executed_false`, `redaction`, `cost_cap`, and
  `test_provider_only`.
- `NoConfiguredVisualJudge` keeps `provider_required`,
  `external_provider_not_configured`, and `visual_ai_executed_false` with
  `ExternalCallExecuted=false`.
- `ExternalHttpDesignConversionVisualJudge` now labels in-memory test-provider
  routes as `TEST_VISUAL_JUDGE_VERDICT_RECEIVED`, carries
  `test_provider_only`, and keeps `ExternalCallExecuted=false` for those tests.
- Real external HTTP provider results can still report
  `external_ai_visual_judge_executed:true` only when explicit provider
  configuration is present and the provider is not labelled test-only.
- Provider payload construction now includes `redaction` policy metadata and
  redacts raw/sensitive metadata keys such as `html`, `css`, `tree_payload`,
  `user_id`, secret/token/password/API-key fields, private fields, and path
  fields before any provider payload is built.
- The request path through `DesignConversionInternalService` is tested to build
  visual judge requests from expected screenshots, rendered image artifacts,
  diff artifact refs, and artifact hashes rather than raw HTML/CSS/tree content.
- Tier policy now records `quality_threshold`, `budget_usd`, and `cost_cap`, and
  emits the `cost_cap` marker when `budget_cap_reached` stops the loop.

## verification

Focused provider-boundary test command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionVisualJudgeTests|FullyQualifiedName~DesignConversionVisualQaBoundaryPolicyTests|FullyQualifiedName~DesignConversionInternalServiceTests" --no-restore --logger "console;verbosity=minimal"
```

Authoritative post-marker result after this evidence artifact was added:
passed, `Failed: 0, Passed: 17, Skipped: 0, Total: 17`.

Optional browser compatibility command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests" --no-restore --logger "console;verbosity=minimal"
```

Result before this evidence artifact was added: passed, `Failed: 0, Passed: 6,
Skipped: 0, Total: 6`. This optional browser filter was not rerun after the
marker-only provider patch because browser provider code was not changed by
Pass79.

Marker coverage in code/tests:

```text
visual_judge_provider_boundary
external_provider_not_configured
provider_required
visual_ai_executed_false
redaction
tier_policy
cost_cap
test_provider_only
product_not_ready
remaining_queue
```

## render_artifact_linkage

Pass78 produced real local browser PNG artifacts for mobile, tablet, and desktop
viewports. Pass79 did not rerun the full Pass78 artifact inventory manually, but
it added a focused Pass78-like policy test using the same viewport shape:

```text
mobile 360x740
tablet 768x1024
desktop 1440x900
```

The simulated route is:

```text
expected image refs/hashes + rendered browser artifact refs/hashes
-> visual judge request
-> provider-required boundary when no provider is configured
-> tier policy stop reason provider_required
-> visual_ai_executed_false
-> product_not_ready
```

Deterministic PNG/pixel/render evidence remains local render evidence only. It
is not visual AI authority and not a trainable visual judge verdict.

## mergeability_check

Commands run before this evidence artifact was added:

```powershell
git fetch --prune
git diff --check
git merge-tree --write-tree origin/DesignToCode-clean HEAD
git merge-tree --write-tree origin/main HEAD
```

Observed:

```text
git fetch --prune: exit 0, with background auto-pack warning about unreachable loose objects
git diff --check: exit 0, empty output
git merge-tree --write-tree origin/DesignToCode-clean HEAD: exit 0, tree 6c418a2b45fc04e59fd34a6790310604f2ddd5d3
git merge-tree --write-tree origin/main HEAD: exit 0, tree e04c4c2b25a4851f89aa1efe6af16f8451172b16
remote evidence artifact path exists in origin/main: false
remote evidence artifact path exists in origin/DesignToCode-clean: false
post-artifact git diff --check: exit 0, empty output
post-artifact marker readback: required literals found in code/tests/evidence
post-artifact mojibake marker scan: no matches
```

Downgrade: `git merge-tree --write-tree ... HEAD` proves the committed branch
heads are mergeable for that command. It does not include the unstaged Pass79
working-tree snapshot. `git diff --check` covers the unstaged snapshot hygiene.
I did not claim a temp-index all-dirty merge-tree proof.

## mental_simulation

Provider missing:

1. Render artifacts exist from browser QA or Pass78-like manifest rows.
2. Expected screenshots/design renders exist.
3. No visual judge provider endpoint/model/key is configured.
4. The visual judge returns `provider_required` with
   `external_provider_not_configured`.
5. `visual_ai_executed_false` is recorded and no provider verdict is faked.
6. Tier policy stops as `provider_required`, preserving `tier_policy`,
   `quality_threshold`, `budget_usd`, and `cost_cap` metadata.

Test provider:

1. Unit tests use an in-memory HTTP handler only.
2. Options label the route `test_provider_only`.
3. The response parser carries score, defects, tokens, and estimated cost.
4. `ExternalCallExecuted=false`, `visual_ai_executed_false`, and
   `test_provider_only` prevent the fake route from being reported as real
   external visual AI.

Cost cap:

1. Complete render evidence exists.
2. Provider result is test-only and below quality threshold.
3. Estimated cost reaches the configured tier budget.
4. Tier policy returns `budget_cap_reached` with `cost_cap`.

## remaining_queue

- Configure an approved external visual AI provider with safe test credentials
  before any real provider call.
- Run the real external provider path once configuration exists, with secret
  redaction and cost accounting preserved.
- Capture/attach the real provider response packet without raw HTML/CSS/tree or
  local private paths.
- Re-run mergeability with a temp-index all-dirty method only if that command
  set is explicitly authorized for the current dirty snapshot.
- Product remains `product_not_ready` until real external visual AI proof,
  approved provider terms, production artifact storage, and end-to-end
  DesignConversion gates are complete.
