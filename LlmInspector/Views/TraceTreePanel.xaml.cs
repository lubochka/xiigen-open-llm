// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Views;

using System.Windows.Controls;

// This control renders the recursive station trace tree in the centre panel.
public partial class TraceTreePanel : UserControl
{
    // Create the panel and load its XAML-defined bindings.
    public TraceTreePanel()
    {
        InitializeComponent();
    }
}
