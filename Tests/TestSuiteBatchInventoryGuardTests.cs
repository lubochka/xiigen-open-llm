// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;

namespace LlmFromScratch.Tests;

// Эти тесты кусают саму лестницу батчей, а не запускают suite: проверяется
// опись классов, first-match назначение и замороженный снимок legacy-состояния.
public sealed class TestSuiteBatchInventoryGuardTests
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
    private static readonly Lazy<ActualRepositoryInventoryContext> ActualRepositoryInventory =
        new(BuildActualRepositoryInventory, isThreadSafe: true);

    [Fact]
    public void CanonicalManifestRejectsSchemaOrderDuplicateAndExactMethodMutations()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var batchesPath = Path.Combine(owner.RootPath, "batches.tsv");
            WriteText(
                batchesPath,
                string.Join(
                    '\n',
                    TestSuiteBatchManifest.BatchHeader,
                    "02\tdup\tDuplicate one\tTests/bin/Release/net9.0/Synthetic.Tests.dll\tFullyQualifiedName~Alpha\t1\ttrue\tany\tSynthetic first row.",
                    "03\tdup\tDuplicate two\tTests/bin/Release/net9.0/Synthetic.Tests.dll\tFullyQualifiedName~Beta\t1\ttrue\tany\tSynthetic duplicate row.",
                    "04\texact\tExact method\tTests/bin/Release/net9.0/Synthetic.Tests.dll\tFullyQualifiedName=Synthetic.AlphaTests.Works\t1\ttrue\tany\tFocused syntax must stay outside canonical.",
                    "05\tshort\tShort row\tOnlyFive\tColumns") + "\n");

            var result = TestSuiteBatchManifest.ReadBatchRows(
                batchesPath,
                requireCanonicalFilters: true);

            Assert.Contains(result.Problems, problem => problem.StartsWith("batch_manifest_order_gap_line:", StringComparison.Ordinal));
            Assert.Contains("batch_manifest_duplicate_batch_id:dup", result.Problems);
            Assert.Contains("canonical_exact_method_filter_rejected:exact", result.Problems);
            Assert.Contains(result.Problems, problem => problem.StartsWith("batch_manifest_schema_line:", StringComparison.Ordinal));
        });

    [Fact]
    public void ManifestAssemblyCoverageRejectsUnknownAndMissingDll()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(repoRoot, "TestsA", "SyntheticA.Tests", ClassSource("Synthetic.AlphaTests"));
            CreateTestProject(repoRoot, "TestsB", "SyntheticB.Tests", ClassSource("Synthetic.BetaTests"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row("01", "alpha", "TestsA/bin/Release/net9.0/SyntheticA.Tests.dll", "FullyQualifiedName~Alpha"),
                    Row("02", "ghost", "Ghost/bin/Release/net9.0/Ghost.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);

            Assert.Contains("batch_manifest_missing_test_assembly:TestsB/bin/Release/net9.0/SyntheticB.Tests.dll", report.Problems);
            Assert.Contains("batch_manifest_unknown_dll:Ghost/bin/Release/net9.0/Ghost.Tests.dll", report.Problems);
        });

    [Fact]
    public void EffectiveAssignmentGuardRejectsOmissionAndDuplicateEffectiveRows()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "Synthetic.Tests",
                string.Join('\n', ClassSource("Synthetic.AlphaTests"), ClassSource("Synthetic.BetaTests")));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(Row("01", "alpha-only", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~Alpha")));

            var omission = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);

            Assert.Contains(omission.Problems, problem => problem.Contains("Synthetic.BetaTests:count=0", StringComparison.Ordinal));
            Assert.Contains(omission.Problems, problem => problem.Contains("batch_manifest_unassigned_class:Tests/bin/Release/net9.0/Synthetic.Tests.dll|Synthetic.BetaTests", StringComparison.Ordinal));

            WriteText(
                batchesPath,
                Manifest(
                    Row("01", "wildcard-a", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"),
                    Row("02", "wildcard-b", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*")));

            var duplicate = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);

            Assert.Contains(duplicate.Problems, problem => problem.Contains("Synthetic.AlphaTests:count=2", StringComparison.Ordinal));
            Assert.Contains(duplicate.Problems, problem => problem.Contains("Synthetic.BetaTests:count=2", StringComparison.Ordinal));
        });

    [Fact]
    public void AtomicGroupMovedToWrongBatchFails()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "Synthetic.Tests",
                ClassSource("Synthetic.DesignConversionLiveLocalServerTests"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row("01", "core-m-z-and-remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~DesignConversionLiveLocalServer"),
                    Row("02", "design-conversion-local-host", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*")));
            var options = TestSuiteBatchGuardOptions.Synthetic with
            {
                EnforceAtomicGroups = true,
                RequireAtomicGroupMembers = false
            };

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: options);

            Assert.Contains(report.Problems, problem => problem.StartsWith("atomic_group_wrong_batch:", StringComparison.Ordinal));
        });

    [Fact]
    public void SnapshotGuardRejectsMissingExtraWrongBatchWrongCountHashAndMovedPath()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "Synthetic.Tests",
                string.Join('\n', ClassSource("Synthetic.AlphaTests"), ClassSource("Synthetic.BetaTests")));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var snapshotPath = Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacySnapshot.txt");
            WriteText(
                batchesPath,
                Manifest(Row("01", "synthetic-all", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*")));
            var baseline = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var rows = TestSuiteBatchManifest.CreateSnapshotRows(baseline.Assignments).ToArray();
            WriteText(snapshotPath, TestSuiteBatchManifest.CreateSnapshotText(rows));

            var clean = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath,
                options: TestSuiteBatchGuardOptions.Synthetic with { RequireSnapshot = true });
            AssertNoProblems(clean);

            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows.Skip(1)),
                "legacy_snapshot_missing_current_row:");
            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows.Concat([new("Tests/bin/Release/net9.0/Synthetic.Tests.dll", "Synthetic.GammaTests", "Tests/GammaTests.cs", "synthetic-all")])),
                "legacy_snapshot_extra_stale_row:");
            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows.Select(row =>
                    row.ClassFqn == "Synthetic.AlphaTests"
                        ? row with { BatchId = "wrong-batch" }
                        : row)),
                "legacy_snapshot_missing_current_row:");
            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows).Replace("total_classes\t2", "total_classes\t99", StringComparison.Ordinal),
                "legacy_snapshot_total_mismatch:");
            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows).Replace("body_sha256\t", "body_sha256\t0000", StringComparison.Ordinal),
                "legacy_snapshot_hash_mismatch:");
            AssertSnapshotProblem(
                repoRoot,
                batchesPath,
                snapshotPath,
                TestSuiteBatchManifest.CreateSnapshotText(rows.Select(row =>
                    row.ClassFqn == "Synthetic.BetaTests"
                        ? row with { SourcePath = "Tests/MovedBetaTests.cs" }
                        : row)),
                "legacy_snapshot_extra_stale_row:");
        });

    [Fact]
    public void SnapshotHashUsesCanonicalLfForLfAndCrlfFiles()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "Synthetic.Tests",
                string.Join('\n', ClassSource("Synthetic.AlphaTests"), ClassSource("Synthetic.BetaTests")));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var snapshotPath = Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacySnapshot.txt");
            WriteText(
                batchesPath,
                Manifest(Row("01", "synthetic-all", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*")));

            var baseline = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var rows = TestSuiteBatchManifest.CreateSnapshotRows(baseline.Assignments).ToArray();
            var lfSnapshot = TestSuiteBatchManifest.CreateSnapshotText(rows);
            var crlfSnapshot = lfSnapshot.Replace("\n", "\r\n", StringComparison.Ordinal);

            Assert.False(lfSnapshot.Contains("\r\n", StringComparison.Ordinal), "Snapshot writer must emit LF-only text.");
            Assert.Equal(
                SnapshotHeaderValue(lfSnapshot, "body_sha256"),
                SnapshotHeaderValue(crlfSnapshot, "body_sha256"));

            WriteText(snapshotPath, lfSnapshot);
            AssertNoProblems(TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath,
                options: TestSuiteBatchGuardOptions.Synthetic with { RequireSnapshot = true }));

            WriteText(snapshotPath, crlfSnapshot);
            AssertNoProblems(TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath,
                options: TestSuiteBatchGuardOptions.Synthetic with { RequireSnapshot = true }));
        });

    [Fact]
    public void WeakSnapshotAcceptsCurrentWeakRowsAlreadyAllowedByBaseline()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                string.Join('\n', ClassSource("Synthetic.AlphaTests"), ClassSource("Synthetic.LegacyLooseTests")),
                Row("01", "alpha", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~Alpha"),
                Row("02", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var weakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));

            var report = ValidateWeakFixture(fixture);

            AssertNoProblems(report);
            Assert.Contains(weakRows, row => row.ClassFqn == "Synthetic.LegacyLooseTests");
            Assert.DoesNotContain(weakRows, row => row.ClassFqn == "Synthetic.AlphaTests");
        });

    [Fact]
    public void WeakSnapshotRejectsStrongClassLeftInActiveSnapshot()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                string.Join('\n', ClassSource("Synthetic.AlphaTests"), ClassSource("Synthetic.LegacyLooseTests")),
                Row("01", "alpha", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~Alpha"),
                Row("02", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var weakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            var strongRow = TestSuiteBatchManifest.CreateSnapshotRows(fixture.Report.Assignments)
                .Single(row => row.ClassFqn == "Synthetic.AlphaTests");
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(weakRows.Append(strongRow)));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(weakRows.Append(strongRow)));

            var report = ValidateWeakFixture(fixture);

            Assert.Contains(report.Problems, problem =>
                problem.StartsWith("legacy_weak_snapshot_active_row_has_strong_batch_signal_remove_from_snapshot:", StringComparison.Ordinal));
        });

    [Fact]
    public void WeakSnapshotRejectsMissingCurrentWeakRowFromActiveSnapshot()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                ClassSource("Synthetic.LegacyLooseTests"),
                Row("01", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var weakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(Array.Empty<TestSuiteBatchSnapshotRow>()));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));

            var report = ValidateWeakFixture(fixture);

            Assert.Contains(report.Problems, problem =>
                problem.StartsWith("legacy_weak_snapshot_missing_current_weak_row:", StringComparison.Ordinal));
        });

    [Fact]
    public void WeakSnapshotRejectsNewWeakClassOutsideBaselineWithHumanRoutingHint()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                ClassSource("Synthetic.LegacyLooseTests"),
                Row("01", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var weakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));
            WriteText(Path.Combine(owner.RootPath, "Tests", "Synthetic_NewLooseTests.cs"), ClassSource("Synthetic.NewLooseTests"));

            var report = ValidateWeakFixture(fixture);

            Assert.Contains(report.Problems, problem =>
                problem.StartsWith("new_test_class_without_strong_batch_signal:", StringComparison.Ordinal) &&
                problem.Contains("Не добавляйте его в active snapshot", StringComparison.Ordinal) &&
                problem.Contains("FullyQualifiedName~", StringComparison.Ordinal) &&
                problem.Contains("tools/test-batches/batches.tsv", StringComparison.Ordinal));
        });

    [Fact]
    public void WeakBaselineExternalAnchorRejectsGrowingBaselineEvenWhenHeadersAreInternallyValid()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                string.Join('\n', ClassSource("Synthetic.LegacyLooseTests"), ClassSource("Synthetic.NewLooseTests")),
                Row("01", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var allWeakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            var oldWeakRows = allWeakRows
                .Where(row => row.ClassFqn == "Synthetic.LegacyLooseTests")
                .ToArray();
            var immutableAnchor = SnapshotHeaderValue(
                TestSuiteBatchManifest.CreateSnapshotText(oldWeakRows),
                "body_sha256");

            // Этот сценарий имитирует опасную правку: новый слабый класс
            // добавили и в active snapshot, и в baseline, пересчитав локальные
            // header-хеши. Без внешнего якоря guard принял бы такой рост.
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(allWeakRows));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(allWeakRows));

            var report = ValidateWeakFixture(fixture, immutableAnchor);

            var problem = Assert.Single(report.Problems);
            Assert.StartsWith("legacy_weak_baseline_anchor_mismatch:", problem, StringComparison.Ordinal);
            Assert.Contains("baseline_is_immutable", problem, StringComparison.Ordinal);
        });

    [Fact]
    public void WeakSnapshotBareClassHintNamesDeclaredPrefixesInsteadOfSnapshotAppend()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                ClassSource("Synthetic.AlphaTests"),
                Row("01", "alpha", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~Alpha"),
                Row("02", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(Array.Empty<TestSuiteBatchSnapshotRow>()));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(Array.Empty<TestSuiteBatchSnapshotRow>()));
            WriteText(Path.Combine(owner.RootPath, "Tests", "BareWeakTests.cs"), ClassSource("BareWeakTests"));

            var report = ValidateWeakFixture(fixture);

            Assert.Contains(report.Problems, problem =>
                problem.Contains("BareWeakTests", StringComparison.Ordinal) &&
                problem.Contains("Класс попал только в wildcard-остаток", StringComparison.Ordinal) &&
                problem.Contains("alpha:FullyQualifiedName~Alpha", StringComparison.Ordinal));
        });

    [Fact]
    public void WeakSnapshotExplicitPrefixMakesNewClassStrongWithoutTouchingSnapshot()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                string.Join('\n', ClassSource("Synthetic.LegacyLooseTests"), ClassSource("Synthetic.GammaTests")),
                Row("01", "gamma", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~Gamma"),
                Row("02", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var weakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(weakRows));

            var report = ValidateWeakFixture(fixture);

            AssertNoProblems(report);
            Assert.DoesNotContain(weakRows, row => row.ClassFqn == "Synthetic.GammaTests");
        });

    [Fact]
    public void WeakSnapshotCanShrinkAfterExistingWeakClassGetsStrongSignal()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var fixture = CreateWeakSnapshotFixture(
                owner.RootPath,
                ClassSource("Synthetic.LegacyLooseTests"),
                Row("01", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*"));
            var legacyWeakRows = TestSuiteBatchManifest.CreateWeakSnapshotRows(fixture.Report.Assignments).ToArray();
            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(legacyWeakRows));
            WriteText(fixture.WeakBaselinePath, TestSuiteBatchManifest.CreateSnapshotText(legacyWeakRows));
            WriteText(
                fixture.BatchesPath,
                Manifest(
                    Row("01", "remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "FullyQualifiedName~LegacyLoose"),
                    Row("02", "later-remainder", "Tests/bin/Release/net9.0/Synthetic.Tests.dll", "*")));

            var staleActive = ValidateWeakFixture(fixture);
            Assert.Contains(staleActive.Problems, problem =>
                problem.StartsWith("legacy_weak_snapshot_active_row_has_strong_batch_signal_remove_from_snapshot:", StringComparison.Ordinal));

            WriteText(fixture.ActiveSnapshotPath, TestSuiteBatchManifest.CreateSnapshotText(Array.Empty<TestSuiteBatchSnapshotRow>()));
            AssertNoProblems(ValidateWeakFixture(fixture));
        });

    [Fact]
    public void LinkedCompileSourceHasSeparateAssemblyIdentity()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            WriteText(Path.Combine(repoRoot, "SharedLinkedTests.cs"), ClassSource("Synthetic.SharedLinkedTests"));
            CreateTestProject(repoRoot, "TestsA", "SyntheticA.Tests", source: null, linkedSources: ["..\\SharedLinkedTests.cs"]);
            CreateTestProject(repoRoot, "TestsB", "SyntheticB.Tests", source: null, linkedSources: ["../SharedLinkedTests.cs"]);
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row("01", "assembly-a", "TestsA/bin/Release/net9.0/SyntheticA.Tests.dll", "*"),
                    Row("02", "assembly-b", "TestsB/bin/Release/net9.0/SyntheticB.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);

            AssertNoProblems(report);
            Assert.Equal(
                ["TestsA/bin/Release/net9.0/SyntheticA.Tests.dll", "TestsB/bin/Release/net9.0/SyntheticB.Tests.dll"],
                report.Classes.Select(testClass => testClass.Dll).Order(StringComparer.Ordinal).ToArray());
            Assert.All(report.Classes, testClass => Assert.Equal("Synthetic.SharedLinkedTests", testClass.FullyQualifiedName));
        });

    [Fact]
    public void ActualRepositoryInventoryMatchesCheckedInSnapshot()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot);

        // Windows-only сборка не исполняется в Linux/WSL, но остается частью
        // описи: guard должен заметить ее исчезновение из canonical-лестницы.
        AssertNoProblems(report);
        Assert.Equal(TestSuiteBatchManifest.CanonicalBatchCount, report.Rows.Count);
        Assert.Equal(3, report.Projects.Count);
        Assert.Equal(report.Classes.Count, report.Assignments.Count);
        Assert.Contains(report.Classes, testClass =>
            testClass.Dll == "Tests.ApiDashboardWpf/bin/Release/net9.0-windows/Tests.ApiDashboardWpf.dll" &&
            testClass.FullyQualifiedName == "ApiDashboardWindowInteractiveTests");
    }

    [Fact]
    public void ActualRepositoryLogicalLadderMetadataMapsSixteenPlannedBatchesToPhysicalRows()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var metadata = TestSuiteBatchManifest.ReadLogicalLadderMetadata(batchesPath);

        AssertNoProblems(report);
        Assert.Empty(metadata.Problems);
        Assert.Equal(TestSuiteBatchManifest.LogicalLadderVersion, metadata.Version);
        Assert.Equal(TestSuiteBatchManifest.CanonicalBatchCount, metadata.DeclaredPhysicalRowCount);
        Assert.Equal(TestSuiteBatchManifest.PlannedLogicalBatchCount, metadata.LogicalBatches.Count);
        Assert.Equal(TestSuiteBatchManifest.LogicalRuleCount, metadata.Rules.Count);

        var mappedPhysicalRows = metadata.LogicalBatches
            .SelectMany(batch => batch.PhysicalBatchIds)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var canonicalPhysicalRows = report.Rows
            .Select(row => row.BatchId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(canonicalPhysicalRows, mappedPhysicalRows);

        // Лестница групп тестов как живой файл и правило классификации
        // (внутренний номер 249) запрещает прятать настольную Windows-сборку
        // ради зелёного числа: логическая строка обязана явно показывать
        // физическую WPF-группу и причину статуса только для Windows.
        var windowsDesktop = Assert.Single(
            metadata.LogicalBatches,
            batch => batch.PhysicalBatchIds.Contains("api-dashboard-wpf-windows", StringComparer.Ordinal));
        Assert.Equal("16", windowsDesktop.Order);
        Assert.Contains("windows", windowsDesktop.HumanStatus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LogicalLadderMetadataRejectsUnknownDuplicateAndUnmappedPhysicalRows()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);

            AssertLogicalLadderMetadataProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    "core-m-z-and-remainder\tplanned_batch=14",
                    "ghost-core-remainder\tplanned_batch=14",
                    StringComparison.Ordinal),
                "logical_ladder_unknown_physical_batch:ghost-core-remainder");

            AssertLogicalLadderMetadataProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    "core-a-f,core-g-l,logical-topology-and-result-gap\tplanned_batch=13",
                    "core-a-f,core-g-l,logical-topology-and-result-gap,core-m-z-and-remainder\tplanned_batch=13",
                    StringComparison.Ordinal),
                "logical_ladder_duplicate_physical_batch:core-m-z-and-remainder");

            AssertLogicalLadderMetadataProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    ",core-m-z-and-remainder\tplanned_batch=14",
                    "\tplanned_batch=14",
                    StringComparison.Ordinal),
                "logical_ladder_unmapped_physical_batch:core-m-z-and-remainder");
        });

    [Fact]
    public void ActualRepositoryRepresentativeClassesFollowSevenStepLadder()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var report = TestSuiteBatchManifest.ValidateRepository(actualRepository.RepoRoot, actualRepository.Inventory);

        AssertNoProblems(report);

        var expectedAssignments = new[]
        {
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "DesignConversionLiveLocalServerTests", "design-conversion-live-local-server-method-diagnostic"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "ServerOwnedContinuousLearningRegistryRootCapabilityTests", "platform-capability-oracles"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "DesignConversionDpoStorageTests", "design-conversion-dpo-storage"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "DesignConversionModelServerApiTests", "design-conversion-model-server-free"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "DesignConversionArtifactAccessPolicyTests", "design-conversion-remainder-a-m"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "Tests.DynamicGraphAccuracyMeterTests", "dynamic-graph"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "LlmFromScratch.Tests.TranslationDomainTests", "teaching-translation-dpo"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "PromptToTaskContractsTests", "orchestration-skills-wiki"),
            ("Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "LlmFromScratch.Tests.AdamWTests", "core-a-f"),
        };

        // Эти примеры привязывают шаги лестницы к реально найденным классам.
        // Доказательство здесь — идентичность класса, а не число строк теории:
        // один метод с несколькими наборами данных всё равно имеет одного
        // владельца группы.
        foreach (var (dll, classFqn, expectedBatchId) in expectedAssignments)
            AssertBatchAssignment(report, dll, classFqn, expectedBatchId);
    }

    [Fact]
    public void ActualRepositoryDesignConversionLiveLocalServerMethodDiagnosticBatchHasExactOneClassAndMethodMode()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = DesignConversionLiveLocalServerMethodDiagnosticClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "design-conversion-live-local-server-method-diagnostic")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(
            report.Rows,
            row => row.BatchId == "design-conversion-live-local-server-method-diagnostic");
        var localHostRow = Assert.Single(report.Rows, row => row.BatchId == "design-conversion-local-host");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal("01", manifestRow.Order);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(60, manifestRow.MinRows);
        // Точка в фильтре отделяет класс от Extended/lookalike, а старая
        // строка всё ещё хранит семейный терм для других локальных серверных классов.
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.Contains("FullyQualifiedName~DesignConversionLiveLocalServer", localHostRow.Filter, StringComparison.Ordinal);
        Assert.DoesNotContain("FullyQualifiedName~DesignConversionLiveLocalServerTests.", localHostRow.Filter, StringComparison.Ordinal);
        Assert.DoesNotContain(report.Assignments, assignment =>
            assignment.BatchId == "design-conversion-local-host" &&
            assignment.TestClass.FullyQualifiedName == "DesignConversionLiveLocalServerTests");

        var overrideRows = TestSuiteBatchManifest.ReadLeafModeOverrides(
            Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv"),
            report.Rows);
        var overrideRow = Assert.Single(
            overrideRows.Rows,
            row => row.BatchId == "design-conversion-live-local-server-method-diagnostic");
        Assert.Equal("method-diagnostic", overrideRow.LeafMode);
    }

    [Fact]
    public void ActualRepositoryDesignConversionLiveLocalServerMethodDiagnosticBatchRejectsRenameMissingExtraAssemblyRemainderAndWrongOverride()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);
            const string batchId = "design-conversion-live-local-server-method-diagnostic";
            const string renamedBatchId = "renamed-design-conversion-live-local-server-method-diagnostic";
            const string liveServerTerm = "FullyQualifiedName~DesignConversionLiveLocalServerTests.";
            const string extraTerm = "|FullyQualifiedName~DesignConversionLiveLocalServerTestsExtended.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "design_conversion_live_local_server_method_diagnostic_wrong_batch:");
            AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(liveServerTerm, "FullyQualifiedName~DesignConversionLiveLocalServerTestsMoved.", StringComparison.Ordinal),
                "design_conversion_live_local_server_method_diagnostic_filter_missing_exact_class:");
            AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(liveServerTerm + "\t60\ttrue", liveServerTerm + extraTerm + "\t60\ttrue", StringComparison.Ordinal),
                "design_conversion_live_local_server_method_diagnostic_filter_unexpected_term:");
            AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tЛокальный сервер конверсии дизайна: методы по одному\t" + canonicalDll,
                    batchId + "\tЛокальный сервер конверсии дизайна: методы по одному\t" + wrongDll,
                    StringComparison.Ordinal),
                "design_conversion_live_local_server_method_diagnostic_wrong_dll:");

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "design_conversion_live_local_server_method_diagnostic_wrong_batch:");

            AssertDesignConversionLiveLocalServerMethodDiagnosticOverrideProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text,
                overrideText.Replace(
                    batchId + "\tmethod-diagnostic",
                    batchId + "\tpack",
                    StringComparison.Ordinal),
                "design_conversion_live_local_server_method_diagnostic_leaf_mode_override_wrong_mode:");
        });

    // Проверяет живую настройку: класс хранилища DPO делится по методам только в своей группе.
    [Fact]
    public void ActualRepositoryDesignConversionDpoStorageBatchUsesClassMethodPackOverride()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var manifestRow = Assert.Single(
            report.Rows,
            row => row.BatchId == "design-conversion-dpo-storage");

        AssertNoProblems(report);
        Assert.Equal("08", manifestRow.Order);
        Assert.Contains("FullyQualifiedName~DesignConversionDpoStorage", manifestRow.Filter, StringComparison.Ordinal);
        Assert.Contains(report.Assignments, assignment =>
            assignment.BatchId == "design-conversion-dpo-storage" &&
            assignment.TestClass.FullyQualifiedName == "DesignConversionDpoStorageTests");

        var overrideRows = TestSuiteBatchManifest.ReadLeafModeOverrides(
            Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv"),
            report.Rows);
        var overrideRow = Assert.Single(
            overrideRows.Rows,
            row => row.BatchId == "design-conversion-dpo-storage");
        Assert.Equal("class-method-pack", overrideRow.LeafMode);
    }

    // Проверяет защиту от поломок: пропущенная, неверная или чужая настройка сразу отвергается.
    [Fact]
    public void ActualRepositoryDesignConversionDpoStorageBatchRejectsMissingWrongAndForeignClassMethodPackOverride()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var batchesText = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);
            const string storageOverride = "design-conversion-dpo-storage\tclass-method-pack";

            AssertDesignConversionDpoStorageOverrideProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                batchesText,
                overrideText.Replace(
                    storageOverride,
                    "design-conversion-dpo-storage\tpack",
                    StringComparison.Ordinal),
                "design_conversion_dpo_storage_leaf_mode_override_wrong_mode:");

            var withoutStorageOverride = string.Join(
                '\n',
                overrideText.Split('\n').Where(line => !line.Contains(storageOverride, StringComparison.Ordinal)));
            AssertDesignConversionDpoStorageOverrideProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                batchesText,
                withoutStorageOverride + "\n",
                "design_conversion_dpo_storage_leaf_mode_override_row_count:");

            AssertDesignConversionDpoStorageOverrideProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                batchesText,
                overrideText.Replace(
                    "test-infrastructure-evidence-measurement\tmethod-diagnostic",
                    "test-infrastructure-evidence-measurement\tclass-method-pack",
                    StringComparison.Ordinal),
                "leaf_mode_override_class_method_pack_wrong_batch:test-infrastructure-evidence-measurement");
        });

    [Fact]
    public void DesignConversionLiveLocalServerMethodDiagnosticClassBoundaryRejectsSyntheticLookalikesWithoutAssigningThemToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                source: null);
            WriteText(
                Path.Combine(repoRoot, "Tests", "DesignConversionLiveLocalServerTests.cs"),
                ClassSource("DesignConversionLiveLocalServerTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "DesignConversionLiveLocalServerTestsExtended.cs"),
                ClassSource("DesignConversionLiveLocalServerTestsExtended"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "Synthetic_DesignConversionLiveLocalServerTests.cs"),
                ClassSource("Synthetic.DesignConversionLiveLocalServerTests"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "design-conversion-live-local-server-method-diagnostic",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        DesignConversionLiveLocalServerMethodDiagnosticClassBoundaryFilter()),
                    Row(
                        "02",
                        "design-conversion-local-host",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        "FullyQualifiedName~DesignConversionLiveLocalServer"),
                    Row("03", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));
            var options = TestSuiteBatchGuardOptions.Synthetic with
            {
                EnforceAtomicGroups = true,
                RequireAtomicGroupMembers = false
            };

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: options);
            var methodDiagnosticClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "design-conversion-live-local-server-method-diagnostic")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var localHostClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "design-conversion-local-host")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(DesignConversionLiveLocalServerMethodDiagnosticClassFqns(), methodDiagnosticClasses);
            Assert.Equal(
                ["DesignConversionLiveLocalServerTestsExtended", "Synthetic.DesignConversionLiveLocalServerTests"],
                localHostClasses);
        });

    [Fact]
    public void ActualRepositorySemanticCompilerSearchVectorBatchRejectsRenamedHumanGroup()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var renamedBatchesPath = Path.Combine(owner.RootPath, "renamed-semantic-search-batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string oldBatchId = "semantic-compiler-search-vector";
            const string renamedBatchId = "renamed-semantic-search-vector";

            Assert.Contains(oldBatchId, text, StringComparison.Ordinal);

            // Мутация меняет только имя группы. Фильтры остаются прежними, и
            // guard обязан понять, что смысловая группа потеряла свой адрес.
            WriteText(
                renamedBatchesPath,
                text.Replace(oldBatchId, renamedBatchId, StringComparison.Ordinal));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                renamedBatchesPath);
            var semanticProblems = report.Problems
                .Where(problem => problem.StartsWith("semantic_compiler_search_vector_wrong_batch:", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(10, semanticProblems.Length);
            Assert.All(semanticProblems, problem =>
            {
                Assert.Contains("expected=semantic-compiler-search-vector", problem, StringComparison.Ordinal);
                Assert.Contains("actual=renamed-semantic-search-vector", problem, StringComparison.Ordinal);
                Assert.Contains("semantic compiler, search and vector-search classes must stay in their named human batch", problem, StringComparison.Ordinal);
            });
        });

    [Fact]
    public void ActualRepositoryOrchestrationRoutingProductBoundariesBatchHasExactSevenClasses()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot);
        var expectedClasses = OrchestrationRoutingProductBoundaryClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "orchestration-routing-product-boundaries")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "orchestration-routing-product-boundaries");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..])
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.DoesNotContain("LlmFromScratch.Tests.ProjectTaskOrchestrationPipelineTests", actualClasses);
    }

    [Fact]
    public void ActualRepositoryOrchestrationRoutingProductBoundariesBatchRejectsRenameMissingExtraAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var inventory = actualRepository.Inventory;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "orchestration-routing-product-boundaries";
            const string renamedBatchId = "renamed-orchestration-routing-product-boundaries";
            const string reflectionTerm = "|FullyQualifiedName~LlmFromScratch.Tests.ReflectionWiringTests";
            const string extraTerm = "|FullyQualifiedName~LlmFromScratch.Tests.ProjectTaskOrchestrationPipelineTests";

            // Опись строим один раз: внутри метода четыре порчи TSV, и новый
            // обход репозитория для каждой порчи может снова подвесить guard.
            // Все плохие сценарии смотрят на один снимок, чтобы проверять только
            // конкретную мутацию batch-файла, а не меняющуюся живую опись.
            AssertOrchestrationBatchProblem(
                repoRoot,
                inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "orchestration_routing_product_boundaries_wrong_batch:");
            AssertOrchestrationBatchProblem(
                repoRoot,
                inventory,
                owner.RootPath,
                text.Replace(reflectionTerm, string.Empty, StringComparison.Ordinal),
                "orchestration_routing_product_boundaries_filter_missing_exact_class:");
            AssertOrchestrationBatchProblem(
                repoRoot,
                inventory,
                owner.RootPath,
                text.Replace(
                    "FullyQualifiedName~ThinClientApiContractTests\t7\ttrue",
                    "FullyQualifiedName~ThinClientApiContractTests" + extraTerm + "\t7\ttrue",
                    StringComparison.Ordinal),
                "orchestration_routing_product_boundaries_filter_unexpected_term:");

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertOrchestrationBatchProblem(
                repoRoot,
                inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "orchestration_routing_product_boundaries_wrong_batch:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchHasExactFiveClasses()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot);
        var expectedClasses = TestInfrastructureEvidenceMeasurementClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "test-infrastructure-evidence-measurement")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "test-infrastructure-evidence-measurement");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..])
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.DoesNotContain("LlmFromScratch.Tests.TopologyMutationFlowTests", actualClasses);

        var overrideRows = TestSuiteBatchManifest.ReadLeafModeOverrides(
            Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv"),
            report.Rows);
        var overrideRow = Assert.Single(
            overrideRows.Rows,
            row => row.BatchId == "test-infrastructure-evidence-measurement");
        Assert.Equal("method-diagnostic", overrideRow.LeafMode);
    }

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsRename()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "test-infrastructure-evidence-measurement";
            const string renamedBatchId = "renamed-test-infrastructure-evidence-measurement";

            AssertTestInfrastructureEvidenceMeasurementBatchProblem(
                repoRoot,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "test_infrastructure_evidence_measurement_wrong_batch:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsMissingMeasurementClass()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string measurementTerm = "|FullyQualifiedName~LlmFromScratch.Tests.MeasurementNoisePolicyTests";

            AssertTestInfrastructureEvidenceMeasurementBatchProblem(
                repoRoot,
                owner.RootPath,
                text.Replace(measurementTerm, string.Empty, StringComparison.Ordinal),
                "test_infrastructure_evidence_measurement_filter_missing_exact_class:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsUnexpectedTopologyClass()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string extraTerm = "|FullyQualifiedName~LlmFromScratch.Tests.TopologyMutationFlowTests";

            AssertTestInfrastructureEvidenceMeasurementBatchProblem(
                repoRoot,
                owner.RootPath,
                text.Replace(
                    "FullyQualifiedName~LlmFromScratch.Tests.WorkLeftRagEvidenceIndexTests\t5\ttrue",
                    "FullyQualifiedName~LlmFromScratch.Tests.WorkLeftRagEvidenceIndexTests" + extraTerm + "\t5\ttrue",
                    StringComparison.Ordinal),
                "test_infrastructure_evidence_measurement_filter_unexpected_term:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsReturnToRemainder()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "test-infrastructure-evidence-measurement";

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertTestInfrastructureEvidenceMeasurementBatchProblem(
                repoRoot,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "test_infrastructure_evidence_measurement_wrong_batch:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsWrongLeafModeOverride()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var batchesText = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);

            AssertTestInfrastructureEvidenceMeasurementOverrideProblem(
                repoRoot,
                owner.RootPath,
                batchesText,
                overrideText.Replace("method-diagnostic", "pack", StringComparison.Ordinal),
                "test_infrastructure_evidence_measurement_leaf_mode_override_wrong_mode:");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsDuplicateLeafModeOverride()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var batchesText = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);

            AssertTestInfrastructureEvidenceMeasurementOverrideProblem(
                repoRoot,
                owner.RootPath,
                batchesText,
                overrideText + overrideText.Split('\n').Last(line => line.Contains("test-infrastructure-evidence-measurement", StringComparison.Ordinal)) + "\n",
                "leaf_mode_override_duplicate_batch_id:test-infrastructure-evidence-measurement");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsUnknownLeafModeOverride()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var batchesText = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);

            AssertTestInfrastructureEvidenceMeasurementOverrideProblem(
                repoRoot,
                owner.RootPath,
                batchesText,
                overrideText.Replace("method-diagnostic", "made-up-mode", StringComparison.Ordinal),
                "leaf_mode_override_unknown_mode:test-infrastructure-evidence-measurement:made-up-mode");
        });

    [Fact]
    public void ActualRepositoryTestInfrastructureEvidenceMeasurementBatchRejectsUnknownLeafModeOverrideTarget()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var canonicalOverridePath = Path.Combine(repoRoot, "tools", "test-batches", "leaf-mode-overrides.tsv");
            var batchesText = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            var overrideText = File.ReadAllText(canonicalOverridePath, Utf8NoBom);

            AssertTestInfrastructureEvidenceMeasurementOverrideProblem(
                repoRoot,
                owner.RootPath,
                batchesText,
                overrideText.Replace("test-infrastructure-evidence-measurement", "missing-target-batch", StringComparison.Ordinal),
                "leaf_mode_override_unknown_batch:missing-target-batch");
        });

    [Fact]
    public void ActualRepositoryServerOwnedReplayManifestAttestationBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = ServerOwnedReplayManifestAttestationClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "server-owned-replay-manifest-attestation")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "server-owned-replay-manifest-attestation");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain(
            "LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike",
            actualClasses);
    }

    [Fact]
    public void ActualRepositoryServerOwnedReplayManifestAttestationBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "server-owned-replay-manifest-attestation";
            const string renamedBatchId = "renamed-server-owned-replay-manifest-attestation";
            const string serverOwnedTerm =
                "FullyQualifiedName~LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests.";
            const string extraTerm =
                "|FullyQualifiedName~LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            AssertServerOwnedReplayManifestAttestationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "server_owned_replay_manifest_attestation_wrong_batch:");
            AssertServerOwnedReplayManifestAttestationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(serverOwnedTerm, serverOwnedTerm.Replace("Tests.", "Tests.Moved", StringComparison.Ordinal), StringComparison.Ordinal),
                "server_owned_replay_manifest_attestation_filter_missing_exact_class:");
            AssertServerOwnedReplayManifestAttestationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(serverOwnedTerm + "\t5\ttrue", serverOwnedTerm + extraTerm + "\t5\ttrue", StringComparison.Ordinal),
                "server_owned_replay_manifest_attestation_filter_unexpected_term:");
            AssertServerOwnedReplayManifestAttestationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tСерверная проверка replay и паспорта исполняемого manifest\t" + canonicalDll,
                    batchId + "\tСерверная проверка replay и паспорта исполняемого manifest\t" + wrongDll,
                    StringComparison.Ordinal),
                "server_owned_replay_manifest_attestation_wrong_dll:");

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertServerOwnedReplayManifestAttestationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "server_owned_replay_manifest_attestation_wrong_batch:");
        });

    [Fact]
    public void ServerOwnedReplayManifestAttestationClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            var sources = ServerOwnedReplayManifestAttestationClassFqns()
                .Append("LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike")
                .Select(ClassSource);
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                string.Join('\n', sources));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "server-owned-replay-manifest-attestation",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        ServerOwnedReplayManifestAttestationClassBoundaryFilter()),
                    Row("02", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "server-owned-replay-manifest-attestation")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(ServerOwnedReplayManifestAttestationClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "server-owned-replay-manifest-attestation" &&
                assignment.TestClass.FullyQualifiedName == "LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike");
        });

    [Fact]
    public void ActualRepositoryPhase08CommonUserTwoLegLifecycleBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = Phase08CommonUserTwoLegLifecycleClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "phase08-common-user-two-leg-lifecycle")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "phase08-common-user-two-leg-lifecycle");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(3, manifestRow.MinRows);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain(
            "LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTestsLookalike",
            actualClasses);
    }

    [Fact]
    public void ActualRepositoryPhase08CommonUserTwoLegLifecycleBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "phase08-common-user-two-leg-lifecycle";
            const string renamedBatchId = "renamed-phase08-common-user-two-leg-lifecycle";
            const string phase08Term =
                "FullyQualifiedName~LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests.";
            const string extraTerm =
                "|FullyQualifiedName~LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "phase08_common_user_two_leg_lifecycle_wrong_batch:");
            AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(phase08Term, phase08Term.Replace("Tests.", "Tests.Moved", StringComparison.Ordinal), StringComparison.Ordinal),
                "phase08_common_user_two_leg_lifecycle_filter_missing_exact_class:");
            AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(phase08Term + "\t3\ttrue", phase08Term + extraTerm + "\t3\ttrue", StringComparison.Ordinal),
                "phase08_common_user_two_leg_lifecycle_filter_unexpected_term:");
            AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tPhase 08: переносимый профиль, одобренный обмен, продолжение обучения и доказательство отката\t" + canonicalDll,
                    batchId + "\tPhase 08: переносимый профиль, одобренный обмен, продолжение обучения и доказательство отката\t" + wrongDll,
                    StringComparison.Ordinal),
                "phase08_common_user_two_leg_lifecycle_wrong_dll:");

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "phase08_common_user_two_leg_lifecycle_wrong_batch:");
        });

    [Fact]
    public void Phase08CommonUserTwoLegLifecycleClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            var sources = Phase08CommonUserTwoLegLifecycleClassFqns()
                .Append("LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTestsLookalike")
                .Select(ClassSource);
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                string.Join('\n', sources));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "phase08-common-user-two-leg-lifecycle",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        Phase08CommonUserTwoLegLifecycleClassBoundaryFilter()),
                    Row("02", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "phase08-common-user-two-leg-lifecycle")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(Phase08CommonUserTwoLegLifecycleClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "phase08-common-user-two-leg-lifecycle" &&
                assignment.TestClass.FullyQualifiedName == "LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTestsLookalike");
        });

    [Fact]
    public void ActualRepositorySingletonFlowNodeIdentityBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = SingletonFlowNodeIdentityClassFqns();
        // Живая опись должна назначать в singleton-группу только тот класс,
        // который проверяет общие объекты потоков/узлов и повторную регистрацию.
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "singleton-flow-node-identity")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "singleton-flow-node-identity");
        // TSV обязан назвать тот же класс явным фильтром; точку снимаем только
        // для сравнения имени класса, саму точку проверяем ниже отдельно.
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(4, manifestRow.MinRows);
        // Защищённый каталог сохраняемых моделей получил собственный маршрут:
        // активный перечень классов без точного адреса теперь пуст.
        AssertActiveWeakSnapshotEmpty(repoRoot);
        // Завершающая точка отделяет имя нужного класса от похожих имён вида
        // SingletonIdentityTestsLookalike и удерживает запуск в границе класса.
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        // Соседний или похожий класс не должен попасть в короткую singleton-группу
        // только из-за совпадающего начала имени.
        Assert.DoesNotContain(
            "LlmFromScratch.Tests.SingletonIdentityTestsLookalike",
            actualClasses);
    }

    [Fact]
    public void ActualRepositorySingletonFlowNodeIdentityBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "singleton-flow-node-identity";
            const string renamedBatchId = "renamed-singleton-flow-node-identity";
            const string singletonTerm =
                "FullyQualifiedName~LlmFromScratch.Tests.SingletonIdentityTests.";
            const string extraTerm =
                "|FullyQualifiedName~LlmFromScratch.Tests.SingletonIdentityTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            // Переименование id имитирует потерю человеческой строки: класс не
            // должен тихо вернуться в остаточную корзину.
            AssertSingletonFlowNodeIdentityBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "singleton_flow_node_identity_wrong_batch:");
            // Сдвиг namespace убирает точное имя класса из фильтра и должен дать
            // отказ "нет нужного класса", а не зелёный неполный маршрут.
            AssertSingletonFlowNodeIdentityBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(singletonTerm, singletonTerm.Replace("Tests.", "Tests.Moved", StringComparison.Ordinal), StringComparison.Ordinal),
                "singleton_flow_node_identity_filter_missing_exact_class:");
            // Добавленный похожий термин проверяет, что соседние классы не
            // расширяют singleton-группу без отдельного человеческого решения.
            AssertSingletonFlowNodeIdentityBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(singletonTerm + "\t4\ttrue", singletonTerm + extraTerm + "\t4\ttrue", StringComparison.Ordinal),
                "singleton_flow_node_identity_filter_unexpected_term:");
            // Подмена DLL ловит перенос singleton-класса в чужую сборку: фильтр
            // должен остаться привязан к основной тестовой сборке.
            AssertSingletonFlowNodeIdentityBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tОбщие экземпляры потоков и узлов без повторной регистрации\t" + canonicalDll,
                    batchId + "\tОбщие экземпляры потоков и узлов без повторной регистрации\t" + wrongDll,
                    StringComparison.Ordinal),
                "singleton_flow_node_identity_wrong_dll:");

            // Удаление строки целиком проверяет самое опасное незаметное изменение: класс снова
            // оказался бы в wildcard-остатке и потерял бы отдельный короткий запуск.
            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertSingletonFlowNodeIdentityBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "singleton_flow_node_identity_wrong_batch:");
        });

    [Fact]
    public void SingletonFlowNodeIdentityClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            // Синтетический проект содержит настоящий singleton-класс и похожее
            // имя рядом, чтобы проверить именно границу класса, а не живой файл.
            var sources = SingletonFlowNodeIdentityClassFqns()
                .Append("LlmFromScratch.Tests.SingletonIdentityTestsLookalike")
                .Select(ClassSource);
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                string.Join('\n', sources));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            // Временная лестница ставит singleton-строку перед остатком: если
            // завершающая точка сломана, похожий класс попадёт в первую строку.
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "singleton-flow-node-identity",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        SingletonFlowNodeIdentityClassBoundaryFilter()),
                    Row("02", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            // Snapshot выключен осознанно: здесь проверяется только first-match
            // назначение по фильтру, а не состояние живого weak snapshot.
            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            // Берём только назначение singleton-группы: lookalike может уйти в
            // остаток, но не имеет права расширить короткую группу.
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "singleton-flow-node-identity")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(SingletonFlowNodeIdentityClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "singleton-flow-node-identity" &&
                assignment.TestClass.FullyQualifiedName == "LlmFromScratch.Tests.SingletonIdentityTestsLookalike");
        });

    [Fact]
    public void ActualRepositoryLogicalTopologyAndResultGapBatchHasExactTwoClasses()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = LogicalTopologyAndResultGapClassFqns();
        // Эта группа должна забрать только двадцать проверок логической
        // структуры: шестнадцать про структуру потоков и четыре про иерархию
        // исправления результата.
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "logical-topology-and-result-gap")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "logical-topology-and-result-gap");
        // TSV хранит полный адрес каждого класса с границей имени; без этого
        // похожие имена могли бы расширить короткий диагностический запуск.
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(20, manifestRow.MinRows);
        // После выделения защищённого каталога активный перечень классов без
        // точного маршрута пуст; это не размер всей остаточной группы.
        AssertActiveWeakSnapshotEmpty(repoRoot);
        // Завершающая точка удерживает фильтр запуска в границе полного имени
        // класса и не захватывает будущие похожие классы.
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain("LlmFromScratch.Tests.TopologyMutationFlowTestsLookalike", actualClasses);
        Assert.DoesNotContain("ResultGapLogicalHierarchyTestsLookalike", actualClasses);
    }

    [Fact]
    public void ActualRepositoryLogicalTopologyAndResultGapBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "logical-topology-and-result-gap";
            const string renamedBatchId = "renamed-logical-topology-and-result-gap";
            const string topologyTerm =
                "FullyQualifiedName~LlmFromScratch.Tests.TopologyMutationFlowTests.";
            const string resultGapTerm =
                "|FullyQualifiedName~ResultGapLogicalHierarchyTests.";
            const string extraTerm =
                "|FullyQualifiedName~LlmFromScratch.Tests.TopologyMutationFlowTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            // Переименование имени группы проверяет, что два класса не вернутся
            // молча в остаточную корзину под другим человеческим адресом.
            AssertLogicalTopologyAndResultGapBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "logical_topology_and_result_gap_wrong_batch:");
            // Удаление одного из двух терминов должно дать точную ошибку
            // неполного маршрута, а не зелёный запуск на половине смысла.
            AssertLogicalTopologyAndResultGapBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(resultGapTerm, string.Empty, StringComparison.Ordinal),
                "logical_topology_and_result_gap_filter_missing_exact_class:");
            // Лишний похожий термин проверяет, что маленькая группа не
            // превращается в скрытую wildcard-группу.
            AssertLogicalTopologyAndResultGapBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(topologyTerm + resultGapTerm + "\t20\ttrue", topologyTerm + resultGapTerm + extraTerm + "\t20\ttrue", StringComparison.Ordinal),
                "logical_topology_and_result_gap_filter_unexpected_term:");
            // DLL остаётся основной тестовой сборкой: перенос строки в чужую
            // сборку должен ломать проверку до запуска тестов.
            AssertLogicalTopologyAndResultGapBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tСтруктура потоков и иерархия исправления результата\t" + canonicalDll,
                    batchId + "\tСтруктура потоков и иерархия исправления результата\t" + wrongDll,
                    StringComparison.Ordinal),
                "logical_topology_and_result_gap_wrong_dll:");

            // Удаление строки целиком ловит самое опасное незаметное изменение: оба класса
            // снова получили бы wildcard-остаток вместо короткого сигнала.
            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertLogicalTopologyAndResultGapBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "logical_topology_and_result_gap_wrong_batch:");
        });

    [Fact]
    public void LogicalTopologyAndResultGapClassBoundaryRejectsSyntheticLookalikesWithoutAssigningThemToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            // Синтетический проект держит оба настоящих класса и оба похожих
            // имени рядом, чтобы проверить именно точку как границу полного
            // имени класса.
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                source: null);
            // Каждый синтетический класс пишется в отдельный файл: вспомогательная
            // функция объявляет пространство имён на весь файл, а два таких
            // объявления в одном файле недопустимы.
            WriteText(
                Path.Combine(repoRoot, "Tests", "TopologyMutationFlowTests.cs"),
                ClassSource("LlmFromScratch.Tests.TopologyMutationFlowTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "TopologyMutationFlowTestsLookalike.cs"),
                ClassSource("LlmFromScratch.Tests.TopologyMutationFlowTestsLookalike"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "ResultGapLogicalHierarchyTests.cs"),
                ClassSource("ResultGapLogicalHierarchyTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "ResultGapLogicalHierarchyTestsLookalike.cs"),
                ClassSource("ResultGapLogicalHierarchyTestsLookalike"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            // Временная лестница ставит новую строку перед остатком: если
            // точка сломана, похожий класс попадёт в эту группу.
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "logical-topology-and-result-gap",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        LogicalTopologyAndResultGapClassBoundaryFilter()),
                    Row("02", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            // Снимок здесь выключен намеренно: проверяется первое совпавшее
            // назначение по фильтру, а не состояние живого перечня слабых классов.
            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "logical-topology-and-result-gap")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(LogicalTopologyAndResultGapClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "logical-topology-and-result-gap" &&
                assignment.TestClass.FullyQualifiedName == "LlmFromScratch.Tests.TopologyMutationFlowTestsLookalike");
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "logical-topology-and-result-gap" &&
                assignment.TestClass.FullyQualifiedName == "ResultGapLogicalHierarchyTestsLookalike");
        });

    [Fact]
    public void ActualRepositoryResultGapTrainingPairWriterBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = ResultGapTrainingPairWriterClassFqns();
        // Эта группа забирает только запись обезличенных пар и исправлений:
        // она не запускает обучение модели и не доказывает качество обучения.
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "result-gap-training-pair-writer")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "result-gap-training-pair-writer");
        AssertPairGraphBatchPriority(report, manifestRow, expectedOrder: "12");
        // TSV хранит полный адрес класса с точкой на конце; точку снимаем только
        // для сравнения имени, сам фильтр проверяется отдельно ниже.
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(6, manifestRow.MinRows);
        // Активный перечень классов без собственного маршрута пуст после того,
        // как каталог сохраняемых моделей получил отдельную строку.
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain("ResultGapPhase07DpoWriterTestsLookalike", actualClasses);
    }

    [Fact]
    public void ActualRepositoryResultGapTrainingPairWriterBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "result-gap-training-pair-writer";
            const string renamedBatchId = "renamed-result-gap-training-pair-writer";
            const string writerTerm = "FullyQualifiedName~ResultGapPhase07DpoWriterTests.";
            const string extraTerm = "|FullyQualifiedName~ResultGapPhase07DpoWriterTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            // Потеря имени группы не должна тихо вернуть запись пар в остаток:
            // маршруту нужен собственный человеческий адрес.
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "result_gap_training_pair_writer_wrong_batch:");
            // Сдвиг терма проверяет, что маршрут не считается валидным без
            // точного класса записи обезличенных пар.
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(writerTerm, writerTerm.Replace("Tests", "TestsMoved", StringComparison.Ordinal), StringComparison.Ordinal),
                "result_gap_training_pair_writer_filter_missing_exact_class:");
            // Похожее имя рядом должно быть отдельным решением, а не скрытым
            // расширением короткой группы записи пар.
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(writerTerm + "\t6\ttrue", writerTerm + extraTerm + "\t6\ttrue", StringComparison.Ordinal),
                "result_gap_training_pair_writer_filter_unexpected_term:");
            // DLL закреплена за основной тестовой сборкой; перенос строки в
            // другую сборку ломает карту до запуска тестов.
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tЗапись обезличенных обучающих пар и исправлений\t" + canonicalDll,
                    batchId + "\tЗапись обезличенных обучающих пар и исправлений\t" + wrongDll,
                    StringComparison.Ordinal),
                "result_gap_training_pair_writer_wrong_dll:");
            // Лестница должна видеть точную строку до широкой группы
            // оркестрации: иначе фильтр запуска широкой строки успеет исключить
            // часть целевого запуска до точного имени класса.
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                MoveBatchLineAfterBatch(text, batchId, "orchestration-skills-wiki"),
                "result_gap_training_pair_writer_priority_after_broad_batch:");

            // Удаление строки целиком ловит возврат записи пар в общую остаточную группу:
            // этот класс уже получил отдельный маршрут.
            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertResultGapTrainingPairWriterBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "result_gap_training_pair_writer_wrong_batch:");
        });

    [Fact]
    public void ResultGapTrainingPairWriterClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                source: null);
            // Настоящий класс и похожее имя пишутся разными .cs файлами:
            // так проверяется именно граница имени класса.
            WriteText(
                Path.Combine(repoRoot, "Tests", "ResultGapPhase07DpoWriterTests.cs"),
                ClassSource("ResultGapPhase07DpoWriterTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "ResultGapPhase07DpoWriterTestsLookalike.cs"),
                ClassSource("ResultGapPhase07DpoWriterTestsLookalike"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            // Временная лестница ставит запись пар перед остатком: без точки на
            // конце похожее имя попало бы в эту группу.
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "result-gap-training-pair-writer",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        ResultGapTrainingPairWriterClassBoundaryFilter()),
                    Row("02", "orchestration-skills-wiki", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "FullyQualifiedName~Orchestration"),
                    Row("03", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "result-gap-training-pair-writer")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(ResultGapTrainingPairWriterClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "result-gap-training-pair-writer" &&
                assignment.TestClass.FullyQualifiedName == "ResultGapPhase07DpoWriterTestsLookalike");
        });

    [Fact]
    public void ActualRepositorySingleCombinationGraphRunnerBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = SingleCombinationGraphRunnerClassFqns();
        // Эта группа забирает только запуск одного варианта графа с отменой и
        // лимитом времени; запись пар и обучение модели остаются вне её смысла.
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "single-combination-graph-runner")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "single-combination-graph-runner");
        AssertPairGraphBatchPriority(report, manifestRow, expectedOrder: "13");
        // Сравниваем только имя класса без завершающей точки; наличие точки в
        // живом фильтре проверяется ниже отдельной проверкой.
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(18, manifestRow.MinRows);
        // Четырнадцать обычных тестов и два теста с двумя наборами входных
        // данных дают 18 выполнений; это не число методов в исходнике.
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain("SingleCombinationGraphCandidateRunnerTestsLookalike", actualClasses);
    }

    [Fact]
    public void ActualRepositorySingleCombinationGraphRunnerBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "single-combination-graph-runner";
            const string renamedBatchId = "renamed-single-combination-graph-runner";
            const string runnerTerm = "FullyQualifiedName~SingleCombinationGraphCandidateRunnerTests.";
            const string extraTerm = "|FullyQualifiedName~SingleCombinationGraphCandidateRunnerTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            // Переименование группы не должно сделать выполнение графа частью
            // общей остаточной группы или чужого человеческого маршрута.
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "single_combination_graph_runner_wrong_batch:");
            // Без точного класса выполнения графа группа не имеет смысла, даже если
            // остальные строки TSV валидны.
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(runnerTerm, runnerTerm.Replace("Tests", "TestsMoved", StringComparison.Ordinal), StringComparison.Ordinal),
                "single_combination_graph_runner_filter_missing_exact_class:");
            // Похожее имя не должно расширять группу одного варианта графа:
            // для него нужен отдельный маршрут или осознанное включение.
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(runnerTerm + "\t18\ttrue", runnerTerm + extraTerm + "\t18\ttrue", StringComparison.Ordinal),
                "single_combination_graph_runner_filter_unexpected_term:");
            // Строка принадлежит основной тестовой сборке, а не сборке
            // включения модели конверсии дизайна.
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tВыполнение одного варианта графа с отменой и ограничением времени\t" + canonicalDll,
                    batchId + "\tВыполнение одного варианта графа с отменой и ограничением времени\t" + wrongDll,
                    StringComparison.Ordinal),
                "single_combination_graph_runner_wrong_dll:");
            // Эта отрицательная копия повторяет найденный дефект: точная строка
            // стоит после широкой группы оркестрации, и первое совпадение уже
            // может отрезать метод с "Orchestration" в имени.
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                MoveBatchLineAfterBatch(text, batchId, "orchestration-skills-wiki"),
                "single_combination_graph_runner_priority_after_broad_batch:");

            // Удаление строки целиком доказывает, что выполнение графа не должно
            // тихо возвращаться в общую остаточную группу M-Z.
            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertSingleCombinationGraphRunnerBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "single_combination_graph_runner_wrong_batch:");
        });

    [Fact]
    public void SingleCombinationGraphRunnerClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                source: null);
            // Настоящий класс и похожее имя пишутся разными .cs файлами: так
            // синтетический проект повторяет реальную границу исходников.
            WriteText(
                Path.Combine(repoRoot, "Tests", "SingleCombinationGraphCandidateRunnerTests.cs"),
                ClassSource("SingleCombinationGraphCandidateRunnerTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "SingleCombinationGraphCandidateRunnerTestsLookalike.cs"),
                ClassSource("SingleCombinationGraphCandidateRunnerTestsLookalike"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            // Строка выполнения графа стоит перед остатком: проверяем, что точка
            // удерживает группу на одном классе.
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "single-combination-graph-runner",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        SingleCombinationGraphRunnerClassBoundaryFilter()),
                    Row("02", "orchestration-skills-wiki", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "FullyQualifiedName~Orchestration"),
                    Row("03", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "single-combination-graph-runner")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(SingleCombinationGraphRunnerClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "single-combination-graph-runner" &&
                assignment.TestClass.FullyQualifiedName == "SingleCombinationGraphCandidateRunnerTestsLookalike");
        });

    [Fact]
    public void ActualRepositoryRetainedDomainModelRegistryBatchHasExactOneClass()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = RetainedDomainModelRegistryClassFqns();
        // Каталог сохраняемых моделей проверяется отдельно от страховочного
        // остатка: переносимые строки запускаются, а Windows-only строки
        // остаются честным NotExecuted-долгом платформы.
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "retained-domain-model-registry")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "retained-domain-model-registry");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal("29", manifestRow.Order);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        Assert.Equal(42, manifestRow.MinRows);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain("RetainedDomainModelRegistryTestsLookalike", actualClasses);
    }

    [Fact]
    public void ActualRepositoryRetainedDomainModelRegistryBatchRejectsRenameMissingExtraAssemblyAndRemainderReturn()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "retained-domain-model-registry";
            const string renamedBatchId = "renamed-retained-domain-model-registry";
            const string registryTerm = "FullyQualifiedName~RetainedDomainModelRegistryTests.";
            const string extraTerm = "|FullyQualifiedName~RetainedDomainModelRegistryTestsLookalike.";
            const string canonicalDll = "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll";
            const string wrongDll = "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll";

            // Переименование группы не должно вернуть защищённый каталог в
            // страховочный остаток под другим адресом.
            AssertRetainedDomainModelRegistryBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "retained_domain_model_registry_wrong_batch:");
            // Без точного имени класса группа не подтверждает защищённый каталог
            // сохраняемых моделей.
            AssertRetainedDomainModelRegistryBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(registryTerm, registryTerm.Replace("Tests", "TestsMoved", StringComparison.Ordinal), StringComparison.Ordinal),
                "retained_domain_model_registry_filter_missing_exact_class:");
            // Похожее имя рядом должно получать отдельное решение, а не
            // расширять короткий маршрут защищённого каталога.
            AssertRetainedDomainModelRegistryBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(registryTerm + "\t42\ttrue", registryTerm + extraTerm + "\t42\ttrue", StringComparison.Ordinal),
                "retained_domain_model_registry_filter_unexpected_term:");
            // Основная тестовая сборка закрепляет реальные runtime-строки этой
            // группы; чужая DLL ломает карту до запуска тестов.
            AssertRetainedDomainModelRegistryBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    batchId + "\tЗащищённый каталог сохраняемых моделей\t" + canonicalDll,
                    batchId + "\tЗащищённый каталог сохраняемых моделей\t" + wrongDll,
                    StringComparison.Ordinal),
                "retained_domain_model_registry_wrong_dll:");

            // Удаление строки целиком ловит возврат последнего слабого класса в
            // wildcard-остаток: активный weak snapshot уже пуст.
            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertRetainedDomainModelRegistryBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "retained_domain_model_registry_wrong_batch:");
        });

    [Fact]
    public void RetainedDomainModelRegistryClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            CreateTestProject(
                repoRoot,
                "Tests",
                "LlmFromScratch.Tests",
                source: null);
            // Глобальный класс и похожее имя пишутся отдельными файлами: так
            // проверяется именно граница имени класса, а не namespace файла.
            WriteText(
                Path.Combine(repoRoot, "Tests", "RetainedDomainModelRegistryTests.cs"),
                ClassSource("RetainedDomainModelRegistryTests"));
            WriteText(
                Path.Combine(repoRoot, "Tests", "RetainedDomainModelRegistryTestsLookalike.cs"),
                ClassSource("RetainedDomainModelRegistryTestsLookalike"));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            // Строка защищённого каталога стоит перед остатком: точка на конце
            // удерживает маршрут на одном реальном классе.
            WriteText(
                batchesPath,
                Manifest(
                    Row(
                        "01",
                        "retained-domain-model-registry",
                        "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll",
                        RetainedDomainModelRegistryClassBoundaryFilter()),
                    Row("02", "remainder", "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll", "*")));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "retained-domain-model-registry")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            AssertNoProblems(report);
            Assert.Equal(RetainedDomainModelRegistryClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "retained-domain-model-registry" &&
                assignment.TestClass.FullyQualifiedName == "RetainedDomainModelRegistryTestsLookalike");
        });

    [Fact]
    public void ActualRepositoryModelActivationBatchHasExactSixClasses()
    {
        var actualRepository = ActualRepositoryInventory.Value;
        var repoRoot = actualRepository.RepoRoot;
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, actualRepository.Inventory);
        var expectedClasses = ModelActivationClassFqns();
        var actualClasses = report.Assignments
            .Where(assignment => assignment.BatchId == "model-activation")
            .Select(assignment => assignment.TestClass.FullyQualifiedName)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var manifestRow = Assert.Single(report.Rows, row => row.BatchId == "model-activation");
        var declaredTerms = manifestRow.Filter
            .Split('|')
            .Select(term => term["FullyQualifiedName~".Length..].TrimEnd('.'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        AssertNoProblems(report);
        Assert.Equal(expectedClasses, actualClasses);
        Assert.Equal(expectedClasses, declaredTerms);
        AssertActiveWeakSnapshotEmpty(repoRoot);
        Assert.All(manifestRow.Filter.Split('|'), term => Assert.EndsWith(".", term, StringComparison.Ordinal));
        Assert.DoesNotContain("DesignConversionHumanApprovalTestsLookalike", actualClasses);
    }

    [Fact]
    public void ActualRepositoryModelActivationBatchRejectsRenameMissingExtraAndAssemblyRowRemoval()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var actualRepository = ActualRepositoryInventory.Value;
            var repoRoot = actualRepository.RepoRoot;
            var canonicalBatchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            var text = File.ReadAllText(canonicalBatchesPath, Utf8NoBom);
            const string batchId = "model-activation";
            const string renamedBatchId = "renamed-model-activation";
            const string humanApprovalTerm = "|FullyQualifiedName~DesignConversionHumanApprovalTests.";
            const string extraTerm = "|FullyQualifiedName~DesignConversionHumanApprovalTestsLookalike.";

            AssertModelActivationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(batchId, renamedBatchId, StringComparison.Ordinal),
                "model_activation_wrong_batch:");
            AssertModelActivationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(humanApprovalTerm, string.Empty, StringComparison.Ordinal),
                "model_activation_filter_missing_exact_class:");
            AssertModelActivationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                text.Replace(
                    "FullyQualifiedName~DesignConversionModelActivationRouteRuntimeTests.\t6\ttrue",
                    "FullyQualifiedName~DesignConversionModelActivationRouteRuntimeTests." + extraTerm + "\t6\ttrue",
                    StringComparison.Ordinal),
                "model_activation_filter_unexpected_term:");

            var withoutNamedBatch = string.Join(
                '\n',
                text.Split('\n').Where(line => !line.Contains(batchId, StringComparison.Ordinal)));
            AssertModelActivationBatchProblem(
                repoRoot,
                actualRepository.Inventory,
                owner.RootPath,
                withoutNamedBatch + "\n",
                "batch_manifest_missing_test_assembly:Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll");
        });

    [Fact]
    public void ModelActivationClassBoundaryRejectsSyntheticLookalikeWithoutAssigningItToBatch()
        => TestOwnedOutputRoot.Run(owner =>
        {
            var repoRoot = owner.RootPath;
            var sources = ModelActivationClassFqns()
                .Append("DesignConversionHumanApprovalTestsLookalike")
                .Select(ClassSource);
            CreateTestProject(
                repoRoot,
                Path.Combine("Tests", "DesignConversion.ModelActivation.Tests"),
                "DesignConversion.ModelActivation.Tests",
                string.Join('\n', sources));
            var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
            WriteText(
                batchesPath,
                Manifest(Row(
                    "01",
                    "model-activation",
                    "Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll",
                    ModelActivationClassBoundaryFilter())));

            var report = TestSuiteBatchManifest.ValidateRepository(
                repoRoot,
                batchesPath,
                snapshotPath: null,
                options: TestSuiteBatchGuardOptions.Synthetic);
            var actualClasses = report.Assignments
                .Where(assignment => assignment.BatchId == "model-activation")
                .Select(assignment => assignment.TestClass.FullyQualifiedName)
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(ModelActivationClassFqns(), actualClasses);
            Assert.DoesNotContain(report.Assignments, assignment =>
                assignment.BatchId == "model-activation" &&
                assignment.TestClass.FullyQualifiedName == "DesignConversionHumanApprovalTestsLookalike");
            Assert.Contains(report.Problems, problem =>
                problem.StartsWith(
                    "batch_manifest_unassigned_class:Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll|DesignConversionHumanApprovalTestsLookalike",
                    StringComparison.Ordinal));
        });

    private static void AssertSnapshotProblem(
        string repoRoot,
        string batchesPath,
        string snapshotPath,
        string snapshotText,
        string expectedPrefix)
    {
        WriteText(snapshotPath, snapshotText);
        var report = TestSuiteBatchManifest.ValidateRepository(
            repoRoot,
            batchesPath,
            snapshotPath,
            options: TestSuiteBatchGuardOptions.Synthetic with { RequireSnapshot = true });

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertOrchestrationBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Мутируем только копию TSV: живой файл остается доказательством
        // хорошего маршрута, а плохой сценарий обязан падать рядом.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertDesignConversionLiveLocalServerMethodDiagnosticBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Проверяем только временную копию TSV: живая строка остаётся хорошим
        // контрактом, а каждая порча доказывает отдельный отказ до запуска сервера.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertDesignConversionLiveLocalServerMethodDiagnosticOverrideProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string overrideText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");
        var mutatedOverridePath = Path.Combine(tempRoot, Guid.NewGuid() + "-leaf-mode-overrides.tsv");

        // Override обязателен именно для нового медленного класса: без режима
        // "один метод" длинная порция снова может потерять соседние результаты.
        WriteText(mutatedBatchesPath, batchesText);
        WriteText(mutatedOverridePath, overrideText);
        var report = TestSuiteBatchManifest.ValidateRepository(
            repoRoot,
            inventory,
            mutatedBatchesPath,
            leafModeOverridesPath: mutatedOverridePath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertDesignConversionDpoStorageOverrideProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string overrideText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");
        var mutatedOverridePath = Path.Combine(tempRoot, Guid.NewGuid() + "-leaf-mode-overrides.tsv");

        // Разбиение хранилища по методам — точечное исключение для одного класса
        // внутри существующей группы; временный файл настроек доказывает, что
        // чужая группа не может получить этот режим простой правкой TSV.
        WriteText(mutatedBatchesPath, batchesText);
        WriteText(mutatedOverridePath, overrideText);
        var report = TestSuiteBatchManifest.ValidateRepository(
            repoRoot,
            inventory,
            mutatedBatchesPath,
            leafModeOverridesPath: mutatedOverridePath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertTestInfrastructureEvidenceMeasurementBatchProblem(
        string repoRoot,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Плохой сценарий живёт только во временной копии TSV: текущая
        // лестница остаётся эталоном, а guard доказывает чувствительность.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertTestInfrastructureEvidenceMeasurementOverrideProblem(
        string repoRoot,
        string tempRoot,
        string batchesText,
        string overrideText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");
        var mutatedOverridePath = Path.Combine(tempRoot, Guid.NewGuid() + "-leaf-mode-overrides.tsv");

        // Override — такой же живой вход, как batches.tsv: плохой режим обязан
        // падать до runtime, иначе старый pack-результат можно перепутать с
        // методовым доказательством.
        WriteText(mutatedBatchesPath, batchesText);
        WriteText(mutatedOverridePath, overrideText);
        var report = TestSuiteBatchManifest.ValidateRepository(
            repoRoot,
            mutatedBatchesPath,
            leafModeOverridesPath: mutatedOverridePath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertServerOwnedReplayManifestAttestationBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Мутации replay/passport batch живут во временном TSV: живая строка
        // остается хорошим контрактом, а плохой маршрут обязан упасть до runtime.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertPhase08CommonUserTwoLegLifecycleBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Мутации Phase 08 живут во временном TSV: checked-in маршрут
        // остаётся хорошим контрактом, а плохой доказывает чувствительность.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertSingletonFlowNodeIdentityBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Мутации singleton-группы живут отдельно от canonical TSV: живая
        // лестница остаётся хорошей, а каждый плохой вариант должен дать свой
        // явный отказ до запуска runtime. Проверка prefix-а привязывает отказ
        // к нужному guard-условию, чтобы проблема не потерялась как общий шум.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertLogicalTopologyAndResultGapBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Каждая порча новой группы получает свой временный TSV: общую опись
        // классов можно кэшировать, а изменяемый список групп нужно проверять
        // заново, чтобы переименование, удаление, лишний класс, неверная DLL и
        // удаление строки не маскировали друг друга.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertResultGapTrainingPairWriterBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Каждая порча группы записи пар получает свой временный TSV: опись классов
        // можно переиспользовать, а результат проверки изменённой лестницы —
        // нельзя кэшировать между сценариями.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertSingleCombinationGraphRunnerBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Каждая мутация строки выполнения графа проверяется отдельно: так
        // переименование, потеря класса, лишний терм, неверная DLL и возврат в
        // остаток не маскируют друг друга.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertPairGraphBatchPriority(
        TestSuiteBatchGuardReport report,
        TestSuiteBatchRow exactRow,
        string expectedOrder)
    {
        Assert.Equal(expectedOrder, exactRow.Order);
        foreach (var broadBatchId in PairGraphBroadCollisionBatchIds())
        {
            var broadRow = Assert.Single(report.Rows, row => row.BatchId == broadBatchId);
            Assert.True(
                exactRow.LineNumber < broadRow.LineNumber,
                $"{exactRow.BatchId} должен стоять до {broadBatchId}: точная строка {exactRow.LineNumber}, широкая строка {broadRow.LineNumber}");
        }
    }

    private static string MoveBatchLineAfterBatch(string text, string movingBatchId, string anchorBatchId)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
        var keepTrailingNewLine = normalized.EndsWith('\n');
        var body = keepTrailingNewLine ? normalized.TrimEnd('\n') : normalized;
        var lines = body.Split('\n').ToList();
        var movingIndex = FindBatchLineIndex(lines, movingBatchId);
        var movingLine = lines[movingIndex];
        lines.RemoveAt(movingIndex);
        var anchorIndex = FindBatchLineIndex(lines, anchorBatchId);

        // Переносим только строку во временной копии TSV: номера оставляем как
        // есть, потому что проверяем именно порядок первого совпадения в файле.
        lines.Insert(anchorIndex + 1, movingLine);
        return string.Join('\n', lines) + (keepTrailingNewLine ? "\n" : string.Empty);
    }

    private static int FindBatchLineIndex(IReadOnlyList<string> lines, string batchId)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var parts = lines[index].Split('\t');
            if (parts.Length > 1 && string.Equals(parts[1], batchId, StringComparison.Ordinal))
                return index;
        }

        throw new InvalidOperationException("Batch line was not found: " + batchId);
    }

    private static void AssertModelActivationBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Мутации описи активации модели живут во временном TSV: текущий
        // canonical-файл остается эталоном, а плохой маршрут обязан упасть.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertLogicalLadderMetadataProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-logical-ladder-batches.tsv");

        // Мутации логической разметки проходят через тот же живой валидатор,
        // что и канонический `batches.tsv`: тест меняет только временную копию
        // и требует жёсткую ошибку проверки, не полагаясь на человеческий
        // комментарий в файле.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertBatchAssignment(
        TestSuiteBatchGuardReport report,
        string dll,
        string classFqn,
        string expectedBatchId)
    {
        var assignment = Assert.Single(
            report.Assignments,
            candidate =>
                string.Equals(candidate.TestClass.Dll, dll, StringComparison.Ordinal) &&
                string.Equals(candidate.TestClass.FullyQualifiedName, classFqn, StringComparison.Ordinal));

        Assert.Equal(expectedBatchId, assignment.BatchId);
    }

    private static void AssertRetainedDomainModelRegistryBatchProblem(
        string repoRoot,
        TestSuiteBatchRepositoryInventory inventory,
        string tempRoot,
        string batchesText,
        string expectedPrefix)
    {
        var mutatedBatchesPath = Path.Combine(tempRoot, Guid.NewGuid() + "-batches.tsv");

        // Каждая мутация защищённого каталога получает свой временный TSV:
        // так переименование, потеря класса, лишний терм, неверная DLL и
        // возврат в остаток проверяются независимо.
        WriteText(mutatedBatchesPath, batchesText);
        var report = TestSuiteBatchManifest.ValidateRepository(repoRoot, inventory, mutatedBatchesPath);

        Assert.Contains(report.Problems, problem => problem.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private static void AssertActiveWeakSnapshotEmpty(string repoRoot)
    {
        // Когда последний старый runtime-класс получил точный человеческий
        // маршрут, активный перечень слабых классов должен стать пустым.
        Assert.Empty(TestSuiteBatchManifest.ReadLegacySnapshot(
            Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacySnapshot.txt")).Rows);
    }

    private static ActualRepositoryInventoryContext BuildActualRepositoryInventory()
    {
        var repoRoot = TestSuiteBatchManifest.FindRepositoryRoot();

        // Живая repo-inventory дорогая: helper поднимает все test csproj и
        // строит Roslyn compilation по исходникам. Кэшируем только
        // неизменяемые проекты/классы; каждая мутация получает свой временный
        // TSV и заново проходит manifest validation, поэтому сценарии не делят
        // mutable state и не загрязняют друг друга.
        var inventory = TestSuiteBatchManifest.CreateRepositoryInventory(repoRoot);

        return new ActualRepositoryInventoryContext(repoRoot, inventory);
    }

    private static string[] OrchestrationRoutingProductBoundaryClassFqns()
        =>
        [
            "LlmFromScratch.Tests.MultiSubPhaseRouterLearnedAuthorityTests",
            "LlmFromScratch.Tests.OrchestratorModelTests",
            "LlmFromScratch.Tests.PluginBoundaryTests",
            "LlmFromScratch.Tests.PromptRoutingInputCompilerUnitTests",
            "LlmFromScratch.Tests.ReflectionWiringTests",
            "LlmFromScratch.Tests.SolutionProductProjectRegistrationTests",
            "ThinClientApiContractTests"
        ];

    private static string[] DesignConversionLiveLocalServerMethodDiagnosticClassFqns()
        =>
        [
            "DesignConversionLiveLocalServerTests"
        ];

    private static string DesignConversionLiveLocalServerMethodDiagnosticClassBoundaryFilter()
        => string.Join('|', DesignConversionLiveLocalServerMethodDiagnosticClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] TestInfrastructureEvidenceMeasurementClassFqns()
        =>
        [
            "LlmFromScratch.Tests.CrossStackFlowSeedHonestyTests",
            "LlmFromScratch.Tests.MeasurementNoisePolicyTests",
            "LlmFromScratch.Tests.TestOwnedOutputRootTests",
            "LlmFromScratch.Tests.TestSuiteBatchInventoryGuardTests",
            "LlmFromScratch.Tests.WorkLeftRagEvidenceIndexTests"
        ];

    private static string[] ServerOwnedReplayManifestAttestationClassFqns()
        =>
        [
            "LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests"
        ];

    private static string ServerOwnedReplayManifestAttestationClassBoundaryFilter()
        => string.Join('|', ServerOwnedReplayManifestAttestationClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] Phase08CommonUserTwoLegLifecycleClassFqns()
        =>
        [
            "LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests"
        ];

    private static string Phase08CommonUserTwoLegLifecycleClassBoundaryFilter()
        => string.Join('|', Phase08CommonUserTwoLegLifecycleClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    // Список намеренно состоит из одного класса: соседние тесты из того же файла
    // должны оставаться в своих маршрутах или в остатке до отдельного решения.
    private static string[] SingletonFlowNodeIdentityClassFqns()
        =>
        [
            "LlmFromScratch.Tests.SingletonIdentityTests"
        ];

    // Фильтр собирается с точкой после имени класса: VSTest видит методы как
    // Class.Method, а похожий SingletonIdentityTestsLookalike не совпадает.
    private static string SingletonFlowNodeIdentityClassBoundaryFilter()
        => string.Join('|', SingletonFlowNodeIdentityClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    // Два класса перечислены в том же порядке, что и полученный массив после
    // сортировки: так проверка сравнивает состав, а не случайный порядок.
    private static string[] LogicalTopologyAndResultGapClassFqns()
        =>
        [
            "LlmFromScratch.Tests.TopologyMutationFlowTests",
            "ResultGapLogicalHierarchyTests"
        ];

    private static string LogicalTopologyAndResultGapClassBoundaryFilter()
        => string.Join('|', LogicalTopologyAndResultGapClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] ResultGapTrainingPairWriterClassFqns()
        =>
        [
            "ResultGapPhase07DpoWriterTests"
        ];

    private static string ResultGapTrainingPairWriterClassBoundaryFilter()
        => string.Join('|', ResultGapTrainingPairWriterClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] SingleCombinationGraphRunnerClassFqns()
        =>
        [
            "SingleCombinationGraphCandidateRunnerTests"
        ];

    private static string SingleCombinationGraphRunnerClassBoundaryFilter()
        => string.Join('|', SingleCombinationGraphRunnerClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] RetainedDomainModelRegistryClassFqns()
        =>
        [
            "RetainedDomainModelRegistryTests"
        ];

    private static string RetainedDomainModelRegistryClassBoundaryFilter()
        => string.Join('|', RetainedDomainModelRegistryClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static string[] PairGraphBroadCollisionBatchIds()
        =>
        [
            "dynamic-graph",
            "teaching-translation-dpo",
            "orchestration-skills-wiki"
        ];

    private static string[] ModelActivationClassFqns()
        =>
        [
            "DesignConversionActivatedModelRepositoryHostRuntimeTests",
            "DesignConversionExternalVisualAiReceiptTrustTests",
            "DesignConversionHumanApprovalTests",
            "DesignConversionImprovementLoopSemanticsTests",
            "DesignConversionModelActivationRegistryTests",
            "DesignConversionModelActivationRouteRuntimeTests"
        ];

    private static string ModelActivationClassBoundaryFilter()
        => string.Join('|', ModelActivationClassFqns().Select(classFqn => "FullyQualifiedName~" + classFqn + "."));

    private static WeakSnapshotFixture CreateWeakSnapshotFixture(
        string repoRoot,
        string source,
        params string[] rows)
    {
        CreateTestProject(repoRoot, "Tests", "Synthetic.Tests", source);
        var batchesPath = Path.Combine(repoRoot, "tools", "test-batches", "batches.tsv");
        var activeSnapshotPath = Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacySnapshot.txt");
        var weakBaselinePath = Path.Combine(repoRoot, "Tests", "TestSuiteBatchLegacyWeakBaseline.txt");
        WriteText(batchesPath, Manifest(rows));
        var report = TestSuiteBatchManifest.ValidateRepository(
            repoRoot,
            batchesPath,
            snapshotPath: null,
            options: TestSuiteBatchGuardOptions.Synthetic);

        return new WeakSnapshotFixture(batchesPath, activeSnapshotPath, weakBaselinePath, report);
    }

    private static TestSuiteBatchGuardReport ValidateWeakFixture(
        WeakSnapshotFixture fixture,
        string? requiredLegacyWeakBaselineBodySha256 = null)
        => TestSuiteBatchManifest.ValidateRepository(
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fixture.BatchesPath)!, "..", "..")),
            fixture.BatchesPath,
            fixture.ActiveSnapshotPath,
            options: TestSuiteBatchGuardOptions.Synthetic with
            {
                RequireSnapshot = true,
                RequireWeakBaseline = true,
                RequiredLegacyWeakBaselineBodySha256 = requiredLegacyWeakBaselineBodySha256
            },
            legacyWeakBaselinePath: fixture.WeakBaselinePath);

    private static string Manifest(params string[] rows)
        => TestSuiteBatchManifest.BatchHeader + "\n" + string.Concat(rows);

    private static string Row(string order, string batchId, string dll, string filter)
        => $"{order}\t{batchId}\tSynthetic human name\t{dll}\t{filter}\t1\ttrue\tany\tSynthetic platform reason.\n";

    private static string ClassSource(string classFqn)
    {
        var lastDot = classFqn.LastIndexOf('.');
        var namespaceName = lastDot < 0 ? string.Empty : classFqn[..lastDot];
        var className = lastDot < 0 ? classFqn : classFqn[(lastDot + 1)..];
        var namespaceLine = string.IsNullOrWhiteSpace(namespaceName)
            ? string.Empty
            : $"namespace {namespaceName};\n";

        return namespaceLine +
            $"public sealed class {className}\n" +
            "{\n" +
            "    [Fact]\n" +
            "    public void Works()\n" +
            "    {\n" +
            "        Assert.True(true);\n" +
            "    }\n" +
            "}\n";
    }

    private static void AssertNoProblems(TestSuiteBatchGuardReport report)
        => Assert.True(
            report.Problems.Count == 0,
            "Guard problems:\n" + string.Join('\n', report.Problems));

    private static string SnapshotHeaderValue(string snapshotText, string key)
    {
        var prefix = key + "\t";
        foreach (var line in snapshotText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (line.StartsWith(prefix, StringComparison.Ordinal))
                return line[prefix.Length..];
        }

        throw new InvalidOperationException("Snapshot header key was not found: " + key);
    }

    private static void CreateTestProject(
        string repoRoot,
        string relativeDirectory,
        string assemblyName,
        string? source,
        IReadOnlyList<string>? linkedSources = null)
    {
        var projectDirectory = Path.Combine(repoRoot, relativeDirectory);
        Directory.CreateDirectory(projectDirectory);
        if (source is not null)
            WriteText(Path.Combine(projectDirectory, assemblyName.Replace('.', '_') + ".cs"), source);

        var linkedCompileItems = string.Concat((linkedSources ?? Array.Empty<string>())
            .Select(path => $"    <Compile Include=\"{path}\" Link=\"{Path.GetFileName(path)}\" />\n"));
        var projectText =
            "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
            "  <PropertyGroup>\n" +
            "    <TargetFramework>net9.0</TargetFramework>\n" +
            $"    <AssemblyName>{assemblyName}</AssemblyName>\n" +
            "  </PropertyGroup>\n" +
            "  <ItemGroup>\n" +
            "    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.12.0\" />\n" +
            "    <PackageReference Include=\"xunit\" Version=\"2.9.2\" />\n" +
            "  </ItemGroup>\n" +
            (linkedCompileItems.Length == 0
                ? string.Empty
                : "  <ItemGroup>\n" + linkedCompileItems + "  </ItemGroup>\n") +
            "</Project>\n";

        WriteText(Path.Combine(projectDirectory, assemblyName + ".csproj"), projectText);
    }

    private static void WriteText(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text, Utf8NoBom);
    }

    private sealed record WeakSnapshotFixture(
        string BatchesPath,
        string ActiveSnapshotPath,
        string WeakBaselinePath,
        TestSuiteBatchGuardReport Report);

    private sealed record ActualRepositoryInventoryContext(
        string RepoRoot,
        TestSuiteBatchRepositoryInventory Inventory);
}
