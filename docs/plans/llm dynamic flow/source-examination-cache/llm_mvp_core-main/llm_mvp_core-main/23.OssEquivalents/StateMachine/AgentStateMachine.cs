namespace OssEquivalents;

// Runs an agent as an explicit state machine.
// Nodes are async functions; edges are conditional predicates.
// A checkpoint is saved after every transition.
// This is the .NET equivalent of @langchain/langgraph from the OSS plan.
public sealed class AgentStateMachine
{
    private readonly Dictionary<string, AgentStateNode> _nodes
        = new(StringComparer.Ordinal);

    // from-state → list of (condition predicate, next state name)
    private readonly Dictionary<string, List<(Func<Dictionary<string, object>, bool> Condition, string NextState)>> _edges
        = new(StringComparer.Ordinal);

    private readonly StateCheckpointer _checkpointer;

    public int CheckpointsSaved { get; private set; }

    public AgentStateMachine(StateCheckpointer checkpointer)
        => _checkpointer = checkpointer;

    // Register a state node.
    public void AddNode(AgentStateNode node)
        => _nodes[node.Name] = node;

    // Register a conditional edge from one state to another.
    public void AddConditionalEdge(
        string fromState,
        Func<Dictionary<string, object>, bool> condition,
        string toState)
    {
        if (!_edges.ContainsKey(fromState))
            _edges[fromState] = [];
        _edges[fromState].Add((condition, toState));
    }

    // Run the machine from startingStateName until terminalStateName.
    public async Task<OperationResult<Dictionary<string, object>>> RunAsync(
        Dictionary<string, object> initialState,
        string                     startingStateName,
        string                     terminalStateName,
        CancellationToken          cancellationToken = default)
    {
        var   current     = initialState;
        string stateName  = startingStateName;

        while (stateName != terminalStateName)
        {
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<Dictionary<string, object>>.Fail("Cancelled", "CANCELLED");

            if (!_nodes.TryGetValue(stateName, out var node))
                return OperationResult<Dictionary<string, object>>.Fail(
                    $"No node registered for state '{stateName}'.", "UNKNOWN_STATE");

            // Execute the node function to produce the updated state.
            current = await node.Execute(current);

            // Save a checkpoint after each transition.
            await _checkpointer.SaveAsync(stateName, current);
            CheckpointsSaved++;

            // Find the first matching conditional edge.
            string? nextState = null;
            if (_edges.TryGetValue(stateName, out var edges))
            {
                foreach (var (cond, target) in edges)
                {
                    if (cond(current)) { nextState = target; break; }
                }
            }

            if (nextState is null)
                return OperationResult<Dictionary<string, object>>.Fail(
                    $"No transition matched from state '{stateName}'.", "NO_TRANSITION");

            stateName = nextState;
        }

        return OperationResult<Dictionary<string, object>>.Ok(current);
    }
}
