// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие двух инвариантов памяти КАНОНИЧЕСКОГО МАРШРУТИЗАТОРА
// ЗАПРОСОВ (файл 29.SemanticCompiler/Routing/RouteClassifier.cs): «память
// принадлежит маршрутизатору» и «отказ новой партии сохраняет принятую память».
//
// Что здесь проверяется по-человечески. Маршрутизатор запросов по признакам
// запроса решает, к какой области относится задача: ошибка, план, архитектура,
// вопрос по коду, переработка. Учат его партией уроков: каждый урок — это
// массив признаков плюс правильная метка области. Вопросов у этих проверок
// ровно два. Первый: чья это память после обучения — маршрутизатора или того,
// кто передал партию. Второй: что происходит с уже принятой памятью, если
// новая партия оказалась негодной.
//
// Что было сломано. Прежний приём партии состоял из двух строк: сначала
// «очистить таблицу», потом «добавить в неё переданные уроки». Отсюда обе беды
// сразу. Первая: в таблицу ложились САМИ массивы признаков вызывающей стороны,
// а не их копии, поэтому любое позднее изменение этих массивов снаружи молча
// меняло обученную память — один и тот же запрос после такого изменения
// начинал отвечать другой областью. Вторая: очистка шла ДО разбора новой
// партии, поэтому негодная партия заставала память уже пустой — один неудачный
// вызов обучения стирал всё, чему маршрутизатор научился раньше, и следующий
// же ответ приходил отказом «не обучен».
//
// Эти же две беды уже были найдены и закрыты у соседнего владельца —
// маршрутизатора наборов данных (22.AgentAlgorithms/Context/
// PredicateDatasetRouter.cs). Там договор приёма партии верен и служит
// проверенным образцом; здесь переносится ровно он, на другого владельца.
//
// Как это доказывается. Сначала маршрутизатор обучается действительной партией,
// и его ответ снимается как снимок — буквальным ожиданием, а не копией самого
// же ответа. Затем происходит ровно одно вредное событие: внешнее изменение
// учебного массива, либо негодная партия. Требуется, чтобы ответ остался
// прежним, а в случае негодной партии — ещё и чтобы вызов отказал. Для обрыва
// чтения сделана управляемая партия, которая отдаёт первую строку и бросает
// ошибку до конца перебора; её же исправная разновидность работает
// положительным контролем, чтобы отказ нельзя было списать на саму заготовку.
//
// Чувствительность. Нигде не сравнивается число уроков: сравниваются ФАКТИЧЕСКИЕ
// ответы маршрутизатора на один и тот же запрос. Случаи подобраны так, что при
// прежней редакции кода ответ заведомо иной: когда память делит массивы с
// вызывающей стороной, ближайшим соседом становится другой урок и область
// меняется с «план» на «вопрос по коду»; а при очистке до разбора новой партии
// ответ становится отказом «не обучен».
//
// Чего эти проверки НЕ делают. Они не закрепляют КОНКРЕТНЫЙ тип исключения:
// договор этой работы — «отказ происходит и память цела», а не «отказ
// называется так-то». Они не измеряют и не заявляют никакого прироста точности
// распознавания области: это договор владения памятью, а не улучшение качества.
// Они не доказывают, что обученный путь готов к подключению к публичному
// фасаду, — они лишь убирают то, что делало такое подключение небезопасным.
//
// Бюджет этой проверки. Десяток маленьких памятей из одного-трёх уроков прямо в
// оперативной памяти. Ни одного обновления весов, ни одной пары предпочтений,
// ни одного обращения к диску или к внешней модели.

using SemanticCompiler29;

namespace LlmFromScratch.Tests.ExperimentFixes;

public sealed class RouteClassifierMemoryOwnershipAndAtomicityTests
{
    // Ровно один ближайший сосед: ответ целиком определяется одним уроком,
    // поэтому сравнение «до события» и «после события» идёт без примеси голосования.
    private const int ExactNearestNeighbour = 1;

    // Два признака, общих для запроса и первого урока. Числа здесь — это номера
    // ячеек профиля признаков; какие именно, маршрутизатору безразлично, он
    // сравнивает наборы номеров между собой.
    private const int SharedSlotOne = 1;
    private const int SharedSlotTwo = 2;

    // Третий признак есть только у второго урока: он делает второй урок чуть
    // дальше от запроса, чем первый, и поэтому первый урок выигрывает.
    private const int ExtraSlotThree = 3;

