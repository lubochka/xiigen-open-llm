// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure.Logical;
using Tokenizer;
using VectorSearch;

return await OrderProbe.Run(args);

internal static class OrderProbe
{
    private const string FixtureSha = "C1576AFB7D915252E99D3C6C01750AAF19936E3761CF8F75E6D24FCC4DF431A7";
    private const string Domain = "micro-order";
    private const string Role = "micro-order-classifier";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private static readonly Type Owner = typeof(KnnLogicalUnit<string, string>);
    private static readonly MethodInfo CoreVectorize = Owner.GetMethod("Vectorize", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static int trainCalls, executeCalls, exactSearchCalls, vectorizeCalls;

    public static async Task<int> Run(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: Rp01MechanismProbe <core-root> <fixture-json> <new-output-directory>");
            return 2;
        }
        string root = Path.GetFullPath(args[0]), fixturePath = Path.GetFullPath(args[1]), output = Path.GetFullPath(args[2]);
        if (File.Exists(Path.Combine(output, "intent.json")))
        {
            Console.Error.WriteLine("Refusing to overwrite an existing experiment intent.");
            return 2;
        }
        Directory.CreateDirectory(output);
        var clock = Stopwatch.StartNew();
        var started = DateTimeOffset.UtcNow;
        try
        {
            byte[] fixtureBytes = File.ReadAllBytes(fixturePath);
            if (Sha(fixtureBytes) != FixtureSha) throw new InvalidDataException("Fixture hash differs from predeclared version.");
            using var fixture = JsonDocument.Parse(fixtureBytes);
            // Held-out labels are not put into recipes, feature extraction, or training.
            var train = fixture.RootElement.GetProperty("train").Deserialize<TrainRow[]>(Json)!;
            var heldout = fixture.RootElement.GetProperty("heldout").EnumerateArray()
                .Select(x => new QueryRow(x.GetProperty("id").GetString()!, x.GetProperty("group").GetString()!, x.GetProperty("input").GetString()!)).ToArray();
            ValidateInputs(train, heldout);
            var recipe = new Recipe("micro-order-predicates-v1", train, heldout, 64, 1, .70, false,
                ["FIRST_ACTION_BACKUP", "FIRST_ACTION_DELETE"]);
            WriteNew(Path.Combine(output, "recipe.json"), recipe);
            string recipeSha = FileSha(Path.Combine(output, "recipe.json"));
            var sources = SourceHashes(root);
            WriteNew(Path.Combine(output, "intent.json"), new
            {
                experiment = "MICRO-KNN-ORDER-01", status = "FROZEN_BEFORE_TRAINING", startedUtc = started,
                sourceHead = GitHead(root), fixturePath, fixtureSha256 = FixtureSha, recipeSha256 = recipeSha,
                sourceFiles = sources,
                assemblies = new[] { Owner.Assembly, typeof(BpeTokenizer).Assembly, typeof(HnswIndex).Assembly, typeof(LogicalUnitRequest<string>).Assembly }
                    .Distinct().Select(a => new { path = a.Location, sha256 = FileSha(a.Location), moduleId = a.ManifestModule.ModuleVersionId }),
                consumer = "actual KnnLogicalUnit<string,string>.Train -> ExecuteAsync -> actual HnswIndex.Search -> stored Output",
                intervention = "A empty predicates; B FIRST_ACTION_BACKUP/DELETE from input-only parser; identity textSelector both",
                inputProvenance = "simulation diagnostic, not DPO", independentTaskFamilies = false,
                repeats = 3, seed = (int?)null, seedReason = "Existing HNSW static unseeded RNG; ANN effect exploratory",
                thresholds = new { eachBAtLeastCorrect = 12, eachBGainAtLeast = 4, noIncreaseFalseSafe = true,
                    labelFlipAtLeastInverted = 12, baselinePairVectorsBitEqual = true, enhancedPairVectorsDifferent = true },
                limits = new { trainCalls = 8, executeCalls = 128, exactSearchCalls = 112, teacherCalls = 0, studentCalls = 0, dpoUpdates = 0 },
                preflightStops = new[] { "INVALID_PREMISE", "FEATURE_NOT_ENCODED", "INVALID_COMPARISON" },
                reflectionScope = "Read private tokenizer/index and invoke the actual private Vectorize; no mutation or duplicate vector formula",
                replayMeaning = "Rebuild from saved recipe; NOT cold binary restore",
                commandArguments = args, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
            });

            Console.WriteLine("Intent frozen. Building first actual core A/B units.");
            var a = Build(recipe, false, false, "repeat-1-A", output);
            var b = Build(recipe, true, false, "repeat-1-B", output);
            var preflight = Preflight(a, b, recipe);
            WriteNew(Path.Combine(output, "vector-preflight.json"), preflight);
            string? stop = !preflight.SameTokenizerAndTokenLayer ? "INVALID_COMPARISON"
                : preflight.Pairs.Any(p => !p.BaselineEqual) ? "INVALID_PREMISE"
                : preflight.Pairs.Any(p => !p.EnhancedDifferent) ? "FEATURE_NOT_ENCODED" : null;
            if (stop is not null)
            {
                WriteNew(Path.Combine(output, "comparison.json"), new { status = stop, calls = Calls(), preflight,
                    elapsedMs = clock.Elapsed.TotalMilliseconds, expectedExitCode = 0, qualityEvaluated = false });
                VerifyUnchanged(root, sources, fixturePath, output);
                Console.WriteLine($"{stop}; no held-out ExecuteAsync calls; no post-hoc change.");
                return 0;
            }

            var gold = fixture.RootElement.GetProperty("heldout").EnumerateArray()
                .ToDictionary(x => x.GetProperty("id").GetString()!, x => x.GetProperty("label").GetString()!, StringComparer.Ordinal);
            if (gold.Values.Any(x => x is not ("SAFE" or "UNSAFE"))) throw new InvalidDataException("Unexpected held-out label.");
            var pairs = new List<PairResult>();
            EvalResult? firstB = null;
            for (int repeat = 1; repeat <= 3; repeat++)
            {
                if (repeat > 1)
                {
                    a = Build(recipe, false, false, $"repeat-{repeat}-A", output);
                    b = Build(recipe, true, false, $"repeat-{repeat}-B", output);
                    if (!Preflight(a, b, recipe).AllValid) throw new InvalidDataException("Preflight changed during rebuild.");
                }
                var evalA = await Evaluate(a, recipe, gold, output, exact: true);
                var evalB = await Evaluate(b, recipe, gold, output, exact: true);
                firstB ??= evalB;
                pairs.Add(new PairResult(repeat, evalA, evalB,
                    evalA.Cases.Zip(evalB.Cases).Count(x => !x.First.Correct && x.Second.Correct),
                    evalA.Cases.Zip(evalB.Cases).Count(x => x.First.Correct && !x.Second.Correct)));
                Console.WriteLine($"Repeat {repeat}: A={evalA.Correct}/16 B={evalB.Correct}/16; false SAFE {evalA.FalseSafe}->{evalB.FalseSafe}");
            }

            var flipped = Build(recipe, true, true, "label-flip-B", output);
            var flipEval = await Evaluate(flipped, recipe, gold, output, exact: true);
            int inverted = firstB!.Cases.Zip(flipEval.Cases).Count(x => x.Second.Output == Flip(x.First.Output));

            // Actual disk recipe readback: no held-out labels or original in-memory train array enter replay.
            var reloaded = JsonSerializer.Deserialize<Recipe>(File.ReadAllBytes(Path.Combine(output, "recipe.json")), Json)!;
            if (FileSha(Path.Combine(output, "recipe.json")) != recipeSha) throw new InvalidDataException("Recipe changed.");
            var replay = Build(reloaded, true, false, "recipe-replay-B", output);
            var replayEval = await Evaluate(replay, reloaded, gold, output, exact: false);
            int sameReplayOutput = firstB.Cases.Zip(replayEval.Cases).Count(x => x.First.Output == x.Second.Output);
            bool replayStateEqual = replay.TokenizerSha == firstB.TokenizerSha && replay.VectorSha == firstB.VectorSha;
            bool labelDependent = inverted >= 12;
            bool allGain = pairs.All(p => p.B.Correct >= 12 && p.B.Correct - p.A.Correct >= 4 && p.B.FalseSafe <= p.A.FalseSafe);
            string status = allGain && labelDependent && replayStateEqual && sameReplayOutput == 16 ? "LOCAL_MEMORY_QUALITY_GAIN"
                : !labelDependent || !replayStateEqual ? "CONTROL_FAILURE"
                : pairs.All(p => p.B.Correct < p.A.Correct) ? "LOCAL_REGRESSION"
                : pairs.All(p => p.B.Correct == p.A.Correct) ? "REPRESENTATION_ONLY" : "INCONCLUSIVE";
            VerifyUnchanged(root, sources, fixturePath, output);
            WriteNew(Path.Combine(output, "comparison.json"), new
            {
                status, causalScope = "One action-order law, new asset names, learned memory; unseeded ANN effect exploratory",
                preflight, repeats = pairs, labelFlip = new { inverted, required = 12, labelDependent, evaluation = flipEval },
                recipeReplay = new { sameReplayOutput, expected = 16, replayStateEqual, evaluation = replayEval,
                    coldBinaryRestore = false, indexGraphIdentityRequired = false },
                calls = Calls(), elapsedMs = clock.Elapsed.TotalMilliseconds, processPeakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64,
                completedUtc = DateTimeOffset.UtcNow, expectedExitCode = 0,
                notProven = new[] { "DPO or LoRA improvement", "ENRU or C# generation", "RP01 consumer integration", "new law generalization", "cold binary restore", "matched-seed ANN causal effect" }
            });
            Console.WriteLine($"{status}; label-flip inverted {inverted}/16; replay outputs {sameReplayOutput}/16; Train={trainCalls}, ExecuteAsync={executeCalls}.");
            return 0;
        }
        catch (Exception error)
        {
            WriteNew(Path.Combine(output, "failure.json"), new { status = "TECHNICAL_FAILURE", error = error.ToString(), calls = Calls(), elapsedMs = clock.Elapsed.TotalMilliseconds, expectedExitCode = 1 });
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static Built Build(Recipe recipe, bool enhanced, bool flip, string name, string output)
    {
        if (++trainCalls > 8) throw new InvalidOperationException("Train budget exceeded.");
        var examples = recipe.Train.Select(r => new LogicalKnnTrainingExample<string, string>(r.Id, r.Input,
            flip ? Flip(r.Label) : r.Label, enhanced ? Predicates(r.Input) : [], [Domain], [])).ToArray();
        var timer = Stopwatch.StartNew();
        var result = KnnLogicalUnit<string, string>.Train("micro-order-unit", Role, Domain, examples, input => input,
                vectorDimensions: recipe.VectorDimensions, k: recipe.K, lessonRequestConfidenceThreshold: recipe.ConfidenceFloor);
        timer.Stop();
        if (!result.IsSuccess || result.Value is null) throw new InvalidOperationException($"{name}: {result.ErrorCode}: {result.ErrorMessage}");
        var unit = result.Value;
        var tokenizer = (BpeTokenizer)Owner.GetField("_tokenizer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(unit)!;
        var index = (HnswIndex)Owner.GetField("_index", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(unit)!;
        string folder = Path.Combine(output, name);
        Directory.CreateDirectory(folder);
        using var stream = new MemoryStream();
        tokenizer.Save(stream);
        byte[] tokenizerBytes = stream.ToArray();
        File.WriteAllBytes(Path.Combine(folder, "tokenizer.json"), tokenizerBytes);
        var trainVectors = recipe.Train.Select(r => new VectorRow(r.Id, r.Input, tokenizer.Encode(r.Input).ToArray(),
            enhanced ? Predicates(r.Input) : [], Vector(tokenizer, r.Input, enhanced), "train")).ToArray();
        var heldoutVectors = recipe.Heldout.Select(r => new VectorRow(r.Id, r.Input, tokenizer.Encode(r.Input).ToArray(),
            enhanced ? Predicates(r.Input) : [], Vector(tokenizer, r.Input, enhanced), "heldout")).ToArray();
        // Check inspector output against vectors actually inserted by the unchanged core Train.
        var inserted = (List<float[]>)typeof(HnswIndex).GetField("_vectors", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(index)!;
        if (inserted.Count != trainVectors.Length || inserted.Where((v, i) => !SameBits(v, trainVectors[i].Vector)).Any())
            throw new InvalidDataException("Inspector vectors differ from actual trained index.");
        string vectorSha = Sha(JsonSerializer.SerializeToUtf8Bytes(new { trainVectors, heldoutVectors }, Json));
        WriteNew(Path.Combine(folder, "vectors.json"), new { trainVectors, heldoutVectors });
        WriteNew(Path.Combine(folder, "training-examples.json"), examples);
        index.Save(Path.Combine(folder, "hnsw-index.json"));
        WriteNew(Path.Combine(folder, "state.json"), new { name, enhanced, flip, tokenizerSha256 = Sha(tokenizerBytes), vectorSha256 = vectorSha,
            indexSha256 = FileSha(Path.Combine(folder, "hnsw-index.json")), examplesSha256 = FileSha(Path.Combine(folder, "training-examples.json")),
            trainingElapsedMs = timer.Elapsed.TotalMilliseconds, manifest = unit.Manifest, coreInsertedVectorsMatchInspector = true });
        return new Built(name, enhanced, unit, tokenizer, Sha(tokenizerBytes), vectorSha, examples, trainVectors, heldoutVectors);
    }

    private static PreflightResult Preflight(Built a, Built b, Recipe recipe)
    {
        var pairChecks = new List<PairCheck>();
        foreach (var group in recipe.Train.Select(r => new QueryRow(r.Id, r.Group, r.Input)).Concat(recipe.Heldout).GroupBy(r => r.Group))
        {
            var rows = group.ToArray();
            if (rows.Length != 2) throw new InvalidDataException("Each inversion group must have exactly two inputs.");
            pairChecks.Add(new PairCheck(group.Key, rows.Select(r => r.Id).ToArray(),
                SameBits(Vector(a.Tokenizer, rows[0].Input, false), Vector(a.Tokenizer, rows[1].Input, false)),
                !SameBits(Vector(b.Tokenizer, rows[0].Input, true), Vector(b.Tokenizer, rows[1].Input, true))));
        }
        bool same = a.TokenizerSha == b.TokenizerSha && recipe.Train.Select(r => r.Input).Concat(recipe.Heldout.Select(r => r.Input))
            .All(input => a.Tokenizer.Encode(input).SequenceEqual(b.Tokenizer.Encode(input)) && SameBits(Vector(a.Tokenizer, input, false), Vector(b.Tokenizer, input, false)));
        return new PreflightResult(same, pairChecks.ToArray(), same && pairChecks.All(x => x.BaselineEqual && x.EnhancedDifferent));
    }

    private static async Task<EvalResult> Evaluate(Built built, Recipe recipe, IReadOnlyDictionary<string, string> gold, string output, bool exact)
    {
        var exactIndex = new BruteForceIndex();
        if (exact) foreach (var row in built.TrainVectors) exactIndex.Add(row.Vector, row.Id);
        var labels = built.Examples.ToDictionary(x => x.ExampleId, x => x.Output, StringComparer.Ordinal);
        var cases = new List<CaseResult>();
        foreach (var row in recipe.Heldout)
        {
            if (++executeCalls > 128) throw new InvalidOperationException("ExecuteAsync budget exceeded.");
            var metadata = new Dictionary<string, object> { ["predicate_ids"] = built.Enhanced ? Predicates(row.Input) : Array.Empty<string>() };
            var timer = Stopwatch.StartNew();
            var result = await built.Unit.ExecuteAsync(new LogicalUnitRequest<string>(built.Name, "micro-order-unit", Role, Domain,
                row.Input, new LogicalExecutionFlags { UseLowConfidenceLessonSignal = false }, metadata));
            timer.Stop();
            if (!result.IsSuccess || result.Value is null) throw new InvalidOperationException($"{built.Name}/{row.Id}: {result.ErrorCode}: {result.ErrorMessage}");
            var actual = result.Value;
            var neighbors = Array.Empty<ExactNeighbor>();
            if (exact)
            {
                if (++exactSearchCalls > 112) throw new InvalidOperationException("Exact-search budget exceeded.");
                neighbors = exactIndex.Search(built.HeldoutVectors.Single(v => v.Id == row.Id).Vector, 16)
                    .Select(h => new ExactNeighbor(h.id, h.score, labels[h.id])).ToArray();
            }
            var item = new CaseResult(row.Id, row.Group, row.Input, gold[row.Id], actual.Output, actual.Output == gold[row.Id],
                gold[row.Id] == "UNSAFE" && actual.Output == "SAFE", actual.Confidence, actual.EvidenceIds,
                actual.PredicateIds, actual.Warnings, timer.Elapsed.TotalMilliseconds, neighbors);
            cases.Add(item);
            File.AppendAllText(Path.Combine(output, "case-events.ndjson"), JsonSerializer.Serialize(new { arm = built.Name, result = item }) + Environment.NewLine);
        }
        var eval = new EvalResult(built.Name, built.TokenizerSha, built.VectorSha, cases.Count(c => c.Correct), cases.Count(c => c.FalseSafe),
            exact ? cases.Count(c => c.ExactNeighbors[0].Output == c.Expected) : null, cases.ToArray());
        WriteNew(Path.Combine(output, built.Name, "cases.json"), eval);
        return eval;
    }

    private static string[] Predicates(string input)
    {
        var match = Regex.Match(input, @"\Aplan (backup|delete) (asset[0-9]+) before (backup|delete) \2\z", RegexOptions.CultureInvariant);
        if (!match.Success || match.Groups[1].Value == match.Groups[3].Value) throw new InvalidDataException("Input is outside the frozen order grammar.");
        return [match.Groups[1].Value == "backup" ? "FIRST_ACTION_BACKUP" : "FIRST_ACTION_DELETE"];
    }

    private static float[] Vector(BpeTokenizer tokenizer, string input, bool enhanced)
    {
        vectorizeCalls++;
        return (float[])CoreVectorize.Invoke(null, [tokenizer, input,
            new HashSet<string>(enhanced ? Predicates(input) : [], StringComparer.Ordinal), 64])!;
    }

    private static void ValidateInputs(TrainRow[] train, QueryRow[] heldout)
    {
        if (train.Length != 16 || heldout.Length != 16) throw new InvalidDataException("Expected fixed 16 train / 16 heldout.");
        if (train.Any(r => r.Label is not ("SAFE" or "UNSAFE")) || train.Count(r => r.Label == "SAFE") != 8)
            throw new InvalidDataException("Unexpected training label distribution.");
        if (train.Select(r => r.Id).Intersect(heldout.Select(r => r.Id), StringComparer.Ordinal).Any()) throw new InvalidDataException("Overlapping IDs.");
        var trainAssets = train.SelectMany(r => Regex.Matches(r.Input, @"asset[0-9]+").Select(m => m.Value)).ToHashSet(StringComparer.Ordinal);
        if (heldout.SelectMany(r => Regex.Matches(r.Input, @"asset[0-9]+").Select(m => m.Value)).Any(trainAssets.Contains))
            throw new InvalidDataException("Overlapping asset names.");
        foreach (string input in train.Select(r => r.Input).Concat(heldout.Select(r => r.Input))) _ = Predicates(input);
    }

    private static object Calls() => new { trainCalls, executeCalls, exactSearchCalls, vectorizeCalls, studentCalls = 0, teacherCalls = 0, dpoUpdates = 0 };
    private static string Flip(string label) => label switch { "SAFE" => "UNSAFE", "UNSAFE" => "SAFE", _ => throw new InvalidDataException("Unexpected output label.") };
    private static bool SameBits(float[] a, float[] b) => a.Length == b.Length && a.Zip(b).All(x => BitConverter.SingleToInt32Bits(x.First) == BitConverter.SingleToInt32Bits(x.Second));
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static string FileSha(string path) => Sha(File.ReadAllBytes(path));
    private static void WriteNew<T>(string path, T value)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, value, Json);
    }

    private static Dictionary<string, string> SourceHashes(string root)
    {
        string[] paths = ["tools/Rp01MechanismProbe/Program.cs", "tools/Rp01MechanismProbe/Rp01MechanismProbe.csproj",
            "21.AgentInfrastructure/Logical/KnnLogicalUnit.cs", "00.Core/LogicalContracts.cs", "00.Core/OperationResult.cs",
            "01.Tokenizer/BpeTokenizer.cs", "01.Tokenizer/RegexSplitter.cs", "06.VectorSearch/HnswIndex.cs",
            "06.VectorSearch/BruteForceIndex.cs", "06.VectorSearch/Distances.cs"];
        return paths.ToDictionary(p => p, p => FileSha(Path.Combine(root, p)), StringComparer.Ordinal);
    }

    private static void VerifyUnchanged(string root, Dictionary<string, string> before, string fixturePath, string output)
    {
        var after = SourceHashes(root);
        bool same = before.All(x => after[x.Key] == x.Value) && FileSha(fixturePath) == FixtureSha;
        WriteNew(Path.Combine(output, "source-integrity.json"), new { unchanged = same, before, after, fixtureSha256After = FileSha(fixturePath) });
        if (!same) throw new InvalidDataException("Source or fixture changed during execution.");
    }

    private static string GitHead(string root)
    {
        var info = new ProcessStartInfo("git") { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("rev-parse"); info.ArgumentList.Add("HEAD");
        using var process = Process.Start(info)!;
        string result = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException(process.StandardError.ReadToEnd());
        return result;
    }
}

internal sealed record TrainRow(string Id, string Group, string Input, string Label, string Law_Id);
internal sealed record QueryRow(string Id, string Group, string Input);
// FIX-LESSON-SIGNAL-01: ЭТИ ДВА ПОЛЯ ПЕРЕИМЕНОВЫВАТЬ НЕЛЬЗЯ, и вот почему.
//
// В договоре признаков продукта одноимённые свойства переименованы:
// `UseConfidenceFloor` стал `UseLowConfidenceLessonSignal`, а порог обучаемой
// памяти соседей — `LessonRequestConfidenceThreshold`. Здесь прежние имена
// оставлены НАМЕРЕННО, и асимметрия эта не недоделка.
//
// Причина в том, что эта запись связывается с ЗАПИСАННЫМ ФАЙЛОМ рецепта по
// именам свойств, без учёта регистра (разборщик настроен так на строке 22).
// Переименуй поле — и ключ в уже записанном файле перестанет связываться
// МОЛЧА: свойство примет значение по умолчанию, прогон пойдёт с другой
// настройкой, и никакой ошибки не возникнет. Собственная сверка файла по
// свёртке этого не поймает, потому что сам файл не менялся: сверка сравнивает
// свёртку файла с записанной, а не имена с полями.
//
// Убедиться можно прямо: ключ `"UseConfidenceFloor"` лежит в файле
// `.kiro/xiigen-slices/work/results/MICRO-KNN-ORDER-20261016-01/recipe.json`,
// строка 202. Рядом в том же файле лежит `"ConfidenceFloor"`.
//
// Если переименование здесь всё же понадобится, его нельзя делать в одиночку:
// надо одновременно переписать ключи во всех записанных файлах рецептов и
// пересчитать их свёртки, иначе прогоны разойдутся с записанными.
//
// Приёмка этого запрета — `Tests/ExperimentFixes/LowConfidenceLessonSignalContractTests.cs`:
// она сверяет имена полей этой записи с ключами записанного файла и падает,
// когда они разойдутся.
internal sealed record Recipe(string Version, TrainRow[] Train, QueryRow[] Heldout, int VectorDimensions, int K, double ConfidenceFloor, bool UseConfidenceFloor, string[] RelationPredicates);
internal sealed record VectorRow(string Id, string Input, int[] Tokens, string[] Predicates, float[] Vector, string Split);
internal sealed record Built(string Name, bool Enhanced, KnnLogicalUnit<string, string> Unit, BpeTokenizer Tokenizer, string TokenizerSha, string VectorSha,
    LogicalKnnTrainingExample<string, string>[] Examples, VectorRow[] TrainVectors, VectorRow[] HeldoutVectors);
internal sealed record PairCheck(string Group, string[] Ids, bool BaselineEqual, bool EnhancedDifferent);
internal sealed record PreflightResult(bool SameTokenizerAndTokenLayer, PairCheck[] Pairs, bool AllValid);
internal sealed record ExactNeighbor(string Id, float Score, string Output);
internal sealed record CaseResult(string Id, string Group, string Input, string Expected, string Output, bool Correct, bool FalseSafe,
    double Confidence, string[] NeighborEvidenceIds, string[] Predicates, string[] Warnings, double ElapsedMs, ExactNeighbor[] ExactNeighbors);
internal sealed record EvalResult(string Name, string TokenizerSha, string VectorSha, int Correct, int FalseSafe, int? ExactTop1Correct, CaseResult[] Cases);
internal sealed record PairResult(int Repeat, EvalResult A, EvalResult B, int AWrongBCorrect, int ACorrectBWrong);
