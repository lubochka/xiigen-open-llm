// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;
using System.Text;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace Inference;

public sealed record TextGenerationReceipt(string Text, int PromptTokenCount,
    int GeneratedTokenCount, string StopReason, double ElapsedMilliseconds,
    // HeldBackByteCount — сколько байтов конца ответа НЕ попало в текст, потому что они
    // образовывали незаконченный символ. Ноль означает, что граница ответа прошла ровно по
    // символу. Поле стоит последним и имеет значение по умолчанию, поэтому все прежние
    // вызывающие и все прежние сборки записи работают без правок.
    //
    // Почему это поле нужно. Без него укорачивание ответа на границе было бы тихим: читатель
    // видел бы текст короче и не знал, что часть байтов отброшена. Молча терять байты нельзя
    // так же, как нельзя выдавать разорванный символ.
    int HeldBackByteCount = 0);

// Диагностика хранит копии чисел, а не Tensor или изменяемое состояние генератора.
public sealed class FirstTokenDecision
{
    internal FirstTokenDecision(float[] logits, int[] promptTokenIds, int selectedTokenId)
    {
        Logits = Array.AsReadOnly((float[])logits.Clone());
        PromptTokenIds = Array.AsReadOnly((int[])promptTokenIds.Clone());
        SelectedTokenId = selectedTokenId;
    }
    public IReadOnlyList<float> Logits { get; }
    public IReadOnlyList<int> PromptTokenIds { get; }
    public int SelectedTokenId { get; }
}

public sealed record FirstTokenGenerationResult(TextGenerationReceipt Receipt, FirstTokenDecision Decision,
    int PrefillForwardCalls, int DecodeForwardCalls, double PrefillElapsedMilliseconds);

// Авторегрессивная генерация текста.
// Prefill: все токены подсказки за один проход.
// Decode: по одному токену, останавливаемся на EOS или maxNewTokens.
public class TextGenerator
{
    private readonly LlmModel _model;
    private readonly BpeTokenizer _tokenizer;
    private readonly Func<int, KvCache> _cacheFactory;
    private readonly Func<Tensor, Random, float, int, float, int> _sampler;

    public TextGenerator(LlmModel model, BpeTokenizer tokenizer)
        : this(model, tokenizer, layers => new KvCache(layers))
    {
    }

    internal TextGenerator(
        LlmModel model,
        BpeTokenizer tokenizer,
        Func<int, KvCache> cacheFactory)
        : this(model, tokenizer, cacheFactory, Sampler.Sample)
    {
    }

    internal TextGenerator(
        LlmModel model,
        BpeTokenizer tokenizer,
        Func<int, KvCache> cacheFactory,
        Func<Tensor, Random, float, int, float, int> sampler)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(tokenizer);
        ArgumentNullException.ThrowIfNull(cacheFactory);
        ArgumentNullException.ThrowIfNull(sampler);

