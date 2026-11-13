// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class EntityImpactMapperDpoGenerator
{
    private readonly EntityImpactMapperUnit _mapper;

    public EntityImpactMapperDpoGenerator(EntityImpactMapperUnit? mapper = null)
    {
        _mapper = mapper ?? new EntityImpactMapperUnit();
    }

    public Task<OperationResult<EntityImpactMapperDpoBatch>> BuildBatchAsync(
        string batchId = "entity-impact-mapper-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<EntityImpactMapperDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var seeds = BuildSeeds(batchId, heldOut);
        var compact = batchId.Contains("compact", StringComparison.OrdinalIgnoreCase);
        var calculationStrong = batchId.Contains("calculation-strong", StringComparison.OrdinalIgnoreCase);
        var derivedStatus = batchId.Contains("derived-status", StringComparison.OrdinalIgnoreCase);
        var calculationPath = batchId.Contains("calculation-path", StringComparison.OrdinalIgnoreCase);
        var calculationSignal = batchId.Contains("calculation-signal", StringComparison.OrdinalIgnoreCase);
        var coverageSignal = batchId.Contains("coverage-signal", StringComparison.OrdinalIgnoreCase);
        var dpoSeparationSignal =
            batchId.Contains("dpo-separation-remediation", StringComparison.OrdinalIgnoreCase) ||
            batchId.Contains("signal-correction", StringComparison.OrdinalIgnoreCase);
        var pairs = seeds.Select((seed, index) =>
        {
            var mapped = _mapper.Map(seed.ProjectId, seed.EntityName, seed.EvidenceRecords).Value!;
            return new EntityImpactMapperDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.ProjectId,
                seed.EntityName,
                seed.EvidenceRecords,
                mapped.EvidenceIds,
                Touchpoints(mapped),
                BuildPrompt(seed, mapped, calculationPath),
                BuildChosen(seed.RejectedType, mapped, compact, calculationStrong, derivedStatus, calculationPath, calculationSignal, coverageSignal, dpoSeparationSignal),
                BuildRejected(seed.RejectedType, mapped, compact, calculationStrong, derivedStatus, calculationPath, calculationSignal, coverageSignal, dpoSeparationSignal),
                seed.RejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<EntityImpactMapperDpoBatch>.Ok(new EntityImpactMapperDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildPrompt(EntityImpactSeed seed, EntityImpactMapResult map, bool calculationPath)
    {
        if (!calculationPath)
            return $"entity_name: {seed.EntityName}\nevidence_records: {seed.EvidenceRecords.Length}";

        return string.Join("\n", new[]
        {
            $"developer_question: I want to add or change {seed.FieldName} on {seed.EntityName}. Where does the value travel and what calculations, branches, outputs, and tests can change?",
            $"entity_name: {seed.EntityName}",
            $"field_name: {seed.FieldName}",
            "required_answer_shape: input validation mapping calculation branch persistence output tests missing_evidence",
            $"available_calculation_touchpoints: {string.Join(", ", map.CalculationTouchpoints)}",
            $"evidence_records: {seed.EvidenceRecords.Length}"
        });
    }

    private static string BuildChosen(string rejectedType, EntityImpactMapResult map, bool compact, bool calculationStrong, bool derivedStatus, bool calculationPath, bool calculationSignal, bool coverageSignal, bool dpoSeparationSignal)
    {
        if (dpoSeparationSignal)
            return BuildDpoSeparationChosen(rejectedType, map);

        if (coverageSignal)
            return rejectedType switch
            {
                "missing_calculation" => BuildCalculationSignalChosen(map),
                "missing_persistence" => BuildPersistenceSignalChosen(map),
                "wrong_project_evidence" => BuildBoundarySignalChosen(map),
                _ => BuildCalculationSignalChosen(map)
            };

        if (calculationSignal && string.Equals(rejectedType, "missing_calculation", StringComparison.Ordinal))
            return BuildCalculationSignalChosen(map);

        if (calculationPath)
            return BuildCalculationPathChosen(map);

        if (compact)
        {
            var lines = new List<string>
            {
                "impact_verdict: complete_entity_change_path",
                $"entity: {map.EntityName}",
                "input_validation_mapping_calculation_persistence_output_tests: present",
                "evidence_scope: same_project",
                "partial_map: false",
                "unknown_impact: explicitly_listed_if_present"
            };
            if (calculationStrong)
                lines.Add("calculation_status: derived_values_totals_scores_discounts_and_workflow_effects_checked");
            if (derivedStatus)
                lines.Add("derived_value_status: totals_scores_discounts_and_workflow_effects_checked");
            return string.Join("\n", lines);
        }

        return BuildFullChosen(map);
    }

    private static string BuildCalculationSignalChosen(EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            "impact_verdict: complete_entity_change_path",
            $"entity: {map.EntityName}",
            "input_validation_mapping_calculation_persistence_output_tests: present",
            "calculation_path: present",
            "formula_branch_status_output_test: checked",
            "value_flow: input_mapper_calculation_storage_projection",
            "evidence_scope: same_project",
            "partial_map: false"
        });

    private static string BuildPersistenceSignalChosen(EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            "PASS_ENTITY_IMPACT: persistence_path_present same_project_complete_touchpoint_map",
            "impact_verdict: complete_entity_change_path",
            $"entity: {map.EntityName}",
            "input_validation_mapping_calculation_persistence_output_tests: present",
            "persistence_path: present",
            "save_load_storage_result: checked",
            "value_flow: input_mapper_calculation_storage_projection",
            "evidence_scope: same_project",
            "partial_map: false"
        });

    private static string BuildBoundarySignalChosen(EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            "PASS_ENTITY_IMPACT: same_project_boundary_verified foreign_evidence_rejected",
            "impact_verdict: complete_entity_change_path",
            $"entity: {map.EntityName}",
            "input_validation_mapping_calculation_persistence_output_tests: present",
            "project_boundary: verified",
            "foreign_evidence: rejected",
            "evidence_scope: same_project",
            "partial_map: false"
        });

    private static string BuildDpoSeparationChosen(string rejectedType, EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            "accepted_entity_impact_answer: complete same_project evidence_grounded",
            $"entity: {map.EntityName}",
            "touchpoint_path: input validation mapping calculation branch persistence output tests",
            "input_path: present present present",
            "validation_path: present present present",
            "mapping_path: present present present",
            "mapper_value_flow: present present present",
            "field_copy_transform_drop: checked checked checked",
            "calculation_path: present present present",
            "persistence_path: present present present",
            "output_path: present present present",
            "test_path: present present present",
            "project_boundary: verified",
            "foreign_evidence: rejected",
            "missing_evidence_policy: list_unknowns_do_not_guess",
            $"rejected_defect_checked: {rejectedType}",
            "verdict: accepted_complete_entity_impact_answer"
        });

    private static string BuildCalculationPathChosen(EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            "impact_verdict: complete_developer_change_impact_path",
            $"entity: {map.EntityName}",
            "touchpoint_path: input -> validation -> mapping -> calculation -> branch -> persistence -> output -> tests",
            $"calculation_impact_path: {string.Join(", ", map.CalculationTouchpoints)}",
            $"value_flow_inputs: {string.Join(", ", map.InputTouchpoints)}",
            $"mapper_and_storage_path: {string.Join(", ", map.MappingTouchpoints.Concat(map.PersistenceTouchpoints))}",
            $"output_and_test_path: {string.Join(", ", map.OutputTouchpoints.Concat(map.TestTouchpoints))}",
            "calculation_questions_answered: formula_inputs, derived_totals, branch_conditions, status_changes, response_projection, regression_tests",
            "missing_evidence_policy: list_unknowns_do_not_guess",
            "evidence_scope: same_project"
        });

    private static string BuildFullChosen(EntityImpactMapResult map)
        => string.Join("\n", new[]
        {
            $"entity: {map.EntityName}",
            $"input: {string.Join(", ", map.InputTouchpoints)}",
            $"validation: {string.Join(", ", map.ValidationTouchpoints)}",
            $"mapping: {string.Join(", ", map.MappingTouchpoints)}",
            $"calculation: {string.Join(", ", map.CalculationTouchpoints)}",
            $"persistence: {string.Join(", ", map.PersistenceTouchpoints)}",
            $"output: {string.Join(", ", map.OutputTouchpoints)}",
            $"tests: {string.Join(", ", map.TestTouchpoints)}",
            $"unknown: {string.Join(", ", map.UnknownTouchpoints)}",
            $"evidence_ids: {string.Join(", ", map.EvidenceIds)}"
        });

    private static string BuildRejected(string rejectedType, EntityImpactMapResult map, bool compact, bool calculationStrong, bool derivedStatus, bool calculationPath, bool calculationSignal, bool coverageSignal, bool dpoSeparationSignal)
    {
        if (dpoSeparationSignal)
            return BuildDpoSeparationRejected(rejectedType, map);

        if (coverageSignal)
            return BuildCoverageSignalRejected(rejectedType, map);

        if (calculationSignal && string.Equals(rejectedType, "missing_calculation", StringComparison.Ordinal))
            return BuildCalculationSignalRejected(rejectedType, map);

        if (calculationPath)
            return BuildCalculationPathRejected(rejectedType, map);

        if (compact)
        {
            var wrong = rejectedType switch
            {
                "missing_persistence" =>
                    ("missing_persistence", "impact_verdict: incomplete_entity_change_path\npersistence_status: missing_repository_save_touchpoints", "hides repository/save touchpoints, so a developer adding a field misses where data is stored."),
                "missing_mapping" =>
                    ("missing_mapping", "impact_verdict: incomplete_entity_change_path\nmapping_status: missing_dto_entity_conversion_touchpoints", "omits DTO/entity conversion touchpoints where a new field commonly disappears."),
                "missing_calculation" =>
                    ("missing_calculation", derivedStatus
                        ? "impact_verdict: incomplete_entity_change_path\nderived_value_status: omitted\nworkflow_effect_status: omitted"
                        : calculationStrong
                        ? "impact_verdict: incomplete_entity_change_path\ncalculation_status: omitted_derived_values_totals_scores_discounts_and_workflow_effects"
                        : "impact_verdict: incomplete_entity_change_path\ncalculation_status: missing_derived_value_workflow_impact", "fails to check calculations and derived values affected by the entity change."),
                "wrong_project_evidence" =>
                    ("wrong_project_evidence", "impact_verdict: source_conflicting_entity_map\nevidence_scope: other_project", "uses evidence from another project and cannot safely answer local wiki questions."),
                _ =>
                    ("partial_touchpoint_map", "impact_verdict: partial_entity_change_path\ncoverage_status: one_touchpoint_only", "records only one touchpoint and misses the end-to-end path developers need.")
            };
            return $"{wrong.Item2}\n[WRONG: {wrong.Item1} {wrong.Item3}]";
        }

        return BuildFullRejected(rejectedType, map);
    }

    private static string BuildCalculationSignalRejected(string rejectedType, EntityImpactMapResult map)
        => rejectedType switch
        {
            "missing_persistence" =>
                $"impact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\npersistence_path: absent\nstorage_result: omitted\n[WRONG: missing_persistence hides save/load touchpoints and makes the developer miss where the changed value is stored.]",
            "missing_mapping" =>
                $"impact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\nmapper_value_flow: absent\nfield_copy_transform_drop: unchecked\n[WRONG: missing_mapping omits DTO/entity conversion where the changed value is copied, transformed, or dropped.]",
            "missing_calculation" =>
                $"impact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\ncalculation_path: absent\nformula_branch_status_output_test: omitted\n[WRONG: missing_calculation omits formulas, branches, status changes, outputs, and tests affected by the field change.]",
            "wrong_project_evidence" =>
                $"impact_verdict: source_conflicting_entity_map\nentity: {map.EntityName}\nevidence_scope: other_project\n[WRONG: wrong_project_evidence uses another project's entity path and cannot answer local impact questions.]",
            _ =>
                $"impact_verdict: partial_entity_change_path\nentity: {map.EntityName}\ncoverage_status: one_touchpoint_only\n[WRONG: partial_touchpoint_map misses the full input, mapping, calculation, storage, output, and test chain.]"
        };

    private static string BuildCoverageSignalRejected(string rejectedType, EntityImpactMapResult map)
        => rejectedType switch
        {
            "missing_persistence" =>
                $"FAIL_ENTITY_IMPACT: missing_persistence persistence_path_absent save_load_storage_omitted\nimpact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\npersistence_path: absent\nsave_load_storage_result: omitted\n[WRONG: missing_persistence hides storage writes and reads, so the developer cannot know where the changed value is persisted.]",
            "missing_mapping" =>
                $"impact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\nmapper_value_flow: absent\nfield_copy_transform_drop: unchecked\n[WRONG: missing_mapping omits DTO/entity conversion where the changed value is copied, transformed, or dropped.]",
            "missing_calculation" =>
                $"impact_verdict: incomplete_entity_change_path\nentity: {map.EntityName}\ncalculation_path: absent\nformula_branch_status_output_test: omitted\n[WRONG: missing_calculation omits formulas, branches, status changes, outputs, and tests affected by the field change.]",
            "wrong_project_evidence" =>
                $"FAIL_ENTITY_IMPACT: wrong_project_evidence project_boundary_violated foreign_evidence_accepted\nimpact_verdict: source_conflicting_entity_map\nentity: {map.EntityName}\nproject_boundary: violated\nforeign_evidence: accepted\nevidence_scope: other_project\n[WRONG: wrong_project_evidence uses another project's entity path and cannot answer local impact questions.]",
            _ =>
                $"impact_verdict: partial_entity_change_path\nentity: {map.EntityName}\ncoverage_status: one_touchpoint_only\n[WRONG: partial_touchpoint_map misses the full input, mapping, calculation, storage, output, and test chain.]"
        };

    private static string BuildDpoSeparationRejected(string rejectedType, EntityImpactMapResult map)
        => rejectedType switch
        {
            "missing_persistence" =>
                $"entity: {map.EntityName}\npersistence_path: absent\n[WRONG: missing_persistence hides save/load storage touchpoints.]",
            "wrong_project_evidence" =>
                $"entity: {map.EntityName}\nevidence_scope: other_project\n[WRONG: wrong_project_evidence uses another project's entity path.]",
            "missing_mapping" =>
                $"entity: {map.EntityName}\nmapping_path: absent\n[WRONG: missing_mapping omits DTO/entity conversion touchpoints.]",
            "dto_conversion_omitted" =>
                $"entity: {map.EntityName}\ndto_conversion_path: absent\n[WRONG: dto_conversion_omitted removes DTO to entity conversion touchpoints.]",
            "missing_calculation" =>
                $"entity: {map.EntityName}\ncalculation_path: absent\n[WRONG: missing_calculation omits derived calculation impact.]",
            _ =>
                $"entity: {map.EntityName}\ncoverage_status: one_touchpoint_only\n[WRONG: partial_touchpoint_map misses the full entity change path.]"
        };

    private static string BuildCalculationPathRejected(string rejectedType, EntityImpactMapResult map)
        => rejectedType switch
        {
            "missing_persistence" =>
                $"impact_verdict: incomplete_developer_change_impact_path\nentity: {map.EntityName}\ntouchpoint_path: input -> validation -> mapping -> calculation -> output\nstorage_write_path: absent\n[WRONG: missing_persistence hides repository/save touchpoints, so a developer can update formulas and responses without storing the changed value.]",
            "missing_mapping" =>
                $"impact_verdict: incomplete_developer_change_impact_path\nentity: {map.EntityName}\ntouchpoint_path: input -> validation -> calculation -> persistence -> output\nmapper_and_value_copy_path: absent\n[WRONG: missing_mapping omits DTO/entity conversion where the new field is copied, transformed, dropped, or renamed before calculations run.]",
            "missing_calculation" =>
                $"impact_verdict: incomplete_developer_change_impact_path\nentity: {map.EntityName}\ntouchpoint_path: input -> validation -> mapping -> persistence -> output\ncalculation_impact_path: absent\nbranch_conditions: not_checked\nresponse_projection_impact: not_checked\nregression_test_impact: not_checked\n[WRONG: missing_calculation omits formula inputs, derived totals, branch conditions, status changes, output projection, and regression tests for the developer field change.]",
            "wrong_project_evidence" =>
                $"impact_verdict: source_conflicting_entity_map\nentity: {map.EntityName}\nevidence_scope: other_project\ncalculation_impact_path: borrowed_from_unrelated_project\n[WRONG: wrong_project_evidence uses another project calculation path and cannot safely answer local wiki impact questions.]",
            _ =>
                $"impact_verdict: partial_developer_change_impact_path\nentity: {map.EntityName}\ntouchpoint_path: only_first_symbol\ncalculation_impact_path: not_connected_to_callers_or_outputs\n[WRONG: partial_touchpoint_map lists one symbol but misses the value-flow, calculation, storage, output, and test chain a developer needs.]"
        };

    private static string BuildFullRejected(string rejectedType, EntityImpactMapResult map)
        => rejectedType switch
        {
            "missing_persistence" =>
                $"entity: {map.EntityName}\ninput: {string.Join(", ", map.InputTouchpoints)}\noutput: {string.Join(", ", map.OutputTouchpoints)}\n[WRONG: missing_persistence hides repository/save touchpoints, so a developer adding a field misses where data is stored.]",
            "missing_mapping" =>
                $"entity: {map.EntityName}\ninput: {string.Join(", ", map.InputTouchpoints)}\npersistence: {string.Join(", ", map.PersistenceTouchpoints)}\n[WRONG: missing_mapping omits DTO/entity conversion touchpoints where a new field commonly disappears.]",
            "missing_calculation" =>
                $"entity: {map.EntityName}\ninput: {string.Join(", ", map.InputTouchpoints)}\npersistence: {string.Join(", ", map.PersistenceTouchpoints)}\n[WRONG: missing_calculation fails to check calculations and derived values affected by the entity change.]",
            "wrong_project_evidence" =>
                $"entity: {map.EntityName}\nevidence_ids: other-project:Fake.Controller\n[WRONG: wrong_project_evidence uses evidence from another project and cannot safely answer local wiki questions.]",
            _ =>
                $"entity: {map.EntityName}\nevidence_ids: {string.Join(", ", map.EvidenceIds.Take(1))}\n[WRONG: partial_touchpoint_map only records one touchpoint and misses the end-to-end path developers need.]"
        };

    private static string[] Touchpoints(EntityImpactMapResult map)
        => map.InputTouchpoints
            .Concat(map.ValidationTouchpoints)
            .Concat(map.MappingTouchpoints)
            .Concat(map.CalculationTouchpoints)
            .Concat(map.PersistenceTouchpoints)
            .Concat(map.OutputTouchpoints)
            .Concat(map.TestTouchpoints)
            .Concat(map.UnknownTouchpoints)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static EntityImpactSeed[] BuildSeeds(string batchId, bool heldOut)
    {
        if (batchId.Contains("coverage-signal", StringComparison.OrdinalIgnoreCase))
            return BuildCoverageSignalSeeds(heldOut);
        if (batchId.Contains("calculation-signal", StringComparison.OrdinalIgnoreCase))
            return BuildCalculationSignalSeeds(heldOut);
        if (batchId.Contains("calculation-path", StringComparison.OrdinalIgnoreCase))
            return BuildCalculationPathSeeds(heldOut);
        if (batchId.Contains("missing-persistence-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "missing_persistence");
        if (batchId.Contains("missing-mapping-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "missing_mapping");
        if (batchId.Contains("mapping-alias-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "dto_conversion_omitted");
        if (batchId.Contains("no-mapping-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, index => index switch
            {
                < 15 => "missing_persistence",
                < 30 => "missing_calculation",
                < 40 => "wrong_project_evidence",
                _ => "partial_touchpoint_map"
            });
        if (batchId.Contains("missing-calculation-remediation", StringComparison.OrdinalIgnoreCase))
            return BuildSeeds(heldOut, _ => "missing_calculation");

        return BuildSeeds(heldOut, index => (index / 10) switch
        {
            0 => "missing_persistence",
            1 => "missing_mapping",
            2 => "missing_calculation",
            3 => "wrong_project_evidence",
            _ => "partial_touchpoint_map"
        });
    }

    private static EntityImpactSeed[] BuildSeeds(bool heldOut, Func<int, string> rejectedTypeForIndex)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var rows = new List<EntityImpactSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            foreach (var entity in entities)
            {
                var projectId = "generic-dotnet-docs";
                var rejected = rejectedTypeForIndex(rows.Count);
                rows.Add(new EntityImpactSeed(
                    projectId,
                    entity,
                    BuildEvidence(projectId, entity, repeat),
                    $"{entity}ImpactField",
                    rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static EntityImpactSeed[] BuildCalculationSignalSeeds(bool heldOut)
        => BuildSeeds(heldOut, index => (index / 10) switch
        {
            0 => "missing_persistence",
            1 => "missing_mapping",
            2 => "missing_calculation",
            3 => "wrong_project_evidence",
            _ => "partial_touchpoint_map"
        });

    private static EntityImpactSeed[] BuildCoverageSignalSeeds(bool heldOut)
        => BuildSeeds(heldOut, index => (index / 10) switch
        {
            0 => "missing_persistence",
            1 => "missing_calculation",
            2 => "wrong_project_evidence",
            3 => "missing_mapping",
            _ => "partial_touchpoint_map"
        });

    private static EntityImpactSeed[] BuildCalculationPathSeeds(bool heldOut)
    {
        var entities = heldOut
            ? new[] { "Invoice", "Profile", "Message", "Shipment", "Ticket", "Payment", "Subscription", "Report", "Alert", "Session" }
            : new[] { "Campaign", "Order", "Client", "DietPlan", "MediaAsset", "Task", "Game", "Article", "Booking", "Survey" };
        var fields = heldOut
            ? new[] { "TaxRegion", "PriorityTier", "RetryCount", "DeliveryZone", "EscalationScore", "PaymentRisk", "RenewalDiscount", "ExportFormat", "AlertSeverity", "IdleTimeout" }
            : new[] { "DiscountCode", "LineItemWeight", "CalorieTarget", "DailyProteinGoal", "EncodingProfile", "DueDateWeight", "DifficultyScore", "PublishWindow", "DepositPercent", "SurveyScore" };
        var rows = new List<EntityImpactSeed>(50);
        for (var repeat = 0; repeat < 5; repeat++)
        {
            for (var index = 0; index < entities.Length; index++)
            {
                var rejected = rows.Count switch
                {
                    < 20 => "missing_calculation",
                    < 30 => "missing_persistence",
                    < 40 => "missing_mapping",
                    < 45 => "wrong_project_evidence",
                    _ => "partial_touchpoint_map"
                };
                var projectId = "generic-dotnet-docs";
                rows.Add(new EntityImpactSeed(
                    projectId,
                    entities[index],
                    BuildCalculationPathEvidence(projectId, entities[index], fields[index], repeat),
                    fields[index],
                    rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private static ProjectWikiEvidenceRecord[] BuildEvidence(string projectId, string entity, int repeat)
        =>
        [
            Record(projectId, $"ev:{entity}:controller:{repeat}", $"{entity}Controller.Create{entity}", $"Controllers/{entity}Controller.cs", $"Accepts {entity}Dto input and starts the {entity} workflow.", [$"{entity}Dto dto"], $"ActionResult {entity} response", [$"{entity}Mapper.Map{entity}", $"{entity}Validator.Validate{entity}"]),
            Record(projectId, $"ev:{entity}:validator:{repeat}", $"{entity}Validator.Validate{entity}", $"Validation/{entity}Validator.cs", $"Validates required {entity} fields before mapping.", [$"{entity}Dto dto"], "ValidationResult", []),
            Record(projectId, $"ev:{entity}:mapper:{repeat}", $"{entity}Mapper.Map{entity}", $"Mapping/{entity}Mapper.cs", $"Maps {entity}Dto to {entity} entity fields.", [$"{entity}Dto dto"], $"{entity}", []),
            Record(projectId, $"ev:{entity}:service:{repeat}", $"{entity}Service.Calculate{entity}Total", $"Services/{entity}Service.cs", $"Calculates {entity} total and derived workflow state.", [$"{entity} entity"], "decimal total", [$"{entity}Repository.Save{entity}"]),
            Record(projectId, $"ev:{entity}:repo:{repeat}", $"{entity}Repository.Save{entity}", $"Data/{entity}Repository.cs", $"Persists {entity} entity state to storage.", [$"{entity} entity"], "bool saved", []),
            Record(projectId, $"ev:{entity}:output:{repeat}", $"{entity}Controller.Get{entity}", $"Controllers/{entity}Controller.cs", $"Loads {entity} and serializes it for response.", ["int id"], $"{entity}Dto response", [$"{entity}Repository.Load{entity}"]),
            Record(projectId, $"ev:{entity}:test:{repeat}", $"{entity}FlowTests.ShouldSave{entity}", $"Tests/{entity}FlowTests.cs", $"Tests create, map, calculate, and persist behavior for {entity}.", [], "void", [])
        ];

    private static ProjectWikiEvidenceRecord[] BuildCalculationPathEvidence(string projectId, string entity, string field, int repeat)
        =>
        [
            Record(projectId, $"ev:{entity}:controller:{repeat}", $"{entity}Controller.Update{field}", $"Controllers/{entity}Controller.cs", $"Accepts {entity}Dto input with {field} and starts the update workflow.", [$"{entity}Dto dto"], $"ActionResult {entity} response", [$"{entity}Mapper.Map{field}", $"{entity}Validator.Validate{field}"]),
            Record(projectId, $"ev:{entity}:validator:{repeat}", $"{entity}Validator.Validate{field}", $"Validation/{entity}Validator.cs", $"Validates {field} before mapping or calculation.", [$"{entity}Dto dto"], "ValidationResult", []),
            Record(projectId, $"ev:{entity}:mapper:{repeat}", $"{entity}Mapper.Map{field}", $"Mapping/{entity}Mapper.cs", $"Copies {field} from DTO to {entity} entity for downstream calculations.", [$"{entity}Dto dto"], $"{entity}", []),
            Record(projectId, $"ev:{entity}:formula:{repeat}", $"{entity}CalculationService.Calculate{field}Total", $"Services/{entity}CalculationService.cs", $"Calculates derived total, score, or price using {field}.", [$"{entity} entity"], "decimal derivedTotal", [$"{entity}WorkflowService.Resolve{field}Branch"]),
            Record(projectId, $"ev:{entity}:branch:{repeat}", $"{entity}WorkflowService.Resolve{field}Branch", $"Services/{entity}WorkflowService.cs", $"Computes workflow branch and status change from {field} derived total.", [$"{entity} entity", "decimal derivedTotal"], $"{entity}WorkflowStatus", [$"{entity}Repository.Save{entity}"]),
            Record(projectId, $"ev:{entity}:repo:{repeat}", $"{entity}Repository.Save{entity}", $"Data/{entity}Repository.cs", $"Persists {entity} entity and derived {field} status to storage.", [$"{entity} entity"], "bool saved", []),
            Record(projectId, $"ev:{entity}:output:{repeat}", $"{entity}Projector.To{entity}Dto", $"Projection/{entity}Projector.cs", $"Projects {field}, derived total, and workflow status to API response.", [$"{entity} entity"], $"{entity}Dto response", []),
            Record(projectId, $"ev:{entity}:test:{repeat}", $"{entity}ImpactTests.ShouldRecalculate{field}", $"Tests/{entity}ImpactTests.cs", $"Tests that changing {field} recalculates totals, branches, persisted status, and response projection.", [], "void", [])
        ];

    private static ProjectWikiEvidenceRecord Record(
        string projectId,
        string evidenceId,
        string symbol,
        string sourcePath,
        string meaning,
        string[] parameters,
        string returns,
        string[] callees)
        => new()
        {
            EvidenceId = evidenceId,
            ProjectId = projectId,
            DocumentationVersion = "generic-v1",
            SymbolId = symbol,
            RelationSymbolId = symbol,
            DocPath = $"documentation/{symbol}.md",
            SourcePath = sourcePath,
            SourceLine = 10,
            DocumentationLevel = "D3",
            TargetFramework = "net8.0",
            CSharpLanguageVersion = "CSharp12",
            Meaning = meaning,
            Parameters = parameters,
            Returns = returns,
            CalledFunctions = callees,
            CalledBy = [],
            KnownCallees = callees
        };

    private sealed record EntityImpactSeed(
        string ProjectId,
        string EntityName,
        ProjectWikiEvidenceRecord[] EvidenceRecords,
        string FieldName,
        string RejectedType);
}
