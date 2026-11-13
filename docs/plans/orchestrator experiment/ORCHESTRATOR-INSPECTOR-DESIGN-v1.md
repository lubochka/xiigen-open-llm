# Orchestrator Inspector — WPF + Helix Toolkit Design Document
# Version: 1.0 — 2026-05-04
# Target: Windows desktop application, .NET 8, WPF, HelixToolkit.Wpf (MIT)
# Purpose: Visual debugger for the ILogicalUnit / ILogicalFlow / ILogicalOrchestrator chain

---

## What the tool does

A developer types a topology command (for example, add a domain) or a natural-language
request into an input box, fires it through LogicalOrchestratorAssistant, and watches
each station light up in sequence: orchestrator selects the flow, the flow routes to
the correct unit, the unit calls the registry, the registry stores and returns the entry.
Every station shows its raw input and raw output. Nothing is summarised.

The 3D viewport shows the current registry state as a live node graph —
domains as large spheres, phases as medium spheres parented to their domain,
sub-phases as small spheres parented to their phase, transitions as cylinders
connecting the nodes they link. When a command runs, the path it takes through
the graph lights up in sequence with a timed pulse animation.

---

## Project structure

```
LlmInspector/
  LlmInspector.csproj          -- WPF, net8.0-windows, HelixToolkit.Wpf NuGet
  App.xaml
  App.xaml.cs
  MainWindow.xaml               -- shell with three-column Grid
  MainWindow.xaml.cs

  Inspector/
    StationTrace.cs             -- the core data record produced per station
    InspectorSession.cs         -- wraps LogicalOrchestratorAssistant, emits traces
    TopologySceneBuilder.cs     -- converts registry state to HelixToolkit geometry

  ViewModels/
    MainViewModel.cs
    TopologyNodeViewModel.cs    -- one per domain/phase/sub-phase/transition in the 3D scene
    StationTraceViewModel.cs    -- one per row in the signal trace panel

  Views/
    TopologyViewport.xaml       -- left panel: HelixViewport3D
    TopologyViewport.xaml.cs
    SignalTracePanel.xaml       -- center panel: scrolling station list
    SignalTracePanel.xaml.cs
    StationInspector.xaml       -- right panel: input/output tree
    StationInspector.xaml.cs

  Styles/
    Colors.xaml                 -- brush resources
    Typography.xaml
    Controls.xaml
```

NuGet packages required:
- HelixToolkit.Wpf             4.x  (MIT) — 3D viewport, geometry helpers, camera control
- CommunityToolkit.Mvvm        8.x  (MIT) — ObservableObject, RelayCommand, source generators
- Microsoft.Extensions.DependencyInjection (optional, for wiring)

No other external dependencies. The tool references 21.AgentInfrastructure directly
as a project reference.

---

## The StationTrace record

This is the single data shape the inspector is built around. Every station in
the execution chain produces one StationTrace. The inspector session collects them
into an ObservableCollection.

```csharp
namespace LlmInspector.Inspector;

// One station event captured during a single assistant invocation.
public sealed record StationTrace(
    // Unique id for this invocation — shared across all traces in the same run.
    string RunId,

    // Sequential position in the execution chain for this run (0 = orchestrator).
    int StationIndex,

    // Human-readable station name: "Orchestrator", "TopologyMutationFlow",
    // "AddDomainTopologyMutationUnit", "InMemoryLogicalTopologyRegistry".
    string StationName,

    // The station kind drives how the inspector panel renders the node.
    StationKind Kind,

    // The raw input to this station, serialised to a property tree.
    IReadOnlyList<PropertyNode> Input,

    // The raw output from this station, serialised to a property tree.
    // Null if the station has not yet completed (used for live streaming).
    IReadOnlyList<PropertyNode>? Output,

    // The error code if the station failed, null if it succeeded.
    string? ErrorCode,

    // The error message if the station failed, null if it succeeded.
    string? ErrorMessage,

    // Wall-clock time when the station received its input.
    DateTimeOffset StartedAt,

    // Wall-clock time when the station returned its output. Null if in progress.
    DateTimeOffset? CompletedAt,

    // The topology node id this trace maps to, for lighting up the 3D scene.
    // For the orchestrator this is null. For a unit it is the unit's DomainTag.
    string? TopologyNodeId
);

public enum StationKind
{
    Orchestrator,   // LogicalOrchestratorAssistant
    Flow,           // ILogicalFlow<T>
    Unit,           // ILogicalUnit<TInput, TOutput>
    Registry        // ILogicalTopologyRegistry
}

// One node in the property tree shown in the inspector panel.
public sealed record PropertyNode(
    string Name,
    string? Value,        // Null when the node has children.
    string? TypeHint,     // "string", "long", "DateTimeOffset", "string[]", etc.
    IReadOnlyList<PropertyNode> Children
);
```

