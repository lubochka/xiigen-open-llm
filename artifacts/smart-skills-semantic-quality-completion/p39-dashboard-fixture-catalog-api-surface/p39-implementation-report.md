# P39 - dashboard fixture catalog / API surface

## Что сделано

P39 добавляет в dashboard честный preview-слой для P36/P37 fixture rows. Теперь dashboard catalog/API умеет показывать человеку fixture-примеры для трех API:

- Prompt to task - 12 примеров из P36.
- Orchestration plan - 12 примеров из P36.
- Skill selection - 12 примеров из P36.

Это не запуск модели, не training, не eval и не доказательство semantic quality. Это test-only dashboard/API surface: человек и тесты могут выбрать fixture, увидеть human-readable имя, пример input, ожидаемый видимый результат и boundary/evidence fields.

## Какие классы и методы затронуты

- `ApiDashboardCatalog.BuildCatalog()` - теперь прикрепляет fixture examples к строкам каталога.
- `SemanticQualityFixtureCatalogProvider.BuildFixtureExamplesByMethod()` - новый provider, который читает P36 fixture rows и dashboard scenarios, затем группирует примеры по dashboard method.
- `ApiDashboardFixtureExampleRow` - новая типизированная строка fixture-примера для dashboard.
- `ApiDashboardRequestBuilder.BuildRequest(ApiDashboardCatalogRow, ApiDashboardFixtureExampleRow, ...)` - строит preview request с safe boundary fields.
- `ApiDashboardRunner.Run(ApiDashboardCatalogRow, ApiDashboardFixtureExampleRow, ...)` - позволяет handler/viewmodel-level тестам вызвать fixture preview через тот же runner path.
- `ApiDashboardViewModel.SelectedFixture` / `SelectedFixtureExamples` / `SelectedFixtureExpectedResult` - позволяют UI выбрать fixture и отправить его как пример.
- `ApiDashboardWindow.xaml` - добавлен список fixture examples и поле expected fixture result.

## Как использовать из dashboard/API

1. Открыть API dashboard.
2. Выбрать один из основных API: Prompt to task, Orchestration plan или Skill selection.
3. В списке `Fixture examples` выбрать P36 fixture.
4. Нажать `Use example`, затем `Run`.
5. Dashboard отправит preview request через обычный dashboard runner.
6. В request будут boundary fields:
   - `fixture_preview_only=true`
   - `training_allowed=false`
   - `eval_execution_allowed=false`
   - `semantic_quality_claimed=false`
   - `runtime_extension_claimed=false`

## Честная граница

P39 доказывает только то, что P36/P37 fixture rows доступны через dashboard catalog/API surface, имеют human-readable labels и проходят handler/viewmodel/WPF QA.

P39 не доказывает:

- live Claude/Cursor/Codex extension runtime;
- trained-model readiness;
- semantic quality;
- model eval execution;
- training;
- checkpoint rewrite;
- mutation model files.

## Проверки

- `node tools/semantic-quality-fixture-qa.js` - PASS, 36 fixtures / 36 scenarios / 36 labels.
- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter "ApiDashboard|SemanticQualityFixture" --nologo` - PASS, 38/38.
- `dotnet test Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj --nologo` - PASS, 9/9.
- `powershell.exe -ExecutionPolicy Bypass -File tools/api-dashboard-qa.ps1` - PASS, 37 checks.

## Исправления по review циклам

- Добавлен provider вместо ad hoc чтения fixture rows прямо во viewmodel.
- Fixture request получил explicit false flags, чтобы UI не мог выглядеть как semantic quality proof.
- WPF automation теперь проверяет fixture list, expected result и fixture invocation path.
- Документация dashboard обновлена: что делает preview layer и чего он не доказывает.

