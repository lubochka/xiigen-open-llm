# Orchestrator Inspector — WPF + Helix Toolkit Design Document
# Version: 3.0 — 2026-05-04
# Source: ORCHESTRATOR-INSPECTOR-DESIGN-v3.md merged with v2 implementation detail
# Review fixes applied from the three-layer review of v2
# Replaces: ORCHESTRATOR-INSPECTOR-DESIGN-v2.md

> Superseded note, 2026-05-05: the default inspector topology viewport no longer uses Helix Toolkit or 3D. The performance-first replacement is documented in `ORCHESTRATOR-INSPECTOR-DESIGN-v4-performance-first.md`. Keep this v3 document as historical design context only.

---

## Purpose

This document defines the Windows desktop inspector for the logical orchestrator
runtime in projects 21-23. The tool is not a registry browser. It is a live
execution inspector. It shows what a user request became at every station:

- top-level `ILogicalOrchestrator`
- selected `ILogicalFlow`
- every `ILogicalUnit`
- nested orchestrators, including the future topology-registry orchestrator
- storage results as generated station outputs

The user must be able to click any station and inspect the exact input and exact
output property tree for that station. No summarisation is allowed.

---

## Core design rules

**The topology is generated.** It is not known ahead of time. The inspector must
not ask the topology registry for a domain list or phase list to build the 3D
scene. The registry is expected to become its own `ILogicalOrchestrator`. The
scene can only grow from completed station outputs.

**The trace is a recursive execution tree.** An orchestrator calls a flow. A
flow calls units. A unit can call another orchestrator. The depth is unbounded.
The inspector renders this without assuming a fixed depth.

**Parallel stations share the same visual level.** `WaveIndex` is the runtime
scheduling field that distinguishes parallel work (same wave) from sequential
work (later wave). It is not a topology field. See the StationTrace record.

**Start and complete are two events for the same trace id.** The tree upserts
by `TraceId`. A start event creates the card. A completion event updates the
existing card. It must not add a second card.

---

## Stack

WPF, .NET 8, net8.0-windows. HelixToolkit.Wpf (MIT) for the 3D viewport.
CommunityToolkit.Mvvm (MIT) for observable objects and relay commands.
Project reference to `21.AgentInfrastructure`. Later phases add references to
projects 22 and 23 as their tracing adapters are built.

---

## Aesthetic direction

The aesthetic is an oscilloscope signal monitor. Every decision follows from
that commitment.

- Near-black background throughout (`#080808`).
- Monospace font everywhere — labels, values, ids, durations.
- No gradients, no drop shadows, no decorative shapes.
- No rounded corners on panel borders or structural chrome.
  Four-pixel radius only on individual station cards.
- Green (`#39D077`) means completed successfully.
- Amber (`#E8922A`) means in flight or needs attention.
- Red (`#CC3333`) means failed or rejected.
- The centre trace tree is primary.
- The 3D topology viewport is secondary.
- The right inspector is dense and exact.

---

## Project structure

```
LlmInspector/
  LlmInspector.csproj
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs

  Inspector/
    StationTrace.cs
    StationTraceTree.cs
    InspectorSession.cs
    TopologySceneObserver.cs
    TraceContextScope.cs
    PropertyNodeSerialiser.cs

  ViewModels/
    MainViewModel.cs
    StationCardViewModel.cs
    TopologyNodeViewModel.cs

  Views/
    TopologyViewport.xaml
    TopologyViewport.xaml.cs
    TraceTreePanel.xaml
    TraceTreePanel.xaml.cs
    StationInspector.xaml
    StationInspector.xaml.cs
    StationCard.xaml
    PropertyNodeView.xaml

  Styles/
    Colors.xaml
    Typography.xaml
    Controls.xaml
```

---

## Window layout

Three columns with a `GridSplitter` between each pair. Command bar spans the
full width at the bottom.

Column proportions: 3 (topology) : 5 (trace tree) : 4 (inspector).

The inspector column starts at width zero and opens to `4*` when a card is
clicked. The topology viewport is always visible but does not dominate.

---

## StationTrace record

The same `TraceId` is emitted twice — once at start (output null) and once at
completion (output populated). The tree upserts by `TraceId`.

```csharp
namespace LlmInspector.Inspector;

public sealed record StationTrace(
    string TraceId,
    string? ParentTraceId,    // null only for the root orchestrator
    string RunId,
    int Depth,                // 0 = top-level orchestrator
    int WaveIndex,            // parallel stations share the same WaveIndex
    StationKind Kind,
    string StationName,
    string StationRole,       // unit role or flow id or orchestrator class name
    IReadOnlyList<PropertyNode> Input,
    IReadOnlyList<PropertyNode>? Output,  // null until completion
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt           // null while in flight
);

public enum StationKind
{
    Orchestrator,
    Flow,
    Unit,
    Storage   // current concrete storage layer;
              // becomes Orchestrator when the registry is promoted
}
```

No topology id is present. The topology node id is discovered from the output
when the station completes.

---

## PropertyNode record and serialiser

```csharp
namespace LlmInspector.Inspector;

public sealed record PropertyNode(
    string Name,
    string? Value,
    string? TypeHint,
    IReadOnlyList<PropertyNode> Children);
```

The serialiser uses reflection. Depth cap: six levels.

