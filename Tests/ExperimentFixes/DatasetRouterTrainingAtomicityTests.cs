// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие инварианта «отказ новой партии сохраняет принятую модель»
// (четвёртая работа плана ремонта 102 по позднему аудиту двадцать девятого
// октября, папка docs/plans/replanning-possible-diffs/fixes-based-on-experiments).
//
// Что здесь проверяется по-человечески. Маршрутизатор наборов данных учится на
// партиях уроков: партия приходит целиком и целиком становится новой памятью.
// Вопрос этой проверки один: что происходит с УЖЕ ПРИНЯТОЙ памятью, если новая
// партия оказалась негодной — её не передали вовсе, внутри урока вместо массива
// пустая ссылка, или чтение партии оборвалось на полпути. Обученная память,
// которую никто не просил выбрасывать, обязана пережить такой отказ целиком.
//
// Что было сломано. Прежний приём партии сначала очищал память, и только потом
// пытался прочитать новую партию. Любая беда с новой партией заставала память
// уже пустой: один неудачный вызов обучения молча стирал всё, чему маршрутизатор
// научился раньше, и следующий же ответ приходил из пустоты. Третья работа того
// же плана ремонта перестроила приём партии ради другой цели — владения
// входными массивами, — и побочно закрыла и эту беду: новая таблица теперь
// собирается в стороне и публикуется последней строкой. Эти проверки закрепляют
// полученное поведение, чтобы прежний порядок «очистить, потом читать» нельзя
// было вернуть молча.
//
// Как это доказывается. Сначала маршрутизатор обучается действительной партией,
// и его ответ снимается как снимок — буквальным ожиданием, а не копией самого
// же ответа. Затем подаётся негодная партия. Требуется ровно два исхода сразу:
// вызов обязан отказать, и следующее чтение обязано вернуть тот же снимок.
// Для обрыва чтения сделана управляемая партия, которая отдаёт первую строку и
// бросает ошибку до конца перебора; её же исправная разновидность работает
// положительным контролем, чтобы отказ нельзя было списать на саму заготовку.
//
// Чего эти проверки НЕ делают. Они не считают отказ на негодном материале
// доказательством того, что действительная замена работает: на это есть
// отдельные положительные контроли — успешная замена, успешное чтение через ту
// же заготовку, короткая и длинная маски. Они не закрепляют КОНКРЕТНЫЙ тип
// исключения: договор этой работы — «отказ происходит и память цела», а не
// «отказ называется так-то». Сегодня все четыре отказа приходят как обращение
// по пустой ссылке; если отказ позже станет осмысленной ошибкой аргумента, эти
// проверки останутся верными и не потребуют правки.
//
// Отдельно закреплено главное свойство успешного приёма: партия ЗАМЕНЯЕТ весь
// набор уроков, а не дописывается к нему. Случай замены подобран так, что при
// дописывании ответ остался бы прежним, — поэтому он ловит и тихое превращение
// замены в дописывание, из-за которого повторный прогон начал бы дублировать
// уроки.
//
// Число корзин здесь намеренно не шесть, а два и три: договор это допускает, и
// жёстко вшитая шестёрка проверяла бы один частный продуктовый список имён.
//
// Бюджет этой проверки. Девять маленьких памятей из одного-двух уроков прямо в
// оперативной памяти. Ни одного обучения с учителем, ни одной пары
// предпочтений, ни одного обращения к диску.

