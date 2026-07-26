// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

// This observer renders generated topology state with lightweight WPF canvas shapes.
public sealed class TopologySceneObserver
{
    // This brush renders the viewport background without involving a 3D device.
    private static readonly Brush ViewportBackground = BrushFromRgb(0x0A, 0x10, 0x18);

    // This brush renders domain nodes as the most important topology level.
    private static readonly Brush DomainFill = BrushFromRgb(0x24, 0x62, 0xA3);

    // This brush renders phase nodes as the second topology level.
    private static readonly Brush PhaseFill = BrushFromRgb(0x1D, 0x84, 0x65);

    // This brush renders sub-phase nodes as compact child topology units.
    private static readonly Brush SubPhaseFill = BrushFromRgb(0x66, 0x54, 0xA8);

    // This brush renders topology transition edges.
    private static readonly Brush TransitionStroke = BrushFromRgb(0xD8, 0x92, 0x38);

    // This brush renders node borders in the dark inspection viewport.
    private static readonly Brush NodeBorder = BrushFromRgb(0x63, 0x76, 0x88);

    // This brush renders readable text on every topology node.
    private static readonly Brush TextBrush = BrushFromRgb(0xEC, 0xF4, 0xFF);

    // This brush renders secondary text on compact topology nodes.
    private static readonly Brush MutedTextBrush = BrushFromRgb(0x9B, 0xAD, 0xBD);

    // The WPF canvas receives cheap 2D visuals as station outputs arrive.
    private readonly Canvas _surface;

    // The tested topology state owns generated nodes, edges, and out-of-order buffers.
    private readonly GeneratedTopologyState _topology = new();

    // Rendered node positions are recalculated whenever the scene changes.
    private readonly Dictionary<string, Point> _positions = new(StringComparer.Ordinal);

    // The latest immutable snapshot is used by the renderer.
    private GeneratedTopologySnapshot _snapshot = new([], [], 0, 0, 0);

    // Detail visibility decides whether generated phase branches are open in the 2D scene.
    private bool _detailVisible;

    // Create an observer that appends generated topology visuals to a lightweight canvas.
    public TopologySceneObserver(Canvas surface)
    {
        // Store the canvas so generated visuals can be drawn without registry reads.
        _surface = surface;

        // Paint the canvas directly so the viewport remains readable before traces arrive.
        _surface.Background = ViewportBackground;
    }

    // ShowOverview collapses generated topology details to domain-level cards.
    public void ShowOverview()
    {
        // Store the overview mode so later generated nodes follow the same visibility.
        _detailVisible = false;

        // Repaint the scene in overview mode.
        RebuildScene();
    }

    // ShowDetail opens the generated topology branch under each visible domain.
    public void ShowDetail()
    {
        // Store the detail mode so later generated nodes follow the same visibility.
        _detailVisible = true;

        // Repaint the scene with generated children visible.
        RebuildScene();
    }

    // FitScene recalculates the cheap 2D layout for the current canvas size.
    public void FitScene()
    {
        // Repaint the generated scene inside the current canvas bounds.
        RebuildScene();
    }

    // React to one completed station trace.
    public void OnTraceCompleted(StationTrace trace)
    {
        // Apply only completed generated output to the tested topology state.
        if (!_topology.ApplyTrace(trace))
            return;

        // Store the latest immutable snapshot for rendering.
        _snapshot = _topology.Snapshot();

        // Repaint the canvas from the tested generated topology state.
        RebuildScene();
    }

    // RebuildScene redraws the whole cheap canvas from generated topology state.
    private void RebuildScene()
    {
        // Clear the current shapes before drawing the latest generated state.
        _surface.Children.Clear();

        // Clear computed positions because layout depends on current canvas size.
        _positions.Clear();

        // Draw the empty-state message when no generated topology has arrived yet.
        if (_snapshot.Nodes.Count == 0)
        {
            // Add a centered empty-state label.
            DrawEmptyState();

            // Stop after drawing the empty state.
            return;
        }

        // Compute node positions for the current detail mode.
        ComputeLayout();

        // Draw transitions behind nodes so labels remain readable.
        DrawTransitions();

        // Draw generated topology nodes above transition lines.
        DrawNodes();
    }