```csharp
public static class PropertyNodeSerialiser
{
    public static IReadOnlyList<PropertyNode> Serialise(object? obj, int depth = 0)
    {
        if (obj is null)         return [Leaf("value", "(null)", null)];
        if (depth > 6)           return [Leaf("…", "(max depth)", null)];
        if (obj is string s)     return [Leaf("value", s, "string")];
        if (obj is long l)       return [Leaf("value", l.ToString(), "long")];
        if (obj is double d)     return [Leaf("value", d.ToString("F4"), "double")];
        if (obj is bool b)       return [Leaf("value", b.ToString(), "bool")];
        if (obj is DateTimeOffset dt)
                                 return [Leaf("value", dt.ToString("O"), "DateTimeOffset")];
        if (obj is string[] arr)
            return arr.Length == 0
                ? [Leaf("value", "(empty)", "string[]")]
                : arr.Select((v, i) => Leaf($"[{i}]", v, "string")).ToArray();

        var type  = obj.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (props.Length == 0) return [Leaf("value", obj.ToString() ?? "", type.Name)];

        return props.Select(p =>
        {
            var val = p.GetValue(obj);
            if (val is null || val is string || val.GetType().IsPrimitive ||
                val is DateTimeOffset || val is long || val is double)
                return Leaf(p.Name, val?.ToString() ?? "(null)", p.PropertyType.Name);
            return new PropertyNode(p.Name, null, p.PropertyType.Name,
                Serialise(val, depth + 1));
        }).ToArray();
    }

    private static PropertyNode Leaf(string name, string? value, string? hint)
        => new(name, value, hint, []);
}
```

The serialiser handles `OperationResult<T>`, `LogicalUnitRequest<T>`,
`LogicalUnitResult<T>`, `LogicalFlowRequest<T>`, `LogicalFlowResult`,
`LogicalOrchestratorRequest`, `LogicalOrchestratorResult`, and all topology
entry records. No special cases needed — reflection covers them all.

---

## TraceContextScope

```csharp
namespace LlmInspector.Inspector;

internal sealed record TraceContext(
    string RunId,
    string ParentTraceId,
    int Depth,
    int WaveIndex);

internal static class TraceContextScope
{
    private static readonly AsyncLocal<TraceContext?> _current = new();
    public static TraceContext? Current => _current.Value;

    public static IDisposable Push(TraceContext ctx)
    {
        var prior = _current.Value;
        _current.Value = ctx;
        return new Restorer(prior);
    }

    private sealed class Restorer(TraceContext? prior) : IDisposable
    {
        public void Dispose() => _current.Value = prior;
    }
}
```

Every tracing decorator reads `TraceContextScope.Current` to obtain
`RunId`, `ParentTraceId`, `Depth`, and `WaveIndex`. It emits a start trace,
pushes a child context with depth + 1 and its own `TraceId` as the new
parent, delegates to the real station, then emits the completion trace.
The parallel/sequential distinction is expressed through `WaveIndex`: work
dispatched concurrently receives the same wave index; dependent work that
follows receives the next wave index.

---

## InspectorSession

```csharp
namespace LlmInspector.Inspector;

public sealed class InspectorSession
{
    private readonly LogicalOrchestratorAssistant _assistant;
    public event EventHandler<StationTrace>? StationTraced;

    public InspectorSession()
    {
        // Build tracing decorators. No registry reference is stored.
        var tracingRegistry = new TracingStorageDecorator(
            new InMemoryLogicalTopologyRegistry(), EmitTrace);

        var addDomainUnit     = new AddDomainTopologyMutationUnit(tracingRegistry);
        var addPhaseUnit      = new AddPhaseTopologyMutationUnit(tracingRegistry);
        var addSubPhaseUnit   = new AddSubPhaseTopologyMutationUnit(tracingRegistry);
        var addTransitionUnit = new AddTransitionTopologyMutationUnit(tracingRegistry);

        var topologyFlow = new TracingFlowDecorator<LogicalTopologyCommand>(
            new TopologyMutationFlow(
                addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit),
            EmitTrace);

        _assistant = new LogicalOrchestratorAssistant(
            new NullSessionStorage(),
            orchestrator: null,
            topologyMutationFlow: topologyFlow);
    }

    public async Task RunAsync(
        LogicalTopologyCommand command,
        CancellationToken cancellationToken = default)
    {
        var runId    = Guid.NewGuid().ToString("N");
        var traceId  = Guid.NewGuid().ToString("N");

        var start = new StationTrace(traceId, null, runId, 0, 0,
            StationKind.Orchestrator, "LogicalOrchestratorAssistant",
            "assistant", SerialiseCommand(command),
            null, null, null, DateTimeOffset.UtcNow, null);
        EmitTrace(start);

        using (TraceContextScope.Push(new TraceContext(runId, traceId, 1, 0)))
        {
            var result = await _assistant.RunTopologyCommandAsync(
                command, cancellationToken: cancellationToken);
            EmitTrace(start with
            {
                Output       = PropertyNodeSerialiser.Serialise(result),
                CompletedAt  = DateTimeOffset.UtcNow,
                ErrorCode    = result.IsSuccess ? null : result.ErrorCode,
                ErrorMessage = result.IsSuccess ? null : result.ErrorMessage
            });
        }
    }

    private void EmitTrace(StationTrace t) => StationTraced?.Invoke(this, t);

    private static IReadOnlyList<PropertyNode> SerialiseCommand(
        LogicalTopologyCommand cmd) => PropertyNodeSerialiser.Serialise(cmd);
}
```

