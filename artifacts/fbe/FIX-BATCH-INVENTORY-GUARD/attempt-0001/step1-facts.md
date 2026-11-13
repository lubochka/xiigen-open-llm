# Шаг 1 — установленные факты по 36 отказам сторожа переписи батчей

Дата: 10.11.2026. Ветка: codex/dpo-implementation. Каталог: C:\Projects\llm_mvp_core

## 1. Что именно падает

Класс проверок `Tests\TestSuiteBatchInventoryGuardTests.cs` содержит 61 проверку
(подсчёт меток `[Fact]` = 61, меток `[Theory]`/`[InlineData]` = 0). Из них красные
ровно 36 — это те проверки, которые обращаются к ЖИВОМУ хранилищу
(`TestSuiteBatchManifest.FindRepositoryRoot()` — 13 обращений,
`ActualRepositoryInventory.Value` — 24 обращения). Остальные 25 работают на
синтетическом корне и зелёные.

Сверка с уликами полного прогона
`artifacts\fbe\FULL-SUITE-20261110\attempt-0001\failures-all-501.md`:
строк с именем класса — 37 (1 строка сводки «| 36 | TestSuiteBatchInventoryGuardTests |»
плюс 36 поимённых проверок). Число 36 подтверждено независимо от журнала.

## 2. Договор сторожа (цитаты кода)

`Tests\TestSuiteBatchManifest.cs:220-234` — корень хранилища опознаётся по двум
признакам сразу: лестница батчей и КАНОНИЧЕСКИЙ тестовый проект.

```
220: internal static string FindRepositoryRoot(string? startDirectory = null)
225:     var hasBatches = File.Exists(Path.Combine(current.FullName, "tools", "test-batches", "batches.tsv"));
226:     var hasTests   = File.Exists(Path.Combine(current.FullName, "Tests", "LlmFromScratch.Tests.csproj"));
```

`Tests\TestSuiteBatchManifest.cs:745-757` — а вот сама область сканирования НИЧЕМ
не ограничена, кроме четырёх имён каталогов:

```
745: internal static IReadOnlyList<TestSuiteBatchProject> DiscoverRunnableTestProjects(string repoRoot)
748:     var projects = EnumerateFilesWithoutIgnoredPathSegments(normalizedRoot, "*.csproj")
749:         .Where(path => !HasIgnoredPathSegment(path))
750:         .Select(path => TryReadTestProject(normalizedRoot, path))
```

`Tests\TestSuiteBatchManifest.cs:1987-1995` — весь нынешний фильтр:

```
1990: return parts.Any(part =>
1991:     string.Equals(part, "bin", ...) ||
1992:     string.Equals(part, "obj", ...) ||
1993:     string.Equals(part, "TestResults", ...) ||
1994:     string.Equals(part, "NuGet", ...));
```

`Tests\TestSuiteBatchManifest.cs:1999-2003` — заявленный СМЫСЛ этого фильтра
(ключ к договору): опись считает исходные личности, а не артефакты выполнения.

```
1999: // Инвентаризация должна учитывать зафиксированные в исходниках
2000: // идентичности, а не артефакты выполнения вроде TestResults. Собственный
2001: // обход отсекает игнорируемые каталоги до входа в них, ...
```

`Tests\TestSuiteBatchManifest.cs:1535-1550` — любой файл проекта с пакетом
тестового набора объявляется тестовым проектом, и отсутствие целевой платформы
бросает исключение (а не добавляет проблему в отчёт):

```
1535: if (!packageNames.Any(name => string.Equals(name, "Microsoft.NET.Test.Sdk", ...)))
1536:     return null;
1549: if (string.IsNullOrWhiteSpace(targetFramework))
1550:     throw new InvalidOperationException($"Test project has no TargetFramework: {projectPath}");
```

**Сколько проектов сторож ОБЯЗАН видеть — три, и это записано трижды:**

1. Ожидание самой проверки: `Tests\TestSuiteBatchInventoryGuardTests.cs:470`
   `Assert.Equal(3, report.Projects.Count);`
2. Лестница групп `tools\test-batches\batches.tsv` объявляет ровно три сборки:
   `Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll`,
   `Tests/DesignConversion.ModelActivation.Tests/bin/Release/net9.0/DesignConversion.ModelActivation.Tests.dll`,
   `Tests.ApiDashboardWpf/bin/Release/net9.0-windows/Tests.ApiDashboardWpf.dll`.
