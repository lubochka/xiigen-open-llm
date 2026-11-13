// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This record describes one registered logical flow and the route it can serve.
public sealed record LogicalFlowDescriptor(
    string FlowId,
    string ScenarioId,
    string Version,
    Type InputType,
    string PhaseId,
    string SubPhaseId,
    string TaskType,
    string[] DomainTags,
    long RegistrationOrder);

// This record carries one already-loaded logical flow or factory plus its persisted descriptor.
public sealed record LogicalFlowRegistryLoadedEntry(LogicalFlowDescriptor Descriptor, object FlowOrFactory);

// Opaque exact-tuple token used only for compensating a registration owned by one publisher.
public sealed class LogicalFlowRegistrationToken
{
    internal LogicalFlowRegistrationToken(
        InMemoryLogicalFlowRegistry registry,
        LogicalFlowDescriptor descriptor,
        object flowOrFactory)
    {
        Registry = registry;
        Descriptor = descriptor;
        FlowOrFactory = flowOrFactory;
    }

    internal InMemoryLogicalFlowRegistry Registry { get; }
    internal LogicalFlowDescriptor Descriptor { get; }
    internal object FlowOrFactory { get; }
}

public sealed record LogicalFlowCompensatableRegistration(
    LogicalFlowDescriptor Descriptor,
    LogicalFlowRegistrationToken Token);

// This registry is the first local storage surface for logical flows.
public sealed class InMemoryLogicalFlowRegistry
{
    private readonly CrossThreadRegistryGate _gate = new();
    private static readonly AsyncLocal<Action?> BeforeCompensatableMutation = new();

    internal static Action? BeforeCompensatableMutationForTesting
    {
        get => BeforeCompensatableMutation.Value;
        set => BeforeCompensatableMutation.Value = value;
    }

    // The flow table stores descriptors and executable flow objects or factories together.
    private readonly List<(LogicalFlowDescriptor Descriptor, object FlowOrFactory)> _entries = [];

    // This counter preserves registration order for latest-flow resolution.
    private long _nextRegistrationOrder;

    // Register one logical flow for one scenario without route metadata.
    public OperationResult<LogicalFlowDescriptor> Register<TInput>(string flowId, string scenarioId, string version, ILogicalFlow<TInput> flow)
    {
        return Register(flowId, scenarioId, version, "", "", "", [], flow);
    }

    // Register one logical flow for one scenario and one route slot.
    public OperationResult<LogicalFlowDescriptor> Register<TInput>(
        string flowId,
        string scenarioId,
        string version,
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        ILogicalFlow<TInput> flow)
    {
        // A missing flow is a bad registry entry even when all identity fields are present.
        if (flow is null)
            return OperationResult<LogicalFlowDescriptor>.Fail("Flow cannot be null.");

        // Store the flow object.
        return RegisterFlowObject(flowId, scenarioId, version, phaseId, subPhaseId, taskType, domainTags, typeof(TInput), flow);
    }

    // Register one logical flow factory for one scenario and one route slot.
    public OperationResult<LogicalFlowDescriptor> RegisterFactory<TInput>(
        string flowId,
        string scenarioId,
        string version,
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        ILogicalFlowFactory<TInput> factory)
    {
        // Reject missing factories before storing a descriptor.
        if (factory is null)
            return OperationResult<LogicalFlowDescriptor>.Fail("Flow factory cannot be null.");

        // Store the factory object.
        return RegisterFlowObject(flowId, scenarioId, version, phaseId, subPhaseId, taskType, domainTags, typeof(TInput), factory);
    }

    // Register one factory and return an opaque token that can remove only that exact tuple.
    public OperationResult<LogicalFlowCompensatableRegistration>
        RegisterFactoryCompensatable<TInput>(
            string flowId,
            string scenarioId,
            string version,
            string phaseId,
            string subPhaseId,
            string taskType,
            string[] domainTags,
            ILogicalFlowFactory<TInput> factory)
    {
        if (factory is null)
            return OperationResult<LogicalFlowCompensatableRegistration>.Fail(
                "Flow factory cannot be null.");

        return RegisterFlowObjectCompensatable(
            flowId,
            scenarioId,
            version,
            phaseId,
            subPhaseId,
            taskType,
            domainTags,
            typeof(TInput),
            factory);
    }