`TracingStorageDecorator` wraps `InMemoryLogicalTopologyRegistry`.
`TracingFlowDecorator<T>` wraps `TopologyMutationFlow`. Both follow the same
pattern: read current context, emit start, push child context, delegate, emit
completion.

---

## StationTraceTree

```csharp
namespace LlmInspector.Inspector;

public sealed class StationTraceTree
{
    public ObservableCollection<StationCardViewModel> Roots { get; } = new();
    private readonly Dictionary<string, StationCardViewModel> _byTraceId = new();
    private readonly List<StationTrace> _pending = new();

    // Called for both start events (Output null) and completion events.
    public void Upsert(StationTrace trace)
    {
        if (_byTraceId.TryGetValue(trace.TraceId, out var existing))
        {
            // Completion event: update the existing card, do not add a new one.
            existing.Update(trace);
            return;
        }

        // Start event: create the card and attach it to its parent.
        var vm = new StationCardViewModel(trace);
        _byTraceId[trace.TraceId] = vm;

        if (trace.ParentTraceId is null)
        {
            Roots.Add(vm);
        }
        else if (_byTraceId.TryGetValue(trace.ParentTraceId, out var parent))
        {
            parent.AddChild(vm);
        }
        else
        {
            _pending.Add(trace);
        }

        FlushPending();
    }

    private void FlushPending()
    {
        bool flushed;
        do
        {
            flushed = false;
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (_pending[i].ParentTraceId is { } pid &&
                    _byTraceId.TryGetValue(pid, out var parent))
                {
                    var vm = new StationCardViewModel(_pending[i]);
                    _byTraceId[_pending[i].TraceId] = vm;
                    parent.AddChild(vm);
                    _pending.RemoveAt(i);
                    flushed = true;
                }
            }
        } while (flushed);
    }
}
```

---

## StationCardViewModel

Children are grouped by `WaveIndex`. Each wave group is an
`ObservableCollection<StationCardViewModel>`. The XAML binds to `WaveGroups`
which is an `ObservableCollection<WaveGroup>`.

**Review fix applied here:** `HasChildren` raises `PropertyChanged` via
subscription to the `WaveGroups` collection. Without this the children
`ItemsControl` `Visibility` binding would never update from Collapsed to
Visible when the first child arrives.

```csharp
namespace LlmInspector.ViewModels;

public sealed class WaveGroup(int waveIndex)
{
    public int WaveIndex { get; } = waveIndex;
    public ObservableCollection<StationCardViewModel> Cards { get; } = new();
}

public sealed partial class StationCardViewModel : ObservableObject
{
    [ObservableProperty] private bool _isRunning = true;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private IReadOnlyList<PropertyNode>? _output;
    private StationTrace _trace;

    public ObservableCollection<WaveGroup> WaveGroups { get; } = new();
    public bool HasChildren => WaveGroups.Count > 0;

    // Review fix: subscribe to WaveGroups so HasChildren raises PropertyChanged.
    public StationCardViewModel(StationTrace trace)
    {
        _trace = trace;
        WaveGroups.CollectionChanged +=
            (_, _) => OnPropertyChanged(nameof(HasChildren));
    }

    // Add a child into the correct wave group, creating the group if absent.
    public void AddChild(StationCardViewModel child)
    {
        var group = WaveGroups.FirstOrDefault(g => g.WaveIndex == child.Trace.WaveIndex);
        if (group is null)
        {
            group = new WaveGroup(child.Trace.WaveIndex);
            // Insert in WaveIndex order so sequential waves stack correctly.
            int insertAt = WaveGroups.Count(g => g.WaveIndex < child.Trace.WaveIndex);
            WaveGroups.Insert(insertAt, group);
        }
        group.Cards.Add(child);
    }

    public StationTrace Trace => _trace;
    public string StationName => _trace.StationName;
    public string StationRole => _trace.StationRole;
    public StationKind Kind   => _trace.Kind;

    public string DurationText => _trace.CompletedAt.HasValue
        ? $"{(_trace.CompletedAt.Value - _trace.StartedAt).TotalMilliseconds:F1} ms"
        : "…";

    public string Summary =>
        _trace.Output is { } o ? FindFirstLeaf(o)?.Value ?? "" : "";

    public void Update(StationTrace completed)
    {
        _trace  = completed;
        Output  = completed.Output;
        IsRunning = false;
        HasError  = completed.ErrorCode is not null;
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(Summary));
    }

    private static PropertyNode? FindFirstLeaf(IReadOnlyList<PropertyNode> nodes)
    {
        foreach (var n in nodes)
        {
            if (n.Value is not null) return n;
            var found = FindFirstLeaf(n.Children);
            if (found is not null) return found;
        }
        return null;
    }
}
```

---

## Trace tree panel — XAML

```xml
<!-- TraceTreePanel.xaml -->
<ScrollViewer VerticalScrollBarVisibility="Auto"
              HorizontalScrollBarVisibility="Disabled"
              Background="{StaticResource BrushBackground}">
  <ItemsControl ItemsSource="{Binding Tree.Roots}">
    <ItemsControl.ItemTemplate>
      <DataTemplate>
        <local:StationCard/>
      </DataTemplate>
    </ItemsControl.ItemTemplate>
  </ItemsControl>
</ScrollViewer>
```

