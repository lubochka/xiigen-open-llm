// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.Windows;

public partial class ApiDashboardWindow : Window
{
    public ApiDashboardWindow()
        : this(new ApiDashboardViewModel())
    {
    }

    public ApiDashboardWindow(ApiDashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
