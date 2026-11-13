Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
$fixturePath = Join-Path $root "row23-runner-contract-fixture.json"
$expectedPath = Join-Path $root "row23-runner-contract-expected.json"
$resultJsonPath = Join-Path $root "row23-runner-contract-smoke-result.json"
$resultMdPath = Join-Path $root "row23-runner-contract-smoke-result.md"

$fixture = Get-Content -LiteralPath $fixturePath -Raw | ConvertFrom-Json
$expected = Get-Content -LiteralPath $expectedPath -Raw | ConvertFrom-Json

$failures = [System.Collections.Generic.List[string]]::new()

function Assert-Equal {
  param(
    [string] $Name,
    $Actual,
    $Expected
  )

  $actualJson = ConvertTo-Json $Actual -Depth 20 -Compress
  $expectedJson = ConvertTo-Json $Expected -Depth 20 -Compress
  if ($actualJson -ne $expectedJson) {
    $script:failures.Add("$Name expected $expectedJson but got $actualJson")
  }
}

Assert-Equal "scenario_id" $fixture.scenario_id $expected.scenario_id
Assert-Equal "row_id" $fixture.row_id $expected.row_id
Assert-Equal "fixture_scope" $fixture.fixture_scope $expected.fixture_scope
Assert-Equal "fixture_is_eval_generation" $fixture.fixture_is_eval_generation $false
Assert-Equal "fixture_is_heldout_dataset" $fixture.fixture_is_heldout_dataset $false
Assert-Equal "fixture_is_eval_dataset" $fixture.fixture_is_eval_dataset $false
Assert-Equal "supports_semantic_quality_claims" $fixture.supports_semantic_quality_claims $false
Assert-Equal "actual_approved_eval_executed" $fixture.actual_approved_eval_executed $false
Assert-Equal "synthetic_fixture_only" $fixture.synthetic_fixture_only $true
Assert-Equal "no_send" $fixture.no_send $true

foreach ($literal in $expected.boundary_literals) {
  if ($fixture.boundary_literals -notcontains $literal) {
    $failures.Add("fixture missing boundary literal: $literal")
  }
}

$status = if ($failures.Count -eq 0) { "passed" } else { "failed" }
$result = [ordered]@{
  schema = "xiigen.extension_readiness.row23.runner_contract_smoke_result.v1"
  created_at_local = (Get-Date).ToString("yyyy-MM-ddTHH:mm:sszzz")
  claim_scope = "Row23 runner-contract/readback smoke evidence only; not semantic quality evaluation."
  evidence_scope = "Local fixed fixture and expected-field readback inside this fresh Row23 folder only."
  count_claimed = [ordered]@{
    semantic_quality_runs = 0
    heldout_passes = 0
    product_ready_extensions = 0
    formal_joined_passes = 0
    runner_contract_smoke_runs = 1
  }
  independent_evidence_count = 0
  subagent_id_or_NONE = "Row23 implementation worker sub-agent"
  downgrade_needed = $true
  scenario_id = $expected.scenario_id
  row_id = $expected.row_id
  runner_contract_status = $status
  fixture_scope = $expected.fixture_scope
  semantic_quality_status = $expected.semantic_quality_status
  heldout_dataset_or_fixture_status = $expected.heldout_dataset_or_fixture_status
  same_scenario_status = $expected.same_scenario_status
  mutation_flags = $expected.mutation_flags
  external_ai_hidden_usage = $expected.external_ai_hidden_usage
  training_eval_dpo_checkpoint_locator_flags = $expected.training_eval_dpo_checkpoint_locator_flags
  host_ui_status = $expected.host_ui_status
  product_readiness_claim = $expected.product_readiness_claim
  formal_joined_pass = $expected.formal_joined_pass
  failures = @($failures)
  boundary_literals = @($expected.boundary_literals)
  non_claims = @(
    "I did not run semantic-quality evaluation.",
    "I did not create or use a heldout/eval dataset.",
    "I did not generate eval data.",
    "I did not call hidden/external AI, adapter/core AI, product code, host UI, training, DPO, checkpoint, locator, network, or git."
  )
}

$result | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $resultJsonPath -Encoding utf8

$md = @"
# Row23 Runner Contract Smoke Result

claim_scope: $($result.claim_scope)
evidence_scope: $($result.evidence_scope)
count_claimed: semantic_quality_runs=0; heldout_passes=0; product_ready_extensions=0; formal_joined_passes=0; runner_contract_smoke_runs=1
independent_evidence_count: 0
subagent_id_or_NONE: Row23 implementation worker sub-agent
downgrade_needed: true
scenario_id: $($result.scenario_id)
row_id: $($result.row_id)
runner_contract_status: $($result.runner_contract_status)
fixture_scope: $($result.fixture_scope)
semantic_quality_status: $($result.semantic_quality_status)
heldout_dataset_or_fixture_status: $($result.heldout_dataset_or_fixture_status)
same_scenario_status: $($result.same_scenario_status)
external_ai_hidden_usage: false
host_ui_status: $($result.host_ui_status)
product_readiness_claim: false
formal_joined_pass: false

Boundary literals:
- boundary/API evidence != host readiness
- marker-only != full local desktop pass
- trace != savings
- schema != training
- checkpoint candidate != promotion

Non-claims:
- I did not run semantic-quality evaluation.
- I did not create or use a heldout/eval dataset.
- I did not generate eval data.
- I did not call hidden/external AI, adapter/core AI, product code, host UI, training, DPO, checkpoint, locator, network, or git.
"@

Set-Content -LiteralPath $resultMdPath -Value $md -Encoding utf8

if ($failures.Count -ne 0) {
  Write-Error "Row23 runner-contract smoke failed."
}

Write-Output "Row23 runner-contract smoke passed."