```xml
<!-- StationCard.xaml -->
<StackPanel>

  <!-- The card -->
  <Border Margin="2,2,2,0" Padding="8,5"
          Background="{Binding KindBackground}"
          BorderBrush="{Binding KindBorder}" BorderThickness="1"
          CornerRadius="4" Cursor="Hand"
          MouseLeftButtonDown="Card_Clicked">
    <Grid>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
      </Grid.ColumnDefinitions>
      <Ellipse Grid.Column="0" Width="7" Height="7"
               Fill="{Binding SignalBrush}" Margin="0,0,7,0"/>
      <StackPanel Grid.Column="1">
        <TextBlock Text="{Binding StationName}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="11" Foreground="{StaticResource BrushTextPrimary}"/>
        <TextBlock Text="{Binding Summary}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="10" Foreground="{StaticResource BrushTextSecondary}"/>
      </StackPanel>
      <TextBlock Grid.Column="2" Text="{Binding DurationText}"
                 FontFamily="{StaticResource FontMono}"
                 FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
                 VerticalAlignment="Center" Margin="8,0,0,0"/>
    </Grid>
  </Border>

  <!-- Children grouped by WaveIndex.
       Each wave group is a horizontal WrapPanel — parallel stations side by side.
       Wave groups stack vertically — sequential dependency order visible. -->
  <ItemsControl ItemsSource="{Binding WaveGroups}" Margin="16,0,0,0"
                Visibility="{Binding HasChildren,
                             Converter={StaticResource BoolToVisible}}">
    <ItemsControl.ItemTemplate>
      <DataTemplate>
        <!-- One wave: horizontal row of parallel cards -->
        <ItemsControl ItemsSource="{Binding Cards}">
          <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
              <WrapPanel Orientation="Horizontal"/>
            </ItemsPanelTemplate>
          </ItemsControl.ItemsPanel>
          <ItemsControl.ItemTemplate>
            <DataTemplate>
              <local:StationCard/>  <!-- recursive -->
            </DataTemplate>
          </ItemsControl.ItemTemplate>
        </ItemsControl>
      </DataTemplate>
    </ItemsControl.ItemTemplate>
  </ItemsControl>

</StackPanel>
```

This produces the correct layout without special cases. Parallel stations at the
same `WaveIndex` share a `WrapPanel` row. Sequential waves stack in separate
rows below. The recursion handles any depth.

---

## TopologySceneObserver

The observer holds no registry reference. It reacts to completed traces only.

The `TryFind...` helpers search the full property tree recursively — not just
the root output object. This matters because the entry may be nested inside
`LogicalUnitResult<T>.Output` or inside a nested orchestrator result.