using LogicExtractor = SemanticCompiler.LogicExtractor;

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class DatasetRouterTrainingAtomicityTests
{
    // Ровно один ближайший сосед: ответ целиком определяется одним уроком,
    // поэтому сравнение «до отказа» и «после отказа» идёт без примеси голосования.
    private const int ExactNearestNeighbour = 1;

    // Две корзины данных. Договор допускает такое число, и выбор прямо
    // показывает, что проверка не привязана к шести продуктовым именам.
    private const int TwoBuckets = 2;

    // ГЛАВНЫЙ ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ. Действительная партия принимается и
    // ЗАМЕНЯЕТ весь набор уроков. Случай подобран так, что при дописывании
    // вместо замены прежние два урока перевесили бы новый и ответ НЕ изменился
    // бы, — значит этот же случай ловит тихое превращение замены в дописывание.
    [Fact]
    public void SuccessfulBatchReplacesTheWholeMemoryAndChangesTheAnswer()
    {
        // Три соседа: при дописывании в голосовании участвовали бы все три урока.
        var router = new AgentAlgorithms.PredicateDatasetRouter(k: 3, datasetCount: TwoBuckets);

        // Первая партия: два урока, оба голосуют за первую корзину.
        router.Train(
        [
            (new[] { LogicExtractor.ITERATE }, new[] { true, false }),
            (new[] { LogicExtractor.ITERATE, LogicExtractor.BRANCH }, new[] { true, false })
        ]);

        // Запрос совпадает с признаками первого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Ответ первой памяти: два голоса из двух за первую корзину, большинство есть.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Вторая партия: один урок, голосующий за вторую корзину.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { false, true })]);

        // Ответ обязан стать ответом новой памяти целиком: единственный урок за
        // вторую корзину. При дописывании здесь остались бы прежние [да, нет].
        Assert.Equal(new[] { false, true }, router.Predict(query));
    }

    // ГЛАВНЫЙ СЛУЧАЙ ОДИН. Партии нет вовсе — вместо неё пустая ссылка.
    // Обучение обязано отказать, а прежняя память — остаться на месте.
    [Fact]
    public void BatchGivenAsANullReference_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок принятой памяти до негодной партии, ожидание выписано буквально.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Негодная партия: вместо списка уроков пустая ссылка.
        var rejection = Record.Exception(() => router.Train(null!));

        // Отказ обязан произойти: молча принять «ничто» вместо партии нельзя.
        Assert.NotNull(rejection);

        // Главное: принятая память пережила отказ и отвечает ровно как раньше.
        // При прежнем порядке «очистить, потом читать» память была бы пуста и
        // пустая память отвечает «подходят все корзины», то есть [да, да].
        Assert.Equal(new[] { true, false }, router.Predict(query));
    }

    // ГЛАВНЫЙ СЛУЧАЙ ДВА. Партия есть, но внутри её единственного урока вместо
    // массива признаков пустая ссылка.
    [Fact]
    public void BatchWhoseLessonHasANullFeatureArray_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок принятой памяти до негодной партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Негодная партия: у урока нет массива признаков, а метки голосуют
        // за вторую корзину — то есть принятие этого урока было бы заметно.
        var rejection = Record.Exception(
            () => router.Train([(null!, new[] { false, true })]));

        // Отказ обязан произойти: урок без признаков в память попасть не может.
        Assert.NotNull(rejection);

        // Принятая память цела: ответ тот же, что и до негодной партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));
    }

    // ГЛАВНЫЙ СЛУЧАЙ ТРИ. Партия есть, но внутри её единственного урока вместо
    // массива меток корзин пустая ссылка.
    [Fact]
    public void BatchWhoseLessonHasANullLabelArray_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок принятой памяти до негодной партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Негодная партия: признаки у урока есть, а маски корзин нет.
        var rejection = Record.Exception(
            () => router.Train([(new[] { LogicExtractor.BRANCH }, null!)]));

        // Отказ обязан произойти: урок без меток корзин ничему не учит.
        Assert.NotNull(rejection);

        // Принятая память цела: ответ тот же, что и до негодной партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));
    }

    // ГЛАВНЫЙ СЛУЧАЙ ЧЕТЫРЕ. Чтение партии обрывается на полпути: первая строка
    // уже отдана, вторую источник отдать не смог. Именно этот случай прежний
    // порядок переживал хуже всего — память была уже очищена, а новая принята
    // наполовину.
    [Fact]
    public void BatchThatFailsWhileBeingRead_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок принятой памяти до негодной партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Управляемая партия из двух строк, которая обрывается после первой.
        // Первая строка голосует за вторую корзину: если бы половина партии
        // осела в памяти, ответ на тот же запрос стал бы другим.
        var batch = new BatchThatFailsWhileBeingRead(OppositeVotingRows(), failAfterRows: 1);

        // Обучение на такой партии обязано отказать.
        var rejection = Record.Exception(() => router.Train(batch));

        // Отказ обязан произойти: оборванное чтение не может считаться партией.
        Assert.NotNull(rejection);

        // Принятая память цела: ни одна строка оборванной партии в неё не легла.
        Assert.Equal(new[] { true, false }, router.Predict(query));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ЗАГОТОВКИ. Та же управляемая партия, но без обрыва.
    // Он нужен, чтобы сохранность памяти в случае выше нельзя было объяснить
    // тем, что заготовка вообще непригодна для чтения.
    [Fact]
    public void BatchReadThroughTheSameHarnessWithoutAFailure_IsAcceptedNormally()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок принятой памяти до новой партии.
        Assert.Equal(new[] { true, false }, router.Predict(query));

        // Та же заготовка и те же строки, но обрыв не назначен.
        var batch = new BatchThatFailsWhileBeingRead(OppositeVotingRows(), failAfterRows: -1);

        // Эта партия обязана приняться как обычная.
        router.Train(batch);

        // Ответ стал ответом новой памяти: ближайший урок голосует за вторую корзину.
        Assert.Equal(new[] { false, true }, router.Predict(query));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ПОЛИТИКИ ШИРИНЫ, КОРОТКАЯ МАСКА. Маска новой партии
    // короче числа корзин, но это не повод для отказа: действующая политика
    // принимает её и дополняет недостающие корзины значением «нет». Новый
    // порядок приёма не вправе ужесточать эту политику.
    [Fact]
    public void ReplacementBatchWithAMaskShorterThanTheBucketCount_IsAcceptedAndPaddedWithNo()
    {
        // Корзин три, а маска новой партии будет длиной один.
        const int threeBuckets = 3;

        // Маршрутизатор с одним ближайшим соседом и тремя корзинами.
        var router = new AgentAlgorithms.PredicateDatasetRouter(
            k: ExactNearestNeighbour, datasetCount: threeBuckets);

        // Первая партия полной ширины: названа только третья корзина.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { false, false, true })]);

        // Запрос совпадает с признаками урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок первой памяти, ожидание выписано буквально.
        Assert.Equal(new[] { false, false, true }, router.Predict(query));

        // Вторая партия с короткой маской: названа только первая корзина.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { true })]);

        // Партия принята, ответ сменился, недостающие корзины дополнены «нет».
        Assert.Equal(new[] { true, false, false }, router.Predict(query));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ПОЛИТИКИ ШИРИНЫ, ДЛИННАЯ МАСКА. Маска новой партии
    // длиннее числа корзин. Действующая политика принимает её и игнорирует
    // лишние элементы; новый порядок приёма обязан сохранить и это.
    [Fact]
    public void ReplacementBatchWithAMaskLongerThanTheBucketCount_IsAcceptedAndTheExtrasAreIgnored()
    {
        // Маршрутизатор с одним ближайшим соседом и двумя корзинами.
        var router = new AgentAlgorithms.PredicateDatasetRouter(
            k: ExactNearestNeighbour, datasetCount: TwoBuckets);

        // Первая партия ровно по ширине: названа вторая корзина.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { false, true })]);

        // Запрос совпадает с признаками урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Снимок первой памяти, ожидание выписано буквально.
        Assert.Equal(new[] { false, true }, router.Predict(query));

        // Вторая партия с длинной маской: третий элемент истинен, корзины с таким номером нет.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { true, false, true })]);

        // Партия принята, ответ сменился, лишний элемент маски не виден.
        Assert.Equal(new[] { true, false }, router.Predict(query));
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ПОСЛЕ ОТКАЗА. Отказ не должен оставлять
    // маршрутизатор в нерабочем состоянии: следующая действительная партия
    // обязана приняться как обычно.
    [Fact]
    public void RouterRejectedANullBatch_StillAcceptsTheNextValidBatch()
    {
        // Маршрутизатор с принятой памятью из одного урока за первую корзину.
        var router = TrainedRouterWithFirstBucketLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { LogicExtractor.ITERATE };

        // Негодная партия отклонена.
        Assert.NotNull(Record.Exception(() => router.Train(null!)));

        // Следующая действительная партия: один урок за вторую корзину.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { false, true })]);

        // Ответ стал ответом новой памяти: отказ не сломал приём партий.
        Assert.Equal(new[] { false, true }, router.Predict(query));
    }

    // Общая исходная память всех случаев отказа: один урок, голосующий за
    // первую корзину. Вынесена в помощник, чтобы снимок «до отказа» в каждом
    // случае был заведомо одним и тем же.
    private static AgentAlgorithms.PredicateDatasetRouter TrainedRouterWithFirstBucketLesson()
    {
        // Один ближайший сосед и две корзины: ответ определяется одним уроком.
        var router = new AgentAlgorithms.PredicateDatasetRouter(
            k: ExactNearestNeighbour, datasetCount: TwoBuckets);

        // Единственный принятый урок: признак цикла, голос за первую корзину.
        router.Train([(new[] { LogicExtractor.ITERATE }, new[] { true, false })]);

        // Готовый маршрутизатор с принятой памятью возвращается случаю проверки.
        return router;
    }

    // Строки управляемой партии: обе голосуют ПРОТИВ принятой памяти, за вторую
    // корзину. Поэтому попадание в память даже одной из них сразу видно по ответу.
    private static IReadOnlyList<(int[] Predicates, bool[] Datasets)> OppositeVotingRows() =>
    [
        (new[] { LogicExtractor.ITERATE }, new[] { false, true }),
        (new[] { LogicExtractor.BRANCH }, new[] { false, true })
    ];

    // Управляемый источник партии. Он честно сообщает число строк, но при
    // назначенном обрыве бросает ошибку ДО конца перебора — так воспроизводится
    // источник, который сломался на полпути (оборванный поток, испорченный файл,
    // отвалившаяся выборка). Назначение обрыва минус один означает «не обрывать»,
    // и тогда тот же класс служит положительным контролем.
    private sealed class BatchThatFailsWhileBeingRead
        : IReadOnlyList<(int[] Predicates, bool[] Datasets)>
    {
        // Строки, которые источник готов отдать.
        private readonly IReadOnlyList<(int[] Predicates, bool[] Datasets)> _rows;

        // После скольких отданных строк оборвать чтение; минус один — не обрывать.
        private readonly int _failAfterRows;

        // Источник собирается из готовых строк и назначенного места обрыва.
        public BatchThatFailsWhileBeingRead(
            IReadOnlyList<(int[] Predicates, bool[] Datasets)> rows, int failAfterRows)
        {
            // Строки запоминаются как есть: подменять материал проверки нельзя.
            _rows = rows;

            // Место обрыва запоминается отдельно от строк.
            _failAfterRows = failAfterRows;
        }

        // Число строк сообщается честно и без ошибки: обрыв проверяется именно
        // на чтении, а не на опросе размера.
        public int Count => _rows.Count;

        // Доступ по номеру отдаёт исходную строку без изменений.
        public (int[] Predicates, bool[] Datasets) this[int index] => _rows[index];

        // Перебор отдаёт строки по одной и обрывается на назначенном месте.
        public IEnumerator<(int[] Predicates, bool[] Datasets)> GetEnumerator()
        {
            // Строки отдаются по порядку, как у обычного списка.
            for (var index = 0; index < _rows.Count; index++)
            {
                // На назначенном месте источник ломается, не дойдя до конца партии.
                if (_failAfterRows >= 0 && index == _failAfterRows)
                    throw new InvalidOperationException(
                        "Чтение партии уроков оборвалось: источник отдал не все строки.");

                // До места обрыва строки отдаются обычным образом.
                yield return _rows[index];
            }
        }

        // Нетипизированный перебор сводится к типизированному: иного поведения нет.
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