---

## InspectorSession

The InspectorSession wraps the real assistant and emits StationTrace events as each
station completes. It does not mock or stub anything — it calls the real
LogicalOrchestratorAssistant.RunTopologyCommandAsync and instruments the call by
decorating the registry and flow with thin tracing wrappers before construction.

```csharp
namespace LlmInspector.Inspector;

// Wraps LogicalOrchestratorAssistant and emits StationTrace events per station.
public sealed class InspectorSession
{
    // The underlying assistant built with tracing-instrumented dependencies.
    private readonly LogicalOrchestratorAssistant _assistant;

    // The tracing registry captures all registry calls and emits traces.
    private readonly TracingLogicalTopologyRegistry _tracingRegistry;

    // Raised when any station starts or completes.
    public event EventHandler<StationTrace>? StationTraced;

    public InspectorSession()
    {
        // Build the tracing registry first — it wraps InMemoryLogicalTopologyRegistry.
        _tracingRegistry = new TracingLogicalTopologyRegistry(
            new InMemoryLogicalTopologyRegistry(),
            trace => StationTraced?.Invoke(this, trace));

        // Build all mutation units pointing at the tracing registry.
        var addDomainUnit  = new AddDomainTopologyMutationUnit(_tracingRegistry);
        var addPhaseUnit   = new AddPhaseTopologyMutationUnit(_tracingRegistry);
        var addSubPhaseUnit = new AddSubPhaseTopologyMutationUnit(_tracingRegistry);
        var addTransitionUnit = new AddTransitionTopologyMutationUnit(_tracingRegistry);

        // Build the tracing flow that emits a trace before and after delegating.
        var topologyFlow = new TracingTopologyMutationFlow(
            new TopologyMutationFlow(addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit),
            trace => StationTraced?.Invoke(this, trace));

        // Build the assistant with the tracing flow injected.
        // CreateDefault is not used here — we construct manually to inject tracers.
        _assistant = new LogicalOrchestratorAssistant(
            new NullSessionStorage(),
            orchestrator: null,  // natural-language RunAsync not wired in this tool yet
            topologyMutationFlow: topologyFlow);
    }

    // Execute a typed topology command and collect all station traces.
    public async Task<IReadOnlyList<StationTrace>> RunAsync(
        LogicalTopologyCommand command,
        CancellationToken cancellationToken = default)
    {
        var traces = new List<StationTrace>();
        void Capture(object? _, StationTrace t) => traces.Add(t);
        StationTraced += Capture;
        try
        {
            // Emit the orchestrator station first.
            var orchestratorTrace = StationTrace.StartOrchestrator(command);
            StationTraced?.Invoke(this, orchestratorTrace);

            var result = await _assistant.RunTopologyCommandAsync(command, cancellationToken: cancellationToken);

            StationTraced?.Invoke(this, orchestratorTrace.Complete(result));
        }
        finally
        {
            StationTraced -= Capture;
        }
        return traces;
    }

    // Expose the tracing registry for the scene builder to read topology state.
    public ILogicalTopologyRegistry Registry => _tracingRegistry.Inner;
}
```

The tracing wrappers (TracingLogicalTopologyRegistry, TracingTopologyMutationFlow) are
thin decorators. Each one records StartedAt, serialises the input to a PropertyNode tree,
calls the wrapped method, serialises the output, and invokes the trace callback. They add
no logic — only observation.

---

## Window layout

```
+----------------------------------------------------------+
|  [icon] Orchestrator Inspector          [minimize][close] |
+------------------------------------------+-------+-------+
| Topology Viewport (3D)          | Signal | Station       |
|                                 | Trace  | Inspector     |
| HelixViewport3D                 | panel  |               |
|  -- domains as large spheres    |        | [Name]        |
|  -- phases as medium spheres    | [row0] | Kind: Unit    |
|  -- sub-phases as small spheres | [row1] |               |
|  -- transitions as cylinders    | [row2] | Input ──────  |
|                                 | [row3] |  ▾ command    |
|  [camera: orbit drag]           | [row4] |    DomainId   |
|  [zoom: scroll]                 | [row5] |    Name       |
|  [reset: double-click]          |        |    Purpose    |
|                                 |        |               |
|                                 |        | Output ─────  |
|                                 |        |  ▾ entry      |
|                                 |        |    Domain     |
| [Command input bar]             |        |    Revision   |
+------------------------------------------+-------+-------+
| [command dropdown] [text input] [Run ▶]    [status bar]  |
+----------------------------------------------------------+
```

The window has no menu bar. The command input bar sits at the bottom spanning the
full width. The three columns have proportions 5 : 3 : 4 in a WPF Grid with
GridSplitters between them so the user can resize.