        _model = model;
        _tokenizer = tokenizer;
        _cacheFactory = cacheFactory;
        _sampler = sampler;
    }

    public string Generate(
        string prompt,
        int maxNewTokens = 200,
        float temperature = 0.8f,
        int topK = 50,
        float topP = 0.95f)
    {
        return GenerateCore(
            prompt,
            maxNewTokens,
            temperature,
            topK,
            topP,
            seed: null,
            CancellationToken.None).Text;
    }

    public string Generate(
        string prompt,
        int maxNewTokens,
        float temperature,
        int topK,
        float topP,
        CancellationToken cancellationToken)
    {
        return GenerateCore(
            prompt,
            maxNewTokens,
            temperature,
            topK,
            topP,
            seed: null,
            cancellationToken: cancellationToken).Text;
    }

    public string Generate(
        string prompt,
        int maxNewTokens,
        float temperature,
        int topK,
        float topP,
        int seed,
        CancellationToken cancellationToken)
    {
        return GenerateCore(
            prompt,
            maxNewTokens,
            temperature,
            topK,
            topP,
            seed,
            cancellationToken).Text;
    }

    public TextGenerationReceipt GenerateDetailed(string prompt, int maxNewTokens,
        float temperature, int topK, float topP, int seed, int? verifiedEosToken,
        CancellationToken cancellationToken)
        => GenerateCore(prompt, maxNewTokens, temperature, topK, topP, seed,
            cancellationToken, detailed: true, verifiedEosToken);

    // ДОПУСК ЦЕЛОГО СИМВОЛА — ВХОД ПО ЗАПРОСУ.
    //
    // Что делает этот вход. Он включает отбор допустимых продолжений ДО выбора токена: негодные
    // кандидаты закрываются в принадлежащей генератору копии логитов, поэтому разорванный символ
    // не может быть выбран вовсе. Обрезка незаконченного хвоста после порождения остаётся на
    // месте как вторая, независимая защита.
    //
    // Почему вход ПО ЗАПРОСУ, а не поведение по умолчанию. Включение по умолчанию изменило бы
    // уже принятый и зелёный договор границы ответа: соседняя приёмка требует, чтобы предел
    // длины, пришедшийся внутрь символа, дал честно более короткий текст и названное число
    // отброшенных байтов. При допуске начальный байт на последнем шаге выбрать нельзя, то есть
    // такой случай перестал бы возникать, и тот договор стал бы недостижимым. Широкое включение
    // по умолчанию — отдельное решение владельца проекта; здесь даётся явный вход, а путь БЕЗ
    // допуска остаётся побайтово прежним.
    public string GenerateWithWholeCharacterAdmission(string prompt, int maxNewTokens,
        float temperature, int topK, float topP, int seed, CancellationToken cancellationToken)
        => GenerateCore(prompt, maxNewTokens, temperature, topK, topP, seed,
            cancellationToken, wholeCharacterAdmission: true).Text;

    // Подробный вход с допуском: та же запись результата, что и у обычного подробного входа,
    // поэтому знак конца ответа задаёт вызывающий, как и прежде.
    public TextGenerationReceipt GenerateDetailedWithWholeCharacterAdmission(string prompt,
        int maxNewTokens, float temperature, int topK, float topP, int seed, int? verifiedEosToken,
        CancellationToken cancellationToken)
        => GenerateCore(prompt, maxNewTokens, temperature, topK, topP, seed,
            cancellationToken, detailed: true, verifiedEosToken, wholeCharacterAdmission: true);

    // Внутренний вход, который дополнительно отдаёт посчитанную цену допуска. Нужен приёмке,
    // чтобы назвать расход числом вместо «кажется, недорого»; на выбор он не влияет.
    internal TextGenerationReceipt GenerateDetailedWithWholeCharacterAdmission(string prompt,
        int maxNewTokens, float temperature, int topK, float topP, int seed, int? verifiedEosToken,
        CancellationToken cancellationToken, Utf8AdmissionCost admissionCost)
        => GenerateCore(prompt, maxNewTokens, temperature, topK, topP, seed,
            cancellationToken, detailed: true, verifiedEosToken,
            wholeCharacterAdmission: true, admissionCost: admissionCost);

    // Opt-in использует тот же forward и sampler; обычный receipt остаётся прежним.
    public FirstTokenGenerationResult GenerateWithFirstDecision(string prompt, int maxNewTokens,
        float temperature, int topK, float topP, int seed, int? verifiedEosToken,
        CancellationToken cancellationToken)
    {
        if (maxNewTokens < 1) throw new ArgumentException("TEXT_GENERATOR_FIRST_DECISION_REQUIRES_TOKEN");
        var capture = new FirstDecisionCapture();
        var receipt = GenerateCore(prompt, maxNewTokens, temperature, topK, topP, seed,
            cancellationToken, detailed: true, verifiedEosToken, capture);
        return new(receipt, capture.Decision ?? throw new InvalidOperationException("TEXT_GENERATOR_FIRST_DECISION_MISSING"),
            capture.PrefillCalls, capture.DecodeCalls, capture.PrefillMilliseconds);
    }

    private sealed class FirstDecisionCapture
    {
        internal FirstTokenDecision? Decision;
        internal int PrefillCalls, DecodeCalls;
        internal double PrefillMilliseconds;
    }

    private TextGenerationReceipt GenerateCore(
        string prompt,
        int maxNewTokens,
        float temperature,
        int topK,
        float topP,
        int? seed,
        CancellationToken cancellationToken,
        bool detailed = false,
        int? verifiedEosToken = null,
        FirstDecisionCapture? firstDecision = null,
        // Признак допуска целого символа стоит ПОСЛЕ всех прежних параметров и имеет значение
        // по умолчанию «ложь», поэтому каждый существующий вызов остаётся прежним, а путь без
        // допуска — побайтово прежним.
        bool wholeCharacterAdmission = false,
        // Счёт цены допуска: диагностика, которая ни на один выбор не влияет.
        Utf8AdmissionCost? admissionCost = null)
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(prompt);
        if (maxNewTokens < 0)
            throw new ArgumentException("TEXT_GENERATOR_MAX_NEW_TOKENS_NEGATIVE");

        var tokenizer = _tokenizer;
        if (detailed)
        {
            using var tokenizerArtifact = new MemoryStream();
            _tokenizer.Save(tokenizerArtifact);
            tokenizer = BpeTokenizer.ValidateAndLoadCanonicalArtifact(
                tokenizerArtifact, _model.Config.VocabSize, cancellationToken);
        }

        var inputIds = tokenizer.Encode(prompt);
        cancellationToken.ThrowIfCancellationRequested();
        if (inputIds.Count == 0)
            throw new ArgumentException("TEXT_GENERATOR_PROMPT_EMPTY");
        if (inputIds.Count > _model.Config.ContextLen)
            throw new ArgumentException("TEXT_GENERATOR_PROMPT_CONTEXT_LENGTH_EXCEEDED");
        if (detailed && (inputIds.Any(id => id < 0 || id >= _model.Config.VocabSize || id >= tokenizer.VocabSize) ||
            verifiedEosToken is { } eos && (eos < 0 || eos >= _model.Config.VocabSize || eos >= tokenizer.VocabSize)))
            throw new ArgumentException("TEXT_GENERATOR_TOKEN_ID_INVALID");
        if (maxNewTokens == 0)
            return new(string.Empty, inputIds.Count, 0, "LENGTH_LIMIT", elapsed.Elapsed.TotalMilliseconds);

        long requiredContextLength = (long)inputIds.Count + maxNewTokens - 1;
        if (requiredContextLength > _model.Config.ContextLen)
            throw new ArgumentException("TEXT_GENERATOR_CONTEXT_BUDGET_EXCEEDED");

        Random random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
        using var cache = _cacheFactory(_model.Config.NumLayers)
            ?? throw new InvalidOperationException("TEXT_GENERATOR_CACHE_FACTORY_RETURNED_NULL");
        cancellationToken.ThrowIfCancellationRequested();
        long prefillStarted = firstDecision is null ? 0 : System.Diagnostics.Stopwatch.GetTimestamp();
        var logits = _model.ForwardWithCache(
            IdsToTensor(inputIds),
            cache,
            cancellationToken);
        if (firstDecision is not null)
        {
            firstDecision.PrefillCalls++;
            firstDecision.PrefillMilliseconds = System.Diagnostics.Stopwatch.GetElapsedTime(prefillStarted).TotalMilliseconds;
        }
        var generated = new List<int>(maxNewTokens);
        // FIX-EOS-MARKER-01: признак конца ответа берётся ТОЛЬКО у вызывающего.
        //
        // Прежде обычная ветвь брала его из словаря, где он не выделен служебным
        // токеном, а вычислялся как ПОСЛЕДНЯЯ ЗАПИСЬ обученного словаря. На
        // измеренном корпусе последней записью оказался обычный кусок слова
        // ` close` с номером 335, поэтому каждый раз, когда модель выдавала этот
        // кусок, обычная ветвь обрывала ответ. Какой кусок станет признаком,
        // зависит от корпуса и меняется при каждом переобучении, и ни одна
        // проверка, закреплённая на старом словаре, этого не поймала бы.
        //
        // Точная граница прежнего вреда, названная прямо: обычные перегрузки
        // возвращают только текст и причину остановки ВЫБРАСЫВАЮТ, поэтому
        // вызывающий получал более короткую строку и никакой причины вовсе. Это
        // был молчаливый обрыв, а не ложное объяснение: причину «конец ответа»
        // отдают только подробные перегрузки, где признак задан вызывающим и
        // проверен, то есть там она правдива.
        //
        // Цена правки названа заранее: пока вызывающий не задаст признак,
        // ответы доходят до предела длины, то есть расходуют больше
        // процессорного времени. Возможности раннего завершения это не отнимает —
        // прежнее завершение срабатывало на произвольном куске слова и
        // возможностью только выглядело. Обе ветви теперь ведут себя одинаково:
        // признак от вызывающего либо никакой.
        int? eosToken = verifiedEosToken;

        // ДОПУСК ЦЕЛОГО СИМВОЛА: снимок словаря и состояние байтов ОТВЕТА.
        //
        // Снимок берётся ОДИН раз на всё порождение, а не на каждом шаге: иначе обход словаря
        // повторялся бы столько раз, сколько выдано токенов, и цена допуска росла бы без нужды.
        // Снимок берётся у ТОГО ЖЕ словаря, которым разбирается ответ ниже, поэтому байты
        // допуска и байты ответа не могут разойтись.
        //
        // Состояние байтов ответа начинается ЧИСТЫМ. Это не упрощение, а требование: законность
        // считается по байтам ОТВЕТА, а подсказка имеет свой путь разбора и свою проверку
        // номеров выше. Иначе неполный конец подсказки молча разрешил бы начать ответ с байта
        // продолжения.
        Utf8TokenByteSnapshot? admissionSnapshot = null;
        Utf8AnswerByteState? answerBytes = null;
        // Буфер решений по кандидатам: выделяется один раз и переиспользуется каждым шагом.
        bool[]? admittedCandidates = null;
        if (wholeCharacterAdmission)
        {
            // Один обход словаря на всё порождение.
            admissionSnapshot = Utf8TokenByteSnapshot.Create(tokenizer, cancellationToken);
            // Чистое состояние ответа: ни одного байта ещё не выдано.
            answerBytes = new Utf8AnswerByteState();
            if (admissionCost is not null)
            {
                // Цена снимка записывается по факту построения.
                admissionCost.SnapshotBuilds++;
                admissionCost.SnapshotIdentifiersScanned = admissionSnapshot.ScannedIdentifierCount;
                admissionCost.SnapshotRetainedBytes = admissionSnapshot.RetainedByteCount;
            }
        }

        while (generated.Count < maxNewTokens)
        {
            var lastLogits = ExtractLastLogits(logits);
            float[]? firstLogits = null;
            if (firstDecision is not null && generated.Count == 0)
            {
                firstLogits = (float[])lastLogits.Data.Clone();
                if (firstLogits.Any(value => !float.IsFinite(value)))
                    throw new InvalidDataException("TEXT_GENERATOR_FIRST_LOGITS_NONFINITE");
            }
            cancellationToken.ThrowIfCancellationRequested();

            // Сколько шагов ответа останется ПОСЛЕ этого. Ноль означает последний шаг: дособрать
            // начатый символ будет уже нечем, поэтому незаконченный вид на нём недопустим.
            var stepsRemainingAfterThis = maxNewTokens - generated.Count - 1;
            // Вектор, который получит выборщик. Без допуска это РОВНО тот же объект, что и прежде.
            var samplerLogits = lastLogits;
            if (wholeCharacterAdmission)
            {
                // Отбор допустимых продолжений происходит ЗДЕСЬ — до вызова выборщика. Порядок
                // выбран однозначно: сначала допуск символа, затем прежние температура, отбор по
                // числу кандидатов и отбор по сумме вероятностей внутри выборщика.
                samplerLogits = AdmitWholeCharacterCandidates(lastLogits, admissionSnapshot!,
                    answerBytes!, eosToken, stepsRemainingAfterThis, ref admittedCandidates, admissionCost);
            }

            int nextToken = _sampler(samplerLogits, random, temperature, topK, topP);
            // Выборщик вызван ровно один раз за шаг, как и прежде: допуск не добавляет вызовов,
            // а значит не добавляет и розыгрышей случайной величины.
            if (admissionCost is not null) admissionCost.SamplerInvocations++;
            // Диагностика первого шага записывает ИСХОДНЫЕ числа до маскирования и отдельно от
            // них — допущенного победителя. Замаскированные логиты вместо измеренного
            // распределения не подставляются.
            if (firstLogits is not null)
                firstDecision!.Decision = new(firstLogits, inputIds.ToArray(), nextToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (detailed && (nextToken < 0 || nextToken >= _model.Config.VocabSize || nextToken >= tokenizer.VocabSize))
                throw new ArgumentException("TEXT_GENERATOR_TOKEN_ID_INVALID");

            if (wholeCharacterAdmission)
            {
                // ПОВТОРНАЯ ПРОВЕРКА ВЫБРАННОГО НОМЕРА ПЕРЕД ДОБАВЛЕНИЕМ.
                //
                // Зачем она нужна, если негодные кандидаты уже закрыты. Потому что выборщик —
                // подставляемая зависимость: ошибочный или испытательный выборщик может вернуть
                // номер, которого в маске не было. Без этой проверки такой выборщик обошёл бы
                // условие, и разорванный символ снова попал бы в ответ. Проверка стоит ДО
                // добавления, поэтому состояние ответа негодным номером не портится.
                var chosenBytes = RequireAdmittedChoice(admissionSnapshot!, answerBytes!, nextToken,
                    eosToken, stepsRemainingAfterThis);
                // Выбор признан годным: только теперь его байты входят в состояние ответа.
                answerBytes!.Append(chosenBytes);
            }

            generated.Add(nextToken);

            if (nextToken == eosToken || generated.Count == maxNewTokens)
                break;

            cancellationToken.ThrowIfCancellationRequested();
            logits = _model.ForwardWithCache(
                IdsToTensor([nextToken]),
                cache,
                cancellationToken);
            if (firstDecision is not null) firstDecision.DecodeCalls++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ГРАНИЦА ОТВЕТА НЕ ДЕЛИТ СИМВОЛ.
        //
        // Что было неправильно. Выдача останавливалась по ЧИСЛУ ТОКЕНОВ — условие выше: токен
        // признака конца либо достижение предела длины. Словарь этого токенизатора байтовый, то
        // есть один токен равен одному байту, а одна русская буква занимает два токена, а
        // составной символ — четыре. Поэтому предел длины мог прийтись на середину символа.
        // Дальше байты превращались в строку одним вызовом, и незаконченный символ становился
        // символом замены: ответ рвался посреди символа ТИХО, без отказа и без следа.
        //
        // Что стало. Байты ответа берутся до превращения в строку, и в текст идёт только тот
        // начальный кусок, который целиком состоит из законченных символов. Незаконченный хвост
        // не попадает в текст, и его длина названа в записи результата отдельным числом.
        //
        // Почему границей, а не перехватом и не заменой на вопросительный знак. Перехват
        // маскирует: он не говорит, что символ был разорван. Замена на символ замены — это и
        // есть нынешняя беда: читатель получает мусор вместо честно более короткого ответа.
        // Граница же не делит символ вообще, и это проверяемое свойство байтов.
        var generatedBytes = tokenizer.DecodeToBytes(generated);
        // Длина начального куска, состоящего только из законченных символов.
        var completeLength = CompleteUtf8PrefixLength(generatedBytes);
        // Строка собирается ровно из этого куска, поэтому разорванного символа в ней быть не может.
        var generatedText = Encoding.UTF8.GetString(generatedBytes, 0, completeLength);
        // Отброшенные байты пересчитываются, а не предполагаются.
        var heldBackByteCount = generatedBytes.Length - completeLength;

        cancellationToken.ThrowIfCancellationRequested();
        return new(generatedText, inputIds.Count, generated.Count,
            eosToken.HasValue && generated[^1] == eosToken.Value ? "EOS" : "LENGTH_LIMIT",
            elapsed.Elapsed.TotalMilliseconds,
            heldBackByteCount);
    }

    // AdmitWholeCharacterCandidates строит ПРИНАДЛЕЖАЩУЮ генератору копию логитов, в которой
    // непригодные кандидаты закрыты минус бесконечностью, и отдаёт её выборщику.
    //
    // Почему копия, а не правка на месте. Исходный вектор логитов — это измеренное
    // распределение модели. Его нельзя портить: по нему работает диагностика первого шага и
    // проверка конечности чисел. Поэтому маска живёт в отдельном массиве.
    private static Tensor AdmitWholeCharacterCandidates(
        Tensor lastLogits,
        Utf8TokenByteSnapshot snapshot,
        Utf8AnswerByteState answerBytes,
        int? eosToken,
        int stepsRemainingAfterThis,
        ref bool[]? admittedCandidates,
        Utf8AdmissionCost? admissionCost)
    {
        // Кандидатов ровно столько, сколько чисел в векторе логитов ЭТОЙ модели. Размер словаря
        // может с ним не совпадать, и это учтено ниже: номер, которого словарь не знает, в
        // допуск не проходит.
        var candidateCount = lastLogits.Shape.TotalElements;
        // Буфер решений выделяется один раз на порождение и дальше переиспользуется, поэтому
        // шаг не создаёт нового массива решений.
        if (admittedCandidates is null || admittedCandidates.Length != candidateCount)
            admittedCandidates = new bool[candidateCount];

        // Сколько кандидатов прошло допуск на этом шаге.
        var admitted = 0;
        for (int identifier = 0; identifier < candidateCount; identifier++)
        {
            // Решение по одному кандидату. Обход словаря не обращается к генератору случайных
            // чисел ни разу, поэтому цена случайности от допуска не меняется.
            admittedCandidates[identifier] =
                IsAdmittedCandidate(snapshot, answerBytes, identifier, eosToken, stepsRemainingAfterThis);
            if (admittedCandidates[identifier]) admitted++;
        }

        if (admissionCost is not null)
        {
            // Цена шага записывается по факту: сколько масок построено, сколько кандидатов
            // опрошено и сколько закрыто.
            admissionCost.MaskBuilds++;
            admissionCost.CandidateClassifications += candidateCount;
            admissionCost.MaskedCandidateTotal += candidateCount - admitted;
            // Наименьшее число допущенных по всем шагам: на нём видно, что выбор всегда был.
            if (admitted < admissionCost.MinimumAdmittedCandidates)
                admissionCost.MinimumAdmittedCandidates = admitted;
        }

        // Пригодных кандидатов не осталось: отказ выдаётся ЯВНО и ДО розыгрыша, то есть до
        // выборщика и до следующего прохода модели. Отдавать выборщику вектор из одних минус
        // бесконечностей запрещено: жадная ветвь вернула бы по нему произвольный номер, то есть
        // выдумала бы ответ, а случайная ветвь упала бы на пустом распределении уже внутри
        // выборщика, где причина была бы не видна.
        if (admitted == 0)
            throw new InvalidOperationException("TEXT_GENERATOR_UTF8_ADMISSION_NO_CANDIDATE");

        // Маска не исключила НИЧЕГО: выборщик получает тот же самый вектор, что и прежде. Здесь
        // возвращается именно исходный объект, а не копия с теми же числами, поэтому прежнее
        // поведение сохраняется побайтово и без всякой арифметики над числами.
        if (admitted == candidateCount)
        {
            if (admissionCost is not null) admissionCost.StepsWithNothingExcluded++;
            return lastLogits;
        }

        // Хотя бы один кандидат исключён: строится копия логитов, принадлежащая генератору.
        var admittedLogits = new float[candidateCount];
        for (int identifier = 0; identifier < candidateCount; identifier++)
            // Годный кандидат сохраняет своё измеренное число; негодный закрывается минус
            // бесконечностью, которую и жадная, и случайная ветви выборщика уже умеют не выбирать.
            admittedLogits[identifier] = admittedCandidates[identifier]
                ? lastLogits.Data[identifier]
                : float.NegativeInfinity;

        // Форма вектора не меняется: выборщик получает ровно то же устройство входа, что и прежде.
        return new Tensor(admittedLogits, lastLogits.Shape);
    }

    // IsAdmittedCandidate — ОДНО правило допуска, общее для всех кандидатов, включая знак конца
    // ответа. Отдельной ветви «этот номер особенный» здесь нет НАМЕРЕННО: знак конца ответа
    // остаётся допустимым ровно там, где он законен, и недопустимым становится не из-за того,
    // что он знак конца, а из-за того, что на нём ответ остался бы недособранным.
    private static bool IsAdmittedCandidate(
        Utf8TokenByteSnapshot snapshot,
        Utf8AnswerByteState answerBytes,
        int identifier,
        int? eosToken,
        int stepsRemainingAfterThis)
    {
        // Неизвестный номер непригоден по признаку ПРОМЕЖУТКА известных номеров, а не по числу
        // байтов. Это различие существенно: сборка байтов отдаёт неизвестному номеру ноль байтов
        // молча, и по числу байтов он выглядел бы как «ничего не добавляет», то есть прошёл бы
        // допуск и потерял бы ответ без следа.
        if (!snapshot.TryGetBytes(identifier, out var bytes)) return false;

        // Чем кончится добавление ВСЕХ байтов этого кандидата и сколько байтов символа осталось
        // бы недособранными.
        var status = answerBytes.Classify(bytes, out var pendingAfterCandidate);
        // Незаконная цепочка байтов непригодна на любом шаге.
        if (status == Utf8CandidateStatus.Invalid) return false;

        // ПРАВИЛО ОСТАТКА БЮДЖЕТА. Незаконченный префикс допустим только пока остаётся
        // возможность дособрать символ. Каждый следующий шаг добавляет не меньше одного байта,
        // поэтому недособранных байтов не должно быть больше, чем осталось шагов.
        //
        // Это правило ОБОБЩАЕТ правило последнего шага: на последнем шаге остаток шагов равен
        // нулю, значит требуется ровно ноль недособранных байтов, то есть законченный вид.
        // Отдельная ветвь «последний шаг» поэтому не нужна и была бы дублем.
        //
        // Почему одного правила последнего шага НЕДОСТАТОЧНО, названо числом: трёхбайтовый знак
        // требует двух байтов продолжения, и если его начало допустить, когда до конца ответа
        // остался один шаг, то на последнем шаге дособрать его будет НЕЧЕМ — ни один кандидат
        // не пройдёт, и вместо законного ответа выйдет отказ. Это измерено: без этого правила
        // порождение длиной двенадцать токенов упало с кодом отсутствия кандидатов.
        //
        // Правило намеренно СТРОГОЕ в одну сторону: оно считает, что шаг даёт один байт, хотя
        // склеенный токен обученного словаря может дать несколько. Поэтому на словаре со
        // склейками оно может отвергнуть префикс, который склейка дособрала бы за один шаг.
        // Такая строгость безопасна — она никогда не допускает недособираемый префикс, — и
        // названа здесь прямо, а не скрыта.
        if (pendingAfterCandidate > stepsRemainingAfterThis) return false;

        // Законченный вид обязателен ещё и на знаке конца ответа, заданном ВЫЗЫВАЮЩИМ: сразу
        // после него выдача прекращается, сколько бы шагов ни оставалось в запасе. Собственного
        // знака конца у словаря нет вовсе, поэтому правило опирается на переданное значение, а
        // не на свойство словаря.
        if (eosToken.HasValue && identifier == eosToken.Value)
            return status == Utf8CandidateStatus.ValidComplete;

        // Остальные кандидаты прошли: цепочка законна и дособираема в оставшийся бюджет.
        return true;
    }

    // RequireAdmittedChoice проверяет УЖЕ ВЫБРАННЫЙ номер тем же правилом и отдаёт его байты.
    // Коды отказа РАЗНЫЕ, потому что причины разные: по ним видно, негоден номер сам по себе
    // или негодна цепочка байтов, которую он образует.
    private static byte[] RequireAdmittedChoice(
        Utf8TokenByteSnapshot snapshot,
        Utf8AnswerByteState answerBytes,
        int chosenIdentifier,
        int? eosToken,
        int stepsRemainingAfterThis)
    {
        // Неизвестный номер: отдельный код, потому что молчаливая пустота хуже отказа —
        // вызывающий иначе не узнал бы, что ответ потерян.
        if (!snapshot.TryGetBytes(chosenIdentifier, out var bytes))
            throw new InvalidOperationException("TEXT_GENERATOR_UTF8_ADMISSION_IDENTIFIER_UNKNOWN");

        // РОВНО ТО ЖЕ правило, что у маски: иначе повторная проверка пропускала бы то, что
        // маска закрыла, или закрывала то, что маска пропустила.
        if (!IsAdmittedCandidate(snapshot, answerBytes, chosenIdentifier, eosToken,
                stepsRemainingAfterThis))
            throw new InvalidOperationException("TEXT_GENERATOR_UTF8_ADMISSION_SEQUENCE_INVALID");

        // Номер годен: его байты отдаются, чтобы состояние ответа обновилось ровно ими.
        return bytes;
    }

    // CompleteUtf8PrefixLength возвращает длину начального куска байтов, который целиком состоит
    // из ЗАКОНЧЕННЫХ символов. Хвост незаконченного символа в эту длину не входит.
    //
    // Как это работает, по-человечески. В кодировке UTF-8 символ занимает от одного до четырёх
    // байтов. Первый байт символа говорит, сколько байтов всего: один байт — старший бит ноль;
    // два байта — старшие биты 110; три байта — 1110; четыре байта — 11110. Все остальные байты
    // символа начинаются с 10 и сами по себе ничего не значат. Поэтому достаточно отойти от конца
    // назад до первого байта последнего символа и сравнить, сколько байтов он обещает и сколько
    // их на самом деле осталось. Если осталось меньше обещанного — этот символ не закончен, и
    // граница ставится перед ним.
    //
    // Чего этот способ НЕ решает, названо прямо. Он работает с границей ОДНОГО символа кодировки.
    // Составной знак из нескольких символов — буква со знаком ударения отдельным символом, флаг
    // из двух символов-указателей страны, цепочка с нулевым соединителем, вариантная пометка —
    // может всё ещё разделиться на границе, потому что каждый его символ сам по себе законен и
    // закончен. Это отдельный класс случаев и отдельная работа: он требует правил разбиения на
    // видимые знаки, а не проверки байтов. Суррогатные пары здесь не возникают вовсе: это способ
    // записи UTF-16, а на границе ответа мы работаем с байтами UTF-8.
    private static int CompleteUtf8PrefixLength(byte[] bytes)
    {
        // Пустой ответ уже цел: делить нечего.
        if (bytes.Length == 0)
            return 0;

        // Отходим от конца назад по байтам-продолжениям: их не может быть больше трёх подряд.
        var index = bytes.Length - 1;
        var continuations = 0;
        while (index >= 0 && (bytes[index] & 0xC0) == 0x80 && continuations < 3)
        {
            // Байт-продолжение начинается с битов 10 и первым байтом символа быть не может.
            continuations++;
            index--;
        }

        // Первого байта символа не нашлось вовсе: весь хвост — обрывок чужого символа.
        if (index < 0)
            return 0;

        // Первый байт последнего символа и число байтов, которое он обещает.
        var lead = bytes[index];
        var needed = lead < 0x80 ? 1
            : (lead & 0xE0) == 0xC0 ? 2
            : (lead & 0xF0) == 0xE0 ? 3
            : (lead & 0xF8) == 0xF0 ? 4
            : 0;

        // Байт, который не является ни первым байтом символа, ни продолжением, — испорченная
        // запись, а не граница. Такой случай эта починка не трогает: байты остаются как есть, и
        // превращение в строку поступит с ними по своим правилам.
        if (needed == 0)
            return bytes.Length;

        // Сколько байтов последнего символа фактически дошло до конца ответа.
        var have = bytes.Length - index;

        // Обещанного числа байтов не хватает: символ не закончен, граница ставится перед ним.
        // Иначе весь ответ состоит из законченных символов и укорачивать его не нужно.
        return have < needed ? index : bytes.Length;
    }

    // Преобразовать список токенов в Tensor [1, seqLen]
    private static Tensor IdsToTensor(IEnumerable<int> ids)
    {
        var arr = ids.Select(i => (float)i).ToArray();
        return new Tensor(arr, new TensorShape(1, arr.Length));
    }

    // Вырезать логиты последней позиции: [1, T, vocab] → [vocab]
    private static Tensor ExtractLastLogits(Tensor logits)
    {
        ArgumentNullException.ThrowIfNull(logits);
        if (logits.Shape.Rank != 3)
            throw new ArgumentException("TEXT_GENERATOR_LOGITS_RANK_INVALID");

        var dimensions = logits.Shape.Dimensions;
        if (dimensions[0] != 1)
            throw new ArgumentException("TEXT_GENERATOR_LOGITS_BATCH_INVALID");

        int queryLength = dimensions[1];
        if (queryLength < 1)
            throw new ArgumentException("TEXT_GENERATOR_LOGITS_QUERY_LENGTH_INVALID");

        int vocabSize = dimensions[2];
        if (vocabSize < 1)
            throw new ArgumentException("TEXT_GENERATOR_LOGITS_VOCAB_SIZE_INVALID");

        int offset = (queryLength - 1) * vocabSize;
        var data = new float[vocabSize];
        Array.Copy(logits.Data, offset, data, 0, vocabSize);
        return new Tensor(data, new TensorShape(vocabSize));
    }
}
