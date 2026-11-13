# LlmInspector

Documentation status: DInspector-B documented - parent review passed.

## Краткая карта

`LlmInspector` - это WPF-приложение для просмотра трасс, которые собирает `LlmInspector.Core`. Оно открывает окно с тремя зонами: слева легкая 2D-сцена топологии, в центре дерево station cards, справа инспектор выбранной станции с входом и выходом в виде `PropertyNode` дерева. Нижняя панель строит простые topology commands и отправляет их через `InspectorSession`.

Честная граница: это UI-оболочка над `LlmInspector.Core`, а не `ILearningMachine`, не обученная логика, не доказательство готовности продукта, плагина или host runtime. Успешный рендер WPF, дерево карточек, кнопка RUN, статусы и XAML-стили показывают наблюдаемость и форму данных, но не модельное качество, не checkpoint/manifest/locator, не fresh-load и не runtime-интеграцию IDE.

## UML

```mermaid
classDiagram
    class App {
      +StartupUri MainWindow.xaml
      +MergedDictionaries
    }
    class MainWindow {
      +MainWindow()
      +Loaded RunSampleAsync
    }
    class MainViewModel {
      +RunSampleAsync()
      +RunCommand
      +StopCommand
      +SelectStation(station)
      -RunCommandBarAsync()
      -OnStationTraced(sender, trace)
    }
    class InspectorSessionCore {
      +StationTraced
      +RunTopologyCommandAsync(command, cancellationToken)
    }
    class TraceTreePanel
    class StationCard
    class StationInspector
    class PropertyNodeView
    class TopologyViewport
    class TopologySceneObserver

    App --> MainWindow : opens startup window
    MainWindow --> MainViewModel : creates DataContext
    MainViewModel --> InspectorSessionCore : runs topology command
    InspectorSessionCore --> MainViewModel : emits StationTraced
    MainViewModel --> TraceTreePanel : Tree.Roots binding
    TraceTreePanel --> StationCard : recursive card template
    StationCard --> MainViewModel : SelectStation on click
    MainViewModel --> StationInspector : SelectedStation binding
    StationInspector --> PropertyNodeView : input/output property trees
    MainViewModel --> TopologyViewport : forwards StationTraced
    TopologyViewport --> TopologySceneObserver : completed traces only
    TopologySceneObserver --> TopologyViewport : redraws Canvas
```

## Симуляции связей

| Связь | Пример входа | Что происходит | Ожидаемый результат | Что нельзя утверждать |
|---|---|---|---|---|
| `App -> MainWindow` | Пользователь запускает `LlmInspector.exe`. | WPF читает `StartupUri` и подключает словари цветов, типографики и контролов. | Открывается окно с XAML-разметкой и общими стилями. | Это не доказывает работу всех кнопок или runtime host интеграцию. |
| `MainWindow -> MainViewModel` | Конструктор окна завершает `InitializeComponent()`. | Создается `MainViewModel`, назначается `DataContext`, на `Loaded` вешается `RunSampleAsync`. | Панели получают общий view model; после загрузки стартует маленькая sample-команда. | Startup smoke не является продуктовым e2e тестом. |
| `MainViewModel -> InspectorSession` | В форме выбран `add domain`, id `qa_automation`, нажата RUN. | ViewModel строит typed command, создает cancellation budget и вызывает `RunTopologyCommandAsync`. | Core emits station traces; статус становится `ok`, `error` или `stopped`. | Это не обучение, не fresh-load, не checkpoint и не внешняя AI-интеграция. |
| `InspectorSession -> MainViewModel.OnStationTraced` | Core прислал start/completion trace. | ViewModel считает trace budget, вызывает `Tree.Upsert(trace)` и пересылает событие `StationTraced`. | В центре обновляется дерево, viewport получает тот же trace. | Дерево карточек не доказывает истинность бизнес-результата. |
| `TraceTreePanel -> StationCard` | `Tree.Roots` содержит root orchestrator card. | ItemsControl создает recursive `StationCard`. | Пользователь видит цепочку station cards. | UI-дерево не заменяет тесты исходных проектов. |
| `StationCard -> MainViewModel.SelectStation` | Пользователь кликает вложенную карточку. | Handler останавливает bubbling, берет `StationCardViewModel` и вызывает `SelectStation`. | Правая панель получает новую `SelectedStation`. | Выбор карточки не выполняет повторно unit. |
| `StationInspector -> PropertyNodeView` | Выбрана completion station с input/output. | XAML выводит station header и рекурсивные property nodes. | Видны поля, значения и вложенные дети. | Property tree не является semantic validator. |
| `TopologyViewport -> TopologySceneObserver` | Completion trace несет output с `LogicalDomainEntry`. | Viewport игнорирует start traces, observer применяет output к `GeneratedTopologyState` и перерисовывает Canvas. | В левой панели появляется topology node. | Это deterministic projection, не trainable topology model. |
| `Viewport buttons -> observer` | Нажаты `OVERVIEW`, `OPEN ACTIVE`, `FIT`. | Кнопки переключают detail mode или пересчитывают layout. | Canvas перерисовывается без повторного core flow. | Кнопки не доказывают интерактивный IDE plugin host. |