---

## MainWindow.xaml layout skeleton

```xml
<Window x:Class="LlmInspector.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:hx="http://helix-toolkit.org/wpf"
        xmlns:views="clr-namespace:LlmInspector.Views"
        Title="Orchestrator Inspector" Height="800" Width="1400"
        Background="{StaticResource BrushBackground}"
        FontFamily="{StaticResource FontPrimary}">

  <Grid>
    <Grid.RowDefinitions>
      <RowDefinition Height="*"/>
      <RowDefinition Height="Auto"/>  <!-- command bar -->
    </Grid.RowDefinitions>

    <!-- Three-column content area -->
    <Grid Grid.Row="0">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="5*" MinWidth="300"/>
        <ColumnDefinition Width="4"/>            <!-- splitter -->
        <ColumnDefinition Width="3*" MinWidth="220"/>
        <ColumnDefinition Width="4"/>            <!-- splitter -->
        <ColumnDefinition Width="4*" MinWidth="260"/>
      </Grid.ColumnDefinitions>

      <views:TopologyViewport Grid.Column="0" DataContext="{Binding TopologyScene}"/>

      <GridSplitter Grid.Column="1" Width="4" HorizontalAlignment="Stretch"
                    Background="{StaticResource BrushBorder}"/>

      <views:SignalTracePanel Grid.Column="2"
                              ItemsSource="{Binding Traces}"
                              SelectedItem="{Binding SelectedTrace, Mode=TwoWay}"/>

      <GridSplitter Grid.Column="3" Width="4" HorizontalAlignment="Stretch"
                    Background="{StaticResource BrushBorder}"/>

      <views:StationInspector Grid.Column="4"
                              DataContext="{Binding SelectedTrace}"/>
    </Grid>

    <!-- Command input bar -->
    <Border Grid.Row="1" BorderThickness="0,1,0,0"
            BorderBrush="{StaticResource BrushBorder}"
            Background="{StaticResource BrushSurface}"
            Padding="12,8">
      <DockPanel>
        <ComboBox DockPanel.Dock="Left" Width="200"
                  ItemsSource="{Binding CommandTypes}"
                  SelectedItem="{Binding SelectedCommandType}"
                  Margin="0,0,8,0"/>
        <Button DockPanel.Dock="Right" Content="Run ▶"
                Width="80" Command="{Binding RunCommand}"
                Style="{StaticResource ButtonPrimary}"/>
        <TextBox Text="{Binding CommandInput, UpdateSourceTrigger=PropertyChanged}"
                 Style="{StaticResource InputBox}"
                 Margin="0,0,8,0"/>
      </DockPanel>
    </Border>
  </Grid>
</Window>
```

---

## Topology viewport (left panel)

### Helix Toolkit setup

```xml
<!-- TopologyViewport.xaml -->
<UserControl ...>
  <hx:HelixViewport3D x:Name="Viewport"
                      Background="Transparent"
                      ShowCoordinateSystem="False"
                      ShowFrameRate="False"
                      IsHeadLightEnabled="True"
                      CameraRotationMode="Trackball">

    <!-- Ambient light so dark-side surfaces are not pure black -->
    <hx:HelixViewport3D.Children>
      <AmbientLight Color="#1A1A2E"/>
      <DirectionalLight Color="#AAAACC" Direction="-1,-1,-1"/>
      <DirectionalLight Color="#334455" Direction="1,0.5,0.5"/>
    </hx:HelixViewport3D.Children>

    <!-- The ModelVisual3D tree is built in code-behind by TopologySceneBuilder -->
    <ModelVisual3D x:Name="SceneRoot"/>
  </hx:HelixViewport3D>
</UserControl>
```

### Node geometry

Each topology element maps to a MeshGeometryModel3D built by HelixToolkit's
mesh builders:

| Registry type | Geometry | Radius | Color (idle) | Color (active) |
|---|---|---|---|---|
| Domain | Sphere | 0.5 | #2A4A7F (steel blue) | #5B9BFF (bright blue) |
| Phase | Sphere | 0.32 | #1E6B5A (teal) | #3DDDAF (mint) |
| Sub-phase | Sphere | 0.2 | #5A3A7A (violet) | #BF8FFF (lavender) |
| Transition | Cylinder | radius 0.05 | #444466 (dim) | #FF9933 (amber pulse) |
| Orchestrator | Icosahedron | 0.6 | #6A2020 (dark red) | #FF4444 (red pulse) |
| Flow | Rounded box | 0.4 | #3A4A2A (dark green) | #88DD44 (lime) |
| Unit | Rounded box | 0.28 | #3A3A4A (dark slate) | #AAAAFF (lilac) |

