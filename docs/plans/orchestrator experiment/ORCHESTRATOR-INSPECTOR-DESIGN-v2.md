# Orchestrator Inspector — WPF + Helix Toolkit Design Document
Version: 2.0 — 2026-05-04
Replaces: ORCHESTRATOR-INSPECTOR-DESIGN-v1.md

---

## Corrections from v1.0

Three design mistakes are corrected in this version.

**Correction 1 — Topology ids are outputs, not inputs.**
v1.0 included a `TopologyNodeId` field on `StationTrace`. This was wrong. The topology node id exists in the station's output — in the `DomainId` of a `LogicalDomainEntry`, the `PhaseId` of a `LogicalPhaseEntry`, and so on. The scene observer reads the output after a station completes and adds the node at that moment. The id is discovered, not declared.

**Correction 2 — The registry becomes an ILogicalOrchestrator, not an ILogicalUnit.**
v1.0 exposed the inner registry directly through `InspectorSession.Registry`. The registry will eventually become a full `ILogicalOrchestrator` with its own flow selection, mutation flows, and unit cells. No inspector component should ever hold a direct registry reference. The inspector reads registry state exclusively from trace outputs.

**Correction 3 — Parallel flows render at the same level, not in a list.**
v1.0 used a `ListView` for the signal trace panel. When two flows run in parallel they share the same parent and the same depth. The panel is a live recursive tree. Parallel stations at the same depth sit side by side in a horizontal `WrapPanel` row. The tree grows downward as stations complete, exactly as HTML data binding adds children to a parent element when data arrives. No pre-built structure, no replay animation.

---

## What the tool does

A developer types a topology command into the command bar and fires it through `LogicalOrchestratorAssistant`. Three things happen simultaneously.

The **trace tree** (centre column, primary) grows downward in real time as stations complete. Each completed station appends a card as a child of its parent station. Parallel stations at the same depth sit side by side on the same level row. Clicking any card opens its full input and output in the inspector panel on the right.

The **3D topology scene** (left column, secondary) grows as registry outputs arrive. Each time a station completes with a registry entry in its output — a domain, a phase, a sub-phase, a transition — the corresponding geometry is added to the Helix Toolkit scene as a child of its parent node. The scene is never pre-built from a registry read. It is generated from trace outputs.

The **station inspector** (right column, on demand) shows the full property tree for the selected station: every field of the input and every field of the output, recursively expanded. It is hidden until a card is clicked.

---

## Aesthetic direction

The aesthetic is an oscilloscope signal monitor. The reference object is measurement equipment, not a productivity application.

- Near-black background (`#080808`) with no decorative elements.
- Active stations use phosphor green (`#39D077`). In-flight stations pulse amber (`#E8922A`). Failed stations use red (`#CC3333`).
- Station kind tints are very dark: near-black with a barely visible colour cast per kind.
- Monospace font everywhere — labels, values, ids, durations.
- No rounded corners on panel borders or structural chrome. Four-pixel radius only on individual station cards to distinguish them from structure.
- No drop shadows. No gradients. No decorative elements.
- The Helix Toolkit grid plane uses 1px lines at 15% opacity to evoke an oscilloscope reference grid.

---

## Project structure

```
LlmInspector/
  LlmInspector.csproj           -- WPF net8.0-windows, HelixToolkit.Wpf, CommunityToolkit.Mvvm
  App.xaml
  App.xaml.cs
  MainWindow.xaml               -- three-column Grid plus command bar at the bottom
  MainWindow.xaml.cs

  Inspector/
    StationTrace.cs             -- one record per station event
    StationTraceTree.cs         -- assembles parent-child tree from flat trace stream
    InspectorSession.cs         -- wraps LogicalOrchestratorAssistant, emits StationTrace events
    TopologySceneObserver.cs    -- reacts to completed traces, adds geometry to the 3D scene
    TraceContextScope.cs        -- AsyncLocal ambient context for depth and parent id
    PropertyNodeSerialiser.cs   -- converts any C# object to a PropertyNode tree via reflection

  ViewModels/
    MainViewModel.cs
    StationCardViewModel.cs     -- one per node in the trace tree
    TopologyNodeViewModel.cs    -- one per node in the 3D scene

  Views/
    TopologyViewport.xaml       -- left: HelixViewport3D, grows from trace outputs
    TopologyViewport.xaml.cs
    TraceTreePanel.xaml         -- centre: live recursive tree
    TraceTreePanel.xaml.cs
    StationInspector.xaml       -- right: property tree for selected station
    StationInspector.xaml.cs
    StationCard.xaml            -- one card in the trace tree, recursive
    PropertyNodeView.xaml       -- one node in the property tree, recursive

  Styles/
    Colors.xaml
    Typography.xaml
    Controls.xaml
```