    public bool Rollback(LogicalFlowRegistrationToken? token)
    {
        if (token is null || !ReferenceEquals(token.Registry, this))
            return false;

        using (_gate.Enter())
        {
            var index = _entries.FindIndex(candidate =>
                candidate.Descriptor == token.Descriptor &&
                ReferenceEquals(candidate.FlowOrFactory, token.FlowOrFactory));
            if (index < 0)
                return false;
            _entries.RemoveAt(index);
            return true;
        }
    }

    // Serialize one multi-entry publisher against every public read/write path.
    internal IDisposable AcquirePublicationLease()
        => _gate.Enter();

    internal OperationResult<IDisposable> TryAcquirePublicationLease(
        TimeSpan timeout)
    {
        var timedOut = OperationResult<IDisposable>.Fail(
            "Logical flow registry publication lease acquisition timed out.",
            "LOGICAL_FLOW_REGISTRY_PUBLICATION_LEASE_TIMEOUT");
        return _gate.TryEnter(timeout, out var lease)
            ? OperationResult<IDisposable>.Ok(lease!)
            : timedOut;
    }

    // Resolve a flow by exact flow id and input type.
    public OperationResult<ILogicalFlow<TInput>> Resolve<TInput>(string flowId)
    {
        // Find the newest matching flow id and input type.
        var entry = FindByFlowId<TInput>(flowId);

        // Return a clean failure when no flow matches.
        if (entry.FlowOrFactory is null)
            return OperationResult<ILogicalFlow<TInput>>.Fail($"No logical flow registered for id '{flowId}'.");

        // Return the executable flow under its typed interface.
        return ResolveFlowObject<TInput>(entry.FlowOrFactory, factoryRequest: null);
    }