### Layout algorithm

The 3D positions are assigned by TopologySceneBuilder using a simple layered layout:
- Orchestrator at (0, 4, 0) — top centre
- Flow at (0, 2.5, 0) — below orchestrator
- Units arranged in a horizontal arc at Y=1, radius 2 from centre
- Domain spheres arranged in a ring at Y=0, radius 4 from centre
- Phase spheres positioned at Y=-1.5, at the XZ position of their domain ± small offset
- Sub-phase spheres at Y=-3, offset from their phase
- Transition cylinders: drawn between the FromTopologyId node and ToTopologyId node

Edges are thin cylinders (radius 0.03) connecting parent to child nodes in the
hierarchy. Transition cylinders use the stored FromTopologyId and ToTopologyId to
find the two sphere positions and draw between them.

### TopologySceneBuilder.cs

```csharp
namespace LlmInspector.Inspector;

// Converts registry state into a HelixToolkit 3D scene.
// Returns a Model3DGroup that can be assigned to a ModelVisual3D.Content.
public sealed class TopologySceneBuilder
{
    // Material for each idle state and active (pulsing) state.
    // Colors match the table in the design document.
    private static readonly DiffuseMaterial DomainIdle  = MakeMaterial("#2A4A7F");
    private static readonly DiffuseMaterial DomainActive = MakeMaterial("#5B9BFF");
    private static readonly DiffuseMaterial PhaseIdle   = MakeMaterial("#1E6B5A");
    private static readonly DiffuseMaterial PhaseActive = MakeMaterial("#3DDDAF");
    // ... etc for each type

    // Build the full scene from current registry state.
    public SceneResult Build(
        ILogicalTopologyRegistry registry,
        IReadOnlySet<string>? activeNodeIds = null)
    {
        var group = new Model3DGroup();
        var nodeMap = new Dictionary<string, Point3D>();

        var domains   = registry.ListDomains().Value   ?? [];
        var phases    = registry.ListPhases().Value    ?? [];
        var subPhases = registry.ListSubPhases().Value ?? [];
        var transitions = registry.ListTransitions().Value ?? [];

        // Position domains in a ring at Y=0.
        int di = 0;
        foreach (var entry in domains)
        {
            double angle = 2 * Math.PI * di / Math.Max(1, domains.Count);
            var pos = new Point3D(4 * Math.Cos(angle), 0, 4 * Math.Sin(angle));
            nodeMap[entry.Domain.DomainId] = pos;

            bool active = activeNodeIds?.Contains(entry.Domain.DomainId) ?? false;
            group.Children.Add(BuildSphere(pos, 0.5, active ? DomainActive : DomainIdle));
            group.Children.Add(BuildLabel(pos + new Vector3D(0, 0.65, 0), entry.Domain.Name));
            di++;
        }

        // Position phases offset from their domain.
        int pi = 0;
        foreach (var entry in phases)
        {
            if (!nodeMap.TryGetValue(entry.Phase.DomainIds.FirstOrDefault() ?? "", out var domainPos))
                domainPos = new Point3D(pi * 1.2 - 3, -1.5, 0);

            var offset = new Vector3D((pi % 3 - 1) * 0.8, -1.5, (pi % 2 == 0 ? 0.4 : -0.4));
            var pos = domainPos + offset;
            nodeMap[entry.Phase.PhaseId] = pos;

            bool active = activeNodeIds?.Contains(entry.Phase.PhaseId) ?? false;
            group.Children.Add(BuildSphere(pos, 0.32, active ? PhaseActive : PhaseIdle));
            // Draw edge to domain
            if (nodeMap.TryGetValue(entry.Phase.DomainIds.FirstOrDefault() ?? "", out var dp))
                group.Children.Add(BuildEdgeCylinder(dp, pos, 0.03));
            pi++;
        }

        // Sub-phases at Y=-3 below their phase.
        int spi = 0;
        foreach (var entry in subPhases)
        {
            if (!nodeMap.TryGetValue(entry.SubPhase.PhaseId, out var phasePos))
                phasePos = new Point3D(spi * 1.0 - 3, -3, 0);

            var offset = new Vector3D((spi % 3 - 1) * 0.5, -1.5, 0);
            var pos = phasePos + offset;
            nodeMap[entry.SubPhase.SubPhaseId] = pos;

            bool active = activeNodeIds?.Contains(entry.SubPhase.SubPhaseId) ?? false;
            group.Children.Add(BuildSphere(pos, 0.2, active ? SubPhaseActive : SubPhaseIdle));
            if (nodeMap.TryGetValue(entry.SubPhase.PhaseId, out var pp))
                group.Children.Add(BuildEdgeCylinder(pp, pos, 0.025));
            spi++;
        }

        // Transition cylinders between any two registered nodes.
        foreach (var entry in transitions)
        {
            if (nodeMap.TryGetValue(entry.Transition.FromTopologyId, out var from)
             && nodeMap.TryGetValue(entry.Transition.ToTopologyId, out var to))
            {
                bool active = activeNodeIds?.Contains(entry.Transition.TransitionId) ?? false;
                group.Children.Add(BuildEdgeCylinder(from, to, 0.06,
                    active ? TransitionActive : TransitionIdle));
            }
        }

        // Fixed positions for orchestrator, flow, unit nodes (above the topology ring).
        nodeMap["orchestrator"] = new Point3D(0, 4, 0);
        nodeMap["flow"]         = new Point3D(0, 2.5, 0);
        nodeMap["unit"]         = new Point3D(0, 1, 0);

        bool orchActive  = activeNodeIds?.Contains("orchestrator") ?? false;
        bool flowActive  = activeNodeIds?.Contains("flow") ?? false;
        bool unitActive  = activeNodeIds?.Contains("unit") ?? false;

        group.Children.Add(BuildIcosahedron(nodeMap["orchestrator"], 0.6,
            orchActive ? OrchestratorActive : OrchestratorIdle));
        group.Children.Add(BuildRoundedBox(nodeMap["flow"], 0.4,
            flowActive ? FlowActive : FlowIdle));
        group.Children.Add(BuildRoundedBox(nodeMap["unit"], 0.28,
            unitActive ? UnitActive : UnitIdle));

        // Edges: orchestrator → flow → unit → first domain (dashed via thin cylinders).
        group.Children.Add(BuildEdgeCylinder(nodeMap["orchestrator"], nodeMap["flow"], 0.03));
        group.Children.Add(BuildEdgeCylinder(nodeMap["flow"], nodeMap["unit"], 0.03));
        if (domains.Count > 0)
            group.Children.Add(BuildEdgeCylinder(nodeMap["unit"], nodeMap[domains[0].Domain.DomainId], 0.03));

        return new SceneResult(group, nodeMap);
    }

    // Helper: sphere at position using HelixToolkit MeshBuilder.
    private static GeometryModel3D BuildSphere(Point3D centre, double radius, Material mat)
    {
        var builder = new MeshBuilder(false, false);
        builder.AddSphere(centre, radius, 16, 12);
        return new GeometryModel3D(builder.ToMesh(), mat);
    }

    // Helper: cylinder between two points.
    private static GeometryModel3D BuildEdgeCylinder(
        Point3D from, Point3D to, double radius,
        Material? mat = null)
    {
        var builder = new MeshBuilder(false, false);
        builder.AddCylinder(from, to, radius, 8);
        return new GeometryModel3D(builder.ToMesh(), mat ?? EdgeIdle);
    }

    // Helper: icosahedron for the orchestrator node.
    private static GeometryModel3D BuildIcosahedron(Point3D centre, double radius, Material mat)
    {
        // HelixToolkit does not have an icosahedron primitive directly.
        // Use a subdivided sphere with low subdivision count to approximate.
        var builder = new MeshBuilder(false, false);
        builder.AddSphere(centre, radius, 6, 4);  // 6×4 gives an approximate icosahedron feel
        return new GeometryModel3D(builder.ToMesh(), mat);
    }

    // Helper: axis-aligned box for flow/unit nodes.
    private static GeometryModel3D BuildRoundedBox(Point3D centre, double size, Material mat)
    {
        var builder = new MeshBuilder(false, false);
        builder.AddBox(centre, size, size * 0.6, size);
        return new GeometryModel3D(builder.ToMesh(), mat);
    }

    private static DiffuseMaterial MakeMaterial(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        return new DiffuseMaterial(new SolidColorBrush(color));
    }
}

// Result returned by the scene builder.
public sealed record SceneResult(
    Model3DGroup Scene,
    IReadOnlyDictionary<string, Point3D> NodePositions);
```

