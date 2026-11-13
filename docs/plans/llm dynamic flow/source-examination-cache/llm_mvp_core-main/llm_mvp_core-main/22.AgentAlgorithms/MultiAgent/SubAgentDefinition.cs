namespace AgentAlgorithms;

// Describes a sub-agent: its role, the tools it may use, and how to route tasks to it.
public sealed record SubAgentDefinition(
    string Name,
    string Role,
    IReadOnlyList<string> AllowedTools);