    // Два заведомо далёких признака. Ими снаружи подменяется первый учебный
    // массив: после подмены первый урок обязан был бы стать дальше второго —
    // если бы память маршрутизатора действительно была чужой.
    private const int FarSlotFifty = 50;
    private const int FarSlotFiftyOne = 51;

    // ПУНКТ ПРИЁМКИ ОДИН. Чужая память у канонического маршрутизатора.
    // Два урока, один ближайший сосед. Запрос сначала отвечает «план». Затем
    // снаружи меняется первый учебный массив признаков. Тот же запрос обязан
    // по-прежнему отвечать «план», а не «вопрос по коду».
    [Fact]
    public void ExternalChangeOfTheFirstLessonFeatures_DoesNotChangeTheTrainedAnswer()
    {
        // Маршрутизатор с одним ближайшим соседом: ответ даст ровно один урок.
        var classifier = NewClassifier(ExactNearestNeighbour);

        // Массив признаков первого урока остаётся в руках вызывающей стороны —
        // именно его мы изменим снаружи после обучения.
        var firstLessonFeatures = new[] { SharedSlotOne, SharedSlotTwo };

        // Партия из двух уроков: первый учит области «план», второй — «вопрос по коду».
        classifier.Train(
        [
            (firstLessonFeatures, Domain.PLAN),
            (new[] { SharedSlotOne, SharedSlotTwo, ExtraSlotThree }, Domain.CODE_Q)
        ]);

        // Запрос точно совпадает с признаками первого урока, поэтому он ближе второго.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти: ожидание выписано буквально, а не взято из самого ответа.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ сразу после обучения");

        // Вредное событие: владелец массива меняет его у себя, про маршрутизатор не думая.
        firstLessonFeatures[0] = FarSlotFifty;

        // Второй элемент меняется тем же порядком: теперь массив вызывающей стороны далёк от запроса.
        firstLessonFeatures[1] = FarSlotFiftyOne;

        // Главное требование: обученная память не принадлежит вызывающей стороне,
        // поэтому ответ обязан остаться прежним. При прежней редакции кода в
        // памяти лежала ССЫЛКА на этот массив, первый урок становился дальше
        // второго, и ответ менялся на «вопрос по коду».
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ после внешнего изменения учебного массива");
    }

    // ПУНКТ ПРИЁМКИ ДВА, ПУСТАЯ ПАРТИЯ. Обучение пустой партией после принятого
    // обучения обязано отказать, а прежний ответ обязан сохраниться. Пустая
    // таблица у этого владельца означает «не обучен», то есть молчаливое
    // принятие пустой партии равносильно потере всей памяти.
    [Fact]
    public void EmptyBatchAfterAcceptedTraining_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ до пустой партии");

        // Негодная партия: уроков в ней нет вовсе.
        var rejection = Record.Exception(() => classifier.Train([]));

        // Отказ обязан произойти: партия без уроков ничему не учит и памятью стать не может.
        Assert.True(rejection is not null, "Обучение пустой партией обязано отказать, а оно прошло молча.");

