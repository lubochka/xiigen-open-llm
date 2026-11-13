namespace ExperimentsV2;

// AttributedRouter combines E21 (PromptSplitter) and E22 (DatasetRouter) into one component.
// Input: full user prompt.
// Output: for each active dataset — only its slice of the prompt.
public sealed class AttributedRouter
{
    private readonly PromptSplitter _splitter;
    private readonly DatasetRouter  _router;

    public AttributedRouter(DatasetRouter trainedRouter,
        double splitThreshold = 0.4, int windowSize = 3)
    {
        _splitter = new PromptSplitter(splitThreshold, windowSize);
        _router   = trainedRouter;
    }

    public OperationResult<AttributedRoutingResult> Route(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<AttributedRoutingResult>.Fail("Empty prompt");

        var splitResult = _splitter.Split(prompt);
        if (!splitResult.IsSuccess)
            return OperationResult<AttributedRoutingResult>.Fail(
                "Splitter: " + splitResult.ErrorMessage);

        var segments    = splitResult.Value!;
        var attribution = new Dictionary<string, List<string>>();

        foreach (var segment in segments)
        {
            int[] preds       = TextToPredicates(segment.Text);
            bool[] dsVector   = _router.Predict(preds);

            for (int d = 0; d < DatasetRouter.DatasetNames.Length; d++)
            {
                if (!dsVector[d]) continue;
                string key = DatasetRouter.DatasetNames[d];
                if (!attribution.ContainsKey(key))
                    attribution[key] = new List<string>();
                attribution[key].Add(segment.Text);
            }
        }

        if (attribution.Count == 0)
            attribution["Generic"] = new List<string> { prompt };

        var result = attribution.ToDictionary(
            kv => kv.Key,
            kv => string.Join("\n", kv.Value));

        return OperationResult<AttributedRoutingResult>.Ok(new AttributedRoutingResult
        {
            DatasetToSubprompt = result.AsReadOnly()
        });
    }

    // Convert a natural-language segment to integer predicates via SemanticNormalizer + FamilyCollapser.
    private static int[] TextToPredicates(string text)
    {
        var words    = SemanticNormalizer.Normalize(text);
        var families = FamilyCollapser.Collapse(words);

        var preds = new List<int>();
        foreach (var fam in families)
        {
            int pred = fam switch
            {
                "VALIDATE_OP" => LogicExtractor.VALIDATE_OP,
                "GET_OP"      => LogicExtractor.GET_CALL,
                "SET_OP"      => LogicExtractor.ASSIGN,
                "CREATE_OP"   => LogicExtractor.CREATE_OP,
                "DELETE_OP"   => LogicExtractor.FAIL,
                "AUTH_OP"     => LogicExtractor.VALIDATE_OP,
                "PROCESS_OP"  => LogicExtractor.PROCESS_OP,
                "SEND_OP"     => LogicExtractor.SEND_OP,
                "LOG_OP"      => LogicExtractor.LOG_OP,
                _             => LogicExtractor.UNKNOWN
            };
            if (pred != LogicExtractor.UNKNOWN)
                preds.Add(pred);
        }

        // No mapped ops → use UNKNOWN sentinel so K-NN sees a "generic" query
        return preds.Count > 0 ? preds.ToArray() : new[] { LogicExtractor.UNKNOWN };
    }
}