    // Resolve exactly one newest string-input factory registration by flow identity.
    public OperationResult<LogicalFlowBinding> ResolveFactoryBindingByFlowId(
        string? flowId,
        string? expectedScenarioId,
        LogicalFlowFactoryRequest? factoryRequest)
    {
        if (factoryRequest is null)
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Flow factory request is required.",
                "FLOW_FACTORY_REQUEST_REQUIRED");
        }

        if (!IsCanonicalBindingIdentity(flowId)
            || !IsCanonicalBindingIdentity(expectedScenarioId)
            || !IsCanonicalBindingIdentity(factoryRequest.RunId))
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Flow factory binding identity is invalid.",
                "FLOW_FACTORY_BINDING_IDENTITY_INVALID");
        }

        var entry = FindByFlowId<string>(flowId!);
        if (entry.FlowOrFactory is null)
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "No exact string-input logical flow factory registration was found.",
                "FLOW_FACTORY_REGISTRATION_NOT_FOUND");
        }

        if (!IsCanonicalBindingIdentity(entry.Descriptor.FlowId)
            || !IsCanonicalBindingIdentity(entry.Descriptor.ScenarioId)
            || !StringComparer.Ordinal.Equals(entry.Descriptor.FlowId, flowId))
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Registered flow descriptor has incompatible binding identity.",
                "INCOMPATIBLE_FLOW_REGISTRATION");
        }

        if (!StringComparer.Ordinal.Equals(entry.Descriptor.ScenarioId, expectedScenarioId))
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Registered flow factory scenario does not match the requested scenario.",
                "FLOW_FACTORY_BINDING_SCENARIO_MISMATCH");
        }

        if (entry.FlowOrFactory is not ILogicalFlowFactory<string> factory)
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Newest exact string-input registration is not a logical flow factory.",
                "FLOW_FACTORY_REGISTRATION_REQUIRED");
        }

        var created = factory.Create(factoryRequest);
        if (created is null || (created.IsSuccess && created.Value is null))
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Flow factory result is required.",
                "FLOW_FACTORY_RESULT_REQUIRED");
        }

        if (!created.IsSuccess)
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                created.ErrorMessage,
                created.ErrorCode);
        }

        return OperationResult<LogicalFlowBinding>.Ok(
            new LogicalFlowBinding(entry.Descriptor.FlowId, created.Value!)
            {
                ScenarioId = entry.Descriptor.ScenarioId
            });
    }

    // Resolve a flow by scenario id and input type.
    public OperationResult<ILogicalFlow<TInput>> ResolveByScenario<TInput>(string scenarioId)
    {
        // Find the newest flow for the requested scenario id and input type.
        var entry = FindByScenario<TInput>(scenarioId);

        // Return a clean failure when no flow matches the scenario.
        if (entry.FlowOrFactory is null)
            return OperationResult<ILogicalFlow<TInput>>.Fail($"No logical flow registered for scenario '{scenarioId}'.");

        // Return the executable flow under its typed interface.
        return ResolveFlowObject<TInput>(entry.FlowOrFactory, factoryRequest: null);
    }

    // Resolve a flow by sub-phase id and input type.
    public OperationResult<ILogicalFlow<TInput>> ResolveBySubPhase<TInput>(string subPhaseId)
    {
        // Reject empty route ids because topology selection needs an explicit sub-phase.
        if (string.IsNullOrWhiteSpace(subPhaseId))
            return OperationResult<ILogicalFlow<TInput>>.Fail("Sub-phase id cannot be empty.");

        // Find the newest flow for the requested sub-phase id and input type.
        var entry = FindBySubPhase<TInput>(subPhaseId);

        // Return a clean failure when no flow matches the route.
        if (entry.FlowOrFactory is null)
            return OperationResult<ILogicalFlow<TInput>>.Fail($"No logical flow registered for sub-phase '{subPhaseId}'.");

        // Return the executable flow under its typed interface.
        return ResolveFlowObject<TInput>(entry.FlowOrFactory, factoryRequest: null);
    }

    // Resolve a scenario binding for the Phase 28 in-memory orchestrator.
    public OperationResult<LogicalFlowBinding> ResolveBindingByScenario(string scenarioId)
    {
        return ResolveBindingByScenario(scenarioId, factoryRequest: null);
    }

    // Resolve a scenario binding and construct factories when requested.
    public OperationResult<LogicalFlowBinding> ResolveBindingByScenario(string scenarioId, LogicalFlowFactoryRequest? factoryRequest)
    {
        // Find the newest string-input flow for the requested scenario.
        var entry = FindByScenario<string>(scenarioId);

        // Return a clean failure when no compatible binding exists.
        if (entry.FlowOrFactory is null)
            return OperationResult<LogicalFlowBinding>.Fail($"No string-input logical flow registered for scenario '{scenarioId}'.");

        // Build the executable flow.
        var flow = ResolveFlowObject<string>(entry.FlowOrFactory, factoryRequest);
        if (!flow.IsSuccess)
            return OperationResult<LogicalFlowBinding>.Fail(flow.ErrorMessage, flow.ErrorCode);

        // Return the binding shape the in-memory orchestrator already understands.
        return OperationResult<LogicalFlowBinding>.Ok(new LogicalFlowBinding(entry.Descriptor.FlowId, flow.Value!)
        {
            ScenarioId = entry.Descriptor.ScenarioId
        });
    }

    // Resolve a string-input binding by sub-phase id.
    public OperationResult<LogicalFlowBinding> ResolveBindingBySubPhase(string subPhaseId)
    {
        return ResolveBindingBySubPhase(subPhaseId, factoryRequest: null);
    }

    // Resolve a string-input binding by sub-phase id and construct factories when requested.
    public OperationResult<LogicalFlowBinding> ResolveBindingBySubPhase(string subPhaseId, LogicalFlowFactoryRequest? factoryRequest)
    {
        // Reject empty route ids because topology selection needs an explicit sub-phase.
        if (string.IsNullOrWhiteSpace(subPhaseId))
            return OperationResult<LogicalFlowBinding>.Fail("Sub-phase id cannot be empty.");

        // Find the newest string-input flow for the requested sub-phase.
        var entry = FindBySubPhase<string>(subPhaseId);

        // Return a clean failure when no compatible binding exists.
        if (entry.FlowOrFactory is null)
            return OperationResult<LogicalFlowBinding>.Fail($"No string-input logical flow registered for sub-phase '{subPhaseId}'.");

        // Reject malformed persisted binding identities before a factory can observe the request.
        if (!IsCanonicalBindingIdentity(entry.Descriptor.FlowId)
            || (!string.IsNullOrWhiteSpace(entry.Descriptor.ScenarioId)
                && !IsCanonicalBindingIdentity(entry.Descriptor.ScenarioId)))
        {
            return OperationResult<LogicalFlowBinding>.Fail(
                "Registered flow descriptor has incompatible binding identity.",
                "INCOMPATIBLE_FLOW_REGISTRATION");
        }

        // Build or return the executable flow.
        var flow = ResolveFlowObject<string>(entry.FlowOrFactory, factoryRequest);
        if (!flow.IsSuccess)
            return OperationResult<LogicalFlowBinding>.Fail(flow.ErrorMessage, flow.ErrorCode);

        // Return the binding shape the orchestrator understands.
        return OperationResult<LogicalFlowBinding>.Ok(new LogicalFlowBinding(entry.Descriptor.FlowId, flow.Value!)
        {
            ScenarioId = entry.Descriptor.ScenarioId
        });
    }

    // Return descriptors for audit and tests.
    public IReadOnlyList<LogicalFlowDescriptor> ListDescriptors()
    {
        // Return fresh descriptor and domain-tag snapshots so inspection cannot mutate registry metadata.
        using (_gate.Enter())
            return _entries.Select(entry => CloneDescriptor(entry.Descriptor)).ToArray();
    }

    internal OperationResult<long> PeekNextRegistrationOrderForPublication()
    {
        using (_gate.Enter())
        {
            return _nextRegistrationOrder >= long.MaxValue - 1
                ? OperationResult<long>.Fail(
                    "Logical flow registration order is exhausted.",
                    "LOGICAL_FLOW_REGISTRATION_ORDER_EXHAUSTED")
                : OperationResult<long>.Ok(_nextRegistrationOrder + 1);
        }
    }

    // Load persisted registry entries directly without re-registering or changing their registration order.
    public OperationResult<int> LoadEntries(
        IReadOnlyList<LogicalFlowRegistryLoadedEntry> entries,
        bool replaceExisting = true)
    {
        using (_gate.Enter())
            return LoadEntriesUnderGate(entries, replaceExisting);
    }

    private OperationResult<int> LoadEntriesUnderGate(
        IReadOnlyList<LogicalFlowRegistryLoadedEntry> entries,
        bool replaceExisting)
    {
        // Reject a missing load list before touching current state.
        if (entries is null)
            return OperationResult<int>.Fail("Loaded flow entries are required.", "LOADED_FLOW_ENTRIES_REQUIRED");

        LogicalFlowRegistryLoadedEntry[] ownedSnapshot;
        try
        {
            // Own the caller sequence once so validation and insertion cannot observe different entries.
            ownedSnapshot = entries.ToArray();
        }
        catch (Exception exception) when (!IsFatalLoadException(exception))
        {
            return OperationResult<int>.Fail(
                "Loaded flow entries could not be materialized.",
                "INVALID_LOADED_FLOW_ENTRY");
        }

        var preparedEntries =
            new (LogicalFlowDescriptor Descriptor, object FlowOrFactory)[ownedSnapshot.Length];
        var acceptedRegistrationOrders = new HashSet<long>();
        HashSet<long>? existingRegistrationOrders = null;
        if (!replaceExisting)
        {
            existingRegistrationOrders = new HashSet<long>(
                _entries.Select(candidate => candidate.Descriptor.RegistrationOrder));
        }

        var acceptedMaximumRegistrationOrder = 0L;

        // Validate and prepare the complete owned snapshot before the first logical mutation.
        for (var index = 0; index < ownedSnapshot.Length; index++)
        {
            var entry = ownedSnapshot[index];
            if (entry?.Descriptor is null || entry.FlowOrFactory is null)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow entries require descriptor and flow or factory.",
                    "INVALID_LOADED_FLOW_ENTRY");
            }

            var descriptor = entry.Descriptor;
            if (!IsCanonicalLoadedIdentity(descriptor.FlowId) ||
                !IsCanonicalLoadedIdentity(descriptor.ScenarioId) ||
                !IsCanonicalLoadedIdentity(descriptor.Version))
            {
                return OperationResult<int>.Fail(
                    "Loaded flow identity fields must be canonical.",
                    "LOADED_FLOW_IDENTITY_INVALID");
            }

            if (!IsCanonicalLoadedRouteMetadata(descriptor.PhaseId) ||
                !IsCanonicalLoadedRouteMetadata(descriptor.SubPhaseId) ||
                !IsCanonicalLoadedRouteMetadata(descriptor.TaskType))
            {
                return OperationResult<int>.Fail(
                    "Loaded flow route metadata must be canonical.",
                    "LOADED_FLOW_ROUTE_METADATA_INVALID");
            }

            if (descriptor.DomainTags is null)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow domain tags must be canonical.",
                    "LOADED_FLOW_DOMAIN_TAGS_INVALID");
            }

            string[] ownedDomainTags;
            try
            {
                // Snapshot the mutable tag array once and validate only the owned copy.
                ownedDomainTags = descriptor.DomainTags.ToArray();
            }
            catch (Exception exception) when (!IsFatalLoadException(exception))
            {
                return OperationResult<int>.Fail(
                    "Loaded flow domain tags could not be materialized.",
                    "LOADED_FLOW_DOMAIN_TAGS_INVALID");
            }

            if (ownedDomainTags.Any(tag => !IsCanonicalLoadedIdentity(tag)))
            {
                return OperationResult<int>.Fail(
                    "Loaded flow domain tags must be canonical.",
                    "LOADED_FLOW_DOMAIN_TAGS_INVALID");
            }

            if (descriptor.RegistrationOrder <= 0 ||
                descriptor.RegistrationOrder == long.MaxValue)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow registration order is invalid.",
                    "LOADED_FLOW_REGISTRATION_ORDER_INVALID");
            }

            if (!acceptedRegistrationOrders.Add(descriptor.RegistrationOrder) ||
                existingRegistrationOrders?.Contains(descriptor.RegistrationOrder) == true)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow registration order conflicts with another registry tuple.",
                    "LOADED_FLOW_REGISTRATION_ORDER_CONFLICT");
            }

            var typeBoundary = ValidateLoadedFlowTypeBoundary(
                descriptor.InputType,
                entry.FlowOrFactory);
            if (!typeBoundary.IsValid)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow type boundary is invalid.",
                    "LOADED_FLOW_TYPE_BOUNDARY_INVALID");
            }

            if (!typeBoundary.IsCompatible)
            {
                return OperationResult<int>.Fail(
                    "Loaded flow does not implement its declared logical-flow or factory interface.",
                    "INCOMPATIBLE_LOADED_FLOW");
            }

            // Clone the descriptor while installing the already-owned, unmodified tag snapshot.
            var preparedDescriptor = descriptor with { DomainTags = ownedDomainTags };
            preparedEntries[index] = (preparedDescriptor, entry.FlowOrFactory);
            acceptedMaximumRegistrationOrder = Math.Max(
                acceptedMaximumRegistrationOrder,
                descriptor.RegistrationOrder);
        }

        if (!replaceExisting &&
            preparedEntries.Length > int.MaxValue - _entries.Count)
        {
            return OperationResult<int>.Fail(
                "Loaded flow entries exceed registry capacity.",
                "LOADED_FLOW_CAPACITY_EXCEEDED");
        }

        // Reserve all required capacity before changing logical registry state.
        var requiredCapacity = replaceExisting
            ? preparedEntries.Length
            : _entries.Count + preparedEntries.Length;
        _entries.EnsureCapacity(requiredCapacity);

        // Replace or append entries in original sequence; do not allocate new registration identities.
        if (replaceExisting)
            _entries.Clear();

        foreach (var preparedEntry in preparedEntries)
            _entries.Add(preparedEntry);

        // Continue future registrations after the largest accepted order without ever decreasing.
        _nextRegistrationOrder = Math.Max(_nextRegistrationOrder, acceptedMaximumRegistrationOrder);

        return OperationResult<int>.Ok(ownedSnapshot.Length);
    }

    // Store one flow object or factory with route metadata.
    private OperationResult<LogicalFlowDescriptor> RegisterFlowObject(
        string flowId,
        string scenarioId,
        string version,
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        Type inputType,
        object flowOrFactory)
    {
        var registration = RegisterFlowObjectCompensatable(
            flowId,
            scenarioId,
            version,
            phaseId,
            subPhaseId,
            taskType,
            domainTags,
            inputType,
            flowOrFactory);
        return registration.IsSuccess
            ? OperationResult<LogicalFlowDescriptor>.Ok(
                registration.Value!.Descriptor)
            : registration.MapError<LogicalFlowDescriptor>();
    }

    private OperationResult<LogicalFlowCompensatableRegistration>
        RegisterFlowObjectCompensatable(
            string flowId,
            string scenarioId,
            string version,
            string phaseId,
            string subPhaseId,
            string taskType,
            string[] domainTags,
            Type inputType,
            object flowOrFactory)
    {
        // Reject empty flow ids because callers cannot select an unnamed topology.
        if (string.IsNullOrWhiteSpace(flowId))
            return OperationResult<LogicalFlowCompensatableRegistration>.Fail("Flow id cannot be empty.");

        // Reject empty scenario ids because scenario selection is the current flow lookup key.
        if (string.IsNullOrWhiteSpace(scenarioId))
            return OperationResult<LogicalFlowCompensatableRegistration>.Fail("Scenario id cannot be empty.");

        // Reject empty versions because future rollback and topology updates need version identity.
        if (string.IsNullOrWhiteSpace(version))
            return OperationResult<LogicalFlowCompensatableRegistration>.Fail("Version cannot be empty.");

        using (_gate.Enter())
        {
            if (_nextRegistrationOrder >= long.MaxValue - 1)
            {
                return OperationResult<LogicalFlowCompensatableRegistration>.Fail(
                    "Logical flow registration order is exhausted.",
                    "LOGICAL_FLOW_REGISTRATION_ORDER_EXHAUSTED");
            }

            var nextRegistrationOrder = _nextRegistrationOrder + 1;
            var storedDomainTags = domainTags?
                .Where(static tag => !string.IsNullOrWhiteSpace(tag))
                .Select(static tag => tag.Trim())
                .ToArray() ?? [];
            _entries.EnsureCapacity(checked(_entries.Count + 1));
            var storedDescriptor = new LogicalFlowDescriptor(
                flowId,
                scenarioId,
                version,
                inputType,
                phaseId?.Trim() ?? "",
                subPhaseId?.Trim() ?? "",
                taskType?.Trim() ?? "",
                storedDomainTags,
                nextRegistrationOrder);
            var returnedDescriptor = CloneDescriptor(storedDescriptor);
            var token = new LogicalFlowRegistrationToken(
                this,
                storedDescriptor,
                flowOrFactory);
            var registration = new LogicalFlowCompensatableRegistration(
                returnedDescriptor,
                token);
            var result = OperationResult<
                LogicalFlowCompensatableRegistration>.Ok(registration);
            BeforeCompensatableMutation.Value?.Invoke();
            _entries.Add((storedDescriptor, flowOrFactory));
            _nextRegistrationOrder = nextRegistrationOrder;
            return result;
        }
    }

    // Clone one descriptor without normalizing, reordering, or deduplicating its domain tags.
    private static LogicalFlowDescriptor CloneDescriptor(LogicalFlowDescriptor descriptor)
    {
        return descriptor with { DomainTags = descriptor.DomainTags.ToArray() };
    }

    // Validate a required persisted identity without trimming or normalizing it.
    private static bool IsCanonicalLoadedIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !StringComparer.Ordinal.Equals(value, value.Trim()) ||
            value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character))
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    // Optional persisted route metadata must be exactly empty or a canonical non-empty identity.
    private static bool IsCanonicalLoadedRouteMetadata(string? value)
    {
        return value is not null &&
               (value.Length == 0 || IsCanonicalLoadedIdentity(value));
    }

    // Construct and test both closed generic type boundaries without leaking malformed Type failures.
    private static (bool IsValid, bool IsCompatible) ValidateLoadedFlowTypeBoundary(
        Type? inputType,
        object flowOrFactory)
    {
        if (inputType is null)
            return (false, false);

        try
        {
            var flowType = typeof(ILogicalFlow<>).MakeGenericType(inputType);
            var factoryType = typeof(ILogicalFlowFactory<>).MakeGenericType(inputType);
            if (flowType.ContainsGenericParameters || factoryType.ContainsGenericParameters)
                return (false, false);

            return (
                true,
                flowType.IsInstanceOfType(flowOrFactory) ||
                factoryType.IsInstanceOfType(flowOrFactory));
        }
        catch (Exception exception) when (!IsFatalLoadException(exception))
        {
            return (false, false);
        }
    }

    // Keep process-corrupting failures observable while sanitizing ordinary hostile load boundaries.
    private static bool IsFatalLoadException(Exception exception)
    {
        return exception is OutOfMemoryException or
                   StackOverflowException or
                   AccessViolationException or
                   AppDomainUnloadedException or
                   BadImageFormatException ||
               StringComparer.Ordinal.Equals(
                   exception.GetType().FullName,
                   "System.Threading.ThreadAbortException");
    }

    // Find a flow entry by flow id and input type.
    private (LogicalFlowDescriptor Descriptor, object? FlowOrFactory) FindByFlowId<TInput>(string flowId)
    {
        // Return newest matching entry.
        using (_gate.Enter())
            return _entries
                .Where(candidate => candidate.Descriptor.FlowId == flowId && candidate.Descriptor.InputType == typeof(TInput))
                .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
                .Select(candidate => (candidate.Descriptor, (object?)candidate.FlowOrFactory))
                .FirstOrDefault();
    }

    // Find a flow entry by scenario id and input type.
    private (LogicalFlowDescriptor Descriptor, object? FlowOrFactory) FindByScenario<TInput>(string scenarioId)
    {
        // Return newest matching entry.
        using (_gate.Enter())
            return _entries
                .Where(candidate => candidate.Descriptor.ScenarioId == scenarioId && candidate.Descriptor.InputType == typeof(TInput))
                .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
                .Select(candidate => (candidate.Descriptor, (object?)candidate.FlowOrFactory))
                .FirstOrDefault();
    }

    // Find a flow entry by sub-phase id and input type.
    private (LogicalFlowDescriptor Descriptor, object? FlowOrFactory) FindBySubPhase<TInput>(string subPhaseId)
    {
        // Return newest matching entry.
        using (_gate.Enter())
            return _entries
                .Where(candidate => candidate.Descriptor.SubPhaseId == subPhaseId && candidate.Descriptor.InputType == typeof(TInput))
                .OrderByDescending(candidate => candidate.Descriptor.RegistrationOrder)
                .Select(candidate => (candidate.Descriptor, (object?)candidate.FlowOrFactory))
                .FirstOrDefault();
    }

    // Validate persisted binding identity without trimming or normalizing it.
    private static bool IsCanonicalBindingIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character))
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    // Resolve one stored flow object or factory into an executable flow.
    private static OperationResult<ILogicalFlow<TInput>> ResolveFlowObject<TInput>(object flowOrFactory, LogicalFlowFactoryRequest? factoryRequest)
    {
        // Direct flow instances can be returned immediately.
        if (flowOrFactory is ILogicalFlow<TInput> flow)
            return OperationResult<ILogicalFlow<TInput>>.Ok(flow);

        // Factories require a factory request.
        if (flowOrFactory is ILogicalFlowFactory<TInput> factory)
        {
            // Refuse to construct a factory without a request because unit selection depends on flags.
            if (factoryRequest is null)
                return OperationResult<ILogicalFlow<TInput>>.Fail("Flow factory request is required.", "FLOW_FACTORY_REQUEST_REQUIRED");

            // Create the flow through the factory.
            return factory.Create(factoryRequest);
        }

        // Anything else is an invalid registry entry.
        return OperationResult<ILogicalFlow<TInput>>.Fail("Registered object is not a compatible logical flow.", "INCOMPATIBLE_FLOW_REGISTRATION");
    }

}
