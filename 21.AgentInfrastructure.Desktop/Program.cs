// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Windows.Forms;

namespace AgentInfrastructure.Desktop;

// ─── Program.cs — Windows Forms entry point ─────────────────────────────────
// This is the executable's startup. It tells Windows Forms to use modern
// visual styles and high-DPI settings, then runs the main form on the UI thread.

internal static class Program
{
    // The [STAThread] attribute is required by Windows Forms for clipboard,
    // drag-drop, and OLE — UI code must run on a single-threaded apartment thread.
    [STAThread]
    private static void Main()
    {
        // Opt in to per-monitor v2 high-DPI scaling; modern font rendering, etc.
        ApplicationConfiguration.Initialize();

        // Open the main window and pump the message loop until the user closes it.
        Application.Run(new MainForm());
    }
}
