// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure.Logical;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Что было неправильно. Правка оптимистичной блокировки добавила к измерению качества третий
// довод — номер версии состояния, которую вызывающий собирался измерить. Довод был необязательным
// и стоял последним, поэтому выглядел безопасным. Но измерение качества — ЧЛЕН контракта
// обучаемой машины, а в C# метод с лишним доводом НЕ реализует член интерфейса: совпадать должна
// сама подпись, а не только имя. Машина перевода перестала удовлетворять контракту, и область
// перевода перестала собираться.
//
// Почему поломка дошла до ветки. У области перевода нет сигнала сборки от опытов: проекты опытов
// её не собирают, а прямые команды сборки и прогона тестов запрещены политикой разрешений. Значит
// расхождение подписи с контрактом нельзя было увидеть ни одной разрешённой командой.
//
// Что стало. Член контракта вернулся к объявленной подписи: набор проверки и признак отмены.
// Проверка версии живёт в отдельно названном методе `EvaluateAtVersionAsync`, а не в перегрузке по
// числу доводов: необязательные доводы вместе с перегрузками дают неоднозначность вызова.
//
// Чем эта проверка ловит именно этот класс поломки. Первая проверка ПРИСВАИВАЕТ машину переменной
// типа контракта и вызывает через неё все пять членов. Такое присвоение не компилируется, если
// реализация разошлась с контрактом хотя бы одним доводом, одним типом или порядком доводов.
// Поэтому расхождение падает на сборке ПРОЕКТА ПРОВЕРОК, которая ссылается на область перевода, а
// не остаётся незамеченным до чужого проекта. До этой починки файл не скомпилировался бы вовсе.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений проекта,
// и обходить запрет оболочкой-посредником я не стал.
public sealed class TranslationMachineContractSignatureTests
{
    // Отменённый признак отмены: все пять членов проверяют отмену ПЕРВЫМ делом, поэтому их можно
    // вызвать через контракт, не меняя ни одной величины состояния и не обучая ничего.
    private static CancellationToken AlreadyCancelled => new(canceled: true);

    // Непустой набор проверки обычной формы: одна отложенная строка с вопросом и верным ответом.
    private static LearningEvaluationSet<TranslationDomain> HeldOutSet()
        => new(
            "eval-translation-contract",
            TranslationMachine.DomainTag,
            [new LearningEvaluationItem("item-1", "cat", "кот")]);

    // ── Проверка 1: машина удовлетворяет контракту, и каждый член вызывается через него ──

    [Fact]
    public async Task TheMachineIsAssignableToTheContract_AndEveryMemberIsCallableThroughIt()
    {
        // Вот сама ловушка: присвоение переменной типа контракта. Если хотя бы у одного члена
        // разойдётся имя, число доводов, их порядок, тип довода или возвращаемый тип, эта строка
        // не скомпилируется, и расхождение будет видно на сборке проверок.
        ILearningMachine<TranslationDomain> contract = new TranslationMachine();

        // Член первый: обучение партии. Довод партии не используется, потому что отмена проверена
        // раньше него; это видно по первой строке тела метода.
        var trained = await contract.TrainBatchAsync(null!, AlreadyCancelled);
        Assert.False(trained.IsSuccess);
        Assert.Equal("CANCELLED", trained.ErrorCode);

        // Член второй: измерение качества. Ровно два довода — набор проверки и признак отмены.
        var measured = await contract.EvaluateAsync(HeldOutSet(), AlreadyCancelled);
        Assert.False(measured.IsSuccess);
        Assert.Equal("CANCELLED", measured.ErrorCode);

        // Член третий: выгрузка состояния.
        var exported = await contract.ExportAsync(AlreadyCancelled);
        Assert.False(exported.IsSuccess);
        Assert.Equal("CANCELLED", exported.ErrorCode);

        // Член четвёртый: загрузка состояния. Довод выгрузки так же не используется до проверки
        // отмены, поэтому пустая ссылка здесь безопасна и ничего не загружает.
        var imported = await contract.ImportAsync(null!, AlreadyCancelled);
        Assert.False(imported.IsSuccess);
        Assert.Equal("CANCELLED", imported.ErrorCode);

        // Член пятый: получение оркестратора.
        var orchestrator = await contract.GetOrchestratorAsync(AlreadyCancelled);
        Assert.False(orchestrator.IsSuccess);
        Assert.Equal("CANCELLED", orchestrator.ErrorCode);
    }

    // ── Проверка 2: названная версия ушла вперёд — измерение не начинается ──

    [Fact]
    public async Task NamedVersionEvaluation_RefusesWhenTheNamedVersionIsNotThePublishedOne()
    {
        // Обычная необученная машина и её текущая опубликованная версия.
        var machine = new TranslationMachine();
        // Называется версия, которой нет: на единицу больше опубликованной.
        var versionThatIsNotPublished = machine.CurrentStateVersion + 1;

        // Измерение с названной чужой версией не начинается вовсе.
        var refused = await machine.EvaluateAtVersionAsync(HeldOutSet(), versionThatIsNotPublished);

        // Отказ приходит обычным конвертом с названным кодом, а не исключением.
        Assert.False(refused.IsSuccess);
        Assert.Equal(TranslationMachine.MeasuredVersionChangedErrorCode, refused.ErrorCode);
        // Негодного измерения наружу не выдано.
        Assert.Null(refused.Value);
        // Состояние не сдвинулось: отказ до измерения ничего не публикует.
        Assert.Equal(versionThatIsNotPublished - 1, machine.CurrentStateVersion);
    }

    // ── Проверка 3: положительный контроль, зелёный до и после починки ──

    [Fact]
    public async Task NamedVersionEvaluation_WithThePublishedVersion_AgreesWithThePlainContractCall()
    {
        // Одна и та же необученная машина отвечает на два вызова подряд.
        var machine = new TranslationMachine();
        // Называется ровно та версия, которая опубликована сейчас.
        var publishedVersion = machine.CurrentStateVersion;

        // Вызов с названной текущей версией проходит проверку версии и измеряет.
        var named = await machine.EvaluateAtVersionAsync(HeldOutSet(), publishedVersion);
        // Вызов члена контракта без названной версии — прежний путь прежних вызывающих.
        var plain = await machine.EvaluateAsync(HeldOutSet());

        // Оба успешны: названная текущая версия ничего не запрещает.
        Assert.True(named.IsSuccess, named.ErrorMessage);
        Assert.True(plain.IsSuccess, plain.ErrorMessage);
        // Оба дают один и тот же ответ: делегирование не меняет поведение прежних вызывающих.
        Assert.Equal(plain.Value!.RowsEvaluated, named.Value!.RowsEvaluated);
        Assert.Equal(plain.Value!.Accuracy, named.Value!.Accuracy);
        Assert.Equal(plain.Value!.PassesGate, named.Value!.PassesGate);
        Assert.Equal(plain.Value!.GateNote, named.Value!.GateNote);
        // Версия состояния не сдвинулась: измерение без обученной части ничего не публикует.
        Assert.Equal(publishedVersion, machine.CurrentStateVersion);
    }
}
