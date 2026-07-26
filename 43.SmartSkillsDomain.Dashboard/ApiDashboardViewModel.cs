// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

public sealed record ApiDashboardHistoryEntry(
    string MethodId,
    string HumanName,
    string Status,
    bool Success,
    string PromptSha256,
    string EvidenceScope,
    bool HostRuntimeProofClaimed,
    bool SemanticQualityOrTrainedModelProofClaimed);

public sealed class ApiDashboardViewModel : INotifyPropertyChanged
{
    private readonly ApiDashboardRunner _runner;
    private ApiDashboardCatalogRow? _selectedApi;
    private ApiDashboardFixtureExampleRow? _selectedFixture;
    private ApiDashboardRunResult? _lastRun;
    private ApiDashboardResultViewModel? _result;
    private string _inputText = string.Empty;
    private string _statusText = "ready";
    private string _footerStatusText = "Dashboard-to-core evidence only; host runtime and semantic quality proof stay separate.";
    private string _lastCopiedText = string.Empty;
    private string _lastExportText = string.Empty;
    private bool _isRawJsonRequested;
    private bool _isDebugRequested;

    public ApiDashboardViewModel()
        : this(new ApiDashboardRunner())
    {
    }

    public ApiDashboardViewModel(ApiDashboardRunner? runner)
    {
        _runner = runner ?? new ApiDashboardRunner();
        ApiRows = LoadCatalogRows();
        _selectedApi = ApiRows.FirstOrDefault(row => row.IsPrimaryUserFacingAction) ?? ApiRows.FirstOrDefault();
        UseExampleCommand = new DashboardCommand(UseSelectedExample, () => SelectedApi is not null);
        RunCommand = new DashboardCommand(RunSelectedApi, () => SelectedApi is not null);
        ToggleRawJsonCommand = new DashboardCommand(ToggleRawJson);
        ToggleDebugCommand = new DashboardCommand(ToggleDebug);
        CopyResultCommand = new DashboardCommand(CopyResultShell, () => Result is not null);
        ExportHistoryCommand = new DashboardCommand(ExportHistoryShell, () => History.Count > 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<ApiDashboardCatalogRow> ApiRows { get; }

    public ObservableCollection<ApiDashboardHistoryEntry> History { get; } = [];

    public ICommand UseExampleCommand { get; }

    public ICommand RunCommand { get; }

    public ICommand ToggleRawJsonCommand { get; }

    public ICommand ToggleDebugCommand { get; }

    public ICommand CopyResultCommand { get; }

    public ICommand ExportHistoryCommand { get; }

    public string EvidenceScopeText => ApiDashboardRunner.DashboardEvidenceScope;

    public ApiDashboardCatalogRow? SelectedApi
    {
        get => _selectedApi;
        set
        {
            if (ReferenceEquals(_selectedApi, value))
                return;

            _selectedApi = value;
            _selectedFixture = _selectedApi?.FixtureExamples.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedApi));
            OnPropertyChanged(nameof(SelectedApiName));
            OnPropertyChanged(nameof(SelectedApiDescription));
            OnPropertyChanged(nameof(SelectedExampleText));
            OnPropertyChanged(nameof(SelectedInputDescription));
            OnPropertyChanged(nameof(SelectedOutputPreview));
            OnPropertyChanged(nameof(SelectedSafetyBoundaryNote));
            OnPropertyChanged(nameof(SelectedFixtureExamples));
            OnPropertyChanged(nameof(SelectedFixture));
            OnPropertyChanged(nameof(SelectedFixtureName));
            OnPropertyChanged(nameof(SelectedFixtureExpectedResult));
            RaiseCommandAvailability();
        }
    }

    public string SelectedApiName => SelectedApi?.HumanName ?? "No API selected";

    public string SelectedApiDescription => SelectedApi?.HumanDescription ?? string.Empty;

    public string SelectedExampleText => SelectedFixture?.ExampleInputText ?? SelectedApi?.ExampleInputText ?? string.Empty;

    public string SelectedInputDescription => SelectedApi?.InputFieldDescription ?? string.Empty;

    public string SelectedOutputPreview => SelectedApi?.OutputPreview ?? string.Empty;

    public string SelectedSafetyBoundaryNote => SelectedApi?.SafetyBoundaryNote ?? string.Empty;

    public IReadOnlyList<ApiDashboardFixtureExampleRow> SelectedFixtureExamples => SelectedApi?.FixtureExamples ?? [];

    public ApiDashboardFixtureExampleRow? SelectedFixture
    {
        get => _selectedFixture;
        set
        {
            if (ReferenceEquals(_selectedFixture, value))
                return;

            _selectedFixture = value;
            OnPropertyChanged(nameof(SelectedFixture));
            OnPropertyChanged(nameof(SelectedFixtureName));
            OnPropertyChanged(nameof(SelectedFixtureExpectedResult));
            OnPropertyChanged(nameof(SelectedExampleText));
        }
    }