        // Главное: принятая память пережила отказ и отвечает ровно как раньше.
        // При прежней редакции кода здесь был бы отказ с кодом «не обучен».
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ после отказа на пустой партии");
    }

    // ПУНКТ ПРИЁМКИ ДВА, ОТСУТСТВУЮЩАЯ ПАРТИЯ. Вместо партии передана пустая
    // ссылка: тот же инвариант.
    [Fact]
    public void BatchGivenAsANullReference_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ до отсутствующей партии");

        // Негодная партия: вместо списка уроков пустая ссылка.
        var rejection = Record.Exception(() => classifier.Train(null!));

        // Отказ обязан произойти: молча принять «ничто» вместо партии нельзя.
        Assert.True(rejection is not null, "Обучение отсутствующей партией обязано отказать, а оно прошло молча.");

        // Принятая память цела: ответ тот же, что и до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ после отказа на отсутствующей партии");
    }

    // ПУНКТ ПРИЁМКИ ТРИ. Партия есть, но внутри её единственного урока вместо
    // массива признаков пустая ссылка.
    [Fact]
    public void BatchWhoseLessonHasANullFeatureArray_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ до партии с пустой ссылкой вместо признаков");

        // Негодная партия: у урока нет массива признаков, а метка области — другая,
        // то есть принятие такого урока было бы сразу заметно по ответу.
        var rejection = Record.Exception(
            () => classifier.Train([(null!, Domain.CODE_Q)]));

        // Отказ обязан произойти: урок без признаков в память попасть не может.
        Assert.True(rejection is not null, "Урок без массива признаков обязан быть отвергнут, а он был принят молча.");

        // Принятая память цела: ответ тот же, что и до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ после отказа на уроке без признаков");
    }

    // ПУНКТ ПРИЁМКИ ЧЕТЫРЕ. Чтение партии обрывается на полпути: первая строка
    // уже отдана, вторую источник отдать не смог. Именно этот случай прежний
    // порядок переживал хуже всего — память была уже очищена, а новая принята
    // наполовину.
    [Fact]
    public void BatchThatFailsWhileBeingRead_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до негодной партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ до оборванной партии");

        // Управляемая партия из двух строк, которая обрывается после первой.
        // Первая строка учит другой области: если бы половина партии осела в
        // памяти, ответ на тот же запрос стал бы другим.
        var batch = new BatchThatFailsWhileBeingRead(OppositeTeachingRows(), failAfterRows: 1);

        // Обучение на такой партии обязано отказать.
        var rejection = Record.Exception(() => classifier.Train(batch));

        // Отказ обязан произойти: оборванное чтение не может считаться партией.
        Assert.True(rejection is not null, "Оборванное чтение партии обязано отказать, а обучение завершилось успехом.");

        // Принятая память цела: ни одна строка оборванной партии в неё не легла.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ после отказа на оборванной партии");
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ЗАГОТОВКИ. Та же управляемая партия, но без обрыва.
    // Он нужен, чтобы сохранность памяти в случае выше нельзя было объяснить
    // тем, что заготовка вообще непригодна для чтения.
    [Fact]
    public void BatchReadThroughTheSameHarnessWithoutAFailure_IsAcceptedNormally()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до новой партии.
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ до партии через ту же заготовку");

        // Та же заготовка и те же строки, но обрыв не назначен.
        var batch = new BatchThatFailsWhileBeingRead(OppositeTeachingRows(), failAfterRows: -1);

        // Эта партия обязана приняться как обычная.
        classifier.Train(batch);

        // Ответ стал ответом новой памяти: ближайший урок учит области «вопрос по коду».
        AssertAnswer(Domain.CODE_Q, classifier.Classify(query), "ответ после исправной партии через ту же заготовку");
    }

    // ПУНКТ ПРИЁМКИ ПЯТЬ. Действительная новая партия ЗАМЕНЯЕТ весь набор
    // уроков и меняет ответ. Случай подобран так, что при дописывании вместо
    // замены прежние два урока перевесили бы новый и ответ НЕ изменился бы, —
    // значит этот же случай ловит тихое превращение замены в дописывание.
    [Fact]
    public void ValidReplacementBatchChangesTheAnswerAndDoesNotAppend()
    {
        // Три соседа: при дописывании в голосовании участвовали бы все три урока.
        var classifier = NewClassifier(k: 3);

        // Первая партия: два урока, оба учат области «план».
        classifier.Train(
        [
            (new[] { SharedSlotOne, SharedSlotTwo }, Domain.PLAN),
            (new[] { SharedSlotOne, SharedSlotTwo, ExtraSlotThree }, Domain.PLAN)
        ]);

        // Запрос совпадает с признаками первого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Ответ первой памяти: оба урока за «план».
        AssertAnswer(Domain.PLAN, classifier.Classify(query), "ответ первой памяти");

        // Вторая партия: один урок, который учит области «архитектура».
        classifier.Train([(new[] { SharedSlotOne, SharedSlotTwo }, Domain.ARCH)]);

        // Ответ обязан стать ответом новой памяти целиком: единственный урок за
        // «архитектуру». При дописывании здесь осталось бы прежнее «план».
        AssertAnswer(Domain.ARCH, classifier.Classify(query), "ответ после действительной замены партии");
    }

    // ПОЛОЖИТЕЛЬНЫЙ КОНТРОЛЬ ПОСЛЕ ОТКАЗА. Отказ не должен оставлять
    // маршрутизатор в нерабочем состоянии: следующая действительная партия
    // обязана приняться как обычно.
    [Fact]
    public void ClassifierRejectedABadBatch_StillAcceptsTheNextValidBatch()
    {
        // Маршрутизатор с принятой памятью из одного урока области «план».
        var classifier = TrainedClassifierWithPlanLesson();

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Негодная партия отклонена.
        Assert.True(
            Record.Exception(() => classifier.Train(null!)) is not null,
            "Обучение отсутствующей партией обязано отказать, а оно прошло молча.");

        // Следующая действительная партия: один урок области «архитектура».
        classifier.Train([(new[] { SharedSlotOne, SharedSlotTwo }, Domain.ARCH)]);

        // Ответ стал ответом новой памяти: отказ не сломал приём партий.
        AssertAnswer(Domain.ARCH, classifier.Classify(query), "ответ после отказа и следующей действительной партии");
    }

    // ПУНКТ ПРИЁМКИ ШЕСТЬ, СОВМЕСТИМОСТНАЯ ОБОЛОЧКА. Тот же инвариант владения
    // памятью обязан держаться и через оболочку 12.SemanticCompiler, потому что
    // вся её работа — это передача партии канону.
    [Fact]
    public void CompatibilityWrapper_ExternalChangeOfLessonFeatures_DoesNotChangeTheAnswer()
    {
        // Оболочка с одним ближайшим соседом: ответ даст ровно один урок.
        var wrapper = new SemanticCompiler.RouteClassifier(ExactNearestNeighbour);

        // Массив признаков первого урока остаётся в руках вызывающей стороны.
        var firstLessonFeatures = new[] { SharedSlotOne, SharedSlotTwo };

        // Партия из двух уроков: первый учит «плану», второй — «вопросу по коду».
        wrapper.Train(
        [
            (firstLessonFeatures, Domain.PLAN),
            (new[] { SharedSlotOne, SharedSlotTwo, ExtraSlotThree }, Domain.CODE_Q)
        ]);

        // Запрос совпадает с признаками первого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти через оболочку.
        AssertAnswer(Domain.PLAN, wrapper.Classify(query), "ответ оболочки сразу после обучения");

        // Внешнее изменение первого учебного массива.
        firstLessonFeatures[0] = FarSlotFifty;

        // Второй элемент меняется тем же порядком.
        firstLessonFeatures[1] = FarSlotFiftyOne;

        // Ответ оболочки обязан остаться прежним: память принадлежит канону, а не вызывающей стороне.
        AssertAnswer(Domain.PLAN, wrapper.Classify(query), "ответ оболочки после внешнего изменения учебного массива");
    }

    // ПУНКТ ПРИЁМКИ ШЕСТЬ, СОВМЕСТИМОСТНАЯ ОБОЛОЧКА, ОТКАЗ. Пустая партия через
    // оболочку обязана отказать и сохранить принятую память.
    [Fact]
    public void CompatibilityWrapper_EmptyBatch_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Оболочка с одним ближайшим соседом.
        var wrapper = new SemanticCompiler.RouteClassifier(ExactNearestNeighbour);

        // Принятая память из одного урока области «план».
        wrapper.Train([(new[] { SharedSlotOne, SharedSlotTwo }, Domain.PLAN)]);

        // Запрос совпадает с признаками принятого урока.
        var query = new[] { SharedSlotOne, SharedSlotTwo };

        // Снимок принятой памяти до негодной партии.
        AssertAnswer(Domain.PLAN, wrapper.Classify(query), "ответ оболочки до пустой партии");

        // Негодная партия через оболочку.
        var rejection = Record.Exception(() => wrapper.Train([]));

        // Отказ обязан дойти до вызывающей стороны, а не раствориться в оболочке.
        Assert.True(rejection is not null, "Пустая партия через оболочку обязана отказать, а она прошла молча.");

        // Принятая память цела.
        AssertAnswer(Domain.PLAN, wrapper.Classify(query), "ответ оболочки после отказа на пустой партии");
    }

    // ПУНКТ ПРИЁМКИ ШЕСТЬ, ПОТРЕБИТЕЛЬ. Тот же инвариант владения памятью
    // обязан держаться у потребителя 23.SemanticCompilerAdapter: он обучает
    // маршрутизатор и сам же читает его ответ.
    [Fact]
    public void ConsumerUnit_ExternalChangeOfLessonFeatures_DoesNotChangeTheAnswer()
    {
        // Потребитель поверх оболочки с одним ближайшим соседом.
        var unit = new SemanticCompilerAdapter.DomainClassifierUnit(
            new SemanticCompiler.RouteClassifier(ExactNearestNeighbour));

        // Массив признаков первого урока остаётся в руках вызывающей стороны.
        var firstLessonFeatures = new[] { SharedSlotOne, SharedSlotTwo };

        // Партия из двух уроков: первый учит «плану», второй — «вопросу по коду».
        unit.Train(
        [
            (firstLessonFeatures, Domain.PLAN),
            (new[] { SharedSlotOne, SharedSlotTwo, ExtraSlotThree }, Domain.CODE_Q)
        ]);

        // Запрос потребителя: текст без ключевых слов плюс тот же профиль признаков.
        var request = ConsumerRequest();

        // Снимок ответа потребителя: он идёт обученным путём ближайших соседей.
        AssertConsumerAnswer(Domain.PLAN, unit.Classify(request), "ответ потребителя сразу после обучения");

        // Внешнее изменение первого учебного массива.
        firstLessonFeatures[0] = FarSlotFifty;

        // Второй элемент меняется тем же порядком.
        firstLessonFeatures[1] = FarSlotFiftyOne;

        // Ответ потребителя обязан остаться прежним.
        AssertConsumerAnswer(Domain.PLAN, unit.Classify(request), "ответ потребителя после внешнего изменения учебного массива");
    }

    // ПУНКТ ПРИЁМКИ ШЕСТЬ, ПОТРЕБИТЕЛЬ, ОТКАЗ. Пустая партия у потребителя
    // обязана отказать, а прежний обученный ответ — сохраниться. При прежней
    // редакции кода потребитель здесь молча считал себя необученным и отвечал
    // по запасному пути сходства признаков, то есть другой областью.
    [Fact]
    public void ConsumerUnit_EmptyBatch_IsRejectedAndKeepsTheAcceptedMemory()
    {
        // Потребитель поверх оболочки с одним ближайшим соседом.
        var unit = new SemanticCompilerAdapter.DomainClassifierUnit(
            new SemanticCompiler.RouteClassifier(ExactNearestNeighbour));

        // Принятая память из одного урока области «план».
        unit.Train([(new[] { SharedSlotOne, SharedSlotTwo }, Domain.PLAN)]);

        // Запрос потребителя: текст без ключевых слов плюс тот же профиль признаков.
        var request = ConsumerRequest();

        // Снимок ответа потребителя до негодной партии.
        AssertConsumerAnswer(Domain.PLAN, unit.Classify(request), "ответ потребителя до пустой партии");

        // Негодная партия у потребителя.
        var rejection = Record.Exception(() => unit.Train([]));

        // Отказ обязан дойти до вызывающей стороны.
        Assert.True(rejection is not null, "Пустая партия у потребителя обязана отказать, а она прошла молча.");

        // Принятая память цела, обученный путь по-прежнему отвечает «планом».
        AssertConsumerAnswer(Domain.PLAN, unit.Classify(request), "ответ потребителя после отказа на пустой партии");
    }

    // Общий запрос потребителя. Текст подобран так, чтобы в нём не было ни
    // ключевых слов областей, ни прямого вопросительного оборота: тогда
    // различие между обученным путём и запасным видно без примесей.
    private static SemanticCompilerAdapter.DomainClassifierRequest ConsumerRequest() =>
        new("tvoi tekst bez klyuchevyh slov", new[] { SharedSlotOne, SharedSlotTwo });

    // Общая исходная память всех случаев отказа: один урок области «план».
    // Вынесена в помощник, чтобы снимок «до события» в каждом случае был
    // заведомо одним и тем же.
    private static RouteClassifier TrainedClassifierWithPlanLesson()
    {
        // Один ближайший сосед: ответ определяется одним уроком.
        var classifier = NewClassifier(ExactNearestNeighbour);

        // Единственный принятый урок: два общих признака и метка области «план».
        classifier.Train([(new[] { SharedSlotOne, SharedSlotTwo }, Domain.PLAN)]);

        // Готовый маршрутизатор с принятой памятью возвращается случаю проверки.
        return classifier;
    }

    // Сборка канонического маршрутизатора с пустыми хранилищем и журналом:
    // проверке нужен только разбор партии, ничего сохранять и записывать не требуется.
    private static RouteClassifier NewClassifier(int k) =>
        new(new NoopCompilerStorage(), new NoopCompilerEventLogger(), k);

    // Строки управляемой партии: обе учат области «вопрос по коду», то есть
    // ПРОТИВ принятой памяти. Поэтому попадание в память даже одной из них
    // сразу видно по ответу.
    private static IReadOnlyList<(int[] Predicates, Domain Domain)> OppositeTeachingRows() =>
    [
        (new[] { SharedSlotOne, SharedSlotTwo }, Domain.CODE_Q),
        (new[] { ExtraSlotThree }, Domain.CODE_Q)
    ];

    // Общая проверка ответа маршрутизатора с человеческим сообщением об ошибке.
    private static void AssertAnswer(Domain expected, OperationResult<Domain> actual, string what)
    {
        // Сначала требуется сам успех: отказ «не обучен» — это потеря памяти, а не другой ответ.
        Assert.True(
            actual.IsSuccess,
            $"{what}: ожидалась область «{expected}», а маршрутизатор отказал с кодом «{actual.ErrorCode}» и сообщением «{actual.ErrorMessage}».");

        // Затем требуется именно та область, которую выучила принятая память.
        Assert.True(
            actual.Value == expected,
            $"{what}: ожидалась область «{expected}», а получена «{actual.Value}».");
    }

    // Та же проверка для совместимостной оболочки: у проекта 12 собственный тип
    // конверта результата, поэтому нужна отдельная перегрузка с тем же смыслом.
    private static void AssertAnswer(
        Domain expected, SemanticCompiler.OperationResult<Domain> actual, string what)
    {
        // Сначала требуется сам успех: отказ «не обучен» — это потеря памяти, а не другой ответ.
        Assert.True(
            actual.IsSuccess,
            $"{what}: ожидалась область «{expected}», а оболочка отказала с кодом «{actual.ErrorCode}» и сообщением «{actual.ErrorMessage}».");

        // Затем требуется именно та область, которую выучила принятая память.
        Assert.True(
            actual.Value == expected,
            $"{what}: ожидалась область «{expected}», а получена «{actual.Value}».");
    }

    // Общая проверка ответа потребителя с человеческим сообщением об ошибке.
    private static void AssertConsumerAnswer(
        Domain expected, SemanticCompilerAdapter.DomainClassification actual, string what)
    {
        // У потребителя отказа нет: он всегда называет область, поэтому сравнивается сама область.
        Assert.True(
            actual.Label == expected,
            $"{what}: ожидалась область «{expected}», а получена «{actual.Label}» с доводами «{string.Join(", ", actual.Evidence)}».");
    }

    // Управляемый источник партии. Он честно сообщает число строк, но при
    // назначенном обрыве бросает ошибку ДО конца перебора — так воспроизводится
    // источник, который сломался на полпути (оборванный поток, испорченный файл,
    // отвалившаяся выборка). Назначение обрыва минус один означает «не обрывать»,
    // и тогда тот же класс служит положительным контролем.
    private sealed class BatchThatFailsWhileBeingRead : IReadOnlyList<(int[] Predicates, Domain Domain)>
    {
        // Строки, которые источник готов отдать.
        private readonly IReadOnlyList<(int[] Predicates, Domain Domain)> _rows;

        // После скольких отданных строк оборвать чтение; минус один — не обрывать.
        private readonly int _failAfterRows;

        // Источник собирается из готовых строк и назначенного места обрыва.
        public BatchThatFailsWhileBeingRead(
            IReadOnlyList<(int[] Predicates, Domain Domain)> rows, int failAfterRows)
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
        public (int[] Predicates, Domain Domain) this[int index] => _rows[index];

        // Перебор отдаёт строки по одной и обрывается на назначенном месте.
        public IEnumerator<(int[] Predicates, Domain Domain)> GetEnumerator()
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
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Пустое хранилище: маршрутизатору оно нужно только по общему устройству
    // служб проекта, ни одна проверка здесь ничего не сохраняет.
    private sealed class NoopCompilerStorage : ICompilerStorage
    {
        // Сохранение ничего не делает: проверке нечего хранить между вызовами.
        public void Save(string key, PipelineOutput output)
        {
        }

        // Чтение всегда отвечает «ничего нет»: сохранений не было.
        public PipelineOutput? Load(string key) => null;
    }

    // Пустой журнал событий: проверке не нужны записи аудита.
    private sealed class NoopCompilerEventLogger : ICompilerEventLogger
    {
        // Запись события ничего не делает.
        public void Log(StandardEvent evt)
        {
        }
    }
}