```csharp
namespace LlmInspector.Inspector;

public sealed class TopologySceneObserver
{
    private readonly Model3DGroup _scene;
    private readonly Dictionary<string, Point3D> _nodePositions = new();
    private readonly Dictionary<string, GeometryModel3D> _nodeGeometry = new();
    private readonly Dictionary<string, GeometryModel3D> _domainEdges = new();
    private readonly List<LogicalPhaseEntry> _pendingPhases = new();
    private readonly List<LogicalSubPhaseEntry> _pendingSubPhases = new();
    private readonly List<LogicalTransitionEntry> _pendingTransitions = new();

    private static readonly Point3D OrchestratorPos = new(0, 4, 0);
    private static readonly Point3D FlowPos         = new(0, 2.5, 0);
    private static readonly Point3D UnitPos         = new(0, 1, 0);

    public TopologySceneObserver(Model3DGroup scene)
    {
        _scene = scene;
        AddProcessingNodes();
    }

    public void OnTraceCompleted(StationTrace trace)
    {
        if (trace.Output is null) return;

        if (TryFindDomainEntry(trace.Output, out var domain))
        {
            AddDomain(domain);
            FlushPending();
            return;
        }
        if (TryFindPhaseEntry(trace.Output, out var phase))
        {
            if (!TryAddPhase(phase)) _pendingPhases.Add(phase);
            FlushPending();
            return;
        }
        if (TryFindSubPhaseEntry(trace.Output, out var subPhase))
        {
            if (!TryAddSubPhase(subPhase)) _pendingSubPhases.Add(subPhase);
            FlushPending();
            return;
        }
        if (TryFindTransitionEntry(trace.Output, out var transition))
        {
            if (!TryAddTransition(transition)) _pendingTransitions.Add(transition);
        }
    }

    private void AddDomain(LogicalDomainEntry entry)
    {
        int existingCount = _nodePositions.Keys.Count(k => k.StartsWith("domain:"));
        int newTotal = existingCount + 1;

        // Review fix: rebalance the ring by removing and re-adding all domain
        // geometry when a new domain arrives, then add the new one.
        RebuildDomainRing(newTotal);

        var pos = DomainRingPosition(existingCount, newTotal);
        var id  = "domain:" + entry.Domain.DomainId;
        _nodePositions[id] = pos;
        var geo = BuildSphere(pos, 0.5, DomainIdleMaterial);
        _nodeGeometry[id] = geo;
        _scene.Children.Add(geo);
        var edge = BuildEdge(UnitPos, pos, 0.03);
        _domainEdges[id] = edge;
        _scene.Children.Add(edge);
    }

    // Rebuilds all existing domain sphere positions and replaces their geometry.
    // Called whenever a new domain is added so the ring stays evenly spaced.
    private void RebuildDomainRing(int newTotal)
    {
        var domainKeys = _nodePositions.Keys.Where(k => k.StartsWith("domain:")).ToList();
        for (int i = 0; i < domainKeys.Count; i++)
        {
            var key    = domainKeys[i];
            var newPos = DomainRingPosition(i, newTotal);
            _nodePositions[key] = newPos;

            // Replace geometry: remove the old sphere, add a new one at the new position.
            if (_nodeGeometry.TryGetValue(key, out var old))
                _scene.Children.Remove(old);
            var geo = BuildSphere(newPos, 0.5, DomainIdleMaterial);
            _nodeGeometry[key] = geo;
            _scene.Children.Add(geo);
        }

        // Review fix continued: rebuild the UnitPos-to-domain edge for every
        // repositioned domain in the same pass so the edge does not point to
        // the old sphere position.
        for (int i = 0; i < domainKeys.Count; i++)
        {
            var key = domainKeys[i];
            var pos = _nodePositions[key];

            if (_domainEdges.TryGetValue(key, out var oldEdge))
                _scene.Children.Remove(oldEdge);

            var newEdge = BuildEdge(UnitPos, pos, 0.03);
            _domainEdges[key] = newEdge;
            _scene.Children.Add(newEdge);
        }
    }

    private bool TryAddPhase(LogicalPhaseEntry entry)
    {
        var parentKey = "domain:" + (entry.Phase.DomainIds.FirstOrDefault() ?? "");
        if (!_nodePositions.TryGetValue(parentKey, out var parentPos)) return false;

        int siblings = _nodePositions.Keys.Count(k => k.StartsWith("phase:") &&
            IsNear(_nodePositions[k], parentPos, 2.0));
        var pos = ChildArcPosition(parentPos, siblings, siblings + 1, 1.2, -1.5);
        // Note: the first child always lands at angle 0 relative to the parent.
        // Subsequent children spread around it. This is asymmetric but produces
        // no collisions at radius 1.2 with phase sphere radius 0.32. Named here
        // as a known behavior, not a defect.

        var id = "phase:" + entry.Phase.PhaseId;
        _nodePositions[id] = pos;
        var geo = BuildSphere(pos, 0.32, PhaseIdleMaterial);
        _nodeGeometry[id] = geo;
        _scene.Children.Add(geo);
        _scene.Children.Add(BuildEdge(parentPos, pos, 0.025));
        return true;
    }

    private bool TryAddSubPhase(LogicalSubPhaseEntry entry)
    {
        var parentKey = "phase:" + entry.SubPhase.PhaseId;
        if (!_nodePositions.TryGetValue(parentKey, out var parentPos)) return false;

        int siblings = _nodePositions.Keys.Count(k => k.StartsWith("subphase:") &&
            IsNear(_nodePositions[k], parentPos, 1.2));
        var pos = ChildArcPosition(parentPos, siblings, siblings + 1, 0.8, -1.5);

        var id = "subphase:" + entry.SubPhase.SubPhaseId;
        _nodePositions[id] = pos;
        var geo = BuildSphere(pos, 0.18, SubPhaseIdleMaterial);
        _nodeGeometry[id] = geo;
        _scene.Children.Add(geo);
        _scene.Children.Add(BuildEdge(parentPos, pos, 0.02));
        return true;
    }

    private bool TryAddTransition(LogicalTransitionEntry entry)
    {
        var fromKey = entry.Transition.FromTopologyKind.ToLowerInvariant()
                    + ":" + entry.Transition.FromTopologyId;
        var toKey   = entry.Transition.ToTopologyKind.ToLowerInvariant()
                    + ":" + entry.Transition.ToTopologyId;
        if (!_nodePositions.TryGetValue(fromKey, out var from)) return false;
        if (!_nodePositions.TryGetValue(toKey,   out var to))   return false;
        _scene.Children.Add(BuildEdge(from, to, 0.06, TransitionMaterial));
        return true;
    }

    private void FlushPending()
    {
        bool flushed;
        do
        {
            flushed = false;
            for (int i = _pendingPhases.Count - 1; i >= 0; i--)
                if (TryAddPhase(_pendingPhases[i])) { _pendingPhases.RemoveAt(i); flushed = true; }
            for (int i = _pendingSubPhases.Count - 1; i >= 0; i--)
                if (TryAddSubPhase(_pendingSubPhases[i])) { _pendingSubPhases.RemoveAt(i); flushed = true; }
            for (int i = _pendingTransitions.Count - 1; i >= 0; i--)
                if (TryAddTransition(_pendingTransitions[i])) { _pendingTransitions.RemoveAt(i); flushed = true; }
        } while (flushed);
    }

    // Position n evenly-spaced items in an arc around a parent.
    private static Point3D ChildArcPosition(
        Point3D parent, int index, int total, double radius, double yOffset)
    {
        double angle = 2 * Math.PI * index / Math.Max(1, total);
        return new Point3D(
            parent.X + radius * Math.Cos(angle),
            parent.Y + yOffset,
            parent.Z + radius * Math.Sin(angle));
    }

    private static Point3D DomainRingPosition(int index, int total)
    {
        double angle = 2 * Math.PI * index / Math.Max(1, total);
        return new Point3D(4 * Math.Cos(angle), 0, 4 * Math.Sin(angle));
    }

    // TryFind helpers search the full property tree, not just the root.
    // A domain entry may be inside LogicalUnitResult<T>.Output or similar.
    private static bool TryFindDomainEntry(
        IReadOnlyList<PropertyNode> nodes, out LogicalDomainEntry entry)
    {
        entry = default!;
        // Look for a node named "Domain" with a child named "DomainId".
        foreach (var node in nodes)
        {
            if (node.Name == "Domain")
            {
                var domainId = node.Children.FirstOrDefault(c => c.Name == "DomainId")?.Value;
                if (domainId is not null)
                {
                    var name    = node.Children.FirstOrDefault(c => c.Name == "Name")?.Value ?? "";
                    var purpose = node.Children.FirstOrDefault(c => c.Name == "Purpose")?.Value ?? "";
                    entry = new LogicalDomainEntry(
                        new LogicalDomain(domainId, name, purpose, [], []),
                        0, DateTimeOffset.UtcNow);
                    return true;
                }
            }
            // Recurse into children.
            if (TryFindDomainEntry(node.Children, out entry)) return true;
        }
        return false;
    }
    // TryFindPhaseEntry, TryFindSubPhaseEntry, TryFindTransitionEntry
    // follow the same recursive pattern.

    private static bool IsNear(Point3D a, Point3D b, double threshold) =>
        (a - b).Length < threshold;
}
```