    public string SelectedFixtureName => SelectedFixture?.HumanName ?? "No fixture example selected";

    public string SelectedFixtureExpectedResult => SelectedFixture?.ExpectedVisibleResult ?? "No fixture preview is selected for this API.";

    public string InputText
    {
        get => _inputText;
        set => SetField(ref _inputText, value, nameof(InputText));
    }

    public bool IsRawJsonRequested
    {
        get => _isRawJsonRequested;
        set
        {
            if (_isRawJsonRequested == value)
                return;

            _isRawJsonRequested = value;
            OnPropertyChanged(nameof(IsRawJsonRequested));
            RefreshPresentedResult();
        }
    }

    public bool IsDebugRequested
    {
        get => _isDebugRequested;
        set
        {
            if (_isDebugRequested == value)
                return;

            _isDebugRequested = value;
            OnPropertyChanged(nameof(IsDebugRequested));
            RefreshPresentedResult();
        }
    }

    public ApiDashboardResultViewModel? Result
    {
        get => _result;
        private set
        {
            if (ReferenceEquals(_result, value))
                return;

            _result = value;
            OnPropertyChanged(nameof(Result));
            OnPropertyChanged(nameof(HumanResultText));
            OnPropertyChanged(nameof(VisibleErrorText));
            OnPropertyChanged(nameof(HasVisibleError));
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(RawJsonText));
            OnPropertyChanged(nameof(DebugTraceText));
            OnPropertyChanged(nameof(IsRawJsonVisible));
            OnPropertyChanged(nameof(IsDebugTraceVisible));
            RaiseCommandAvailability();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value, nameof(StatusText));
    }

    public string FooterStatusText
    {
        get => _footerStatusText;
        private set => SetField(ref _footerStatusText, value, nameof(FooterStatusText));
    }

    public string HumanResultText => Result?.HumanResult ?? string.Empty;

    public string VisibleErrorText => Result is null
        ? string.Empty
        : string.Join(Environment.NewLine, Result.Errors);

    public bool HasVisibleError => !string.IsNullOrWhiteSpace(VisibleErrorText);

    public bool HasWarnings => Result?.Warnings.Count > 0;

    public string RawJsonText => Result?.RawJsonText ?? string.Empty;

    public bool IsRawJsonVisible => Result?.IsRawJsonVisible == true;

    public string DebugTraceText => Result is null
        ? string.Empty
        : string.IsNullOrWhiteSpace(Result.DebugTraceText)
            ? Result.DebugTraceSuppressionReason
            : Result.DebugTraceText;

    public bool IsDebugTraceVisible => Result?.IsDebugTraceVisible == true;

    public string LastCopiedText
    {
        get => _lastCopiedText;
        private set => SetField(ref _lastCopiedText, value, nameof(LastCopiedText));
    }

    public string LastExportText
    {
        get => _lastExportText;
        private set => SetField(ref _lastExportText, value, nameof(LastExportText));
    }

    public void UseSelectedExample()
    {
        InputText = SelectedFixture?.ExampleInputText ?? SelectedApi?.ExampleInputText ?? string.Empty;
        StatusText = "example loaded";
        FooterStatusText = SelectedSafetyBoundaryNote;
    }

    public void RunSelectedApi()
    {
        if (SelectedApi is null)
        {
            Result = BuildLocalError("API_DASHBOARD_SELECTION_REQUIRED", "Select an API before running.");
            StatusText = "selection required";
            return;
        }

        var runResult = ShouldRunSelectedFixture()
            ? _runner.Run(
                SelectedApi,
                SelectedFixture!,
                IsDebugRequested,
                IsRawJsonRequested,
                correlationId: $"api-dashboard-fixture-ui-{Guid.NewGuid():N}")
            : _runner.Run(
                SelectedApi,
                InputText,
                IsDebugRequested,
                IsRawJsonRequested,
                correlationId: $"api-dashboard-ui-{Guid.NewGuid():N}");
        if (!runResult.IsSuccess || runResult.Value is null)
        {
            Result = BuildLocalError(
                string.IsNullOrWhiteSpace(runResult.ErrorCode) ? "API_DASHBOARD_RUN_FAILED" : runResult.ErrorCode,
                string.IsNullOrWhiteSpace(runResult.ErrorMessage) ? "Dashboard run failed." : runResult.ErrorMessage);
            StatusText = "run failed";
            return;
        }

        _lastRun = runResult.Value;
        Result = runResult.Value.ViewModel;
        AddHistoryEntry(runResult.Value);
        StatusText = runResult.Value.ViewModel.Success ? "result ready" : "error visible";
        FooterStatusText = runResult.Value.EvidenceScope;
    }

    public void ToggleRawJson()
    {
        IsRawJsonRequested = !IsRawJsonRequested;
        StatusText = IsRawJsonRequested ? "raw json on" : "raw json off";
    }

    public void ToggleDebug()
    {
        IsDebugRequested = !IsDebugRequested;
        StatusText = IsDebugRequested ? "debug on" : "debug off";
    }

    public void CopyResultShell()
    {
        LastCopiedText = HumanResultText;
        StatusText = string.IsNullOrWhiteSpace(LastCopiedText) ? "nothing to prepare" : "result copy text ready";
    }

    public void ExportHistoryShell()
    {
        LastExportText = string.Join(
            Environment.NewLine,
            History.Select(entry => $"{entry.MethodId}|{entry.Status}|{entry.Success}|{entry.PromptSha256}|{entry.EvidenceScope}"));
        StatusText = History.Count == 0 ? "history empty" : "safe history text ready";
    }

    private void RefreshPresentedResult()
    {
        if (_lastRun is null)
            return;

        var presented = ApiDashboardResultPresenter.BuildViewModel(
            _lastRun.Response,
            IsDebugRequested,
            IsRawJsonRequested);
        if (presented.IsSuccess && presented.Value is not null)
            Result = presented.Value;
    }

    private void AddHistoryEntry(ApiDashboardRunResult run)
    {
        var methodId = run.Request?.Method ?? SelectedApi?.MethodId ?? string.Empty;
        var humanName = SelectedApi?.HumanName ?? methodId;
        var entry = new ApiDashboardHistoryEntry(
            methodId,
            humanName,
            run.ViewModel.Status,
            run.ViewModel.Success,
            BuildPromptHash(InputText),
            run.EvidenceScope,
            run.HostRuntimeProofClaimed,
            run.SemanticQualityOrTrainedModelProofClaimed);
        History.Insert(0, entry);
        OnPropertyChanged(nameof(History));
        ((DashboardCommand)ExportHistoryCommand).RaiseCanExecuteChanged();
    }

    private bool ShouldRunSelectedFixture()
        => SelectedFixture is not null
            && string.Equals(InputText, SelectedFixture.ExampleInputText, StringComparison.Ordinal);

    private static IReadOnlyList<ApiDashboardCatalogRow> LoadCatalogRows()
    {
        var catalog = ApiCatalogProvider.BuildCatalog();
        return catalog.IsSuccess && catalog.Value is not null
            ? catalog.Value
            : [];
    }

    private static ApiDashboardResultViewModel BuildLocalError(
        string errorCode,
        string errorMessage)
        => new(
            Success: false,
            Status: ApiDashboardRunner.DashboardValidationFailedStatus,
            HumanResult: "Dashboard validation failed.",
            Warnings:
            [
                "core_boundary_only_no_host_runtime_claim",
                "external_ai=false",
                "script_parity_used=false"
            ],
            Errors: [errorCode, errorMessage],
            SelectedSkills: [],
            StructuredTaskRefs: [],
            ScenarioRefs: [],
            CheckSetResults:
            [
                new("core_boundary_called", "false"),
                new("runtime_extension_claimed", "false"),
                new("semantic_quality_claimed", "false")
            ],
            IsRawJsonVisible: false,
            RawJsonText: string.Empty,
            IsDebugTraceVisible: false,
            DebugTraceText: string.Empty,
            DebugTraceSuppressionReason: "Debug trace was not requested.",
            AreBoundaryFlagsVisible: true,
            RuntimeExtensionProofClaimed: false,
            SemanticQualityOrTrainedModelProofClaimed: false,
            BoundaryFlags:
            [
                new("runtime_extension_claimed", false, "Dashboard result is not Claude, Cursor, or Codex host runtime proof."),
                new("semantic_quality_claimed", false, "Dashboard result is not semantic quality or trained-model proof.")
            ]);

    private static string BuildPromptHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private void RaiseCommandAvailability()
    {
        ((DashboardCommand)UseExampleCommand).RaiseCanExecuteChanged();
        ((DashboardCommand)RunCommand).RaiseCanExecuteChanged();
        ((DashboardCommand)CopyResultCommand).RaiseCanExecuteChanged();
        ((DashboardCommand)ExportHistoryCommand).RaiseCanExecuteChanged();
    }

    private void SetField(
        ref string field,
        string value,
        string propertyName)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class DashboardCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public DashboardCommand(Action execute)
            : this(execute, () => true)
        {
        }

        public DashboardCommand(
            Action execute,
            Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => _canExecute();

        public void Execute(object? parameter)
            => _execute();

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