3. Файл решения `LlmFromScratch.sln` объявляет ровно три проекта с тестовым
   пакетом (четвёртый, `Tests.Tools\RegistryLockHolder`, — служебный, без пакета
   тестового набора, и сканер сам его отбрасывает возвратом null).

Вывод по договору: сторож обязан описывать тестовые проекты ХРАНИЛИЩА (те, что
хранилище само объявляет своим решением и своей лестницей), а не любой найденный
на диске файл проекта.

## 3. Найденные копии файла тестового проекта

Файлов `*.csproj` на диске: 877. Из них внутри `.kiro`: 48.
Файлов с именем `LlmFromScratch.Tests.csproj`: 38 — один настоящий
(`Tests\LlmFromScratch.Tests.csproj`) и **37 посторонних внутри `.kiro`**.

Проектов с пакетом `Microsoft.NET.Test.Sdk`, уцелевших после нынешнего фильтра
сканера: **44**. Из них вне `.kiro` — пять:

```
Tests\LlmFromScratch.Tests.csproj                                             (канон 1)
Tests\DesignConversion.ModelActivation.Tests\DesignConversion.ModelActivation.Tests.csproj (канон 2)
Tests.ApiDashboardWpf\Tests.ApiDashboardWpf.csproj                            (канон 3)
experiments-v2\_runners\R29-AUDIT-R3-attempt-0001\guard-fixture\GuardFixture.csproj  (ЛИШНИЙ)
experiments-v2\RP01-MICRO\P13101\Checks\Checks.csproj                                (ЛИШНИЙ)
```

Внутри `.kiro` — 39 проектов с тестовым пакетом: 36 **без** целевой платформы
и 3 с целевой платформой.

Все 36 негодных копий байт-в-байт одинаковы (длина 113 байт) и содержат ровно одну
строку — это синтетическая заготовка, а не собираемый проект:

```xml
<Project><ItemGroup><PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" /></ItemGroup></Project>
```

Лежат они по путям вида
`.kiro\xiigen-slices\work\results\S04-...\compile-owner-controls\Tests\LlmFromScratch.Tests.csproj`,
то есть внутри каталога РЕЗУЛЬТАТОВ работ владельца — ровно того класса, который
нынешний фильтр уже объявил не подлежащим описи («артефакты выполнения»).

## 4. Отслеживание и история

- `.kiro` отслеживается хранилищем: `git ls-files -- .kiro` → 36 103 файла,
  `git ls-files -- .kiro/**/*.csproj` → 48 файлов.
- `git check-ignore -v --no-index` для негодной копии и для `.kiro/anything.txt`:
  код выхода 1, вывода нет → **совпадающего правила игнорирования нет,
  путь НЕ игнорируется**. (Проверено по правилу: вердикт даёт текст выигравшего
  шаблона, а не код выхода.)
- Негодные копии добавлены коммитом `bf14103eb59` от 12.10.2026 («slice 4 artifacts»).
- Два лишних проекта в `experiments-v2` добавлены позже:
  `GuardFixture.csproj` — `72b283e0145` от 29.10.2026 (целевая платформа net9.0),
  `Checks.csproj` — `b2778359374` от 03.11.2026 (целевая платформа net9.0).
- Ни `Tests\*.cs`, ни сканер нигде не упоминают `.kiro`: совпадений по `\.kiro`
  в `Tests\*.cs` — 0; совпадений по `kiro`/`experiments-v2` в
  `Tests\TestSuiteBatchManifest.cs` — 0. История сканера тоже никогда их не
  упоминала (`git log -S kiro` и `git log -S experiments-v2` по файлу сканера
  пусты).

## 5. Вердикт о причине — УСТАНОВЛЕНА

Сторож выходит за свой договор. Он обязан описывать тестовые проекты хранилища
(три, объявленные решением и лестницей), но его сканер объявляет тестовым
проектом ЛЮБОЙ файл `*.csproj` в рабочем дереве, если в нём есть ссылка на пакет
тестового набора. В дереве лежат 41 посторонний такой файл: 39 внутри каталога
результатов работ владельца `.kiro` и 2 внутри изолированных опытных площадок
`experiments-v2`. Первая же встреченная однострочная заготовка без целевой
платформы бросает исключение — и все 36 проверок, обращающихся к живому
хранилищу, падают с одним и тем же текстом.

Это дефект области сканирования сторожа, а не дефект каталога `.kiro`. Важно:
даже если бы заготовки имели целевую платформу, сторож нашёл бы 8 проектов
вместо 3, и ожидание `Assert.Equal(3, report.Projects.Count)` всё равно было бы
нарушено. То есть течь области двойная, и починка обязана закрыть обе.