NuGet packages:
- `HelixToolkit.Wpf` 4.x (MIT)
- `CommunityToolkit.Mvvm` 8.x (MIT)

Project reference: `21.AgentInfrastructure`.

---

## Window layout

Three columns with a `GridSplitter` between each pair. Command bar spans the full width at the bottom.

Column proportions: 3 (topology) : 5 (trace tree) : 4 (inspector).

The trace tree is the primary view and receives the most horizontal space. The inspector column starts at width zero and opens to `4*` when a card is clicked. The topology viewport is always visible but secondary.

---

## The StationTrace record

`TopologyNodeId` is removed entirely. No field on `StationTrace` refers to topology state ahead of time.

```csharp
namespace LlmInspector.Inspector;

public sealed record StationTrace(
    string TraceId,
    string? ParentTraceId,      // null only for the root orchestrator
    string RunId,
    int Depth,                  // 0 = top-level orchestrator
    string StationName,
    StationKind Kind,
    IReadOnlyList<PropertyNode> Input,
    IReadOnlyList<PropertyNode>? Output,  // null until the station completes
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
    Registry   // current kind; becomes Orchestrator when the registry evolves
}
```

---

## TraceContextScope

An `AsyncLocal<TraceContext>` that flows through the async execution chain. Each tracing decorator pushes a new context with incremented depth and its own trace id as the parent before calling the wrapped method, and restores the prior context on exit. This is how child traces know their depth and parent id without any decorator being explicitly passed those values.

```csharp
namespace LlmInspector.Inspector;

internal sealed record TraceContext(
    string RunId,
    string ParentTraceId,
    int Depth);

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

---

## InspectorSession

Does not hold a registry reference. Emits `StationTrace` events only. Tracing decorators wrap the flow and the current registry layer. When the registry becomes an `ILogicalOrchestrator`, its decorator follows the same pattern — wrap the orchestrator, push a new `TraceContext`, emit traces before and after each nested call.

```csharp
namespace LlmInspector.Inspector;

public sealed class InspectorSession
{
    private readonly LogicalOrchestratorAssistant _assistant;
    public event EventHandler<StationTrace>? StationTraced;

    public InspectorSession()
    {
        var tracingRegistry = new TracingRegistryDecorator(
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
        var runId = Guid.NewGuid().ToString("N");
        var rootTrace = BuildStartTrace(runId, null, 0,
            "LogicalOrchestratorAssistant", StationKind.Orchestrator, command);
        EmitTrace(rootTrace);

        using (TraceContextScope.Push(
            new TraceContext(runId, rootTrace.TraceId, 1)))
        {
            var result = await _assistant.RunTopologyCommandAsync(
                command, cancellationToken: cancellationToken);

            EmitTrace(rootTrace with
            {
                Output = PropertyNodeSerialiser.Serialise(result),
                CompletedAt = DateTimeOffset.UtcNow,
                ErrorCode   = result.IsSuccess ? null : result.ErrorCode,
                ErrorMessage = result.IsSuccess ? null : result.ErrorMessage
            });
        }
    }

    private void EmitTrace(StationTrace t) => StationTraced?.Invoke(this, t);
}
```

`TracingRegistryDecorator` and `TracingFlowDecorator<T>` each read `TraceContextScope.Current` to obtain `RunId`, `ParentTraceId`, and `Depth`. They emit a start trace with null output, push a new context with their own trace id as the new parent and depth incremented by one, call the wrapped method, then emit the complete trace with output populated.

---

## StationTraceTree

Assembles the live parent-child tree from the flat stream of `StationTrace` events emitted by `InspectorSession`. The tree is the data model for the trace tree panel.

```csharp
namespace LlmInspector.Inspector;

public sealed class StationTraceTree
{
    public ObservableCollection<StationCardViewModel> Roots { get; } = new();
    private readonly Dictionary<string, StationCardViewModel> _all = new();
    private readonly List<StationTrace> _pending = new(); // arrived before parent