    // ComputeLayout assigns visible nodes to simple rows and columns.
    private void ComputeLayout()
    {
        // Read available canvas width with a small fallback for early layout passes.
        var width = Math.Max(420.0, _surface.ActualWidth);

        // Place domain nodes in a stable top row.
        var domains = _snapshot.Nodes.Where(node => node.Kind == "domain").OrderBy(node => node.Id, StringComparer.Ordinal).ToArray();

        // Position every domain in evenly spaced slots.
        for (var index = 0; index < domains.Length; index++)
            _positions[domains[index].Key] = RowPoint(index, domains.Length, width, 95.0);

        // Stop at domain-level layout when details are collapsed.
        if (!_detailVisible)
            return;

        // Position every phase under its parent domain.
        foreach (var domain in domains)
            ComputeChildrenLayout(domain, width, childY: 175.0, childKind: "phase");

        // Position every sub-phase under its parent phase.
        foreach (var phase in _snapshot.Nodes.Where(node => node.Kind == "phase").OrderBy(node => node.Id, StringComparer.Ordinal))
            ComputeChildrenLayout(phase, width, childY: 250.0, childKind: "subphase");
    }

    // ComputeChildrenLayout places children under one parent in a compact horizontal group.
    private void ComputeChildrenLayout(GeneratedTopologyNode parent, double width, double childY, string childKind)
    {
        // Skip parents that are not visible in the current layout.
        if (!_positions.TryGetValue(parent.Key, out var parentPoint))
            return;

        // Read visible children for this parent and kind.
        var children = _snapshot.Nodes
            .Where(node => node.Kind == childKind && string.Equals(node.ParentKey, parent.Key, StringComparison.Ordinal))
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .ToArray();

        // Stop when this parent has no generated children of the requested kind.
        if (children.Length == 0)
            return;

        // Keep child groups within the canvas by capping their horizontal spread.
        var spread = Math.Min(width * 0.45, Math.Max(120.0, children.Length * 88.0));

        // Compute the left edge of the child group around the parent.
        var startX = parentPoint.X - spread / 2.0;

        // Position every generated child in a stable slot.
        for (var index = 0; index < children.Length; index++)
        {
            // Compute the normalized child slot inside the group.
            var ratio = children.Length == 1 ? 0.5 : index / (double)(children.Length - 1);

            // Clamp the child position so labels do not leave the viewport.
            var x = Math.Clamp(startX + spread * ratio, 72.0, width - 72.0);

            // Store the computed child position.
            _positions[children[index].Key] = new Point(x, childY);
        }
    }

    // RowPoint places one item in a horizontal row.
    private static Point RowPoint(int index, int total, double width, double y)
    {
        // Reserve edge padding so node labels are not clipped.
        const double padding = 92.0;

        // Compute the usable row width.
        var usableWidth = Math.Max(1.0, width - padding * 2.0);

        // Center a single item.
        if (total <= 1)
            return new Point(width / 2.0, y);

        // Compute the normalized position across the usable row.
        var ratio = index / (double)(total - 1);

        // Return the item point inside the padded row.
        return new Point(padding + usableWidth * ratio, y);
    }

