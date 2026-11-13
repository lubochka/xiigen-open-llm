// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

// This group holds station cards that belong to the same parallel execution wave.
public sealed class WaveGroup
{
    // Create one wave group for one wave index.
    public WaveGroup(int waveIndex)
    {
        // Store the wave index so sequential waves can be sorted visually.
        WaveIndex = waveIndex;
    }

    // WaveIndex identifies which siblings can render side by side.
    public int WaveIndex { get; }

    // Cards contains every station card in this wave.
    public ObservableCollection<StationCardViewModel> Cards { get; } = new();
}

// This view model represents one upserted station card in the trace tree.
public sealed class StationCardViewModel : INotifyPropertyChanged
{
    // The current trace stores the latest start or completion state for the station.
    private StationTrace _trace;

    // The output field is nullable while the station is still running.
    private IReadOnlyList<PropertyNode>? _output;

    // This flag tells the UI whether the station is still in flight.
    private bool _isRunning = true;

    // This flag tells the UI whether the station completed with an error code.
    private bool _hasError;

    // Create a card from the first trace event for one station.
    public StationCardViewModel(StationTrace trace)
    {
        // Store the initial trace so the card can render immediately on start events.
        _trace = trace;

        // Store the initial output when the first event is already a completion event.
        _output = trace.Output;

        // Mark running state from the completion timestamp.
        _isRunning = trace.CompletedAt is null;

        // Mark error state from the error code.
        _hasError = trace.ErrorCode is not null;

        // Raise HasChildren when the first wave group is added so collapsed child panels can open.
        WaveGroups.CollectionChanged += OnWaveGroupsChanged;
    }

    // PropertyChanged notifies XAML bindings and tests when a computed property changes.
    public event PropertyChangedEventHandler? PropertyChanged;

    // WaveGroups groups child station cards by parallel execution wave.
    public ObservableCollection<WaveGroup> WaveGroups { get; } = new();

    // HasChildren controls whether the recursive children panel is visible.
    public bool HasChildren => WaveGroups.Count > 0;

    // Trace exposes the latest trace for tests and detailed inspector binding.
    public StationTrace Trace => _trace;

    // StationName is the display title on the station card.
    public string StationName => _trace.StationName;

    // StationRole is the role subtitle on the station card.
    public string StationRole => _trace.StationRole;

    // Kind selects the card tint and station category.
    public StationKind Kind => _trace.Kind;

    // StationTypeText shows the human station type expected by the inspector.
    public string StationTypeText => _trace.Kind switch
    {
        // Orchestrator cards select or coordinate flows.
        StationKind.Orchestrator => "ORCHESTRATOR",

        // Flow cards run a topology over one request shape.
        StationKind.Flow => "FLOW",

        // Storage cards represent today storage boundary before registry promotion.
        StationKind.Storage => "STORAGE",

        // Arbiter-like unit roles should be visibly different from normal nodes.
        StationKind.Unit when _trace.StationRole.Contains("arbiter", StringComparison.OrdinalIgnoreCase) => "ARBITER",

        // Every other unit station is shown as a node because it performs one transformation.
        StationKind.Unit => "NODE",

        // Unknown station kinds fall back to their enum value.
        _ => _trace.Kind.ToString().ToUpperInvariant()
    };

    // Input exposes the exact station input property tree.
    public IReadOnlyList<PropertyNode> Input => _trace.Input;

    // Output exposes the exact station output property tree after completion.
    public IReadOnlyList<PropertyNode>? Output
    {
        // Return the latest output tree.
        get => _output;

        // Store the latest output tree and notify bindings.
        private set
        {
            // Do nothing when the output reference is unchanged.
            if (ReferenceEquals(_output, value))
                return;

            // Store the new output tree.
            _output = value;

            // Notify bindings that output is now available or changed.
            OnPropertyChanged(nameof(Output));
        }
    }

    // IsRunning tells the UI whether to show the active signal state.
    public bool IsRunning
    {
        // Return the current running state.
        get => _isRunning;

        // Store the new running state and notify bindings.
        private set
        {
            // Do nothing when the running state is unchanged.
            if (_isRunning == value)
                return;

            // Store the new running state.
            _isRunning = value;

            // Notify bindings that the signal state changed.
            OnPropertyChanged(nameof(IsRunning));
        }
    }