### Hit-testing (clicking a node)

In TopologyViewport.xaml.cs, subscribe to Viewport.MouseDown. Use
HelixViewport3D.FindNearestVisual to get the clicked model, match it to the
node map, find which domain/phase/sub-phase it represents, and set
MainViewModel.SelectedTopologyNodeId. The signal trace panel scrolls to the
matching trace, and the inspector panel opens the station data for that node.

---

## Signal trace panel (centre panel)

A ListView bound to MainViewModel.Traces (ObservableCollection<StationTraceViewModel>).
Auto-scrolls to the newest item as traces arrive. Each row is a DataTemplate.

```xml
<!-- SignalTracePanel.xaml -->
<ListView ItemsSource="{Binding}"
          SelectedItem="{Binding DataContext.SelectedTrace,
                         RelativeSource={RelativeSource AncestorType=Window}, Mode=TwoWay}"
          ScrollViewer.HorizontalScrollBarVisibility="Disabled"
          Background="Transparent" BorderThickness="0">
  <ListView.ItemTemplate>
    <DataTemplate>
      <Border Margin="0,1" Padding="10,8"
              Background="{Binding KindBrush}"
              CornerRadius="4">
        <Grid>
          <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
          </Grid.ColumnDefinitions>

          <!-- Station index badge -->
          <Border Grid.Column="0" Width="20" Height="20" CornerRadius="10"
                  Background="{Binding IndexBrush}" Margin="0,0,8,0">
            <TextBlock Text="{Binding StationIndex}"
                       FontSize="10" FontWeight="Bold"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       Foreground="White"/>
          </Border>

          <!-- Station name and summary -->
          <StackPanel Grid.Column="1">
            <TextBlock Text="{Binding StationName}" FontSize="12" FontWeight="SemiBold"
                       Foreground="{StaticResource BrushTextPrimary}"/>
            <TextBlock Text="{Binding Summary}" FontSize="11"
                       Foreground="{StaticResource BrushTextSecondary}"
                       TextTrimming="CharacterEllipsis"/>
          </StackPanel>

          <!-- Duration -->
          <TextBlock Grid.Column="2" Text="{Binding DurationText}"
                     FontSize="10" FontFamily="{StaticResource FontMono}"
                     Foreground="{StaticResource BrushTextTertiary}"
                     VerticalAlignment="Center"/>
        </Grid>
      </Border>
    </DataTemplate>
  </ListView.ItemTemplate>
</ListView>
```

