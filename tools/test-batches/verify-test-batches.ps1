param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
    [string]$BatchesFile = (Join-Path $PSScriptRoot 'batches.tsv'),
    [string]$RunRoot = '',
    [switch]$AllowIncomplete,
    [switch]$Focused,
    [switch]$SelfCheckEmptyResidual,
    [switch]$SelfCheckIndivisibleNamespaceCollision,
    [switch]$SelfCheckExactHumanBatchAssignmentMap,
    [switch]$SelfCheckPreparedFilterParsing
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$script:QwenCanaryOptInEnvironmentName = 'XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY'
$script:LegacyQwenCanaryFqn = 'DesignConversionLocalIndependentQwenRuntimeCanaryTests.LocalQwenJudge_FourCallsChooseSameQualityAndReverseDirectionOnSwap'
$script:LegacyQwenDynamicSkipPrefix = '$XunitDynamicSkip$WINDOWS_ONLY_RUNTIME:'
# Живой основной TSV-список сейчас содержит 31 именованную группу и одну остаточную.
$script:CanonicalBatchCount = 33
# Лестница групп тестов как живой файл и правило классификации (внутренний
# номер 249) хранится поверх физических строк запускателя: проверяющий скрипт
# следит, чтобы 16 плановых групп и 7 правил не стали простой документацией,
# оторванной от живого файла.
$script:PlannedLogicalBatchCount = 16
$script:LogicalRuleCount = 7
$script:LogicalLadderVersion = 'test-suite-batch-ladder-v1'
$script:LogicalLadderRuleRows = 0
$script:LogicalLadderLogicalBatchRows = 0
$script:LogicalLadderMappedPhysicalRows = 0
$script:LogicalLadderUnknownPhysicalRows = 0
$script:LogicalLadderDuplicatePhysicalRows = 0
$script:LogicalLadderUnmappedPhysicalRows = 0
# Последний старый runtime-класс получил собственный маршрут, поэтому активный
# перечень классов без точного маршрута должен быть пустым.
$script:CanonicalWeakSnapshotCount = 0
$script:LeafModeOverrideHeader = '# batch_id	leaf_mode	human_reason'
$script:RunInvocationHistoryHeader = "invocation_order`tinvocation_id`tstarted_utc`tverification_scope`tselected_batches`tselected_batch_count`trequested_leaf_mode`teffective_leaf_modes`tmax_leaves`tmax_elapsed_minutes`tplan_only`tno_build`tfocused_mode`tbatches_tsv_sha256`tstatus_at_append`tprevious_rows_sha256`trow_sha256"
$script:OrchestrationRoutingProductBoundariesBatchId = 'orchestration-routing-product-boundaries'
$script:OrchestrationRoutingProductBoundaryClassFqns = @(
    'LlmFromScratch.Tests.MultiSubPhaseRouterLearnedAuthorityTests',
    'LlmFromScratch.Tests.OrchestratorModelTests',
    'LlmFromScratch.Tests.PluginBoundaryTests',
    'LlmFromScratch.Tests.PromptRoutingInputCompilerUnitTests',
    'LlmFromScratch.Tests.ReflectionWiringTests',
    'LlmFromScratch.Tests.SolutionProductProjectRegistrationTests',
    'ThinClientApiContractTests'
)
$script:TestInfrastructureEvidenceMeasurementBatchId = 'test-infrastructure-evidence-measurement'
$script:TestInfrastructureEvidenceMeasurementClassFqns = @(
    'LlmFromScratch.Tests.CrossStackFlowSeedHonestyTests',
    'LlmFromScratch.Tests.MeasurementNoisePolicyTests',
    'LlmFromScratch.Tests.TestOwnedOutputRootTests',
    'LlmFromScratch.Tests.TestSuiteBatchInventoryGuardTests',
    'LlmFromScratch.Tests.WorkLeftRagEvidenceIndexTests'
)
# Этот класс выделен отдельно, потому что измеренный pack слишком большой:
# verifier должен проверить ровно один глобальный класс и один метод на leaf.
$script:DesignConversionLiveLocalServerMethodDiagnosticBatchId = 'design-conversion-live-local-server-method-diagnostic'
$script:DesignConversionLiveLocalServerMethodDiagnosticClassFqns = @(
    'DesignConversionLiveLocalServerTests'
)
# Эта группа остаётся общей storage-группой, но один глобальный тяжёлый класс
# внутри неё получает узкий method-pack; verifier проверяет именно этот класс,
# а остальные классы storage-группы должны оставаться цельными pack-units.
$script:DesignConversionDpoStorageBatchId = 'design-conversion-dpo-storage'
$script:DesignConversionDpoStorageMethodPackClassFqn = 'DesignConversionDpoStorageTests'
$script:DesignConversionDpoStorageSlowMethodFqn = 'DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite'
$script:ServerOwnedReplayManifestAttestationBatchId = 'server-owned-replay-manifest-attestation'
$script:ServerOwnedReplayManifestAttestationClassFqns = @(
    'LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests'
)
$script:Phase08CommonUserTwoLegLifecycleBatchId = 'phase08-common-user-two-leg-lifecycle'
$script:Phase08CommonUserTwoLegLifecycleClassFqns = @(
    'LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests'
)
# PowerShell-verifier зеркалит C# guard: singleton-маршрут принадлежит ровно
# одному классу с проверками общих объектов потоков/узлов.
$script:SingletonFlowNodeIdentityBatchId = 'singleton-flow-node-identity'
$script:SingletonFlowNodeIdentityClassFqns = @(
    'LlmFromScratch.Tests.SingletonIdentityTests'
)
$script:LogicalTopologyAndResultGapBatchId = 'logical-topology-and-result-gap'
$script:LogicalTopologyAndResultGapClassFqns = @(
    'LlmFromScratch.Tests.TopologyMutationFlowTests',
    'ResultGapLogicalHierarchyTests'
)
$script:ResultGapTrainingPairWriterBatchId = 'result-gap-training-pair-writer'
$script:ResultGapTrainingPairWriterClassFqns = @(
    'ResultGapPhase07DpoWriterTests'
)
$script:SingleCombinationGraphRunnerBatchId = 'single-combination-graph-runner'
$script:SingleCombinationGraphRunnerClassFqns = @(
    'SingleCombinationGraphCandidateRunnerTests'
)
$script:RetainedDomainModelRegistryBatchId = 'retained-domain-model-registry'
$script:RetainedDomainModelRegistryClassFqns = @(
    'RetainedDomainModelRegistryTests'
)
$script:PairGraphPriorityBatchIds = @(
    $script:ResultGapTrainingPairWriterBatchId,
    $script:SingleCombinationGraphRunnerBatchId
)
$script:PairGraphBroadCollisionBatchIds = @(
    'dynamic-graph',
    'teaching-translation-dpo',
    'orchestration-skills-wiki'
)
$script:ModelActivationBatchId = 'model-activation'
$script:ModelActivationClassFqns = @(
    'DesignConversionActivatedModelRepositoryHostRuntimeTests',
    'DesignConversionExternalVisualAiReceiptTrustTests',
    'DesignConversionHumanApprovalTests',
    'DesignConversionImprovementLoopSemanticsTests',
    'DesignConversionModelActivationRegistryTests',
    'DesignConversionModelActivationRouteRuntimeTests'
)

function Get-NormalizedPath {
    param([string]$Path)

    try {
        return [System.IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path)
    }
    catch {
        return [System.IO.Path]::GetFullPath($Path)
    }
}

function Test-IsDefaultBatchesFile {
    param([string]$Path)

    return [string]::Equals(
        (Get-NormalizedPath -Path $Path),
        (Get-NormalizedPath -Path (Join-Path $PSScriptRoot 'batches.tsv')),
        [StringComparison]::OrdinalIgnoreCase)
}

function Read-BatchRows {
    param([string]$Path)

    $rows = New-Object System.Collections.Generic.List[object]
    $lineNumber = 0

    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) {
            continue
        }

        # TSV остается намеренно плоским: человек может открыть файл глазами, а
        # verifier строго проверяет, что исполнитель не спрятал настройку в коде.
        $parts = $line -split "`t", 9
        if ($parts.Count -ne 9) {
            throw "batches.tsv line $lineNumber must have 9 columns; found $($parts.Count)."
        }

        $rows.Add([pscustomobject]@{
            Order = $parts[0]
            BatchId = $parts[1]
            HumanName = $parts[2]
            Dll = $parts[3].Replace([char]'/', [char]92)
            Filter = $parts[4]
            MinRows = [int]$parts[5]
            SplitOnRepeatTimeout = $parts[6]
            Platform = $parts[7]
            PlatformReason = $parts[8]
            LineNumber = $lineNumber
        })
    }

    return $rows
}

function Read-LogicalLadderMetadata {
    param([string]$Path)

    $rules = New-Object System.Collections.Generic.List[object]
    $logicalBatches = New-Object System.Collections.Generic.List[object]
    $values = @{}
    $problems = New-Object System.Collections.Generic.List[string]
    $lineNumber = 0

    foreach ($line in Get-Content -LiteralPath $Path -Encoding UTF8) {
        $lineNumber++
        $trimmed = $line.TrimEnd("`r")
        if (-not $trimmed.StartsWith('# ')) {
            continue
        }

        $parts = $trimmed -split "`t"
        switch ($parts[0]) {
            '# ladder_manifest_version' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_manifest_version' }
            '# ladder_physical_row_count' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_physical_row_count' }
            '# ladder_logical_batch_count' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_logical_batch_count' }
            '# ladder_rule_count' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_rule_count' }
            '# ladder_assignment_identity' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_assignment_identity' }
            '# ladder_first_match_rule' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'ladder_first_match_rule' }
            '# new_test_author_obligation' { Add-LogicalMetadataValue -Values $values -Problems $problems -Parts $parts -LineNumber $lineNumber -Key 'new_test_author_obligation' }
            '# logical_rule' {
                if ($parts.Count -ne 4) {
                    $problems.Add("logical_ladder_rule_schema_line:$lineNumber`:columns=$($parts.Count)")
                    continue
                }

                # Строки правил делают семь шагов проверяемыми из того же
                # живого файла, который читает запускатель; проверяющий скрипт
                # сверяет договор, а формулировки оставляет людям.
                $rules.Add([pscustomobject]@{
                    Order = $parts[1]
                    RuleId = $parts[2]
                    HumanRule = $parts[3]
                    LineNumber = $lineNumber
                })
            }
            '# logical_batch' {
                if ($parts.Count -ne 6) {
                    $problems.Add("logical_ladder_batch_schema_line:$lineNumber`:columns=$($parts.Count)")
                    continue
                }

                $physicalIds = @($parts[4] -split ',' | ForEach-Object { ([string]$_).Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
                $logicalBatches.Add([pscustomobject]@{
                    Order = $parts[1]
                    LogicalBatchId = $parts[2]
                    HumanName = $parts[3]
                    PhysicalBatchIds = $physicalIds
                    HumanStatus = $parts[5]
                    LineNumber = $lineNumber
                })
            }
        }
    }

    return [pscustomobject]@{
        Values = $values
        Rules = @($rules.ToArray())
        LogicalBatches = @($logicalBatches.ToArray())
        Problems = @($problems.ToArray())
    }
}

function Add-LogicalMetadataValue {
    param(
        [hashtable]$Values,
        [System.Collections.Generic.List[string]]$Problems,
        [string[]]$Parts,
        [int]$LineNumber,
        [string]$Key
    )

    if ($Parts.Count -ne 2) {
        $Problems.Add("logical_ladder_metadata_schema_line:$LineNumber`:key=$Key`:columns=$($Parts.Count)")
        return
    }

    if ([string]::IsNullOrWhiteSpace($Parts[1])) {
        $Problems.Add("logical_ladder_metadata_blank:$Key`:line=$LineNumber")
    }

    $Values[$Key] = $Parts[1]
}

function Test-LogicalMetadataIntValue {
    param(
        [hashtable]$Values,
        [string]$Key,
        [int]$Expected
    )

    if (-not $Values.ContainsKey($Key)) {
        Write-Host "logical_ladder_metadata_missing:$Key"
        return 1
    }

    $parsed = 0
    if (-not [int]::TryParse([string]$Values[$Key], [ref]$parsed) -or $parsed -ne $Expected) {
        Write-Host "logical_ladder_metadata_int_mismatch:$Key`:expected=$Expected`:actual=$($Values[$Key])"
        return 1
    }

    return 0
}

function Test-LogicalLadderMetadata {
    param(
        [string]$Path,
        [object[]]$Rows
    )

    $metadata = Read-LogicalLadderMetadata -Path $Path
    $script:LogicalLadderRuleRows = $metadata.Rules.Count
    $script:LogicalLadderLogicalBatchRows = $metadata.LogicalBatches.Count
    $script:LogicalLadderMappedPhysicalRows = 0
    $script:LogicalLadderUnknownPhysicalRows = 0
    $script:LogicalLadderDuplicatePhysicalRows = 0
    $script:LogicalLadderUnmappedPhysicalRows = 0
    $problems = 0
    foreach ($problem in $metadata.Problems) {
        Write-Host $problem
        $problems++
    }

    if (-not $metadata.Values.ContainsKey('ladder_manifest_version') -or
        $metadata.Values['ladder_manifest_version'] -ne $script:LogicalLadderVersion) {
        Write-Host "logical_ladder_version_mismatch:expected=$script:LogicalLadderVersion`:actual=$($metadata.Values['ladder_manifest_version'])"
        $problems++
    }

    $problems += Test-LogicalMetadataIntValue -Values $metadata.Values -Key 'ladder_physical_row_count' -Expected $Rows.Count
    $problems += Test-LogicalMetadataIntValue -Values $metadata.Values -Key 'ladder_logical_batch_count' -Expected $script:PlannedLogicalBatchCount
    $problems += Test-LogicalMetadataIntValue -Values $metadata.Values -Key 'ladder_rule_count' -Expected $script:LogicalRuleCount

    foreach ($key in @('ladder_assignment_identity', 'ladder_first_match_rule', 'new_test_author_obligation')) {
        if (-not $metadata.Values.ContainsKey($key) -or [string]::IsNullOrWhiteSpace([string]$metadata.Values[$key])) {
            Write-Host "logical_ladder_metadata_missing_or_blank:$key"
            $problems++
        }
    }

    if ($metadata.Rules.Count -ne $script:LogicalRuleCount) {
        Write-Host "logical_ladder_rule_count_mismatch:expected=$script:LogicalRuleCount`:actual=$($metadata.Rules.Count)"
        $problems++
    }

    $seenRules = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 0; $index -lt $metadata.Rules.Count; $index++) {
        $rule = $metadata.Rules[$index]
        $expectedOrder = '{0:00}' -f ($index + 1)
        if ($rule.Order -ne $expectedOrder) {
            Write-Host "logical_ladder_rule_order_mismatch:line=$($rule.LineNumber):expected=$expectedOrder`:actual=$($rule.Order)"
            $problems++
        }

        if ([string]::IsNullOrWhiteSpace([string]$rule.RuleId) -or -not $seenRules.Add([string]$rule.RuleId)) {
            Write-Host "logical_ladder_rule_id_invalid_or_duplicate:line=$($rule.LineNumber):actual=$($rule.RuleId)"
            $problems++
        }

        if ([string]::IsNullOrWhiteSpace([string]$rule.HumanRule)) {
            Write-Host "logical_ladder_rule_text_missing:line=$($rule.LineNumber):rule=$($rule.RuleId)"
            $problems++
        }
    }

    if ($metadata.LogicalBatches.Count -ne $script:PlannedLogicalBatchCount) {
        Write-Host "logical_ladder_batch_count_mismatch:expected=$script:PlannedLogicalBatchCount`:actual=$($metadata.LogicalBatches.Count)"
        $problems++
    }

    $knownPhysicalIds = @{}
    foreach ($row in $Rows) {
        $knownPhysicalIds[$row.BatchId] = $true
    }

    $mappedPhysicalIds = @{}
    $seenLogicalIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 0; $index -lt $metadata.LogicalBatches.Count; $index++) {
        $logicalBatch = $metadata.LogicalBatches[$index]
        $expectedOrder = '{0:00}' -f ($index + 1)
        if ($logicalBatch.Order -ne $expectedOrder) {
            Write-Host "logical_ladder_batch_order_mismatch:line=$($logicalBatch.LineNumber):expected=$expectedOrder`:actual=$($logicalBatch.Order)"
            $problems++
        }

        if ([string]::IsNullOrWhiteSpace([string]$logicalBatch.LogicalBatchId) -or -not $seenLogicalIds.Add([string]$logicalBatch.LogicalBatchId)) {
            Write-Host "logical_ladder_batch_id_invalid_or_duplicate:line=$($logicalBatch.LineNumber):actual=$($logicalBatch.LogicalBatchId)"
            $problems++
        }

        if ([string]::IsNullOrWhiteSpace([string]$logicalBatch.HumanName) -or
            [string]::IsNullOrWhiteSpace([string]$logicalBatch.HumanStatus)) {
            Write-Host "logical_ladder_batch_human_fields_missing:line=$($logicalBatch.LineNumber):logical=$($logicalBatch.LogicalBatchId)"
            $problems++
        }

        if (@($logicalBatch.PhysicalBatchIds).Count -eq 0) {
            Write-Host "logical_ladder_batch_has_no_physical_rows:line=$($logicalBatch.LineNumber):logical=$($logicalBatch.LogicalBatchId)"
            $problems++
        }

        foreach ($physicalBatchId in @($logicalBatch.PhysicalBatchIds)) {
            if (-not $knownPhysicalIds.ContainsKey($physicalBatchId)) {
                Write-Host "logical_ladder_unknown_physical_batch:$physicalBatchId`:logical=$($logicalBatch.LogicalBatchId):line=$($logicalBatch.LineNumber)"
                $script:LogicalLadderUnknownPhysicalRows++
                $problems++
                continue
            }

            if ($mappedPhysicalIds.ContainsKey($physicalBatchId)) {
                Write-Host "logical_ladder_duplicate_physical_batch:$physicalBatchId`:first=$($mappedPhysicalIds[$physicalBatchId]):second=$($logicalBatch.LogicalBatchId)"
                $script:LogicalLadderDuplicatePhysicalRows++
                $problems++
            }
            else {
                $mappedPhysicalIds[$physicalBatchId] = $logicalBatch.LogicalBatchId
                $script:LogicalLadderMappedPhysicalRows++
            }
        }
    }

    foreach ($row in $Rows) {
        if (-not $mappedPhysicalIds.ContainsKey($row.BatchId)) {
            Write-Host "logical_ladder_unmapped_physical_batch:$($row.BatchId)"
            $script:LogicalLadderUnmappedPhysicalRows++
            $problems++
        }
    }

    $desktopLogicalRows = @($metadata.LogicalBatches | Where-Object { @($_.PhysicalBatchIds) -contains 'api-dashboard-wpf-windows' })
    if ($desktopLogicalRows.Count -ne 1) {
        Write-Host "logical_ladder_windows_desktop_batch_missing_or_duplicate:count=$($desktopLogicalRows.Count)"
        $problems++
    }
    else {
        if ($desktopLogicalRows[0].Order -ne '16') {
            Write-Host "logical_ladder_windows_desktop_wrong_logical_order:expected=16:actual=$($desktopLogicalRows[0].Order)"
            $problems++
        }

        if (-not ([string]$desktopLogicalRows[0].HumanStatus).Contains('windows', [StringComparison]::OrdinalIgnoreCase)) {
            Write-Host 'logical_ladder_windows_desktop_status_missing_windows_reason'
            $problems++
        }
    }

    return $problems
}

function Read-LegacyWeakSnapshotRowsForVerifier {
    param([string]$Root)

    $path = Join-Path $Root 'Tests\TestSuiteBatchLegacySnapshot.txt'
    $rows = New-Object System.Collections.Generic.List[object]
    $problems = New-Object System.Collections.Generic.List[string]
    if (-not (Test-Path -LiteralPath $path)) {
        $problems.Add("legacy_weak_snapshot_missing:$path")
        return [pscustomobject]@{ Rows = @($rows.ToArray()); Problems = @($problems.ToArray()) }
    }

    $lines = @(Get-Content -LiteralPath $path)
    $declaredTotal = $null
    $insideBody = $false
    foreach ($line in $lines) {
        $trimmed = $line.TrimEnd("`r")
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) {
            continue
        }

        if ($trimmed.StartsWith("total_classes`t")) {
            $declaredTotal = [int]($trimmed.Substring("total_classes`t".Length))
            continue
        }

        if ($trimmed -eq "dll`tclass_fqn`tsource_path`tbatch_id") {
            $insideBody = $true
            continue
        }

        if (-not $insideBody) {
            continue
        }

        $parts = $trimmed -split "`t"
        if ($parts.Count -ne 4) {
            $problems.Add("legacy_weak_snapshot_schema_line:$trimmed")
            continue
        }

        $rows.Add([pscustomobject]@{
            Dll = $parts[0]
            ClassFqn = $parts[1]
            SourcePath = $parts[2]
            BatchId = $parts[3]
        })
    }

    if ($null -ne $declaredTotal -and $declaredTotal -ne $rows.Count) {
        $problems.Add("legacy_weak_snapshot_total_mismatch:header=$declaredTotal`:body=$($rows.Count)")
    }

    return [pscustomobject]@{ Rows = @($rows.ToArray()); Problems = @($problems.ToArray()) }
}

function Read-LeafModeOverrides {
    param(
        [string]$Path,
        [object[]]$Rows
    )

    $resultRows = New-Object System.Collections.Generic.List[object]
    $problems = New-Object System.Collections.Generic.List[string]
    if (-not (Test-Path -LiteralPath $Path)) {
        $problems.Add("leaf_mode_override_missing:$Path")
        return [pscustomobject]@{ Rows = @($resultRows.ToArray()); Problems = @($problems.ToArray()) }
    }

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -eq 0) {
        $problems.Add('leaf_mode_override_empty')
        return [pscustomobject]@{ Rows = @($resultRows.ToArray()); Problems = @($problems.ToArray()) }
    }

    if ($lines[0].TrimStart([char]0xFEFF) -ne $script:LeafModeOverrideHeader) {
        $problems.Add('leaf_mode_override_header_mismatch')
    }

    $known = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($row in $Rows) {
        [void]$known.Add($row.BatchId)
    }

    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 1; $index -lt $lines.Count; $index++) {
        $lineNumber = $index + 1
        $line = $lines[$index].TrimEnd("`r")
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith('#')) {
            continue
        }

        $parts = $line -split "`t"
        if ($parts.Count -ne 3) {
            $problems.Add("leaf_mode_override_schema_line:$lineNumber`:columns=$($parts.Count)")
            continue
        }

        $batchId = $parts[0]
        $leafMode = $parts[1]
        $humanReason = $parts[2]
        if ([string]::IsNullOrWhiteSpace($batchId) -or
            [string]::IsNullOrWhiteSpace($leafMode) -or
            [string]::IsNullOrWhiteSpace($humanReason)) {
            $problems.Add("leaf_mode_override_required_field_blank_line:$lineNumber")
        }

        if (-not $seen.Add($batchId)) {
            $problems.Add("leaf_mode_override_duplicate_batch_id:$batchId")
        }

        if (-not $known.Contains($batchId)) {
            $problems.Add("leaf_mode_override_unknown_batch:$batchId")
        }

        if ($leafMode -notin @('pack', 'method-diagnostic', 'class-method-pack', 'off')) {
            $problems.Add("leaf_mode_override_unknown_mode:$batchId`:$leafMode")
        }
        elseif ($leafMode -eq 'class-method-pack' -and $batchId -ne $script:DesignConversionDpoStorageBatchId) {
            $problems.Add("leaf_mode_override_class_method_pack_wrong_batch:$batchId")
        }

        $resultRows.Add([pscustomobject]@{
            BatchId = $batchId
            LeafMode = $leafMode
            HumanReason = $humanReason
            LineNumber = $lineNumber
        })
    }

    return [pscustomobject]@{ Rows = @($resultRows.ToArray()); Problems = @($problems.ToArray()) }
}

function Test-TestInfrastructureLeafModeOverride {
    param([object[]]$Overrides)

    $problems = 0
    $rows = @($Overrides | Where-Object { $_.BatchId -eq $script:TestInfrastructureEvidenceMeasurementBatchId })
    if ($rows.Count -ne 1) {
        Write-Host "test_infrastructure_evidence_measurement_leaf_mode_override_row_count: expected one override row, found $($rows.Count)."
        return 1
    }

    if ($rows[0].LeafMode -ne 'method-diagnostic') {
        Write-Host "test_infrastructure_evidence_measurement_leaf_mode_override_wrong_mode: expected method-diagnostic, actual $($rows[0].LeafMode)."
        $problems++
    }

    return $problems
}

function Test-DesignConversionLiveLocalServerMethodDiagnosticLeafModeOverride {
    param([object[]]$Overrides)

    $problems = 0
    $rows = @($Overrides | Where-Object { $_.BatchId -eq $script:DesignConversionLiveLocalServerMethodDiagnosticBatchId })
    if ($rows.Count -ne 1) {
        Write-Host "design_conversion_live_local_server_method_diagnostic_leaf_mode_override_row_count: expected one override row, found $($rows.Count)."
        return 1
    }

    if ($rows[0].LeafMode -ne 'method-diagnostic') {
        Write-Host "design_conversion_live_local_server_method_diagnostic_leaf_mode_override_wrong_mode: expected method-diagnostic, actual $($rows[0].LeafMode)."
        $problems++
    }

    return $problems
}

function Test-DesignConversionDpoStorageLeafModeOverride {
    param([object[]]$Overrides)

    $problems = 0
    $rows = @($Overrides | Where-Object { $_.BatchId -eq $script:DesignConversionDpoStorageBatchId })
    if ($rows.Count -ne 1) {
        Write-Host "design_conversion_dpo_storage_leaf_mode_override_row_count: expected one override row, found $($rows.Count)."
        return 1
    }

    if ($rows[0].LeafMode -ne 'class-method-pack') {
        Write-Host "design_conversion_dpo_storage_leaf_mode_override_wrong_mode: expected class-method-pack, actual $($rows[0].LeafMode)."
        $problems++
    }

    return $problems
}

function Get-DllForTestFile {
    param([string]$RelativePath)

    if ($RelativePath -like 'Tests.ApiDashboardWpf\*') {
        return 'Tests.ApiDashboardWpf\bin\Release\net9.0-windows\Tests.ApiDashboardWpf.dll'
    }

    if ($RelativePath -like 'Tests\DesignConversion.ModelActivation.Tests\*') {
        return 'Tests\DesignConversion.ModelActivation.Tests\bin\Release\net9.0\DesignConversion.ModelActivation.Tests.dll'
    }

    return 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
}

function Get-TestProjectLinkedSourceDllMap {
    param([string]$Root)

    $map = @{}
    foreach ($project in Get-ChildItem -LiteralPath (Join-Path $Root 'Tests') -Recurse -File -Filter *.csproj) {
        [xml]$document = Get-Content -LiteralPath $project.FullName -Raw
        $packageReferences = @($document.SelectNodes("//*[local-name()='PackageReference']"))
        $isTestProject = $packageReferences | Where-Object {
            $_.GetAttribute('Include') -eq 'Microsoft.NET.Test.Sdk' -or
            $_.GetAttribute('Update') -eq 'Microsoft.NET.Test.Sdk'
        }
        if ($null -eq $isTestProject) {
            continue
        }

        $targetFrameworkNode = $document.SelectSingleNode("//*[local-name()='TargetFramework']")
        $targetFramework = if ($null -ne $targetFrameworkNode) { [string]$targetFrameworkNode.InnerText } else { '' }
        if ([string]::IsNullOrWhiteSpace($targetFramework)) {
            $targetFrameworksNode = $document.SelectSingleNode("//*[local-name()='TargetFrameworks']")
            if ($null -ne $targetFrameworksNode) {
                $targetFramework = (([string]$targetFrameworksNode.InnerText) -split ';' | Select-Object -First 1).Trim()
            }
        }

        if ([string]::IsNullOrWhiteSpace($targetFramework)) {
            continue
        }

        $assemblyNameNode = $document.SelectSingleNode("//*[local-name()='AssemblyName']")
        $assemblyName = if ($null -ne $assemblyNameNode -and -not [string]::IsNullOrWhiteSpace($assemblyNameNode.InnerText)) {
            [string]$assemblyNameNode.InnerText
        }
        else {
            [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
        }
        $projectDirectory = Split-Path -Parent $project.FullName
        $relativeProjectDirectory = $projectDirectory.Substring($Root.Length).TrimStart('\')
        $dll = (Join-Path (Join-Path $relativeProjectDirectory "bin\Release\$targetFramework") "$assemblyName.dll") -replace '/', '\'

        foreach ($compile in @($document.SelectNodes("//*[local-name()='Compile']"))) {
            $include = $compile.GetAttribute('Include')
            $link = $compile.GetAttribute('Link')
            if ([string]::IsNullOrWhiteSpace($include) -or [string]::IsNullOrWhiteSpace($link)) {
                continue
            }

            $fullSourcePath = if ([System.IO.Path]::IsPathRooted($include)) {
                [System.IO.Path]::GetFullPath($include)
            }
            else {
                [System.IO.Path]::GetFullPath((Join-Path $projectDirectory $include))
            }
            if (-not $map.ContainsKey($fullSourcePath)) {
                $map[$fullSourcePath] = New-Object System.Collections.Generic.List[string]
            }

            if (-not $map[$fullSourcePath].Contains($dll)) {
                [void]$map[$fullSourcePath].Add($dll)
            }
        }
    }

    return $map
}

function Get-SourceTestFiles {
    param([string[]]$Roots)

    $files = New-Object System.Collections.Generic.List[object]
    $pending = New-Object System.Collections.Generic.Stack[string]

    foreach ($root in $Roots) {
        if (Test-Path -LiteralPath $root) {
            $pending.Push($root)
        }
    }

    while ($pending.Count -gt 0) {
        $current = $pending.Pop()

        # Verifier читает исходники, а не runtime-артефакты. Если сначала зайти
        # в Tests/bin или Tests/obj, временные TestResults из VSTest могут
        # исчезнуть во время обхода и сорвать проверку уже готового RunRoot.
        $leafName = Split-Path -Leaf $current
        if ([string]::Equals($leafName, 'bin', [StringComparison]::OrdinalIgnoreCase) -or
            [string]::Equals($leafName, 'obj', [StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        foreach ($file in Get-ChildItem -LiteralPath $current -File -Filter *.cs -ErrorAction Stop) {
            $files.Add($file)
        }

        foreach ($directory in Get-ChildItem -LiteralPath $current -Directory -ErrorAction Stop) {
            $pending.Push($directory.FullName)
        }
    }

    return $files
}

function Get-TestClasses {
    param([string]$Root)

    $items = New-Object System.Collections.Generic.List[object]
    $linkedSourceDlls = Get-TestProjectLinkedSourceDllMap -Root $Root
    $testRoots = @(
        (Join-Path $Root 'Tests'),
        (Join-Path $Root 'Tests.ApiDashboardWpf')
    )
    $testFiles = Get-SourceTestFiles -Roots $testRoots

    foreach ($file in $testFiles) {
        $text = Get-Content -LiteralPath $file.FullName -Raw
        if ($text -notmatch '\[(Fact|Theory)[^\]]*\]' -and $text -notmatch '\[\w*(Fact|Theory)\w*') {
            continue
        }

        $relativePath = $file.FullName.Substring($Root.Length).TrimStart('\')
        $namespaceMatch = [regex]::Match($text, '(?m)^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*[;{]')
        $namespaceName = if ($namespaceMatch.Success) { $namespaceMatch.Groups[1].Value } else { '' }
        foreach ($match in [regex]::Matches($text, '(?m)^\s*public\s+(?:sealed\s+|partial\s+|abstract\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)')) {
            $className = $match.Groups[1].Value
            $fullyQualifiedName = if ([string]::IsNullOrWhiteSpace($namespaceName)) { $className } else { "$namespaceName.$className" }
            $dlls = [System.Collections.Generic.List[object]]::new()
            $dllNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            $primaryDll = Get-DllForTestFile $relativePath
            if ($dllNames.Add($primaryDll)) {
                [void]$dlls.Add([pscustomobject]@{
                    Dll = $primaryDll
                    IsLinkedSourceExpansion = $false
                })
            }
            if ($linkedSourceDlls.ContainsKey($file.FullName)) {
                foreach ($linkedDll in $linkedSourceDlls[$file.FullName]) {
                    if ($dllNames.Add($linkedDll)) {
                        [void]$dlls.Add([pscustomobject]@{
                            Dll = $linkedDll
                            IsLinkedSourceExpansion = $true
                        })
                    }
                }
            }

            # Внутренние публичные классы-помощники встречаются в тестовых
            # файлах. Для арифметики батчей они все равно получают корзину,
            # чтобы wildcard-остаток не скрывал дыру распределения.
            # Namespace нужен verifier-у по той же причине, что и runner-у:
            # runtime leaf-methods пишет Namespace.Class, и широкие короткие
            # термы не должны маскировать чужой метод как свой класс.
            foreach ($dll in $dlls) {
                $items.Add([pscustomobject]@{
                    ClassName = $className
                    FullyQualifiedName = $fullyQualifiedName
                    RelativePath = $relativePath
                    Dll = $dll.Dll
                    IsLinkedSourceExpansion = $dll.IsLinkedSourceExpansion
                })
            }
        }
    }

    return $items
}

function Get-TestMethods {
    param([string]$Root)

    $methods = New-Object System.Collections.Generic.HashSet[string]([StringComparer]::Ordinal)
    $testRoots = @(
        (Join-Path $Root 'Tests'),
        (Join-Path $Root 'Tests.ApiDashboardWpf')
    )
    $testFiles = Get-SourceTestFiles -Roots $testRoots

    foreach ($file in $testFiles) {
        $namespaceName = ''
        $className = ''
        $pendingFactOrTheory = $false
        foreach ($line in Get-Content -LiteralPath $file.FullName) {
            if ($line -match '^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*[;{]') {
                # VSTest печатает полный namespace.class.method, а старый
                # source-scanner знал только class.method. Для focused manifest
                # принимаем обе формы, чтобы старые manifest-ы не сломались.
                $namespaceName = $Matches[1]
                continue
            }

            if ($line -match '^\s*public\s+(?:sealed\s+|partial\s+|abstract\s+)*class\s+([A-Za-z_][A-Za-z0-9_]*)') {
                $className = $Matches[1]
                $pendingFactOrTheory = $false
                continue
            }

            if ($line -match '^\s*\[\w*(Fact|Theory)\w*(?:\(|\])') {
                $pendingFactOrTheory = $true
                continue
            }

            if ($pendingFactOrTheory -and $line -match '^\s*\[') {
                continue
            }

            if ($pendingFactOrTheory -and
                -not [string]::IsNullOrWhiteSpace($className) -and
                $line -match '^\s*public\s+(?:async\s+)?(?:Task|void)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(') {
                $methodName = $Matches[1]
                [void]$methods.Add("$className.$methodName")
                if (-not [string]::IsNullOrWhiteSpace($namespaceName)) {
                    [void]$methods.Add("$namespaceName.$className.$methodName")
                }
                $pendingFactOrTheory = $false
                continue
            }

            if (-not [string]::IsNullOrWhiteSpace($line) -and $line -notmatch '^\s*//') {
                $pendingFactOrTheory = $false
            }
        }
    }

    return [string[]]$methods
}

function Split-UnescapedPipe {
    param([string]$Filter)

    $parts = New-Object System.Collections.Generic.List[string]
    $builder = [System.Text.StringBuilder]::new()
    for ($index = 0; $index -lt $Filter.Length; $index++) {
        $character = $Filter[$index]
        if ($character -eq '|') {
            $backslashes = 0
            for ($cursor = $index - 1; $cursor -ge 0 -and $Filter[$cursor] -eq '\'; $cursor--) {
                $backslashes++
            }

            if (($backslashes % 2) -eq 0) {
                [void]$parts.Add($builder.ToString())
                [void]$builder.Clear()
                continue
            }
        }

        [void]$builder.Append($character)
    }

    [void]$parts.Add($builder.ToString())
    return [string[]]$parts
}

function Get-FocusedExactMethodTerms {
    param([string]$Filter)

    if ([string]::IsNullOrWhiteSpace($Filter)) {
        throw 'focused_filter_empty'
    }

    if ($Filter -eq '*' -or $Filter.Contains('*')) {
        throw 'focused_filter_wildcard'
    }

    if ($Filter.Contains('&')) {
        throw 'focused_filter_conjunction_not_allowed'
    }

    $terms = New-Object System.Collections.Generic.List[string]
    foreach ($piece in (Split-UnescapedPipe -Filter $Filter)) {
        $trimmed = $piece.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            throw 'focused_filter_empty_term'
        }

        if ($trimmed.Contains('!') -or $trimmed.Contains('~')) {
            throw "focused_filter_substring_or_negation_not_allowed:$trimmed"
        }

        if ($trimmed -notmatch '^FullyQualifiedName=([A-Za-z0-9_.+]+)$') {
            throw "focused_filter_must_be_exact_method_fqn:$trimmed"
        }

        [void]$terms.Add($Matches[1])
    }

    return [string[]]$terms
}

function Test-FocusedBatchRows {
    param(
        [object[]]$Rows,
        [string[]]$KnownMethods
    )

    $problems = 0
    if ($Rows.Count -lt 1) {
        Write-Host 'Focused manifest must contain at least one batch row.'
        $problems++
    }

    $known = New-Object System.Collections.Generic.HashSet[string]([StringComparer]::Ordinal)
    foreach ($method in $KnownMethods) {
        [void]$known.Add($method)
    }

    $seenMethods = New-Object System.Collections.Generic.HashSet[string]([StringComparer]::Ordinal)
    foreach ($row in $Rows) {
        try {
            # Focused mode нарочно принимает только exact method FQN. Substring,
            # wildcard и отрицания остаются правом canonical ladder, иначе
            # маленький acceptance-manifest легко превратится в скрытый broad run.
            $methods = @(Get-FocusedExactMethodTerms -Filter $row.Filter)
            foreach ($method in $methods) {
                if (-not $seenMethods.Add($method)) {
                    Write-Host "Focused manifest assigns method more than once: $method"
                    $problems++
                }

                if (-not $known.Contains($method)) {
                    Write-Host "Focused manifest references unknown test method: $method"
                    $problems++
                }
            }
        }
        catch {
            Write-Host "Focused batch $($row.BatchId) has invalid exact filter: $($_.Exception.Message)"
            $problems++
        }
    }

    return $problems
}

function Get-FileSha256Lower {
    param([string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-TextSha256Lower {
    param([string]$Text)

    $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Normalize-FocusedExactFilterForBinding {
    param(
        [string]$Filter,
        [switch]$AllowLegacyOuterParentheses
    )

    $candidate = $Filter.Trim()
    if ($AllowLegacyOuterParentheses -and
        $candidate.StartsWith('(', [StringComparison]::Ordinal) -and
        $candidate.EndsWith(')', [StringComparison]::Ordinal) -and
        $candidate.Length -gt 2) {
        # Единственная legacy-поблажка: старый focused runner оборачивал весь
        # exact-disjunction в одну пару скобок. Порядок методов и сами методы
        # остаются байндящим контрактом; substring/wildcard сюда не проходят.
        $candidate = $candidate.Substring(1, $candidate.Length - 2).Trim()
    }

    $methods = @(Get-FocusedExactMethodTerms -Filter $candidate)
    return (($methods | ForEach-Object { "FullyQualifiedName=$_" }) -join '|')
}

function New-FocusedExpectedBatchMap {
    param(
        [object[]]$Rows,
        [string]$ManifestPath
    )

    $byBatchId = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    $manifestSha256 = Get-FileSha256Lower -Path $ManifestPath
    $totalMethodCount = 0

    foreach ($row in $Rows) {
        $methods = @(Get-FocusedExactMethodTerms -Filter $row.Filter)
        $methodSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($method in $methods) {
            [void]$methodSet.Add($method)
        }

        $totalMethodCount += $methodSet.Count
        $byBatchId[$row.BatchId] = [pscustomobject]@{
            BatchId = $row.BatchId
            Methods = $methodSet
            NormalizedFilter = Normalize-FocusedExactFilterForBinding -Filter $row.Filter
            ManifestSha256 = $manifestSha256
        }
    }

    return [pscustomobject]@{
        ByBatchId = $byBatchId
        BatchIds = [string[]]$byBatchId.Keys
        ManifestSha256 = $manifestSha256
        TotalMethodCount = $totalMethodCount
    }
}

function Test-FocusedRunBatchSet {
    param(
        [object[]]$BatchDirs,
        [object[]]$JournalRows,
        [object]$FocusedExpectedBatches
    )

    if ($null -eq $FocusedExpectedBatches) {
        return 0
    }

    $problems = 0
    $expected = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($batchId in $FocusedExpectedBatches.BatchIds) {
        [void]$expected.Add($batchId)
    }

    $actualDirs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($batchDir in $BatchDirs) {
        [void]$actualDirs.Add((Get-BatchIdFromDirectory -Path $batchDir.FullName))
    }

    $journalIds = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($journalRow in $JournalRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.BatchId) -and $_.State -ne 'CORRUPT_JOURNAL_ROW' }) {
        [void]$journalIds.Add($journalRow.BatchId)
    }

    foreach ($batchId in $expected) {
        if (-not $actualDirs.Contains($batchId)) {
            Write-Host "Focused run root is missing manifest batch directory: $batchId"
            $problems++
        }
        if (-not $journalIds.Contains($batchId)) {
            Write-Host "Focused run root is missing manifest journal row: $batchId"
            $problems++
        }
    }

    foreach ($batchId in $actualDirs) {
        if (-not $expected.Contains($batchId)) {
            Write-Host "Focused run root has batch outside current manifest: $batchId"
            $problems++
        }
    }

    foreach ($batchId in $journalIds) {
        if (-not $expected.Contains($batchId)) {
            Write-Host "Focused journal has batch outside current manifest: $batchId"
            $problems++
        }
    }

    return $problems
}

function Read-KeyValueTsv {
    param([string]$Path)

    $values = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        return $values
    }

    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++
        if ($lineNumber -eq 1 -and $line -eq "key`tvalue") {
            continue
        }

        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $parts = $line -split "`t", 2
        if ($parts.Count -eq 2 -and -not [string]::IsNullOrWhiteSpace($parts[0])) {
            $values[$parts[0]] = $parts[1]
        }
    }

    return $values
}

function Test-KeyValueEquals {
    param(
        [hashtable]$Values,
        [string]$Key,
        [string]$Expected,
        [string]$Context
    )

    if (-not $Values.ContainsKey($Key)) {
        Write-Host "$Context has no required evidence key: $Key"
        return 1
    }

    if ([string]$Values[$Key] -ne $Expected) {
        Write-Host "$Context evidence key differs: $Key"
        return 1
    }

    return 0
}

function Normalize-DllTextForCompare {
    param([string]$Value)

    return $Value.Replace([char]92, [char]'/')
}

function Get-CurrentGitHeadForEvidence {
    try {
        $head = (& git -C $RepoRoot rev-parse HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($head)) {
            return ([string]$head).Trim()
        }
    }
    catch {
    }

    return ''
}

function Get-RunScopeValue {
    param([string]$Path)

    $scopeFile = Join-Path $Path 'verification-scope.tsv'
    $values = Read-KeyValueTsv -Path $scopeFile
    if ($values.ContainsKey('scope')) {
        return $values['scope']
    }

    return ''
}

function Read-RunInvocationHistory {
    param([string]$Path)

    $rows = New-Object System.Collections.Generic.List[object]
    $problems = 0

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "Run invocation history is missing: $Path"
        return [pscustomobject]@{ Problems = 1; Rows = @(); SelectedBatchCountSum = 0 }
    }

    $text = [System.IO.File]::ReadAllText($Path, [System.Text.UTF8Encoding]::new($false))
    if ([string]::IsNullOrEmpty($text)) {
        Write-Host "Run invocation history is empty: $Path"
        return [pscustomobject]@{ Problems = 1; Rows = @(); SelectedBatchCountSum = 0 }
    }

    if (-not $text.EndsWith("`n", [StringComparison]::Ordinal)) {
        Write-Host "Run invocation history does not end with a newline before append: $Path"
        $problems++
    }

    $lines = [System.IO.File]::ReadAllLines($Path, [System.Text.UTF8Encoding]::new($false))
    if ($lines[0] -ne $script:RunInvocationHistoryHeader) {
        Write-Host "Run invocation history header mismatch: $Path"
        return [pscustomobject]@{ Problems = 1; Rows = @(); SelectedBatchCountSum = 0 }
    }

    $seenIds = New-Object 'System.Collections.Generic.HashSet[string]'
    $previousPayload = ''
    $selectedBatchCountSum = 0

    for ($index = 1; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        if ([string]::IsNullOrWhiteSpace($line)) {
            Write-Host "Run invocation history has blank data row: line $($index + 1)"
            $problems++
            continue
        }

        $parts = $line.Split([char]9)
        $expectedOrder = $index
        if ($parts.Count -ne 17) {
            Write-Host "Run invocation history row $expectedOrder has $($parts.Count) columns instead of 17."
            $problems++
            continue
        }

        $actualOrder = 0
        if (-not [int]::TryParse($parts[0], [ref]$actualOrder) -or $actualOrder -ne $expectedOrder) {
            Write-Host "Run invocation history row order is nonmonotonic: expected $expectedOrder, actual $($parts[0])."
            $problems++
        }

        $expectedId = 'invocation-{0:000000}' -f $expectedOrder
        if ($parts[1] -ne $expectedId) {
            Write-Host "Run invocation history id mismatch on row ${expectedOrder}: expected $expectedId, actual $($parts[1])."
            $problems++
        }
        if (-not $seenIds.Add($parts[1])) {
            Write-Host "Run invocation history duplicate id: $($parts[1])"
            $problems++
        }

        if ($parts[2] -notmatch '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$') {
            Write-Host "Run invocation history timestamp is not UTC ISO-8601 on row ${expectedOrder}: $($parts[2])"
            $problems++
        }
        if ($parts[3] -notin @('CANONICAL', 'FOCUSED')) {
            Write-Host "Run invocation history scope is unknown on row ${expectedOrder}: $($parts[3])"
            $problems++
        }

        $selectedCount = 0
        if (-not [int]::TryParse($parts[5], [ref]$selectedCount) -or $selectedCount -lt 0) {
            Write-Host "Run invocation history selected_batch_count is invalid on row ${expectedOrder}: $($parts[5])"
            $problems++
        }
        else {
            $selectedBatchCountSum += $selectedCount
        }

        if ($parts[6] -notin @('pack', 'method-diagnostic', 'class-method-pack', 'off')) {
            Write-Host "Run invocation history requested leaf mode is unknown on row ${expectedOrder}: $($parts[6])"
            $problems++
        }
        foreach ($budgetColumn in @(8, 9)) {
            $budgetValue = 0
            if (-not [int]::TryParse($parts[$budgetColumn], [ref]$budgetValue) -or $budgetValue -lt 0) {
                Write-Host "Run invocation history budget is invalid on row $expectedOrder column ${budgetColumn}: $($parts[$budgetColumn])"
                $problems++
            }
        }
        foreach ($booleanColumn in @(10, 11, 12)) {
            if ($parts[$booleanColumn] -notin @('true', 'false')) {
                Write-Host "Run invocation history boolean is invalid on row $expectedOrder column ${booleanColumn}: $($parts[$booleanColumn])"
                $problems++
            }
        }
        if ($parts[13] -notmatch '^[0-9a-f]{64}$') {
            Write-Host "Run invocation history batches hash is invalid on row ${expectedOrder}: $($parts[13])"
            $problems++
        }
        if ($parts[14] -ne 'STARTED') {
            Write-Host "Run invocation history status_at_append is invalid on row ${expectedOrder}: $($parts[14])"
            $problems++
        }

        $expectedPreviousHash = Get-TextSha256Lower -Text $previousPayload
        if ($parts[15] -ne $expectedPreviousHash) {
            Write-Host "Run invocation history previous-row hash mismatch on row $expectedOrder."
            $problems++
        }

        $rowWithoutHash = ($parts[0..15] -join "`t")
        $expectedRowHash = Get-TextSha256Lower -Text $rowWithoutHash
        if ($parts[16] -ne $expectedRowHash) {
            Write-Host "Run invocation history row hash mismatch on row $expectedOrder."
            $problems++
        }

        $rows.Add([pscustomobject]@{
            Order = $actualOrder
            InvocationId = $parts[1]
            StartedUtc = $parts[2]
            VerificationScope = $parts[3]
            SelectedBatches = $parts[4]
            SelectedBatchCount = $selectedCount
            RequestedLeafMode = $parts[6]
            EffectiveLeafModes = $parts[7]
            MaxLeaves = $parts[8]
            MaxElapsedMinutes = $parts[9]
            PlanOnly = $parts[10]
            NoBuild = $parts[11]
            FocusedMode = $parts[12]
            BatchesTsvSha256 = $parts[13]
            StatusAtAppend = $parts[14]
            PreviousRowsSha256 = $parts[15]
            RowSha256 = $parts[16]
        }) | Out-Null

        $previousPayload += $line + "`n"
    }

    return [pscustomobject]@{
        Problems = $problems
        Rows = @($rows.ToArray())
        SelectedBatchCountSum = $selectedBatchCountSum
    }
}

function Test-RunInvocationHistoryForRunRoot {
    param(
        [string]$Path,
        [hashtable]$ScopeValues,
        [object[]]$JournalRows
    )

    $history = Read-RunInvocationHistory -Path (Join-Path $Path 'run-invocations.tsv')
    $problems = $history.Problems
    if ($problems -ne 0) {
        return $problems
    }

    $rows = @($history.Rows)
    if ($rows.Count -eq 0) {
        Write-Host "Run invocation history has no invocation rows: $Path"
        return ($problems + 1)
    }

    $last = $rows[-1]
    if ($ScopeValues.ContainsKey('scope') -and $last.VerificationScope -ne $ScopeValues['scope']) {
        Write-Host "Run invocation history scope does not match verification-scope.tsv: $($last.VerificationScope) vs $($ScopeValues['scope'])"
        $problems++
    }
    if ($ScopeValues.ContainsKey('batches_tsv_sha256') -and $last.BatchesTsvSha256 -ne $ScopeValues['batches_tsv_sha256']) {
        Write-Host "Run invocation history batches hash does not match verification-scope.tsv."
        $problems++
    }
    if ($ScopeValues.ContainsKey('invocation_max_leaves') -and $last.MaxLeaves -ne $ScopeValues['invocation_max_leaves']) {
        Write-Host "Run invocation history latest max_leaves does not match verification-scope.tsv: $($last.MaxLeaves) vs $($ScopeValues['invocation_max_leaves'])"
        $problems++
    }
    if ($ScopeValues.ContainsKey('invocation_max_elapsed_minutes') -and $last.MaxElapsedMinutes -ne $ScopeValues['invocation_max_elapsed_minutes']) {
        Write-Host "Run invocation history latest max_elapsed_minutes does not match verification-scope.tsv: $($last.MaxElapsedMinutes) vs $($ScopeValues['invocation_max_elapsed_minutes'])"
        $problems++
    }

    $nonCorruptJournalRows = @($JournalRows | Where-Object { $_.State -ne 'CORRUPT_JOURNAL_ROW' })
    if ($nonCorruptJournalRows.Count -gt $history.SelectedBatchCountSum) {
        Write-Host "Run invocation history cannot account for journal rows: selected batch capacity $($history.SelectedBatchCountSum), journal rows $($nonCorruptJournalRows.Count)."
        $problems++
    }

    return $problems
}

function Get-RunRootCounts {
    param(
        [string]$Path,
        [object[]]$Rows = @()
    )

    $counts = [pscustomobject]@{ Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Rows = 0; RawRows = 0; EmptyVerified = 0 }
    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $counts
    }

    $journalPath = Join-Path $Path 'journal.tsv'
    if (-not (Test-Path -LiteralPath $journalPath)) {
        return $counts
    }

    $journalRows = @(Read-JournalRows -Path $journalPath)
    $counts.RawRows = @($journalRows | Where-Object { $_.State -ne 'CORRUPT_JOURNAL_ROW' }).Count

    foreach ($row in @(Get-LatestJournalRowsByManifestOrder -JournalRows $journalRows -Rows $Rows)) {
        $projectedRow = $row
        $leafProjection = Get-LeafBatchCountsForRunRootProjection -RunRoot $Path -JournalRow $row
        if ($leafProjection.Valid) {
            # Верхний journal остаётся append-only историей, но публичная
            # projected-сводка leaf-packed batch должна брать проверенную leaf
            # арифметику. Это нужно для старых RunRoot, где raw batch row ещё
            # хранит до-normalization Qwen runtime-skip как Failed.
            $projectedRow = $leafProjection
        }

        $counts.Total += $projectedRow.Total
        $counts.Passed += $projectedRow.Passed
        $counts.Failed += $projectedRow.Failed
        $counts.NotExecuted += $projectedRow.NotExecuted
        if ($row.State -eq 'EMPTY_VERIFIED') {
            $counts.EmptyVerified++
        }
        $counts.Rows++
    }

    return $counts
}

function Get-LeafBatchCountsForRunRootProjection {
    param(
        [string]$RunRoot,
        [object]$JournalRow
    )

    $empty = [pscustomobject]@{ Valid = $false; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    if ($null -eq $JournalRow) {
        return $empty
    }

    $resultLeafName = (([string]$JournalRow.ResultPath) -replace '\\', '/') -split '/' | Select-Object -Last 1
    if ($resultLeafName -ne 'leaf-journal.tsv') {
        return $empty
    }

    $batchDir = $null
    foreach ($candidate in @(Get-ChildItem -LiteralPath $RunRoot -Directory -Filter 'batch-*' -ErrorAction SilentlyContinue)) {
        if ((Get-BatchIdFromDirectory -Path $candidate.FullName) -eq $JournalRow.BatchId) {
            $batchDir = $candidate.FullName
            break
        }
    }

    if ([string]::IsNullOrWhiteSpace($batchDir)) {
        return $empty
    }

    $planPath = Join-Path $batchDir 'leaf-plan.tsv'
    $methodPath = Join-Path $batchDir 'leaf-methods.tsv'
    if (-not (Test-Path -LiteralPath $planPath) -or -not (Test-Path -LiteralPath $methodPath)) {
        return $empty
    }

    try {
        $planRows = @(Read-LeafPlanRows -Path $planPath)
        $methodRows = @(Read-LeafMethodRows -Path $methodPath)
    }
    catch {
        return $empty
    }

    $total = 0
    $passed = 0
    $failed = 0
    $notExecuted = 0
    foreach ($planRow in $planRows) {
        $leafDir = Join-Path $batchDir $planRow.LeafId
        $leafState = Read-StateValue -Path (Join-Path $leafDir 'state.txt')

        if ($leafState -in @('COMPLETED_GREEN', 'COMPLETED_RED')) {
            $attemptDir = Get-CurrentAttemptDirectory -BatchDir $leafDir
            $trx = if ([string]::IsNullOrWhiteSpace($attemptDir)) { Join-Path $leafDir 'result.trx' } else { Join-Path $attemptDir 'result.trx' }
            $membership = Test-LeafTrxMembership -TrxPath $trx -MethodRows $methodRows -PlanRow $planRow
            if (-not $membership.Valid) {
                return $empty
            }

            $total += $membership.Total
            $passed += $membership.Passed
            $failed += $membership.Failed
            $notExecuted += $membership.NotExecuted
        }
        elseif ($leafState -eq 'LEAF_TIMED_OUT') {
            # Timeout leaf остаётся честной красной единицей, но projected
            # totals должны считать её как planned NotExecuted, ровно как
            # evidence verifier делает ниже.
            $total += $planRow.EstimatedRows
            $notExecuted += $planRow.EstimatedRows
        }
        else {
            return $empty
        }
    }

    return [pscustomobject]@{
        Valid = $true
        Total = $total
        Passed = $passed
        Failed = $failed
        NotExecuted = $notExecuted
    }
}

function Get-PositiveFilterTerms {
    param([string]$Filter)

    $terms = New-Object System.Collections.Generic.List[string]
    foreach ($piece in ($Filter -split '[|&]')) {
        $trimmed = $piece.Trim().TrimStart('(').TrimEnd(')')
        if ($trimmed.StartsWith('FullyQualifiedName~')) {
            [void]$terms.Add($trimmed.Substring('FullyQualifiedName~'.Length))
        }
    }

    return $terms
}

function Get-NegativeFilterTerms {
    param([string]$Filter)

    $terms = New-Object System.Collections.Generic.List[string]
    foreach ($piece in ($Filter -split '[|&]')) {
        $trimmed = $piece.Trim().TrimStart('(').TrimEnd(')')
        if ($trimmed.StartsWith('FullyQualifiedName!~')) {
            [void]$terms.Add($trimmed.Substring('FullyQualifiedName!~'.Length))
        }
    }

    return $terms
}

function Normalize-FilterTermForParsing {
    param([string]$Term)

    $trimmed = $Term.Trim()
    while ($trimmed.StartsWith('(')) {
        $trimmed = $trimmed.Substring(1).Trim()
    }

    while ($trimmed.EndsWith(')')) {
        $trimmed = $trimmed.Substring(0, $trimmed.Length - 1).Trim()
    }

    return $trimmed
}

# Проверяющий скрипт тысячи раз спрашивает один и тот же вопрос:
# "попадает ли этот класс под эту строку списка групп?". Здесь мы заранее
# разбираем фильтр на безопасные части для быстрого повторного применения.
# Плоские списки положительных и отрицательных термов остаются построенными
# старыми извлекателями, потому что они питают effective-filter и не должны
# менять историческую арифметику первого совпадения.
function New-ParsedFilter {
    param([string]$Filter)

    $positiveTerms = New-Object System.Collections.Generic.List[string]
    $negativeTerms = New-Object System.Collections.Generic.List[string]
    $clauses = New-Object System.Collections.Generic.List[object]

    if ($Filter -eq '*') {
        return [pscustomobject]@{
            Filter = $Filter
            IsWildcard = $true
            Clauses = @()
            PositiveTerms = @()
            NegativeTerms = @()
        }
    }

    foreach ($orPart in ($Filter -split '\|')) {
        $terms = New-Object System.Collections.Generic.List[object]
        foreach ($piece in ($orPart -split '&')) {
            $trimmed = Normalize-FilterTermForParsing -Term $piece
            if ($trimmed -eq '*') {
                [void]$terms.Add([pscustomobject]@{ Operator = 'wildcard'; Term = '' })
                continue
            }

            if ($trimmed.StartsWith('FullyQualifiedName~')) {
                $term = $trimmed.Substring('FullyQualifiedName~'.Length)
                [void]$terms.Add([pscustomobject]@{ Operator = 'contains'; Term = $term })
                continue
            }

            if ($trimmed.StartsWith('FullyQualifiedName!~')) {
                $term = $trimmed.Substring('FullyQualifiedName!~'.Length)
                [void]$terms.Add([pscustomobject]@{ Operator = 'not_contains'; Term = $term })
                continue
            }

            # Старый путь разбора фильтра вычисляет части лениво: если первая
            # ветка уже совпала, неподдержанный хвост не трогается. Поэтому
            # подготовленный объект хранит такой терм и бросает ошибку только
            # тогда, когда вычисление реально дошло до него.
            [void]$terms.Add([pscustomobject]@{ Operator = 'raw_unsupported'; Term = $trimmed })
        }

        [void]$clauses.Add([object]([pscustomobject]@{ Terms = $terms.ToArray() }))
    }

    foreach ($term in (Get-PositiveFilterTerms -Filter $Filter)) {
        [void]$positiveTerms.Add($term)
    }

    foreach ($term in (Get-NegativeFilterTerms -Filter $Filter)) {
        [void]$negativeTerms.Add($term)
    }

    return [pscustomobject]@{
        Filter = $Filter
        IsWildcard = $false
        Clauses = $clauses.ToArray()
        PositiveTerms = $positiveTerms.ToArray()
        NegativeTerms = $negativeTerms.ToArray()
    }
}

function Test-ParsedFilterTerm {
    param(
        [object]$ParsedTerm,
        [string]$FullyQualifiedName
    )

    if ($ParsedTerm.Operator -eq 'wildcard') {
        return $true
    }

    if ($ParsedTerm.Operator -eq 'contains') {
        return Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $ParsedTerm.Term
    }

    if ($ParsedTerm.Operator -eq 'not_contains') {
        return -not (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $ParsedTerm.Term)
    }

    if ($ParsedTerm.Operator -eq 'raw_unsupported') {
        throw "Unsupported filter term: $($ParsedTerm.Term)"
    }

    throw "Unsupported parsed filter operator: $($ParsedTerm.Operator)"
}

# Подготовленный фильтр можно применять только к той же самой строке. Точное
# сравнение защищает от "почти того же" фильтра: смена регистра оператора,
# перестановка или мутация строки должна идти старым свежим путём и падать или
# пересчитываться ровно так же, как до ускорения.
function Test-ParsedBatchFilter {
    param(
        [string]$Filter,
        [string]$FullyQualifiedName,
        [object]$ParsedFilter = $null
    )

    if ($null -eq $ParsedFilter -or -not [string]::Equals($ParsedFilter.Filter, $Filter, [StringComparison]::Ordinal)) {
        return Test-BatchFilter -Filter $Filter -FullyQualifiedName $FullyQualifiedName
    }

    if ($ParsedFilter.IsWildcard) {
        return $true
    }

    foreach ($clause in @($ParsedFilter.Clauses)) {
        $allMatched = $true
        foreach ($term in @($clause.Terms)) {
            if (-not (Test-ParsedFilterTerm -ParsedTerm $term -FullyQualifiedName $FullyQualifiedName)) {
                $allMatched = $false
                break
            }
        }

        if ($allMatched) {
            return $true
        }
    }

    return $false
}

function Build-EffectiveFilter {
    param(
        [string]$RawFilter,
        [string[]]$PriorTerms
    )

    # Это та же first-match арифметика, что в bash-runner: строка сохраняет
    # свой положительный фильтр, а все более ранние назначения той же сборки
    # становятся явными отрицаниями.
    $effective = ''
    if ($RawFilter -ne '*') {
        $effective = "($RawFilter)"
    }

    foreach ($term in $PriorTerms) {
        if ([string]::IsNullOrWhiteSpace($term)) {
            continue
        }

        if ([string]::IsNullOrWhiteSpace($effective)) {
            $effective = "FullyQualifiedName!~$term"
        }
        else {
            $effective = "$effective&FullyQualifiedName!~$term"
        }
    }

    if ([string]::IsNullOrWhiteSpace($effective)) {
        return '*'
    }

    return $effective
}

function Get-EffectiveRows {
    param([object[]]$Rows)

    $termsByDll = @{}
    $effectiveRows = New-Object System.Collections.Generic.List[object]

    foreach ($row in $Rows) {
        if (-not $termsByDll.ContainsKey($row.Dll)) {
            $termsByDll[$row.Dll] = New-Object System.Collections.Generic.List[string]
        }

        $parsedFilter = New-ParsedFilter -Filter $row.Filter
        $priorTerms = @($termsByDll[$row.Dll])
        $effectiveFilter = Build-EffectiveFilter -RawFilter $row.Filter -PriorTerms $priorTerms
        $parsedEffectiveFilter = New-ParsedFilter -Filter $effectiveFilter
        $effectiveRows.Add([pscustomobject]@{
            Order = $row.Order
            BatchId = $row.BatchId
            Dll = $row.Dll
            Filter = $row.Filter
            RawFilter = $row.Filter
            EffectiveFilter = $effectiveFilter
            PriorTerms = $priorTerms.Count
            ParsedFilter = $parsedFilter
            ParsedEffectiveFilter = $parsedEffectiveFilter
        })

        foreach ($term in @($parsedFilter.PositiveTerms)) {
            [void]$termsByDll[$row.Dll].Add($term)
        }
    }

    return $effectiveRows
}

function Convert-ClassBoundaryTermToClassFqn {
    param([string]$Term)

    if ($Term.EndsWith('.')) {
        return $Term.Substring(0, $Term.Length - 1)
    }

    return $Term
}

function Test-ClassOrMethodFilterTerm {
    param(
        [string]$FullyQualifiedName,
        [string]$Term
    )

    if ($Term.EndsWith('.')) {
        $classFqn = Convert-ClassBoundaryTermToClassFqn -Term $Term
        return $FullyQualifiedName.Equals($classFqn, [StringComparison]::OrdinalIgnoreCase) -or
            $FullyQualifiedName.StartsWith($Term, [StringComparison]::OrdinalIgnoreCase)
    }

    return $FullyQualifiedName.IndexOf($Term, [StringComparison]::OrdinalIgnoreCase) -ge 0
}

function Test-LadderEffectiveFilters {
    param([object[]]$EffectiveRows)

    $problems = 0
    foreach ($row in $EffectiveRows) {
        if ($row.RawFilter -eq '*' -and $row.PriorTerms -gt 0 -and $row.EffectiveFilter -eq '*') {
            Write-Host "Late residual batch became wildcard: $($row.BatchId)."
            $problems++
        }
    }

    $coreRemainder = $EffectiveRows | Where-Object { $_.BatchId -eq 'core-m-z-and-remainder' } | Select-Object -First 1
    if ($null -eq $coreRemainder -or $coreRemainder.EffectiveFilter -eq '*' -or $coreRemainder.PriorTerms -le 0) {
        Write-Host "Core residual batch has no deterministic prior exclusions."
        $problems++
    }

    return $problems
}

function Test-FilterTerm {
    param(
        [string]$Term,
        [string]$FullyQualifiedName
    )

    $trimmed = $Term.Trim()
    while ($trimmed.StartsWith('(')) {
        $trimmed = $trimmed.Substring(1).Trim()
    }

    while ($trimmed.EndsWith(')')) {
        $trimmed = $trimmed.Substring(0, $trimmed.Length - 1).Trim()
    }

    if ($trimmed -eq '*') {
        return $true
    }

    if ($trimmed.StartsWith('FullyQualifiedName~')) {
        $needle = $trimmed.Substring('FullyQualifiedName~'.Length)
        return Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $needle
    }

    if ($trimmed.StartsWith('FullyQualifiedName!~')) {
        $needle = $trimmed.Substring('FullyQualifiedName!~'.Length)
        return -not (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $needle)
    }

    throw "Unsupported filter term: $trimmed"
}

function Test-BatchFilter {
    param(
        [string]$Filter,
        [string]$FullyQualifiedName
    )

    if ($Filter -eq '*') {
        return $true
    }

    foreach ($orPart in ($Filter -split '\|')) {
        $andTerms = $orPart -split '&'
        $allMatched = $true
        foreach ($term in $andTerms) {
            if (-not (Test-FilterTerm -Term $term -FullyQualifiedName $FullyQualifiedName)) {
                $allMatched = $false
                break
            }
        }

        if ($allMatched) {
            return $true
        }
    }

    return $false
}

function Test-EffectiveFilterForClass {
    param(
        [string]$Filter,
        [string]$FullyQualifiedName,
        [object]$ParsedFilter = $null
    )

    if ($null -ne $ParsedFilter -and [string]::Equals($ParsedFilter.Filter, $Filter, [StringComparison]::Ordinal)) {
        if ($ParsedFilter.IsWildcard) {
            return $true
        }

        # В горячем canonical-цикле один и тот же effective-filter проверяется
        # против сотен классов. Подготовленные термы экономят повторный split,
        # но только если исходная строка фильтра совпадает с разобранной строкой.
        foreach ($negative in @($ParsedFilter.NegativeTerms)) {
            if (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $negative) {
                return $false
            }
        }

        $positives = @($ParsedFilter.PositiveTerms)
        if ($positives.Count -eq 0) {
            return $true
        }

        foreach ($positive in $positives) {
            if (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $positive) {
                return $true
            }
        }

        return $false
    }

    if ($Filter -eq '*') {
        return $true
    }

    # Effective-фильтр runner-а состоит из положительных FullyQualifiedName~
    # терминов и отрицаний FullyQualifiedName!~. Для проверки пересечений нам
    # нужен именно смысл этих терминов, а не строковый split по скобкам.
    foreach ($negative in (Get-NegativeFilterTerms -Filter $Filter)) {
        if (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $negative) {
            return $false
        }
    }

    $positives = @(Get-PositiveFilterTerms -Filter $Filter)
    if ($positives.Count -eq 0) {
        return $true
    }

    foreach ($positive in $positives) {
        if (Test-ClassOrMethodFilterTerm -FullyQualifiedName $FullyQualifiedName -Term $positive) {
            return $true
        }
    }

    return $false
}

function Find-FirstBatch {
    param(
        [object]$TestClass,
        [object[]]$Rows,
        [object[]]$PreparedRows = $null
    )

    for ($index = 0; $index -lt $Rows.Count; $index++) {
        $row = $Rows[$index]
        if ($row.Dll -ne $TestClass.Dll) {
            continue
        }

        $parsedFilter = $null
        if ($null -ne $PreparedRows -and $PreparedRows.Count -eq $Rows.Count) {
            $preparedRow = $PreparedRows[$index]
            $preparedNames = @($preparedRow.PSObject.Properties.Name)
            if ($preparedNames -contains 'Order' -and
                $preparedNames -contains 'BatchId' -and
                $preparedNames -contains 'Dll' -and
                $preparedNames -contains 'ParsedFilter' -and
                $preparedRow.Order -eq $row.Order -and
                $preparedRow.BatchId -eq $row.BatchId -and
                $preparedRow.Dll -eq $row.Dll) {
                $parsedFilter = $preparedRow.ParsedFilter
            }
        }

        if (Test-ParsedBatchFilter -Filter $row.Filter -FullyQualifiedName $TestClass.FullyQualifiedName -ParsedFilter $parsedFilter) {
            return $row
        }
    }

    return $null
}

function New-PreparedFilterAssignmentMapForSelfCheck {
    param(
        [object[]]$Rows,
        [object[]]$Classes,
        [object[]]$PreparedRows = $null
    )

    $map = @{}
    foreach ($testClass in $Classes) {
        $arguments = @{
            TestClass = $testClass
            Rows = $Rows
        }
        if ($null -ne $PreparedRows) {
            $arguments.PreparedRows = $PreparedRows
        }

        $batch = Find-FirstBatch @arguments
        $key = "$($testClass.Dll)|$($testClass.FullyQualifiedName)"
        if ($null -eq $batch) {
            $map[$key] = ''
        }
        else {
            $map[$key] = $batch.BatchId
        }
    }

    return $map
}

function Test-PreparedFilterAssignmentMap {
    param(
        [hashtable]$Actual,
        [hashtable]$Expected,
        [string]$Label
    )

    $problems = 0
    foreach ($key in $Expected.Keys) {
        if (-not $Actual.ContainsKey($key)) {
            Write-Host "prepared_filter_parsing_self_check_${Label}_missing_key: $key"
            $problems++
            continue
        }

        if ($Actual[$key] -ne $Expected[$key]) {
            Write-Host "prepared_filter_parsing_self_check_${Label}_wrong_owner: $key expected $($Expected[$key]), actual $($Actual[$key])"
            $problems++
        }
    }

    foreach ($key in $Actual.Keys) {
        if (-not $Expected.ContainsKey($key)) {
            Write-Host "prepared_filter_parsing_self_check_${Label}_unexpected_key: $key"
            $problems++
        }
    }

    return $problems
}

function Test-PreparedFilterThrows {
    param([scriptblock]$Action)

    try {
        [void](& $Action)
        return $false
    }
    catch {
        return $true
    }
}

function Test-PreparedFilterParsingSelfCheck {
    $dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
    $otherDll = 'Tests\Other\bin\Release\net9.0\Other.Tests.dll'
    $rows = @(
        [pscustomobject]@{ Order = '01'; BatchId = 'target-boundary'; Dll = $dll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '02'; BatchId = 'broad-example-with-negative'; Dll = $dll; Filter = 'FullyQualifiedName~Example.&FullyQualifiedName!~Example.Blocked.' },
        [pscustomobject]@{ Order = '03'; BatchId = 'namespace-target'; Dll = $dll; Filter = 'FullyQualifiedName~OtherNamespace.Target.' },
        [pscustomobject]@{ Order = '04'; BatchId = 'other-dll-target'; Dll = $otherDll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '05'; BatchId = 'remainder'; Dll = $dll; Filter = '*' }
    )
    $classes = @(
        [pscustomobject]@{ Dll = $dll; ClassName = 'Target'; FullyQualifiedName = 'Example.Target' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'TargetLookalike'; FullyQualifiedName = 'Example.TargetLookalike' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'Blocked'; FullyQualifiedName = 'Example.Blocked' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'Target'; FullyQualifiedName = 'OtherNamespace.Target' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'Class'; FullyQualifiedName = 'Random.Class' },
        [pscustomobject]@{ Dll = $otherDll; ClassName = 'Target'; FullyQualifiedName = 'Example.Target' }
    )
    $expected = @{
        "$dll|Example.Target" = 'target-boundary'
        "$dll|Example.TargetLookalike" = 'broad-example-with-negative'
        "$dll|Example.Blocked" = 'remainder'
        "$dll|OtherNamespace.Target" = 'namespace-target'
        "$dll|Random.Class" = 'remainder'
        "$otherDll|Example.Target" = 'other-dll-target'
    }

    $problems = 0
    $preparedRows = @(Get-EffectiveRows -Rows $rows)
    $noPreparedMap = New-PreparedFilterAssignmentMapForSelfCheck -Rows $rows -Classes $classes
    $preparedMap = New-PreparedFilterAssignmentMapForSelfCheck -Rows $rows -Classes $classes -PreparedRows $preparedRows

    # Самопроверка покрывает не только "счастливый путь": она сравнивает старый
    # и быстрый маршрут на wildcard, отрицаниях, границе класса, namespace и
    # другой DLL, а затем отдельно ломает stale-фильтр и ленивый неподдержанный
    # хвост. Поэтому ускорение не может спрятать чужой класс или принять старый
    # разбор после изменения строки.
    $problems += Test-PreparedFilterAssignmentMap -Actual $noPreparedMap -Expected $expected -Label 'old_path'
    $problems += Test-PreparedFilterAssignmentMap -Actual $preparedMap -Expected $expected -Label 'prepared_path'
    foreach ($key in $expected.Keys) {
        if ($noPreparedMap[$key] -ne $preparedMap[$key]) {
            Write-Host "prepared_filter_parsing_self_check_prepared_differs_from_old_path: $key old $($noPreparedMap[$key]), prepared $($preparedMap[$key])"
            $problems++
        }
    }

    $lazyParsed = New-ParsedFilter -Filter 'FullyQualifiedName~Example.Lazy.|UnsupportedSyntax'
    $lazyOldAllowed = Test-BatchFilter -Filter 'FullyQualifiedName~Example.Lazy.|UnsupportedSyntax' -FullyQualifiedName 'Example.Lazy'
    $lazyPreparedAllowed = Test-ParsedBatchFilter -Filter 'FullyQualifiedName~Example.Lazy.|UnsupportedSyntax' -FullyQualifiedName 'Example.Lazy' -ParsedFilter $lazyParsed
    if (-not $lazyOldAllowed -or -not $lazyPreparedAllowed) {
        Write-Host "prepared_filter_parsing_self_check_lazy_supported_head_changed: old=$lazyOldAllowed prepared=$lazyPreparedAllowed"
        $problems++
    }

    $lazyOldThrows = Test-PreparedFilterThrows { Test-BatchFilter -Filter 'FullyQualifiedName~Example.Lazy.|UnsupportedSyntax' -FullyQualifiedName 'Example.Missing' }
    $lazyPreparedThrows = Test-PreparedFilterThrows { Test-ParsedBatchFilter -Filter 'FullyQualifiedName~Example.Lazy.|UnsupportedSyntax' -FullyQualifiedName 'Example.Missing' -ParsedFilter $lazyParsed }
    if (-not $lazyOldThrows -or -not $lazyPreparedThrows) {
        Write-Host "prepared_filter_parsing_self_check_lazy_unsupported_branch_was_not_rejected: old=$lazyOldThrows prepared=$lazyPreparedThrows"
        $problems++
    }

    $changedRows = @(
        [pscustomobject]@{ Order = '01'; BatchId = 'target-boundary'; Dll = $dll; Filter = 'FullyQualifiedName~Example.TargetLookalike.' },
        [pscustomobject]@{ Order = '02'; BatchId = 'broad-example-with-negative'; Dll = $dll; Filter = 'FullyQualifiedName~Example.&FullyQualifiedName!~Example.Blocked.' },
        [pscustomobject]@{ Order = '03'; BatchId = 'namespace-target'; Dll = $dll; Filter = 'FullyQualifiedName~OtherNamespace.Target.' },
        [pscustomobject]@{ Order = '04'; BatchId = 'other-dll-target'; Dll = $otherDll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '05'; BatchId = 'remainder'; Dll = $dll; Filter = '*' }
    )
    $changedMap = New-PreparedFilterAssignmentMapForSelfCheck -Rows $changedRows -Classes $classes -PreparedRows $preparedRows
    if ($changedMap["$dll|Example.Target"] -ne 'broad-example-with-negative') {
        Write-Host "prepared_filter_parsing_self_check_stale_filter_was_accepted: expected broad-example-with-negative, actual $($changedMap["$dll|Example.Target"])"
        $problems++
    }

    $caseChangedRows = @(
        [pscustomobject]@{ Order = '01'; BatchId = 'target-boundary'; Dll = $dll; Filter = 'fullyqualifiedname~Example.Target.' },
        [pscustomobject]@{ Order = '02'; BatchId = 'broad-example-with-negative'; Dll = $dll; Filter = 'FullyQualifiedName~Example.&FullyQualifiedName!~Example.Blocked.' },
        [pscustomobject]@{ Order = '03'; BatchId = 'namespace-target'; Dll = $dll; Filter = 'FullyQualifiedName~OtherNamespace.Target.' },
        [pscustomobject]@{ Order = '04'; BatchId = 'other-dll-target'; Dll = $otherDll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '05'; BatchId = 'remainder'; Dll = $dll; Filter = '*' }
    )
    $caseChangedThrows = Test-PreparedFilterThrows {
        Find-FirstBatch -TestClass ([pscustomobject]@{ Dll = $dll; ClassName = 'Target'; FullyQualifiedName = 'Example.Target' }) -Rows $caseChangedRows -PreparedRows $preparedRows
    }
    if (-not $caseChangedThrows) {
        Write-Host 'prepared_filter_parsing_self_check_operator_case_change_reused_stale_parse'
        $problems++
    }

    $broadRow = $preparedRows | Where-Object { $_.BatchId -eq 'broad-example-with-negative' } | Select-Object -First 1
    $preparedEffectiveAllowed = Test-EffectiveFilterForClass -Filter $broadRow.EffectiveFilter -FullyQualifiedName 'Example.TargetLookalike' -ParsedFilter $broadRow.ParsedEffectiveFilter
    $plainEffectiveAllowed = Test-EffectiveFilterForClass -Filter $broadRow.EffectiveFilter -FullyQualifiedName 'Example.TargetLookalike'
    $preparedEffectiveBlocked = Test-EffectiveFilterForClass -Filter $broadRow.EffectiveFilter -FullyQualifiedName 'Example.Blocked' -ParsedFilter $broadRow.ParsedEffectiveFilter
    $plainEffectiveBlocked = Test-EffectiveFilterForClass -Filter $broadRow.EffectiveFilter -FullyQualifiedName 'Example.Blocked'
    if ($preparedEffectiveAllowed -ne $plainEffectiveAllowed -or -not $preparedEffectiveAllowed -or $preparedEffectiveBlocked -ne $plainEffectiveBlocked -or $preparedEffectiveBlocked) {
        Write-Host "prepared_filter_parsing_self_check_effective_filter_semantics_changed: allowed=$preparedEffectiveAllowed/$plainEffectiveAllowed blocked=$preparedEffectiveBlocked/$plainEffectiveBlocked"
        $problems++
    }

    $staleParsedAccepted = Test-ParsedBatchFilter -Filter 'FullyQualifiedName~Example.TargetLookalike.' -FullyQualifiedName 'Example.Target' -ParsedFilter $preparedRows[0].ParsedFilter
    if ($staleParsedAccepted) {
        Write-Host 'prepared_filter_parsing_self_check_stale_parsed_filter_direct_call_was_accepted'
        $problems++
    }

    if ($problems -eq 0) {
        Write-Host 'Prepared filter parsing self-check: prepared and original filter paths agree; wildcard, negatives, exact class boundary, namespace, DLL, stale filters, operator-case changes and lazy unsupported branches are guarded.'
    }

    return $problems
}

function Get-ExactHumanBatchRowsIdentity {
    param([object[]]$Rows)

    $payload = ($Rows | ForEach-Object {
        "$($_.Order)`t$($_.BatchId)`t$($_.Dll)`t$($_.Filter)"
    }) -join "`n"
    return Get-TextSha256Lower -Text $payload
}

function Get-ExactHumanBatchOwnerId {
    param(
        [object]$TestClass,
        [object[]]$Rows,
        [hashtable]$AssignmentByClass,
        [bool]$UseAssignmentMap = $false
    )

    if ($UseAssignmentMap) {
        $key = "$($TestClass.Dll)|$($TestClass.FullyQualifiedName)"
        if ($AssignmentByClass.ContainsKey($key)) {
            return [string]$AssignmentByClass[$key]
        }
    }

    # Быстрая карта создаётся только в текущем canonical-вызове. Если ключа нет
    # или строки manifest изменились, verifier сознательно возвращается к
    # свежему first-match, чтобы missing/foreign class не спрятался за старой map.
    $batch = Find-FirstBatch -TestClass $TestClass -Rows $Rows
    if ($null -eq $batch) {
        return $null
    }

    return $batch.BatchId
}

function Test-IsLastWildcardBatchForDll {
    param(
        [object]$Row,
        [object[]]$Rows
    )

    if ($null -eq $Row -or $Row.Filter -ne '*') {
        return $false
    }

    $targetDll = Normalize-DllTextForCompare -Value $Row.Dll
    $sameDllRows = @($Rows | Where-Object { (Normalize-DllTextForCompare -Value $_.Dll) -eq $targetDll })
    if ($sameDllRows.Count -eq 0) {
        return $false
    }

    $last = $sameDllRows[$sameDllRows.Count - 1]
    return $last.BatchId -eq $Row.BatchId -and $last.Filter -eq '*'
}

function Test-IndivisibleGroups {
    param(
        [object[]]$Classes,
        [hashtable]$AssignmentByClass,
        [switch]$Quiet
    )

    $groups = @(
        @{ Name = 'design conversion local host'; BatchId = 'design-conversion-local-host'; Terms = @('AutonomousPromotionEvaluationComposition','DesignConversionLiveLocalBootstrapCheckpoint','DesignConversionLiveLocalServer','DesignConversionModelServerAspNetHost','DesignConversionModelServerArtifactUploadAspNetEndpointMapper','DesignConversionModelServerDpoUsefulness','DesignConversionModelServerTrainingIntake','DesignConversionModelServerExecutableHost','DesignConversionModelServerFlow38Quarantine','DesignConversionPromotion') },
        @{ Name = 'process spawning tests'; BatchId = 'process-spawning-tests'; Terms = @('DesignConversionBrowserQaProvider','DesignConversionBrowserRenderQaProbeIntegration','DesignConversionModelServerImprovementLoopBrowserIntegration','XiigenExtensionApiContracts','ExtensionQuality03ProofContractRunner','ExtensionTokenQualityRunnerHarness') },
        @{ Name = 'extension token quality artifacts'; BatchId = 'extension-token-quality'; Terms = @('ExtensionAdaptiveEffectGap02','ExtensionQualitySemanticGates','ExtensionToken04QualityScoringArtifacts','SkillSelectionPhase06TrainedSelectorEvidence','SmartSkillsSemanticQualityP17','SmartSkillsSemanticQualityP23','SmartSkillsSemanticQualityP24','SmartSkillsSemanticQualityP25') },
        @{ Name = 'continuous learning environment'; BatchId = 'model-server-continuous-learning'; Terms = @('ContinuousLearningBaseBootstrapTool','ContinuousLearningModelServerReadinessEndpoint','TranslationDpoModelServerCoordinator') },
        @{ Name = 'single serial groups'; BatchId = 'single-serial-groups'; Terms = @('DefaultLogicalCapabilityTrainer','DesignConversionTrainingPipeline','DynamicGraphCompositionLiveLocalServer','ProductHostCapabilityServing') }
    )

    $broken = 0
    foreach ($group in $groups) {
        $groupAssignments = New-Object System.Collections.Generic.HashSet[string]
        foreach ($testClass in $Classes) {
            if ($group.Terms | Where-Object { $testClass.ClassName -like "*$_*" }) {
                $key = "$($testClass.Dll)|$($testClass.FullyQualifiedName)"
                if ($AssignmentByClass.ContainsKey($key)) {
                    if ($group.BatchId -eq 'design-conversion-local-host' -and
                        $AssignmentByClass[$key] -eq $script:DesignConversionLiveLocalServerMethodDiagnosticBatchId -and
                        (Normalize-DllTextForCompare -Value $testClass.Dll) -eq (Normalize-DllTextForCompare -Value 'Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll') -and
                        $testClass.FullyQualifiedName -eq 'DesignConversionLiveLocalServerTests') {
                        # Исключение узкое: старый family-guard продолжает ловить
                        # lookalike и namespace-двойники, но реальный глобальный
                        # класс запускается по одному методу в новой строке.
                        continue
                    }

                    [void]$groupAssignments.Add($AssignmentByClass[$key])
                }
            }
        }

        if ($groupAssignments.Count -ne 1 -or -not $groupAssignments.Contains($group.BatchId)) {
            if (-not $Quiet) {
                Write-Host "Broken indivisible group: $($group.Name) -> $($groupAssignments -join ', ')"
            }
            $broken++
        }
    }

    return $broken
}

function Test-IndivisibleNamespaceCollisionSelfCheck {
    $problems = 0
    foreach ($dll in @('Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll', 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll')) {
        $classes = @(
            [pscustomobject]@{ Dll = $dll; ClassName = 'DesignConversionLiveLocalServerTests'; FullyQualifiedName = 'DesignConversionLiveLocalServerTests' },
            [pscustomobject]@{ Dll = $dll; ClassName = 'DesignConversionLiveLocalServerTests'; FullyQualifiedName = 'Synthetic.DesignConversionLiveLocalServerTests' },
            [pscustomobject]@{ Dll = $dll; ClassName = 'DesignConversionBrowserQaProviderTests'; FullyQualifiedName = 'Synthetic.DesignConversionBrowserQaProviderTests' },
            [pscustomobject]@{ Dll = $dll; ClassName = 'ExtensionAdaptiveEffectGap02Tests'; FullyQualifiedName = 'Synthetic.ExtensionAdaptiveEffectGap02Tests' },
            [pscustomobject]@{ Dll = $dll; ClassName = 'ContinuousLearningBaseBootstrapToolTests'; FullyQualifiedName = 'Synthetic.ContinuousLearningBaseBootstrapToolTests' },
            [pscustomobject]@{ Dll = $dll; ClassName = 'DefaultLogicalCapabilityTrainerTests'; FullyQualifiedName = 'Synthetic.DefaultLogicalCapabilityTrainerTests' }
        )

        $validAssignment = @{}
        $validAssignment["$dll|DesignConversionLiveLocalServerTests"] = $script:DesignConversionLiveLocalServerMethodDiagnosticBatchId
        $validAssignment["$dll|Synthetic.DesignConversionLiveLocalServerTests"] = 'design-conversion-local-host'
        $validAssignment["$dll|Synthetic.DesignConversionBrowserQaProviderTests"] = 'process-spawning-tests'
        $validAssignment["$dll|Synthetic.ExtensionAdaptiveEffectGap02Tests"] = 'extension-token-quality'
        $validAssignment["$dll|Synthetic.ContinuousLearningBaseBootstrapToolTests"] = 'model-server-continuous-learning'
        $validAssignment["$dll|Synthetic.DefaultLogicalCapabilityTrainerTests"] = 'single-serial-groups'
        if ((Test-IndivisibleGroups -Classes $classes -AssignmentByClass $validAssignment -Quiet) -ne 0) {
            Write-Host "indivisible_namespace_collision_self_check_valid_exception_failed dll=$dll"
            $problems++
        }

        $badAssignment = @{}
        $badAssignment["$dll|DesignConversionLiveLocalServerTests"] = $script:DesignConversionLiveLocalServerMethodDiagnosticBatchId
        $badAssignment["$dll|Synthetic.DesignConversionLiveLocalServerTests"] = 'core-m-z-and-remainder'
        $badAssignment["$dll|Synthetic.DesignConversionBrowserQaProviderTests"] = 'process-spawning-tests'
        $badAssignment["$dll|Synthetic.ExtensionAdaptiveEffectGap02Tests"] = 'extension-token-quality'
        $badAssignment["$dll|Synthetic.ContinuousLearningBaseBootstrapToolTests"] = 'model-server-continuous-learning'
        $badAssignment["$dll|Synthetic.DefaultLogicalCapabilityTrainerTests"] = 'single-serial-groups'
        if ((Test-IndivisibleGroups -Classes $classes -AssignmentByClass $badAssignment -Quiet) -eq 0) {
            # Одноимённый класс в другом namespace не получает исключение нового
            # маршрута: старый семейный guard обязан заметить его перенос.
            Write-Host "indivisible_namespace_collision_self_check_missed_moved_namespace_lookalike dll=$dll"
            $problems++
        }
    }

    return $problems
}

function Test-ExactHumanBatch {
    param(
        [object[]]$Rows,
        [object[]]$Classes,
        [string]$BatchId,
        [string[]]$ExpectedClassFqns,
        [string]$ProblemPrefix,
        [string]$HumanReason,
        [string]$ExpectedDll = '',
        [hashtable]$AssignmentByClass = $null
    )

    $problems = 0
    $namedRows = @($Rows | Where-Object { $_.BatchId -eq $BatchId })
    if ($namedRows.Count -ne 1) {
        Write-Host "$($ProblemPrefix)_batch_row_count: expected one row for $BatchId, found $($namedRows.Count). Reason: $HumanReason"
        $problems++
    }

    $expected = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($classFqn in $ExpectedClassFqns) {
        [void]$expected.Add($classFqn)
    }

    foreach ($row in $namedRows) {
        if (-not [string]::IsNullOrWhiteSpace($ExpectedDll) -and $row.Dll -ne $ExpectedDll) {
            Write-Host "$($ProblemPrefix)_wrong_dll: expected $ExpectedDll, actual $($row.Dll). Reason: $HumanReason"
            $problems++
        }

        $declared = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($term in (Get-PositiveFilterTerms -Filter $row.Filter)) {
            [void]$declared.Add((Convert-ClassBoundaryTermToClassFqn -Term $term))
        }

        foreach ($classFqn in $ExpectedClassFqns) {
            if (-not $declared.Contains($classFqn)) {
                Write-Host "$($ProblemPrefix)_filter_missing_exact_class: $classFqn must be named in $BatchId with a full class-level FullyQualifiedName~ term or class-boundary term."
                $problems++
            }
        }

        foreach ($term in $declared) {
            if (-not $expected.Contains($term)) {
                Write-Host "$($ProblemPrefix)_filter_unexpected_term: $term widens $BatchId beyond its named human group."
                $problems++
            }
        }
    }

    $useAssignmentMap = $false
    if ($null -ne $AssignmentByClass -and $AssignmentByClass.ContainsKey('__rows_identity')) {
        # Отпечаток строк нужен только один раз на guard-вызов: карта создана
        # текущим canonical foreach и не должна ускорять проверку старого manifest.
        $useAssignmentMap = $AssignmentByClass['__rows_identity'] -eq (Get-ExactHumanBatchRowsIdentity -Rows $Rows)
    }

    $actual = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($testClass in $Classes) {
        if (-not [string]::IsNullOrWhiteSpace($ExpectedDll) -and $testClass.Dll -ne $ExpectedDll) {
            continue
        }

        $ownerBatchId = Get-ExactHumanBatchOwnerId -TestClass $testClass -Rows $Rows -AssignmentByClass $AssignmentByClass -UseAssignmentMap $useAssignmentMap
        if ($null -eq $ownerBatchId) {
            continue
        }

        if ($expected.Contains($testClass.FullyQualifiedName) -and $ownerBatchId -ne $BatchId) {
            Write-Host "$($ProblemPrefix)_wrong_batch: $($testClass.FullyQualifiedName) expected $BatchId, actual $ownerBatchId. Reason: $HumanReason"
            $problems++
        }

        if ($ownerBatchId -eq $BatchId) {
            [void]$actual.Add($testClass.FullyQualifiedName)
        }
    }

    foreach ($classFqn in $ExpectedClassFqns) {
        if (-not $actual.Contains($classFqn)) {
            Write-Host "$($ProblemPrefix)_missing_class: $classFqn is not assigned to $BatchId. Reason: $HumanReason"
            $problems++
        }
    }

    foreach ($classFqn in $actual) {
        if (-not $expected.Contains($classFqn)) {
            # Маленькая диагностическая группа не должна превращаться в
            # запасной остаток: один лишний класс уже делает сигнал нечитаемым.
            Write-Host "$($ProblemPrefix)_unexpected_class: $classFqn was assigned to $BatchId. Reason: $HumanReason"
            $problems++
        }
    }

    return $problems
}

function New-ExactHumanBatchAssignmentMapForSelfCheck {
    param(
        [object[]]$Rows,
        [object[]]$Classes
    )

    $map = @{}
    $map['__rows_identity'] = Get-ExactHumanBatchRowsIdentity -Rows $Rows
    foreach ($testClass in $Classes) {
        $batch = Find-FirstBatch -TestClass $testClass -Rows $Rows
        if ($null -ne $batch) {
            $map["$($testClass.Dll)|$($testClass.FullyQualifiedName)"] = $batch.BatchId
        }
    }

    return $map
}

function Invoke-ExactHumanBatchAssignmentMapCase {
    param(
        [object[]]$Rows,
        [object[]]$Classes,
        [hashtable]$AssignmentByClass
    )

    $arguments = @{
        Rows = $Rows
        Classes = $Classes
        BatchId = 'target-batch'
        ExpectedClassFqns = @('Example.Target')
        ProblemPrefix = 'exact_map_self_check'
        HumanReason = 'synthetic exact human batch assignment map regression'
        ExpectedDll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
    }
    if ($null -ne $AssignmentByClass) {
        $arguments.AssignmentByClass = $AssignmentByClass
    }

    $value = Test-ExactHumanBatch @arguments 6>$null
    return [int]$value
}

function Test-ExactHumanBatchAssignmentMapSelfCheck {
    $dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
    $otherDll = 'Tests\Other\bin\Release\net9.0\Other.Tests.dll'
    $rows = @(
        [pscustomobject]@{ Order = '01'; BatchId = 'target-batch'; Dll = $dll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '02'; BatchId = 'other-batch'; Dll = $dll; Filter = 'FullyQualifiedName~Example.Other.' },
        [pscustomobject]@{ Order = '03'; BatchId = 'remainder'; Dll = $dll; Filter = '*' },
        [pscustomobject]@{ Order = '04'; BatchId = 'other-dll-target'; Dll = $otherDll; Filter = 'FullyQualifiedName~Example.Target.' }
    )
    $classes = @(
        [pscustomobject]@{ Dll = $dll; ClassName = 'Target'; FullyQualifiedName = 'Example.Target' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'Other'; FullyQualifiedName = 'Example.Other' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'TargetLookalike'; FullyQualifiedName = 'Example.TargetLookalike' },
        [pscustomobject]@{ Dll = $dll; ClassName = 'Target'; FullyQualifiedName = 'OtherNamespace.Target' },
        [pscustomobject]@{ Dll = $otherDll; ClassName = 'Target'; FullyQualifiedName = 'Example.Target' }
    )
    $validMap = New-ExactHumanBatchAssignmentMapForSelfCheck -Rows $rows -Classes $classes
    $problems = 0

    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes) -ne 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_no_map_positive_failed'
        $problems++
    }

    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $validMap) -ne 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_map_positive_failed'
        $problems++
    }

    $wrongRouteMap = $validMap.Clone()
    $wrongRouteMap["$dll|Example.Target"] = 'other-batch'
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $wrongRouteMap) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_wrong_route_was_accepted'
        $problems++
    }

    $foreignClassMap = $validMap.Clone()
    $foreignClassMap["$dll|Example.Other"] = 'target-batch'
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $foreignClassMap) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_foreign_class_was_accepted'
        $problems++
    }

    $lookalikeMap = $validMap.Clone()
    $lookalikeMap["$dll|Example.TargetLookalike"] = 'target-batch'
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $lookalikeMap) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_class_lookalike_was_accepted'
        $problems++
    }

    $namespaceLookalikeMap = $validMap.Clone()
    $namespaceLookalikeMap["$dll|OtherNamespace.Target"] = 'target-batch'
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $namespaceLookalikeMap) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_namespace_lookalike_was_accepted'
        $problems++
    }

    $otherDllMap = $validMap.Clone()
    $otherDllMap["$otherDll|Example.Target"] = 'target-batch'
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $otherDllMap) -ne 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_other_dll_leaked_into_expected_dll'
        $problems++
    }

    $missingKeyMap = $validMap.Clone()
    $missingKeyMap.Remove("$dll|Example.Target")
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $rows -Classes $classes -AssignmentByClass $missingKeyMap) -ne 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_missing_key_did_not_fallback_to_fresh_assignment'
        $problems++
    }

    $changedRows = @(
        [pscustomobject]@{ Order = '01'; BatchId = 'stealer-batch'; Dll = $dll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '02'; BatchId = 'target-batch'; Dll = $dll; Filter = 'FullyQualifiedName~Example.Target.' },
        [pscustomobject]@{ Order = '03'; BatchId = 'remainder'; Dll = $dll; Filter = '*' }
    )
    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $changedRows -Classes $classes -AssignmentByClass $validMap) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_stale_map_was_accepted_for_changed_rows'
        $problems++
    }

    if ((Invoke-ExactHumanBatchAssignmentMapCase -Rows $changedRows -Classes $classes) -eq 0) {
        Write-Host 'exact_human_batch_assignment_map_self_check_changed_rows_did_not_use_fresh_assignment'
        $problems++
    }

    if ($problems -eq 0) {
        Write-Host 'Exact human batch assignment map self-check: map/no-map agree, stale rows fall back to fresh assignment, wrong route, foreign class, class/namespace lookalikes and other DLL boundaries are guarded.'
    }

    return $problems
}

function Test-PairGraphBatchPriority {
    param([object[]]$Rows)

    $problems = 0
    foreach ($batchId in $script:PairGraphPriorityBatchIds) {
        $exactRows = @($Rows | Where-Object { $_.BatchId -eq $batchId })
        if ($exactRows.Count -ne 1) {
            Write-Host "pair_graph_priority_exact_row_count: $batchId expected one row, found $($exactRows.Count)."
            $problems++
            continue
        }

        foreach ($broadBatchId in $script:PairGraphBroadCollisionBatchIds) {
            $broadRows = @($Rows | Where-Object { $_.BatchId -eq $broadBatchId })
            if ($broadRows.Count -ne 1) {
                Write-Host "pair_graph_priority_broad_row_count: $broadBatchId expected one row, found $($broadRows.Count)."
                $problems++
                continue
            }

            # Запускалка проверяет группы по первому совпадению: точная группа
            # пары/графа должна стоять до широких графовых, обучающих и групп
            # оркестрации, иначе фильтр запуска может отрезать метод с
            # "Orchestration" в имени до проверки точного имени класса.
            if ($exactRows[0].LineNumber -ge $broadRows[0].LineNumber) {
                Write-Host "pair_graph_priority_after_broad_batch: $batchId must precede $broadBatchId; exact line $($exactRows[0].LineNumber), broad line $($broadRows[0].LineNumber)."
                $problems++
            }
        }
    }

    return $problems
}

function Read-JournalRows {
    param([string]$Path)

    $rows = New-Object System.Collections.Generic.List[object]
    if (-not (Test-Path -LiteralPath $Path)) {
        return $rows
    }

    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++
        if ($lineNumber -eq 1 -or [string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $parts = $line -split "`t", 16
        if ($parts.Count -lt 16) {
            $rows.Add([pscustomobject]@{ BatchId = ''; State = 'CORRUPT_JOURNAL_ROW'; LineNumber = $lineNumber })
            continue
        }

        $rows.Add([pscustomobject]@{
            Order = $parts[0]
            BatchId = $parts[1]
            HumanName = $parts[2]
            State = $parts[6]
            ExitCode = $parts[7]
            Total = [int]$parts[8]
            Passed = [int]$parts[9]
            Failed = [int]$parts[10]
            NotExecuted = [int]$parts[11]
            ResultPath = $parts[12]
            Platform = $parts[13]
            PlatformReason = $parts[14]
            SplitOnRepeatTimeout = $parts[15]
            LineNumber = $lineNumber
        })
    }

    return $rows
}

function Get-LatestJournalRowsByManifestOrder {
    param(
        [object[]]$JournalRows,
        [object[]]$Rows = @()
    )

    $latestById = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    $firstSeenIds = New-Object System.Collections.Generic.List[string]
    foreach ($journalRow in $JournalRows) {
        if ([string]::IsNullOrWhiteSpace($journalRow.BatchId) -or $journalRow.State -eq 'CORRUPT_JOURNAL_ROW') {
            continue
        }

        if (-not $latestById.ContainsKey($journalRow.BatchId)) {
            [void]$firstSeenIds.Add($journalRow.BatchId)
        }

        # journal.tsv append-only: resume добавляет события, а публичная
        # проекция состояния берёт последнюю запись конкретного batch_id.
        $latestById[$journalRow.BatchId] = $journalRow
    }

    $projected = New-Object System.Collections.Generic.List[object]
    if ($Rows.Count -gt 0) {
        foreach ($row in $Rows) {
            if ($latestById.ContainsKey($row.BatchId)) {
                [void]$projected.Add($latestById[$row.BatchId])
            }
        }
    }
    else {
        foreach ($batchId in $firstSeenIds) {
            [void]$projected.Add($latestById[$batchId])
        }
    }

    return [object[]]$projected.ToArray()
}

function Test-IsTerminalJournalState {
    param([string]$State)

    return $State -like 'COMPLETED_*' -or
        $State -eq 'EMPTY_VERIFIED' -or
        $State -eq 'SKIPPED_PLATFORM' -or
        $State -eq 'INVALID_RESULT' -or
        $State -eq 'INVALID_TOO_FEW_ROWS' -or
        $State -eq 'INVALID_NO_TRX' -or
        $State -eq 'TIMED_OUT_BATCH' -or
        $State -eq 'RETRY_REQUIRED_TIMEOUT' -or
        $State -eq 'SPLIT_REQUIRED'
}

function Get-JournalTerminalSignature {
    param([object]$Row)

    return @(
        $Row.State,
        $Row.ExitCode,
        $Row.Total,
        $Row.Passed,
        $Row.Failed,
        $Row.NotExecuted,
        $Row.ResultPath,
        $Row.Platform,
        $Row.PlatformReason,
        $Row.SplitOnRepeatTimeout
    ) -join "`t"
}

function Test-JournalTerminalDuplicateConflicts {
    param([object[]]$JournalRows)

    $problems = 0
    foreach ($group in @($JournalRows |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_.BatchId) -and $_.State -ne 'CORRUPT_JOURNAL_ROW' } |
            Group-Object BatchId)) {
        $terminalRows = @($group.Group | Where-Object { Test-IsTerminalJournalState -State $_.State })
        if ($terminalRows.Count -le 1) {
            continue
        }

        $signatures = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($terminalRow in $terminalRows) {
            [void]$signatures.Add((Get-JournalTerminalSignature -Row $terminalRow))
        }

        if ($signatures.Count -gt 1) {
            # Повторный LEAF_INCOMPLETE перед terminal строкой нормален для
            # resume, но две разные terminal-версии одного batch уже конфликтуют
            # как evidence и не должны молча исчезать в latest projection.
            Write-Host "Journal has conflicting terminal rows for batch: $($group.Name)"
            $problems++
        }
    }

    return $problems
}

function Test-IsExactUtcSecondTimestamp {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value) -or $Value -notmatch '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$') {
        return $false
    }

    $parsed = [datetime]::MinValue
    return [datetime]::TryParseExact(
        $Value,
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AssumeUniversal,
        [ref]$parsed)
}

function Test-IsNonNegativeIntegerText {
    param([string]$Value)

    return (-not [string]::IsNullOrWhiteSpace($Value) -and $Value -match '^[0-9]+$')
}

function Test-LeafJournalTimingRow {
    param([object]$Row)

    $requiredFields = @(
        'LeafId',
        'BatchId',
        'StartedUtc',
        'FinishedUtc',
        'Seconds',
        'State',
        'ExitCode',
        'Total',
        'Passed',
        'Failed',
        'NotExecuted',
        'MethodCount',
        'EstimatedRows',
        'Filter'
    )

    foreach ($field in $requiredFields) {
        $property = $Row.PSObject.Properties[$field]
        if ($null -eq $property -or [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            return [pscustomobject]@{ Valid = $false; Reason = "missing_$field" }
        }
    }

    # Leaf timing — часть публикуемого evidence, поэтому verifier проверяет
    # точный UTC second format runner'а и не даёт строковому мусору стать нулём.
    if (-not (Test-IsExactUtcSecondTimestamp -Value $Row.StartedUtc)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'started_utc_bad_utc' }
    }
    if (-not (Test-IsExactUtcSecondTimestamp -Value $Row.FinishedUtc)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'finished_utc_bad_utc' }
    }

    $started = [datetime]::ParseExact(
        $Row.StartedUtc,
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AssumeUniversal)
    $finished = [datetime]::ParseExact(
        $Row.FinishedUtc,
        "yyyy-MM-dd'T'HH:mm:ss'Z'",
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AssumeUniversal)
    if ($finished -lt $started) {
        return [pscustomobject]@{ Valid = $false; Reason = 'finished_before_started' }
    }

    if (-not (Test-IsNonNegativeIntegerText -Value ([string]$Row.Seconds))) {
        return [pscustomobject]@{ Valid = $false; Reason = 'seconds_not_nonnegative_integer' }
    }

    return [pscustomobject]@{ Valid = $true; Reason = '' }
}

function Get-BatchIdFromDirectory {
    param([string]$Path)

    $name = Split-Path -Leaf $Path
    if ($name -match '^batch-\d+-(.+)$') {
        return $Matches[1]
    }

    return $name
}

function Get-BatchMinRowsEvidence {
    param(
        [object[]]$Rows,
        [string]$BatchId
    )

    $matches = @($Rows | Where-Object { $_.BatchId -eq $BatchId })
    if ($matches.Count -ne 1) {
        return [pscustomobject]@{ Known = $false; Value = 0; Count = $matches.Count }
    }

    return [pscustomobject]@{ Known = $true; Value = [int]$matches[0].MinRows; Count = 1 }
}

function Read-StateValue {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return ''
    }

    return (Get-Content -LiteralPath $Path -First 1).Trim()
}

function Get-TrxMethodDefinitions {
    param([xml]$Xml)

    $definitions = @{}
    foreach ($unit in @($Xml.GetElementsByTagName('UnitTest'))) {
        $testId = $unit.GetAttribute('id')
        if ([string]::IsNullOrWhiteSpace($testId) -or $definitions.ContainsKey($testId)) {
            continue
        }

        $methodNode = @($unit.GetElementsByTagName('TestMethod')) | Select-Object -First 1
        if ($null -eq $methodNode) {
            continue
        }

        $className = ($methodNode.GetAttribute('className') -split ',', 2)[0].Trim()
        $methodName = ($methodNode.GetAttribute('name') -split '\(', 2)[0].Trim()
        if ([string]::IsNullOrWhiteSpace($className) -or [string]::IsNullOrWhiteSpace($methodName)) {
            continue
        }

        if ($methodName.StartsWith($className + '.', [StringComparison]::Ordinal)) {
            $definitions[$testId] = $methodName
        }
        else {
            $definitions[$testId] = "$className.$methodName"
        }
    }

    return $definitions
}

function Get-TrxResultMethodFqn {
    param(
        [object]$ResultNode,
        [hashtable]$Definitions
    )

    $testName = ($ResultNode.GetAttribute('testName') -split '\(', 2)[0].Trim()
    if (-not [string]::IsNullOrWhiteSpace($testName) -and $testName.Contains('.')) {
        return $testName
    }

    $testId = $ResultNode.GetAttribute('testId')
    if (-not [string]::IsNullOrWhiteSpace($testId) -and $Definitions.ContainsKey($testId)) {
        return $Definitions[$testId]
    }

    return ''
}

function Get-TrxResultMessage {
    param([object]$ResultNode)

    $messages = @($ResultNode.GetElementsByTagName('Message'))
    if ($messages.Count -eq 0) {
        return ''
    }

    return [string]$messages[0].InnerText
}

function Test-LegacyQwenRuntimeSkipResult {
    param(
        [object]$ResultNode,
        [hashtable]$Definitions
    )

    if ($ResultNode.GetAttribute('outcome') -ne 'Failed') {
        return $false
    }

    $methodFqn = Get-TrxResultMethodFqn -ResultNode $ResultNode -Definitions $Definitions
    if ($methodFqn -ne $script:LegacyQwenCanaryFqn) {
        return $false
    }

    $message = Get-TrxResultMessage -ResultNode $ResultNode
    # Историческая нормализация зависит от сохранённого evidence, а не от
    # сегодняшней env verifier-процесса. Если старый TRX содержит точный FQN и
    # точный WINDOWS_ONLY_RUNTIME prefix, он должен проецироваться одинаково при
    # env unset и env=1. Свежая явная Windows-приёмка управляется canary
    # FactAttribute/body и обязана дать реальный pass/fail TRX, а не этот prefix.
    return $message.StartsWith($script:LegacyQwenDynamicSkipPrefix, [StringComparison]::Ordinal)
}

function Read-TrxCounts {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'missing'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = 0 }
    }

    $item = Get-Item -LiteralPath $Path
    if ($item.Length -lt 128) {
        return [pscustomobject]@{ Valid = $false; Reason = 'too_short'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    $text = Get-Content -LiteralPath $Path -Raw
    try {
        [xml]$xml = $text
    }
    catch {
        return [pscustomobject]@{ Valid = $false; Reason = 'corrupt_xml'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    # Результаты читаются как XML-узлы, а не регулярным выражением. Это не дает
    # тексту внутри лога притвориться настоящей строкой результата.
    $resultNodes = @($xml.GetElementsByTagName('UnitTestResult'))
    if ($resultNodes.Count -eq 0) {
        return [pscustomobject]@{ Valid = $false; Reason = 'no_unit_test_results'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    $definitions = Get-TrxMethodDefinitions -Xml $xml
    $outcomeCounts = @{ Passed = 0; Failed = 0; NotExecuted = 0 }
    $rawOutcomeCounts = @{ Passed = 0; Failed = 0; NotExecuted = 0 }
    $legacyNormalizedDynamicSkips = 0
    foreach ($resultNode in $resultNodes) {
        $rawOutcome = $resultNode.GetAttribute('outcome')
        if ([string]::IsNullOrWhiteSpace($rawOutcome)) {
            return [pscustomobject]@{ Valid = $false; Reason = 'unit_test_result_without_outcome'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }

        # Неизвестный outcome нельзя отнести к passed, failed или NotExecuted
        # без потери смысла, поэтому новая семантика закрывает результат.
        if (-not $outcomeCounts.ContainsKey($rawOutcome)) {
            return [pscustomobject]@{ Valid = $false; Reason = "unsupported_outcome:$rawOutcome"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }

        $rawOutcomeCounts[$rawOutcome]++
        $outcome = $rawOutcome
        if (Test-LegacyQwenRuntimeSkipResult -ResultNode $resultNode -Definitions $definitions) {
            # Узкая legacy-нормализация нужна только для старых TRX, где xUnit
            # runtime SkipException был записан VSTest как Failed. Точный FQN и
            # точный префикс защищают реальные падения Qwen от маскировки.
            $outcome = 'NotExecuted'
            $legacyNormalizedDynamicSkips++
        }

        $outcomeCounts[$outcome]++
    }

    $passed = $outcomeCounts['Passed']
    $failed = $outcomeCounts['Failed']
    $notExecuted = $outcomeCounts['NotExecuted']
    $rawPassed = $rawOutcomeCounts['Passed']
    $rawFailed = $rawOutcomeCounts['Failed']
    $rawNotExecuted = $rawOutcomeCounts['NotExecuted']

    $counters = $xml.GetElementsByTagName('Counters') | Select-Object -First 1
    if ($null -eq $counters) {
        return [pscustomobject]@{ Valid = $false; Reason = 'missing_counters'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    $parsedCounters = @{}
    foreach ($name in @('total', 'executed', 'passed', 'failed', 'notExecuted')) {
        $raw = $counters.GetAttribute($name)
        $parsed = 0
        if ([string]::IsNullOrWhiteSpace($raw) -or -not [int]::TryParse($raw, [ref]$parsed)) {
            return [pscustomobject]@{ Valid = $false; Reason = "non_numeric_counter_$name"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
        if ($parsed -lt 0) {
            return [pscustomobject]@{ Valid = $false; Reason = "negative_counter_$name"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
        $parsedCounters[$name] = $parsed
    }

    # Подробные семейства VSTest допустимы только как числовые нули. Если одно
    # из них ненулевое, физические строки содержат неподдерживаемый исход и
    # итоговые passed/failed числа уже нельзя считать полными.
    $extraCounterNames = @('error', 'timeout', 'aborted', 'inconclusive', 'passedButRunAborted', 'notRunnable', 'disconnected', 'warning', 'completed', 'inProgress', 'pending')
    $allowedCounterNames = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($name in (@('total', 'executed', 'passed', 'failed', 'notExecuted') + $extraCounterNames)) {
        [void]$allowedCounterNames.Add($name)
    }

    foreach ($attribute in @($counters.Attributes)) {
        if ($attribute.Prefix -eq 'xmlns' -or $attribute.Name -eq 'xmlns') {
            continue
        }

        if (-not $allowedCounterNames.Contains($attribute.LocalName)) {
            return [pscustomobject]@{ Valid = $false; Reason = "unsupported_counter_attribute:$($attribute.LocalName)"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
    }

    foreach ($name in $extraCounterNames) {
        if (-not $counters.HasAttribute($name)) {
            continue
        }

        $parsed = 0
        if (-not [int]::TryParse($counters.GetAttribute($name), [ref]$parsed)) {
            return [pscustomobject]@{ Valid = $false; Reason = "non_numeric_counter_$name"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
        if ($parsed -ne 0) {
            return [pscustomobject]@{ Valid = $false; Reason = "unsupported_counter_nonzero:$name"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
    }

    if ($legacyNormalizedDynamicSkips -eq 0) {
        if ($parsedCounters['total'] -ne $resultNodes.Count -or $parsedCounters['passed'] -ne $passed -or $parsedCounters['failed'] -ne $failed) {
            return [pscustomobject]@{ Valid = $false; Reason = 'counter_outcome_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
    }
    else {
        if ($parsedCounters['total'] -ne $resultNodes.Count -or $parsedCounters['passed'] -ne $rawPassed -or $parsedCounters['failed'] -ne $rawFailed) {
            return [pscustomobject]@{ Valid = $false; Reason = 'legacy_qwen_dynamic_skip_counter_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
        }
    }

    # executed обязан в точности состоять из физических Passed и Failed. Раньше
    # диапазонная проверка пропускала правдоподобное, но ложное число.
    $expectedExecuted = if ($legacyNormalizedDynamicSkips -eq 0) { $passed + $failed } else { $rawPassed + $rawFailed }
    if ($parsedCounters['executed'] -ne $expectedExecuted) {
        return [pscustomobject]@{ Valid = $false; Reason = 'counter_executed_outcome_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    # Физические NotExecuted восстанавливают точную вторую часть total. В
    # честных TRX от VSTest одноименный служебный атрибут бывает нулем, поэтому
    # разрешены только ноль или точное физическое число, но не произвольное.
    $counterNotExecutedPartition = if ($legacyNormalizedDynamicSkips -eq 0) { $notExecuted } else { $rawNotExecuted }
    if ($parsedCounters['total'] -ne ($parsedCounters['executed'] + $counterNotExecutedPartition)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'counter_total_partition_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }
    $allowedNotExecutedCounter = if ($legacyNormalizedDynamicSkips -eq 0) { $notExecuted } else { $rawNotExecuted }
    if ($parsedCounters['notExecuted'] -notin @(0, $allowedNotExecutedCounter)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'counter_not_executed_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; Length = $item.Length }
    }

    return [pscustomobject]@{
        Valid = $true
        Reason = ''
        Total = $resultNodes.Count
        Passed = $passed
        Failed = $failed
        NotExecuted = $notExecuted
        # Эти raw-счётчики используются только для одной legacy-сверки старых
        # RunRoot: до discovery-time skip Qwen canary писал runtime SkipException
        # в TRX и leaf-journal как Failed. Нормализовать разрешено только когда
        # Read-TrxCounts уже доказал точный FQN и точный служебный префикс.
        RawPassed = $rawPassed
        RawFailed = $rawFailed
        RawNotExecuted = $rawNotExecuted
        LegacyNormalizedDynamicSkips = $legacyNormalizedDynamicSkips
        Length = $item.Length
    }
}

function Read-TrxResultMethodCounts {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'missing'; Methods = @{} }
    }

    try {
        [xml]$xml = Get-Content -LiteralPath $Path -Raw
    }
    catch {
        return [pscustomobject]@{ Valid = $false; Reason = 'corrupt_xml'; Methods = @{} }
    }

    $definitions = Get-TrxMethodDefinitions -Xml $xml
    $methods = @{}
    foreach ($result in @($xml.GetElementsByTagName('UnitTestResult'))) {
        $method = Get-TrxResultMethodFqn -ResultNode $result -Definitions $definitions
        if ([string]::IsNullOrWhiteSpace($method)) {
            return [pscustomobject]@{ Valid = $false; Reason = 'unit_test_result_without_method_identity'; Methods = @{} }
        }

        if (-not $methods.ContainsKey($method)) {
            $methods[$method] = 0
        }
        $methods[$method]++
    }

    if ($methods.Count -eq 0) {
        return [pscustomobject]@{ Valid = $false; Reason = 'no_unit_test_results'; Methods = @{} }
    }

    return [pscustomobject]@{ Valid = $true; Reason = ''; Methods = $methods }
}

function Test-BatchOnlyClassFirstTrxEvidence {
    param(
        [string]$BatchDir,
        [string]$TrxPath,
        [object[]]$Rows = @(),
        [switch]$RequireEvidence
    )

    $problems = 0
    $expectedPath = Join-Path $BatchDir 'class-first-methods.tsv'
    if (-not (Test-Path -LiteralPath $expectedPath)) {
        if ($RequireEvidence) {
            Write-Host "Canonical batch-only evidence has no class-first method list: $BatchDir"
            return 1
        }

        return 0
    }

    try {
        $expectedRows = @(Read-ClassFirstMethodRows -Path $expectedPath)
    }
    catch {
        Write-Host "Class-first method list cannot be parsed for batch-only evidence: $BatchDir ($($_.Exception.Message))"
        return 1
    }

    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $expected = @{}
    foreach ($row in $expectedRows) {
        if ($row.BatchId -ne $batchId) {
            Write-Host "Class-first batch-only row belongs to another batch: $($row.MethodFqn) -> $($row.BatchId)"
            $problems++
        }
        if ($expected.ContainsKey($row.MethodFqn)) {
            Write-Host "Class-first batch-only list duplicates method: $($row.MethodFqn)"
            $problems++
        }
        else {
            $expected[$row.MethodFqn] = [int]$row.EstimatedRows
        }
    }

    if ($Rows.Count -gt 0) {
        $rawExpected = Get-ClassFirstExpectedRowsFromRawDiscovery -BatchDir $BatchDir -Rows $Rows -RequireEvidence:$RequireEvidence
        if (-not $rawExpected.Valid) {
            Write-Host "Class-first raw ListTests evidence cannot be used for batch-only evidence: $BatchDir ($($rawExpected.Reason))"
            $problems++
        }
        else {
            $fromRaw = @{}
            foreach ($row in $rawExpected.Rows) {
                $fromRaw[$row.MethodFqn] = $row
            }
            foreach ($method in $fromRaw.Keys) {
                if (-not $expected.ContainsKey($method)) {
                    Write-Host "Class-first batch-only list omitted raw-discovered method: $method"
                    $problems++
                }
                elseif ($expected[$method] -ne $fromRaw[$method].EstimatedRows) {
                    Write-Host "Class-first batch-only list row count differs from raw ListTests: $method"
                    $problems++
                }
            }
            foreach ($method in $expected.Keys) {
                if (-not $fromRaw.ContainsKey($method)) {
                    Write-Host "Class-first batch-only list adds method outside raw ListTests owner: $method"
                    $problems++
                }
            }
        }
    }

    $actual = Read-TrxResultMethodCounts -Path $TrxPath
    if (-not $actual.Valid) {
        Write-Host "Cannot read methods from batch-only TRX: $BatchDir ($($actual.Reason))"
        return ($problems + 1)
    }

    foreach ($method in $actual.Methods.Keys) {
        if (-not $expected.ContainsKey($method)) {
            Write-Host "Batch-only TRX contains method outside class-first owner: $method"
            $problems++
        }
    }

    foreach ($method in $expected.Keys) {
        $actualRows = if ($actual.Methods.ContainsKey($method)) { $actual.Methods[$method] } else { 0 }
        # Batch-only режим запускает весь batch одним VSTest вызовом, но
        # полнота всё равно сверяется по физическим строкам TRX, а не по фильтру.
        if ($actualRows -lt $expected[$method]) {
            Write-Host "Batch-only TRX is missing class-first method rows: $method ($actualRows < $($expected[$method]))"
            $problems++
        }
    }

    return $problems
}

function Get-CurrentAttemptDirectory {
    param([string]$BatchDir)

    $marker = Join-Path $BatchDir 'current-attempt.txt'
    if (-not (Test-Path -LiteralPath $marker)) {
        return ''
    }

    $attemptName = (Get-Content -LiteralPath $marker -First 1).Trim()
    if ($attemptName -notmatch '^attempt-\d{4}$') {
        return ''
    }

    $attemptDir = Join-Path $BatchDir $attemptName
    if (-not (Test-Path -LiteralPath $attemptDir)) {
        return ''
    }

    return $attemptDir
}

function Get-LastJournalRow {
    param(
        [object[]]$JournalRows,
        [string]$BatchId
    )

    $matches = @($JournalRows | Where-Object { $_.BatchId -eq $BatchId })
    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[$matches.Count - 1]
}

function Read-SplitPlanRows {
    param([string]$Path)

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -lt 2 -or $lines[0] -ne "child_id`tfilter`tassignment_reason") {
        throw 'split-plan.tsv has no exact three-column header and data rows.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        # Фильтр не должен содержать табы: ровно три колонки сохраняют план
        # читаемым человеком и исключают скрытые хвосты после причины.
        $parts = $lines[$index] -split "`t", 3
        if ($parts.Count -ne 3 -or [string]::IsNullOrWhiteSpace($parts[0]) -or [string]::IsNullOrWhiteSpace($parts[1]) -or [string]::IsNullOrWhiteSpace($parts[2])) {
            throw "split-plan.tsv line $($index + 1) is incomplete."
        }

        $rows.Add([pscustomobject]@{
            ChildId = $parts[0]
            Filter = $parts[1]
            AssignmentReason = $parts[2]
        })
    }

    if ($rows.Count -eq 0) {
        throw 'split-plan.tsv has no child rows.'
    }

    return $rows
}

function Get-TestClassIdentity {
    param([object]$TestClass)

    # Путь участвует в ключе: одинаковое имя класса в двух исходниках не должно
    # схлопнуться и скрыть пропавшее либо лишнее назначение.
    return "$($TestClass.Dll)|$($TestClass.RelativePath)|$($TestClass.ClassName)"
}

function Test-SplitPlanEvidence {
    param(
        [string]$BatchDir,
        [string]$BatchId,
        [object[]]$Rows,
        [object[]]$EffectiveRows,
        [object[]]$Classes
    )

    $planPath = Join-Path $BatchDir 'split-plan.tsv'
    if (-not (Test-Path -LiteralPath $planPath)) {
        Write-Host "Split-required batch has no split plan: $BatchDir"
        return 1
    }

    try {
        $planRows = @(Read-SplitPlanRows -Path $planPath)
    }
    catch {
        Write-Host "Split plan cannot be parsed for ${BatchId}: $($_.Exception.Message)"
        return 1
    }

    $sourceRow = @($Rows | Where-Object { $_.BatchId -eq $BatchId })
    $effectiveRow = @($EffectiveRows | Where-Object { $_.BatchId -eq $BatchId })
    if ($sourceRow.Count -ne 1 -or $effectiveRow.Count -ne 1) {
        Write-Host "Split plan has no unique source ladder row: $BatchId"
        return 1
    }

    $manualRows = @($planRows | Where-Object { $_.ChildId -eq 'manual' })
    if ($manualRows.Count -gt 0) {
        # Ручной план честен только как единственная строка: он не притворяется
        # автоматически доказанными непересекающимися дочерними корзинами.
        $manualProblems = 0
        if ($manualRows.Count -ne 1 -or $planRows.Count -ne 1) {
            Write-Host "Manual split plan must contain exactly one manual row: $BatchId"
            $manualProblems++
        }
        Write-Host "Split plan ${BatchId}: mode=manual; automatic child class counts are not claimed; validation_problems=$manualProblems"
        return $manualProblems
    }

    $childA = @($planRows | Where-Object { $_.ChildId -eq 'A' })
    $childB = @($planRows | Where-Object { $_.ChildId -eq 'B' })
    if ($planRows.Count -ne 2 -or $childA.Count -ne 1 -or $childB.Count -ne 1) {
        Write-Host "Automatic split plan must contain exactly child A and child B: $BatchId"
        return 1
    }

    $problems = 0
    $positiveTermsA = @(Get-PositiveFilterTerms -Filter $childA[0].Filter)
    $positiveTermsB = @(Get-PositiveFilterTerms -Filter $childB[0].Filter)
    if ($positiveTermsA.Count -eq 0 -or $positiveTermsB.Count -eq 0 -or $childA[0].Filter -eq '*' -or $childB[0].Filter -eq '*') {
        Write-Host "Automatic split child has no bounded positive filter: $BatchId"
        $problems++
    }

    # Сначала перечисляем текущие классы исходного effective-фильтра, затем
    # независимо перечисляем оба дочерних фильтра. Термины и классы выводятся
    # разными счетчиками: четыре термина не означают четыре класса заранее.
    $assemblyClasses = @($Classes | Where-Object { $_.Dll -eq $sourceRow[0].Dll })
    $originalSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $childASet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $childBSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

    foreach ($testClass in $assemblyClasses) {
        $identity = Get-TestClassIdentity -TestClass $testClass
        if (Test-EffectiveFilterForClass -Filter $effectiveRow[0].EffectiveFilter -FullyQualifiedName $testClass.FullyQualifiedName) {
            [void]$originalSet.Add($identity)
        }
        if (Test-EffectiveFilterForClass -Filter $childA[0].Filter -FullyQualifiedName $testClass.FullyQualifiedName) {
            [void]$childASet.Add($identity)
        }
        if (Test-EffectiveFilterForClass -Filter $childB[0].Filter -FullyQualifiedName $testClass.FullyQualifiedName) {
            [void]$childBSet.Add($identity)
        }
    }

    $intersection = @($childASet | Where-Object { $childBSet.Contains($_) })
    $childUnion = [System.Collections.Generic.HashSet[string]]::new($childASet, [StringComparer]::OrdinalIgnoreCase)
    $childUnion.UnionWith($childBSet)
    $missing = @($originalSet | Where-Object { -not $childUnion.Contains($_) })
    $extra = @($childUnion | Where-Object { -not $originalSet.Contains($_) })

    Write-Host ("Split plan {0}: positive_terms_A={1}; positive_terms_B={2}; original_classes={3}; child_A_classes={4}; child_B_classes={5}; intersection={6}; missing={7}; extra={8}" -f $BatchId, $positiveTermsA.Count, $positiveTermsB.Count, $originalSet.Count, $childASet.Count, $childBSet.Count, $intersection.Count, $missing.Count, $extra.Count)

    if ($intersection.Count -gt 0) {
        Write-Host "Split plan child filters overlap for ${BatchId}: $($intersection -join ', ')"
        $problems += $intersection.Count
    }
    if ($missing.Count -gt 0) {
        Write-Host "Split plan loses source classes for ${BatchId}: $($missing -join ', ')"
        $problems += $missing.Count
    }
    if ($extra.Count -gt 0) {
        Write-Host "Split plan adds classes outside the source batch for ${BatchId}: $($extra -join ', ')"
        $problems += $extra.Count
    }

    return $problems
}

function Read-LeafPlanRows {
    param([string]$Path)

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -lt 2 -or $lines[0] -ne "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter") {
        throw 'leaf-plan.tsv has no exact six-column header and data rows.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t", 6
        if ($parts.Count -ne 6) {
            throw "leaf-plan.tsv line $($index + 1) is not six columns."
        }

        $rows.Add([pscustomobject]@{
            LeafId = $parts[0]
            BatchId = $parts[1]
            LeafOrder = $parts[2]
            MethodCount = [int]$parts[3]
            EstimatedRows = [int]$parts[4]
            Filter = $parts[5]
        })
    }

    return $rows
}

function Read-LeafJournalRows {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return @()
    }

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -eq 0) {
        return @()
    }
    if ($lines[0] -ne "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter") {
        throw 'leaf-journal.tsv has no exact fifteen-column header.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t", 15
        if ($parts.Count -ne 15) {
            $rows.Add([pscustomobject]@{ LeafId = ''; BatchId = ''; StartedUtc = ''; FinishedUtc = ''; Seconds = ''; State = 'CORRUPT_LEAF_JOURNAL_ROW'; ExitCode = ''; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0; ResultPath = ''; MethodCount = 0; EstimatedRows = 0; Filter = ''; LineNumber = $index + 1 })
            continue
        }

        $rows.Add([pscustomobject]@{
            LeafId = $parts[0]
            BatchId = $parts[1]
            StartedUtc = $parts[2]
            FinishedUtc = $parts[3]
            Seconds = $parts[4]
            State = $parts[5]
            ExitCode = $parts[6]
            Total = [int]$parts[7]
            Passed = [int]$parts[8]
            Failed = [int]$parts[9]
            NotExecuted = [int]$parts[10]
            ResultPath = $parts[11]
            MethodCount = [int]$parts[12]
            EstimatedRows = [int]$parts[13]
            Filter = $parts[14]
            LineNumber = $index + 1
        })
    }

    return $rows
}

function Read-LeafMethodRows {
    param([string]$Path)

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -lt 2) {
        throw 'leaf-methods.tsv has no data rows.'
    }

    $header = $lines[0]
    $legacy = "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows"
    $current = "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key"
    $collectionAware = "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`tcollection_name`tcollection_kind"
    if ($header -notin @($legacy, $current, $collectionAware)) {
        throw 'leaf-methods.tsv has an unexpected header.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t"
        if (($header -eq $legacy -and $parts.Count -ne 5) -or ($header -eq $current -and $parts.Count -ne 7) -or ($header -eq $collectionAware -and $parts.Count -ne 9)) {
            throw "leaf-methods.tsv line $($index + 1) has unexpected columns."
        }

        $classFqn = if ($header -ne $legacy) { $parts[5] } else { ($parts[0] -replace '\.[^.]+$', '') }
        $unitKey = if ($header -ne $legacy) { $parts[6] } else { "class:$classFqn" }
        $collectionName = if ($header -eq $collectionAware) { $parts[7] } else { '' }
        $collectionKind = if ($header -eq $collectionAware) { $parts[8] } else { if ($unitKey.StartsWith('collection:', [StringComparison]::Ordinal)) { 'shared_fixture' } else { 'none' } }
        $rows.Add([pscustomobject]@{
            MethodFqn = $parts[0]
            BatchId = $parts[1]
            LeafId = $parts[2]
            MethodOrder = $parts[3]
            EstimatedRows = [int]$parts[4]
            ClassFqn = $classFqn
            UnitKey = $unitKey
            CollectionName = $collectionName
            CollectionKind = $collectionKind
        })
    }

    return $rows
}

function Read-LeafSourceExactBindingRows {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw 'leaf-source-exact-bindings.tsv is missing.'
    }

    $lines = @(Get-Content -LiteralPath $Path)
    $expectedHeader = "class_fqn`tsource_paths`tunit_key`tcollection_name`tcollection_kind`tdeclaration_count`tbinding_kind"
    if ($lines.Count -lt 2 -or $lines[0] -ne $expectedHeader) {
        throw 'leaf-source-exact-bindings.tsv has no exact seven-column header and data rows.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    $seen = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t", 7
        if ($parts.Count -ne 7) {
            throw "leaf-source-exact-bindings.tsv line $($index + 1) is not seven columns."
        }
        if (-not $seen.Add($parts[0])) {
            throw "leaf-source-exact-bindings.tsv duplicates class binding: $($parts[0])"
        }

        $declarationCount = 0
        if (-not [int]::TryParse($parts[5], [ref]$declarationCount)) {
            throw "leaf-source-exact-bindings.tsv line $($index + 1) has non-integer declaration_count."
        }

        $sourcePaths = @($parts[1] -split ',' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        $rows.Add([pscustomobject]@{
            ClassFqn = $parts[0]
            SourcePaths = $sourcePaths
            UnitKey = $parts[2]
            CollectionName = $parts[3]
            CollectionKind = $parts[4]
            DeclarationCount = $declarationCount
            BindingKind = $parts[6]
        })
    }

    return $rows
}

function Read-LeafOversizedUnitRows {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return @()
    }

    $lines = @(Get-Content -LiteralPath $Path)
    $expectedHeader = "unit_key`testimated_rows`tmethod_count`tunit_kind`treason"
    if ($lines.Count -lt 1 -or $lines[0] -ne $expectedHeader) {
        throw 'leaf-oversized-units.tsv has no exact five-column header.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t", 5
        if ($parts.Count -ne 5) {
            throw "leaf-oversized-units.tsv line $($index + 1) is not five columns."
        }

        $rows.Add([pscustomobject]@{
            UnitKey = $parts[0]
            EstimatedRows = [int]$parts[1]
            MethodCount = [int]$parts[2]
            UnitKind = $parts[3]
            Reason = $parts[4]
        })
    }

    return $rows
}

function Test-DesignConversionDpoStorageExactSourceBinding {
    param(
        [object]$Binding,
        [string]$RepositoryRoot
    )

    $problems = 0
    $rootFull = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $rootBoundary = $rootFull + [System.IO.Path]::DirectorySeparatorChar
    if ($null -eq $Binding) {
        Write-Host 'DesignConversion DPO storage method-pack has no exact source binding for the global class.'
        return 1
    }

    if ($Binding.BindingKind -ne 'exact_declaration') {
        Write-Host "DesignConversion DPO storage source binding is not an exact declaration binding: $($Binding.BindingKind)"
        $problems++
    }
    if ($Binding.UnitKey -ne "class:$script:DesignConversionDpoStorageMethodPackClassFqn") {
        Write-Host "DesignConversion DPO storage source binding has wrong unit key: $($Binding.UnitKey)"
        $problems++
    }
    if ($Binding.CollectionKind -ne 'none' -or -not [string]::IsNullOrWhiteSpace($Binding.CollectionName)) {
        Write-Host "DesignConversion DPO storage source binding reports shared state: collection=$($Binding.CollectionName) kind=$($Binding.CollectionKind)"
        $problems++
    }
    if ($Binding.DeclarationCount -le 0 -or $Binding.SourcePaths.Count -eq 0) {
        Write-Host 'DesignConversion DPO storage source binding is empty.'
        return ($problems + 1)
    }

    $boundSourcePaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $boundFullPaths = New-Object System.Collections.Generic.List[string]
    foreach ($relativePath in $Binding.SourcePaths) {
        if ([System.IO.Path]::IsPathRooted($relativePath)) {
            Write-Host "DesignConversion DPO storage source binding uses an absolute path: $relativePath"
            $problems++
            continue
        }

        $candidateFull = [System.IO.Path]::GetFullPath((Join-Path $rootFull $relativePath))
        if (-not $candidateFull.StartsWith($rootBoundary, [StringComparison]::OrdinalIgnoreCase)) {
            Write-Host "DesignConversion DPO storage source binding escapes the repository root: $relativePath"
            $problems++
            continue
        }

        $normalizedRelative = $candidateFull.Substring($rootBoundary.Length).Replace('\', '/')
        if (-not $boundSourcePaths.Add($normalizedRelative)) {
            Write-Host "DesignConversion DPO storage source binding duplicates a source path: $normalizedRelative"
            $problems++
        }
        [void]$boundFullPaths.Add($candidateFull)
    }

    if ($boundSourcePaths.Count -eq 0) {
        Write-Host 'DesignConversion DPO storage source binding has no in-repository source paths.'
        return ($problems + 1)
    }

    $observedDeclarations = 0
    $observedSourcePaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $testRoots = @(
        (Join-Path $rootFull 'Tests'),
        (Join-Path $rootFull 'Tests.ApiDashboardWpf')
    )
    foreach ($file in Get-SourceTestFiles -Roots $testRoots) {
        $fullPath = [System.IO.Path]::GetFullPath($file.FullName)
        if (-not (Test-Path -LiteralPath $fullPath)) {
            Write-Host "DesignConversion DPO storage source disappeared while scanning: $fullPath"
            $problems++
            continue
        }

        $relativePath = $fullPath.Substring($rootBoundary.Length).Replace('\', '/')
        $text = Get-Content -LiteralPath $fullPath -Raw
        $classPattern = '(?ms)^\s*(?<attrs>(?:\[[^\]]+\]\s*)*)(?:(?:public|internal|private|protected|sealed|abstract|partial|static|file)\s+)*(?:class|record)\s+DesignConversionDpoStorageTests\b(?<tail>[^{;]*)\{'
        foreach ($match in [regex]::Matches($text, $classPattern)) {
            $namespaceBefore = [regex]::Matches($text.Substring(0, $match.Index), '(?m)^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)\s*[;{]') | Select-Object -Last 1
            if ($null -ne $namespaceBefore) {
                continue
            }

            $observedDeclarations++
            [void]$observedSourcePaths.Add($relativePath)
            if (-not [string]::IsNullOrWhiteSpace($match.Groups['attrs'].Value)) {
                Write-Host "DesignConversion DPO storage global class has class-level attributes in source: $relativePath"
                $problems++
            }

            $beforeWhere = ($match.Groups['tail'].Value -split '\bwhere\b', 2)[0]
            if ($beforeWhere.Contains(':')) {
                Write-Host "DesignConversion DPO storage global class has a base/interface list in source: $relativePath"
                $problems++
            }
        }
    }

    foreach ($sourcePath in $boundSourcePaths) {
        if (-not $observedSourcePaths.Contains($sourcePath)) {
            Write-Host "DesignConversion DPO storage source binding names a file without an exact global declaration: $sourcePath"
            $problems++
        }
    }
    foreach ($sourcePath in $observedSourcePaths) {
        if (-not $boundSourcePaths.Contains($sourcePath)) {
            Write-Host "DesignConversion DPO storage source binding omits an exact global declaration file: $sourcePath"
            $problems++
        }
    }

    if ($observedDeclarations -ne $Binding.DeclarationCount) {
        Write-Host "DesignConversion DPO storage exact source declaration count mismatch: source=$observedDeclarations binding=$($Binding.DeclarationCount)"
        $problems++
    }

    return $problems
}

function Test-DesignConversionDpoStorageClassMethodPackEvidence {
    param(
        [string]$BatchDir,
        [object[]]$PlanRows,
        [object[]]$MethodRows,
        [hashtable]$PackReport,
        [hashtable]$FingerprintValues,
        [string]$RepositoryRoot = $RepoRoot
    )

    $problems = 0
    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    if ($PackReport['mode'] -ne 'class-method-pack') {
        Write-Host "DesignConversion DPO storage verifier was called for a non class-method-pack report: $($PackReport['mode'])"
        $problems++
    }
    if ($batchId -ne $script:DesignConversionDpoStorageBatchId) {
        Write-Host "class-method-pack is allowed only for the DesignConversion DPO storage batch: $BatchDir"
        return 1
    }

    $targetClass = $script:DesignConversionDpoStorageMethodPackClassFqn
    $slowMethod = $script:DesignConversionDpoStorageSlowMethodFqn
    $hardCap = if ($PackReport.ContainsKey('hard_cap_rows')) { [int]$PackReport['hard_cap_rows'] } else { 60 }
    $targetRowsBudget = if ($PackReport.ContainsKey('target_rows')) { [int]$PackReport['target_rows'] } else { 38 }
    if (-not $FingerprintValues.ContainsKey('leaf_hard_cap_rows') -or [int]$FingerprintValues['leaf_hard_cap_rows'] -ne $hardCap) {
        Write-Host "DesignConversion DPO storage pack report hard cap differs from fingerprint: report=$hardCap fingerprint=$($FingerprintValues['leaf_hard_cap_rows'])"
        $problems++
    }
    if (-not $FingerprintValues.ContainsKey('leaf_target_rows') -or [int]$FingerprintValues['leaf_target_rows'] -ne $targetRowsBudget) {
        Write-Host "DesignConversion DPO storage pack report target rows differ from fingerprint: report=$targetRowsBudget fingerprint=$($FingerprintValues['leaf_target_rows'])"
        $problems++
    }
    $targetRows = @($MethodRows | Where-Object { $_.ClassFqn -eq $targetClass })
    $nonTargetRows = @($MethodRows | Where-Object { $_.ClassFqn -ne $targetClass })
    $slowLeafId = ''

    if ($targetRows.Count -eq 0) {
        Write-Host "DesignConversion DPO storage method-pack has no rows for $targetClass."
        return 1
    }

    $oversizedByUnit = @{}
    try {
        foreach ($oversizedRow in Read-LeafOversizedUnitRows -Path (Join-Path $BatchDir 'leaf-oversized-units.tsv')) {
            $oversizedByUnit[$oversizedRow.UnitKey] = $oversizedRow
        }
    }
    catch {
        Write-Host "DesignConversion DPO storage oversized-unit evidence cannot be parsed: $BatchDir ($($_.Exception.Message))"
        $problems++
    }

    foreach ($row in $targetRows) {
        if ($row.UnitKey -ne "method:$($row.MethodFqn)") {
            Write-Host "DesignConversion DPO storage method row is not packed by method unit: $($row.MethodFqn) -> $($row.UnitKey)"
            $problems++
        }
        if ($row.CollectionKind -ne 'none' -or -not [string]::IsNullOrWhiteSpace($row.CollectionName)) {
            Write-Host "DesignConversion DPO storage method row carries shared collection metadata: $($row.MethodFqn)"
            $problems++
        }
    }

    $slowRows = @($targetRows | Where-Object { $_.MethodFqn -eq $slowMethod })
    if ($slowRows.Count -ne 1) {
        Write-Host "DesignConversion DPO storage slow method must appear exactly once, found $($slowRows.Count)."
        $problems++
    }
    else {
        $slowLeafId = $slowRows[0].LeafId
        $slowLeafMethods = @($MethodRows | Where-Object { $_.LeafId -eq $slowLeafId })
        $slowPlanRows = @($PlanRows | Where-Object { $_.LeafId -eq $slowLeafId })
        if ($slowPlanRows.Count -ne 1 -or $slowPlanRows[0].MethodCount -ne 1 -or $slowPlanRows[0].EstimatedRows -ne $slowRows[0].EstimatedRows) {
            Write-Host "DesignConversion DPO storage slow leaf plan is not a single-method leaf: $slowLeafId"
            $problems++
        }
        if ($slowLeafMethods.Count -ne 1 -or $slowLeafMethods[0].MethodFqn -ne $slowMethod) {
            Write-Host "DesignConversion DPO storage slow method is not isolated in its own leaf: $slowLeafId"
            $problems++
        }
    }

    $targetLeafIds = @($targetRows | Select-Object -ExpandProperty LeafId -Unique)
    foreach ($leafId in $targetLeafIds) {
        $leafMethods = @($MethodRows | Where-Object { $_.LeafId -eq $leafId })
        $foreignInStorageLeaf = @($leafMethods | Where-Object { $_.ClassFqn -ne $targetClass })
        if ($foreignInStorageLeaf.Count -gt 0) {
            Write-Host "DesignConversion DPO storage method-pack leaf mixes other classes: $leafId"
            $problems++
        }

        $planRow = @($PlanRows | Where-Object { $_.LeafId -eq $leafId }) | Select-Object -First 1
        if ($null -eq $planRow) {
            Write-Host "DesignConversion DPO storage method-pack leaf has no plan row: $leafId"
            $problems++
            continue
        }
        if (([string]::IsNullOrWhiteSpace($slowLeafId) -or $leafId -ne $slowLeafId) -and $planRow.EstimatedRows -gt $hardCap) {
            $singleMethodOversized = $false
            if ($leafMethods.Count -eq 1) {
                $oversized = if ($oversizedByUnit.ContainsKey($leafMethods[0].UnitKey)) { $oversizedByUnit[$leafMethods[0].UnitKey] } else { $null }
                $singleMethodOversized =
                    $null -ne $oversized -and
                    $oversized.MethodCount -eq 1 -and
                    $oversized.EstimatedRows -eq $planRow.EstimatedRows -and
                    $oversized.Reason -eq 'unit_exceeds_leaf_hard_cap'
            }

            if (-not $singleMethodOversized) {
                Write-Host "DesignConversion DPO storage non-slow leaf exceeds hard cap without single-method oversized proof: $leafId rows=$($planRow.EstimatedRows) cap=$hardCap"
                $problems++
            }
        }
    }

    $nonTargetSplits = Test-LeafPackUnitSplits -MethodRows $nonTargetRows
    if ($nonTargetSplits.ClassSplits -ne 0 -or $nonTargetSplits.UnitSplits -ne 0 -or $nonTargetSplits.RealSharedCollectionSplits -ne 0) {
        Write-Host "DesignConversion DPO storage class-method-pack split a non-target class or shared unit: class=$($nonTargetSplits.ClassSplits) unit=$($nonTargetSplits.UnitSplits) real_shared=$($nonTargetSplits.RealSharedCollectionSplits)"
        $problems++
    }

    $allSplits = Test-LeafPackUnitSplits -MethodRows $MethodRows
    if ($allSplits.UnitSplits -ne 0 -or $allSplits.RealSharedCollectionSplits -ne 0) {
        Write-Host "DesignConversion DPO storage class-method-pack split a unit or shared collection: unit=$($allSplits.UnitSplits) real_shared=$($allSplits.RealSharedCollectionSplits)"
        $problems++
    }
    if (($PackReport['class_splits'] -as [int]) -ne $allSplits.ClassSplits -or ($PackReport['unit_splits'] -as [int]) -ne $allSplits.UnitSplits) {
        Write-Host "DesignConversion DPO storage pack report split counts differ from independent calculation: $BatchDir"
        $problems++
    }
    if ($PackReport['largest_unit_key'] -eq "class:$targetClass") {
        Write-Host "DesignConversion DPO storage pack report still describes the old whole-class storage unit."
        $problems++
    }
    if ($PackReport['method_pack_class'] -ne $targetClass -or $PackReport['method_pack_slow_method'] -ne $slowMethod) {
        Write-Host "DesignConversion DPO storage pack report names the wrong method-pack target."
        $problems++
    }
    if (($PackReport['method_pack_method_count'] -as [int]) -ne $targetRows.Count -or
        ($PackReport['method_pack_estimated_rows'] -as [int]) -ne (($targetRows | Measure-Object EstimatedRows -Sum).Sum -as [int]) -or
        ($PackReport['method_pack_slow_leaf_count'] -as [int]) -ne 1) {
        Write-Host "DesignConversion DPO storage pack report counters do not match leaf-methods.tsv."
        $problems++
    }

    $exactBindingsPath = Join-Path $BatchDir 'leaf-source-exact-bindings.tsv'
    try {
        $exactRows = @(Read-LeafSourceExactBindingRows -Path $exactBindingsPath)
    }
    catch {
        Write-Host "DesignConversion DPO storage exact source bindings cannot be parsed: $BatchDir ($($_.Exception.Message))"
        return ($problems + 1)
    }

    if (-not $FingerprintValues.ContainsKey('source_exact_bindings_sha256') -or
        $FingerprintValues['source_exact_bindings_sha256'] -ne (Get-FileSha256Lower -Path $exactBindingsPath)) {
        Write-Host "DesignConversion DPO storage exact source binding hash is missing or stale: $BatchDir"
        $problems++
    }

    $targetBindings = @($exactRows | Where-Object { $_.ClassFqn -eq $targetClass })
    if ($targetBindings.Count -ne 1) {
        Write-Host "DesignConversion DPO storage exact source binding row count must be one, found $($targetBindings.Count)."
        $problems++
    }
    else {
        $problems += Test-DesignConversionDpoStorageExactSourceBinding -Binding $targetBindings[0] -RepositoryRoot $RepositoryRoot
    }

    return $problems
}

function Read-ClassFirstMethodRows {
    param([string]$Path)

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -lt 1 -or $lines[0] -ne "method_fqn`testimated_rows`tclass_fqn`tbatch_id`tdll`tunit_key`tcollection_name`tcollection_kind") {
        throw 'class-first-methods.tsv has no exact eight-column header.'
    }

    $rows = New-Object System.Collections.Generic.List[object]
    for ($index = 1; $index -lt $lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($lines[$index])) {
            continue
        }

        $parts = $lines[$index] -split "`t"
        if ($parts.Count -ne 8) {
            throw "class-first-methods.tsv line $($index + 1) has unexpected columns."
        }

        $rows.Add([pscustomobject]@{
            MethodFqn = $parts[0]
            EstimatedRows = [int]$parts[1]
            ClassFqn = $parts[2]
            BatchId = $parts[3]
            Dll = $parts[4]
            UnitKey = $parts[5]
            CollectionName = $parts[6]
            CollectionKind = $parts[7]
        })
    }

    return $rows
}

function Read-ListTestsMethodRows {
    param([string]$Path)

    $rows = New-Object System.Collections.Generic.List[object]
    $counts = [ordered]@{}
    $capture = $false
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line.Contains('The following Tests are available')) {
            $capture = $true
            continue
        }
        if (-not $capture) {
            continue
        }

        $text = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($text)) {
            continue
        }
        if ($text.StartsWith('Passed!') -or $text.StartsWith('Failed!') -or $text.StartsWith('Total tests') -or $text.StartsWith('Test Run') -or $text.StartsWith('Microsoft') -or $text.StartsWith('Copyright')) {
            continue
        }

        $method = ($text -split '\(', 2)[0].Trim()
        if ($method -notmatch '^[A-Za-z0-9_.+]+$' -or -not $method.Contains('.')) {
            continue
        }

        if (-not $counts.Contains($method)) {
            $counts[$method] = 0
        }
        $counts[$method]++
    }

    foreach ($method in $counts.Keys) {
        $rows.Add([pscustomobject]@{
            MethodFqn = $method
            EstimatedRows = [int]$counts[$method]
            ClassFqn = ($method -replace '\.[^.]+$', '')
        })
    }

    return $rows
}

function Get-ClassFirstExpectedRowsFromRawDiscovery {
    param(
        [string]$BatchDir,
        [object[]]$Rows,
        [switch]$RequireEvidence
    )

    $rawPath = Join-Path $BatchDir 'class-first-list-tests.stdout.log'
    if (-not (Test-Path -LiteralPath $rawPath)) {
        if ($RequireEvidence) {
            return [pscustomobject]@{ Valid = $false; Reason = 'missing_raw_list_tests'; Rows = @(); RawMethodCount = 0; RawEstimatedRows = 0; UnownedClasses = @() }
        }
        return [pscustomobject]@{ Valid = $true; Reason = 'not_recorded'; Rows = @(); RawMethodCount = 0; RawEstimatedRows = 0; UnownedClasses = @() }
    }

    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $manifestRow = @($Rows | Where-Object { $_.BatchId -eq $batchId })
    if ($manifestRow.Count -ne 1) {
        return [pscustomobject]@{ Valid = $false; Reason = "manifest_row_count:$($manifestRow.Count)"; Rows = @(); RawMethodCount = 0; RawEstimatedRows = 0; UnownedClasses = @() }
    }

    $expected = New-Object System.Collections.Generic.List[object]
    $unownedClasses = New-Object System.Collections.Generic.List[string]
    $rawMethods = @(Read-ListTestsMethodRows -Path $rawPath)
    if ($rawMethods.Count -eq 0) {
        return [pscustomobject]@{ Valid = $false; Reason = 'raw_list_tests_empty'; Rows = @(); RawMethodCount = 0; RawEstimatedRows = 0; UnownedClasses = @() }
    }

    $ownerByClass = @{}
    $rawEstimatedRows = 0
    foreach ($method in $rawMethods) {
        $rawEstimatedRows += [int]$method.EstimatedRows
        if (-not $ownerByClass.ContainsKey($method.ClassFqn)) {
            $classAsTest = [pscustomobject]@{
                Dll = $manifestRow[0].Dll
                FullyQualifiedName = $method.ClassFqn
            }
            $owner = Find-FirstBatch -TestClass $classAsTest -Rows $Rows
            $ownerByClass[$method.ClassFqn] = if ($null -eq $owner) { '' } else { $owner.BatchId }
            if ($null -eq $owner) {
                $unownedClasses.Add($method.ClassFqn)
            }
        }

        if ($ownerByClass[$method.ClassFqn] -eq $batchId) {
            $expected.Add([pscustomobject]@{
                MethodFqn = $method.MethodFqn
                EstimatedRows = $method.EstimatedRows
                ClassFqn = $method.ClassFqn
                BatchId = $batchId
            })
        }
    }

    return [pscustomobject]@{ Valid = $true; Reason = ''; Rows = @($expected.ToArray()); RawMethodCount = $rawMethods.Count; RawEstimatedRows = $rawEstimatedRows; UnownedClasses = @($unownedClasses.ToArray()) }
}

function Test-ClassFirstExpectedMethods {
    param(
        [string]$BatchDir,
        [object[]]$ActualMethodRows,
        [object[]]$Rows = @(),
        [hashtable]$FingerprintValues = @{},
        [switch]$RequireFingerprintHash
    )

    $problems = 0
    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $expectedPath = Join-Path $BatchDir 'class-first-methods.tsv'
    if (-not (Test-Path -LiteralPath $expectedPath)) {
        if ($RequireFingerprintHash) {
            Write-Host "Canonical leaf batch has no class-first method evidence: $BatchDir"
            return 1
        }

        return 0
    }

    if ($RequireFingerprintHash) {
        if (-not $FingerprintValues.ContainsKey('class_first_methods_sha256')) {
            Write-Host "Leaf fingerprint has no class-first method hash: $BatchDir"
            $problems++
        }
        else {
            $actualHash = Get-FileSha256Lower -Path $expectedPath
            if ($FingerprintValues['class_first_methods_sha256'] -ne $actualHash) {
                Write-Host "Leaf class-first method evidence hash mismatch: $BatchDir"
                $problems++
            }
        }

        if (-not $FingerprintValues.ContainsKey('class_first_list_tests_sha256')) {
            Write-Host "Leaf fingerprint has no raw ListTests hash: $BatchDir"
            $problems++
        }
        else {
            $rawPath = Join-Path $BatchDir 'class-first-list-tests.stdout.log'
            if (-not (Test-Path -LiteralPath $rawPath) -or $FingerprintValues['class_first_list_tests_sha256'] -ne (Get-FileSha256Lower -Path $rawPath)) {
                Write-Host "Leaf raw ListTests evidence hash mismatch: $BatchDir"
                $problems++
            }
        }
    }

    try {
        $expectedRows = @(Read-ClassFirstMethodRows -Path $expectedPath)
    }
    catch {
        Write-Host "Class-first method evidence cannot be parsed: $BatchDir ($($_.Exception.Message))"
        return ($problems + 1)
    }

    if ($Rows.Count -gt 0) {
        $rawExpected = Get-ClassFirstExpectedRowsFromRawDiscovery -BatchDir $BatchDir -Rows $Rows -RequireEvidence:$RequireFingerprintHash
        if (-not $rawExpected.Valid) {
            Write-Host "Class-first raw ListTests evidence cannot be used: $BatchDir ($($rawExpected.Reason))"
            $problems++
        }
        else {
            $fromRaw = @{}
            foreach ($row in $rawExpected.Rows) {
                $fromRaw[$row.MethodFqn] = $row
            }
            $fromDerived = @{}
            foreach ($row in $expectedRows) {
                $fromDerived[$row.MethodFqn] = $row
            }

            foreach ($method in $fromRaw.Keys) {
                if (-not $fromDerived.ContainsKey($method)) {
                    Write-Host "Class-first derived evidence omitted raw-discovered method: $method"
                    $problems++
                }
                elseif ($fromDerived[$method].EstimatedRows -ne $fromRaw[$method].EstimatedRows -or $fromDerived[$method].ClassFqn -ne $fromRaw[$method].ClassFqn) {
                    Write-Host "Class-first derived evidence differs from raw ListTests assignment: $method"
                    $problems++
                }
            }

            foreach ($method in $fromDerived.Keys) {
                if (-not $fromRaw.ContainsKey($method)) {
                    Write-Host "Class-first derived evidence adds method outside raw ListTests owner: $method"
                    $problems++
                }
            }
        }
    }

    $expected = @{}
    foreach ($row in $expectedRows) {
        if ($row.BatchId -ne $batchId) {
            Write-Host "Class-first method row belongs to another batch: $($row.MethodFqn) -> $($row.BatchId)"
            $problems++
        }
        if ($expected.ContainsKey($row.MethodFqn)) {
            Write-Host "Class-first method evidence duplicates method: $($row.MethodFqn)"
            $problems++
        }
        else {
            $expected[$row.MethodFqn] = $row
        }
    }

    $actual = @{}
    foreach ($row in $ActualMethodRows) {
        if ($actual.ContainsKey($row.MethodFqn)) {
            Write-Host "Leaf method evidence duplicates method after class-first comparison: $($row.MethodFqn)"
            $problems++
        }
        else {
            $actual[$row.MethodFqn] = $row
        }
    }

    foreach ($method in $expected.Keys) {
        if (-not $actual.ContainsKey($method)) {
            # Здесь ловится именно недобор: runner не может взять filtered
            # ListTests как источник истины, если полный class-first список
            # назначил методу этот batch.
            Write-Host "Canonical leaf evidence is missing class-first method: $method"
            $problems++
            continue
        }

        if ($actual[$method].ClassFqn -ne $expected[$method].ClassFqn -or $actual[$method].EstimatedRows -ne $expected[$method].EstimatedRows) {
            Write-Host "Canonical leaf evidence differs from class-first method row: $method"
            $problems++
        }
    }

    foreach ($method in $actual.Keys) {
        if (-not $expected.ContainsKey($method)) {
            Write-Host "Canonical leaf evidence contains method outside class-first owner: $method"
            $problems++
        }
    }

    return $problems
}

function Test-EmptyResidualEvidence {
    param(
        [string]$BatchDir,
        [object]$JournalRow,
        [object[]]$Rows = @()
    )

    $problems = 0
    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $manifestRows = @($Rows | Where-Object { $_.BatchId -eq $batchId })
    $evidencePath = Join-Path $BatchDir 'empty-residual.tsv'
    $methodsPath = Join-Path $BatchDir 'class-first-methods.tsv'
    $originPath = Join-Path $BatchDir 'class-first-origin.tsv'
    $rawPath = Join-Path $BatchDir 'class-first-list-tests.stdout.log'

    if ($manifestRows.Count -ne 1) {
        Write-Host "Empty residual batch has no unique manifest row: $BatchDir (matches=$($manifestRows.Count))"
        $problems++
    }
    else {
        if (-not (Test-IsLastWildcardBatchForDll -Row $manifestRows[0] -Rows $Rows)) {
            Write-Host "Empty residual state is allowed only for the last wildcard row of this DLL: $BatchDir"
            $problems++
        }
    }

    if ($null -eq $JournalRow) {
        Write-Host "Empty residual batch has no required journal row: $BatchDir"
        $problems++
    }
    else {
        if ($JournalRow.State -ne 'EMPTY_VERIFIED' -or $JournalRow.ExitCode -ne 'empty-residual') {
            Write-Host "Empty residual journal does not name the empty state explicitly: $BatchDir"
            $problems++
        }
        if ($JournalRow.Total -ne 0 -or $JournalRow.Passed -ne 0 -or $JournalRow.Failed -ne 0 -or $JournalRow.NotExecuted -ne 0) {
            Write-Host "Empty residual journal pretends to have physical result rows: $BatchDir"
            $problems++
        }
        $journalEvidencePath = if ([System.IO.Path]::IsPathRooted($JournalRow.ResultPath)) { $JournalRow.ResultPath } else { Join-Path $BatchDir $JournalRow.ResultPath }
        if ([string]::IsNullOrWhiteSpace($JournalRow.ResultPath) -or
            -not [string]::Equals((Get-NormalizedPath -Path $journalEvidencePath), (Get-NormalizedPath -Path $evidencePath), [StringComparison]::OrdinalIgnoreCase)) {
            Write-Host "Empty residual journal result path does not point to empty-residual.tsv: $BatchDir"
            $problems++
        }
    }

    if (Test-Path -LiteralPath (Join-Path $BatchDir 'result.trx')) {
        Write-Host "Empty residual batch has fake root TRX: $BatchDir"
        $problems++
    }
    if (Test-Path -LiteralPath (Join-Path $BatchDir 'current-attempt.txt')) {
        Write-Host "Empty residual batch has a VSTest current-attempt marker: $BatchDir"
        $problems++
    }
    if (-not (Test-Path -LiteralPath $evidencePath)) {
        Write-Host "Empty residual batch has no empty-residual.tsv proof: $BatchDir"
        return ($problems + 1)
    }
    if (-not (Test-Path -LiteralPath $methodsPath) -or -not (Test-Path -LiteralPath $originPath) -or -not (Test-Path -LiteralPath $rawPath)) {
        Write-Host "Empty residual batch is missing class-first raw or derived evidence: $BatchDir"
        return ($problems + 1)
    }

    $values = Read-KeyValueTsv -Path $evidencePath
    $originValues = Read-KeyValueTsv -Path $originPath
    $context = "Empty residual evidence for $batchId"
    $problems += Test-KeyValueEquals -Values $values -Key 'empty_semantics' -Expected 'class_first_empty_residual_v1' -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'batch_id' -Expected $batchId -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'filter' -Expected '*' -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'residual_method_count' -Expected '0' -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'residual_estimated_rows' -Expected '0' -Context $context

    if ($manifestRows.Count -eq 1) {
        $actualDll = if ($values.ContainsKey('test_dll')) { Normalize-DllTextForCompare -Value $values['test_dll'] } else { '' }
        $expectedDll = Normalize-DllTextForCompare -Value $manifestRows[0].Dll
        if ($actualDll -ne $expectedDll) {
            Write-Host "Empty residual evidence points to another DLL: $BatchDir"
            $problems++
        }

        $dllPath = $manifestRows[0].Dll
        if (-not [System.IO.Path]::IsPathRooted($dllPath)) {
            $dllPath = Join-Path $RepoRoot $dllPath
        }
        if (-not (Test-Path -LiteralPath $dllPath)) {
            Write-Host "Empty residual DLL cannot be found for hash check: $BatchDir ($dllPath)"
            $problems++
        }
        else {
            $problems += Test-KeyValueEquals -Values $values -Key 'test_dll_sha256' -Expected (Get-FileSha256Lower -Path $dllPath) -Context $context
        }
    }

    $currentHead = Get-CurrentGitHeadForEvidence
    if ([string]::IsNullOrWhiteSpace($currentHead)) {
        Write-Host "Empty residual verifier cannot read current git HEAD: $BatchDir"
        $problems++
    }
    else {
        $problems += Test-KeyValueEquals -Values $values -Key 'head' -Expected $currentHead -Context $context
    }

    $runnerPath = Join-Path $RepoRoot 'tools\test-batches\run-test-batches.sh'
    if (Test-Path -LiteralPath $runnerPath) {
        $problems += Test-KeyValueEquals -Values $values -Key 'runner_sha256' -Expected (Get-FileSha256Lower -Path $runnerPath) -Context $context
    }
    else {
        Write-Host "Empty residual verifier cannot find runner script: $runnerPath"
        $problems++
    }
    $problems += Test-KeyValueEquals -Values $values -Key 'verifier_sha256' -Expected (Get-FileSha256Lower -Path $PSCommandPath) -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'batches_tsv_sha256' -Expected (Get-FileSha256Lower -Path $BatchesFile) -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'class_first_methods_sha256' -Expected (Get-FileSha256Lower -Path $methodsPath) -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'class_first_origin_sha256' -Expected (Get-FileSha256Lower -Path $originPath) -Context $context
    $problems += Test-KeyValueEquals -Values $values -Key 'class_first_list_tests_sha256' -Expected (Get-FileSha256Lower -Path $rawPath) -Context $context

    if ($originValues.ContainsKey('method_assignments_sha256')) {
        $problems += Test-KeyValueEquals -Values $values -Key 'method_assignments_sha256' -Expected $originValues['method_assignments_sha256'] -Context $context
    }
    else {
        Write-Host "Empty residual origin evidence has no method assignment hash: $BatchDir"
        $problems++
    }
    if ($originValues.ContainsKey('cache_identity_sha256')) {
        $problems += Test-KeyValueEquals -Values $values -Key 'cache_identity_sha256' -Expected $originValues['cache_identity_sha256'] -Context $context
    }
    else {
        Write-Host "Empty residual origin evidence has no cache identity hash: $BatchDir"
        $problems++
    }
    if ($originValues.ContainsKey('selected_method_count') -and $originValues['selected_method_count'] -ne '0') {
        Write-Host "Empty residual origin evidence is not empty: $BatchDir"
        $problems++
    }

    try {
        $derivedRows = @(Read-ClassFirstMethodRows -Path $methodsPath)
    }
    catch {
        Write-Host "Empty residual class-first method evidence cannot be parsed: $BatchDir ($($_.Exception.Message))"
        return ($problems + 1)
    }
    if ($derivedRows.Count -ne 0) {
        Write-Host "Empty residual class-first method list is not empty: $BatchDir"
        $problems++
    }

    if ($Rows.Count -gt 0) {
        $rawExpected = Get-ClassFirstExpectedRowsFromRawDiscovery -BatchDir $BatchDir -Rows $Rows -RequireEvidence
        if (-not $rawExpected.Valid) {
            Write-Host "Empty residual raw ListTests evidence cannot be used: $BatchDir ($($rawExpected.Reason))"
            $problems++
        }
        else {
            if ($rawExpected.RawMethodCount -le 0) {
                Write-Host "Empty residual raw ListTests is empty: $BatchDir"
                $problems++
            }
            if (@($rawExpected.UnownedClasses).Count -gt 0) {
                Write-Host "Empty residual raw ListTests contains unowned class: $($rawExpected.UnownedClasses[0])"
                $problems++
            }
            if (@($rawExpected.Rows).Count -ne 0) {
                Write-Host "Empty residual raw ListTests assigns methods to residual batch: $BatchDir"
                $problems++
            }
            $problems += Test-KeyValueEquals -Values $values -Key 'total_runtime_methods' -Expected ([string]$rawExpected.RawMethodCount) -Context $context
            $problems += Test-KeyValueEquals -Values $values -Key 'total_runtime_rows' -Expected ([string]$rawExpected.RawEstimatedRows) -Context $context
        }
    }

    return $problems
}

function Test-LeafExactFilter {
    param([string]$Filter)

    if ([string]::IsNullOrWhiteSpace($Filter) -or $Filter.Contains('FullyQualifiedName~') -or $Filter.Contains('FullyQualifiedName!~')) {
        return $false
    }

    foreach ($part in $Filter -split '\|') {
        # Leaf-фильтр принимает только точные method FQN. Это защита от prefix
        # collisions: TestA не должен захватывать TestAExtended.
        if ($part -notmatch '^FullyQualifiedName=[A-Za-z0-9_.+]+$') {
            return $false
        }
    }

    return $true
}

function Get-MethodsFromLeafFilter {
    param([string]$Filter)

    $methods = New-Object System.Collections.Generic.List[string]
    foreach ($part in $Filter -split '\|') {
        if ($part -match '^FullyQualifiedName=([A-Za-z0-9_.+]+)$') {
            $methods.Add($matches[1])
        }
    }

    return $methods
}

function Test-LeafFingerprintSelfConsistent {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return [pscustomobject]@{ Valid = $false; Reason = 'missing' }
    }

    $lines = @(Get-Content -LiteralPath $Path)
    if ($lines.Count -lt 2 -or $lines[0] -ne "key`tvalue") {
        return [pscustomobject]@{ Valid = $false; Reason = 'bad_header' }
    }

    $stored = @($lines | Where-Object { $_ -like "fingerprint_sha256`t*" })
    if ($stored.Count -ne 1) {
        return [pscustomobject]@{ Valid = $false; Reason = 'fingerprint_row_count' }
    }

    $storedValue = ($stored[0] -split "`t", 2)[1]
    $baseText = (($lines | Where-Object { $_ -notlike "fingerprint_sha256`t*" }) -join "`n") + "`n"
    $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($baseText)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $computed = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }

    if ($storedValue -ne $computed) {
        return [pscustomobject]@{ Valid = $false; Reason = 'metadata_hash_mismatch' }
    }

    return [pscustomobject]@{ Valid = $true; Reason = '' }
}

function Test-LeafPackUnitSplits {
    param([object[]]$MethodRows)

    $classSplits = @($MethodRows | Group-Object ClassFqn | Where-Object { @($_.Group | Select-Object -ExpandProperty LeafId -Unique).Count -gt 1 }).Count
    $unitSplits = @($MethodRows | Group-Object UnitKey | Where-Object { @($_.Group | Select-Object -ExpandProperty LeafId -Unique).Count -gt 1 }).Count
    $realSharedSplits = @($MethodRows |
        Where-Object { $_.CollectionKind -in @('shared_fixture', 'manifest_shared') -and -not [string]::IsNullOrWhiteSpace($_.CollectionName) } |
        Group-Object CollectionName |
        Where-Object { @($_.Group | Select-Object -ExpandProperty LeafId -Unique).Count -gt 1 }).Count
    $schedulingOnlySpans = @($MethodRows |
        Where-Object { $_.CollectionKind -eq 'scheduling_only' -and -not [string]::IsNullOrWhiteSpace($_.CollectionName) } |
        Group-Object CollectionName |
        Where-Object { @($_.Group | Select-Object -ExpandProperty LeafId -Unique).Count -gt 1 }).Count
    return [pscustomobject]@{
        ClassSplits = $classSplits
        UnitSplits = $unitSplits
        RealSharedCollectionSplits = $realSharedSplits
        SchedulingOnlyCollectionSpans = $schedulingOnlySpans
    }
}

function Test-LeafTrxMembership {
    param(
        [string]$TrxPath,
        [object[]]$MethodRows,
        [object]$PlanRow
    )

    $expectedRows = @($MethodRows | Where-Object { $_.LeafId -eq $PlanRow.LeafId })
    if ($expectedRows.Count -eq 0) {
        return [pscustomobject]@{ Valid = $false; Reason = 'leaf_methods_empty_for_leaf'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    $expected = @{}
    foreach ($row in $expectedRows) {
        if ($expected.ContainsKey($row.MethodFqn)) {
            return [pscustomobject]@{ Valid = $false; Reason = "leaf_methods_duplicate:$($row.MethodFqn)"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
        $expected[$row.MethodFqn] = [int]$row.EstimatedRows
    }

    $filterMethods = @(Get-MethodsFromLeafFilter -Filter $PlanRow.Filter)
    if ($filterMethods.Count -ne $expected.Keys.Count) {
        return [pscustomobject]@{ Valid = $false; Reason = 'filter_method_count_mismatch'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }
    foreach ($method in $filterMethods) {
        if (-not $expected.ContainsKey($method)) {
            return [pscustomobject]@{ Valid = $false; Reason = "filter_foreign_method:$method"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
    }

    $expectedTotal = ($expected.Values | Measure-Object -Sum).Sum
    if ($expectedTotal -ne [int]$PlanRow.EstimatedRows) {
        return [pscustomobject]@{ Valid = $false; Reason = "leaf_plan_expected_rows_mismatch:$expectedTotal!=$($PlanRow.EstimatedRows)"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    $counts = Read-TrxCounts -Path $TrxPath
    if (-not $counts.Valid) {
        return [pscustomobject]@{ Valid = $false; Reason = $counts.Reason; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    try {
        [xml]$xml = Get-Content -LiteralPath $TrxPath -Raw
    }
    catch {
        return [pscustomobject]@{ Valid = $false; Reason = 'corrupt_xml'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    $definitions = @{}
    foreach ($unit in @($xml.GetElementsByTagName('UnitTest'))) {
        $testId = $unit.GetAttribute('id')
        if ([string]::IsNullOrWhiteSpace($testId)) {
            return [pscustomobject]@{ Valid = $false; Reason = 'unit_test_without_id'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
        if ($definitions.ContainsKey($testId)) {
            return [pscustomobject]@{ Valid = $false; Reason = "duplicate_unit_test_id:$testId"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }

        $methodNode = @($unit.GetElementsByTagName('TestMethod')) | Select-Object -First 1
        $className = ''
        $methodName = ''
        if ($null -ne $methodNode) {
            $className = ($methodNode.GetAttribute('className') -split ',', 2)[0].Trim()
            $methodName = ($methodNode.GetAttribute('name') -split '\(', 2)[0].Trim()
        }
        $fallback = ($unit.GetAttribute('name') -split '\(', 2)[0].Trim()
        if (-not [string]::IsNullOrWhiteSpace($className) -and -not [string]::IsNullOrWhiteSpace($methodName)) {
            if ($methodName.StartsWith($className + '.', [StringComparison]::Ordinal)) {
                $definitions[$testId] = $methodName
            }
            else {
                $definitions[$testId] = "$className.$methodName"
            }
        }
        elseif ($fallback.Contains('.')) {
            $definitions[$testId] = $fallback
        }
        else {
            return [pscustomobject]@{ Valid = $false; Reason = "definition_without_method_identity:$testId"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
    }

    if ($definitions.Count -eq 0) {
        return [pscustomobject]@{ Valid = $false; Reason = 'no_unit_test_definitions'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    $actual = @{}
    foreach ($result in @($xml.GetElementsByTagName('UnitTestResult'))) {
        $testId = $result.GetAttribute('testId')
        if ([string]::IsNullOrWhiteSpace($testId)) {
            return [pscustomobject]@{ Valid = $false; Reason = 'unit_test_result_without_test_id'; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
        if (-not $definitions.ContainsKey($testId)) {
            return [pscustomobject]@{ Valid = $false; Reason = "unit_test_result_unknown_test_id:$testId"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }

        $method = $definitions[$testId]
        if (-not $actual.ContainsKey($method)) {
            $actual[$method] = 0
        }
        $actual[$method]++
    }

    foreach ($method in $actual.Keys) {
        if (-not $expected.ContainsKey($method)) {
            return [pscustomobject]@{ Valid = $false; Reason = "foreign_method:$method"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
    }

    foreach ($method in $expected.Keys) {
        $actualRows = if ($actual.ContainsKey($method)) { $actual[$method] } else { 0 }
        # estimated_rows хранит минимум для упаковки leaf-пака. Один [Theory]
        # может раскрыться в несколько строк того же метода, поэтому verifier
        # запрещает недобор и чужие методы, но принимает честный избыток.
        if ($actualRows -lt $expected[$method]) {
            return [pscustomobject]@{ Valid = $false; Reason = "method_row_count_mismatch:${method}:$actualRows<$($expected[$method])"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
        }
    }

    if ($counts.Total -lt $expectedTotal) {
        return [pscustomobject]@{ Valid = $false; Reason = "leaf_total_rows_below_plan:$($counts.Total)<$expectedTotal"; Total = 0; Passed = 0; Failed = 0; NotExecuted = 0 }
    }

    return [pscustomobject]@{
        Valid = $true
        Reason = 'membership_valid'
        Total = $counts.Total
        Passed = $counts.Passed
        Failed = $counts.Failed
        NotExecuted = $counts.NotExecuted
        RawPassed = $counts.RawPassed
        RawFailed = $counts.RawFailed
        RawNotExecuted = $counts.RawNotExecuted
        LegacyNormalizedDynamicSkips = $counts.LegacyNormalizedDynamicSkips
    }
}

function Get-LastLeafJournalRow {
    param(
        [object[]]$JournalRows,
        [string]$LeafId
    )

    $matches = @($JournalRows | Where-Object { $_.LeafId -eq $LeafId })
    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[$matches.Count - 1]
}

function Test-LeafJournalMatchesMembership {
    param(
        [object]$JournalRow,
        [string]$LeafState,
        [object]$Membership
    )

    if ($JournalRow.State -ne $LeafState -or $JournalRow.Total -ne $Membership.Total) {
        return $false
    }

    if ($JournalRow.Passed -eq $Membership.Passed -and $JournalRow.Failed -eq $Membership.Failed -and $JournalRow.NotExecuted -eq $Membership.NotExecuted) {
        return $true
    }

    # Старые evidence-файлы нельзя переписывать: journal остаётся append-only
    # историей. Поэтому для exact legacy Qwen runtime-skip разрешаем старую raw
    # leaf-journal строку, но только если TRX-нормализация уже прошла строгие
    # проверки FQN+prefix и opt-in не был включён.
    if (($Membership.LegacyNormalizedDynamicSkips -as [int]) -gt 0) {
        return $JournalRow.Passed -eq $Membership.RawPassed -and
            $JournalRow.Failed -eq $Membership.RawFailed -and
            $JournalRow.NotExecuted -eq $Membership.RawNotExecuted
    }

    return $false
}

function Test-LeafBatchEvidence {
    param(
        [string]$BatchDir,
        [object[]]$Rows = @(),
        [object[]]$EffectiveRows = @(),
        [object[]]$Classes = @(),
        [string]$ExpectedScope = '',
        [object]$FocusedExpectation = $null,
        [string]$CurrentBatchesSha256 = '',
        [hashtable]$ExpectedLeafModes = @{},
        [switch]$AllowLegacyFocusedFilterNormalization,
        [switch]$RequireScopeInFingerprint,
        [switch]$AllowIncomplete
    )

    $problems = 0
    $planPath = Join-Path $BatchDir 'leaf-plan.tsv'
    $journalPath = Join-Path $BatchDir 'leaf-journal.tsv'
    $methodPath = Join-Path $BatchDir 'leaf-methods.tsv'
    $fingerprintPath = Join-Path $BatchDir 'leaf-fingerprint.tsv'
    $batchState = Read-StateValue -Path (Join-Path $BatchDir 'state.txt')
    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $minRowsEvidence = Get-BatchMinRowsEvidence -Rows $Rows -BatchId $batchId

    try {
        $planRows = @(Read-LeafPlanRows -Path $planPath)
        $journalRows = @(Read-LeafJournalRows -Path $journalPath)
        $methodRows = @(Read-LeafMethodRows -Path $methodPath)
    }
    catch {
        Write-Host "Leaf evidence cannot be parsed: $BatchDir ($($_.Exception.Message))"
        return 1
    }

    if ($Rows.Count -gt 0 -and -not $minRowsEvidence.Known) {
        # Реальный manifest обязан давать одну строку с min_rows для каждой
        # проверяемой папки; Rows=@() оставлен только для старых изолированных
        # самопроверок без канонического списка групп.
        Write-Host "Leaf batch has no unique manifest min_rows row: $BatchDir (matches=$($minRowsEvidence.Count))"
        $problems++
    }

    $fingerprint = Test-LeafFingerprintSelfConsistent -Path $fingerprintPath
    if (-not $fingerprint.Valid) {
        Write-Host "Leaf batch fingerprint is not self-consistent: $BatchDir ($($fingerprint.Reason))"
        $problems++
    }

    $fingerprintValues = Read-KeyValueTsv -Path $fingerprintPath
    if ($RequireScopeInFingerprint) {
        # Новый runner пишет область проверки в fingerprint каждого leaf. Verifier
        # сверяет это независимо от journal, чтобы focused-корень не мог
        # переодеться в canonical и заявить полный suite.
        $actualScope = if ($fingerprintValues.ContainsKey('verification_scope')) { $fingerprintValues['verification_scope'] } else { '' }
        if ($actualScope -ne $ExpectedScope) {
            Write-Host "Leaf fingerprint scope mismatch: $BatchDir ($actualScope vs $ExpectedScope)"
            $problems++
        }

        if ($fingerprintValues.ContainsKey('full_suite_claim_allowed')) {
            $expectedClaim = if ($ExpectedScope -eq 'FOCUSED') { 'false' } else { 'true' }
            if ($fingerprintValues['full_suite_claim_allowed'] -ne $expectedClaim) {
                Write-Host "Leaf fingerprint full-suite claim flag mismatch: $BatchDir"
                $problems++
            }
        }
    }

    if ($null -ne $FocusedExpectation) {
        # Focused evidence обязан принадлежать ровно текущему manifest. Иначе
        # сохраненный TRX от метода A можно было бы проверить manifest-ом метода B.
        if (-not $fingerprintValues.ContainsKey('batches_tsv_sha256') -or $fingerprintValues['batches_tsv_sha256'] -ne $CurrentBatchesSha256) {
            $actualHash = if ($fingerprintValues.ContainsKey('batches_tsv_sha256')) { $fingerprintValues['batches_tsv_sha256'] } else { '' }
            Write-Host "Focused leaf fingerprint manifest hash mismatch: $BatchDir ($actualHash vs $CurrentBatchesSha256)"
            $problems++
        }

        if (-not $fingerprintValues.ContainsKey('selected_filter')) {
            Write-Host "Focused leaf fingerprint has no selected_filter: $BatchDir"
            $problems++
        }
        else {
            try {
                $actualFilter = Normalize-FocusedExactFilterForBinding -Filter $fingerprintValues['selected_filter'] -AllowLegacyOuterParentheses:$AllowLegacyFocusedFilterNormalization
                if ($actualFilter -ne $FocusedExpectation.NormalizedFilter) {
                    Write-Host "Focused leaf selected_filter differs from current manifest: $BatchDir"
                    $problems++
                }
            }
            catch {
                Write-Host "Focused leaf selected_filter is not an exact focused filter: $BatchDir ($($_.Exception.Message))"
                $problems++
            }
        }
    }

    if (Test-Path -LiteralPath (Join-Path $BatchDir 'fingerprint-mismatch.txt')) {
        Write-Host "Leaf batch reports fingerprint mismatch: $BatchDir"
        $problems++
    }

    $expectedLeafMode = if ($ExpectedLeafModes.ContainsKey($batchId)) { [string]$ExpectedLeafModes[$batchId] } else { '' }
    if (-not [string]::IsNullOrWhiteSpace($expectedLeafMode)) {
        $actualFingerprintMode = if ($fingerprintValues.ContainsKey('effective_leaf_mode')) { $fingerprintValues['effective_leaf_mode'] } else { '' }
        if ($actualFingerprintMode -ne $expectedLeafMode) {
            Write-Host "Leaf fingerprint mode mismatch: $BatchDir ($actualFingerprintMode vs $expectedLeafMode)"
            $problems++
        }
    }

    $duplicateMethods = @($methodRows | Group-Object MethodFqn | Where-Object Count -gt 1)
    if ($duplicateMethods.Count -gt 0) {
        Write-Host "Leaf method plan assigns a method more than once: $($duplicateMethods[0].Name)"
        $problems += $duplicateMethods.Count
    }

    $classFirstEvidencePath = Join-Path $BatchDir 'class-first-methods.tsv'
    $hasClassFirstEvidence = Test-Path -LiteralPath $classFirstEvidencePath
    if ($ExpectedScope -eq 'CANONICAL' -and $null -eq $FocusedExpectation -and -not $hasClassFirstEvidence -and $EffectiveRows.Count -gt 0 -and $Classes.Count -gt 0) {
        $effectiveRow = @($EffectiveRows | Where-Object { $_.BatchId -eq $batchId })
        if ($effectiveRow.Count -ne 1) {
            Write-Host "Canonical leaf batch has no unique effective manifest row: $BatchDir"
            $problems++
        }
        else {
            $allowedClasses = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
            foreach ($testClass in $Classes) {
                if ($testClass.Dll -eq $effectiveRow[0].Dll -and (Test-EffectiveFilterForClass -Filter $effectiveRow[0].EffectiveFilter -FullyQualifiedName $testClass.FullyQualifiedName)) {
                    [void]$allowedClasses.Add($testClass.FullyQualifiedName)
                }
            }

            foreach ($methodRow in $methodRows) {
                if ($methodRow.BatchId -ne $batchId) {
                    Write-Host "Canonical leaf-methods row belongs to another batch: $($methodRow.MethodFqn) -> $($methodRow.BatchId)"
                    $problems++
                    continue
                }

                # Runtime discovery приносит имена методов, а лестница батчей
                # назначает целые классы. Эта проверка не даёт коротким словам
                # вроде Tokenizer протащить чужой класс в class-level batch.
                if (-not $allowedClasses.Contains($methodRow.ClassFqn)) {
                    Write-Host "Canonical leaf evidence contains class outside effective assignment: $($methodRow.ClassFqn) -> $batchId"
                    $problems++
                }
            }
        }
    }

    if ($ExpectedScope -eq 'CANONICAL' -and $null -eq $FocusedExpectation) {
        $problems += Test-ClassFirstExpectedMethods -BatchDir $BatchDir -ActualMethodRows $methodRows -Rows $Rows -FingerprintValues $fingerprintValues -RequireFingerprintHash:$RequireScopeInFingerprint
    }

    if ($null -ne $FocusedExpectation) {
        $expectedMethods = $FocusedExpectation.Methods
        $actualMethods = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($methodRow in $methodRows) {
            if ($methodRow.BatchId -ne $FocusedExpectation.BatchId) {
                Write-Host "Focused leaf-methods row belongs to another batch: $($methodRow.MethodFqn) -> $($methodRow.BatchId)"
                $problems++
            }
            [void]$actualMethods.Add($methodRow.MethodFqn)
        }

        foreach ($planRow in $planRows) {
            if ($planRow.BatchId -ne $FocusedExpectation.BatchId) {
                Write-Host "Focused leaf-plan row belongs to another batch: $($planRow.LeafId) -> $($planRow.BatchId)"
                $problems++
            }
        }

        foreach ($method in $expectedMethods) {
            if (-not $actualMethods.Contains($method)) {
                Write-Host "Focused evidence is missing manifest method: $method"
                $problems++
            }
        }

        foreach ($method in $actualMethods) {
            if (-not $expectedMethods.Contains($method)) {
                Write-Host "Focused evidence contains method outside current manifest: $method"
                $problems++
            }
        }
    }

    $packReportPath = Join-Path $BatchDir 'leaf-pack-report.tsv'
    $classMethodPackEvidenceChecked = $false
    if (Test-Path -LiteralPath $packReportPath) {
        $packReport = @{}
        foreach ($line in @(Get-Content -LiteralPath $packReportPath | Select-Object -Skip 1)) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            $parts = $line -split "`t", 2
            if ($parts.Count -eq 2) { $packReport[$parts[0]] = $parts[1] }
        }
        if ($packReport['mode'] -eq 'pack') {
            # Pack mode обязан держать xUnit class/collection целиком. Отчет
            # runner'а проверяется заново из leaf-methods, а не принимается на веру.
            $splits = Test-LeafPackUnitSplits -MethodRows $methodRows
            if ($splits.ClassSplits -ne 0 -or $splits.UnitSplits -ne 0) {
                Write-Host "Leaf pack splits class/collection units: $BatchDir class=$($splits.ClassSplits) unit=$($splits.UnitSplits)"
                $problems++
            }
            if ($splits.RealSharedCollectionSplits -ne 0) {
                Write-Host "Leaf pack splits real shared fixture collections: $BatchDir real_shared=$($splits.RealSharedCollectionSplits)"
                $problems++
            }
            if (($packReport['class_splits'] -as [int]) -ne $splits.ClassSplits -or ($packReport['unit_splits'] -as [int]) -ne $splits.UnitSplits) {
                Write-Host "Leaf pack report does not match independent split calculation: $BatchDir"
                $problems++
            }
            if ($packReport.ContainsKey('real_shared_collection_splits') -and (($packReport['real_shared_collection_splits'] -as [int]) -ne $splits.RealSharedCollectionSplits)) {
                Write-Host "Leaf pack report does not match real shared collection split calculation: $BatchDir"
                $problems++
            }
            if ($packReport.ContainsKey('scheduling_only_collection_spans') -and (($packReport['scheduling_only_collection_spans'] -as [int]) -ne $splits.SchedulingOnlyCollectionSpans)) {
                Write-Host "Leaf pack report does not match scheduling-only collection span calculation: $BatchDir"
                $problems++
            }
        }
        elseif ($packReport['mode'] -eq 'method-diagnostic') {
            if (-not [string]::IsNullOrWhiteSpace($expectedLeafMode) -and $expectedLeafMode -ne 'method-diagnostic') {
                Write-Host "Leaf pack report mode differs from expected override: $BatchDir"
                $problems++
            }
        }
        elseif ($packReport['mode'] -eq 'class-method-pack') {
            if ($expectedLeafMode -ne 'class-method-pack') {
                Write-Host "Leaf pack report class-method-pack mode is not expected for this batch: $BatchDir"
                $problems++
            }
            else {
                $problems += Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $BatchDir -PlanRows $planRows -MethodRows $methodRows -PackReport $packReport -FingerprintValues $fingerprintValues
                $classMethodPackEvidenceChecked = $true
            }
        }
        elseif (-not [string]::IsNullOrWhiteSpace($packReport['mode'])) {
            Write-Host "Leaf pack report has unknown mode: $BatchDir ($($packReport['mode']))"
            $problems++
        }
    }

    if ($expectedLeafMode -eq 'method-diagnostic') {
        if (-not (Test-Path -LiteralPath $packReportPath)) {
            Write-Host "Method-diagnostic leaf batch has no pack report: $BatchDir"
            $problems++
        }
        if ($planRows.Count -ne $methodRows.Count) {
            Write-Host "Method-diagnostic leaf count differs from method count: $BatchDir ($($planRows.Count) vs $($methodRows.Count))"
            $problems++
        }
        foreach ($row in $planRows) {
            if (($row.MethodCount -as [int]) -ne 1) {
                Write-Host "Method-diagnostic leaf contains more than one method: $($row.LeafId)"
                $problems++
            }
            if ($row.Filter -notmatch '^FullyQualifiedName=[A-Za-z0-9_.+]+$') {
                Write-Host "Method-diagnostic leaf filter is not one exact method: $($row.LeafId)"
                $problems++
            }
        }
    }

    if ($expectedLeafMode -eq 'class-method-pack') {
        if (-not (Test-Path -LiteralPath $packReportPath)) {
            Write-Host "Class-method-pack leaf batch has no pack report: $BatchDir"
            $problems++
        }
        elseif (-not $classMethodPackEvidenceChecked) {
            Write-Host "Class-method-pack leaf batch was not checked with the storage verifier: $BatchDir"
            $problems++
        }
    }

    $openLeaves = 0
    $redLeaves = 0
    $sumTotal = 0
    $sumPassed = 0
    $sumFailed = 0
    $sumNotExecuted = 0
    $physicalTotal = 0

    foreach ($row in $planRows) {
        if (-not (Test-LeafExactFilter -Filter $row.Filter)) {
            Write-Host "Leaf filter is not exact: $($row.LeafId)"
            $problems++
        }

        $leafDir = Join-Path $BatchDir $row.LeafId
        if (-not (Test-Path -LiteralPath $leafDir)) {
            if (-not $AllowIncomplete) {
                Write-Host "Leaf directory is missing: $leafDir"
                $problems++
            }
            $openLeaves++
            continue
        }

        $leafState = Read-StateValue -Path (Join-Path $leafDir 'state.txt')
        $journalRow = Get-LastLeafJournalRow -JournalRows $journalRows -LeafId $row.LeafId

        if ($null -ne $journalRow) {
            $timing = Test-LeafJournalTimingRow -Row $journalRow
            if (-not $timing.Valid) {
                Write-Host "Leaf journal timing is invalid: $leafDir ($($timing.Reason))"
                $problems++
            }
        }

        if ($leafState -in @('COMPLETED_GREEN', 'COMPLETED_RED')) {
            $attemptDir = Get-CurrentAttemptDirectory -BatchDir $leafDir
            $trx = if ([string]::IsNullOrWhiteSpace($attemptDir)) { Join-Path $leafDir 'result.trx' } else { Join-Path $attemptDir 'result.trx' }
            $rootTrx = Join-Path $leafDir 'result.trx'

            if (-not (Test-Path -LiteralPath $rootTrx)) {
                Write-Host "Closed leaf was not promoted to root TRX: $leafDir"
                $problems++
            }
            elseif (-not [string]::IsNullOrWhiteSpace($attemptDir) -and (Test-Path -LiteralPath $trx)) {
                if ((Get-FileHash -LiteralPath $rootTrx -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $trx -Algorithm SHA256).Hash) {
                    Write-Host "Leaf root TRX does not match current attempt TRX: $leafDir"
                    $problems++
                }
            }

            $membership = Test-LeafTrxMembership -TrxPath $trx -MethodRows $methodRows -PlanRow $row
            if (-not $membership.Valid) {
                Write-Host "Closed leaf TRX membership is invalid: $leafDir ($($membership.Reason))"
                $problems++
                continue
            }

            if ($null -eq $journalRow) {
                Write-Host "Closed leaf has no leaf journal row: $leafDir"
                $problems++
            }
            else {
                if (-not (Test-LeafJournalMatchesMembership -JournalRow $journalRow -LeafState $leafState -Membership $membership)) {
                    Write-Host "Leaf journal differs from TRX/state: $leafDir"
                    $problems++
                }
            }

            $sumTotal += $membership.Total
            $sumPassed += $membership.Passed
            $sumFailed += $membership.Failed
            $sumNotExecuted += $membership.NotExecuted
            $physicalTotal += $membership.Total
            if ($leafState -eq 'COMPLETED_RED') {
                $redLeaves++
            }
        }
        elseif ($leafState -eq 'LEAF_TIMED_OUT') {
            $attemptDir = Get-CurrentAttemptDirectory -BatchDir $leafDir
            $rootTrx = Join-Path $leafDir 'result.trx'

            if (Test-Path -LiteralPath $rootTrx) {
                Write-Host "Timed-out leaf promoted an untrusted root TRX: $leafDir"
                $problems++
            }

            if ([string]::IsNullOrWhiteSpace($attemptDir)) {
                Write-Host "Timed-out leaf has no current attempt marker: $leafDir"
                $problems++
            }
            else {
                $attemptState = Read-StateValue -Path (Join-Path $attemptDir 'state.txt')
                if ($attemptState -ne 'LEAF_TIMED_OUT') {
                    Write-Host "Timed-out leaf attempt state differs from leaf state: $attemptDir ($attemptState)"
                    $problems++
                }

                $exitPath = Join-Path $attemptDir 'exit.txt'
                $attemptExit = if (Test-Path -LiteralPath $exitPath) { (Get-Content -LiteralPath $exitPath -First 1).Trim() } else { '' }
                if ($attemptExit -ne '124') {
                    Write-Host "Timed-out leaf attempt does not record timeout exit code 124: $attemptDir ($attemptExit)"
                    $problems++
                }
            }

            if ($null -eq $journalRow) {
                Write-Host "Timed-out leaf has no leaf journal row: $leafDir"
                $problems++
            }
            else {
                # Timeout leaf — это честная красная/blocked единица учёта, а не
                # закрытый TRX. Все ожидаемые строки leaf'а должны быть названы
                # not_executed, result_path остаётся пустым, suite-green невозможен.
                if ($journalRow.State -ne 'LEAF_TIMED_OUT' -or $journalRow.ExitCode -ne '124' -or -not [string]::IsNullOrWhiteSpace($journalRow.ResultPath)) {
                    Write-Host "Timed-out leaf journal is not explicit timeout evidence: $leafDir"
                    $problems++
                }

                if ($journalRow.Total -ne $row.EstimatedRows -or $journalRow.Passed -ne 0 -or $journalRow.Failed -ne 0 -or $journalRow.NotExecuted -ne $row.EstimatedRows) {
                    Write-Host "Timed-out leaf journal does not account all planned rows as not_executed: $leafDir"
                    $problems++
                }
            }

            $sumTotal += $row.EstimatedRows
            $sumNotExecuted += $row.EstimatedRows
            $redLeaves++
        }
        elseif ($leafState -in @('STARTED', 'RUNNING', 'LEAF_TIMED_OUT') -and $AllowIncomplete) {
            $openLeaves++
        }
        else {
            Write-Host "Leaf is not terminal: $leafDir ($leafState)"
            $problems++
            $openLeaves++
        }
    }

    Write-Host ("leaf_batch={0}; leaves={1}; open_leaves={2}; red_leaves={3}; total={4}; passed={5}; failed={6}; not_executed={7}; physical_total={8}" -f (Split-Path -Leaf $BatchDir), $planRows.Count, $openLeaves, $redLeaves, $sumTotal, $sumPassed, $sumFailed, $sumNotExecuted, $physicalTotal)

    if ($openLeaves -gt 0) {
        if (-not $AllowIncomplete -or $batchState -ne 'LEAF_INCOMPLETE') {
            Write-Host "Leaf batch is partial without explicit incomplete state: $BatchDir ($batchState)"
            $problems++
        }
    }
    else {
        $belowMinimum = $minRowsEvidence.Known -and $physicalTotal -lt $minRowsEvidence.Value
        if ($belowMinimum) {
            # Нижний порог сверяется с физическими UnitTestResult из TRX.
            # Ожидаемые строки таймаута и числа из журнала не закрывают недобор.
            Write-Host "Leaf batch has fewer physical TRX result rows than manifest min_rows: $BatchDir ($physicalTotal < $($minRowsEvidence.Value))"
            $problems++
            if ($batchState -ne 'INVALID_TOO_FEW_ROWS') {
                Write-Host "Leaf batch below manifest min_rows is not marked INVALID_TOO_FEW_ROWS: $BatchDir ($batchState)"
                $problems++
            }
        }
        else {
            if ($batchState -notin @('COMPLETED_GREEN', 'COMPLETED_RED')) {
                Write-Host "Leaf batch closed leaves but parent state is not completed: $BatchDir ($batchState)"
                $problems++
            }
        }

        if ($batchState -eq 'COMPLETED_GREEN' -and ($redLeaves -gt 0 -or $sumFailed -gt 0)) {
            Write-Host "Leaf parent green hides red leaves: $BatchDir"
            $problems++
        }
    }

    return $problems
}

function Test-BatchEvidence {
    param(
        [string]$BatchDir,
        [object[]]$JournalRows,
        [object[]]$Rows = @(),
        [object[]]$EffectiveRows = @(),
        [object[]]$Classes = @(),
        [string]$ExpectedScope = '',
        [object]$FocusedExpectation = $null,
        [string]$CurrentBatchesSha256 = '',
        [hashtable]$ExpectedLeafModes = @{},
        [switch]$AllowLegacyFocusedFilterNormalization,
        [switch]$RequireScopeInFingerprint,
        [switch]$AllowIncomplete
    )

    $problems = 0
    if (Test-Path -LiteralPath (Join-Path $BatchDir 'leaf-plan.tsv')) {
        return (Test-LeafBatchEvidence -BatchDir $BatchDir -Rows $Rows -EffectiveRows $EffectiveRows -Classes $Classes -ExpectedScope $ExpectedScope -FocusedExpectation $FocusedExpectation -CurrentBatchesSha256 $CurrentBatchesSha256 -ExpectedLeafModes $ExpectedLeafModes -AllowLegacyFocusedFilterNormalization:$AllowLegacyFocusedFilterNormalization -RequireScopeInFingerprint:$RequireScopeInFingerprint -AllowIncomplete:$AllowIncomplete)
    }

    $batchId = Get-BatchIdFromDirectory -Path $BatchDir
    $minRowsEvidence = Get-BatchMinRowsEvidence -Rows $Rows -BatchId $batchId
    $state = Read-StateValue -Path (Join-Path $BatchDir 'state.txt')
    if ([string]::IsNullOrWhiteSpace($state)) {
        Write-Host "Batch has no state: $BatchDir"
        return 1
    }

    if ($Rows.Count -gt 0 -and -not $minRowsEvidence.Known) {
        # Закрытая batch-only папка тоже проверяется против одной строки
        # manifest; иначе min_rows можно случайно выключить отсутствием строки.
        Write-Host "Batch has no unique manifest min_rows row: $BatchDir (matches=$($minRowsEvidence.Count))"
        $problems++
    }

    if ($null -ne $FocusedExpectation -and $state -ne 'SKIPPED_PLATFORM') {
        Write-Host "Focused evidence must use leaf evidence with leaf-methods binding: $BatchDir"
        $problems++
    }

    if ($state -in @('STARTED', 'RUNNING')) {
        Write-Host "Batch is not terminal: $BatchDir ($state)"
        $problems++
    }

    $journalRow = Get-LastJournalRow -JournalRows $JournalRows -BatchId $batchId
    if ($null -eq $journalRow) {
        Write-Host "Batch has no required journal row: $BatchDir"
        $problems++
    }

    if ($state -in @('COMPLETED_GREEN', 'COMPLETED_RED')) {
        $attemptDir = Get-CurrentAttemptDirectory -BatchDir $BatchDir
        if ([string]::IsNullOrWhiteSpace($attemptDir)) {
            $trx = Join-Path $BatchDir 'result.trx'
        }
        else {
            # При наличии current-attempt verifier читает только эту попытку:
            # старый result.trx в корне больше не может закрыть новый запуск.
            $attemptState = Read-StateValue -Path (Join-Path $attemptDir 'state.txt')
            if ($attemptState -ne $state) {
                Write-Host "Current attempt state differs from batch state: $attemptDir ($attemptState vs $state)"
                $problems++
            }

            $trx = Join-Path $attemptDir 'result.trx'
            $rootTrx = Join-Path $BatchDir 'result.trx'
            if (-not (Test-Path -LiteralPath $rootTrx)) {
                Write-Host "Closed batch was not promoted to root TRX: $BatchDir"
                $problems++
            }
            elseif (Test-Path -LiteralPath $trx) {
                # Hash сравнивается только после проверки наличия attempt TRX.
                # Иначе сама проверка падала бы раньше, чем успевала назвать
                # главную проблему: текущая попытка не дала результата.
                if ((Get-FileHash -LiteralPath $rootTrx -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $trx -Algorithm SHA256).Hash) {
                    Write-Host "Root TRX does not match current attempt TRX: $BatchDir"
                    $problems++
                }
            }
            else {
                Write-Host "Root TRX does not match current attempt TRX: $BatchDir"
                $problems++
            }
        }

        $counts = Read-TrxCounts -Path $trx
        if (-not $counts.Valid) {
            Write-Host "Closed batch has invalid current TRX: $BatchDir ($($counts.Reason))"
            $problems++
        }
        elseif ($null -ne $journalRow) {
            if ($ExpectedScope -eq 'CANONICAL' -and $null -eq $FocusedExpectation) {
                $problems += Test-BatchOnlyClassFirstTrxEvidence -BatchDir $BatchDir -TrxPath $trx -Rows $Rows -RequireEvidence:$RequireScopeInFingerprint
            }

            if ($journalRow.State -ne $state) {
                Write-Host "Journal state differs from batch state: $BatchDir"
                $problems++
            }

            if ([string]::IsNullOrWhiteSpace($journalRow.ResultPath)) {
                Write-Host "Closed batch journal has no result path: $BatchDir"
                $problems++
            }

            if ($journalRow.Total -ne $counts.Total -or $journalRow.Passed -ne $counts.Passed -or $journalRow.Failed -ne $counts.Failed -or $journalRow.NotExecuted -ne $counts.NotExecuted) {
                Write-Host "Journal counts differ from TRX counts: $BatchDir"
                $problems++
            }

            if ($minRowsEvidence.Known -and $counts.Total -lt $minRowsEvidence.Value) {
                # Batch-only режим уже писал INVALID_TOO_FEW_ROWS; verifier
                # отдельно защищает старые закрытые RunRoot от принятия ниже порога.
                Write-Host "Closed batch has fewer physical TRX result rows than manifest min_rows: $BatchDir ($($counts.Total) < $($minRowsEvidence.Value))"
                $problems++
            }

            if ($state -eq 'COMPLETED_GREEN' -and ($counts.Failed -ne 0 -or $journalRow.ExitCode -ne '0')) {
                Write-Host "Green batch has failed rows or nonzero exit: $BatchDir"
                $problems++
            }

            if ($state -eq 'COMPLETED_RED' -and ($counts.Failed -eq 0 -and $journalRow.ExitCode -eq '0')) {
                Write-Host "Red batch has no failed rows and zero exit: $BatchDir"
                $problems++
            }
        }
    }
    elseif ($state -eq 'EMPTY_VERIFIED') {
        if ($ExpectedScope -ne 'CANONICAL' -or -not (Test-IsDefaultBatchesFile -Path $BatchesFile)) {
            Write-Host "Empty residual state is allowed only for the canonical default batches.tsv manifest: $BatchDir"
            $problems++
        }
        else {
            $problems += Test-EmptyResidualEvidence -BatchDir $BatchDir -JournalRow $journalRow -Rows $Rows
        }
    }
    elseif ($state -eq 'SKIPPED_PLATFORM') {
        if ($null -ne $journalRow) {
            if ($journalRow.State -ne 'SKIPPED_PLATFORM' -or $journalRow.Total -le 0 -or $journalRow.Passed -ne 0 -or $journalRow.Failed -ne 0 -or $journalRow.NotExecuted -ne $journalRow.Total -or [string]::IsNullOrWhiteSpace($journalRow.PlatformReason) -or -not [string]::IsNullOrWhiteSpace($journalRow.ResultPath)) {
                Write-Host "Platform skip is not explicit and count-complete: $BatchDir"
                $problems++
            }

            if ($null -ne $FocusedExpectation) {
                $sourceRows = @($Rows | Where-Object { $_.BatchId -eq $batchId })
                if ($sourceRows.Count -ne 1) {
                    Write-Host "Focused platform skip has no unique manifest row: $BatchDir"
                    $problems++
                }
                elseif ($journalRow.Total -ne $sourceRows[0].MinRows) {
                    # Focused platform-skip не имеет TRX/leaf-methods, поэтому
                    # единственная честная физическая мощность берётся из
                    # текущего focused manifest min_rows, а не из старого лога.
                    Write-Host "Focused platform skip count differs from manifest min_rows: $BatchDir"
                    $problems++
                }
            }
        }

        if (Test-Path -LiteralPath (Join-Path $BatchDir 'result.trx')) {
            Write-Host "Platform skip has fake root TRX: $BatchDir"
            $problems++
        }

        $attemptDir = Get-CurrentAttemptDirectory -BatchDir $BatchDir
        if (-not [string]::IsNullOrWhiteSpace($attemptDir)) {
            $attemptState = Read-StateValue -Path (Join-Path $attemptDir 'state.txt')
            if ($attemptState -ne 'SKIPPED_PLATFORM' -or -not (Test-Path -LiteralPath (Join-Path $attemptDir 'platform-skip.txt'))) {
                Write-Host "Platform skip attempt is missing its explicit reason: $attemptDir"
                $problems++
            }
        }
    }
    elseif ($state -in @('INVALID_RESULT', 'INVALID_NO_TRX', 'INVALID_TOO_FEW_ROWS', 'TIMED_OUT_BATCH', 'RETRY_REQUIRED_TIMEOUT', 'SPLIT_REQUIRED')) {
        if ($state -eq 'SPLIT_REQUIRED') {
            $splitProblems = Test-SplitPlanEvidence -BatchDir $BatchDir -BatchId $batchId -Rows $Rows -EffectiveRows $EffectiveRows -Classes $Classes
            $script:SplitPlanValidationProblems += $splitProblems
            $problems += $splitProblems
        }

        Write-Host "Unclosed batch in checked run root: $BatchDir ($state)"
        $problems++
    }
    elseif ($state -notin @('STARTED', 'RUNNING')) {
        Write-Host "Batch has unknown terminal state: $BatchDir ($state)"
        $problems++
    }

    return $problems
}

function Test-RunRootEvidence {
    param(
        [string]$Path,
        [object[]]$Rows,
        [object[]]$EffectiveRows,
        [object[]]$Classes,
        [string]$ExpectedScope = '',
        [object]$FocusedExpectedBatches = $null,
        [hashtable]$ExpectedLeafModes = @{},
        [switch]$AllowIncomplete
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return 0
    }

    $problems = 0
    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "Run root not found: $Path"
        return 1
    }

    $actualScope = Get-RunScopeValue -Path $Path
    $scopeIsRecorded = -not [string]::IsNullOrWhiteSpace($actualScope)
    $scopeValues = @{}
    if ($scopeIsRecorded) {
        $scopeValues = Read-KeyValueTsv -Path (Join-Path $Path 'verification-scope.tsv')
    }
    if ($scopeIsRecorded -and -not [string]::IsNullOrWhiteSpace($ExpectedScope) -and $actualScope -ne $ExpectedScope) {
        Write-Host "Run root verification scope mismatch: $Path ($actualScope vs $ExpectedScope)"
        $problems++
    }
    if ($scopeIsRecorded -and $null -ne $FocusedExpectedBatches) {
        $scopeManifestSha = if ($scopeValues.ContainsKey('batches_tsv_sha256')) { $scopeValues['batches_tsv_sha256'] } else { '' }
        if ($scopeManifestSha -ne $FocusedExpectedBatches.ManifestSha256) {
            Write-Host "Focused run root manifest hash mismatch: $Path ($scopeManifestSha vs $($FocusedExpectedBatches.ManifestSha256))"
            $problems++
        }
        if (-not $scopeValues.ContainsKey('full_suite_claim_allowed') -or $scopeValues['full_suite_claim_allowed'] -ne 'false') {
            Write-Host "Focused run root does not explicitly forbid full-suite green claim: $Path"
            $problems++
        }
        if (-not $scopeValues.ContainsKey('focused_mode') -or $scopeValues['focused_mode'] -ne 'true') {
            Write-Host "Focused run root does not record focused_mode=true: $Path"
            $problems++
        }
    }

    $rootState = Read-StateValue -Path (Join-Path $Path 'state.txt')
    if ($rootState -ne 'FINISHED') {
        if ($AllowIncomplete -and $rootState -eq 'INCOMPLETE') {
            Write-Host "Run root is explicitly incomplete and accepted only because -AllowIncomplete was set: $Path"
        }
        else {
            Write-Host "Run root is not terminal: $Path ($rootState)"
            $problems++
        }
    }

    $journalPath = Join-Path $Path 'journal.tsv'
    if (-not (Test-Path -LiteralPath $journalPath) -or (Get-Item -LiteralPath $journalPath).Length -eq 0) {
        Write-Host "Run root has no journal: $Path"
        return ($problems + 1)
    }

    $journalRows = @(Read-JournalRows -Path $journalPath)
    if ($journalRows.Count -eq 0) {
        Write-Host "Run root journal is empty: $Path"
        $problems++
    }

    foreach ($row in $journalRows) {
        if ($row.State -eq 'CORRUPT_JOURNAL_ROW') {
            Write-Host "Run root journal has corrupt row: line $($row.LineNumber)"
            $problems++
        }
    }
    $problems += Test-JournalTerminalDuplicateConflicts -JournalRows $journalRows
    if ($scopeIsRecorded) {
        $problems += Test-RunInvocationHistoryForRunRoot -Path $Path -ScopeValues $scopeValues -JournalRows $journalRows
    }

    $batchDirs = @(Get-ChildItem -LiteralPath $Path -Directory -Filter 'batch-*' -ErrorAction SilentlyContinue)
    $allowIncompleteForChildren = $AllowIncomplete -and $rootState -eq 'INCOMPLETE'
    if ($null -ne $FocusedExpectedBatches) {
        $problems += Test-FocusedRunBatchSet -BatchDirs $batchDirs -JournalRows $journalRows -FocusedExpectedBatches $FocusedExpectedBatches
    }

    $allowLegacyFocusedFilterNormalization = $null -ne $FocusedExpectedBatches -and -not $scopeIsRecorded
    foreach ($batchDir in $batchDirs) {
        $focusedExpectation = $null
        if ($null -ne $FocusedExpectedBatches) {
            $batchId = Get-BatchIdFromDirectory -Path $batchDir.FullName
            if ($FocusedExpectedBatches.ByBatchId.ContainsKey($batchId)) {
                $focusedExpectation = $FocusedExpectedBatches.ByBatchId[$batchId]
            }
        }

        $problems += Test-BatchEvidence -BatchDir $batchDir.FullName -JournalRows $journalRows -Rows $Rows -EffectiveRows $EffectiveRows -Classes $Classes -ExpectedScope $ExpectedScope -FocusedExpectation $focusedExpectation -CurrentBatchesSha256 $(if ($null -ne $FocusedExpectedBatches) { $FocusedExpectedBatches.ManifestSha256 } else { '' }) -ExpectedLeafModes $ExpectedLeafModes -AllowLegacyFocusedFilterNormalization:$allowLegacyFocusedFilterNormalization -RequireScopeInFingerprint:$scopeIsRecorded -AllowIncomplete:$allowIncompleteForChildren
    }

    foreach ($journalRow in $journalRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.BatchId) }) {
        $matchingDir = @($batchDirs | Where-Object { (Get-BatchIdFromDirectory -Path $_.FullName) -eq $journalRow.BatchId })
        if ($matchingDir.Count -eq 0) {
            Write-Host "Journal row has no batch directory: $($journalRow.BatchId)"
            $problems++
        }
    }

    if ($Rows.Count -gt 0 -and $null -eq $FocusedExpectedBatches -and -not $AllowIncomplete) {
        $expectedIds = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($row in $Rows) {
            [void]$expectedIds.Add($row.BatchId)
        }

        $journalIds = New-Object 'System.Collections.Generic.HashSet[string]'
        foreach ($journalRow in $journalRows | Where-Object { -not [string]::IsNullOrWhiteSpace($_.BatchId) }) {
            [void]$journalIds.Add($journalRow.BatchId)
        }

        foreach ($expectedId in $expectedIds) {
            if (-not $journalIds.Contains($expectedId)) {
                Write-Host "Canonical run root is missing batch journal row: $expectedId"
                $problems++
            }
        }

        foreach ($journalId in $journalIds) {
            if (-not $expectedIds.Contains($journalId)) {
                Write-Host "Canonical run root has journal row outside current manifest: $journalId"
                $problems++
            }
        }
    }

    return $problems
}

function Test-StaleRootCannotCloseCurrentAttempt {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-verifier-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'
    $attemptDir = Join-Path $batchDir 'attempt-0001'

    try {
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'STARTED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'result.trx') -Value '<TestRun><Results><UnitTestResult outcome="Passed"/></Results></TestRun>' -Encoding UTF8
        $journalRows = @([pscustomobject]@{
            BatchId = 'self-check'
            State = 'COMPLETED_GREEN'
            ExitCode = '0'
            Total = 1
            Passed = 1
            Failed = 0
            NotExecuted = 0
            ResultPath = Join-Path $batchDir 'result.trx'
            Platform = 'any'
            PlatformReason = 'self-check'
        })

        # Самопроверка моделирует старый root result.trx и новую незавершенную
        # attempt-попытку. Правильный verifier обязан увидеть проблему.
        $problems = & { Test-BatchEvidence -BatchDir $batchDir -JournalRows $journalRows } 6>$null
        if ($problems -le 0) {
            Write-Host "Attempt self-check failed: stale root TRX closed a new incomplete attempt."
            return 1
        }

        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-InconsistentExecutedCounterRejected {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-trx-counter-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $trxPath = Join-Path $tempRoot 'well-formed-wrong-executed.trx'

    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

        # XML синтаксически исправен и содержит настоящую Passed-строку, но
        # executed=0 противоречит ей. Такая смысловая ложь обязана быть красной.
        $xml = '<TestRun><Results><UnitTestResult outcome="Passed" testName="verifier-self-check" /></Results><ResultSummary><Counters total="1" executed="0" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" /></ResultSummary></TestRun>'
        Set-Content -LiteralPath $trxPath -Value $xml -Encoding UTF8
        $result = Read-TrxCounts -Path $trxPath

        if ($result.Valid -or $result.Reason -ne 'counter_executed_outcome_mismatch') {
            Write-Host "TRX counter self-check failed: inconsistent executed was not rejected exactly ($($result.Reason))."
            return 1
        }

        Write-Host "TRX counter self-check: well-formed wrong executed rejected ($($result.Reason))."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-MinimalValidTrx {
    param(
        [string]$ClassName = 'Example.Tests',
        [string]$MethodName = 'One',
        [string]$Outcome = 'Passed',
        [int]$RowCount = 1,
        [string]$Message = ''
    )

    if ($RowCount -lt 1) {
        throw 'RowCount must be positive.'
    }

    $executed = if ($Outcome -eq 'NotExecuted') { 0 } else { $RowCount }
    $passed = if ($Outcome -eq 'Passed') { $RowCount } else { 0 }
    $failed = if ($Outcome -eq 'Failed') { $RowCount } else { 0 }
    $notExecuted = if ($Outcome -eq 'NotExecuted') { $RowCount } else { 0 }
    $results = New-Object System.Text.StringBuilder
    $definitions = New-Object System.Text.StringBuilder
    for ($index = 1; $index -le $RowCount; $index++) {
        $testId = "test-$index"
        $executionId = "execution-$index"
        if ([string]::IsNullOrWhiteSpace($Message)) {
            [void]$results.AppendLine("    <UnitTestResult executionId=""$executionId"" testId=""$testId"" testName=""$ClassName.$MethodName"" outcome=""$Outcome"" />")
        }
        else {
            $escapedMessage = [System.Security.SecurityElement]::Escape($Message)
            # ErrorInfo нужен для legacy Qwen self-check: именно так VSTest
            # записал runtime SkipException в старом TRX, который теперь
            # нормализуется только по точному FQN и префиксу.
            [void]$results.AppendLine("    <UnitTestResult executionId=""$executionId"" testId=""$testId"" testName=""$ClassName.$MethodName"" outcome=""$Outcome"">")
            [void]$results.AppendLine("      <Output><ErrorInfo><Message>$escapedMessage</Message></ErrorInfo></Output>")
            [void]$results.AppendLine('    </UnitTestResult>')
        }
        [void]$definitions.AppendLine("    <UnitTest name=""$ClassName.$MethodName"" id=""$testId"">")
        [void]$definitions.AppendLine("      <Execution id=""$executionId"" />")
        [void]$definitions.AppendLine("      <TestMethod className=""$ClassName"" name=""$MethodName"" />")
        [void]$definitions.AppendLine('    </UnitTest>')
    }

    return @"
<TestRun>
  <Results>
$($results.ToString().TrimEnd())
  </Results>
  <TestDefinitions>
$($definitions.ToString().TrimEnd())
  </TestDefinitions>
  <ResultSummary>
    <Counters total="$RowCount" executed="$executed" passed="$passed" failed="$failed" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="$notExecuted" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
  </ResultSummary>
</TestRun>
"@
}

function New-LeafTimingSelfCheckRoot {
    param(
        [string]$Root,
        [string]$StartedUtc,
        [string]$FinishedUtc,
        [string]$Seconds
    )

    $batchDir = Join-Path $Root 'batch-01-self-check'
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null

    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value 'FINISHED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t$StartedUtc`t$FinishedUtc`t$Seconds`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t001`t1`t1`tFullyQualifiedName=Example.Tests.One" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`nExample.Tests.One`tself-check`tleaf-001`t00001`t1`tExample.Tests`tclass:Example.Tests" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t$StartedUtc`t$FinishedUtc`t$Seconds`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=Example.Tests.One" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx) -Encoding UTF8
    Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')
}

function Test-LeafTimingSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-timing-selfcheck-" + [guid]::NewGuid().ToString('N'))

    try {
        $validRoot = Join-Path $tempRoot 'valid'
        New-LeafTimingSelfCheckRoot -Root $validRoot -StartedUtc '2026-08-27T00:00:00Z' -FinishedUtc '2026-08-27T00:00:02Z' -Seconds '2'
        $validProblems = Test-RunRootEvidence -Path $validRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($validProblems -ne 0) {
            Write-Host "Leaf timing self-check failed: valid synthetic timing was rejected ($validProblems)."
            return 1
        }

        # Эти corrupt fixtures доказывают fail-closed: verifier не даёт плохому
        # timestamp или seconds стать publishable evidence.
        $badTimestampRoot = Join-Path $tempRoot 'bad-timestamp'
        New-LeafTimingSelfCheckRoot -Root $badTimestampRoot -StartedUtc '2026-08-27 00:00:00Z' -FinishedUtc '2026-08-27T00:00:02Z' -Seconds '2'
        $badTimestampProblems = Test-RunRootEvidence -Path $badTimestampRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($badTimestampProblems -le 0) {
            Write-Host 'Leaf timing self-check failed: invalid timestamp was accepted.'
            return 1
        }

        $badSecondsRoot = Join-Path $tempRoot 'bad-seconds'
        New-LeafTimingSelfCheckRoot -Root $badSecondsRoot -StartedUtc '2026-08-27T00:00:00Z' -FinishedUtc '2026-08-27T00:00:02Z' -Seconds '2x'
        $badSecondsProblems = Test-RunRootEvidence -Path $badSecondsRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($badSecondsProblems -le 0) {
            Write-Host 'Leaf timing self-check failed: invalid seconds were accepted.'
            return 1
        }

        Write-Host 'Leaf timing self-check: invalid timestamp and invalid seconds rejected.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LeafTheoryExtraMembershipSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-theory-extra-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $trxPath = Join-Path $tempRoot 'theory-extra.trx'

    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
        Set-Content -LiteralPath $trxPath -Value (New-MinimalValidTrx -ClassName 'Example.Tests' -MethodName 'Expected' -Outcome 'Passed' -RowCount 3) -Encoding UTF8
        $methodRows = @([pscustomobject]@{ MethodFqn = 'Example.Tests.Expected'; BatchId = 'self-check'; LeafId = 'leaf-001'; EstimatedRows = 1 })
        $planRow = [pscustomobject]@{ LeafId = 'leaf-001'; EstimatedRows = 1; Filter = 'FullyQualifiedName=Example.Tests.Expected' }
        $result = Test-LeafTrxMembership -TrxPath $trxPath -MethodRows $methodRows -PlanRow $planRow

        if (-not $result.Valid -or $result.Total -ne 3 -or $result.Passed -ne 3 -or $result.Failed -ne 0 -or $result.NotExecuted -ne 0) {
            Write-Host "Leaf theory extra self-check failed: result valid=$($result.Valid) reason=$($result.Reason) total=$($result.Total)."
            return 1
        }

        Write-Host 'Leaf theory extra self-check: honest extra physical rows for the same method accepted.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LeafIncompleteAllowSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-incomplete-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'
    $closedLeaf = Join-Path $batchDir 'leaf-001'
    $openLeaf = Join-Path $batchDir 'leaf-002'
    $attemptDir = Join-Path $closedLeaf 'attempt-0001'

    try {
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $openLeaf 'attempt-0001') | Out-Null

        Set-Content -LiteralPath (Join-Path $tempRoot 'state.txt') -Value 'INCOMPLETE' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tLEAF_INCOMPLETE`tleaf-pack`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'LEAF_INCOMPLETE' -Encoding UTF8
        Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t001`t1`t1`tFullyQualifiedName=Example.Tests.One`nleaf-002`tself-check`t002`t1`t1`tFullyQualifiedName=Example.Tests.Two" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`nExample.Tests.One`tself-check`tleaf-001`t00001`t1`nExample.Tests.Two`tself-check`tleaf-002`t00002`t1" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=Example.Tests.One" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $closedLeaf 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $closedLeaf 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx) -Encoding UTF8
        Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $closedLeaf 'result.trx')
        Set-Content -LiteralPath (Join-Path $openLeaf 'state.txt') -Value 'STARTED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $openLeaf 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $openLeaf 'attempt-0001\state.txt') -Value 'STARTED' -Encoding UTF8

        $ordinaryProblems = Test-RunRootEvidence -Path $tempRoot -Rows @() -EffectiveRows @() -Classes @()
        $allowedProblems = Test-RunRootEvidence -Path $tempRoot -Rows @() -EffectiveRows @() -Classes @() -AllowIncomplete
        if ($ordinaryProblems -le 0 -or $allowedProblems -ne 0) {
            Write-Host "Leaf incomplete self-check failed: ordinary=$ordinaryProblems allowed=$allowedProblems"
            return 1
        }

        Write-Host "Leaf incomplete self-check: ordinary verifier rejects, -AllowIncomplete accepts."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Set-SelfConsistentFingerprint {
    param([string]$Path)

    $baseText = "key`tvalue`nself_check`ttrue`nverifier_sha256`tself-check`nrunsettings_sha256`tself-check`n"
    $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($baseText)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $computed = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }

    [System.IO.File]::WriteAllText($Path, $baseText + "fingerprint_sha256`t$computed`n", [System.Text.UTF8Encoding]::new($false))
}

function Set-SelfConsistentFingerprintRows {
    param(
        [string]$Path,
        [string[]]$Rows
    )

    $baseText = "key`tvalue`n" + (($Rows | Where-Object { $_ -notlike "fingerprint_sha256`t*" }) -join "`n") + "`n"
    $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes($baseText)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $computed = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }

    [System.IO.File]::WriteAllText($Path, $baseText + "fingerprint_sha256`t$computed`n", [System.Text.UTF8Encoding]::new($false))
}

function New-CanonicalLeafClassAssignmentSelfCheckRoot {
    param(
        [string]$Root,
        [string]$EvidenceClass
    )

    $batchId = 'class-assignment'
    $methodName = 'One'
    $methodFqn = "$EvidenceClass.$methodName"
    $batchDir = Join-Path $Root "batch-01-$batchId"
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null

    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value 'FINISHED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`t$batchId`tClass assignment self-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`ttrue" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`t$batchId`t001`t1`t1`tFullyQualifiedName=$methodFqn" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`n$methodFqn`t$batchId`tleaf-001`t00001`t1`t$EvidenceClass`tclass:$EvidenceClass" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`t$batchId`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=$methodFqn" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName $EvidenceClass -MethodName $methodName) -Encoding UTF8
    Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')
}

function Test-CanonicalLeafClassAssignmentSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-class-assignment-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
    $batchId = 'class-assignment'
    $allowedClass = 'Example.AllowedClassTests'
    $lookalikeClass = 'Example.AllowedClassTestsLookalike'
    $foreignClass = 'Example.ForeignClassTests'
    $row = [pscustomobject]@{
        BatchId = $batchId
        Dll = $dll
        Filter = "FullyQualifiedName~$allowedClass."
        HumanName = 'Class assignment self-check'
        MinRows = 1
        SplitOnRepeatTimeout = 'true'
        Platform = 'any'
        PlatformReason = 'self-check'
    }
    $effectiveRow = [pscustomobject]@{
        BatchId = $batchId
        Dll = $dll
        RawFilter = $row.Filter
        EffectiveFilter = $row.Filter
        PriorTerms = 0
    }
    $classes = @(
        [pscustomobject]@{ Dll = $dll; FullyQualifiedName = $allowedClass; RelativePath = 'Tests/AllowedClassTests.cs' },
        [pscustomobject]@{ Dll = $dll; FullyQualifiedName = $lookalikeClass; RelativePath = 'Tests/AllowedClassTestsLookalike.cs' },
        [pscustomobject]@{ Dll = $dll; FullyQualifiedName = $foreignClass; RelativePath = 'Tests/ForeignClassTests.cs' }
    )

    try {
        $validRoot = Join-Path $tempRoot 'valid'
        New-CanonicalLeafClassAssignmentSelfCheckRoot -Root $validRoot -EvidenceClass $allowedClass
        $validProblems = Test-RunRootEvidence -Path $validRoot -Rows @($row) -EffectiveRows @($effectiveRow) -Classes $classes -ExpectedScope 'CANONICAL'
        if ($validProblems -ne 0) {
            Write-Host "Canonical leaf class assignment self-check failed: valid class was rejected ($validProblems)."
            return 1
        }

        $foreignRoot = Join-Path $tempRoot 'foreign-class'
        New-CanonicalLeafClassAssignmentSelfCheckRoot -Root $foreignRoot -EvidenceClass $foreignClass
        $foreignProblems = Test-RunRootEvidence -Path $foreignRoot -Rows @($row) -EffectiveRows @($effectiveRow) -Classes $classes -ExpectedScope 'CANONICAL'
        if ($foreignProblems -le 0) {
            Write-Host 'Canonical leaf class assignment self-check failed: foreign runtime class was accepted.'
            return 1
        }

        $lookalikeRoot = Join-Path $tempRoot 'lookalike-class'
        New-CanonicalLeafClassAssignmentSelfCheckRoot -Root $lookalikeRoot -EvidenceClass $lookalikeClass
        $lookalikeProblems = Test-RunRootEvidence -Path $lookalikeRoot -Rows @($row) -EffectiveRows @($effectiveRow) -Classes $classes -ExpectedScope 'CANONICAL'
        if ($lookalikeProblems -le 0) {
            Write-Host 'Canonical leaf class assignment self-check failed: lookalike class matched a class-boundary filter.'
            return 1
        }

        Write-Host 'Canonical leaf class assignment self-check: runtime class outside effective batch assignment and lookalike class rejected.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-ClassFirstMethodEvidenceBatch {
    param(
        [string]$Root,
        [object[]]$ExpectedRows,
        [object[]]$ActualRows,
        [object[]]$RawRows = @(),
        [string]$FingerprintHashMode = 'current'
    )

    $batchId = 'class-first-methods'
    $batchDir = Join-Path $Root "batch-01-$batchId"
    New-Item -ItemType Directory -Force -Path $batchDir | Out-Null
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    if ($RawRows.Count -eq 0) {
        $RawRows = $ExpectedRows
    }

    $expectedLines = New-Object System.Collections.Generic.List[string]
    [void]$expectedLines.Add("method_fqn`testimated_rows`tclass_fqn`tbatch_id`tdll`tunit_key`tcollection_name`tcollection_kind")
    foreach ($row in $ExpectedRows) {
        [void]$expectedLines.Add("$($row.Method)`t$($row.Rows)`t$($row.Class)`t$batchId`tTests\bin\Release\net9.0\LlmFromScratch.Tests.dll`tclass:$($row.Class)`t`tnone")
    }
    $expectedPath = Join-Path $batchDir 'class-first-methods.tsv'
    [System.IO.File]::WriteAllText($expectedPath, ($expectedLines -join "`n") + "`n", [System.Text.UTF8Encoding]::new($false))
    $rawLines = New-Object System.Collections.Generic.List[string]
    [void]$rawLines.Add('The following Tests are available:')
    foreach ($row in $RawRows) {
        for ($caseIndex = 1; $caseIndex -le [int]$row.Rows; $caseIndex++) {
            if ([int]$row.Rows -gt 1) {
                [void]$rawLines.Add("    $($row.Method)($caseIndex)")
            }
            else {
                [void]$rawLines.Add("    $($row.Method)")
            }
        }
    }
    [System.IO.File]::WriteAllText((Join-Path $batchDir 'class-first-list-tests.stdout.log'), ($rawLines -join "`n") + "`n", [System.Text.UTF8Encoding]::new($false))

    $methodLines = New-Object System.Collections.Generic.List[string]
    $planLines = New-Object System.Collections.Generic.List[string]
    $journalLines = New-Object System.Collections.Generic.List[string]
    [void]$methodLines.Add("method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`tcollection_name`tcollection_kind")
    [void]$planLines.Add("leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter")
    [void]$journalLines.Add("leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter")

    $index = 0
    foreach ($row in $ActualRows) {
        $index++
        $leafId = 'leaf-{0:000}' -f $index
        $leafDir = Join-Path $batchDir $leafId
        $attemptDir = Join-Path $leafDir 'attempt-0001'
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        $filter = "FullyQualifiedName=$($row.Method)"
        [void]$planLines.Add("$leafId`t$batchId`t$('{0:000}' -f $index)`t1`t$($row.Rows)`t$filter")
        [void]$methodLines.Add("$($row.Method)`t$batchId`t$leafId`t$('{0:00000}' -f $index)`t$($row.Rows)`t$($row.Class)`tclass:$($row.Class)`t`tnone")
        [void]$journalLines.Add("$leafId`t$batchId`t2026-08-30T00:00:00Z`t2026-08-30T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t$($row.Rows)`t$($row.Rows)`t0`t0`tresult.trx`t1`t$($row.Rows)`t$filter")
        Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        $methodName = $row.Method.Substring($row.Class.Length + 1)
        Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName $row.Class -MethodName $methodName -RowCount $row.Rows) -Encoding UTF8
        Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')
    }

    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value ($planLines -join "`n") -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value ($methodLines -join "`n") -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value ($journalLines -join "`n") -Encoding UTF8

    $expectedHash = Get-FileSha256Lower -Path $expectedPath
    $rawHash = Get-FileSha256Lower -Path (Join-Path $batchDir 'class-first-list-tests.stdout.log')
    if ($FingerprintHashMode -eq 'mutated') {
        $expectedHash = '0' * 64
    }
    Set-SelfConsistentFingerprintRows -Path (Join-Path $batchDir 'leaf-fingerprint.tsv') -Rows @(
        'verification_scope	CANONICAL',
        'full_suite_claim_allowed	true',
        'verifier_sha256	self-check',
        'runsettings_sha256	self-check',
        "class_first_methods_sha256	$expectedHash",
        "class_first_list_tests_sha256	$rawHash"
    )

    return $batchDir
}

function Test-ClassFirstMethodEvidenceSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-class-first-methods-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $apiClass = 'Example.ApiDashboardCatalogTests'
    $lookalikeClass = 'Example.ApiDashboardCatalogTestsLookalike'
    $globalRuntimeClass = 'ContinuousLearningModelServerCompositionTests'
    $apiMethod = [pscustomobject]@{ Method = 'Example.ApiDashboardCatalogTests.ApiCatalogProvider_BuildCatalog_CoversEveryXiigenExtensionContractMethod'; Class = $apiClass; Rows = 1 }
    $theoryMethod = [pscustomobject]@{ Method = 'Example.ApiDashboardCatalogTests.TheoryRows'; Class = $apiClass; Rows = 2 }
    $globalRuntimeMethod = [pscustomobject]@{ Method = 'ContinuousLearningModelServerCompositionTests.AcceptsRuntimeClassWithoutNamespace'; Class = $globalRuntimeClass; Rows = 1 }
    $lookalikeMethod = [pscustomobject]@{ Method = 'Example.ApiDashboardCatalogTestsLookalike.LookalikeShouldStayOut'; Class = $lookalikeClass; Rows = 1 }
    $manifestRow = [pscustomobject]@{
        BatchId = 'class-first-methods'
        Dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
        Filter = "FullyQualifiedName~$apiClass.|FullyQualifiedName~$globalRuntimeClass."
        MinRows = 1
    }
    $effectiveManifestRow = [pscustomobject]@{
        BatchId = $manifestRow.BatchId
        Dll = $manifestRow.Dll
        EffectiveFilter = $manifestRow.Filter
    }
    $staticApiClass = [pscustomobject]@{
        Dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
        FullyQualifiedName = $apiClass
    }
    $misleadingStaticClass = [pscustomobject]@{
        Dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
        FullyQualifiedName = "Example.$globalRuntimeClass"
    }

    try {
        $validBatch = New-ClassFirstMethodEvidenceBatch -Root (Join-Path $tempRoot 'valid') -ExpectedRows @($apiMethod, $theoryMethod, $globalRuntimeMethod) -ActualRows @($apiMethod, $theoryMethod, $globalRuntimeMethod)
        $validProblems = Test-LeafBatchEvidence -BatchDir $validBatch -Rows @($manifestRow) -EffectiveRows @($effectiveManifestRow) -Classes @($staticApiClass, $misleadingStaticClass) -ExpectedScope 'CANONICAL' -RequireScopeInFingerprint
        if ($validProblems -ne 0) {
            Write-Host "Class-first method evidence self-check failed: valid evidence was rejected ($validProblems)."
            return 1
        }

        $missingBatch = New-ClassFirstMethodEvidenceBatch -Root (Join-Path $tempRoot 'missing') -ExpectedRows @($apiMethod, $theoryMethod) -ActualRows @($apiMethod)
        $missingProblems = Test-LeafBatchEvidence -BatchDir $missingBatch -Rows @($manifestRow) -EffectiveRows @() -Classes @() -ExpectedScope 'CANONICAL' -RequireScopeInFingerprint
        if ($missingProblems -le 0) {
            Write-Host 'Class-first method evidence self-check failed: omitted expected method was accepted.'
            return 1
        }

        $missingDerivedBatch = New-ClassFirstMethodEvidenceBatch -Root (Join-Path $tempRoot 'missing-derived') -ExpectedRows @($apiMethod) -ActualRows @($apiMethod) -RawRows @($apiMethod, $theoryMethod)
        $missingDerivedProblems = Test-LeafBatchEvidence -BatchDir $missingDerivedBatch -Rows @($manifestRow) -EffectiveRows @() -Classes @() -ExpectedScope 'CANONICAL' -RequireScopeInFingerprint
        if ($missingDerivedProblems -ne 1) {
            Write-Host "Class-first method evidence self-check failed: method omitted from both derived files was not caught exactly by raw discovery (problems=$missingDerivedProblems)."
            return 1
        }

        $foreignBatch = New-ClassFirstMethodEvidenceBatch -Root (Join-Path $tempRoot 'foreign') -ExpectedRows @($apiMethod, $theoryMethod) -ActualRows @($apiMethod, $theoryMethod, $lookalikeMethod)
        $foreignProblems = Test-LeafBatchEvidence -BatchDir $foreignBatch -Rows @($manifestRow) -EffectiveRows @() -Classes @() -ExpectedScope 'CANONICAL' -RequireScopeInFingerprint
        if ($foreignProblems -le 0) {
            Write-Host 'Class-first method evidence self-check failed: lookalike foreign method was accepted.'
            return 1
        }

        $hashBatch = New-ClassFirstMethodEvidenceBatch -Root (Join-Path $tempRoot 'hash') -ExpectedRows @($apiMethod, $theoryMethod) -ActualRows @($apiMethod, $theoryMethod) -FingerprintHashMode 'mutated'
        $hashProblems = Test-LeafBatchEvidence -BatchDir $hashBatch -Rows @($manifestRow) -EffectiveRows @() -Classes @() -ExpectedScope 'CANONICAL' -RequireScopeInFingerprint
        if ($hashProblems -le 0) {
            Write-Host 'Class-first method evidence self-check failed: mutated class-first hash was accepted.'
            return 1
        }

        Write-Host 'Class-first method evidence self-check: valid method set with namespace-free runtime class accepted; omitted method, omitted derived method, lookalike foreign method and hash mutation rejected.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-EmptyResidualEvidenceBatch {
    param(
        [string]$Root,
        [string]$DllPath,
        [string]$BatchId = 'residual',
        [string]$EvidenceFilter = '*',
        [string[]]$RawMethods = @('Example.NamedTests.Known'),
        [string]$EvidenceMutation = ''
    )

    $batchDir = Join-Path $Root "batch-02-$BatchId"
    New-Item -ItemType Directory -Force -Path $batchDir | Out-Null
    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value 'FINISHED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'EMPTY_VERIFIED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n02`t$BatchId`tEmpty residual self-check`t2026-08-31T00:00:00Z`t2026-08-31T00:00:00Z`t0`tEMPTY_VERIFIED`tempty-residual`t0`t0`t0`t0`tempty-residual.tsv`tany`tself-check`ttrue" -Encoding UTF8

    $rawLines = @('The following Tests are available:') + ($RawMethods | ForEach-Object { "    $_" })
    Set-Content -LiteralPath (Join-Path $batchDir 'class-first-list-tests.stdout.log') -Value ($rawLines -join "`n") -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'class-first-methods.tsv') -Value "method_fqn`testimated_rows`tclass_fqn`tbatch_id`tdll`tunit_key`tcollection_name`tcollection_kind" -Encoding UTF8

    $rawHash = Get-FileSha256Lower -Path (Join-Path $batchDir 'class-first-list-tests.stdout.log')
    $methodsHash = Get-FileSha256Lower -Path (Join-Path $batchDir 'class-first-methods.tsv')
    Set-Content -LiteralPath (Join-Path $batchDir 'class-first-origin.tsv') -Value @(
        "key`tvalue",
        "selection_semantics`tclass_first_v1",
        "source`tlist_tests_unfiltered",
        "test_dll`t$DllPath",
        "test_dll_sha256`t$(Get-FileSha256Lower -Path $DllPath)",
        "batches_tsv_sha256`t$(Get-FileSha256Lower -Path $BatchesFile)",
        "list_tests_stdout`tclass-first-list-tests.stdout.log",
        "list_tests_stdout_sha256`t$rawHash",
        "method_assignments_sha256`tself-check-method-assignments",
        "cache_identity_sha256`tself-check-cache-identity",
        "selected_method_count`t0",
        "selected_estimated_rows`t0"
    ) -Encoding UTF8

    $originHash = Get-FileSha256Lower -Path (Join-Path $batchDir 'class-first-origin.tsv')
    $runtimeMethodCount = @($RawMethods | Select-Object -Unique).Count
    $runtimeRows = @($RawMethods).Count
    $runnerPath = Join-Path $RepoRoot 'tools\test-batches\run-test-batches.sh'
    $runnerHash = if (Test-Path -LiteralPath $runnerPath) { Get-FileSha256Lower -Path $runnerPath } else { 'missing-runner' }
    $head = Get-CurrentGitHeadForEvidence
    if ([string]::IsNullOrWhiteSpace($head)) { $head = 'self-check-head-unavailable' }

    $evidenceRows = @(
        "key`tvalue",
        "empty_semantics`tclass_first_empty_residual_v1",
        "head`t$head",
        "batch_id`t$BatchId",
        "filter`t$EvidenceFilter",
        "test_dll`t$DllPath",
        "test_dll_identity`t$(Get-NormalizedPath -Path $DllPath)",
        "test_dll_sha256`t$(Get-FileSha256Lower -Path $DllPath)",
        "batches_tsv_sha256`t$(Get-FileSha256Lower -Path $BatchesFile)",
        "runner_sha256`t$runnerHash",
        "verifier_sha256`t$(Get-FileSha256Lower -Path $PSCommandPath)",
        "class_first_methods_sha256`t$methodsHash",
        "class_first_origin_sha256`t$originHash",
        "class_first_list_tests_sha256`t$rawHash",
        "method_assignments_sha256`tself-check-method-assignments",
        "cache_identity_sha256`tself-check-cache-identity",
        "total_runtime_methods`t$runtimeMethodCount",
        "total_runtime_rows`t$runtimeRows",
        "residual_method_count`t0",
        "residual_estimated_rows`t0"
    )
    if ($EvidenceMutation -eq 'runner_hash') {
        $evidenceRows = $evidenceRows | ForEach-Object { if ($_ -like 'runner_sha256*') { "runner_sha256`t$('0' * 64)" } else { $_ } }
    }
    if ($EvidenceMutation -eq 'raw_hash') {
        $evidenceRows = $evidenceRows | ForEach-Object { if ($_ -like 'class_first_list_tests_sha256*') { "class_first_list_tests_sha256`t$('0' * 64)" } else { $_ } }
    }
    Set-Content -LiteralPath (Join-Path $batchDir 'empty-residual.tsv') -Value ($evidenceRows -join "`n") -Encoding UTF8

    return $batchDir
}

function Test-EmptyResidualEvidenceSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-empty-residual-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $dllPath = Join-Path $tempRoot 'Fake.Empty.Tests.dll'
    $rows = @()
    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
        Set-Content -LiteralPath $dllPath -Value 'fake-dll-for-empty-residual' -Encoding UTF8
        # Позитивная ветка empty-residual доказывает только основной список
        # групп. Если сам verifier запущен в focused-режиме, временно берём
        # основной TSV, а ниже отдельно проверяем отказ произвольного списка.
        $BatchesFile = Join-Path $PSScriptRoot 'batches.tsv'
        $rows = @(
            [pscustomobject]@{ BatchId = 'named'; Dll = $dllPath; Filter = 'FullyQualifiedName~Example.NamedTests.'; MinRows = 1 },
            [pscustomobject]@{ BatchId = 'residual'; Dll = $dllPath; Filter = '*'; MinRows = 1 }
        )

        $validRoot = Join-Path $tempRoot 'valid'
        $validBatch = New-EmptyResidualEvidenceBatch -Root $validRoot -DllPath $dllPath
        $validJournal = @(Read-JournalRows -Path (Join-Path $validRoot 'journal.tsv'))
        $validProblems = Test-BatchEvidence -BatchDir $validBatch -JournalRows $validJournal -Rows $rows -ExpectedScope 'CANONICAL'
        if ($validProblems -ne 0) {
            Write-Host "Empty residual self-check failed: valid empty evidence was rejected ($validProblems)."
            return 1
        }

        $missingOtherProblems = Test-RunRootEvidence -Path $validRoot -Rows $rows -EffectiveRows @() -Classes @() -ExpectedScope 'CANONICAL'
        if ($missingOtherProblems -le 0) {
            Write-Host 'Empty residual self-check failed: full run root forgot missing earlier batch.'
            return 1
        }

        $namedRows = @(
            [pscustomobject]@{ BatchId = 'named-empty'; Dll = $dllPath; Filter = 'FullyQualifiedName~Example.MissingTests.'; MinRows = 1 },
            [pscustomobject]@{ BatchId = 'named'; Dll = $dllPath; Filter = 'FullyQualifiedName~Example.NamedTests.'; MinRows = 1 },
            [pscustomobject]@{ BatchId = 'residual'; Dll = $dllPath; Filter = '*'; MinRows = 1 }
        )
        $namedRoot = Join-Path $tempRoot 'named-empty'
        $namedBatch = New-EmptyResidualEvidenceBatch -Root $namedRoot -DllPath $dllPath -BatchId 'named-empty' -EvidenceFilter 'FullyQualifiedName~Example.MissingTests.'
        $namedProblems = Test-BatchEvidence -BatchDir $namedBatch -JournalRows @(Read-JournalRows -Path (Join-Path $namedRoot 'journal.tsv')) -Rows $namedRows -ExpectedScope 'CANONICAL'
        if ($namedProblems -le 0) {
            Write-Host 'Empty residual self-check failed: named empty batch was accepted.'
            return 1
        }

        $rawEmptyRoot = Join-Path $tempRoot 'raw-empty'
        $rawEmptyBatch = New-EmptyResidualEvidenceBatch -Root $rawEmptyRoot -DllPath $dllPath -RawMethods @()
        $rawEmptyProblems = Test-BatchEvidence -BatchDir $rawEmptyBatch -JournalRows @(Read-JournalRows -Path (Join-Path $rawEmptyRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'CANONICAL'
        if ($rawEmptyProblems -le 0) {
            Write-Host 'Empty residual self-check failed: empty raw ListTests was accepted.'
            return 1
        }

        $residualMethodRoot = Join-Path $tempRoot 'residual-method'
        $residualMethodBatch = New-EmptyResidualEvidenceBatch -Root $residualMethodRoot -DllPath $dllPath -RawMethods @('Example.NamedTests.Known', 'Example.ResidualTests.NewMethod')
        $residualMethodProblems = Test-BatchEvidence -BatchDir $residualMethodBatch -JournalRows @(Read-JournalRows -Path (Join-Path $residualMethodRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'CANONICAL'
        if ($residualMethodProblems -le 0) {
            Write-Host 'Empty residual self-check failed: residual method in raw discovery was accepted.'
            return 1
        }

        $tamperedRoot = Join-Path $tempRoot 'tampered'
        $tamperedBatch = New-EmptyResidualEvidenceBatch -Root $tamperedRoot -DllPath $dllPath -EvidenceMutation 'runner_hash'
        $tamperedProblems = Test-BatchEvidence -BatchDir $tamperedBatch -JournalRows @(Read-JournalRows -Path (Join-Path $tamperedRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'CANONICAL'
        if ($tamperedProblems -le 0) {
            Write-Host 'Empty residual self-check failed: tampered runner hash was accepted.'
            return 1
        }

        $rawHashRoot = Join-Path $tempRoot 'raw-hash'
        $rawHashBatch = New-EmptyResidualEvidenceBatch -Root $rawHashRoot -DllPath $dllPath -EvidenceMutation 'raw_hash'
        $rawHashProblems = Test-BatchEvidence -BatchDir $rawHashBatch -JournalRows @(Read-JournalRows -Path (Join-Path $rawHashRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'CANONICAL'
        if ($rawHashProblems -le 0) {
            Write-Host 'Empty residual self-check failed: tampered raw ListTests hash was accepted.'
            return 1
        }

        $oldBatchesFile = $BatchesFile
        try {
            $BatchesFile = Join-Path $tempRoot 'custom-batches.tsv'
            Set-Content -LiteralPath $BatchesFile -Value "custom empty residual manifest" -Encoding UTF8
            $customRoot = Join-Path $tempRoot 'custom-manifest'
            $customBatch = New-EmptyResidualEvidenceBatch -Root $customRoot -DllPath $dllPath
            $customProblems = Test-BatchEvidence -BatchDir $customBatch -JournalRows @(Read-JournalRows -Path (Join-Path $customRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'CANONICAL'
            if ($customProblems -le 0) {
                Write-Host 'Empty residual self-check failed: custom manifest empty residual was accepted.'
                return 1
            }

            $focusedProblems = Test-BatchEvidence -BatchDir $customBatch -JournalRows @(Read-JournalRows -Path (Join-Path $customRoot 'journal.tsv')) -Rows $rows -ExpectedScope 'FOCUSED'
            if ($focusedProblems -le 0) {
                Write-Host 'Empty residual self-check failed: focused empty residual was accepted.'
                return 1
            }
        }
        finally {
            $BatchesFile = $oldBatchesFile
        }

        Write-Host 'Empty residual evidence self-check: valid empty residual accepted; named empty, raw empty, residual method, custom/focused manifest and tampered identity rejected; full run still requires other batches.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-FocusedBindingSelfCheckRoot {
    param(
        [string]$Root,
        [string]$ManifestPath,
        [string]$EvidenceMethod,
        [string]$FingerprintManifestSha256,
        [string]$FingerprintSelectedFilter
    )

    $batchId = 'focused-binding'
    $batchDir = Join-Path $Root "batch-01-$batchId"
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'
    $manifestHash = Get-FileSha256Lower -Path $ManifestPath
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value 'FINISHED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $Root 'verification-scope.tsv') -Value "key`tvalue`nscope`tFOCUSED`nfull_suite_claim_allowed`tfalse`nbatches_file`t$ManifestPath`nbatches_tsv_sha256`t$manifestHash`nfocused_mode`ttrue" -Encoding UTF8
    [System.IO.File]::WriteAllText(
        (Join-Path $Root 'run-invocations.tsv'),
        $script:RunInvocationHistoryHeader + "`n" + (New-RunInvocationHistoryLine -Order 1 -PreviousPayload '' -VerificationScope 'FOCUSED' -SelectedBatches $batchId -EffectiveLeafModes "$batchId`:pack" -BatchesTsvSha256 $manifestHash -FocusedMode 'true') + "`n",
        [System.Text.UTF8Encoding]::new($false))
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`t$batchId`tFocused binding self-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-SelfConsistentFingerprintRows -Path (Join-Path $batchDir 'leaf-fingerprint.tsv') -Rows @(
        'head	self-check',
        'verification_scope	FOCUSED',
        'full_suite_claim_allowed	false',
        "batches_tsv_sha256	$FingerprintManifestSha256",
        "selected_filter	$FingerprintSelectedFilter",
        'verifier_sha256	self-check',
        'runsettings_sha256	self-check'
    )
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`t$batchId`t001`t1`t1`tFullyQualifiedName=$EvidenceMethod" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`n$EvidenceMethod`t$batchId`tleaf-001`t00001`t1`tExample.Tests`tclass:Example.Tests" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`t$batchId`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t1`tFullyQualifiedName=$EvidenceMethod" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
    $methodName = ($EvidenceMethod -split '\.')[-1]
    $className = $EvidenceMethod.Substring(0, $EvidenceMethod.Length - $methodName.Length - 1)
    Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName $className -MethodName $methodName) -Encoding UTF8
    Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')
}

function Test-FocusedManifestBindingSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-focused-binding-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $methodA = 'Example.Tests.One'
    $methodB = 'Example.Tests.Two'

    try {
        $cases = @(
            [pscustomobject]@{ Name = 'matching'; ManifestMethod = $methodA; EvidenceMethod = $methodA; HashMode = 'match'; FilterMethod = $methodA; ShouldReject = $false },
            [pscustomobject]@{ Name = 'swapped_manifest'; ManifestMethod = $methodB; EvidenceMethod = $methodA; HashMode = 'match'; FilterMethod = $methodB; ShouldReject = $true },
            [pscustomobject]@{ Name = 'changed_manifest_hash'; ManifestMethod = $methodA; EvidenceMethod = $methodA; HashMode = 'wrong'; FilterMethod = $methodA; ShouldReject = $true },
            [pscustomobject]@{ Name = 'changed_selected_filter'; ManifestMethod = $methodA; EvidenceMethod = $methodA; HashMode = 'match'; FilterMethod = $methodB; ShouldReject = $true }
        )

        foreach ($case in $cases) {
            $caseRoot = Join-Path $tempRoot $case.Name
            New-Item -ItemType Directory -Force -Path $caseRoot | Out-Null
            $manifestPath = Join-Path $caseRoot 'focused-batches.tsv'
            Set-Content -LiteralPath $manifestPath -Value "01`tfocused-binding`tFocused binding self-check`tTests/bin/Release/net9.0/LlmFromScratch.Tests.dll`tFullyQualifiedName=$($case.ManifestMethod)`t1`tfalse`tany`tself-check" -Encoding UTF8
            $manifestRows = @(Read-BatchRows -Path $manifestPath)
            $expected = New-FocusedExpectedBatchMap -Rows $manifestRows -ManifestPath $manifestPath
            $fingerprintHash = if ($case.HashMode -eq 'match') { $expected.ManifestSha256 } else { '0000000000000000000000000000000000000000000000000000000000000000' }
            New-FocusedBindingSelfCheckRoot -Root $caseRoot -ManifestPath $manifestPath -EvidenceMethod $case.EvidenceMethod -FingerprintManifestSha256 $fingerprintHash -FingerprintSelectedFilter "FullyQualifiedName=$($case.FilterMethod)"

            $problems = Test-RunRootEvidence -Path $caseRoot -Rows $manifestRows -EffectiveRows @() -Classes @() -ExpectedScope 'FOCUSED' -FocusedExpectedBatches $expected
            if ($case.ShouldReject -and $problems -le 0) {
                Write-Host "Focused manifest binding self-check failed: $($case.Name) was accepted."
                return 1
            }
            if (-not $case.ShouldReject -and $problems -ne 0) {
                Write-Host "Focused manifest binding self-check failed: matching evidence was rejected with $problems problems."
                return 1
            }
        }

        Write-Host "Focused manifest binding self-check: swapped manifest, hash change, and selected-filter change rejected."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LeafWrongMembershipSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-membership-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'

    try {
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        Set-Content -LiteralPath (Join-Path $tempRoot 'state.txt') -Value 'FINISHED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t001`t1`t2`tFullyQualifiedName=Example.Tests.Expected" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`nExample.Tests.Expected`tself-check`tleaf-001`t00001`t2`tExample.Tests`tclass:Example.Tests" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t2`tFullyQualifiedName=Example.Tests.Expected" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        # Reviewer exploit: completed leaf выглядит правдоподобно по Counters,
        # но testId указывает на Foreign, не на Expected. Verifier обязан
        # отвергнуть это как invalid evidence, а не поверить журналу.
        Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName 'Example.Tests' -MethodName 'Foreign') -Encoding UTF8
        Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')

        $problems = Test-RunRootEvidence -Path $tempRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($problems -le 0) {
            Write-Host "Leaf wrong-membership self-check failed: verifier accepted a foreign single-row TRX."
            return 1
        }

        Write-Host "Leaf wrong-membership self-check: foreign single-row TRX rejected."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LeafMissingMembershipSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-missing-row-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'
    $leafDir = Join-Path $batchDir 'leaf-001'
    $attemptDir = Join-Path $leafDir 'attempt-0001'

    try {
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        Set-Content -LiteralPath (Join-Path $tempRoot 'state.txt') -Value 'FINISHED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t001`t1`t2`tFullyQualifiedName=Example.Tests.Expected" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`nExample.Tests.Expected`tself-check`tleaf-001`t00001`t2`tExample.Tests`tclass:Example.Tests" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`t1`t2`tFullyQualifiedName=Example.Tests.Expected" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        # Здесь метод правильный, но физических строк меньше, чем обещали
        # leaf-plan/leaf-methods. Это отдельный missing-row exploit без чужого
        # метода, и он тоже должен закрываться как invalid evidence.
        Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName 'Example.Tests' -MethodName 'Expected') -Encoding UTF8
        Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')

        $problems = Test-RunRootEvidence -Path $tempRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($problems -le 0) {
            Write-Host "Leaf missing-row self-check failed: verifier accepted too few physical TRX rows."
            return 1
        }

        Write-Host "Leaf missing-row self-check: too few physical TRX rows rejected."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-MinRowsManifestEvidenceRow {
    param(
        [string]$BatchId = 'self-check',
        [int]$MinRows = 1
    )

    return [pscustomobject]@{
        Order = '01'
        BatchId = $BatchId
        HumanName = 'Self check'
        Dll = 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll'
        Filter = 'FullyQualifiedName=Example.Tests.One'
        MinRows = $MinRows
        SplitOnRepeatTimeout = 'false'
        Platform = 'any'
        PlatformReason = 'self-check'
        LineNumber = 1
    }
}

function New-MinRowsLeaf {
    param(
        [string]$Method,
        [string]$Outcome = 'Passed',
        [int]$RowCount = 1,
        [string]$State = ''
    )

    if ([string]::IsNullOrWhiteSpace($State)) {
        $State = if ($Outcome -eq 'Failed') { 'COMPLETED_RED' } else { 'COMPLETED_GREEN' }
    }

    return [pscustomobject]@{
        Method = $Method
        Outcome = $Outcome
        RowCount = $RowCount
        State = $State
    }
}

function New-MinRowsLeafRunRoot {
    param(
        [string]$Root,
        [object[]]$Leaves,
        [string]$BatchState = 'COMPLETED_GREEN',
        [string]$RootState = 'FINISHED',
        [string]$JournalState = $BatchState
    )

    $batchDir = Join-Path $Root 'batch-01-self-check'
    New-Item -ItemType Directory -Force -Path $batchDir | Out-Null
    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value $RootState -Encoding UTF8
    Set-SelfConsistentFingerprint -Path (Join-Path $batchDir 'leaf-fingerprint.tsv')
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value $BatchState -Encoding UTF8

    $planRows = New-Object System.Collections.Generic.List[string]
    $methodRows = New-Object System.Collections.Generic.List[string]
    $journalRows = New-Object System.Collections.Generic.List[string]
    [void]$planRows.Add("leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter")
    [void]$methodRows.Add("method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`tcollection_name`tcollection_kind")
    [void]$journalRows.Add("leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter")

    $total = 0
    $passed = 0
    $failed = 0
    $notExecuted = 0
    $index = 0
    foreach ($leaf in $Leaves) {
        $index++
        $leafId = 'leaf-{0:000}' -f $index
        $method = [string]$leaf.Method
        $className = $method -replace '\.[^.]+$', ''
        $methodName = $method -replace '^.*\.', ''
        $leafDir = Join-Path $batchDir $leafId
        $attemptDir = Join-Path $leafDir 'attempt-0001'
        $filter = "FullyQualifiedName=$method"

        [void]$planRows.Add("$leafId`tself-check`t$('{0:000}' -f $index)`t1`t1`t$filter")
        [void]$methodRows.Add("$method`tself-check`t$leafId`t$('{0:00000}' -f $index)`t1`t$className`tclass:$className`t`tnone")
        New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
        Set-Content -LiteralPath (Join-Path $leafDir 'state.txt') -Value $leaf.State -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $leafDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value $leaf.State -Encoding UTF8

        if ($leaf.State -in @('COMPLETED_GREEN', 'COMPLETED_RED')) {
            $rowTotal = [int]$leaf.RowCount
            $rowPassed = if ($leaf.Outcome -eq 'Passed') { $rowTotal } else { 0 }
            $rowFailed = if ($leaf.Outcome -eq 'Failed') { $rowTotal } else { 0 }
            $rowNotExecuted = if ($leaf.Outcome -eq 'NotExecuted') { $rowTotal } else { 0 }
            $exitCode = if ($leaf.Outcome -eq 'Failed') { '1' } else { '0' }
            $trx = New-MinimalValidTrx -ClassName $className -MethodName $methodName -Outcome $leaf.Outcome -RowCount $rowTotal
            Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value $trx -Encoding UTF8
            Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $leafDir 'result.trx')
            [void]$journalRows.Add("$leafId`tself-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`t$($leaf.State)`t$exitCode`t$rowTotal`t$rowPassed`t$rowFailed`t$rowNotExecuted`tresult.trx`t1`t1`t$filter")
            $total += $rowTotal
            $passed += $rowPassed
            $failed += $rowFailed
            $notExecuted += $rowNotExecuted
        }
    }

    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-plan.tsv') -Value ($planRows -join "`n") -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-methods.tsv') -Value ($methodRows -join "`n") -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'leaf-journal.tsv') -Value ($journalRows -join "`n") -Encoding UTF8

    $rootExit = if ($JournalState -eq 'COMPLETED_RED') { 'leaf-pack' } else { 'leaf-pack' }
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`t$JournalState`t$rootExit`t$total`t$passed`t$failed`t$notExecuted`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
}

function New-MinRowsBatchOnlyRunRoot {
    param(
        [string]$Root,
        [int]$RowCount,
        [string]$Outcome = 'Passed',
        [string]$BatchState = 'COMPLETED_GREEN'
    )

    $batchDir = Join-Path $Root 'batch-01-self-check'
    $attemptDir = Join-Path $batchDir 'attempt-0001'
    New-Item -ItemType Directory -Force -Path $attemptDir | Out-Null
    $passed = if ($Outcome -eq 'Passed') { $RowCount } else { 0 }
    $failed = if ($Outcome -eq 'Failed') { $RowCount } else { 0 }
    $notExecuted = if ($Outcome -eq 'NotExecuted') { $RowCount } else { 0 }
    $exitCode = if ($Outcome -eq 'Failed') { '1' } else { '0' }

    Set-Content -LiteralPath (Join-Path $Root 'state.txt') -Value 'FINISHED' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value $BatchState -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $batchDir 'current-attempt.txt') -Value 'attempt-0001' -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'state.txt') -Value $BatchState -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $attemptDir 'result.trx') -Value (New-MinimalValidTrx -ClassName 'Example.Tests' -MethodName 'BatchOnly' -Outcome $Outcome -RowCount $RowCount) -Encoding UTF8
    Copy-Item -LiteralPath (Join-Path $attemptDir 'result.trx') -Destination (Join-Path $batchDir 'result.trx')
    Set-Content -LiteralPath (Join-Path $Root 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`t$BatchState`t$exitCode`t$RowCount`t$passed`t$failed`t$notExecuted`tresult.trx`tany`tself-check`tfalse" -Encoding UTF8
}

function Test-MinRowsEvidenceSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-min-rows-selfcheck-" + [guid]::NewGuid().ToString('N'))

    try {
        $exactRoot = Join-Path $tempRoot 'exact'
        New-MinRowsLeafRunRoot -Root $exactRoot -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.ExactOne'),
            (New-MinRowsLeaf -Method 'Example.Tests.ExactTwo')
        )
        $exactProblems = Test-RunRootEvidence -Path $exactRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2)) -EffectiveRows @() -Classes @()
        if ($exactProblems -ne 0) {
            Write-Host "Min rows self-check failed: exact minimum was rejected (problems=$exactProblems)."
            return 1
        }

        $batchOnlyExactRoot = Join-Path $tempRoot 'batch-only-exact'
        New-MinRowsBatchOnlyRunRoot -Root $batchOnlyExactRoot -RowCount 2
        $batchOnlyExactProblems = Test-RunRootEvidence -Path $batchOnlyExactRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2)) -EffectiveRows @() -Classes @()
        if ($batchOnlyExactProblems -ne 0) {
            Write-Host "Min rows self-check failed: batch-only exact minimum was rejected (problems=$batchOnlyExactProblems)."
            return 1
        }

        $batchOnlyBelowRoot = Join-Path $tempRoot 'batch-only-below'
        New-MinRowsBatchOnlyRunRoot -Root $batchOnlyBelowRoot -RowCount 2
        $batchOnlyBelowProblems = Test-RunRootEvidence -Path $batchOnlyBelowRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @()
        if ($batchOnlyBelowProblems -le 0) {
            Write-Host 'Min rows self-check failed: batch-only root TRX below manifest minimum was accepted.'
            return 1
        }

        $belowGreenRoot = Join-Path $tempRoot 'below-green'
        New-MinRowsLeafRunRoot -Root $belowGreenRoot -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.GreenOne'),
            (New-MinRowsLeaf -Method 'Example.Tests.GreenTwo')
        )
        $belowGreenProblems = Test-RunRootEvidence -Path $belowGreenRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @()
        $belowGreenAllowedProblems = Test-RunRootEvidence -Path $belowGreenRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @() -AllowIncomplete
        if ($belowGreenProblems -le 0 -or $belowGreenAllowedProblems -le 0) {
            Write-Host "Min rows self-check failed: below-minimum green leaf-pack was accepted (ordinary=$belowGreenProblems allow=$belowGreenAllowedProblems)."
            return 1
        }

        $inflatedRootJournalRoot = Join-Path $tempRoot 'inflated-root-journal'
        New-MinRowsLeafRunRoot -Root $inflatedRootJournalRoot -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.InflatedOne'),
            (New-MinRowsLeaf -Method 'Example.Tests.InflatedTwo')
        )
        # Верхний журнал нарочно завышен до трех строк. Leaf-journal и TRX
        # остаются честными с двумя строками, поэтому порог обязан читать TRX.
        Set-Content -LiteralPath (Join-Path $inflatedRootJournalRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`tleaf-pack`t3`t3`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
        $inflatedRootJournalProblems = Test-RunRootEvidence -Path $inflatedRootJournalRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @()
        if ($inflatedRootJournalProblems -le 0) {
            Write-Host 'Min rows self-check failed: inflated root journal hid a physical TRX shortfall.'
            return 1
        }

        $belowRedRoot = Join-Path $tempRoot 'below-red'
        New-MinRowsLeafRunRoot -Root $belowRedRoot -BatchState 'COMPLETED_RED' -JournalState 'COMPLETED_RED' -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.RedOne'),
            (New-MinRowsLeaf -Method 'Example.Tests.RedFail' -Outcome 'Failed')
        )
        $belowRedProblems = Test-RunRootEvidence -Path $belowRedRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @()
        if ($belowRedProblems -le 0) {
            Write-Host 'Min rows self-check failed: below-minimum red leaf-pack was accepted.'
            return 1
        }

        $extraRowsRoot = Join-Path $tempRoot 'extra-rows'
        New-MinRowsLeafRunRoot -Root $extraRowsRoot -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.ThreeRows' -RowCount 3)
        )
        $extraRowsProblems = Test-RunRootEvidence -Path $extraRowsRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 3)) -EffectiveRows @() -Classes @()
        if ($extraRowsProblems -ne 0) {
            Write-Host "Min rows self-check failed: real extra theory rows were rejected (problems=$extraRowsProblems)."
            return 1
        }

        $skipRoot = Join-Path $tempRoot 'honest-skip'
        New-MinRowsLeafRunRoot -Root $skipRoot -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.SkipOne' -Outcome 'NotExecuted'),
            (New-MinRowsLeaf -Method 'Example.Tests.PassOne')
        )
        $skipProblems = Test-RunRootEvidence -Path $skipRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2)) -EffectiveRows @() -Classes @()
        $skipCounts = Get-RunRootCounts -Path $skipRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2))
        if ($skipProblems -ne 0 -or $skipCounts.Total -ne 2 -or $skipCounts.Passed -ne 1 -or $skipCounts.NotExecuted -ne 1) {
            Write-Host "Min rows self-check failed: explicit NotExecuted row was counted as passed or rejected (problems=$skipProblems total=$($skipCounts.Total) passed=$($skipCounts.Passed) not_executed=$($skipCounts.NotExecuted))."
            return 1
        }

        $partialRoot = Join-Path $tempRoot 'partial'
        New-MinRowsLeafRunRoot -Root $partialRoot -RootState 'INCOMPLETE' -BatchState 'LEAF_INCOMPLETE' -JournalState 'LEAF_INCOMPLETE' -Leaves @(
            (New-MinRowsLeaf -Method 'Example.Tests.ResumeOne'),
            (New-MinRowsLeaf -Method 'Example.Tests.ResumeTwo' -State 'STARTED')
        )
        $partialOrdinaryProblems = Test-RunRootEvidence -Path $partialRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2)) -EffectiveRows @() -Classes @()
        $partialAllowedProblems = Test-RunRootEvidence -Path $partialRoot -Rows @((New-MinRowsManifestEvidenceRow -MinRows 2)) -EffectiveRows @() -Classes @() -AllowIncomplete
        if ($partialOrdinaryProblems -le 0 -or $partialAllowedProblems -ne 0) {
            Write-Host "Min rows self-check failed: partial resume state was not handled honestly (ordinary=$partialOrdinaryProblems allow=$partialAllowedProblems)."
            return 1
        }

        $missingRowsProblems = Test-RunRootEvidence -Path $exactRoot -Rows @((New-MinRowsManifestEvidenceRow -BatchId 'different-batch' -MinRows 2)) -EffectiveRows @() -Classes @()
        $duplicateRowsProblems = Test-RunRootEvidence -Path $exactRoot -Rows @(
            (New-MinRowsManifestEvidenceRow -MinRows 2),
            (New-MinRowsManifestEvidenceRow -MinRows 2)
        ) -EffectiveRows @() -Classes @()
        if ($missingRowsProblems -le 0 -or $duplicateRowsProblems -le 0) {
            Write-Host "Min rows self-check failed: missing or duplicate manifest min_rows row was accepted (missing=$missingRowsProblems duplicate=$duplicateRowsProblems)."
            return 1
        }

        Write-Host 'Min rows self-check: exact minimum accepted for leaf-pack and batch-only; below-minimum green/red, old green below floor, inflated journal, missing row and duplicate row rejected; extra theory rows, explicit skip and partial resume handled.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-FingerprintSelfConsistencySelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-leaf-fingerprint-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $fingerprintPath = Join-Path $tempRoot 'leaf-fingerprint.tsv'

    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
        Set-SelfConsistentFingerprint -Path $fingerprintPath
        $valid = Test-LeafFingerprintSelfConsistent -Path $fingerprintPath
        if (-not $valid.Valid) {
            Write-Host "Fingerprint self-check failed: freshly written fingerprint was rejected ($($valid.Reason))."
            return 1
        }

        $text = Get-Content -LiteralPath $fingerprintPath -Raw
        $text = $text.Replace("verifier_sha256`tself-check", "verifier_sha256`tmutated-self-check")
        [System.IO.File]::WriteAllText($fingerprintPath, $text, [System.Text.UTF8Encoding]::new($false))
        $tampered = Test-LeafFingerprintSelfConsistent -Path $fingerprintPath
        if ($tampered.Valid) {
            Write-Host "Fingerprint self-check failed: verifier hash metadata mutation was accepted."
            return 1
        }

        Write-Host "Fingerprint self-check: verifier hash metadata mutation rejected."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-DesignConversionDpoStorageClassMethodPackSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-storage-method-pack-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $repoRoot = Join-Path $tempRoot 'repo'
    $sourceDir = Join-Path $repoRoot 'Tests'
    $batchDir = Join-Path $tempRoot 'batch-08-design-conversion-dpo-storage'
    $sourceRel = 'Tests/DesignConversionDpoStorageTests.cs'
    $exactPath = Join-Path $batchDir 'leaf-source-exact-bindings.tsv'
    $oversizedPath = Join-Path $batchDir 'leaf-oversized-units.tsv'

    try {
        New-Item -ItemType Directory -Force -Path $sourceDir, $batchDir | Out-Null
        [System.IO.File]::WriteAllText(
            (Join-Path $repoRoot $sourceRel),
            "public sealed partial class DesignConversionDpoStorageTests { }`npublic sealed partial class DesignConversionDpoStorageTests { public void Helper() { } }`n",
            [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::WriteAllText(
            $exactPath,
            "class_fqn`tsource_paths`tunit_key`tcollection_name`tcollection_kind`tdeclaration_count`tbinding_kind`nDesignConversionDpoStorageTests`t$sourceRel`tclass:DesignConversionDpoStorageTests`t`tnone`t2`texact_declaration`n",
            [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::WriteAllText(
            $oversizedPath,
            "unit_key`testimated_rows`tmethod_count`tunit_kind`treason`nmethod:DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows`t75`t1`tnone`tunit_exceeds_leaf_hard_cap`n",
            [System.Text.UTF8Encoding]::new($false))

        $planRows = @(
            [pscustomobject]@{ LeafId = 'leaf-001'; BatchId = 'design-conversion-dpo-storage'; LeafOrder = '001'; MethodCount = 2; EstimatedRows = 3; Filter = 'FullyQualifiedName=DesignConversionDpoStorageTests.FastOne|FullyQualifiedName=DesignConversionDpoStorageTests.FastTwo' },
            [pscustomobject]@{ LeafId = 'leaf-002'; BatchId = 'design-conversion-dpo-storage'; LeafOrder = '002'; MethodCount = 1; EstimatedRows = 75; Filter = 'FullyQualifiedName=DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows' },
            [pscustomobject]@{ LeafId = 'leaf-003'; BatchId = 'design-conversion-dpo-storage'; LeafOrder = '003'; MethodCount = 1; EstimatedRows = 1; Filter = 'FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite' },
            [pscustomobject]@{ LeafId = 'leaf-004'; BatchId = 'design-conversion-dpo-storage'; LeafOrder = '004'; MethodCount = 2; EstimatedRows = 4; Filter = 'FullyQualifiedName=Example.OtherStorageTests.First|FullyQualifiedName=Example.OtherStorageTests.Second' }
        )
        $methodRows = @(
            [pscustomobject]@{ MethodFqn = 'DesignConversionDpoStorageTests.FastOne'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-001'; MethodOrder = '00001'; EstimatedRows = 2; ClassFqn = 'DesignConversionDpoStorageTests'; UnitKey = 'method:DesignConversionDpoStorageTests.FastOne'; CollectionName = ''; CollectionKind = 'none' },
            [pscustomobject]@{ MethodFqn = 'DesignConversionDpoStorageTests.FastTwo'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-001'; MethodOrder = '00002'; EstimatedRows = 1; ClassFqn = 'DesignConversionDpoStorageTests'; UnitKey = 'method:DesignConversionDpoStorageTests.FastTwo'; CollectionName = ''; CollectionKind = 'none' },
            [pscustomobject]@{ MethodFqn = 'DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-002'; MethodOrder = '00003'; EstimatedRows = 75; ClassFqn = 'DesignConversionDpoStorageTests'; UnitKey = 'method:DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows'; CollectionName = ''; CollectionKind = 'none' },
            [pscustomobject]@{ MethodFqn = 'DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-003'; MethodOrder = '00004'; EstimatedRows = 1; ClassFqn = 'DesignConversionDpoStorageTests'; UnitKey = 'method:DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite'; CollectionName = ''; CollectionKind = 'none' },
            [pscustomobject]@{ MethodFqn = 'Example.OtherStorageTests.First'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-004'; MethodOrder = '00005'; EstimatedRows = 2; ClassFqn = 'Example.OtherStorageTests'; UnitKey = 'class:Example.OtherStorageTests'; CollectionName = ''; CollectionKind = 'none' },
            [pscustomobject]@{ MethodFqn = 'Example.OtherStorageTests.Second'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-004'; MethodOrder = '00006'; EstimatedRows = 2; ClassFqn = 'Example.OtherStorageTests'; UnitKey = 'class:Example.OtherStorageTests'; CollectionName = ''; CollectionKind = 'none' }
        )
        $packReport = @{
            mode = 'class-method-pack'
            hard_cap_rows = '60'
            target_rows = '38'
            class_splits = '1'
            unit_splits = '0'
            largest_unit_key = 'method:DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows'
            method_pack_class = 'DesignConversionDpoStorageTests'
            method_pack_method_count = '4'
            method_pack_estimated_rows = '79'
            method_pack_slow_method = 'DesignConversionDpoStorageTests.LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite'
            method_pack_slow_leaf_count = '1'
        }
        $fingerprint = @{
            source_exact_bindings_sha256 = Get-FileSha256Lower -Path $exactPath
            leaf_hard_cap_rows = '60'
            leaf_target_rows = '38'
        }

        $positive = Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $batchDir -PlanRows $planRows -MethodRows $methodRows -PackReport $packReport -FingerprintValues $fingerprint -RepositoryRoot $repoRoot
        if ($positive -ne 0) {
            Write-Host "DesignConversion DPO storage method-pack self-check failed: positive oversized theory method was rejected."
            return 1
        }

        [System.IO.File]::WriteAllText(
            (Join-Path $sourceDir 'HiddenStoragePartial.cs'),
            "public sealed partial class DesignConversionDpoStorageTests : HiddenFixtureBase { }`n",
            [System.Text.UTF8Encoding]::new($false))
        $hiddenPartial = Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $batchDir -PlanRows $planRows -MethodRows $methodRows -PackReport $packReport -FingerprintValues $fingerprint -RepositoryRoot $repoRoot
        if ($hiddenPartial -eq 0) {
            Write-Host 'DesignConversion DPO storage method-pack self-check failed: hidden partial source was accepted.'
            return 1
        }
        Remove-Item -LiteralPath (Join-Path $sourceDir 'HiddenStoragePartial.cs') -Force

        [System.IO.File]::WriteAllText(
            $exactPath,
            "class_fqn`tsource_paths`tunit_key`tcollection_name`tcollection_kind`tdeclaration_count`tbinding_kind`nDesignConversionDpoStorageTests`t../outside.cs`tclass:DesignConversionDpoStorageTests`t`tnone`t1`texact_declaration`n",
            [System.Text.UTF8Encoding]::new($false))
        $fingerprint['source_exact_bindings_sha256'] = Get-FileSha256Lower -Path $exactPath
        $escapedPath = Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $batchDir -PlanRows $planRows -MethodRows $methodRows -PackReport $packReport -FingerprintValues $fingerprint -RepositoryRoot $repoRoot
        if ($escapedPath -eq 0) {
            Write-Host 'DesignConversion DPO storage method-pack self-check failed: escaped source path was accepted.'
            return 1
        }

        [System.IO.File]::WriteAllText(
            $exactPath,
            "class_fqn`tsource_paths`tunit_key`tcollection_name`tcollection_kind`tdeclaration_count`tbinding_kind`nDesignConversionDpoStorageTests`t$sourceRel`tclass:DesignConversionDpoStorageTests`t`tnone`t2`texact_declaration`n",
            [System.Text.UTF8Encoding]::new($false))
        $fingerprint['source_exact_bindings_sha256'] = Get-FileSha256Lower -Path $exactPath
        $tamperedCapReport = $packReport.Clone()
        $tamperedCapReport['hard_cap_rows'] = '90'
        $tamperedCap = Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $batchDir -PlanRows $planRows -MethodRows $methodRows -PackReport $tamperedCapReport -FingerprintValues $fingerprint -RepositoryRoot $repoRoot
        if ($tamperedCap -eq 0) {
            Write-Host 'DesignConversion DPO storage method-pack self-check failed: tampered cap was accepted.'
            return 1
        }

        $mixedPlanRows = @(
            $planRows[0],
            [pscustomobject]@{ LeafId = 'leaf-002'; BatchId = 'design-conversion-dpo-storage'; LeafOrder = '002'; MethodCount = 2; EstimatedRows = 76; Filter = 'FullyQualifiedName=DesignConversionDpoStorageTests.TheoryWithSeventyFiveRows|FullyQualifiedName=DesignConversionDpoStorageTests.FastTwo' },
            $planRows[2],
            $planRows[3]
        )
        $mixedMethodRows = @(
            $methodRows[0],
            [pscustomobject]@{ MethodFqn = 'DesignConversionDpoStorageTests.FastTwo'; BatchId = 'design-conversion-dpo-storage'; LeafId = 'leaf-002'; MethodOrder = '00002'; EstimatedRows = 1; ClassFqn = 'DesignConversionDpoStorageTests'; UnitKey = 'method:DesignConversionDpoStorageTests.FastTwo'; CollectionName = ''; CollectionKind = 'none' },
            $methodRows[2],
            $methodRows[3],
            $methodRows[4],
            $methodRows[5]
        )
        $mixedOversized = Test-DesignConversionDpoStorageClassMethodPackEvidence -BatchDir $batchDir -PlanRows $mixedPlanRows -MethodRows $mixedMethodRows -PackReport $packReport -FingerprintValues $fingerprint -RepositoryRoot $repoRoot
        if ($mixedOversized -eq 0) {
            Write-Host 'DesignConversion DPO storage method-pack self-check failed: mixed oversized leaf was accepted.'
            return 1
        }

        Write-Host 'DesignConversion DPO storage method-pack self-check: positive oversized theory accepted; hidden partial, escaped path, tampered cap and mixed oversized rejected.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function New-RunInvocationHistoryLine {
    param(
        [int]$Order,
        [string]$PreviousPayload,
        [string]$InvocationId = ('invocation-{0:000000}' -f $Order),
        [string]$MaxLeaves = '0',
        [string]$MaxElapsedMinutes = '0',
        [string]$VerificationScope = 'CANONICAL',
        [string]$SelectedBatches = 'self-check',
        [string]$EffectiveLeafModes = 'self-check:pack',
        [string]$BatchesTsvSha256 = ('0' * 64),
        [string]$FocusedMode = 'false'
    )

    $parts = @(
        [string]$Order,
        $InvocationId,
        '2026-08-27T00:00:00Z',
        $VerificationScope,
        $SelectedBatches,
        '1',
        'pack',
        $EffectiveLeafModes,
        $MaxLeaves,
        $MaxElapsedMinutes,
        'false',
        'true',
        $FocusedMode,
        $BatchesTsvSha256,
        'STARTED',
        (Get-TextSha256Lower -Text $PreviousPayload)
    )
    $rowWithoutHash = $parts -join "`t"
    return $rowWithoutHash + "`t" + (Get-TextSha256Lower -Text $rowWithoutHash)
}

function Write-RunInvocationHistoryFixture {
    param(
        [string]$Path,
        [string[]]$Rows,
        [string]$Header = $script:RunInvocationHistoryHeader
    )

    [System.IO.File]::WriteAllText(
        $Path,
        $Header + "`n" + (($Rows | ForEach-Object { $_ }) -join "`n") + "`n",
        [System.Text.UTF8Encoding]::new($false))
}

function New-RunInvocationHistorySelfCheckRows {
    $row1 = New-RunInvocationHistoryLine -Order 1 -PreviousPayload '' -MaxLeaves '2'
    $row2 = New-RunInvocationHistoryLine -Order 2 -PreviousPayload ($row1 + "`n") -MaxLeaves '1'
    return [pscustomobject]@{
        Row1 = $row1
        Row2 = $row2
        Rows = @($row1, $row2)
    }
}

function Invoke-RunInvocationHistoryTempSelfCheck {
    param([scriptblock]$Body)

    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-run-invocations-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $historyPath = Join-Path $tempRoot 'run-invocations.tsv'

    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
        return [int](& $Body $historyPath)
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-RunInvocationHistoryValidSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows $fixture.Rows
        $valid = Read-RunInvocationHistory -Path $historyPath
        if ($valid.Problems -ne 0 -or @($valid.Rows).Count -ne 2 -or $valid.SelectedBatchCountSum -ne 2) {
            Write-Host "Run invocation self-check failed: valid append-only history was rejected."
            return 1
        }

        Write-Host "Run invocation history self-check: valid 2+1 append-only fixture accepted."
        return 0
    }
}

function Test-RunInvocationHistoryMissingFileSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $missing = Read-RunInvocationHistory -Path $historyPath
        if ($missing.Problems -le 0) {
            Write-Host "Run invocation self-check failed: missing history file was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: missing history file rejected."
        return 0
    }
}

function Test-RunInvocationHistoryBadHeaderSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows $fixture.Rows -Header 'bad	header'
        $badHeader = Read-RunInvocationHistory -Path $historyPath
        if ($badHeader.Problems -le 0) {
            Write-Host "Run invocation self-check failed: bad header was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: bad header rejected."
        return 0
    }
}

function Test-RunInvocationHistoryDeletedFirstRowSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows @($fixture.Row2)
        $deletedFirst = Read-RunInvocationHistory -Path $historyPath
        if ($deletedFirst.Problems -le 0) {
            Write-Host "Run invocation self-check failed: deleted first invocation was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: deleted first invocation rejected."
        return 0
    }
}

function Test-RunInvocationHistoryNonmonotonicSequenceSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        $row3 = New-RunInvocationHistoryLine -Order 3 -PreviousPayload ($fixture.Row1 + "`n") -InvocationId 'invocation-000003' -MaxLeaves '1'
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows @($fixture.Row1, $row3)
        $nonmonotonic = Read-RunInvocationHistory -Path $historyPath
        if ($nonmonotonic.Problems -le 0) {
            Write-Host "Run invocation self-check failed: nonmonotonic sequence was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: nonmonotonic sequence rejected."
        return 0
    }
}

function Test-RunInvocationHistoryDuplicateIdSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        $duplicateIdRow2 = New-RunInvocationHistoryLine -Order 2 -PreviousPayload ($fixture.Row1 + "`n") -InvocationId 'invocation-000001' -MaxLeaves '1'
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows @($fixture.Row1, $duplicateIdRow2)
        $duplicateId = Read-RunInvocationHistory -Path $historyPath
        if ($duplicateId.Problems -le 0) {
            Write-Host "Run invocation self-check failed: duplicate invocation id was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: duplicate invocation id rejected."
        return 0
    }
}

function Test-RunInvocationHistoryInvalidBudgetSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $badBudgetRow1 = New-RunInvocationHistoryLine -Order 1 -PreviousPayload '' -MaxLeaves 'two'
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows @($badBudgetRow1)
        $badBudget = Read-RunInvocationHistory -Path $historyPath
        if ($badBudget.Problems -le 0) {
            Write-Host "Run invocation self-check failed: invalid numeric budget was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: invalid numeric budget rejected."
        return 0
    }
}

function Test-RunInvocationHistoryTamperedPriorRowSelfCheck {
    return Invoke-RunInvocationHistoryTempSelfCheck {
        param([string]$historyPath)

        $fixture = New-RunInvocationHistorySelfCheckRows
        $tamperedRow1 = $fixture.Row1.Replace("`t2`t0`tfalse", "`t9`t0`tfalse")
        if ($tamperedRow1 -eq $fixture.Row1) {
            Write-Host "Run invocation self-check failed: fixture mutation did not change the first row."
            return 1
        }

        # Русский комментарий: старую строку меняем без пересчета ее row_hash и
        # без пересчета previous_rows_sha256 во второй строке; verifier обязан
        # увидеть подмену как нарушение append-only цепочки.
        Write-RunInvocationHistoryFixture -Path $historyPath -Rows @($tamperedRow1, $fixture.Row2)
        $tamperedPrior = Read-RunInvocationHistory -Path $historyPath
        if ($tamperedPrior.Problems -le 0) {
            Write-Host "Run invocation self-check failed: tampered prior invocation was accepted."
            return 1
        }

        Write-Host "Run invocation history self-check: tampered prior invocation rejected."
        return 0
    }
}

function Test-LegacyBatchEvidenceSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-legacy-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'

    try {
        New-Item -ItemType Directory -Force -Path $batchDir | Out-Null
        Set-Content -LiteralPath (Join-Path $tempRoot 'state.txt') -Value 'FINISHED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_GREEN`t0`t1`t1`t0`t0`tresult.trx`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'result.trx') -Value (New-MinimalValidTrx) -Encoding UTF8

        $problems = Test-RunRootEvidence -Path $tempRoot -Rows @() -EffectiveRows @() -Classes @()
        if ($problems -ne 0) {
            Write-Host "Legacy batch evidence self-check failed: problems=$problems"
            return 1
        }

        Write-Host "Legacy batch evidence self-check: old batch-only run root accepted."
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LatestJournalProjectionSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-latest-journal-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $batchDir = Join-Path $tempRoot 'batch-01-self-check'
    $manifestPath = Join-Path $tempRoot 'batches.tsv'

    try {
        New-Item -ItemType Directory -Force -Path $batchDir | Out-Null
        Set-Content -LiteralPath $manifestPath -Value "# order`tbatch_id`thuman_name`tdll`tfilter`tmin_rows`tsplit_on_repeat_timeout`tplatform`tplatform_reason`n01`tself-check`tSelf check`tTests/bin/Release/net9.0/LlmFromScratch.Tests.dll`tFullyQualifiedName=Example.Tests.One`t1`tfalse`tany`tself-check" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'state.txt') -Value 'FINISHED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tLEAF_INCOMPLETE`tleaf-pack`t1`t1`t0`t0`tleaf-journal.tsv`tany`tself-check`tfalse`n01`tself-check`tSelf check`t2026-08-27T00:00:02Z`t2026-08-27T00:00:03Z`t1`tCOMPLETED_GREEN`t0`t2`t2`t0`t0`tresult.trx`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'state.txt') -Value 'COMPLETED_GREEN' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $batchDir 'result.trx') -Value (New-MinimalValidTrx -RowCount 2) -Encoding UTF8

        $rows = @(Read-BatchRows -Path $manifestPath)
        $problems = Test-RunRootEvidence -Path $tempRoot -Rows $rows -EffectiveRows @() -Classes @()
        $counts = Get-RunRootCounts -Path $tempRoot -Rows $rows
        if ($problems -ne 0 -or $counts.Rows -ne 1 -or $counts.RawRows -ne 2 -or $counts.Total -ne 2 -or $counts.Passed -ne 2 -or $counts.Failed -ne 0 -or $counts.NotExecuted -ne 0) {
            Write-Host "Latest journal projection self-check failed: problems=$problems projected=$($counts.Rows) raw=$($counts.RawRows) total=$($counts.Total)."
            return 1
        }

        Set-Content -LiteralPath (Join-Path $tempRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_RED`t1`t2`t1`t1`t0`tresult.trx`tany`tself-check`tfalse`n01`tself-check`tSelf check`t2026-08-27T00:00:02Z`t2026-08-27T00:00:03Z`t1`tCOMPLETED_GREEN`t0`t2`t2`t0`t0`tresult.trx`tany`tself-check`tfalse" -Encoding UTF8
        $conflictProblems = Test-RunRootEvidence -Path $tempRoot -Rows $rows -EffectiveRows @() -Classes @()
        if ($conflictProblems -le 0) {
            Write-Host 'Latest journal projection self-check failed: conflicting terminal duplicate was accepted.'
            return 1
        }

        Write-Host 'Latest journal projection self-check: partial row superseded, raw rows stay visible, terminal conflict rejected.'
        return 0
    }
    finally {
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

function Test-LegacyQwenDynamicSkipNormalizationSelfCheck {
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("batch-qwen-legacy-skip-selfcheck-" + [guid]::NewGuid().ToString('N'))
    $className = 'DesignConversionLocalIndependentQwenRuntimeCanaryTests'
    $methodName = 'LocalQwenJudge_FourCallsChooseSameQualityAndReverseDirectionOnSwap'
    $legacyMessage = '$XunitDynamicSkip$WINDOWS_ONLY_RUNTIME: локальный Qwen/Ollama canary запускается на Windows.'
    $oldOptIn = [Environment]::GetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName)

    try {
        New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, $null)

        $goodTrx = Join-Path $tempRoot 'legacy-good.trx'
        Set-Content -LiteralPath $goodTrx -Value (New-MinimalValidTrx -ClassName $className -MethodName $methodName -Outcome 'Failed' -Message $legacyMessage) -Encoding UTF8
        $goodUnset = Read-TrxCounts -Path $goodTrx
        if (-not $goodUnset.Valid -or $goodUnset.Total -ne 1 -or $goodUnset.Passed -ne 0 -or $goodUnset.Failed -ne 0 -or $goodUnset.NotExecuted -ne 1 -or $goodUnset.LegacyNormalizedDynamicSkips -ne 1) {
            Write-Host "Legacy Qwen dynamic skip self-check failed: exact old case was not normalized with env unset (valid=$($goodUnset.Valid), reason=$($goodUnset.Reason), failed=$($goodUnset.Failed), not_executed=$($goodUnset.NotExecuted))."
            return 1
        }
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, '1')
        $goodEnvOne = Read-TrxCounts -Path $goodTrx
        if (-not $goodEnvOne.Valid -or
            $goodEnvOne.Total -ne $goodUnset.Total -or
            $goodEnvOne.Passed -ne $goodUnset.Passed -or
            $goodEnvOne.Failed -ne $goodUnset.Failed -or
            $goodEnvOne.NotExecuted -ne $goodUnset.NotExecuted -or
            $goodEnvOne.LegacyNormalizedDynamicSkips -ne $goodUnset.LegacyNormalizedDynamicSkips) {
            Write-Host "Legacy Qwen dynamic skip self-check failed: historical TRX changed under current env=1 (unset failed=$($goodUnset.Failed), unset not_executed=$($goodUnset.NotExecuted), env1 failed=$($goodEnvOne.Failed), env1 not_executed=$($goodEnvOne.NotExecuted))."
            return 1
        }
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, $null)

        $oldLeafJournalRow = [pscustomobject]@{ State = 'COMPLETED_RED'; Total = 1; Passed = 0; Failed = 1; NotExecuted = 0 }
        if (-not (Test-LeafJournalMatchesMembership -JournalRow $oldLeafJournalRow -LeafState 'COMPLETED_RED' -Membership $goodUnset)) {
            Write-Host 'Legacy Qwen dynamic skip self-check failed: exact old leaf-journal raw row was not accepted.'
            return 1
        }
        $unprovedNormalization = [pscustomobject]@{
            Total = 1
            Passed = 0
            Failed = 0
            NotExecuted = 1
            RawPassed = 0
            RawFailed = 1
            RawNotExecuted = 0
            LegacyNormalizedDynamicSkips = 0
        }
        if (Test-LeafJournalMatchesMembership -JournalRow $oldLeafJournalRow -LeafState 'COMPLETED_RED' -Membership $unprovedNormalization) {
            Write-Host 'Legacy Qwen dynamic skip self-check failed: unproved raw leaf-journal normalization was accepted.'
            return 1
        }
        $projectionRoot = Join-Path $tempRoot 'projection-root'
        $projectionBatchDir = Join-Path $projectionRoot 'batch-01-self-check'
        $projectionLeafDir = Join-Path $projectionBatchDir 'leaf-001'
        New-Item -ItemType Directory -Force -Path $projectionLeafDir | Out-Null
        Set-Content -LiteralPath (Join-Path $projectionRoot 'journal.tsv') -Value "order`tbatch_id`thuman_name`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tplatform`tplatform_reason`tsplit_on_repeat_timeout`n01`tself-check`tSelf check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_RED`t1`t1`t0`t1`t0`tleaf-journal.tsv`tany`tself-check`tfalse" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $projectionBatchDir 'state.txt') -Value 'COMPLETED_RED' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $projectionBatchDir 'leaf-plan.tsv') -Value "leaf_id`tbatch_id`tleaf_order`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t001`t1`t1`tFullyQualifiedName=$script:LegacyQwenCanaryFqn" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $projectionBatchDir 'leaf-methods.tsv') -Value "method_fqn`tbatch_id`tleaf_id`tmethod_order`testimated_rows`tclass_fqn`tunit_key`n$script:LegacyQwenCanaryFqn`tself-check`tleaf-001`t00001`t1`t$className`tclass:$className" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $projectionBatchDir 'leaf-journal.tsv') -Value "leaf_id`tbatch_id`tstarted_utc`tfinished_utc`tseconds`tstate`texit_code`ttotal`tpassed`tfailed`tnot_executed`tresult_path`tmethod_count`testimated_rows`tfilter`nleaf-001`tself-check`t2026-08-27T00:00:00Z`t2026-08-27T00:00:01Z`t1`tCOMPLETED_RED`t1`t1`t0`t1`t0`tresult.trx`t1`t1`tFullyQualifiedName=$script:LegacyQwenCanaryFqn" -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $projectionLeafDir 'state.txt') -Value 'COMPLETED_RED' -Encoding UTF8
        Copy-Item -LiteralPath $goodTrx -Destination (Join-Path $projectionLeafDir 'result.trx') -Force
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, '1')
        $projectionCounts = Get-RunRootCounts -Path $projectionRoot -Rows @([pscustomobject]@{ BatchId = 'self-check' })
        if ($projectionCounts.Rows -ne 1 -or $projectionCounts.RawRows -ne 1 -or $projectionCounts.Total -ne 1 -or $projectionCounts.Failed -ne 0 -or $projectionCounts.NotExecuted -ne 1) {
            Write-Host "Legacy Qwen dynamic skip self-check failed: RunRoot projection used raw old batch counts (rows=$($projectionCounts.Rows), raw=$($projectionCounts.RawRows), failed=$($projectionCounts.Failed), not_executed=$($projectionCounts.NotExecuted))."
            return 1
        }
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, $null)

        $wrongFqnTrx = Join-Path $tempRoot 'wrong-fqn.trx'
        Set-Content -LiteralPath $wrongFqnTrx -Value (New-MinimalValidTrx -ClassName 'Example.Tests' -MethodName 'LocalQwenJudge_FourCallsChooseSameQualityAndReverseDirectionOnSwap' -Outcome 'Failed' -Message $legacyMessage) -Encoding UTF8
        $wrongFqn = Read-TrxCounts -Path $wrongFqnTrx
        if (-not $wrongFqn.Valid -or $wrongFqn.Failed -ne 1 -or $wrongFqn.NotExecuted -ne 0) {
            Write-Host 'Legacy Qwen dynamic skip self-check failed: broad FQN normalization was accepted.'
            return 1
        }

        $wrongPrefixTrx = Join-Path $tempRoot 'wrong-prefix.trx'
        Set-Content -LiteralPath $wrongPrefixTrx -Value (New-MinimalValidTrx -ClassName $className -MethodName $methodName -Outcome 'Failed' -Message '$XunitDynamicSkip$OPTIONAL_LOCAL_RUNTIME: задайте opt-in.') -Encoding UTF8
        $wrongPrefix = Read-TrxCounts -Path $wrongPrefixTrx
        if (-not $wrongPrefix.Valid -or $wrongPrefix.Failed -ne 1 -or $wrongPrefix.NotExecuted -ne 0) {
            Write-Host 'Legacy Qwen dynamic skip self-check failed: broad dynamic-skip prefix normalization was accepted.'
            return 1
        }

        $realFailureTrx = Join-Path $tempRoot 'real-failure.trx'
        Set-Content -LiteralPath $realFailureTrx -Value (New-MinimalValidTrx -ClassName $className -MethodName $methodName -Outcome 'Failed' -Message 'Assert.Equal() Failure: Qwen chose the wrong side.') -Encoding UTF8
        $realFailure = Read-TrxCounts -Path $realFailureTrx
        if (-not $realFailure.Valid -or $realFailure.Failed -ne 1 -or $realFailure.NotExecuted -ne 0) {
            Write-Host 'Legacy Qwen dynamic skip self-check failed: real canary failure was normalized.'
            return 1
        }

        Write-Host 'Legacy Qwen dynamic skip self-check: exact old skip is env-invariant and leaf-journal raw row normalized; wrong FQN, wrong prefix and real failure rejected.'
        return 0
    }
    finally {
        [Environment]::SetEnvironmentVariable($script:QwenCanaryOptInEnvironmentName, $oldOptIn)
        $resolvedTemp = [System.IO.Path]::GetFullPath($tempRoot)
        $safePrefix = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if ($resolvedTemp.StartsWith($safePrefix, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $resolvedTemp)) {
            Remove-Item -LiteralPath $resolvedTemp -Recurse -Force
        }
    }
}

if ($SelfCheckEmptyResidual) {
    exit (Test-EmptyResidualEvidenceSelfCheck)
}

if ($SelfCheckIndivisibleNamespaceCollision) {
    # Узкая самопроверка живого PowerShell-зеркала: реальный глобальный класс
    # получает исключение независимо от / или \ в DLL-пути, а одноимённый класс
    # в namespace всё ещё обязан оставаться под семейной защитой старой группы.
    exit (Test-IndivisibleNamespaceCollisionSelfCheck)
}

if ($SelfCheckExactHumanBatchAssignmentMap) {
    exit (Test-ExactHumanBatchAssignmentMapSelfCheck)
}

if ($SelfCheckPreparedFilterParsing) {
    exit (Test-PreparedFilterParsingSelfCheck)
}

$rows = @(Read-BatchRows -Path $BatchesFile)
$batchesIsDefault = Test-IsDefaultBatchesFile -Path $BatchesFile
$recordedScope = if (-not [string]::IsNullOrWhiteSpace($RunRoot) -and (Test-Path -LiteralPath $RunRoot)) { Get-RunScopeValue -Path $RunRoot } else { '' }
$effectiveRows = @()
$classes = @()
$errors = 0
$script:SplitPlanValidationProblems = 0

$duplicateBatchIds = @($rows | Group-Object BatchId | Where-Object Count -gt 1)
if ($duplicateBatchIds.Count -gt 0) {
    Write-Host "Duplicate batch ids: $($duplicateBatchIds.Name -join ', ')"
    $errors += $duplicateBatchIds.Count
}

foreach ($row in $rows) {
    if ([string]::IsNullOrWhiteSpace($row.HumanName) -or [string]::IsNullOrWhiteSpace($row.Filter)) {
        Write-Host "Batch $($row.BatchId) has no human name or filter."
        $errors++
    }

    if ($row.SplitOnRepeatTimeout -notin @('true', 'false')) {
        Write-Host "Batch $($row.BatchId) split_on_repeat_timeout must be true or false."
        $errors++
    }

    if ($row.Platform -notin @('any', 'linux', 'windows')) {
        Write-Host "Batch $($row.BatchId) has unknown platform: $($row.Platform)."
        $errors++
    }
}

$assignmentByClass = @{}
$unassigned = New-Object System.Collections.Generic.List[object]
$duplicateExamples = New-Object System.Collections.Generic.List[string]
$duplicateAssignments = 0
$brokenGroups = 0
$windowsDesktopAccounted = $true
$ladderProblems = 0
$orchestrationRoutingProductBoundaryProblems = 0
$designConversionLiveLocalServerMethodDiagnosticProblems = 0
$testInfrastructureEvidenceMeasurementProblems = 0
$serverOwnedReplayManifestAttestationProblems = 0
$phase08CommonUserTwoLegLifecycleProblems = 0
# Отдельные счётчики нужны, чтобы незаметное изменение коротких маршрутов
# завершало verifier ненулевым кодом, а не оставалось строкой в отчёте.
$singletonFlowNodeIdentityProblems = 0
$logicalTopologyAndResultGapProblems = 0
$resultGapTrainingPairWriterProblems = 0
$singleCombinationGraphRunnerProblems = 0
$retainedDomainModelRegistryProblems = 0
$pairGraphPriorityProblems = 0
$modelActivationProblems = 0
$leafModeOverrideProblems = 0
$indivisibleNamespaceCollisionSelfCheckProblems = 0
$exactHumanBatchAssignmentMapSelfCheckProblems = 0
$preparedFilterParsingSelfCheckProblems = 0
$canonicalExplicitFilterRows = 0
$canonicalWildcardRows = 0
$coreRemainderAssignmentCount = 0
$legacyWeakSnapshotRows = 0
$legacyWeakSnapshotProblems = 0
$expectedLeafModes = @{}
$coreRemainderExclusions = 0
$verificationScope = ''
$focusedMethodProblems = 0
$focusedMethodCountForReport = 0
$focusedExpectedBatches = $null
$linkedSourceClassIdentities = 0
$logicalLadderMetadataProblems = 0

if ($Focused) {
    if ($batchesIsDefault) {
        Write-Host "Focused verifier requires a nondefault focused manifest; default batches.tsv is canonical-only."
        $errors++
    }

    if (-not [string]::IsNullOrWhiteSpace($recordedScope) -and $recordedScope -ne 'FOCUSED') {
        Write-Host "Focused verifier rejects run root with non-focused scope: $recordedScope"
        $errors++
    }

    $verificationScope = if (-not [string]::IsNullOrWhiteSpace($recordedScope)) { 'FOCUSED' } elseif (-not [string]::IsNullOrWhiteSpace($RunRoot)) { 'FOCUSED_LEGACY' } else { 'FOCUSED' }
    $knownMethods = @(Get-TestMethods -Root $RepoRoot)
    $focusedMethodProblems = Test-FocusedBatchRows -Rows $rows -KnownMethods $knownMethods
    try {
        $focusedExpectedBatches = New-FocusedExpectedBatchMap -Rows $rows -ManifestPath $BatchesFile
        $focusedMethodCountForReport = $focusedExpectedBatches.TotalMethodCount
    }
    catch {
        Write-Host "Focused verifier cannot build manifest binding map: $($_.Exception.Message)"
        $focusedMethodCountForReport = 0
        $errors++
    }

    # Focused-манифест уже является явным списком method FQN. Мы намеренно
    # пропускаем только canonical-проверку "все классы проекта распределены",
    # но TRX membership и leaf-факты ниже остаются обязательными.
    $effectiveRows = @($rows | ForEach-Object {
        [pscustomobject]@{
            Order = $_.Order
            BatchId = $_.BatchId
            Dll = $_.Dll
            RawFilter = $_.Filter
            EffectiveFilter = $_.Filter
            PriorTerms = 0
        }
    })
}
else {
    if (-not $batchesIsDefault) {
        Write-Host "Canonical verifier requires the default batches.tsv. Use -Focused for a custom exact-method manifest."
        $errors++
    }

    if ($recordedScope -eq 'FOCUSED') {
        Write-Host "Canonical verifier refuses focused run root; rerun with -Focused for this evidence scope."
        $errors++
    }
    elseif (-not [string]::IsNullOrWhiteSpace($recordedScope) -and $recordedScope -ne 'CANONICAL') {
        Write-Host "Canonical verifier rejects unknown run root scope: $recordedScope"
        $errors++
    }

    $verificationScope = if ($recordedScope -eq 'CANONICAL') { 'CANONICAL' } elseif (-not [string]::IsNullOrWhiteSpace($RunRoot)) { 'CANONICAL_LEGACY' } else { 'CANONICAL' }
    if ($batchesIsDefault) {
        $effectiveRows = @(Get-EffectiveRows -Rows $rows)
        $classes = @(Get-TestClasses -Root $RepoRoot)
        $logicalLadderMetadataProblems = Test-LogicalLadderMetadata -Path $BatchesFile -Rows $rows
        $linkedSourceClassIdentities = @($classes | Where-Object {
            $_.PSObject.Properties.Name -contains 'IsLinkedSourceExpansion' -and $_.IsLinkedSourceExpansion
        }).Count

        if ($rows.Count -ne $script:CanonicalBatchCount) {
            Write-Host "Batch count must be $script:CanonicalBatchCount; found $($rows.Count)."
            $errors++
        }

        $canonicalWildcardRows = @($rows | Where-Object { $_.Filter -eq '*' }).Count
        $canonicalExplicitFilterRows = $rows.Count - $canonicalWildcardRows
        if ($canonicalWildcardRows -ne 1) {
            Write-Host "Canonical wildcard row count must be 1; found $canonicalWildcardRows."
            $errors++
        }

        if ($canonicalExplicitFilterRows -ne ($script:CanonicalBatchCount - 1)) {
            Write-Host "Canonical explicit filter row count must be $($script:CanonicalBatchCount - 1); found $canonicalExplicitFilterRows."
            $errors++
        }

        foreach ($testClass in $classes) {
            $batch = Find-FirstBatch -TestClass $testClass -Rows $rows -PreparedRows $effectiveRows
            if ($null -eq $batch) {
                $unassigned.Add($testClass)
                continue
            }

            if ($batch.Filter -eq '*') {
                $coreRemainderAssignmentCount++
            }

            $assignmentByClass["$($testClass.Dll)|$($testClass.FullyQualifiedName)"] = $batch.BatchId

            $effectiveMatches = @($effectiveRows | Where-Object {
                $_.Dll -eq $testClass.Dll -and (Test-EffectiveFilterForClass -Filter $_.EffectiveFilter -FullyQualifiedName $testClass.FullyQualifiedName -ParsedFilter $_.ParsedEffectiveFilter)
            })
            if ($effectiveMatches.Count -gt 1) {
                $duplicateAssignments++
                if ($duplicateExamples.Count -lt 20) {
                    $duplicateExamples.Add("$($testClass.ClassName): $($effectiveMatches.BatchId -join ', ')")
                }
            }
        }
        $assignmentByClass['__rows_identity'] = Get-ExactHumanBatchRowsIdentity -Rows $rows

        $brokenGroups = Test-IndivisibleGroups -Classes $classes -AssignmentByClass $assignmentByClass
        $indivisibleNamespaceCollisionSelfCheckProblems = Test-IndivisibleNamespaceCollisionSelfCheck
        $windowsRows = @($rows | Where-Object { $_.Dll -eq 'Tests.ApiDashboardWpf\bin\Release\net9.0-windows\Tests.ApiDashboardWpf.dll' -and $_.Platform -eq 'windows' -and $_.MinRows -ge 9 -and -not [string]::IsNullOrWhiteSpace($_.PlatformReason) })
        $windowsDesktopAccounted = $windowsRows.Count -eq 1
        $ladderProblems = Test-LadderEffectiveFilters -EffectiveRows $effectiveRows
        $orchestrationRoutingProductBoundaryProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:OrchestrationRoutingProductBoundariesBatchId -ExpectedClassFqns $script:OrchestrationRoutingProductBoundaryClassFqns -ProblemPrefix 'orchestration_routing_product_boundaries' -HumanReason 'request compilation, route selection, orchestrator, wiring, plugin and thin-client boundary classes must stay in their named human batch' -AssignmentByClass $assignmentByClass
        # Медленный локальный сервер вынесен из pack, но только один точный
        # класс: verifier не должен принять широкий семейный фильтр или lookalike.
        $designConversionLiveLocalServerMethodDiagnosticProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:DesignConversionLiveLocalServerMethodDiagnosticBatchId -ExpectedClassFqns $script:DesignConversionLiveLocalServerMethodDiagnosticClassFqns -ProblemPrefix 'design_conversion_live_local_server_method_diagnostic' -HumanReason 'the slow DesignConversionLiveLocalServerTests class must stay in its one-class method-diagnostic route' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        $testInfrastructureEvidenceMeasurementProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:TestInfrastructureEvidenceMeasurementBatchId -ExpectedClassFqns $script:TestInfrastructureEvidenceMeasurementClassFqns -ProblemPrefix 'test_infrastructure_evidence_measurement' -HumanReason 'test infrastructure, RAG evidence index and measurement guard classes must stay in their named human batch' -AssignmentByClass $assignmentByClass
        $serverOwnedReplayManifestAttestationProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:ServerOwnedReplayManifestAttestationBatchId -ExpectedClassFqns $script:ServerOwnedReplayManifestAttestationClassFqns -ProblemPrefix 'server_owned_replay_manifest_attestation' -HumanReason 'server-owned replay verification and executable manifest passport tests must stay in their one-class class-boundary batch' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        $phase08CommonUserTwoLegLifecycleProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:Phase08CommonUserTwoLegLifecycleBatchId -ExpectedClassFqns $script:Phase08CommonUserTwoLegLifecycleClassFqns -ProblemPrefix 'phase08_common_user_two_leg_lifecycle' -HumanReason 'Phase 08 portable profile, approved share, continued training and rollback evidence tests must stay in their one-class class-boundary batch' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Эта строка повторяет правила проверки на C#: один singleton-класс,
        # основная DLL и точный фильтр с границей полного имени класса, чтобы
        # соседние классы не расширили группу.
        $singletonFlowNodeIdentityProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:SingletonFlowNodeIdentityBatchId -ExpectedClassFqns $script:SingletonFlowNodeIdentityClassFqns -ProblemPrefix 'singleton_flow_node_identity' -HumanReason 'singleton-проверки общих объектов потоков и узлов должны оставаться в маршруте одного класса без расширения соседними классами' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Проверка этой пары держит двадцать строк логической структуры в
        # отдельном маршруте и не даёт им вернуться в wildcard-остаток.
        $logicalTopologyAndResultGapProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:LogicalTopologyAndResultGapBatchId -ExpectedClassFqns $script:LogicalTopologyAndResultGapClassFqns -ProblemPrefix 'logical_topology_and_result_gap' -HumanReason 'проверки структуры потоков и иерархии исправления результата должны оставаться в коротком маршруте из двух классов' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Запись обезличенных пар проверяется отдельным счётчиком: при поломке
        # verifier должен вернуть ненулевой код до запуска большой корзины.
        $resultGapTrainingPairWriterProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:ResultGapTrainingPairWriterBatchId -ExpectedClassFqns $script:ResultGapTrainingPairWriterClassFqns -ProblemPrefix 'result_gap_training_pair_writer' -HumanReason 'запись обезличенных обучающих пар и исправлений должна оставаться отдельным маршрутом одного класса; этот маршрут не запускает обучение модели и не доказывает качество обучения' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Выполнение графа получает отдельный счётчик, чтобы при падении было
        # видно выполнение графа, а не запись пары.
        $singleCombinationGraphRunnerProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:SingleCombinationGraphRunnerBatchId -ExpectedClassFqns $script:SingleCombinationGraphRunnerClassFqns -ProblemPrefix 'single_combination_graph_runner' -HumanReason 'выполнение одного варианта графа с отменой и ограничением времени должно оставаться отдельным маршрутом одного класса; этот маршрут не запускает обучение модели и не доказывает качество обучения' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Защищённый каталог сохраняемых моделей получает собственный маршрут:
        # Linux-исполняемые строки запускаются, а Windows-only строки остаются
        # честным NotExecuted, не зелёной подменой.
        $retainedDomainModelRegistryProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:RetainedDomainModelRegistryBatchId -ExpectedClassFqns $script:RetainedDomainModelRegistryClassFqns -ProblemPrefix 'retained_domain_model_registry' -HumanReason 'защищённый каталог сохраняемых моделей должен оставаться отдельным маршрутом одного класса; Windows-only строки учитываются как NotExecuted, а не как пройденные' -ExpectedDll 'Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll' -AssignmentByClass $assignmentByClass
        # Общая проверка порядка для двух коротких групп: обе должны идти до
        # широких графовых, обучающих и групп оркестрации.
        $pairGraphPriorityProblems = Test-PairGraphBatchPriority -Rows $rows
        $modelActivationProblems = Test-ExactHumanBatch -Rows $rows -Classes $classes -BatchId $script:ModelActivationBatchId -ExpectedClassFqns $script:ModelActivationClassFqns -ProblemPrefix 'model_activation' -HumanReason 'design conversion model activation classes must stay in the six-class assembly batch without wildcard or lookalike expansion' -ExpectedDll 'Tests\DesignConversion.ModelActivation.Tests\bin\Release\net9.0\DesignConversion.ModelActivation.Tests.dll' -AssignmentByClass $assignmentByClass
        $weakSnapshot = Read-LegacyWeakSnapshotRowsForVerifier -Root $RepoRoot
        $legacyWeakSnapshotRows = $weakSnapshot.Rows.Count
        foreach ($problem in $weakSnapshot.Problems) {
            Write-Host $problem
        }
        $legacyWeakSnapshotProblems += $weakSnapshot.Problems.Count
        if ($legacyWeakSnapshotRows -ne $script:CanonicalWeakSnapshotCount) {
            Write-Host "Legacy weak snapshot row count must be $script:CanonicalWeakSnapshotCount; found $legacyWeakSnapshotRows."
            $legacyWeakSnapshotProblems++
        }

        $overrideResult = Read-LeafModeOverrides -Path (Join-Path $PSScriptRoot 'leaf-mode-overrides.tsv') -Rows $rows
        foreach ($problem in $overrideResult.Problems) {
            Write-Host $problem
        }
        $leafModeOverrideProblems += $overrideResult.Problems.Count
        $leafModeOverrideProblems += Test-DesignConversionLiveLocalServerMethodDiagnosticLeafModeOverride -Overrides $overrideResult.Rows
        $leafModeOverrideProblems += Test-DesignConversionDpoStorageLeafModeOverride -Overrides $overrideResult.Rows
        $leafModeOverrideProblems += Test-TestInfrastructureLeafModeOverride -Overrides $overrideResult.Rows
        foreach ($overrideRow in $overrideResult.Rows) {
            $expectedLeafModes[$overrideRow.BatchId] = $overrideRow.LeafMode
        }

        $coreRemainder = $effectiveRows | Where-Object { $_.BatchId -eq 'core-m-z-and-remainder' } | Select-Object -First 1
        if ($null -ne $coreRemainder) {
            $coreRemainderExclusions = ([regex]::Matches($coreRemainder.EffectiveFilter, 'FullyQualifiedName!~')).Count
        }
    }
    else {
        # После fail-closed отказа custom manifest не прогоняем через canonical
        # substring-лестницу: иначе полезная ошибка превращается в stack trace.
        $effectiveRows = @($rows | ForEach-Object {
            [pscustomobject]@{
                Order = $_.Order
                BatchId = $_.BatchId
                Dll = $_.Dll
                RawFilter = $_.Filter
                EffectiveFilter = $_.Filter
                PriorTerms = 0
            }
        })
    }
}

$runRootProblems = Test-RunRootEvidence -Path $RunRoot -Rows $rows -EffectiveRows $effectiveRows -Classes $classes -ExpectedScope $(if ($Focused) { 'FOCUSED' } else { 'CANONICAL' }) -FocusedExpectedBatches $focusedExpectedBatches -ExpectedLeafModes $expectedLeafModes -AllowIncomplete:$AllowIncomplete
$attemptSelfCheckProblems = Test-StaleRootCannotCloseCurrentAttempt
$trxCounterSelfCheckProblems = Test-InconsistentExecutedCounterRejected
$leafTimingSelfCheckProblems = Test-LeafTimingSelfCheck
$leafTheoryExtraSelfCheckProblems = Test-LeafTheoryExtraMembershipSelfCheck
$leafIncompleteSelfCheckProblems = Test-LeafIncompleteAllowSelfCheck
$leafWrongMembershipSelfCheckProblems = Test-LeafWrongMembershipSelfCheck
$leafMissingMembershipSelfCheckProblems = Test-LeafMissingMembershipSelfCheck
$minRowsEvidenceSelfCheckProblems = Test-MinRowsEvidenceSelfCheck
$exactHumanBatchAssignmentMapSelfCheckProblems = Test-ExactHumanBatchAssignmentMapSelfCheck
$preparedFilterParsingSelfCheckProblems = Test-PreparedFilterParsingSelfCheck
$canonicalLeafClassAssignmentSelfCheckProblems = Test-CanonicalLeafClassAssignmentSelfCheck
$classFirstMethodEvidenceSelfCheckProblems = Test-ClassFirstMethodEvidenceSelfCheck
$emptyResidualEvidenceSelfCheckProblems = Test-EmptyResidualEvidenceSelfCheck
$focusedBindingSelfCheckProblems = Test-FocusedManifestBindingSelfCheck
$fingerprintSelfCheckProblems = Test-FingerprintSelfConsistencySelfCheck
$designConversionDpoStorageClassMethodPackSelfCheckProblems = Test-DesignConversionDpoStorageClassMethodPackSelfCheck
$runInvocationHistoryValidSelfCheckProblems = Test-RunInvocationHistoryValidSelfCheck
$runInvocationHistoryMissingFileSelfCheckProblems = Test-RunInvocationHistoryMissingFileSelfCheck
$runInvocationHistoryBadHeaderSelfCheckProblems = Test-RunInvocationHistoryBadHeaderSelfCheck
$runInvocationHistoryDeletedFirstRowSelfCheckProblems = Test-RunInvocationHistoryDeletedFirstRowSelfCheck
$runInvocationHistoryNonmonotonicSequenceSelfCheckProblems = Test-RunInvocationHistoryNonmonotonicSequenceSelfCheck
$runInvocationHistoryDuplicateIdSelfCheckProblems = Test-RunInvocationHistoryDuplicateIdSelfCheck
$runInvocationHistoryInvalidBudgetSelfCheckProblems = Test-RunInvocationHistoryInvalidBudgetSelfCheck
$runInvocationHistoryTamperedPriorRowSelfCheckProblems = Test-RunInvocationHistoryTamperedPriorRowSelfCheck
$runInvocationHistorySelfCheckProblems =
    $runInvocationHistoryValidSelfCheckProblems +
    $runInvocationHistoryMissingFileSelfCheckProblems +
    $runInvocationHistoryBadHeaderSelfCheckProblems +
    $runInvocationHistoryDeletedFirstRowSelfCheckProblems +
    $runInvocationHistoryNonmonotonicSequenceSelfCheckProblems +
    $runInvocationHistoryDuplicateIdSelfCheckProblems +
    $runInvocationHistoryInvalidBudgetSelfCheckProblems +
    $runInvocationHistoryTamperedPriorRowSelfCheckProblems
$legacyBatchSelfCheckProblems = Test-LegacyBatchEvidenceSelfCheck
$latestJournalProjectionSelfCheckProblems = Test-LatestJournalProjectionSelfCheck
$legacyQwenDynamicSkipSelfCheckProblems = Test-LegacyQwenDynamicSkipNormalizationSelfCheck
$runCounts = Get-RunRootCounts -Path $RunRoot -Rows $rows

Write-Host "verification_scope=$verificationScope"
Write-Host "full_suite_green_claimed=False"
if ($Focused) {
    Write-Host "focused_full_suite_green=False"
    Write-Host "Checked focused test methods: $focusedMethodCountForReport."
}
else {
    Write-Host "Checked public test classes: $($classes.Count - $linkedSourceClassIdentities)."
    Write-Host "Checked public test assembly identities: $($classes.Count)."
}
Write-Host "Batch rows: $($rows.Count)."
Write-Host ("canonical_explicit_filter_rows={0}" -f $canonicalExplicitFilterRows)
Write-Host ("canonical_wildcard_rows={0}" -f $canonicalWildcardRows)
Write-Host ("core_remainder_assignment_count={0}" -f $coreRemainderAssignmentCount)
Write-Host ("legacy_weak_snapshot_rows={0}" -f $legacyWeakSnapshotRows)
Write-Host ("legacy_weak_snapshot_problems={0}" -f $legacyWeakSnapshotProblems)
Write-Host ("logical_ladder_metadata_problems={0}" -f $logicalLadderMetadataProblems)
Write-Host ("logical_ladder_rule_rows={0}" -f $script:LogicalLadderRuleRows)
Write-Host ("logical_ladder_logical_batch_rows={0}" -f $script:LogicalLadderLogicalBatchRows)
Write-Host ("logical_ladder_mapped_physical_rows={0}" -f $script:LogicalLadderMappedPhysicalRows)
Write-Host ("logical_ladder_unknown_physical_rows={0}" -f $script:LogicalLadderUnknownPhysicalRows)
Write-Host ("logical_ladder_duplicate_physical_rows={0}" -f $script:LogicalLadderDuplicatePhysicalRows)
Write-Host ("logical_ladder_unmapped_physical_rows={0}" -f $script:LogicalLadderUnmappedPhysicalRows)
Write-Host ("focused_method_validation_problems={0}" -f $focusedMethodProblems)
Write-Host ("unassigned={0}" -f $unassigned.Count)
Write-Host ("duplicates={0}" -f $duplicateAssignments)
Write-Host ("broken_indivisible_groups={0}" -f $brokenGroups)
Write-Host ("indivisible_namespace_collision_self_check_problems={0}" -f $indivisibleNamespaceCollisionSelfCheckProblems)
Write-Host ("windows_desktop_accounted={0}" -f $windowsDesktopAccounted)
Write-Host ("late_residual_wildcard_violations={0}" -f $ladderProblems)
Write-Host ("orchestration_routing_product_boundary_problems={0}" -f $orchestrationRoutingProductBoundaryProblems)
Write-Host ("design_conversion_live_local_server_method_diagnostic_problems={0}" -f $designConversionLiveLocalServerMethodDiagnosticProblems)
Write-Host ("test_infrastructure_evidence_measurement_problems={0}" -f $testInfrastructureEvidenceMeasurementProblems)
Write-Host ("server_owned_replay_manifest_attestation_problems={0}" -f $serverOwnedReplayManifestAttestationProblems)
Write-Host ("phase08_common_user_two_leg_lifecycle_problems={0}" -f $phase08CommonUserTwoLegLifecycleProblems)
# Печатаем отдельный итог singleton-проверки, чтобы в логах было видно, какая
# человеческая группа сломалась, ещё до общего exit 1.
Write-Host ("singleton_flow_node_identity_problems={0}" -f $singletonFlowNodeIdentityProblems)
Write-Host ("logical_topology_and_result_gap_problems={0}" -f $logicalTopologyAndResultGapProblems)
Write-Host ("result_gap_training_pair_writer_problems={0}" -f $resultGapTrainingPairWriterProblems)
Write-Host ("single_combination_graph_runner_problems={0}" -f $singleCombinationGraphRunnerProblems)
Write-Host ("retained_domain_model_registry_problems={0}" -f $retainedDomainModelRegistryProblems)
Write-Host ("pair_graph_priority_problems={0}" -f $pairGraphPriorityProblems)
Write-Host ("model_activation_problems={0}" -f $modelActivationProblems)
Write-Host ("leaf_mode_override_problems={0}" -f $leafModeOverrideProblems)
Write-Host ("core_remainder_exclusion_terms={0}" -f $coreRemainderExclusions)
Write-Host ("linked_source_class_identities={0}" -f $linkedSourceClassIdentities)
Write-Host ("attempt_self_check_problems={0}" -f $attemptSelfCheckProblems)
Write-Host ("trx_counter_self_check_problems={0}" -f $trxCounterSelfCheckProblems)
Write-Host ("leaf_timing_self_check_problems={0}" -f $leafTimingSelfCheckProblems)
Write-Host ("leaf_theory_extra_self_check_problems={0}" -f $leafTheoryExtraSelfCheckProblems)
Write-Host ("leaf_incomplete_self_check_problems={0}" -f $leafIncompleteSelfCheckProblems)
Write-Host ("leaf_wrong_membership_self_check_problems={0}" -f $leafWrongMembershipSelfCheckProblems)
Write-Host ("leaf_missing_membership_self_check_problems={0}" -f $leafMissingMembershipSelfCheckProblems)
Write-Host ("min_rows_evidence_self_check_problems={0}" -f $minRowsEvidenceSelfCheckProblems)
Write-Host ("exact_human_batch_assignment_map_self_check_problems={0}" -f $exactHumanBatchAssignmentMapSelfCheckProblems)
Write-Host ("prepared_filter_parsing_self_check_problems={0}" -f $preparedFilterParsingSelfCheckProblems)
Write-Host ("canonical_leaf_class_assignment_self_check_problems={0}" -f $canonicalLeafClassAssignmentSelfCheckProblems)
Write-Host ("class_first_method_evidence_self_check_problems={0}" -f $classFirstMethodEvidenceSelfCheckProblems)
Write-Host ("empty_residual_evidence_self_check_problems={0}" -f $emptyResidualEvidenceSelfCheckProblems)
Write-Host ("focused_manifest_binding_self_check_problems={0}" -f $focusedBindingSelfCheckProblems)
Write-Host ("fingerprint_self_check_problems={0}" -f $fingerprintSelfCheckProblems)
Write-Host ("design_conversion_dpo_storage_class_method_pack_self_check_problems={0}" -f $designConversionDpoStorageClassMethodPackSelfCheckProblems)
Write-Host ("run_invocation_history_valid_self_check_problems={0}" -f $runInvocationHistoryValidSelfCheckProblems)
Write-Host ("run_invocation_history_missing_file_self_check_problems={0}" -f $runInvocationHistoryMissingFileSelfCheckProblems)
Write-Host ("run_invocation_history_bad_header_self_check_problems={0}" -f $runInvocationHistoryBadHeaderSelfCheckProblems)
Write-Host ("run_invocation_history_deleted_first_row_self_check_problems={0}" -f $runInvocationHistoryDeletedFirstRowSelfCheckProblems)
Write-Host ("run_invocation_history_nonmonotonic_sequence_self_check_problems={0}" -f $runInvocationHistoryNonmonotonicSequenceSelfCheckProblems)
Write-Host ("run_invocation_history_duplicate_id_self_check_problems={0}" -f $runInvocationHistoryDuplicateIdSelfCheckProblems)
Write-Host ("run_invocation_history_invalid_budget_self_check_problems={0}" -f $runInvocationHistoryInvalidBudgetSelfCheckProblems)
Write-Host ("run_invocation_history_tampered_prior_row_self_check_problems={0}" -f $runInvocationHistoryTamperedPriorRowSelfCheckProblems)
Write-Host ("run_invocation_history_self_check_problems={0}" -f $runInvocationHistorySelfCheckProblems)
Write-Host ("legacy_batch_self_check_problems={0}" -f $legacyBatchSelfCheckProblems)
Write-Host ("latest_journal_projection_self_check_problems={0}" -f $latestJournalProjectionSelfCheckProblems)
Write-Host ("legacy_qwen_dynamic_skip_self_check_problems={0}" -f $legacyQwenDynamicSkipSelfCheckProblems)
Write-Host ("split_plan_validation_problems={0}" -f $script:SplitPlanValidationProblems)
Write-Host ("run_root_evidence_problems={0}" -f $runRootProblems)
Write-Host ("run_root_rows={0}" -f $runCounts.Rows)
Write-Host ("run_root_raw_journal_rows={0}" -f $runCounts.RawRows)
Write-Host ("run_root_total={0}" -f $runCounts.Total)
Write-Host ("run_root_passed={0}" -f $runCounts.Passed)
Write-Host ("run_root_failed={0}" -f $runCounts.Failed)
Write-Host ("run_root_not_executed={0}" -f $runCounts.NotExecuted)
Write-Host ("run_root_empty_verified={0}" -f $runCounts.EmptyVerified)

if ($unassigned.Count -gt 0) {
    $unassigned | Select-Object -First 20 | ForEach-Object {
        Write-Host "Unassigned class: $($_.ClassName) [$($_.RelativePath)]"
    }
}

if ($duplicateAssignments -gt 0) {
    $duplicateExamples | ForEach-Object {
        Write-Host "Duplicate effective assignment: $_"
    }
}

# Включение счётчиков коротких маршрутов в общий предикат делает ошибку
# исполняемой: verifier обязан вернуть ненулевой код, если такой маршрут расползся.
if ($unassigned.Count -gt 0 -or $duplicateAssignments -ne 0 -or $brokenGroups -ne 0 -or $indivisibleNamespaceCollisionSelfCheckProblems -ne 0 -or -not $windowsDesktopAccounted -or $ladderProblems -ne 0 -or $orchestrationRoutingProductBoundaryProblems -ne 0 -or $designConversionLiveLocalServerMethodDiagnosticProblems -ne 0 -or $testInfrastructureEvidenceMeasurementProblems -ne 0 -or $serverOwnedReplayManifestAttestationProblems -ne 0 -or $phase08CommonUserTwoLegLifecycleProblems -ne 0 -or $singletonFlowNodeIdentityProblems -ne 0 -or $logicalTopologyAndResultGapProblems -ne 0 -or $resultGapTrainingPairWriterProblems -ne 0 -or $singleCombinationGraphRunnerProblems -ne 0 -or $retainedDomainModelRegistryProblems -ne 0 -or $pairGraphPriorityProblems -ne 0 -or $modelActivationProblems -ne 0 -or $legacyWeakSnapshotProblems -ne 0 -or $logicalLadderMetadataProblems -ne 0 -or $leafModeOverrideProblems -ne 0 -or $runRootProblems -ne 0 -or $attemptSelfCheckProblems -ne 0 -or $trxCounterSelfCheckProblems -ne 0 -or $leafTimingSelfCheckProblems -ne 0 -or $leafTheoryExtraSelfCheckProblems -ne 0 -or $leafIncompleteSelfCheckProblems -ne 0 -or $leafWrongMembershipSelfCheckProblems -ne 0 -or $leafMissingMembershipSelfCheckProblems -ne 0 -or $minRowsEvidenceSelfCheckProblems -ne 0 -or $exactHumanBatchAssignmentMapSelfCheckProblems -ne 0 -or $preparedFilterParsingSelfCheckProblems -ne 0 -or $canonicalLeafClassAssignmentSelfCheckProblems -ne 0 -or $classFirstMethodEvidenceSelfCheckProblems -ne 0 -or $emptyResidualEvidenceSelfCheckProblems -ne 0 -or $focusedBindingSelfCheckProblems -ne 0 -or $fingerprintSelfCheckProblems -ne 0 -or $designConversionDpoStorageClassMethodPackSelfCheckProblems -ne 0 -or $runInvocationHistorySelfCheckProblems -ne 0 -or $legacyBatchSelfCheckProblems -ne 0 -or $latestJournalProjectionSelfCheckProblems -ne 0 -or $legacyQwenDynamicSkipSelfCheckProblems -ne 0 -or $focusedMethodProblems -ne 0 -or $errors -ne 0) {
    exit 1
}

exit 0
