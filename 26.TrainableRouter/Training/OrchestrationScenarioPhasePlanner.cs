// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;

public sealed class OrchestrationScenarioPhasePlanner
{
    private static readonly (string Phase, string[] Keywords)[] PhaseRules =
    [
        ("intake-source", ["figma", "drawio", "xml", "docx", "attached", "source", "form", "audio", "voice", "text", "explanation", "specification", "examples", "zip", "zips"]),
        ("rag-context-planning", ["rag", "context", "previous", "documents", "retrieval", "graph", "vector", "pinecone", "cosmos", "gremlin"]),
        ("screen-and-system-analysis", ["screen", "screens", "component", "components", "theme", "style", "system parts", "document all"]),
        ("architecture-flow-design", ["architecture", "flow", "flows", "orchestrate", "orchestrated", "microservice", "microservices", "interface", "fabric", "queue", "database", "pattern", "uml", "drawio", "entity relations", "dynamic", "dna", "generic", "lifecycle", "pipeline", "content generation"]),
        ("model-round-execution", ["ai model", "ai stage", "ai stages", "several ai", "model outputs", "round", "review", "generation", "generator", "generate music", "avatar"]),
        ("technical-design-and-implementation-plan", ["technical design", "tech design", "implementation plan", "implement", "implementation", "code", "source code", "phase", "phases", "swagger", "mcp", "diet plan", "schedule", "platform-specific", "microservice base"]),
        ("test-debug-deploy", ["test", "tests", "testing", "debug", "deploy", "ci cd", "start microservice", "validate", "validation", "safe code"]),
        ("feedback-memory-loop", ["feedback", "store", "stored", "save", "saved", "whatsapp", "daily", "user input", "later execution", "previous jobs", "chat"]),
        ("multi-technology-alternatives", ["kafka", "sqs", "node.js", "rust", "java", "python", "php", "angular", "vue", "mongo", "prisma", "redis", "railway", "render", "fly.io", "kubernetes", "nomad", "swarm", "openshift", "rancher", "alternative", "alternatives", "third-party", "databases", "queues", "hosting", "azure", "cosmos", "backend", "swappable", "devops", "ci cd"]),
        ("skill-library-packaging", ["skill", "skills", "library", "copilot", "claude", "cursor", "language", "common combinations", "subfolder", "documentation skill"])
    ];

    public OperationResult<string[]> PlanPhases(
        string prompt,
        IReadOnlyList<string> expectedSteps)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return OperationResult<string[]>.Fail("Scenario prompt is required.", "SCENARIO_PROMPT_REQUIRED");

        var text = $"{prompt} {string.Join(" ", expectedSteps)}".ToLowerInvariant();
        var phases = PhaseRules
            .Where(rule => rule.Keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Select(rule => rule.Phase)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return phases.Length == 0
            ? OperationResult<string[]>.Fail("No orchestration phase could be inferred from the prompt.", "NO_PHASE_INFERRED")
            : OperationResult<string[]>.Ok(phases);
    }
}
