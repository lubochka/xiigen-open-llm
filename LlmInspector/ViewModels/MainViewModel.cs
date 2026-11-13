// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.ViewModels;

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

// This view model drives the live inspector trace shown in the shell.
public sealed class MainViewModel : INotifyPropertyChanged
{
    // Startup gets a smaller budget because opening the window must stay lightweight.
    private const int StartupRunTimeoutSeconds = 2;

    // Command-bar runs get a short budget until the inspector proves larger runs are cheap.
    private const int CommandRunTimeoutSeconds = 3;

    // Trace count is capped so a bad flow cannot grow the tree and topology scene without bound.
    private const int MaxTraceEventsPerRun = 24;

    // The inspector session emits station traces from the real topology mutation path.
    private readonly InspectorSession _session = new();

    // The command object lets the footer run typed topology mutations.
    private readonly InspectorCommand _runCommand;

    // The stop command cancels the active bounded run.
    private readonly InspectorCommand _stopCommand;

    // This flag prevents the sample command from running twice if the window reloads.
    private bool _sampleRunStarted;

    // This flag prevents overlapping command-bar runs.
    private bool _isRunning;

    // This source cancels the current command-bar run when time or trace budgets are exceeded.
    private CancellationTokenSource? _activeRunCancellation;

    // This counter tracks the number of trace events emitted during the current bounded run.
    private int _activeRunTraceCount;

    // This flag records whether the trace budget stopped the current run.
    private bool _traceBudgetExceeded;

    // The selected station drives the right-side inspector panel.
    private StationCardViewModel? _selectedStation;

    // The selected command type controls which topology command is built from the form.
    private string _selectedCommandType = "add domain";

    // This field stores the stable id entered by the user.
    private string _commandId = "qa_automation";

    // This field stores the human-readable topology name entered by the user.
    private string _commandName = "QA Automation";

    // This field stores the human-readable purpose entered by the user.
    private string _commandPurpose = "Owns automated validation and regression flow orchestration.";

    // This field stores the parent domain id used by phase and sub-phase commands.
    private string _parentDomainId = "implementation";

    // This field stores the parent phase id used by sub-phase commands.
    private string _parentPhaseId = "technical_design_phase";

    // This field stores the source topology kind for transition commands.
    private string _fromTopologyKind = "phase";

    // This field stores the source topology id for transition commands.
    private string _fromTopologyId = "technical_design_phase";

    // This field stores the target topology kind for transition commands.
    private string _toTopologyKind = "subphase";

    // This field stores the target topology id for transition commands.
    private string _toTopologyId = "architecture_decisions";

    // This field stores comma-separated predicate ids for the generated topology command.
    private string _predicateIdsText = "DECLARE,VALIDATE";

    // This field stores comma-separated constraint ids for transition commands.
    private string _constraintIdsText = "CONTEXT_CARRY_FORWARD";

    // This field stores comma-separated source ids for the generated topology command.
    private string _sourceIdsText = "COMMAND-BAR";

    // This field stores the audit reason for the command-bar run.
    private string _reason = "Command bar topology mutation.";

    // This field stores the latest command-bar run status.
    private string _commandStatus = "ready";

    // This field stores the real default flag surface shown in the command bar.
    private string _appliedFlagsText = BuildFlagSurface(new LogicalExecutionFlags());

    // Create the view model and connect the session trace stream to the tree.
    public MainViewModel()
    {
        // Create the run command after fields have default values.
        _runCommand = new InspectorCommand(RunCommandBarAsync, () => !IsRunning);

        // Create the stop command after the run state exists.
        _stopCommand = new InspectorCommand(StopActiveRunAsync, () => IsRunning);

        // Subscribe once so every emitted trace updates the bound tree.
        _session.StationTraced += OnStationTraced;
    }

    // Tree is bound by the trace tree panel in the centre column.
    public StationTraceTree Tree { get; } = new();

    // CommandTypes lists the typed topology commands currently supported by the command bar.
    public IReadOnlyList<string> CommandTypes { get; } = ["add domain", "add phase", "add sub-phase", "add transition"];

    // PropertyChanged notifies the shell when bound values change.
    public event PropertyChangedEventHandler? PropertyChanged;

    // StationTraced forwards every raw trace event to secondary visual observers.
    public event EventHandler<StationTrace>? StationTraced;