---

## Station inspector panel

```xml
<!-- StationInspector.xaml -->
<ScrollViewer VerticalScrollBarVisibility="Auto"
              Background="{StaticResource BrushPanel}">
  <StackPanel Margin="12">

    <TextBlock Text="{Binding StationName}"
               FontFamily="{StaticResource FontMono}"
               FontSize="13" FontWeight="Bold"
               Foreground="{StaticResource BrushTextPrimary}"
               Margin="0,0,0,2"/>
    <TextBlock Text="{Binding StationRole}"
               FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
               Margin="0,0,0,4"/>
    <TextBlock Text="{Binding KindLabel}"
               FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
               Margin="0,0,0,12"/>

    <Border Background="#140000" BorderBrush="#440000" BorderThickness="1"
            Padding="8" Margin="0,0,0,12"
            Visibility="{Binding HasError, Converter={StaticResource BoolToVisible}}">
      <StackPanel>
        <TextBlock Text="{Binding ErrorCode}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="11" FontWeight="Bold" Foreground="#CC3333"/>
        <TextBlock Text="{Binding ErrorMessage}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="11" Foreground="#994444" TextWrapping="Wrap"/>
      </StackPanel>
    </Border>

    <TextBlock Text="INPUT" FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
               Margin="0,0,0,6"/>
    <ItemsControl ItemsSource="{Binding Input}">
      <ItemsControl.ItemTemplate>
        <DataTemplate><local:PropertyNodeView/></DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>

    <Separator Margin="0,12" Background="{StaticResource BrushBorder}"/>

    <TextBlock Text="OUTPUT" FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
               Margin="0,0,0,6"/>
    <ItemsControl ItemsSource="{Binding Output}">
      <ItemsControl.ItemTemplate>
        <DataTemplate><local:PropertyNodeView/></DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>

  </StackPanel>
</ScrollViewer>
```

```xml
<!-- PropertyNodeView.xaml — recursive -->
<StackPanel Margin="0,1">
  <Grid Visibility="{Binding HasNoChildren, Converter={StaticResource BoolToVisible}}">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="120" MaxWidth="160"/>
      <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <TextBlock Text="{Binding Name}"
               FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextSecondary}"/>
    <TextBlock Grid.Column="1" Text="{Binding Value}"
               FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextPrimary}"
               TextWrapping="Wrap"/>
  </Grid>
  <Expander IsExpanded="True"
            Visibility="{Binding HasChildren, Converter={StaticResource BoolToVisible}}">
    <Expander.Header>
      <StackPanel Orientation="Horizontal">
        <TextBlock Text="{Binding Name}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="10" Foreground="{StaticResource BrushAccent}"/>
        <TextBlock Text="{Binding TypeHint}"
                   FontFamily="{StaticResource FontMono}"
                   FontSize="9" Foreground="{StaticResource BrushTextTertiary}"
                   Margin="6,0,0,0"/>
      </StackPanel>
    </Expander.Header>
    <ItemsControl ItemsSource="{Binding Children}" Margin="12,0,0,0">
      <ItemsControl.ItemTemplate>
        <DataTemplate><local:PropertyNodeView/></DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </Expander>
</StackPanel>
```

---

## 3D viewport setup

```xml
<!-- TopologyViewport.xaml -->
<hx:HelixViewport3D Background="#080808"
                     ShowCoordinateSystem="False"
                     ShowFrameRate="False"
                     IsHeadLightEnabled="False"
                     CameraRotationMode="Trackball">
  <hx:HelixViewport3D.Children>
    <AmbientLight Color="#1A1A16"/>
    <DirectionalLight Color="#204A30" Direction="-1,-1,-0.5"/>
    <DirectionalLight Color="#0A1820" Direction="1,0.3,0.3"/>
  </hx:HelixViewport3D.Children>
  <hx:GridLinesVisual3D Width="20" Length="20"
                         MinorDistance="1" MajorDistance="5"
                         Thickness="0.008" Fill="#1A1A14"/>
  <ModelVisual3D x:Name="SceneRoot"/>
</hx:HelixViewport3D>
```