KindBrush returns a very light background tint per station kind:
- Orchestrator: #1A0A0A (dark red tint)
- Flow: #0A1A0A (dark green tint)
- Unit: #0A0A1A (dark blue tint)
- Registry: #1A1A0A (dark amber tint)

DurationText: formatted as "0.4 ms" or "12 ms" from (CompletedAt - StartedAt).

---

## Station inspector (right panel)

When a StationTrace is selected in the signal trace panel, the inspector shows
its full input and output as expandable property trees.

```xml
<!-- StationInspector.xaml -->
<UserControl DataContext="{Binding SelectedTrace}">
  <ScrollViewer VerticalScrollBarVisibility="Auto">
    <StackPanel Margin="12">

      <!-- Station header -->
      <TextBlock Text="{Binding StationName}" FontSize="15" FontWeight="SemiBold"
                 Foreground="{StaticResource BrushTextPrimary}" Margin="0,0,0,4"/>
      <TextBlock Text="{Binding KindLabel}" FontSize="11"
                 Foreground="{StaticResource BrushTextSecondary}" Margin="0,0,0,16"/>

      <!-- Error block (collapsed when no error) -->
      <Border Background="#2A0000" CornerRadius="4" Padding="8"
              Margin="0,0,0,12"
              Visibility="{Binding HasError, Converter={StaticResource BoolToVisible}}">
        <StackPanel>
          <TextBlock Text="{Binding ErrorCode}" FontSize="11" FontWeight="Bold"
                     Foreground="#FF6666" FontFamily="{StaticResource FontMono}"/>
          <TextBlock Text="{Binding ErrorMessage}" FontSize="11"
                     Foreground="#FFAAAA" TextWrapping="Wrap"/>
        </StackPanel>
      </Border>

      <!-- Input tree -->
      <TextBlock Text="Input" FontSize="11" FontWeight="Bold"
                 Foreground="{StaticResource BrushTextTertiary}"
                 Margin="0,0,0,6"/>
      <ItemsControl ItemsSource="{Binding Input}">
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <local:PropertyNodeView DataContext="{Binding}"/>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>

      <Separator Margin="0,12" Background="{StaticResource BrushBorder}"/>

      <!-- Output tree -->
      <TextBlock Text="Output" FontSize="11" FontWeight="Bold"
                 Foreground="{StaticResource BrushTextTertiary}"
                 Margin="0,0,0,6"/>
      <ItemsControl ItemsSource="{Binding Output}">
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <local:PropertyNodeView DataContext="{Binding}"/>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>

    </StackPanel>
  </ScrollViewer>
</UserControl>
```

### PropertyNodeView (recursive tree control)