    public void Add(StationTrace trace)
    {
        var vm = new StationCardViewModel(trace);
        _all[trace.TraceId] = vm;

        if (trace.ParentTraceId is null)
        {
            Roots.Add(vm);
        }
        else if (_all.TryGetValue(trace.ParentTraceId, out var parent))
        {
            parent.Children.Add(vm);
        }
        else
        {
            _pending.Add(trace); // buffer out-of-order arrivals
        }

        // Flush any pending traces whose parent has now arrived.
        FlushPending();
    }

    public void Complete(StationTrace completed)
    {
        if (_all.TryGetValue(completed.TraceId, out var vm))
            vm.Update(completed);
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
                    _all.TryGetValue(pid, out var parent))
                {
                    var vm = new StationCardViewModel(_pending[i]);
                    _all[_pending[i].TraceId] = vm;
                    parent.Children.Add(vm);
                    _pending.RemoveAt(i);
                    flushed = true;
                }
            }
        } while (flushed);
    }
}
```

---

## TopologySceneObserver

Never reads the registry. Reacts to completed `StationTrace` events, inspects the output property tree, recognises registry entry types, and adds geometry to the Helix Toolkit scene.

The key design rule: child node positions are calculated at insertion time by counting existing siblings. No global layout pass. Each new node finds its parent's position in `_nodePositions`, counts how many siblings already exist, and places itself in the next available arc slot around the parent. The 3D hierarchy grows organically from the data hierarchy, exactly as HTML binding builds a DOM tree.

```csharp
namespace LlmInspector.Inspector;

public sealed class TopologySceneObserver
{
    private readonly Model3DGroup _scene;
    private readonly Dictionary<string, Point3D> _nodePositions = new();

    // Processing nodes at fixed positions — added once at construction.
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

