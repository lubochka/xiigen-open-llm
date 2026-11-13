namespace SemanticCompiler29;

// ISemanticCompiler — main fabric interface for the five-stage pipeline.
// 16.AutonomousLearning.DataPipeline must inject this interface, not copy the implementation.
// Rule: grep -n "ISemanticCompiler29" 16.AutonomousLearning/DataPipeline.cs must return a line.
public interface ISemanticCompiler29
{
    // Run the full pipeline: StackDetector → RouteClassifier → E15 → E16 → E17.
    // Returns predicates, domain, and compression ratio.
    OperationResult<PipelineOutput> Compile(PipelineInput input);
}

// IStackDetector29 — detects programming language from raw text.
// Must run BEFORE any normalization (E20 Critical Rule A).
public interface IStackDetector29
{
    // Returns "csharp" | "typescript" | "python" | "unknown".
    string Detect(string rawText);
}

// IRouteClassifier29 — routes input to one of five domains (E18, 95% accuracy).
public interface IRouteClassifier29
{
    // Classify a token array into one of the five D-04 canonical domains.
    Domain Classify(string[] tokens);
}

// ISemanticNormalizer29 — stemming + stop-word removal (E15, +9.7%).
public interface ISemanticNormalizer29
{
    // Return a normalized (stemmed, stop-words removed) token array.
    string[] Normalize(string text);
}

// IFamilyCollapser29 — collapse word families to root form (E16, +8.4%).
public interface IFamilyCollapser29
{
    // Collapse tokens so "validation", "validates", "validated" → "validate".
    string[] Collapse(string[] tokens);
}

// ILogicExtractor29 — extract logical predicates, 5% of original size (E17, +97%).
public interface ILogicExtractor29
{
    // Return predicate strings like "VALIDATE_OP(X)", "NULL_CHECK(Y)".
    string[] Extract(string[] tokens);
}
