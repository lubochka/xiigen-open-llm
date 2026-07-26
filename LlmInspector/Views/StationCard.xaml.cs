// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Views;

using LlmInspector.Inspector;
using LlmInspector.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// This control renders one station card and recursively renders its child wave groups.
public partial class StationCard : UserControl
{
    // Create the station card and load its recursive XAML template.
    public StationCard()
    {
        InitializeComponent();
    }

    // Card_Clicked selects this station for the right-side inspector panel.
    private void Card_Clicked(object sender, MouseButtonEventArgs e)
    {
        // Stop parent station cards from also handling this nested click.
        e.Handled = true;

        // Continue only when this control is bound to a station card.
        if (DataContext is not StationCardViewModel station)
            return;

        // Find the containing window so the click can reach the main view model.
        var window = Window.GetWindow(this);

        // Continue only when the window is bound to the inspector view model.
        if (window?.DataContext is not MainViewModel viewModel)
            return;

        // Select the clicked station in the main view model.
        viewModel.SelectStation(station);
    }
}