## Алгоритмы без кода

### `App.xaml`: startup/resource load

Вход: запуск WPF-приложения. Выход: `MainWindow` с подключенными ресурсами. Алгоритм: `Application` указывает `StartupUri="MainWindow.xaml"`, затем сливает три словаря ресурсов: цвета, типографику и стили контролов. Это декларативная загрузка XAML, а не runtime проверка UI.

`LlmInspector/App.xaml:1-14`

```xml
<Application x:Class="LlmInspector.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Styles/Colors.xaml" />
                <ResourceDictionary Source="Styles/Typography.xaml" />
                <ResourceDictionary Source="Styles/Controls.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### `MainWindow.MainWindow()`

Вход: создание окна WPF. Выход: окно с `DataContext = MainViewModel` и startup sample hook. Алгоритм: загрузить XAML, создать view model, назначить DataContext, после `Loaded` вызвать `RunSampleAsync`.

`LlmInspector/MainWindow.xaml.cs:14-26`

```csharp
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
```

### `MainViewModel.MainViewModel()` и bound surface

Вход: создание view model. Выход: команды RUN/STOP, trace tree, список command types, event stream для viewport. Алгоритм: создать `InspectorCommand` для запуска и остановки, подписаться на `_session.StationTraced`, открыть `Tree` для binding, открыть `StationTraced` как вторичный event.

`LlmInspector/ViewModels/MainViewModel.cs:96-128`

```csharp
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
```

### `MainViewModel.RunSampleAsync()`

Вход: событие `Loaded` окна. Выход: один bounded startup trace или status `startup smoke stopped`. Алгоритм: защита от повторного запуска, создание детерминированного `AddDomainCommand`, cancellation на 2 секунды, вызов `_session.RunTopologyCommandAsync`. Это стартовый visual seed, не полноценный smoke gate.

`LlmInspector/ViewModels/MainViewModel.cs:338-371`

```csharp
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
```

### `MainViewModel.RunCommandBarAsync()`

Вход: поля command bar и кнопка RUN. Выход: trace events и `CommandStatus`. Алгоритм: если уже идет run, выйти; создать 3-секундный cancellation; сбросить trace counter; выставить `IsRunning`; построить typed command; вызвать core session; выбрать status по trace budget, cancellation, success/error.

`LlmInspector/ViewModels/MainViewModel.cs:380-439`

```csharp
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
```

### `MainViewModel.BuildCommandFromForm()`

Вход: `SelectedCommandType` и поля формы. Выход: один `LogicalTopologyCommand`. Алгоритм: явные ветки для `add phase`, `add sub-phase`, `add transition`; неизвестное значение безопасно уходит в `BuildAddDomainCommand`.

`LlmInspector/ViewModels/MainViewModel.cs:454-471`

```csharp
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
```

### `MainViewModel.OnStationTraced(object? sender, StationTrace trace)`

Вход: station trace из `LlmInspector.Core`. Выход: обновленное дерево и событие для viewport. Алгоритм: во время explicit run считает events, при достижении `MaxTraceEventsPerRun` отменяет активный run, затем всегда делает `Tree.Upsert(trace)` и пересылает trace наружу.

`LlmInspector/ViewModels/MainViewModel.cs:540-565`

```csharp
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
```

### `TraceTreePanel` и `StationCard`

Вход: `Tree.Roots` из view model. Выход: центр окна с recursive station cards. `TraceTreePanel.xaml` строит `ItemsControl` по root cards, а `StationCard.xaml.cs` выбирает station в `MainViewModel` и не дает nested click всплыть к родителям.

`LlmInspector/Views/TraceTreePanel.xaml:6-17`

```xml
    <Border Style="{StaticResource EmptyPanel}">
        <ScrollViewer VerticalScrollBarVisibility="Auto"
                      HorizontalScrollBarVisibility="Disabled">
            <ItemsControl ItemsSource="{Binding Tree.Roots}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <views:StationCard />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </Border>