        if (TryExtractDomainEntry(trace.Output, out var domain))
        {
            AddDomainNode(domain);
            return;
        }
        if (TryExtractPhaseEntry(trace.Output, out var phase))
        {
            AddPhaseNode(phase);
            return;
        }
        if (TryExtractSubPhaseEntry(trace.Output, out var subPhase))
        {
            AddSubPhaseNode(subPhase);
            return;
        }
        if (TryExtractTransitionEntry(trace.Output, out var transition))
        {
            AddTransitionEdge(transition);
        }
    }

    private void AddDomainNode(LogicalDomainEntry entry)
    {
        int n = _nodePositions.Keys.Count(k => k.StartsWith("domain:"));
        // Place this domain at its arc position in the ring.
        // As new domains arrive, earlier domains shift to maintain even spacing.
        // Full repositioning on each add keeps the ring balanced.
        RepositionDomainRing(n + 1);
        var pos = DomainRingPosition(n, n + 1);
        _nodePositions["domain:" + entry.Domain.DomainId] = pos;
        _scene.Children.Add(BuildSphere(pos, 0.5, DomainIdleMaterial));
        _scene.Children.Add(BuildEdge(UnitPos, pos, 0.03));
    }

    private void AddPhaseNode(LogicalPhaseEntry entry)
    {
        var parentKey = "domain:" + (entry.Phase.DomainIds.FirstOrDefault() ?? "");
        if (!_nodePositions.TryGetValue(parentKey, out var parentPos)) return;
        int siblings = _nodePositions.Keys.Count(k => k.StartsWith("phase:") &&
            IsNear(_nodePositions[k], parentPos, 2.0));
        var pos = ChildArcPosition(parentPos, siblings, siblings + 1, 1.2, -1.5);
        _nodePositions["phase:" + entry.Phase.PhaseId] = pos;
        _scene.Children.Add(BuildSphere(pos, 0.32, PhaseIdleMaterial));
        _scene.Children.Add(BuildEdge(parentPos, pos, 0.025));
    }

    private void AddSubPhaseNode(LogicalSubPhaseEntry entry)
    {
        var parentKey = "phase:" + entry.SubPhase.PhaseId;
        if (!_nodePositions.TryGetValue(parentKey, out var parentPos)) return;
        int siblings = _nodePositions.Keys.Count(k => k.StartsWith("subphase:") &&
            IsNear(_nodePositions[k], parentPos, 1.2));
        var pos = ChildArcPosition(parentPos, siblings, siblings + 1, 0.8, -1.5);
        _nodePositions["subphase:" + entry.SubPhase.SubPhaseId] = pos;
        _scene.Children.Add(BuildSphere(pos, 0.18, SubPhaseIdleMaterial));
        _scene.Children.Add(BuildEdge(parentPos, pos, 0.02));
    }

    private void AddTransitionEdge(LogicalTransitionEntry entry)
    {
        var fromKey = entry.Transition.FromTopologyKind.ToLowerInvariant()
                    + ":" + entry.Transition.FromTopologyId;
        var toKey   = entry.Transition.ToTopologyKind.ToLowerInvariant()
                    + ":" + entry.Transition.ToTopologyId;
        if (_nodePositions.TryGetValue(fromKey, out var from) &&
            _nodePositions.TryGetValue(toKey, out var to))
            _scene.Children.Add(BuildEdge(from, to, 0.06, TransitionMaterial));
    }

    // Position n evenly spaced children in an arc around a parent.
    private static Point3D ChildArcPosition(
        Point3D parent, int index, int total, double radius, double yOffset)
    {
        double angle = 2 * Math.PI * index / Math.Max(1, total);
        return new Point3D(
            parent.X + radius * Math.Cos(angle),
            parent.Y + yOffset,
            parent.Z + radius * Math.Sin(angle));
    }

    // Position domains evenly around a ring centred at the origin.
    private static Point3D DomainRingPosition(int index, int total)
    {
        double angle = 2 * Math.PI * index / Math.Max(1, total);
        return new Point3D(4 * Math.Cos(angle), 0, 4 * Math.Sin(angle));
    }

    // Reposition all existing domain spheres when a new domain is added.
    // Keeps the ring balanced as it grows.
    private void RepositionDomainRing(int newTotal)
    {
        var domainKeys = _nodePositions.Keys
            .Where(k => k.StartsWith("domain:")).ToList();
        for (int i = 0; i < domainKeys.Count; i++)
            _nodePositions[domainKeys[i]] = DomainRingPosition(i, newTotal);
        // Rebuild scene geometry for repositioned nodes.
        // In practice this means removing and re-adding their GeometryModel3D.
        // Handled by the scene rebuild helper in TopologyViewport.xaml.cs.
    }

    private static bool IsNear(Point3D a, Point3D b, double threshold) =>
        (a - b).Length < threshold;
}
```

Type recognition helpers (`TryExtractDomainEntry` etc.) inspect the `PropertyNode` output tree for known field names — look for a root node named `"Domain"` with a child named `"DomainId"`, and so on. These are the same field names the `PropertyNodeSerialiser` produces from reflection, so the match is stable.

---

## PropertyNodeSerialiser

Converts any C# object to a `PropertyNode` tree using reflection. Handles primitives, records, arrays, and nested objects. Depth capped at six levels to prevent cycles.

```csharp
namespace LlmInspector.Inspector;

public sealed record PropertyNode(
    string Name,
    string? Value,
    string? TypeHint,
    IReadOnlyList<PropertyNode> Children);

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
        if (obj is DateTimeOffset dt) return [Leaf("value", dt.ToString("O"), "DateTimeOffset")];
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

---

## Trace tree panel — XAML structure

The trace tree is a recursive `ItemsControl` where each level uses a horizontal `WrapPanel`. Children of the same parent are rendered side by side. A vertical connector line runs from each parent card to the row of its children via a `Border` with `BorderThickness="0,0,0,1"` on the parent and a left-margin indent on the child group.

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
<!-- StationCard.xaml — recursive -->
<StackPanel>

  <!-- The card itself -->
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

      <!-- Signal dot: pulses while in flight, solid on complete -->
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

  <!-- Children: WrapPanel so parallel stations sit side by side on the same row -->
  <ItemsControl ItemsSource="{Binding Children}" Margin="16,0,0,0"
                Visibility="{Binding HasChildren, Converter={StaticResource BoolToVisible}}">
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