```xml
<!-- PropertyNodeView.xaml — handles one PropertyNode, including its children -->
<UserControl x:Name="Root">
  <StackPanel Margin="0,1">

    <!-- Leaf node: Name = Value -->
    <Grid Visibility="{Binding HasChildren, Converter={StaticResource BoolToCollapsed}}">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
      </Grid.ColumnDefinitions>
      <TextBlock Text="{Binding Name}" FontSize="11" FontWeight="SemiBold"
                 Foreground="{StaticResource BrushTextSecondary}"
                 FontFamily="{StaticResource FontMono}" Margin="0,0,8,0"/>
      <TextBlock Grid.Column="1" Text="{Binding Value}" FontSize="11"
                 Foreground="{StaticResource BrushTextPrimary}"
                 FontFamily="{StaticResource FontMono}"
                 TextWrapping="Wrap"/>
    </Grid>

    <!-- Parent node: expandable -->
    <Expander Visibility="{Binding HasChildren, Converter={StaticResource BoolToVisible}}"
              IsExpanded="True">
      <Expander.Header>
        <StackPanel Orientation="Horizontal">
          <TextBlock Text="{Binding Name}" FontSize="11" FontWeight="SemiBold"
                     Foreground="{StaticResource BrushAccent}"
                     FontFamily="{StaticResource FontMono}"/>
          <TextBlock Text="{Binding TypeHint}" FontSize="10"
                     Foreground="{StaticResource BrushTextTertiary}"
                     Margin="6,0,0,0"/>
        </StackPanel>
      </Expander.Header>
      <ItemsControl ItemsSource="{Binding Children}" Margin="12,0,0,0">
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <local:PropertyNodeView DataContext="{Binding}"/>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </Expander>

  </StackPanel>
</UserControl>
```

---

## PropertyNode serialiser

Converts any C# object (a LogicalUnitResult<T>, LogicalFlowResult, LogicalTopologyCommand,
or OperationResult<T>) into a PropertyNode tree using reflection. Handles the specific
types used by the logical contracts.

```csharp
namespace LlmInspector.Inspector;

// Converts any object to a PropertyNode tree for the inspector panel.
public static class PropertyNodeSerialiser
{
    // Serialise any object. Handles primitives, records, arrays, collections.
    public static IReadOnlyList<PropertyNode> Serialise(object? obj, int depth = 0)
    {
        if (obj is null) return [new PropertyNode("(null)", null, null, [])];
        if (depth > 6) return [new PropertyNode("...", "(max depth)", null, [])];

        // Primitives and known value types render as leaf nodes.
        if (obj is string s) return [Leaf("value", s, "string")];
        if (obj is long l)   return [Leaf("value", l.ToString(), "long")];
        if (obj is double d) return [Leaf("value", d.ToString("F4"), "double")];
        if (obj is bool b)   return [Leaf("value", b.ToString(), "bool")];
        if (obj is DateTimeOffset dt) return [Leaf("value", dt.ToString("O"), "DateTimeOffset")];

        // Arrays and IEnumerable<string> render as indexed children.
        if (obj is string[] arr)
        {
            var items = arr.Select((v, i) => Leaf($"[{i}]", v, "string")).ToArray();
            return items.Length == 0
                ? [Leaf("value", "(empty)", "string[]")]
                : (IReadOnlyList<PropertyNode>)items;
        }

        // For record types and classes, reflect over public properties.
        var type = obj.GetType();
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (props.Length == 0) return [Leaf("value", obj.ToString() ?? "", type.Name)];

        return props.Select(p =>
        {
            var value = p.GetValue(obj);
            var typeName = p.PropertyType.Name;

            // Leaf case: primitive property.
            if (value is null || value is string || value.GetType().IsPrimitive ||
                value is DateTimeOffset || value is long || value is double)
                return Leaf(p.Name, value?.ToString() ?? "(null)", typeName);

            // Recursive case: complex property.
            var children = Serialise(value, depth + 1);
            return new PropertyNode(p.Name, null, typeName, children);
        }).ToArray();
    }

    private static PropertyNode Leaf(string name, string? value, string? typeHint)
        => new PropertyNode(name, value, typeHint, []);
}
```

---

## Pulse animation for active nodes

When RunAsync completes, MainViewModel receives the full StationTrace list and
replays the pulse in sequence. For each trace, it sets TopologyScene.ActiveNodeIds
to a set containing only that trace's TopologyNodeId, waits 400 ms, then advances
to the next trace. After all traces have pulsed, ActiveNodeIds is set to empty.

The scene builder rebuilds the geometry on every ActiveNodeIds change — WPF
data binding triggers a rebuild of SceneRoot.Content. Because HelixViewport3D
is GPU-accelerated, a full scene rebuild takes less than a millisecond for graphs
of a few dozen nodes.

```csharp
// In MainViewModel.cs
private async Task AnimatePulse(IReadOnlyList<StationTrace> traces)
{
    foreach (var trace in traces)
    {
        if (trace.TopologyNodeId is not null)
        {
            TopologyScene.SetActive(trace.TopologyNodeId);
            await Task.Delay(400);
        }
    }
    TopologyScene.ClearActive();
}
```

