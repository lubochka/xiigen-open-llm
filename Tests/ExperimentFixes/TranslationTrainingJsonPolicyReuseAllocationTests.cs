// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using TranslationDomain;
// Общий приёмник доказательств опытов живёт в соседнем пространстве имён этого же каталога.
using LlmFromScratch.Tests.ExperimentFixes;

// ПОСТОЯННЫЙ ИЗМЕРИТЕЛЬ стоимости записи урока единицей-исполнителем перевода.
// Строка очереди переносов 41, идентификатор работы FIX-JO, карточка
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/46-training-json-options-reuse.md.
//
// Что проверяется по-человечески. Единица-исполнитель читает каждый урок из
// текста JSON. Прежде она создавала настройки разбора заново НА КАЖДЫЙ урок, и
// вместе с ними заново строился внутренний справочник разбора по типам — именно
// эта повторная постройка и была главной стоимостью. Перенос заменил создание
// настроек на один общий неизменяемый экземпляр. Смысл разбора при этом не
// изменился ни на символ: значения настроек те же, преобразователь тот же,
// порядок и аргументы двух вызовов разбора те же.
//
// Чего этот измеритель НЕ делает. Он не утверждает процент экономии и не
// закрепляет снятое число: сборщик мусора и компилятор на другой машине дадут
// другое число, и узкий порог покраснел бы не по вине продукта. Поэтому число
// попадает в доказательства, а утверждение держит только заведомо широкую
// границу — см. комментарий у самой границы.
// Фикстура класса ниже даёт этому испытанию ранний захват попытки: оснастка создаёт её ДО
// конструктора тестового класса и до любого урока обучения, а освобождает ОДИН раз после
// последнего теста класса.
public sealed class TranslationTrainingJsonPolicyReuseAllocationTests
    : IClassFixture<TranslationTrainingJsonPolicyReuseAllocationTests.AttemptScopeFixture>
{
    // Число уроков в измеряемом окне: как в исходном опыте JO1.
    private const int LessonCount = 16;

    // Тема урока: та же, что в существующих проверках этой области.
    private const string Topic = "general";

    // Язык источника: английский, как у измеряемого направления.
    private const string SourceLang = "en";

    // Опознаватель источника уроков: собственный, чтобы не пересекаться с чужими.
    private const string SourceId = "fix-jo-allocation-v1";

    // Направление перевода: с английского на русский — то, которое мерил опыт.
    private static readonly TranslationDirection Direction = TranslationDirection.EN_RU;

    // Заведомо широкая граница: двадцать пять тысяч байт на урок.
    //
    // Почему она широкая, а не точная. Снятые наблюдения кандидата дали около
    // 3 244 и 3 485 байт на урок, то есть запас до границы примерно семикратный.
    // Такой запас намеренно покрывает разброс сборщика мусора, иную версию
    // компилятора и иную машину: честное измерение не должно краснеть от смены
    // окружения.
    //
    // Что именно граница ловит. Возврат к созданию настроек на каждый урок даёт
    // около 81 561 байта на урок — это проверено мутацией, которая компилируется
    // и воспроизводит базовое число до байта. Такая правка пробьёт границу более
    // чем в три раза, и проверка покраснеет. То есть граница ловит ровно потерю
    // переиспользования, а не колебания окружения.
    private const long MaxAllocatedBytesPerLesson = 25_000;

    // Настройки сериализации для подготовки текстов уроков. Это НЕ предмет
    // измерения: тексты готовятся до окна и в окно не попадают.
    private static readonly JsonSerializerOptions UnitJson = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Корень хранилища ищется по файлу правил проекта, вверх от каталога сборки.
    private static string RepositoryRoot()
    {
        // Поиск начинается от каталога, откуда запущена сборка проверок.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        // Подъём вверх до каталога, где лежит файл правил агента.
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        // Без корня хранилища доказательства писать некуда.
        Assert.NotNull(directory);

        // Полный путь корня возвращается вызывающей стороне.
        return directory!.FullName;
    }

    // Каталог доказательств этой работы — ровно тот, который назначает карточка.
    private static string AttemptDirectory() => Path.Combine(
        RepositoryRoot(), "artifacts", "fbe", "FIX-JO", "attempt-0001");

    // Куда этому прогону разрешено писать. Решение принимает общий приёмник
    // доказательств: пока попытка не опубликована, прогон пишет в назначенный
    // каталог; как только она опубликована, тот же прогон уходит в игнорируемый
    // каталог результатов испытаний и не трогает опубликованное наблюдение.
    //
    // Почему ленивое поле заменено фикстурой класса. В этом испытании есть настоящие уроки обучения,
    // поэтому поздний захват означал оплату обучения до того, как станет известен отказ. Фикстура
    // класса создаётся раньше конструктора теста, поэтому захват адреса стал первым действием
    // попытки, а завершение попытки ставится ровно один раз.
    public sealed class AttemptScopeFixture : ExperimentAttemptScope
    {
        /// <summary>Захватывает попытку этого испытания и ставит её в поле класса.</summary>
        public AttemptScopeFixture()
            : base(
                RepositoryRoot(),
                AttemptDirectory(),
                nameof(TranslationTrainingJsonPolicyReuseAllocationTests))
            // Поле класса заполняется только после успешного захвата: иначе области не существует.
            => AttemptScope = this;
    }

    /// <summary>Захваченная попытка этого класса: её ставит фикстура до первого теста.</summary>
    private static AttemptScopeFixture? AttemptScope;

    /// <summary>Приёмник доказательств захваченной попытки этого класса.</summary>
    private static ExperimentEvidenceSink Evidence =>
        (AttemptScope ?? throw new InvalidOperationException(
            "Область попытки не захвачена: фикстура класса " +
            nameof(TranslationTrainingJsonPolicyReuseAllocationTests) + " не была создана.")).Evidence;

    // Доказательства пишутся сразу в пригодном для хранилища виде: без метки
    // порядка байтов, перевод строки одним знаком, без пробелов в конце строк.
    private static void WriteEvidence(string relativePath, string text)
    {
        // Нормализация оставлена здесь намеренно: приёмник пишет байты как есть.
        var normalized = string.Join("\n",
            text.Replace("\r\n", "\n").Split('\n').Select(line => line.TrimEnd()));

        // Запись выполняется приёмником, который и выбрал разрешённый каталог.
        Evidence.WriteTextExactly(relativePath, normalized);
    }

    [Fact]
    public void LessonWriteAllocatesFarLessThanPerLessonPolicyConstruction()
    {
        // Хранилище соседей для измеряемой единицы.
        var store = new TranslationKnnStore();

        // Измеряемая единица-исполнитель.
        var unit = new ExecutorUnit(Direction, store);

        // Уроки готовятся ПОЛНОСТЬЮ до окна: тексты JSON уже собраны.
        var lessons = BuildLessons(LessonCount, "probe");

        // Прогрев вне окна: отдельная единица и отдельное хранилище, чтобы
        // разовая подготовка чтения типов не попала в измеряемое число.
        var warmUnit = new ExecutorUnit(Direction, new TranslationKnnStore());

        // Два прогревочных урока с другими текстами.
        foreach (var warm in BuildLessons(2, "warm"))
            warmUnit.TrainOnExample(warm);

        // Счётчик выделенных байтов ЭТОГО ПОТОКА на начало окна.
        //
        // Почему поток, а не весь процесс. Проверки проекта идут параллельно, и
        // счётчик всего процесса ловит выделения чужих проверок: при общем
        // прогоне семей такое измерение краснело бы не по вине продукта — это
        // наблюдалось лично на 481 проверке. Счётчик потока считает только наши
        // вызовы, поэтому число не зависит от соседей по прогону.
        //
        // Полной сборки мусора здесь намеренно нет: счётчик потока в ней не
        // нуждается, а соседним параллельным проверкам она бы мешала.
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        // Измеряемое окно: шестнадцать вызовов продуктовой записи урока.
        foreach (var lesson in lessons)
            unit.TrainOnExample(lesson);

        // Счётчик выделенных байтов этого же потока на конец окна.
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        // Разность и есть стоимость окна в байтах.
        var allocatedInWindow = allocatedAfter - allocatedBefore;

        // Стоимость одного урока — величина, с которой сравнивается граница.
        var allocatedPerLesson = allocatedInWindow / LessonCount;

        // Отпечаток выученного состояния: число пар в хранилище соседей.
        var storePairs = store.Count;

        // Личность фактически загруженной библиотеки предметной области.
        var loadedAssembly = typeof(ExecutorUnit).Assembly;

        // Доказательство пишется ДО утверждений, чтобы красный прогон тоже
        // оставлял измеренный след, а не обрывал измерение.
        WriteEvidence("allocation-reuse-observation.md", string.Join("\n", new[]
        {
            "# Стоимость записи урока единицей-исполнителем перевода",
            "",
            "Строка очереди переносов 41, идентификатор работы FIX-JO.",
            "",
            $"- уроков в окне: {LessonCount}",
            $"- выделено за окно, байт: {allocatedInWindow}",
            $"- выделено на урок, байт: {allocatedPerLesson}",
            $"- пар в памяти соседей после обучения: {storePairs}",
            $"- широкая граница на урок, байт: {MaxAllocatedBytesPerLesson}",
            $"- загруженная библиотека: {loadedAssembly.Location}",
            $"- номер модуля библиотеки: {loadedAssembly.ManifestModule.ModuleVersionId}",
            "",
            "Число снято одним прогоном и устойчивым процентом не объявляется.",
            "Граница широкая намеренно: она ловит возврат к созданию настроек",
            "разбора на каждый урок, а не разброс окружения.",
            ""
        }));

        // Поведенческое утверждение, от окружения не зависящее: шестнадцать
        // уроков обязаны лечь под три адреса каждый — полный, префиксный и
        // структурный, то есть ровно сорок восемь пар.
        Assert.Equal(LessonCount * 3, storePairs);

        // Широкая ресурсная граница: см. длинный комментарий у самой постоянной.
        Assert.True(
            allocatedPerLesson < MaxAllocatedBytesPerLesson,
            $"на урок выделено {allocatedPerLesson} байт при широкой границе {MaxAllocatedBytesPerLesson}; "
            + "столько тратит создание настроек разбора на каждый урок, то есть переиспользование потеряно");
    }

    // Построение набора законных учебных строк исполнителя.
    private static List<TranslationLearningExample> BuildLessons(int count, string marker)
    {
        // Накопитель готовых учебных строк.
        var lessons = new List<TranslationLearningExample>(count);

        // Токенизатор подписей создаётся один раз на набор.
        var tokenizer = new TranslationSemanticTokenizer();

        // Каждый урок получает свой текст источника и свой перевод.
        for (var index = 0; index < count; index++)
        {
            // Текст источника: различимый по номеру, чтобы адреса не совпадали.
            var source = $"{marker} sentence {index}";

            // Текст перевода: различимый по тому же номеру.
            var target = $"перевод {marker} {index}";

            // Подпись настоящего токенизатора для адреса памяти соседей.
            var signature = tokenizer.Tokenize(source, SourceLang).Signature;

            // Учебная строка собирается ровно по форме предметной области.
            lessons.Add(new TranslationLearningExample(
                ExampleId: $"fix-jo-{marker}-{index}",
                UnitId: TranslationUnitIds.ExecutorFor(Direction),
                FlowId: TranslationRoutingFlow.FlowId,
                InputJson: JsonSerializer.Serialize(new ExecutorInput(source, SourceLang, Topic), UnitJson),
                ChosenOutputJson: JsonSerializer.Serialize(new ExecutorOutput(target, 0.95, null, null), UnitJson),
                RejectedOutputJson: JsonSerializer.Serialize(new ExecutorOutput("[WRONG: missing translation]", 0.1, null), UnitJson),
                RejectedType: "synthetic_missing_translation",
                SemanticSignature: signature,
                SourceId: SourceId,
                HeldOut: false));
        }

        // Готовый набор возвращается целиком.
        return lessons;
    }
}