    // DrawTransitions renders generated edges that are visible in the current mode.
    private void DrawTransitions()
    {
        // Hide transition edges while details are collapsed.
        if (!_detailVisible)
            return;

        // Draw each generated transition whose endpoints are visible.
        foreach (var transition in _snapshot.Transitions)
        {
            // Skip transitions whose source node is not visible.
            if (!_positions.TryGetValue(transition.FromKey, out var from))
                continue;

            // Skip transitions whose target node is not visible.
            if (!_positions.TryGetValue(transition.ToKey, out var to))
                continue;

            // Create a cheap WPF line for the generated edge.
            var line = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = TransitionStroke,
                StrokeThickness = 1.4,
                Opacity = 0.75,
                SnapsToDevicePixels = true
            };

            // Add the transition line behind node cards.
            _surface.Children.Add(line);
        }
    }

    // DrawNodes renders every visible generated topology node.
    private void DrawNodes()
    {
        // Draw generated nodes in snapshot order for predictable visual output.
        foreach (var node in _snapshot.Nodes)
        {
            // Skip nodes hidden by overview mode.
            if (!_positions.TryGetValue(node.Key, out var point))
                continue;

            // Draw one compact card for this generated node.
            DrawNodeCard(node, point);
        }
    }

    // DrawNodeCard draws one generated topology node as a small readable card.
    private void DrawNodeCard(GeneratedTopologyNode node, Point center)
    {
        // Choose the node size from its topology kind.
        var size = NodeSize(node.Kind);

        // Build the card shell for this generated node.
        var card = new Border
        {
            Width = size.Width,
            Height = size.Height,
            CornerRadius = new CornerRadius(7),
            Background = NodeFill(node.Kind),
            BorderBrush = NodeBorder,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8, 4, 8, 4),
            SnapsToDevicePixels = true
        };

        // Build the vertical text stack inside the card.
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        // Add the node kind label so users see domain, phase, or sub-phase at a glance.
        stack.Children.Add(new TextBlock
        {
            Text = node.Kind.ToUpperInvariant(),
            Foreground = MutedTextBrush,
            FontSize = 8,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        // Add the generated node display name.
        stack.Children.Add(new TextBlock
        {
            Text = node.Name,
            Foreground = TextBrush,
            FontSize = node.Kind == "domain" ? 11 : 10,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        // Attach the text stack to the card.
        card.Child = stack;

        // Position the card around its computed center point.
        Canvas.SetLeft(card, center.X - size.Width / 2.0);

        // Position the card around its computed center point.
        Canvas.SetTop(card, center.Y - size.Height / 2.0);

        // Add the card to the canvas.
        _surface.Children.Add(card);
    }

    // DrawEmptyState gives the viewport a cheap idle state.
    private void DrawEmptyState()
    {
        // Read available canvas width with a fallback for early layout passes.
        var width = Math.Max(420.0, _surface.ActualWidth);

        // Read available canvas height with a fallback for early layout passes.
        var height = Math.Max(260.0, _surface.ActualHeight);

        // Create the empty-state text.
        var text = new TextBlock
        {
            Text = "topology appears as traces complete",
            Foreground = MutedTextBrush,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        };

        // Measure the text before centering it.
        text.Measure(new Size(width, height));

        // Center the empty-state label.
        Canvas.SetLeft(text, (width - text.DesiredSize.Width) / 2.0);

        // Center the empty-state label.
        Canvas.SetTop(text, (height - text.DesiredSize.Height) / 2.0);

        // Add the label to the viewport.
        _surface.Children.Add(text);
    }

    // NodeSize chooses a compact card size for each topology kind.
    private static Size NodeSize(string kind)
    {
        // Return a larger size for domains because they are the overview anchor.
        if (kind == "domain")
            return new Size(138.0, 42.0);

        // Return a medium size for phases.
        if (kind == "phase")
            return new Size(124.0, 38.0);

        // Return the smallest card for sub-phases.
        return new Size(112.0, 34.0);
    }

    // NodeFill chooses the fill brush for one generated topology kind.
    private static Brush NodeFill(string kind)
    {
        // Return the domain brush for domain nodes.
        if (kind == "domain")
            return DomainFill;

        // Return the phase brush for phase nodes.
        if (kind == "phase")
            return PhaseFill;

        // Return the sub-phase brush for all lower topology nodes.
        return SubPhaseFill;
    }

    // BrushFromRgb creates one frozen solid brush for repeated scene rendering.
    private static SolidColorBrush BrushFromRgb(byte red, byte green, byte blue)
    {
        // Build a WPF brush from the supplied RGB values.
        var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));

        // Freeze the brush so WPF can render it cheaply.
        brush.Freeze();

        // Return the reusable brush.
        return brush;
    }
}