    // HasError tells the UI whether to show the error state.
    public bool HasError
    {
        // Return the current error state.
        get => _hasError;

        // Store the new error state and notify bindings.
        private set
        {
            // Do nothing when the error state is unchanged.
            if (_hasError == value)
                return;

            // Store the new error state.
            _hasError = value;

            // Notify bindings that the error state changed.
            OnPropertyChanged(nameof(HasError));
        }
    }

    // DurationText shows a readable duration when the station has completed.
    public string DurationText => _trace.CompletedAt.HasValue
        ? $"{(_trace.CompletedAt.Value - _trace.StartedAt).TotalMilliseconds:F1} ms"
        : "...";

    // ResourceText shows the latest process and PC resource snapshot for this station.
    public string ResourceText => _trace.Resources is { } resources
        ? $"CPU {resources.ProcessCpuPercent:F1}%   RAM {resources.ProcessMemoryMb:F0} MB   PC RAM {resources.SystemMemoryUsedPercent:F0}%"
        : "CPU --   RAM --   PC RAM --";

    // Summary gives the compact first visible value from the output tree.
    public string Summary => _trace.Output is { } output
        ? FindFirstLeaf(output)?.Value ?? string.Empty
        : string.Empty;

    // AddChild attaches a child station under the wave matching the child's WaveIndex.
    public void AddChild(StationCardViewModel child)
    {
        // Find an existing wave group for the child's wave index.
        var group = WaveGroups.FirstOrDefault(candidate => candidate.WaveIndex == child.Trace.WaveIndex);

        // Create the wave group when this is the first child in that wave.
        if (group is null)
        {
            // Build the new wave group.
            group = new WaveGroup(child.Trace.WaveIndex);

            // Find the sorted insert position so sequential waves remain ordered.
            var insertAt = WaveGroups.Count(candidate => candidate.WaveIndex < child.Trace.WaveIndex);

            // Insert the wave group in sorted order.
            WaveGroups.Insert(insertAt, group);
        }

        // Add the child card to the correct wave group.
        group.Cards.Add(child);
    }

    // Update applies a completion event to the existing station card.
    public void Update(StationTrace completed)
    {
        // Replace the stored trace with the latest event for this station.
        _trace = completed;

        // Update the output tree from the completion event.
        Output = completed.Output;

        // Update the running state from the completion timestamp.
        IsRunning = completed.CompletedAt is null;

        // Update the error state from the error code.
        HasError = completed.ErrorCode is not null;

        // Notify bindings whose values are computed from the trace.
        OnPropertyChanged(nameof(Trace));

        // Notify bindings that show the station name.
        OnPropertyChanged(nameof(StationName));

        // Notify bindings that show the station role.
        OnPropertyChanged(nameof(StationRole));

        // Notify bindings that show the station kind.
        OnPropertyChanged(nameof(Kind));

        // Notify bindings that show the human station type.
        OnPropertyChanged(nameof(StationTypeText));

        // Notify bindings that show the input tree.
        OnPropertyChanged(nameof(Input));

        // Notify bindings that show elapsed time.
        OnPropertyChanged(nameof(DurationText));

        // Notify bindings that show CPU and RAM usage.
        OnPropertyChanged(nameof(ResourceText));

        // Notify bindings that show the compact output summary.
        OnPropertyChanged(nameof(Summary));
    }

    // OnWaveGroupsChanged raises HasChildren when the wave group collection changes.
    private void OnWaveGroupsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        // Notify bindings because HasChildren is computed from WaveGroups.Count.
        OnPropertyChanged(nameof(HasChildren));
    }

    // OnPropertyChanged raises one property notification.
    private void OnPropertyChanged(string propertyName)
    {
        // Notify subscribers that the named property changed.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // FindFirstLeaf returns the first scalar leaf in a property tree.
    private static PropertyNode? FindFirstLeaf(IReadOnlyList<PropertyNode> nodes)
    {
        // Examine every node in display order.
        foreach (var node in nodes)
        {
            // Return the node immediately when it has a value.
            if (node.Value is not null)
                return node;

            // Search child nodes recursively.
            var found = FindFirstLeaf(node.Children);

            // Return the first child leaf found.
            if (found is not null)
                return found;
        }

        // Return null when no scalar value exists in this tree.
        return null;
    }
}