    // RunCommand executes the command-bar mutation through the real inspector session.
    public ICommand RunCommand => _runCommand;

    // StopCommand cancels the currently active bounded run.
    public ICommand StopCommand => _stopCommand;

    // RunBudgetText shows the currently enforced bounded-run policy.
    public string RunBudgetText => $"budget {CommandRunTimeoutSeconds}s / {MaxTraceEventsPerRun} trace events";

    // SelectedStation is the station currently shown in the inspector panel.
    public StationCardViewModel? SelectedStation
    {
        // Return the currently selected station.
        get => _selectedStation;

        // Store the selected station and notify the inspector binding.
        private set
        {
            // Do nothing when the same station is selected again.
            if (ReferenceEquals(_selectedStation, value))
                return;

            // Store the new selected station.
            _selectedStation = value;

            // Notify bindings that the inspector target changed.
            OnPropertyChanged(nameof(SelectedStation));
        }
    }

    // SelectedCommandType controls which typed topology command the footer builds.
    public string SelectedCommandType
    {
        // Return the current command type.
        get => _selectedCommandType;

        // Store the selected command type.
        set => SetField(ref _selectedCommandType, value, nameof(SelectedCommandType));
    }

    // CommandId is the stable topology id typed into the command bar.
    public string CommandId
    {
        // Return the current topology id.
        get => _commandId;

        // Store the current topology id.
        set => SetField(ref _commandId, value, nameof(CommandId));
    }

    // CommandName is the readable topology name typed into the command bar.
    public string CommandName
    {
        // Return the current topology name.
        get => _commandName;

        // Store the current topology name.
        set => SetField(ref _commandName, value, nameof(CommandName));
    }

    // CommandPurpose is the topology purpose typed into the command bar.
    public string CommandPurpose
    {
        // Return the current topology purpose.
        get => _commandPurpose;

        // Store the current topology purpose.
        set => SetField(ref _commandPurpose, value, nameof(CommandPurpose));
    }

    // ParentDomainId is used by phase and sub-phase commands.
    public string ParentDomainId
    {
        // Return the current parent domain id.
        get => _parentDomainId;

        // Store the current parent domain id.
        set => SetField(ref _parentDomainId, value, nameof(ParentDomainId));
    }

    // ParentPhaseId is used by sub-phase commands.
    public string ParentPhaseId
    {
        // Return the current parent phase id.
        get => _parentPhaseId;

        // Store the current parent phase id.
        set => SetField(ref _parentPhaseId, value, nameof(ParentPhaseId));
    }

    // FromTopologyKind is used by transition commands.
    public string FromTopologyKind
    {
        // Return the current source topology kind.
        get => _fromTopologyKind;

        // Store the current source topology kind.
        set => SetField(ref _fromTopologyKind, value, nameof(FromTopologyKind));
    }

    // FromTopologyId is used by transition commands.
    public string FromTopologyId
    {
        // Return the current source topology id.
        get => _fromTopologyId;

        // Store the current source topology id.
        set => SetField(ref _fromTopologyId, value, nameof(FromTopologyId));
    }

    // ToTopologyKind is used by transition commands.
    public string ToTopologyKind
    {
        // Return the current target topology kind.
        get => _toTopologyKind;

        // Store the current target topology kind.
        set => SetField(ref _toTopologyKind, value, nameof(ToTopologyKind));
    }

    // ToTopologyId is used by transition commands.
    public string ToTopologyId
    {
        // Return the current target topology id.
        get => _toTopologyId;

        // Store the current target topology id.
        set => SetField(ref _toTopologyId, value, nameof(ToTopologyId));
    }

    // PredicateIdsText is parsed into generated predicate ids.
    public string PredicateIdsText
    {
        // Return the current predicate id list.
        get => _predicateIdsText;

        // Store the current predicate id list.
        set => SetField(ref _predicateIdsText, value, nameof(PredicateIdsText));
    }

    // ConstraintIdsText is parsed into generated transition constraint ids.
    public string ConstraintIdsText
    {
        // Return the current constraint id list.
        get => _constraintIdsText;

        // Store the current constraint id list.
        set => SetField(ref _constraintIdsText, value, nameof(ConstraintIdsText));
    }

