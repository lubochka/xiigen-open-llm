// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Приёмка переноса кандидата девяносто восьмого опыта: обучение словаря кусочков
// слов сообщает, был ли достигнут запрошенный размер.
//
// Что было неверно. Цикл обучения выходил по двум разным причинам — по
// достижению запрошенного размера и по исчерпанию соседних пар, — и наружу
// обе выглядели одинаково: метод возвращал `void`. Вызывающий, рассчитавший
// размер матрицы представлений по запрошенным 2048, получал 336 и узнавал об
// этом только если сам догадывался прочитать фактический размер словаря.
// Девяносто восьмой опыт измерил это прямо: запросы 629 и 2048 на корпусе из 21
// различного куска дали побайтово одно и то же состояние.
//
// Почему это не теоретический вред. Два боевых вызывающих обходят молчаливую
// недостижимость ПРОТИВОПОЛОЖНЫМИ способами, и знание живёт в комментариях, а
// не в договоре: обучающий способности подмешивает в корпус 512 синтетических
// слов, чтобы запрос гарантированно достигался
// (`18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs`, строка 4074,
// рядом с настройкой, где размер словаря задан литералом 512), а средство
// студенческих проверок сознательно НЕ подмешивает и принимает недобор
// (`tools/Rp01StudentControls/Program.cs`, строка 89, где это прямо сказано в
// комментарии). Загрузчик базовой модели расхождение замечает, но уже после
// того, как построил модель и сохранил контрольную точку, и винит артефакт, а
// не корпус (`tools/ContinuousLearningBaseBootstrap/Program.cs`, строки 1099 и
// 1157).
//
// Почему приёмка поведенческая, а не структурная. Различение доступно прямо по
// поведению: обучи на маленьком корпусе с большим запросом — и итог скажет, что
// запрос не достигнут и почему. На прежнем коде такого итога не существовало
// вовсе, поэтому утверждения ниже на нём не просто падают, а не выражаются.
// Отдельное договорное утверждение ниже исполняется и на прежнем коде, и там
// падает: оно читает объявленный тип возврата.
//
// Чего эта приёмка НЕ доказывает. Она не меняет и не проверяет качество
// обучения: выученный словарь обязан остаться ТЕМ ЖЕ, и это проверяется
// отдельно — существующие проверки закрепляют отпечатки состояния, снятые с
// токенизатора до прежнего переноса, и они должны продолжать совпадать.

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Reflection;
using Tokenizer;

public sealed class BpeTrainingReportsWhetherRequestedVocabSizeWasReachedTests
{
    // Корпус, на котором запрос ДОСТИГАЕТСЯ. Взят дословно из существующей
    // проверки `BpeNoMatchCopyTests`, где к нему закреплены отпечатки
    // состояния, снятые с токенизатора ДО прежнего переноса. Поэтому он служит
    // здесь эталоном с заведомо известным ответом сразу в двух смыслах: и по
    // достижимости запроса, и по побайтовой неизменности выученного состояния.
    private const string ReachableCorpus =
        "the quick brown fox jumps over the lazy dog the quick brown fox " +
        "a tokenizer learns merges from repeated pieces of ordinary text " +
        "the tokenizer learns the pieces the tokenizer repeats the pieces";

    // Исторические отпечатки того же корпуса: размер, число токенов и отпечаток
    // состояния. Это запись прошлого, а не вывод нынешнего кода.
    private static readonly (int Vocab, string StateSha256)[] ReachableReference =
    [
        (260, "a5e6c4f48f0b4f6b2dec33d2b07af4d574a96010f5aef3ff8dbd31a28bd53cd2"),
        (288, "41448a7971f75b41df5e3e79b03d70864bb53a125f45116f520bf11e7b28601b"),
        (320, "3278531fbd374e077f8c84aa4ee006d9dbf6303f7b79e2a79490d76302479296"),
    ];

    // Корпус, на котором запрос НЕ достигается: каждый кусок различен, и пары
    // кончаются раньше запрошенного размера. Взят дословно из существующей
    // проверки `BpeWeightedChunkTests`, где достигнутый размер 286 закреплён
    // прямо с объяснением «обучение останавливается раньше, когда склеивать
    // больше нечего». То есть ответ на этом корпусе известен заранее и не
    // выведен из нынешнего кода.
    private const string UnreachableCorpus =
        "alpha beta gamma delta epsilon zeta eta theta";

    private const int UnreachableRequest = 300;
    private const int UnreachableAchieved = 286;

    [Fact]
    public void UnreachableRequestIsReportedWithItsReasonAndShortfall()
    {
        var tokenizer = new BpeTokenizer();

        // Итог обучения теперь существует. На прежнем коде метод возвращал
        // `void`, поэтому эта строка там не компилировалась вовсе.
        var outcome = tokenizer.Train(UnreachableCorpus, UnreachableRequest);

        // Запрос не достигнут, и это сказано прямо, а не выведено вызывающим.
        Assert.False(outcome.RequestedVocabSizeReached);
        Assert.Equal(
            BpeTrainingExitReason.AdjacentPairsExhausted,
            outcome.ExitReason);

        // Числа совпадают с закреплённым заранее ответом: запрошено 300,
        // получено 286, недобор 14.
        Assert.Equal(UnreachableRequest, outcome.RequestedVocabSize);
        Assert.Equal(UnreachableAchieved, outcome.AchievedVocabSize);
        Assert.Equal(UnreachableRequest - UnreachableAchieved, outcome.VocabSizeShortfall);

        // Итог не расходится с самим словарём: достигнутый размер — это тот же
        // размер, который словарь сообщает о себе, а не отдельно посчитанное
        // число, которое могло бы разойтись.
        Assert.Equal(tokenizer.VocabSize, outcome.AchievedVocabSize);

        // И он согласован с числом выученных слияний: 256 однобайтовых токенов
        // плюс по одному токену за каждое слияние.
        Assert.Equal(256 + outcome.LearnedMergeCount, outcome.AchievedVocabSize);
    }

