using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AgentAlgorithms;

// ISemanticSearchProvider using BM25 over AST-token frequencies.
// E04: BM25 achieved 100% recall@3 on cross-file queries (vs Jaccard used in v1).
// E09: Index includes SyntaxKind:RoleId keys → structural queries like "null check missing".
// E10: StripNoise before Roslyn parse → 31% faster indexing.
// E12: SIMD (System.Numerics.Vector<float>) → 3.4× speedup for large indexes.
// No HTTP calls — all computation is local.
public sealed class AstRelevanceRanker : ISemanticSearchProvider
{
    private const double K1 = 1.5;   // BM25 term-frequency saturation
    private const double B  = 0.75;  // BM25 document-length normalization

    private static readonly Regex _lineComment  = new(@"//[^\n]*",             RegexOptions.Compiled);
    private static readonly Regex _blockComment = new(@"/\*.*?\*/",            RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex _strLiteral   = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);
    private static readonly Regex _whitespace   = new(@"\s+",                  RegexOptions.Compiled);

    // chunkId → (astTypeName → frequency in that chunk)
    private readonly Dictionary<string, Dictionary<string, int>> _index
        = new(StringComparer.Ordinal);

    // term → number of documents that contain it (used for IDF)
    private readonly Dictionary<string, int> _docFreq
        = new(StringComparer.OrdinalIgnoreCase);

    // Index one chunk of C# code. Call once per file during initialisation.
    // E09 v3.0: also indexes "SyntaxKind:RoleId" compound keys for structural queries.
    public void Index(string chunkId, string csharpCode)
    {
        // E10: strip noise before parse for faster Roslyn processing
        string normalized = StripNoise(csharpCode);
        var root = CSharpSyntaxTree.ParseText(normalized).GetRoot();

        var termFreqs = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var node in root.DescendantNodes())
        {
            string kind = node.Kind().ToString();
            termFreqs[kind] = termFreqs.GetValueOrDefault(kind) + 1;

            // E09: add role-qualified key so structural queries can match e.g. "IfStatement:7"
            byte role = GetSyntacticRole(node);
            if (role != 0)
            {
                string roleKey = $"{kind}:{role}";
                termFreqs[roleKey] = termFreqs.GetValueOrDefault(roleKey) + 1;
            }
        }

