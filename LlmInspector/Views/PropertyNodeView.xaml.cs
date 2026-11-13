// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Views;

using System.Windows.Controls;

// This control recursively renders one property node and all child nodes.
public partial class PropertyNodeView : UserControl
{
    // Create the property-node view and load the recursive XAML template.
    public PropertyNodeView()
    {
        InitializeComponent();
    }
}
