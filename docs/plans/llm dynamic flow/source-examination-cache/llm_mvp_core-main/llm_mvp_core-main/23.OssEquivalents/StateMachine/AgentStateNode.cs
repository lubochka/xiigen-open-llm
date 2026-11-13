namespace OssEquivalents;

// One node in the agent state machine.
// A node is an async function that receives the current state dictionary
// and returns an updated state dictionary.
// This mirrors LangGraph's "node" concept from the OSS plan.
public sealed record AgentStateNode(
    string                                                        Name,
    Func<Dictionary<string, object>, Task<Dictionary<string, object>>> Execute);