</StackPanel>
```

This produces correct layout at any depth with no special cases. A single child renders as a single card below the parent. Two parallel children render side by side. Each is itself a `StationCard` that renders its own children the same way.

---

## StationCardViewModel

```csharp
namespace LlmInspector.ViewModels;

public sealed partial class StationCardViewModel : ObservableObject
{
    [ObservableProperty] private bool _isRunning = true;
    [ObservableProperty] private bool _hasError;
    private StationTrace _trace;

    public StationCardViewModel(StationTrace trace) => _trace = trace;

    public ObservableCollection<StationCardViewModel> Children { get; } = new();
    public bool HasChildren => Children.Count > 0;

    public string StationName => _trace.StationName;
    public IReadOnlyList<PropertyNode> Input  => _trace.Input;
    public IReadOnlyList<PropertyNode>? Output => _trace.Output;

    public string DurationText => _trace.CompletedAt.HasValue
        ? $"{(_trace.CompletedAt.Value - _trace.StartedAt).TotalMilliseconds:F1} ms"
        : "…";

    // One-line summary: first leaf value from the output tree.
    public string Summary =>
        _trace.Output is { } out_ ? FindFirstLeaf(out_)?.Value ?? "" : "";

    public Brush KindBackground => _trace.Kind switch
    {
        StationKind.Orchestrator => Brushes.OrchestratorTint,
        StationKind.Flow         => Brushes.FlowTint,
        StationKind.Unit         => Brushes.UnitTint,
        StationKind.Registry     => Brushes.RegistryTint,
        _                        => Brushes.Transparent
    };

    // Phosphor green when complete, amber pulsing when in flight, red on error.
    public Brush SignalBrush =>
        _hasError    ? Brushes.ErrorRed :
        _isRunning   ? Brushes.Amber :
                       Brushes.PhosphorGreen;

    public void Update(StationTrace completed)
    {
        _trace   = completed;
        IsRunning = false;
        HasError  = completed.ErrorCode is not null;
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(Output));
        OnPropertyChanged(nameof(SignalBrush));
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
               Margin="0,0,0,4"/>

    <TextBlock Text="{Binding KindLabel}"
               FontFamily="{StaticResource FontMono}"
               FontSize="10" Foreground="{StaticResource BrushTextTertiary}"
               Margin="0,0,0,16"/>

    <!-- Error block: visible only when HasError is true -->
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
<!-- PropertyNodeView.xaml — one node in the property tree, recursive -->
<StackPanel Margin="0,1">

  <!-- Leaf: Name = Value on one line -->
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

  <!-- Parent: expandable, children rendered below -->
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

## 3D scene — Helix Toolkit setup

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

  <!-- Oscilloscope reference grid -->
  <hx:GridLinesVisual3D Width="20" Length="20"
                         MinorDistance="1" MajorDistance="5"
                         Thickness="0.008" Fill="#1A1A14"/>

