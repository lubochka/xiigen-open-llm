namespace SemanticNlp;

// ─── Core annotated-token records ────────────────────────────────────────────

// SemanticToken describes a single word after a semantic algorithm has labelled it.
// All five algorithms (FrameNet, SRL, WordNet, AMR, UD) produce SemanticToken lists
// so downstream code can consume their output uniformly.
public sealed record SemanticToken
{
    // The exact substring as it appeared in the input sentence.
    public required string OriginalText { get; init; }

    // Zero-based position of this token in the sentence (in token order).
    public required int Position { get; init; }

    // Name of the semantic family the token belongs to. Examples:
    //   "Attack" (a FrameNet frame), "SRL" (the labelling system),
    //   "synset:dog.n.01" (a WordNet synset id), "nsubj" (a UD relation).
    public string? SemanticFamily { get; init; }

    // Role of the token inside its family. Examples:
    //   "Attacker" / "Victim" (FrameNet roles),
    //   "A0 (Agent)" / "A1 (Theme)" (SRL roles),
    //   "Predicate" (the trigger word of the family).
    public string? RoleWithinFamily { get; init; }
}

// SemanticAnnotation is the full result of running one algorithm on one sentence.
public sealed record SemanticAnnotation
{
    // Original sentence as received on input — kept verbatim for traceability.
    public required string OriginalSentence { get; init; }

    // The list of annotated tokens; same length and order as the input tokens.
    public required IReadOnlyList<SemanticToken> Tokens { get; init; }

    // Which algorithm produced this annotation: "FrameNet", "SRL", "WordNet", etc.
    public required string AlgorithmName { get; init; }

    // The trigger predicate (main verb) when the algorithm reports one (SRL/FrameNet).
    public string? PredicateText { get; init; }

    // The activated frame name when the algorithm reports one (FrameNet only).
    public string? FrameName { get; init; }
}

// ─── WordNet synset record ───────────────────────────────────────────────────

// Synset (synonym set) — a group of words that share one meaning.
// Every synset has a stable identifier of the form "word.pos.NN", for example
// "dog.n.01" (the noun "dog", first sense). Synsets form a directed-acyclic
// hierarchy via Hypernym (parent) and Hyponym (child) ids.
public sealed record Synset
{
    // Stable id, formatted as "lemma.partOfSpeechLetter.senseNumber".
    public required string SynsetId { get; init; }

    // All words that share this meaning — they are mutual synonyms in this sense.
    public required IReadOnlyList<string> Words { get; init; }

    // Plain-language definition of the meaning — read by humans only.
    public required string Definition { get; init; }

    // Identifiers of broader (parent) synsets. "dog.n.01" → "canine.n.01".
    public IReadOnlyList<string> HypernymIds { get; init; } = [];

    // Identifiers of narrower (child) synsets. "canine.n.01" → "dog.n.01", "wolf.n.01".
    public IReadOnlyList<string> HyponymIds { get; init; } = [];
}

// ─── AMR (Abstract Meaning Representation) graph types ──────────────────────

// AmrNode — one node in an AMR graph. Each node represents a concept.
// Penman example "(b / boy)" → variable "b", concept "boy".
public sealed record AmrNode
{
    // Short variable name used to reference this node from other nodes (e.g. "b").
    public required string VariableName { get; init; }

    // The concept this node represents. For events the form is "lemma-NN" where
    // NN is the PropBank sense number (for example "want-01"). For entities the
    // concept is just a noun (for example "boy", "police").
    public required string Concept { get; init; }

    // Optional attributes attached to this node (for example ":polarity -" for negation).
    public IReadOnlyDictionary<string, string> Attributes { get; init; }
        = new Dictionary<string, string>();
}

// AmrEdge — one labelled, directed edge between two AMR nodes.
public sealed record AmrEdge
{
    // Variable name of the source node (the parent in Penman nesting).
    public required string SourceVariable { get; init; }

    // Edge label, always starting with ':'. Examples:
    //   ":ARG0" (the agent), ":ARG1" (the theme), ":time", ":location".
    public required string RelationLabel { get; init; }

    // Variable name of the target node, or a literal string when the edge points
    // to a constant value (for example a name or a number).
    public required string TargetVariable { get; init; }
}

// AmrGraph — the full meaning graph for one sentence.
public sealed record AmrGraph
{
    // The root node of the graph — usually the main predicate of the sentence.
    public required AmrNode RootNode { get; init; }

    // All nodes indexed by their variable name for quick lookup.
    public required IReadOnlyDictionary<string, AmrNode> Nodes { get; init; }

    // All directed edges in declaration order.
    public required IReadOnlyList<AmrEdge> Edges { get; init; }

    // Original sentence used to build this graph; empty if built from raw Penman.
    public required string OriginalSentence { get; init; }
}

// ─── Universal Dependencies token + tree ────────────────────────────────────

// DependencyToken — one row of a CoNLL-U file. Ten fields are defined by the
// Universal Dependencies project; we keep the ones we use.
public sealed record DependencyToken
{
    // 1-based token number inside the sentence (the CoNLL-U "ID" field).
    public required int TokenId { get; init; }

    // Original surface form of the word (the CoNLL-U "FORM" field).
    public required string OriginalForm { get; init; }

    // Dictionary base form of the word (the CoNLL-U "LEMMA" field). "_" if absent.
    public string Lemma { get; init; } = "_";

    // Universal part-of-speech tag (the CoNLL-U "UPOS" field). Examples:
    //   "NOUN", "VERB", "ADJ", "ADV", "DET", "ADP", "CCONJ", "PUNCT".
    public string UniversalPosTag { get; init; } = "X";

    // Token id of this token's syntactic head (the CoNLL-U "HEAD" field).
    // 0 means this token is the root of the sentence.
    public required int HeadTokenId { get; init; }

    // Universal dependency relation label (the CoNLL-U "DEPREL" field). Examples:
    //   "nsubj" (nominal subject), "obj" (direct object),
    //   "det" (determiner), "amod" (adjectival modifier), "root", "punct".
    public required string DependencyRelation { get; init; }
}

// DependencyTree — the full set of DependencyToken rows for one sentence,
// plus the original surface text recovered from the "# text =" CoNLL-U comment.
public sealed record DependencyTree
{
    // Tokens in linear sentence order — the same order they appear in the file.
    public required IReadOnlyList<DependencyToken> Tokens { get; init; }

    // Original sentence text recovered from the "# text =" comment line.
    public required string OriginalSentence { get; init; }
}