Node materials use `EmissiveMaterial` so nodes glow against the dark background
independent of light direction.

| Node type | Idle hex | Active hex | Shape | Size |
|---|---|---|---|---|
| Orchestrator | `#2A1A00` | `#E8922A` | Box | 0.55 |
| Flow | `#001A08` | `#39D077` | Box | 0.38 |
| Unit | `#00081A` | `#5599FF` | Box | 0.26 |
| Domain | `#1A2A3A` | `#5599FF` | Sphere | 0.50 |
| Phase | `#1A3A2A` | `#39D077` | Sphere | 0.32 |
| Sub-phase | `#2A1A3A` | `#AA77FF` | Sphere | 0.18 |
| Transition | `#2A2200` | `#E8922A` | Cylinder | r=0.05 |

---

## Color resources

```xml
<!-- Colors.xaml -->
<ResourceDictionary>
  <SolidColorBrush x:Key="BrushBackground"  Color="#080808"/>
  <SolidColorBrush x:Key="BrushSurface"     Color="#0E0E0E"/>
  <SolidColorBrush x:Key="BrushPanel"       Color="#111111"/>
  <SolidColorBrush x:Key="BrushBorder"      Color="#222222"/>

  <SolidColorBrush x:Key="BrushTextPrimary"   Color="#C8C8B4"/>
  <SolidColorBrush x:Key="BrushTextSecondary" Color="#7A7A66"/>
  <SolidColorBrush x:Key="BrushTextTertiary"  Color="#444438"/>
  <SolidColorBrush x:Key="BrushAccent"        Color="#39D077"/>

  <SolidColorBrush x:Key="BrushPhosphorGreen" Color="#39D077"/>
  <SolidColorBrush x:Key="BrushAmber"         Color="#E8922A"/>
  <SolidColorBrush x:Key="BrushErrorRed"      Color="#CC3333"/>

  <SolidColorBrush x:Key="BrushOrchestratorTint" Color="#140A00"/>
  <SolidColorBrush x:Key="BrushFlowTint"         Color="#000A06"/>
  <SolidColorBrush x:Key="BrushUnitTint"         Color="#00060E"/>
  <SolidColorBrush x:Key="BrushStorageTint"      Color="#0A0A04"/>

  <SolidColorBrush x:Key="BrushOrchestratorBorder" Color="#3A2000"/>
  <SolidColorBrush x:Key="BrushFlowBorder"         Color="#003A18"/>
  <SolidColorBrush x:Key="BrushUnitBorder"         Color="#001830"/>
  <SolidColorBrush x:Key="BrushStorageBorder"      Color="#2A2800"/>
</ResourceDictionary>
```

```xml
<!-- Typography.xaml -->
<ResourceDictionary>
  <FontFamily x:Key="FontMono">Cascadia Code, Consolas, Courier New</FontFamily>
</ResourceDictionary>
```

---

## Command bar

Phase 1 through Phase 12 support typed topology commands: add domain, add phase,
add sub-phase, add transition.

Later phases add natural-language mutation requests, route classification
requests, KNN skill binding requests, internal-first assistant requests, and
OSS-equivalent state-machine requests.

The command bar shows the active flags for every run. It must reflect the real
`LogicalExecutionFlags` surface, not a shortened UI-only subset.

Default-on optimization flags:

```
UseCompiledPredicateRepresentation
UseDomainScopedRetrieval
UseConfidenceFloor
UseLearnedUnitWhenAvailable
UseTraceEvidence
UseContextCarryForward
UseIdleLearningScheduling
```

Default-on safety flags:

```
UseArbiterAuthority
UseKnowledgeEdgeDetection
UseInternalFirstExecution
RequireExternalAiApproval
RejectArbiterContradictions
```

Experiment-control flags:

```
UseDeterministicFixtureBaseline
WriteEvidenceArtifacts
CompareAgainstOracle
FailOnMetricDrift
```

---

## What must be inspectable per project

**Project 21:** `LogicalOrchestratorAssistant`, topology command routing,
topology mutation flow invocation, permission gate for external AI approval,
learning event scheduling path.

**Project 22:** route classification, KNN skill filtering, internal LLM gateway
routing, internal result examination, external AI request package construction,
external result adaptation.

**Project 23:** OSS-equivalent state machine transitions, approval-request
guard, external AI state guard, result acceptance and rejection paths.

Each of these becomes a station trace when it is wrapped by a tracing adapter or
promoted behind `ILogicalUnit`, `ILogicalFlow`, or `ILogicalOrchestrator`.

---

## Acceptance gates

Gate 1: the inspector shows root orchestrator → mutation flow → mutation unit
→ storage in the recursive tree.

Gate 2: clicking each card shows exact input and exact output fields.

Gate 3: adding a domain creates a 3D sphere from the completed output, not from
a registry list call.

Gate 4: adding a second domain repositions the first and places the second
correctly in the rebalanced ring. Both spheres are visible without overlap.

Gate 5: adding a phase creates a child sphere under its domain after the phase
output is observed.

Gate 6: adding a transition creates an edge only after both endpoint outputs
exist.

