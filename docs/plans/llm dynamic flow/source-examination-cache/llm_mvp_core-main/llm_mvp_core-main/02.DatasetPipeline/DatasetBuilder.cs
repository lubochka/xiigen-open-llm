using Tokenizer;

namespace DatasetPipeline;

/// <summary>
/// Scans a source directory, tokenises every text/code file, and writes uint16 .npy shards.
/// </summary>
public class DatasetBuilder
{
    private readonly BpeTokenizer _tokenizer;
    private readonly int _seqLen;
    private readonly int _shardSize;

    // Token appended between files so the model learns file boundaries
    private const int EndOfFileToken = 1;

    public DatasetBuilder(BpeTokenizer tokenizer, int seqLen = 2048, int shardSize = 10_000_000)
    {
        _tokenizer = tokenizer;
        _seqLen = seqLen;
        _shardSize = shardSize;
    }

    public async Task BuildAsync(string sourceDir, string outputDir,
        string[] extensions = null!, CancellationToken ct = default)
    {
        extensions ??= [".ts", ".cs", ".py", ".js", ".txt", ".md"];
        Directory.CreateDirectory(outputDir);

        var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .ToArray();

        var buffer = new List<ushort>(_shardSize + 4096);
        int shardIndex = 0;

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file, ct);
            var tokens = _tokenizer.Encode(text);
            foreach (var t in tokens)
                buffer.Add((ushort)Math.Min(t, ushort.MaxValue));
            buffer.Add(EndOfFileToken);

            while (buffer.Count >= _shardSize)
            {
                var shard = buffer.Take(_shardSize).ToArray();
                var shardPath = Path.Combine(outputDir, $"shard_{shardIndex:D4}.npy");
                NpyWriter.Write(shardPath, shard);
                buffer.RemoveRange(0, _shardSize);
                shardIndex++;
            }
        }

        if (buffer.Count > 0)
        {
            var shardPath = Path.Combine(outputDir, $"shard_{shardIndex:D4}.npy");
            NpyWriter.Write(shardPath, buffer.ToArray());
        }
    }
}