    // SourceIdsText is parsed into generated source ids.
    public string SourceIdsText
    {
        // Return the current source id list.
        get => _sourceIdsText;

        // Store the current source id list.
        set => SetField(ref _sourceIdsText, value, nameof(SourceIdsText));
    }

    // Reason is the audit reason sent with the generated topology command.
    public string Reason
    {
        // Return the current audit reason.
        get => _reason;

        // Store the current audit reason.
        set => SetField(ref _reason, value, nameof(Reason));
    }

    // CommandStatus reports the latest command-bar result.
    public string CommandStatus
    {
        // Return the current command status.
        get => _commandStatus;

        // Store the current command status.
        private set => SetField(ref _commandStatus, value, nameof(CommandStatus));
    }

    // AppliedFlagsText displays the full current LogicalExecutionFlags surface.
    public string AppliedFlagsText
    {
        // Return the current flag display.
        get => _appliedFlagsText;

        // Store the current flag display.
        private set => SetField(ref _appliedFlagsText, value, nameof(AppliedFlagsText));
    }

    // IsRunning disables the run button while a command is in flight.
    public bool IsRunning
    {
        // Return whether a command is running.
        get => _isRunning;

        // Store the running state and update command availability.
        private set
        {
            // Do nothing when the value is unchanged.
            if (_isRunning == value)
                return;

            // Store the new running state.
            _isRunning = value;

            // Notify bindings that the running state changed.
            OnPropertyChanged(nameof(IsRunning));

            // Notify the command bar that the Run button availability changed.
            _runCommand.RaiseCanExecuteChanged();

            // Notify the command bar that the Stop button availability changed.
            _stopCommand.RaiseCanExecuteChanged();
        }
    }

    // RunSampleAsync executes one tiny topology command so startup stays bounded.
    public async Task RunSampleAsync()
    {
        // Do nothing when the sample has already been executed for this window.
        if (_sampleRunStarted)
            return;

        // Mark the sample as started before awaiting to avoid duplicate runs.
        _sampleRunStarted = true;

        // Build one deterministic add-domain command for the initial visual inspection.
        var command = new AddDomainCommand(
            new LogicalDomain(
                "technical_design",
                "Technical Design",
                "Owns technical design orchestration.",
                ["DECLARE"],
                ["STARTUP-SMOKE"]),
            "Startup smoke command.");

        try
        {
            // Create a short startup stopper so the inspector cannot hang during window opening.
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(StartupRunTimeoutSeconds));

            // Execute one command only so the inspector opens without a heavy automatic trace run.
            _ = await _session.RunTopologyCommandAsync(command, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Keep the window open even if the startup smoke run exceeds its tiny budget.
            CommandStatus = "startup smoke stopped";
        }
    }

    // SelectStation is called by a station card click.
    public void SelectStation(StationCardViewModel station)
    {
        // Store the clicked station as the current inspector target.
        SelectedStation = station;
    }

    // RunCommandBarAsync builds and executes one command from the footer fields.
    private async Task RunCommandBarAsync()
    {
        // Do nothing when another command is already running.
        if (IsRunning)
            return;

        // Create a bounded cancellation source for this one command-bar run.
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(CommandRunTimeoutSeconds));

        // Store the active cancellation source so the Stop button can cancel it.
        _activeRunCancellation = cancellation;

        // Reset the trace counter for this bounded run.
        _activeRunTraceCount = 0;

        // Reset the trace-budget flag for this bounded run.
        _traceBudgetExceeded = false;

        // Mark the command as running before awaiting the session.
        IsRunning = true;

        // Display the real default flags applied by the current session path.
        AppliedFlagsText = BuildFlagSurface(new LogicalExecutionFlags());

        // Show that the command has started with its active budget.
        CommandStatus = "running " + SelectedCommandType + " (" + RunBudgetText + ")";