    [Theory]
    [InlineData(260)]
    [InlineData(288)]
    [InlineData(320)]
    public void ReachedRequestIsReportedAsReachedWithoutShortfall(int vocabSize)
    {
        var tokenizer = new BpeTokenizer();
        var outcome = tokenizer.Train(ReachableCorpus, vocabSize);

        // Законный случай: запрос достигнут, причина выхода — именно достижение
        // размера, недобора нет.
        Assert.True(outcome.RequestedVocabSizeReached);
        Assert.Equal(
            BpeTrainingExitReason.RequestedSizeReached,
            outcome.ExitReason);
        Assert.Equal(vocabSize, outcome.RequestedVocabSize);
        Assert.Equal(vocabSize, outcome.AchievedVocabSize);
        Assert.Equal(0, outcome.VocabSizeShortfall);
        Assert.Equal(tokenizer.VocabSize, outcome.AchievedVocabSize);
    }

    [Fact]
    public void LegitimateTrainingLearnsExactlyTheSameVocabularyAsBefore()
    {
        // Это главная защита переноса: сообщать итог можно только если сам
        // выученный словарь не изменился ни на байт. Отпечатки ниже сняты с
        // токенизатора ДО прежнего переноса и закреплены существующей
        // проверкой; если бы добавление итога сдвинуло хоть одно слияние или их
        // порядок, они бы не совпали.
        foreach (var (vocab, stateSha256) in ReachableReference)
        {
            var tokenizer = new BpeTokenizer();
            var outcome = tokenizer.Train(ReachableCorpus, vocab);

            Assert.Equal(vocab, tokenizer.VocabSize);
            Assert.Equal(stateSha256, tokenizer.ComputeStateSha256());

            // И итог описывает именно это состояние, а не своё представление о нём.
            Assert.Equal(vocab, outcome.AchievedVocabSize);
            Assert.True(outcome.RequestedVocabSizeReached);
        }
    }

    [Fact]
    public void UnreachableTrainingAlsoLearnsExactlyTheSameVocabularyAsBefore()
    {
        // Недостижимый случай тоже обязан остаться прежним: обучение на том же
        // корпусе дважды даёт побайтово одно состояние, и достигнутый размер
        // равен закреплённому заранее числу.
        var first = new BpeTokenizer();
        var firstOutcome = first.Train(UnreachableCorpus, UnreachableRequest);
        var second = new BpeTokenizer();
        var secondOutcome = second.Train(UnreachableCorpus, UnreachableRequest);

        Assert.Equal(UnreachableAchieved, first.VocabSize);
        Assert.Equal(first.ComputeStateSha256(), second.ComputeStateSha256());
        Assert.Equal(firstOutcome, secondOutcome);

        // Текст, на котором словарь учился, по-прежнему восстанавливается точно.
        Assert.Equal(
            UnreachableCorpus,
            first.Decode(first.Encode(UnreachableCorpus)));
    }

    [Fact]
    public void RequestedSizeBelowTheByteFloorIsStillRejectedAsBefore()
    {
        // Проверка аргумента оставлена исключением НАМЕРЕННО: размер меньше 256
        // — ошибка программиста, а не свойство данных, и её замена отказом была
        // бы отдельным фактором. Эта проверка закрепляет, что перенос её не
        // тронул.
        var tokenizer = new BpeTokenizer();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => tokenizer.Train(ReachableCorpus, 255));
    }

    [Fact]
    public void TrainingNoLongerEndsSilently()
    {
        // Договорное утверждение, которое исполняется и на ПРЕЖНЕМ коде — и там
        // падает, потому что метод объявлял возврат `void`. Остальные
        // утверждения этого файла на прежнем коде не компилировались бы вовсе,
        // поэтому именно это служит различающим, которое можно прогнать на обеих
        // версиях.
        var train = typeof(BpeTokenizer).GetMethod(
            nameof(BpeTokenizer.Train),
            BindingFlags.Public | BindingFlags.Instance,
            [typeof(string), typeof(int)]);
        Assert.NotNull(train);
        Assert.NotEqual(typeof(void), train!.ReturnType);
        Assert.Equal(typeof(BpeTrainingOutcome), train.ReturnType);
    }

    [Fact]
    public void ExitReasonCoversBothWaysTheLoopCanEndAndNothingElse()
    {
        // Причин выхода ровно две, и обе достижимы на эталонах выше. Если
        // появится третья, эта проверка упадёт громко, а не примет её молча.
        var reasons = Enum.GetValues<BpeTrainingExitReason>();
        Assert.Equal(2, reasons.Length);
        Assert.Contains(BpeTrainingExitReason.RequestedSizeReached, reasons);
        Assert.Contains(BpeTrainingExitReason.AdjacentPairsExhausted, reasons);

        // И обе действительно наблюдаются, а не только объявлены.
        var reached = new BpeTokenizer().Train(ReachableCorpus, 260);
        var exhausted = new BpeTokenizer().Train(UnreachableCorpus, UnreachableRequest);
        Assert.Equal(BpeTrainingExitReason.RequestedSizeReached, reached.ExitReason);
        Assert.Equal(BpeTrainingExitReason.AdjacentPairsExhausted, exhausted.ExitReason);
    }
}
