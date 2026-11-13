using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tokenizer;

internal static class Program
{
    private const string CoreCommit = "62bcc8f2368840e22d2ea98ef16d9bd526ad9a39";

    public static int Main(string[] args)
    {
        if (args.Length != 3 || (args[0] != "A" && args[0] != "B"))
            throw new ArgumentException("Usage: <A|B> <code-doc|en-ru> <new-result.json>");
        var arm = args[0];
        var domain = args[1];
        var (train, exam) = Data(domain);
        // Exactly eight repeats form one fixed corpus; there are no epoch loops
        // or caller-selected repetitions. The native tokenizer learns once.
        var corpus = string.Join("\n", Enumerable.Range(0, 8).SelectMany(_ => train));
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("", 256); // A0/B0 byte-vocabulary state, no learned merges.
        BpeTokenizer.ExperimentElideUnchangedChunkCopies = false;
        var before = Observe(tokenizer, exam);
        var beforeHash = tokenizer.ComputeStateSha256();
        var beforeVocab = tokenizer.VocabSize;
        BpeTokenizer.ExperimentElideUnchangedChunkCopies = arm == "B";
        using var process = Process.GetCurrentProcess();
        var cpu0 = process.TotalProcessorTime;
        var watch = new Stopwatch();
        var alloc0 = GC.GetAllocatedBytesForCurrentThread();
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        watch.Start();
        tokenizer.Train(corpus, 320); // Actual unchanged native BPE learner.
        watch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - alloc0;
        process.Refresh();
        var cpuMs = (process.TotalProcessorTime - cpu0).TotalMilliseconds;
        var gcCounts = new[] { GC.CollectionCount(0)-gen0, GC.CollectionCount(1)-gen1, GC.CollectionCount(2)-gen2 };
        var trainNoMatchReuses = BpeTokenizer.ExperimentNoMatchReuses;
        var trainChangedCopies = BpeTokenizer.ExperimentChangedChunkCopies;
        var peakWorkingSet = process.PeakWorkingSet64;
        var afterHash = tokenizer.ComputeStateSha256();
        var after = Observe(tokenizer, exam);
        var readout = new
        {
            id = "BPE-NO-MATCH-COPY-01", core_commit = CoreCommit, arm, domain,
            actual_learner = "01.Tokenizer/BpeTokenizer.cs::Train",
            downstream_learning_callsite = "21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:136-153",
            neural_optimizer_steps = 0, tokenizer_training_calls = 2,
            learning_transitions_measured = 1, target_vocabulary = 320,
            train_utf8_bytes = Encoding.UTF8.GetByteCount(corpus),
            train_sha256 = Hash(Encoding.UTF8.GetBytes(corpus)),
            train_strings = train, corpus_repeat_count = 8, exam_strings = exam,
            state_before_sha256 = beforeHash, state_after_sha256 = afterHash,
            vocabulary_before = beforeVocab, vocabulary_after = tokenizer.VocabSize,
            learned_merge_count = tokenizer.VocabSize - beforeVocab,
            before, after,
            train_cost = new { allocated_bytes = allocated, elapsed_ms = watch.Elapsed.TotalMilliseconds,
                process_cpu_ms = cpuMs, gc_collections = gcCounts },
            process_peak_working_set_bytes = peakWorkingSet,
            mechanism = new { train_no_match_reuses = trainNoMatchReuses, train_changed_chunk_copies = trainChangedCopies },
            limitations = new[] { "No neural/KNN accuracy measured", "One run per arm/domain; short wall-time measurement may be noisy", "BPE ordering/vocabulary/ranks intentionally unchanged" }
        };
        using var stream = new FileStream(args[2], FileMode.CreateNew, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(stream, readout, new JsonSerializerOptions { WriteIndented = true });
        return 0;
    }

    private static object[] Observe(BpeTokenizer tokenizer, string[] exam)
    {
        return exam.Select(text =>
        {
            var ids = tokenizer.Encode(text);
            var decoded = tokenizer.Decode(ids);
            return (object)new { input = text, token_count = ids.Count, tokens = ids,
                roundtrip_exact = decoded == text, decoded };
        }).ToArray();
    }

    private static string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();

    private static (string[] Train, string[] Exam) Data(string domain) => domain switch
    {
        "code-doc" => (
            ["int Add(int a, int b) => a + b; // return the sum",
             "int Subtract(int a, int b) => a - b; // return the difference",
             "int Multiply(int a, int b) => a * b; // return the product",
             "string Echo(string value) => value; // return the same text",
             "// identifiers: alpha beta gamma delta aaaa aaaaa aaaaaa"],
            ["int Add(int left, int right) => left + right; // return the sum",
             "int Subtract(int left, int right) => left - right; // return the difference",
             "aaaa aaaaa aaaaaa", "// value: Ω + ё"]),
        "en-ru" => (
            ["Dana opened the door. => Дана открыла дверь.",
             "Dana did not open the door. => Дана не открыла дверь.",
             "Mira closed the window. => Мира закрыла окно.",
             "Mira did not close the window. => Мира не закрыла окно.",
             "book books door doors окно окна аааа ааааа"],
            ["Nora opened the window. => Нора открыла окно.",
             "Nora did not open the window. => Нора не открыла окно.",
             "аааа ааааа aaaa aaaaa", "Ёж увидел окно; Ω."]),
        _ => throw new ArgumentException("Only the two preregistered domains are permitted.")
    };
}