---

## Color and typography resources

```xml
<!-- Colors.xaml -->
<ResourceDictionary>
  <!-- Backgrounds -->
  <SolidColorBrush x:Key="BrushBackground" Color="#0D0D12"/>
  <SolidColorBrush x:Key="BrushSurface"    Color="#13131A"/>
  <SolidColorBrush x:Key="BrushPanel"      Color="#16161F"/>
  <SolidColorBrush x:Key="BrushBorder"     Color="#2A2A3A"/>

  <!-- Text -->
  <SolidColorBrush x:Key="BrushTextPrimary"   Color="#D8D8E8"/>
  <SolidColorBrush x:Key="BrushTextSecondary" Color="#8888AA"/>
  <SolidColorBrush x:Key="BrushTextTertiary"  Color="#555570"/>
  <SolidColorBrush x:Key="BrushAccent"        Color="#5B9BFF"/>

  <!-- Station kind tints (very dark, used as row backgrounds) -->
  <SolidColorBrush x:Key="BrushKindOrchestrator" Color="#180808"/>
  <SolidColorBrush x:Key="BrushKindFlow"         Color="#081808"/>
  <SolidColorBrush x:Key="BrushKindUnit"         Color="#080818"/>
  <SolidColorBrush x:Key="BrushKindRegistry"     Color="#181808"/>

  <!-- Status -->
  <SolidColorBrush x:Key="BrushSuccess" Color="#3DDDAF"/>
  <SolidColorBrush x:Key="BrushWarning" Color="#FF9933"/>
  <SolidColorBrush x:Key="BrushError"   Color="#FF4444"/>
</ResourceDictionary>

<!-- Typography.xaml -->
<ResourceDictionary>
  <FontFamily x:Key="FontPrimary">Segoe UI</FontFamily>
  <FontFamily x:Key="FontMono">Cascadia Code, Consolas, Courier New</FontFamily>
</ResourceDictionary>
```

---

## Implementation phase plan

Each phase is a single executor session. Gate B for every phase is:
  dotnet build LlmInspector.sln — zero errors
  dotnet test (if tests added) — zero failures

Phase 1  — Solution scaffold: LlmInspector.csproj, App.xaml, MainWindow.xaml (empty shell),
            HelixToolkit.Wpf and CommunityToolkit.Mvvm NuGet packages, project reference
            to 21.AgentInfrastructure. Gate B: solution builds, blank window opens.

Phase 2  — Data layer: StationTrace.cs, PropertyNode.cs, StationKind enum,
            PropertyNodeSerialiser with unit tests for LogicalDomainEntry serialisation.
            Gate B: serialiser tests pass, record types compile cleanly.

Phase 3  — InspectorSession: TracingLogicalTopologyRegistry decorator wrapping
            InMemoryLogicalTopologyRegistry. Emits StationTrace on every registry call.
            Gate B: integration test that fires AddOrUpdateDomain and captures one trace.

Phase 4  — TracingTopologyMutationFlow decorator and InspectorSession wiring.
            Gate B: RunAsync fires a domain command and produces a list of 3 traces
            (orchestrator, flow, unit). No UI yet.

Phase 5  — TopologySceneBuilder: builds Model3DGroup from registry state.
            Gate B: builder accepts an empty registry and produces a non-null group.
            Builder accepts registry with one domain and produces a sphere geometry.

Phase 6  — TopologyViewport.xaml + HelixViewport3D integration. MainWindow shell
            with left column showing the 3D scene. Camera orbit and zoom working.
            Gate B: window opens, empty scene visible, camera moves on drag.

Phase 7  — Scene update from registry state. After running a domain command, the
            domain sphere appears in the viewport.
            Gate B: fire add-domain command, observe sphere appears without restarting.

Phase 8  — SignalTracePanel.xaml: ListView with DataTemplate, bound to Traces collection.
            Gate B: fire a command, trace rows appear in centre panel.

Phase 9  — PropertyNodeView.xaml recursive tree control.
            StationInspector.xaml with input and output sections.
            Gate B: click a trace row, inspector shows the full property tree.

Phase 10 — Pulse animation: active node highlighting with 400 ms per station replay.
            Gate B: after a command runs, each node lights up in sequence in the 3D view.

Phase 11 — Hit-testing: click a domain sphere in 3D view, inspector opens that node.
            Gate B: click a sphere, station inspector shows that domain's registry entry.

Phase 12 — Command input bar: ComboBox for command type (AddDomain, AddPhase,
            AddSubPhase, AddTransition), TextBox for JSON input or guided form fields,
            Run button wired to RunCommand.
            Gate B: full end-to-end: type a domain id and name, click Run, see the trace
            and the sphere appear.