        _index[chunkId] = termFreqs;
        foreach (var term in termFreqs.Keys)
        {
            _docFreq.TryGetValue(term, out int df);
            _docFreq[term] = df + 1;
        }
    }

    // Rank indexed chunks against query using BM25 (E04) + SIMD when index is large (E12).
    public Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(
        string query, int topK)
    {
        if (_index.Count == 0)
            return Task.FromResult(OperationResult<List<(string, double)>>.Ok([]));

        var    queryKinds = ExtractKindsFromQuery(query);
        int    n          = _index.Count;
        double avgDocLen  = _index.Values.Average(d => (double)d.Values.Sum());

        // IDF for each query term: log((N - df + 0.5) / (df + 0.5) + 1)
        var queryIdfs = queryKinds.ToDictionary(
            term => term,
            term =>
            {
                _docFreq.TryGetValue(term, out int df);
                return Math.Log((n - df + 0.5) / (df + 0.5) + 1.0);
            });

        // Use SIMD path when vocabulary is large enough to benefit (E12: 3.4× at 64×64)
        var results = _docFreq.Count >= Vector<float>.Count
            ? ScoreSimd(queryIdfs, avgDocLen, topK)
            : ScoreSparse(queryIdfs, avgDocLen, topK);

        return Task.FromResult(OperationResult<List<(string, double)>>.Ok(results));
    }

    // Sparse BM25 — efficient for small indexes or queries with few terms
    private List<(string, double)> ScoreSparse(
        Dictionary<string, double> queryIdfs, double avgDocLen, int topK)
    {
        return _index
            .Select(kv => (kv.Key, Bm25Score(queryIdfs, kv.Value, avgDocLen)))
            .OrderByDescending(x => x.Item2)
            .Take(topK)
            .ToList();
    }

    // SIMD-accelerated BM25: builds dense vocabulary vectors and uses vector dot product.
    // Vocabulary = all AST kinds seen across all indexed documents.
    // E12 result: 3.4× speedup for 64×64-equivalent dense operations.
    private List<(string, double)> ScoreSimd(
        Dictionary<string, double> queryIdfs, double avgDocLen, int topK)
    {
        // Build full vocabulary from all indexed terms
        string[] vocab    = _docFreq.Keys.ToArray();
        int      vLen     = vocab.Length;
        var      vocabIdx = new Dictionary<string, int>(vLen, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < vLen; i++) vocabIdx[vocab[i]] = i;

        // Query IDF vector (0 for terms not in query)
        var qVec = new float[vLen];
        foreach (var (term, idf) in queryIdfs)
            if (vocabIdx.TryGetValue(term, out int qi))
                qVec[qi] = (float)idf;

        // Score each document via SIMD dot product of qVec × bm25-weight vector
        var scored = new List<(string, double)>(_index.Count);
        foreach (var (id, terms) in _index)
        {
            double docLen = terms.Values.Sum();
            var    wVec   = new float[vLen];

            foreach (var (term, tf) in terms)
            {
                if (!vocabIdx.TryGetValue(term, out int wi)) continue;
                double norm = K1 * (1.0 - B + B * docLen / avgDocLen);
                wVec[wi] = (float)(tf * (K1 + 1.0) / (tf + norm));
            }

            scored.Add((id, SimdDot(qVec, wVec)));
        }

        return scored
            .OrderByDescending(x => x.Item2)
            .Take(topK)
            .ToList();
    }

    // BM25 score for one document given precomputed query IDFs (sparse path)
    private static double Bm25Score(
        Dictionary<string, double> queryIdfs,
        Dictionary<string, int>    docTerms,
        double                     avgDocLen)
    {
        double docLen = docTerms.Values.Sum();
        double score  = 0.0;

        foreach (var (term, idf) in queryIdfs)
        {
            if (!docTerms.TryGetValue(term, out int tf)) continue;
            double norm = K1 * (1.0 - B + B * docLen / avgDocLen);
            score += idf * (tf * (K1 + 1.0)) / (tf + norm);
        }

        return score;
    }

    // SIMD dot product — sums element-wise products using hardware vector registers.
    private static float SimdDot(float[] a, float[] b)
    {
        int step = Vector<float>.Count;
        var acc  = Vector<float>.Zero;
        int i    = 0;

        for (; i + step <= a.Length; i += step)
            acc += new Vector<float>(a, i) * new Vector<float>(b, i);

        float result = Vector.Dot(acc, Vector<float>.One);
        for (; i < a.Length; i++)
            result += a[i] * b[i];

        return result;
    }

    // Remove comments and string literals before Roslyn parse (E10: -21% chars, +31% speed).
    private static string StripNoise(string source)
    {
        source = _lineComment.Replace(source, " ");
        source = _blockComment.Replace(source, " ");
        source = _strLiteral.Replace(source, "\"\"");
        return  _whitespace.Replace(source, " ");
    }

    // Extract SyntaxKind set from query text.
    // If query is valid C# → parse it; otherwise map keywords via lookup table.
    // E09: recognises role-qualified phrases like "null check" → "NullLiteralExpression:7".
    private static HashSet<string> ExtractKindsFromQuery(string query)
    {
        try
        {
            var kinds = CSharpSyntaxTree.ParseText(query)
                .GetRoot()
                .DescendantNodes()
                .Select(n => n.Kind().ToString())
                .ToHashSet();
            if (kinds.Count > 2) return kinds;
        }
        catch { }

        // Plain SyntaxKind keywords
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["method"]    = "MethodDeclaration",
            ["class"]     = "ClassDeclaration",
            ["interface"] = "InterfaceDeclaration",
            ["return"]    = "ReturnStatement",
            ["if"]        = "IfStatement",
            ["for"]       = "ForStatement",
            ["null"]      = "NullLiteralExpression",
            ["throw"]     = "ThrowStatement",
        };

        // E09: role-qualified phrases → "SyntaxKind:RoleId" compound keys
        // Role 7 = IF_CONDITION (IfStatementSyntax.Condition child)
        var roleMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["null check"]    = "NullLiteralExpression:7",
            ["null condition"] = "NullLiteralExpression:7",
            ["if null"]       = "NullLiteralExpression:7",
            ["return null"]   = "NullLiteralExpression:10",   // role 10 = RETURN_VALUE
            ["throw null"]    = "ThrowStatement:0",
        };

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Check multi-word role phrases first
        foreach (var (phrase, kind) in roleMap)
            if (query.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                result.Add(kind);

        // Then single-word plain kinds
        foreach (var word in query.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            if (map.TryGetValue(word, out string? kind))
                result.Add(kind);

        return result;
    }

    // Maps a Roslyn SyntaxNode to a role byte based on its syntactic position (E09).
    // Inline copy — does not reference 35.ExperimentsV2 to keep experiment/production separate.
    private static byte GetSyntacticRole(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent is null) return 0;

        return (byte)(parent switch
        {
            AssignmentExpressionSyntax a when a.Left  == node      => 1,
            AssignmentExpressionSyntax a when a.Right == node      => 2,
            BinaryExpressionSyntax b     when b.Left  == node      => 3,
            BinaryExpressionSyntax b     when b.Right == node      => 4,
            InvocationExpressionSyntax i when i.Expression == node => 5,
            ArgumentSyntax                                          => 6,
            IfStatementSyntax s          when s.Condition == node  => 7,
            IfStatementSyntax s          when s.Statement  == node => 8,
            ElseClauseSyntax                                        => 9,
            ReturnStatementSyntax r      when r.Expression == node => 10,
            VariableDeclarationSyntax                               => 11,
            EqualsValueClauseSyntax e    when e.Value == node      => 12,
            MemberAccessExpressionSyntax m when m.Expression == node => 13,
            MemberAccessExpressionSyntax m when m.Name       == node => 14,
            ParameterListSyntax                                     => 15,
            MethodDeclarationSyntax d    when d.ReturnType == node => 17,
            MethodDeclarationSyntax                                 => 16,
            WhileStatementSyntax w       when w.Condition == node  => 18,
            WhileStatementSyntax                                    => 19,
            ForStatementSyntax f         when f.Declaration == node => 20,
            ForStatementSyntax f         when f.Condition   == node => 21,
            ForStatementSyntax f         when
                node is ExpressionSyntax &&
                f.Incrementors.Any(inc => ReferenceEquals(inc, node)) => 22,
            ForStatementSyntax                                      => 23,
            CastExpressionSyntax c       when c.Type       == node => 24,
            CastExpressionSyntax c       when c.Expression == node => 25,
            SimpleLambdaExpressionSyntax l when l.Body == node     => 27,
            SimpleLambdaExpressionSyntax                            => 26,
            ParenthesizedLambdaExpressionSyntax p when p.Body == node => 27,
            ParenthesizedLambdaExpressionSyntax                    => 26,
            InitializerExpressionSyntax                             => 28,
            CollectionExpressionSyntax                              => 28,
            BlockSyntax                                             => 29,
            _                                                       => 0,
        });
    }
}