  <!-- Scene root: TopologySceneObserver adds children here as traces arrive -->
  <ModelVisual3D x:Name="SceneRoot"/>
</hx:HelixViewport3D>
```

Node materials use `EmissiveMaterial` so nodes glow against the dark background independent of light direction.

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
  <SolidColorBrush x:Key="BrushRegistryTint"     Color="#0A0A04"/>

  <SolidColorBrush x:Key="BrushOrchestratorBorder" Color="#3A2000"/>
  <SolidColorBrush x:Key="BrushFlowBorder"         Color="#003A18"/>
  <SolidColorBrush x:Key="BrushUnitBorder"         Color="#001830"/>
  <SolidColorBrush x:Key="BrushRegistryBorder"     Color="#2A2800"/>
</ResourceDictionary>
```

```xml
<!-- Typography.xaml -->
<ResourceDictionary>
  <FontFamily x:Key="FontMono">Cascadia Code, Consolas, Courier New</FontFamily>
</ResourceDictionary>
```

---

## Implementation phase plan

Each phase is one executor session. Gate B for every phase is:
`dotnet build LlmInspector.sln` — zero errors, then focused run if tests were added.

**Phase 1 — Solution scaffold.**
`LlmInspector.csproj`, `App.xaml`, `MainWindow.xaml` (blank three-column grid, command bar), NuGet packages, project reference to `21.AgentInfrastructure`. Colors.xaml and Typography.xaml loaded.
Gate B: solution builds, blank near-black window opens.

**Phase 2 — Data layer.**
`PropertyNode.cs`, `StationTrace.cs`, `StationKind.cs`, `PropertyNodeSerialiser.cs`. Unit tests serialising a real `LogicalDomainEntry`, `LogicalPhaseEntry`, and `LogicalTransitionEntry` instance and asserting field names and values.
Gate B: all serialiser tests pass.

**Phase 3 — TraceContextScope.**
`TraceContextScope.cs` with `AsyncLocal<TraceContext>` and `Push`/`Current`. Unit test: context pushed in one async branch is not visible in a sibling branch started before the push.
Gate B: isolation test passes.

**Phase 4 — Tracing decorators and InspectorSession.**
`TracingRegistryDecorator.cs`, `TracingFlowDecorator<T>.cs`, `InspectorSession.cs`. Integration test: fire one `AddDomainCommand`, capture all emitted `StationTrace` events, assert four traces with correct `Depth` values (0 orchestrator, 1 flow, 2 unit, 3 registry) and correct `ParentTraceId` chain.
Gate B: integration test passes.

**Phase 5 — StationTraceTree and StationCardViewModel.**
`StationTraceTree.cs`, `StationCardViewModel.cs`. Unit test: feed the four traces from Phase 4 into the tree, assert `Roots.Count == 1`, `Roots[0].Children.Count == 1`, depth chain correct to leaf.
Gate B: tree assembly test passes.

**Phase 6 — Trace tree panel.**
`TraceTreePanel.xaml`, `StationCard.xaml`. Bound to `MainViewModel.Tree`. Fire one `AddDomainCommand` and observe the four-level card tree rendered with correct station names, summaries, and durations.
Gate B: four-level tree visible in the centre panel.

**Phase 7 — Parallel station rendering.**
Add a test `InspectorSession` variant that runs two unit commands in parallel. Assert that both unit cards appear side by side on the same `WrapPanel` row at the same depth level.
Gate B: two cards visible side by side.

**Phase 8 — 3D viewport and processing nodes.**
`TopologyViewport.xaml`, `TopologySceneObserver.cs` with `AddProcessingNodes()`. Helix Toolkit `HelixViewport3D` with reference grid, ambient lights. Three processing node boxes appear immediately on startup.
Gate B: window opens, three box nodes visible, camera orbits on drag.

**Phase 9 — Domain node from trace output.**
Extend `TopologySceneObserver.OnTraceCompleted` to recognise `LogicalDomainEntry` in output and add a domain sphere.
Gate B: fire `AddDomainCommand`, domain sphere appears in 3D scene without restarting.

**Phase 10 — Phase and sub-phase nodes.**
Extend `OnTraceCompleted` for `LogicalPhaseEntry` and `LogicalSubPhaseEntry`. Child spheres appear below their parent at insertion time.
Gate B: fire `AddPhaseCommand` after `AddDomainCommand`, phase sphere appears as child of domain sphere in the correct arc position.

**Phase 11 — Transition edges.**
Extend `OnTraceCompleted` for `LogicalTransitionEntry`. Cylinder drawn between the two named existing nodes.
Gate B: fire `AddTransitionCommand`, cylinder connects the two spheres.

**Phase 12 — Station inspector.**
`StationInspector.xaml`, `PropertyNodeView.xaml`. Inspector column animates from zero width to `4*` when a card is clicked, back to zero when closed. Full property tree visible with correct field names and values.
Gate B: click a card, inspector opens with complete input and output trees.

**Phase 13 — Processing node pulse.**
While a station is in flight (`IsRunning == true`), its corresponding processing node pulses from idle to active material at 300 ms intervals via a `DispatcherTimer`. Returns to idle on complete.
Gate B: observe orchestrator box pulse amber during command execution, return to idle on complete.

**Phase 14 — Command input bar.**
`ComboBox` for command type, structured form fields for the selected command type (domain id, name, purpose for `AddDomainCommand` etc.), `Run ▶` button wired to `MainViewModel.RunCommand`. Full end-to-end: fill the form, click Run, trace tree grows, domain sphere appears, inspector opens on click.
Gate B: complete round trip works without touching code between runs.
