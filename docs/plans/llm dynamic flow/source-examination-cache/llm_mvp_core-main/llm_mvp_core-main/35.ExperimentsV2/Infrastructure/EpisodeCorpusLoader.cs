namespace ExperimentsV2;

// Loads orchestrator training data from episode documents.
// Each episode = task + sub-episode count + type tags.
public static class EpisodeCorpusLoader
{
    public record EpisodeExample(
        string CanonicalPrompt,
        string[] AlternativePhrasings,
        int     SubEpisodeCount,
        string  EpisodeType,
        bool    HasIteration);

    public static IReadOnlyList<EpisodeExample> LoadAll() => new EpisodeExample[]
    {
        new(
            "I have a business description and some diagrams. Help me turn this into a system I can actually build.",
            new[] {
                "I know what I want to build but I don't know where to start",
                "How do I go from a feature list to an actual architecture?",
                "I have a rough idea of the product — can you help me structure it?",
                "We sketched out the flows on a whiteboard, now what?",
                "I need to turn my wireframes and notes into something a developer can work with",
                "How do I get from business requirements to a technical plan?",
                "My product spec is ready — how do I figure out what to build first?",
                "I have diagrams of what users do but no idea what the actual code should look like",
            },
            SubEpisodeCount: 4,
            EpisodeType: "DESIGN",
            HasIteration: false),

        new(
            "I'm building a tool that produces output I can see, but it keeps coming out wrong. I need to fix it round by round.",
            new[] {
                "My tool generates output but it never looks right",
                "I keep fixing the same type of problem but it breaks somewhere else",
                "The output looks almost right but there's always something off",
                "How do I improve something when I can only tell it's wrong by looking at it?",
                "We've done ten rounds of fixes and it's still not working properly",
                "I fix one bug and another appears — how do I get ahead of this?",
                "The AI keeps generating the wrong layout even after I tell it what's wrong",
                "How do you handle a problem that only shows up in the final output?",
            },
            SubEpisodeCount: 5,
            EpisodeType: "BUILD",
            HasIteration: true),

        new(
            "My platform's modules are too tightly coupled. I want tenants to be able to copy, adapt, and share them independently.",
            new[] {
                "How do I make my system less tangled?",
                "I want clients to be able to customize without breaking everything",
                "My modules depend on each other too much — how do I untangle them?",
                "I need a way to let customers fork parts of the system",
                "The platform is monolithic and I need to make it modular",
                "How do I make each part independent enough to be reused?",
                "Tenants need their own version of certain modules without affecting others",
                "How do I decouple the parts of my system so they can evolve separately?",
            },
            SubEpisodeCount: 5,
            EpisodeType: "RESEARCH",
            HasIteration: false),

        new(
            "Before anything can run, too much is missing. I need to build the foundation before anything else can start.",
            new[] {
                "Nothing works because the basics aren't in place",
                "I need to fix the infrastructure before I can build features",
                "The system won't start because too many dependencies are missing",
                "I can't test anything until the foundation is working",
                "Everything is blocked waiting for the base layer to be ready",
                "How do I build the scaffolding before the real work can begin?",
                "The prerequisites aren't met — I need to clear them before moving forward",
                "I need to get the platform to a working state before adding features",
            },
            SubEpisodeCount: 2,
            EpisodeType: "FIX",
            HasIteration: false),

        new(
            "Move my entire platform from one technology to another without breaking what's working.",
            new[] {
                "I need to migrate the platform to a new stack",
                "How do I switch technologies without losing functionality?",
                "We decided to change frameworks — how do we do it safely?",
                "I need to port the whole system to a different language or platform",
                "The old technology is no longer supported — how do I move forward?",
                "How do I migrate incrementally without a big-bang rewrite?",
                "We need to move from X to Y — where do we start?",
                "I need to replace the foundation while the house is still running",
            },
            SubEpisodeCount: 3,
            EpisodeType: "MIGRATE",
            HasIteration: false),

        new(
            "Make my platform work inside a WordPress site without changing how WordPress works.",
            new[] {
                "I need to embed my system inside an existing CMS",
                "How do I integrate without changing the host platform?",
                "My tool needs to live inside someone else's platform",
                "I need a plugin that adds my system to an existing site",
                "How do I make my app work as an add-on to WordPress?",
                "The client wants this inside their WordPress — how do we do that?",
                "I need to create a bridge between my platform and WordPress",
                "How do I package my system so it can run as a plugin?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "BUILD",
            HasIteration: true),

        new(
            "Expand to 14 platforms and build a marketplace where others can publish plugins.",
            new[] {
                "I want to support many more platforms at once",
                "How do I build a plugin marketplace for my tool?",
                "I need an ecosystem where third parties can contribute",
                "How do I make my platform extensible by others?",
                "I want to scale from one integration to dozens simultaneously",
                "We need a way for external developers to add to the system",
                "How do I create a distribution model for plugins and extensions?",
                "I want to build something that others can build on top of",
            },
            SubEpisodeCount: 4,
            EpisodeType: "DESIGN",
            HasIteration: false),

        new(
            "Extract what my old codebases know so any AI assistant can use that knowledge.",
            new[] {
                "How do I get knowledge out of old code into something useful?",
                "I want to turn legacy code into training data",
                "My old projects contain a lot of hard-won knowledge — how do I preserve it?",
                "How do I make past work useful for future AI assistance?",
                "I need to extract patterns from codebases that no longer run",
                "How do I teach an AI what my old team knew?",
                "I want to mine my codebase history for reusable knowledge",
                "How do I turn years of code into a structured knowledge base?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "RESEARCH",
            HasIteration: false),

        new(
            "The AI assistant's own working guidelines were getting stale and inconsistent. They needed to be rebuilt from scratch.",
            new[] {
                "My AI's instructions are contradicting each other",
                "The system prompt has grown out of control — how do I fix it?",
                "My working guidelines for the AI are out of date",
                "How do I rebuild the assistant's rules without losing what works?",
                "The AI keeps making the same mistakes because the instructions are wrong",
                "I need to audit and rewrite the instructions that govern how the AI works",
                "The guidance documents are inconsistent — how do I align them?",
                "My AI assistant is following outdated rules — how do I update them systematically?",
            },
            SubEpisodeCount: 2,
            EpisodeType: "FIX",
            HasIteration: true),

        new(
            "Build an orchestration advisor that searches past solutions and returns grounded suggestions using three local models.",
            new[] {
                "I want a tool that recommends how to approach a problem based on past work",
                "How do I build a system that searches its own history for answers?",
                "I need an advisor that uses previous solutions as context",
                "How do I make an AI that learns from its own past decisions?",
                "I want to build something that can recommend approaches based on solved problems",
                "How do I create a retrieval system over my own problem-solving history?",
                "I need a multi-model system that compares different approaches",
                "How do I build a grounded advisor that doesn't hallucinate?",
            },
            SubEpisodeCount: 3,
            EpisodeType: "BUILD",
            HasIteration: false),
    }.AsReadOnly();

    // Expand dataset: canonical + 8 alternative phrasings per episode = 90 examples.
    public static List<(string Prompt, int SubEpisodeCount, string Type)> Expand()
    {
        var result = new List<(string, int, string)>();
        foreach (var ep in LoadAll())
        {
            result.Add((ep.CanonicalPrompt, ep.SubEpisodeCount, ep.EpisodeType));
            foreach (var alt in ep.AlternativePhrasings)
                result.Add((alt, ep.SubEpisodeCount, ep.EpisodeType));
        }
        return result;
    }
}
