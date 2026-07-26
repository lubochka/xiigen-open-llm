// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Windows;
using LlmInspector.ViewModels;

namespace LlmInspector;

/// <summary>
/// Provides the Phase 61 WPF shell for the logical orchestrator inspector.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Creates the shell and loads the XAML-defined panel structure.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Create the view model that owns the inspector session and trace tree.
        var viewModel = new MainViewModel();

        // Bind the shell to the inspector view model.
        DataContext = viewModel;

        // Run the sample command after the window loads so the centre panel has live station data.
        Loaded += async (_, _) => await viewModel.RunSampleAsync();
    }
}