```

`LlmInspector/Views/StationCard.xaml.cs:18-37`

```csharp
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
```

### `StationInspector` и `PropertyNodeView`

Вход: `SelectedStation`. Выход: правая панель с station header, runtime/resource text, input tree and output tree. `StationInspector.xaml` binds station properties and renders each input/output node through recursive `PropertyNodeView`. `PropertyNodeView` itself only projects `Name`, `Value`, and `Children`.

`LlmInspector/Views/StationInspector.xaml:43-70`

```xml
                <TextBlock Text="INPUT"
                           FontFamily="{StaticResource FontMono}"
                           FontSize="10"
                           Foreground="{StaticResource BrushTextTertiary}"
                           Margin="0,0,0,6" />
                <ItemsControl ItemsSource="{Binding Input}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <views:PropertyNodeView />
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>

                <Separator Margin="0,12,0,12"
                           Background="{StaticResource BrushBorder}" />

                <TextBlock Text="OUTPUT"
                           FontFamily="{StaticResource FontMono}"
                           FontSize="10"
                           Foreground="{StaticResource BrushTextTertiary}"
                           Margin="0,0,0,6" />
                <ItemsControl ItemsSource="{Binding Output}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <views:PropertyNodeView />
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
```

`LlmInspector/Views/PropertyNodeView.xaml:12-34`

```xml
            <TextBlock Grid.Column="0"
                       Text="{Binding Name}"
                       FontFamily="{StaticResource FontMono}"
                       FontSize="10"
                       Foreground="{StaticResource BrushTextSecondary}"
                       TextTrimming="CharacterEllipsis" />

            <TextBlock Grid.Column="1"
                       Text="{Binding Value}"
                       FontFamily="{StaticResource FontMono}"
                       FontSize="10"
                       Foreground="{StaticResource BrushTextPrimary}"
                       TextWrapping="Wrap" />
        </Grid>

        <ItemsControl ItemsSource="{Binding Children}"
                      Margin="12,0,0,0">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <views:PropertyNodeView />
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
```

### `TopologyViewport` и `TopologySceneObserver`

Вход: raw station trace stream из `MainViewModel`. Выход: Canvas shapes на левой панели. `TopologyViewport.AttachToViewModel()` подписывается на view model, `OnStationTraced` пропускает только completed traces, observer применяет их к `GeneratedTopologyState` и перерисовывает весь Canvas. Кнопки `OVERVIEW`, `OPEN ACTIVE`, `FIT` переключают видимость деталей и layout без повторного выполнения core session.

`LlmInspector/Views/TopologyViewport.xaml.cs:71-105`

```csharp
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
```

`LlmInspector/Inspector/TopologySceneObserver.cs:87-99`

```csharp
    // React to one completed station trace.
    public void OnTraceCompleted(StationTrace trace)
    {
        // Apply only completed generated output to the tested topology state.
        if (!_topology.ApplyTrace(trace))
            return;

        // Store the latest immutable snapshot for rendering.
        _snapshot = _topology.Snapshot();

        // Repaint the canvas from the tested generated topology state.
        RebuildScene();
    }