        try
        {
            // Build the typed topology command from the current form values.
            var command = BuildCommandFromForm();

            // Execute the typed command through the same traced runtime path as the sample.
            var result = await _session.RunTopologyCommandAsync(command, cancellation.Token);

            // Prefer the trace-budget status when that guard fired.
            if (_traceBudgetExceeded)
                CommandStatus = "stopped: trace budget";
            // Prefer the time/manual status when the cancellation source was requested.
            else if (cancellation.IsCancellationRequested)
                CommandStatus = "stopped: time or manual";
            // Store the visible command status after successful completion.
            else
                CommandStatus = result.IsSuccess ? "ok: " + result.Value!.OverallVerdict : "error: " + result.ErrorCode;
        }
        catch (OperationCanceledException)
        {
            // Show a bounded-run stop message when the session observes cancellation by throwing.
            CommandStatus = _traceBudgetExceeded ? "stopped: trace budget" : "stopped: time or manual";
        }
        finally
        {
            // Clear the active source so future Stop clicks do not touch a disposed source.
            _activeRunCancellation = null;

            // Mark the command as idle after the trace has been emitted or stopped.
            IsRunning = false;
        }
    }

    // StopActiveRunAsync cancels the current bounded run from the command bar.
    private Task StopActiveRunAsync()
    {
        // Request cancellation only when a command is currently active.
        _activeRunCancellation?.Cancel();

        // Show an immediate stopping status while the active await unwinds.
        CommandStatus = "stopping";

        // Return a completed task because cancellation itself is synchronous.
        return Task.CompletedTask;
    }

    // BuildCommandFromForm creates one typed topology command from the current footer fields.
    private LogicalTopologyCommand BuildCommandFromForm()
    {
        // Route to the add-phase builder when the selector asks for a phase.
        if (string.Equals(SelectedCommandType, "add phase", StringComparison.Ordinal))
            return BuildAddPhaseCommand();

        // Route to the add-sub-phase builder when the selector asks for a sub-phase.
        if (string.Equals(SelectedCommandType, "add sub-phase", StringComparison.Ordinal))
            return BuildAddSubPhaseCommand();

        // Route to the add-transition builder when the selector asks for a transition.
        if (string.Equals(SelectedCommandType, "add transition", StringComparison.Ordinal))
            return BuildAddTransitionCommand();

        // Default to add-domain so an unknown selector value still produces a safe topology mutation.
        return BuildAddDomainCommand();
    }

    // BuildAddDomainCommand creates one add-domain topology command.
    private AddDomainCommand BuildAddDomainCommand()
    {
        // Create the domain from the form values.
        var domain = new LogicalDomain(
            NonEmpty(CommandId, "new_domain"),
            NonEmpty(CommandName, NonEmpty(CommandId, "New Domain")),
            NonEmpty(CommandPurpose, "Domain added from the inspector command bar."),
            ParseCsv(PredicateIdsText),
            ParseCsv(SourceIdsText));

        // Return the typed domain mutation command.
        return new AddDomainCommand(domain, NonEmpty(Reason, "Inspector command bar add-domain."));
    }

    // BuildAddPhaseCommand creates one add-phase topology command.
    private AddPhaseCommand BuildAddPhaseCommand()
    {
        // Create the phase from the form values.
        var phase = new LogicalPhase(
            NonEmpty(CommandId, "new_phase"),
            NonEmpty(CommandName, NonEmpty(CommandId, "New Phase")),
            NonEmpty(CommandPurpose, "Phase added from the inspector command bar."),
            [NonEmpty(ParentDomainId, "technical_design")],
            ParseCsv(PredicateIdsText),
            ParseCsv(SourceIdsText));

        // Return the typed phase mutation command.
        return new AddPhaseCommand(phase, NonEmpty(Reason, "Inspector command bar add-phase."));
    }

    // BuildAddSubPhaseCommand creates one add-sub-phase topology command.
    private AddSubPhaseCommand BuildAddSubPhaseCommand()
    {
        // Create the sub-phase from the form values.
        var subPhase = new LogicalSubPhase(
            NonEmpty(CommandId, "new_sub_phase"),
            NonEmpty(ParentPhaseId, "technical_design_phase"),
            NonEmpty(CommandName, NonEmpty(CommandId, "New Sub Phase")),
            NonEmpty(CommandPurpose, "Sub-phase added from the inspector command bar."),
            [NonEmpty(ParentDomainId, "technical_design")],
            ParseCsv(PredicateIdsText),
            ParseCsv(SourceIdsText));

        // Return the typed sub-phase mutation command.
        return new AddSubPhaseCommand(subPhase, NonEmpty(Reason, "Inspector command bar add-sub-phase."));
    }

    // BuildAddTransitionCommand creates one add-transition topology command.
    private AddTransitionCommand BuildAddTransitionCommand()
    {
        // Create the transition from the form values.
        var transition = new LogicalTransition(
            NonEmpty(CommandId, "new_transition"),
            NonEmpty(FromTopologyKind, "phase"),
            NonEmpty(FromTopologyId, "technical_design_phase"),
            NonEmpty(ToTopologyKind, "subphase"),
            NonEmpty(ToTopologyId, "architecture_decisions"),
            NonEmpty(CommandPurpose, "Transition added from the inspector command bar."),
            ParseCsv(PredicateIdsText),
            ParseCsv(ConstraintIdsText),
            ParseCsv(SourceIdsText));

        // Return the typed transition mutation command.
        return new AddTransitionCommand(transition, NonEmpty(Reason, "Inspector command bar add-transition."));
    }

    // OnStationTraced applies each trace event to the bound tree.
    private void OnStationTraced(object? sender, StationTrace trace)
    {
        // Count traces only during explicit command-bar runs.
        if (IsRunning)
            _activeRunTraceCount++;

        // Stop the active run when the trace count exceeds the bounded-run guard.
        if (IsRunning && _activeRunTraceCount >= MaxTraceEventsPerRun)
        {
            // Record that the trace guard was the reason for stopping.
            _traceBudgetExceeded = true;

            // Request cancellation from the active run.
            _activeRunCancellation?.Cancel();

            // Surface the guard status immediately.
            CommandStatus = "stopping: trace budget";
        }

        // Upsert the trace so start and completion events share one card.
        Tree.Upsert(trace);

        // Forward the same trace to observers such as the topology viewport.
        StationTraced?.Invoke(this, trace);
    }

    // SetField stores one property value and raises the matching notification.
    private void SetField(ref string field, string value, string propertyName)
    {
        // Do nothing when the value is unchanged.
        if (string.Equals(field, value, StringComparison.Ordinal))
            return;

        // Store the new value.
        field = value;

        // Notify bindings that the property changed.
        OnPropertyChanged(propertyName);
    }

    // OnPropertyChanged raises one property notification.
    private void OnPropertyChanged(string propertyName)
    {
        // Notify subscribers that the named property changed.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // NonEmpty returns a trimmed value or a safe fallback.
    private static string NonEmpty(string value, string fallback)
    {
        // Trim the input value before testing it.
        var trimmed = value.Trim();

        // Return the fallback when the input is blank.
        if (string.IsNullOrWhiteSpace(trimmed))
            return fallback;

        // Return the trimmed input value.
        return trimmed;
    }

    // ParseCsv converts a comma-separated field into stable identifiers.
    private static string[] ParseCsv(string value)
    {
        // Split the input on commas.
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    // BuildFlagSurface formats the real LogicalExecutionFlags boolean surface.
    private static string BuildFlagSurface(LogicalExecutionFlags flags)
    {
        // Read every public boolean flag property in declaration-independent order.
        var flagParts = typeof(LogicalExecutionFlags)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.PropertyType == typeof(bool))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => property.Name + "=" + property.GetValue(flags));

        // Return the compact flag string shown in the command bar.
        return string.Join("  ", flagParts);
    }

    // InspectorCommand adapts an async view-model method to WPF ICommand.
    private sealed class InspectorCommand : ICommand
    {
        // This delegate runs the command work.
        private readonly Func<Task> _execute;

        // This delegate reports whether the command can run.
        private readonly Func<bool> _canExecute;

        // Create a command from executable delegates.
        public InspectorCommand(Func<Task> execute, Func<bool> canExecute)
        {
            // Store the execution delegate.
            _execute = execute;

            // Store the availability delegate.
            _canExecute = canExecute;
        }

        // CanExecuteChanged notifies WPF that button availability changed.
        public event EventHandler? CanExecuteChanged;

        // CanExecute returns whether the command can run now.
        public bool CanExecute(object? parameter)
        {
            // Return the delegate result.
            return _canExecute();
        }

        // Execute starts the async command work.
        public async void Execute(object? parameter)
        {
            // Await the command work so exceptions stay in the UI async path.
            await _execute();
        }

        // RaiseCanExecuteChanged refreshes the bound Run button availability.
        public void RaiseCanExecuteChanged()
        {
            // Notify WPF subscribers that command availability changed.
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