Gate 7: two parallel child stations render side by side in the same wave row.

Gate 8: a nested topology-registry orchestrator renders as an orchestrator
subtree with its own child flow and unit cards, not as a storage leaf.

Gate 9: no component in the UI layer holds an `ILogicalTopologyRegistry`
reference.

---

## Implementation phases

Each phase touches one unit presence or one UI panel. Gate B for every phase is
`dotnet build LlmInspector.sln` with zero errors, plus focused test run if
tests are added.

**Phase 1 — Shell.** WPF project, three-column `Grid`, `GridSplitter`, command
bar placeholder. Colors.xaml and Typography.xaml loaded.
Gate B: blank near-black window opens.

**Phase 2 — Data layer.** `PropertyNode`, `StationTrace`, `StationKind`,
`PropertyNodeSerialiser`. Unit tests serialise a real `LogicalDomainEntry`,
`LogicalPhaseEntry`, `LogicalTransitionEntry` and assert field names and values.
Gate B: serialiser tests pass.

**Phase 3 — TraceContextScope.** `AsyncLocal<TraceContext>` with `Push` and
`Current`. Unit test: context pushed in one async branch is invisible in a
sibling branch started before the push.
Gate B: isolation test passes.

**Phase 4 — Tracing decorators.** `TracingStorageDecorator` wrapping
`InMemoryLogicalTopologyRegistry`. `TracingFlowDecorator<T>` wrapping
`TopologyMutationFlow`. `InspectorSession` wiring. Integration test: fire one
`AddDomainCommand`, capture all emitted `StationTrace` events, assert four
traces with correct `Depth`, `WaveIndex`, and `ParentTraceId` chain
(0 orchestrator → 1 flow → 2 unit → 3 storage).
Gate B: integration test passes.

**Phase 5 — StationTraceTree and StationCardViewModel.** `StationTraceTree`
with upsert behavior. `StationCardViewModel` with `WaveGroups` collection.
`WaveGroups.CollectionChanged` subscription raising `PropertyChanged` for
`HasChildren` is required here.
Unit tests: (a) feed four traces from Phase 4, assert `Roots.Count == 1` with
correct depth chain; (b) add a child to a root card, assert `HasChildren`
becomes true and fires `PropertyChanged`.
Gate B: both tests pass.

**Phase 6 — Trace tree panel.** `TraceTreePanel.xaml` and `StationCard.xaml`
with recursive `WaveGroups` rendering. Fire one `AddDomainCommand` and observe
the four-level card tree rendered with correct station names, roles, and
durations.
Gate B: four-level tree visible in the centre column.

**Phase 7 — Parallel wave rendering.** Modify `InspectorSession` to dispatch
two unit commands with the same `WaveIndex`. Assert that both unit cards appear
side by side in the same `WrapPanel` row.
Gate B: two unit cards side by side in the same wave row.

**Phase 8 — Station inspector.** `StationInspector.xaml` and
`PropertyNodeView.xaml` recursive expander. Inspector column animates from zero
to `4*` when a card is clicked, returns to zero when closed.
Gate B: click a card, inspector opens with full input and output property trees.

**Phase 9 — 3D viewport and domain node.** `TopologyViewport.xaml` with
`HelixViewport3D`, reference grid, and processing node boxes.
`TopologySceneObserver` reacting to domain output. `RebuildDomainRing`
implemented fully — not deferred to a helper.
Gate B: fire two `AddDomainCommand` runs. First domain sphere appears after the
first run. Ring rebalances after the second run — both spheres visible and
correctly spaced with no overlap.
(Two domains required here to surface the ring-rebuild behavior.)

**Phase 10 — Phase and sub-phase nodes.** Extend `TopologySceneObserver` for
`LogicalPhaseEntry` and `LogicalSubPhaseEntry`. Child spheres appear below
their parent at insertion time. Asymmetric arc distribution noted but confirmed
collision-free at the design radii.
Gate B: fire `AddPhaseCommand` after `AddDomainCommand`, phase sphere appears
as a child of the domain sphere.

**Phase 11 — Pending-child and pending-transition buffering.** `FlushPending`
activated. Test: emit a phase trace before its domain trace. Assert the phase
sphere appears after the domain trace is processed.
Gate B: out-of-order arrival test passes. Transition buffering: emit a
transition trace before its target domain exists. Assert the edge appears after
the target domain trace is processed.

**Phase 12 — Command bar.** `ComboBox` for command type, structured form fields
per command, `Run ▶` button, applied flags display.
Gate B: full round trip — fill the form, click Run, four-level trace tree
appears, domain sphere appears in 3D scene, inspector opens on card click.

**Phase 13 — Project 21 traces beyond topology mutation.** Tracing adapters for
permission gate, learning event scheduler.

**Phase 14 — Project 22 traces.** Tracing adapters for route classifier, KNN
skill filter, internal LLM gateway, internal result examiner, external AI
request package, external result adapter.

**Phase 15 — Project 23 traces.** Tracing adapters for OSS-equivalent state
machine, external AI state guard.

**Phase 16 — Nested registry orchestrator.** When the topology registry is
promoted to `ILogicalOrchestrator`, its storage leaf in the trace tree is
replaced by an orchestrator subtree with child flow and unit cards.
Gate B: Gate 8 of the acceptance gates passes — the registry appears as an
orchestrator subtree, not a storage leaf.
