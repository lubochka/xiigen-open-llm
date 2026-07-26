// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Views;

using System.Windows.Controls;

// This control shows the selected station input and output property trees.
public partial class StationInspector : UserControl
{
    // Create the inspector and load its XAML bindings.
    public StationInspector()
    {
        InitializeComponent();
    }
}
