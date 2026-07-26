// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Views;

using LlmInspector.Inspector;
using LlmInspector.ViewModels;
using System.Windows;
using System.Windows.Controls;

// This control renders the generated topology scene with lightweight WPF drawing.
public partial class TopologyViewport : UserControl
{
    // The scene observer owns the generated topology scene state.
    private readonly TopologySceneObserver _observer;

    // The attached view model is kept so the event handler can be removed cleanly.
    private MainViewModel? _viewModel;

    // Create the viewport and attach the scene group.
    public TopologyViewport()
    {
        // Load the XAML viewport.
        InitializeComponent();

        // Create the observer after the lightweight canvas exists.
        _observer = new TopologySceneObserver(Surface);

        // Attach to the window view model when the control enters the visual tree.
        Loaded += OnLoaded;

        // Attach again when inherited DataContext arrives after the control load event.
        DataContextChanged += OnDataContextChanged;

        // Detach from the window view model when the control leaves the visual tree.
        Unloaded += OnUnloaded;

        // Redraw the lightweight scene when the canvas size changes.
        Surface.SizeChanged += OnSurfaceSizeChanged;
    }

    // OnLoaded subscribes to the main trace stream.
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Attach to the current view model when it is already available.
        AttachToViewModel();
    }

    // OnDataContextChanged attaches when the window assigns the inherited view model after XAML load.
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Attach to the current view model when it becomes available.
        AttachToViewModel();
    }

    // OnUnloaded unsubscribes from the main trace stream.
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Detach only when a view model was attached.
        if (_viewModel is not null)
            _viewModel.StationTraced -= OnStationTraced;

        // Clear the stored view model reference.
        _viewModel = null;
    }

    // OnSurfaceSizeChanged redraws the generated topology at the new canvas size.
    private void OnSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Ask the observer to redraw without changing generated state.
        _observer.FitScene();
    }

    // AttachToViewModel subscribes to the active main view model exactly once.
    private void AttachToViewModel()
    {
        // Find the owning window.
        var window = Window.GetWindow(this);

        // Continue only when the window has the expected view model.
        if (window?.DataContext is not MainViewModel viewModel)
            return;

        // Do nothing when this control is already attached to the same view model.
        if (ReferenceEquals(_viewModel, viewModel))
            return;

        // Detach from any prior view model before switching.
        if (_viewModel is not null)
            _viewModel.StationTraced -= OnStationTraced;

        // Store the view model so it can be detached later.
        _viewModel = viewModel;

        // Subscribe to raw station traces for topology scene updates.
        _viewModel.StationTraced += OnStationTraced;
    }

    // OnStationTraced forwards completed traces to the scene observer.
    private void OnStationTraced(object? sender, StationTrace trace)
    {
        // Ignore start traces because topology nodes are generated from completed outputs.
        if (trace.CompletedAt is null)
            return;

        // Let the observer inspect the completed output for topology entries.
        _observer.OnTraceCompleted(trace);
    }

    // Overview_Click collapses generated topology to the domain-level overview.
    private void Overview_Click(object sender, RoutedEventArgs e)
    {
        // Ask the observer to hide generated phase, sub-phase, and transition details.
        _observer.ShowOverview();
    }

    // OpenActive_Click expands generated topology details that arrived from completed traces.
    private void OpenActive_Click(object sender, RoutedEventArgs e)
    {
        // Ask the observer to show generated phase, sub-phase, and transition details.
        _observer.ShowDetail();
    }

    // Fit_Click redraws the currently visible generated topology inside the viewport.
    private void Fit_Click(object sender, RoutedEventArgs e)
    {
        // Ask the observer to fit the lightweight scene inside the visible surface.
        _observer.FitScene();
    }
}