```

`LlmInspector/Views/TopologyViewport.xaml.cs:107-126`

```csharp
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
```

## QA/test mapping

Тесты были найдены и сопоставлены, но не запускались в этой документационной фазе.

| Тестовый файл | Что подтверждает | Честная граница |
|---|---|---|
| `Tests/LlmInspectorTracingTests.cs` | Core session emits orchestrator/flow/unit/storage traces and sample project 21/22/23 station roles. | Это core-level evidence, не WPF UI automation. |
| `Tests/LlmInspectorTraceTreeTests.cs` | `StationTraceTree` builds parent/child chain, completion events update cards, station badge text maps kinds. | Не проверяет XAML rendering, click behavior in WPF, window layout. |
| `Tests/LlmInspectorGeneratedTopologyStateTests.cs` | Generated topology state attaches/buffers domain, phase, subphase, transition output. | Не проверяет Canvas rendering or user button clicks. |
| `Tests/LlmInspectorDataLayerTests.cs` | `PropertyNodeSerialiser` emits nested fields shown by inspector. | Не доказывает semantic correctness of every possible object. |
| `Tests/LlmInspectorParallelWaveTests.cs` | Parallel wave grouping in core view models. | Не доказывает WPF performance or full runtime concurrency. |
| `Tests/LlmInspectorTraceContextTests.cs` | Trace context isolation and restore. | Не доказывает distributed tracing. |

Прямого WPF UI automation/e2e теста для `MainWindow`, `TraceTreePanel`, `TopologyViewport`, `StationInspector`, `StationCard` или `PropertyNodeView` в найденных inspector tests нет. Это документационный gap: код можно читать и статически проверять, но в этой фазе UI не запускался и не проверялся визуально.

`Tests/LlmInspectorTraceTreeTests.cs:29-37`

```csharp
        // Feed all start and completion events through the upsert path.
        foreach (var trace in traces)
            tree.Upsert(trace);

        // The tree should have one root orchestrator.
        var root = Assert.Single(tree.Roots);

        // Completion events must update the root instead of adding a duplicate.
        Assert.False(root.IsRunning);
```

## Status and boundaries

| Компонент | Реальный статус | Не доказывает |
|---|---|---|
| `LlmInspector.csproj` | Windows WPF shell referencing `LlmInspector.Core` and `21.AgentInfrastructure`. | Не отдельный model/runtime asset lifecycle. |
| `App.xaml`/styles | Declarative startup and visual resource load. | Не UI scenario pass. |
| `MainWindow` | Three-panel shell plus command bar, bound to `MainViewModel`. | Не plugin host or IDE integration. |
| `MainViewModel` | UI state, command construction, trace event handling, bounded cancellation. | Не training, не semantic authority, не product readiness. |
| `TraceTreePanel`/`StationCard` | Recursive display and selection of station cards. | Не повторное выполнение station logic. |
| `StationInspector`/`PropertyNodeView` | Display of selected station input/output property trees. | Не validation of business correctness. |
| `TopologyViewport`/`TopologySceneObserver` | Deterministic 2D Canvas projection from completed core traces. | Не trainable graph model and not runtime topology learning. |

## Anti-false-done

- WPF window, XAML layout, styles and station cards are inspector UI infrastructure only.
- `LlmInspector` is not an `ILearningMachine`, not trained reasoning, not model readiness and not product/plugin host runtime evidence.
- JSON/static docs/UI markup/Canvas rendering are not runtime completion by themselves.
- Startup sample and command-bar runs are bounded inspection flows, not end-to-end product gates.
- `GeneratedTopologyState` projection and `TopologySceneObserver` drawing are deterministic helper logic; they must not be presented as semantic model training.
- Existing tests mostly cover `LlmInspector.Core` data/tracing behavior. They were read/mapped, not run, and they do not substitute for WPF UI automation.
