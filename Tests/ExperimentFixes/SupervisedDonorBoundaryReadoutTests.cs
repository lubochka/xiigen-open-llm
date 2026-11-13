// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using ContinuousLearning;
using LoRA;
using ModelServer;
using TransformerModel;

/// <summary>
/// Штатное чтение двух уже выученных ответов на двух восстановленных снимках
/// весов. Обучения здесь нет ни одного обновления: снимки только загружаются,
/// а ответ читается тем же читателем, который берёт формат у единственного
/// владельца формата обучения с учителем (SFT — supervised fine-tuning,
/// дообучение по готовым парам «запрос → ответ»). Сравниваются ровно две
/// границы ответа: прежняя диагностическая склейка встык без разделителя и
/// принятая продуктовая склейка через один перевод строки (LF — line feed).
/// Это проверка правильности измерения, а не политика остановки обучения и не
/// повторение самого опыта: числа опыта здесь ожидания, а не новое открытие.
/// </summary>
public sealed class SupervisedDonorBoundaryReadoutTests
{
    // Отпечаток файла настроек генерации: тот же, что удостоверял прибор опыта.
    private const string ConfigSha256 =
        "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40";

    // Сколько именованных групп параметров обязано быть в каждом снимке.
    private const int ExpectedCheckpointParameterEntries = 55;

    // Размер каждого неповреждённого снимка в байтах.
    private const long ExpectedDonorBytes = 47_899_519L;

    // Куда складываются улики этого чтения внутри хранилища проекта.
    private const string EvidenceRelativeDirectory =
        @"artifacts\fbe\ROW124-Q06-DONORS\attempt-0001";

    /// <summary>
    /// Один донор: файл снимка, его ожидаемый отпечаток, учебная пара и
    /// ожидаемые качества обеих границ, выписанные из сохранённого опыта.
    /// </summary>
    private sealed record Donor(
        string Row,
        string File,
        string Sha256,
        string Prompt,
        string Response,
        int ProductCorrectTokens,
        int ProductDenominator,
        int LegacyCorrectTokens,
        int LegacyDenominator);

    // Оба донора строки работ «Прочитать два уже выученных ответа»: пары взяты
    // из той же оснастки, по которой снимки и обучались.
    private static readonly Donor[] Donors =
    {
        new(
            "abs",
            "producer-abs.bin",
            "59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f",
            "// file: Abs.cs",
            "static int Abs(int n)=>n<0?-n:n;",
            23,
            23,
            22,
            23),
        new(
            "sign",
            "producer-sign.bin",
            "af714d961cc8d2cdc22a245afe3202e71564e9c5ac5f50dd09a8f7af4db426e4",
            "// file: Sign.cs",
            "static int Sign(int x)=>x<0?-1:1;",
            21,
            21,
            20,
            21)
    };

    /// <summary>
    /// На одних и тех же восстановленных весах и при одинаковых правильных
    /// префиксах продуктовая граница ответа читает ответ целиком, а прежняя
    /// граница встык теряет ровно первый знак ответа. Веса при чтении не
    /// меняются: состояние модели до и после обоих чтений побайтно совпадает.
    /// </summary>
    [Fact]
    public void RestoredDonors_ProductBoundaryReadsWholeAnswer_LegacyBoundaryLosesFirstToken()
    {
        // Корень хранилища ищется от папки сборки, чтобы путь не зависел от места запуска.
        var repositoryRoot = FindRepositoryRoot();
        // Настройки читаются тем же удостоверяющим читателем, что и в продукте.
        var verified = CodeDocumentationGenerationConfiguration.ReadVerified(
            name => name switch
            {
                // Чтение настроек разрешено явно, как в прежнем прогоне опыта.
                "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true",
                // Отпечаток настроек сверяется с ожидаемым, иначе читается другая модель.
                "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => ConfigSha256,
                // Любое другое имя настройки этому чтению не нужно.
                _ => null
            },
            repositoryRoot);
        // Без удостоверенных настроек читать нечего: это отказ прибора, а не промах модели.
        Assert.True(verified.IsSuccess, $"{verified.ErrorCode}: {verified.ErrorMessage}");
        var options = verified.Value!;

        // Каждая строка улик собирается по ходу и целиком пишется на диск в конце.
        var observations = new List<object>();
        foreach (var donor in Donors)
        {
            // Полный путь к восстановленному снимку внутри хранилища проекта.
            var donorPath = Path.Combine(
                repositoryRoot,
                "experiments-v2",
                "RP01-MICRO",
                "SEP01",
                "results-01",
                donor.File);
            // Отсутствующий снимок — это отказ восстановления, а не ноль правильных знаков.
            Assert.True(File.Exists(donorPath), "Снимок весов не найден: " + donorPath);
            // Размер файла доказывает, что читается неповреждённая версия.
            Assert.Equal(ExpectedDonorBytes, new FileInfo(donorPath).Length);
            // Отпечаток файла доказывает, что это тот самый сохранённый снимок.
            Assert.Equal(donor.Sha256, FileSha256(donorPath));

            // Модель собирается по тем же настройкам, с которыми снимок сохранялся.
            var model = new LlmModel(options.StartRequest.ModelConfig);
            // Переходник присоединяется до загрузки, как это делает продуктовый
            // путь открытия сохранённого состояния: снимок обучения содержит и
            // тензоры переходника, и без его гнёзд загрузка не сойдётся по
            // числу именованных групп параметров.
            var adapter = LoraApplicator.ValidateAndCanonicalize(
                options.StartRequest.LoraConfig!);
            var seedAuthority = options.StartRequest.AdapterSeedAuthority;
            if (seedAuthority is not null)
            {
                // При объявленной authority переходник ставится детерминированно.
                LoraApplicator.ApplySeeded(
                    model,
                    adapter,
                    seedAuthority.RootInitializationSeed,
                    seedAuthority.PolicyDropoutStreamDomain);
            }
            else
            {
                // Без authority допускается только переходник без отключений.
                LoraApplicator.Apply(model, adapter);
            }

            // Веса берутся из восстановленного снимка; ни одного обучающего шага нет.
            model.LoadCheckpoint(donorPath);
            // Число именованных групп параметров обязано совпасть с карточкой опыта.
            Assert.Equal(
                ExpectedCheckpointParameterEntries,
                model.GetCheckpointParameterEntries().Count);

            // Отпечаток состояния до чтений: с ним сравнивается состояние после.
            var stateBeforeReading = StateSha256(model);

            // Прежняя граница ответа: запрос и ответ склеены встык, без разделителя.
            var legacy = ReadBoundary(
                model,
                options.Tokenizer,
                donor,
                SupervisedResponseReadout.DiagnosticNoSeparatorProfile);
            // Продуктовая граница ответа: та же склейка, что в обучении, через перевод строки.
            var product = ReadBoundary(
                model,
                options.Tokenizer,
                donor,
                SupervisedResponseReadout.SupervisedJointLfProfile);

            // Отпечаток состояния после обоих чтений: обновлений обязан быть ноль.
            var stateAfterReading = StateSha256(model);
            // Побайтное совпадение состояний и есть доказательство нулевого числа обновлений.
            Assert.Equal(stateBeforeReading, stateAfterReading);

            // Продуктовая граница обязана прочитать ответ целиком.
            Assert.Equal(donor.ProductDenominator, product.ResponseTargetCount);
            Assert.Equal(donor.ProductCorrectTokens, product.CorrectTokens);
            Assert.True(product.AllCorrect);
            // Прежняя граница обязана потерять ровно один знак — первый знак ответа.
            Assert.Equal(donor.LegacyDenominator, legacy.ResponseTargetCount);
            Assert.Equal(donor.LegacyCorrectTokens, legacy.CorrectTokens);
            Assert.False(legacy.AllCorrect);
            // Неверная позиция прежней границы — именно первая ответная позиция.
            var legacyWrong = legacy.Positions
                .Where(static position => !position.Correct)
                .ToArray();
            Assert.Equal(legacy.Positions[0].ShiftedIndex, Assert.Single(legacyWrong).ShiftedIndex);
            // Набор нужных знаков ответа у двух границ совпадает знак в знак:
            // различается только левый контекст, а не измеряемая цель.
            Assert.Equal(product.ResponseTargetTokenIds, legacy.ResponseTargetTokenIds);
            // Отпечатки последовательностей различаются: форматы видно по самому отпечатку.
            Assert.NotEqual(product.SequenceDigestSha256, legacy.SequenceDigestSha256);

            // Одна строка улик по этому донору на человеческом языке и в числах.
            observations.Add(new
            {
                строка = donor.Row,
                файл_снимка = donor.File,
                байт = ExpectedDonorBytes,
                отпечаток_снимка = donor.Sha256,
                именованных_групп_параметров = ExpectedCheckpointParameterEntries,
                запрос = donor.Prompt,
                правильный_ответ = donor.Response,
                прежняя_граница = new
                {
                    профиль = legacy.ProfileVersion,
                    качество = $"{legacy.CorrectTokens}/{legacy.ResponseTargetCount}",
                    неверных_позиций = legacyWrong.Length,
                    первая_ответная_позиция_верна = legacy.Positions[0].Correct,
                    отпечаток_последовательности = legacy.SequenceDigestSha256
                },
                продуктовая_граница = new
                {
                    профиль = product.ProfileVersion,
                    качество = $"{product.CorrectTokens}/{product.ResponseTargetCount}",
                    неверных_позиций = product.Positions.Count(static position => !position.Correct),
                    первая_ответная_позиция_верна = product.Positions[0].Correct,
                    отпечаток_последовательности = product.SequenceDigestSha256
                },
                обновлений_при_чтении = 0,
                состояние_до_чтения = stateBeforeReading,
                состояние_после_чтения = stateAfterReading,
                состояние_не_изменилось = StringComparer.Ordinal.Equals(
                    stateBeforeReading,
                    stateAfterReading)
            });
        }

        // Улики пишутся в хранилище проекта, чтобы чтение можно было перечитать позже.
        var evidenceDirectory = Path.Combine(repositoryRoot, EvidenceRelativeDirectory);
        Directory.CreateDirectory(evidenceDirectory);
        File.WriteAllText(
            Path.Combine(evidenceDirectory, "05-чтение-двух-границ.json"),
            JsonSerializer.Serialize(
                new
                {
                    проверка = "штатное чтение двух выученных ответов на восстановленных снимках",
                    читатель = "SupervisedResponseReadout.Evaluate (один вызов модели, обучение выключено)",
                    владелец_формата = "SupervisedTextSequence",
                    продуктовый_профиль = SupervisedResponseReadout.SupervisedJointLfProfile,
                    прежний_профиль = SupervisedResponseReadout.DiagnosticNoSeparatorProfile,
                    разделитель_продукта = "один перевод строки",
                    отпечаток_настроек = ConfigSha256,
                    обновлений_всего = 0,
                    наблюдения = observations
                },
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
                }));
    }

    /// <summary>
    /// Отдельная проверка второго, написанного руками сборщика формата. Оснастка
    /// происхождения учебных строк (<see cref="ProvenanceTestFactory"/>) склеивает
    /// запрос и ответ переводом строки сама, а не берёт склейку у единственного
    /// владельца формата. Номер источника строки — это отпечаток именно этого
    /// склеенного текста, поэтому расхождение двух сборщиков означало бы, что
    /// происхождение описывает не те байты, на которых шло обучение. Проверка
    /// сравнивает оба сборщика на точных байтах и не входит в чтение качества:
    /// ни одного вызова модели и ни одного обновления весов здесь нет.
    /// </summary>
    [Fact]
    public void ProvenanceTestFactory_ManualJointTextMatchesTheSingleFormatOwner()
    {
        foreach (var donor in Donors)
        {
            // Склейка единственного владельца формата обучения с учителем.
            var ownerJointText = SupervisedTextSequence.ComposeJointText(
                donor.Prompt,
                donor.Response);
            // Номер источника, посчитанный по склейке владельца формата.
            var expectedSourceId = ProvenanceTestFactory.SourceId(
                donor.Prompt,
                ownerJointText);
            // Учебная строка, которой оснастка сама выставляет номер источника.
            var example = ProvenanceTestFactory.WithSource(
                new TrainingExample(
                    donor.Prompt,
                    donor.Response,
                    "CODE_Q",
                    1.0d,
                    DateTimeOffset.UnixEpoch));

            // Руками написанная склейка обязана совпасть с владельцем формата
            // знак в знак: иначе номер источника описывает другие байты.
            Assert.Equal(expectedSourceId, example.SourceId);
            // Разделителем остаётся ровно один перевод строки.
            Assert.Equal(
                donor.Prompt + "\n" + donor.Response,
                ownerJointText);
        }
    }

    // Одно чтение одной границы: материал готовится читателем, затем один вызов модели.
    private static SupervisedResponseReadoutResult ReadBoundary(
        LlmModel model,
        Tokenizer.BpeTokenizer tokenizer,
        Donor donor,
        string profileVersion)
    {
        // Материал чтения: знаки строки, граница ответа и номера ответных целей.
        var material = SupervisedResponseReadout.Prepare(
            tokenizer,
            donor.Prompt,
            donor.Response,
            profileVersion);
        // Неподтверждённая граница — отказ чтения, его нельзя считать промахом модели.
        Assert.True(material.IsSuccess, $"{material.ErrorCode}: {material.ErrorMessage}");
        // Единственный вызов модели: веса не меняются, обновлений ноль.
        var readout = SupervisedResponseReadout.Evaluate(model, material.Value!);
        // Отказ чтения так же не превращается в ноль правильных знаков.
        Assert.True(readout.IsSuccess, $"{readout.ErrorCode}: {readout.ErrorMessage}");
        return readout.Value!;
    }

    // Отпечаток файла: им доказывается, что читается именно восстановленный снимок.
    private static string FileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    // Отпечаток состояния модели: сравнением до и после доказывается нулевое обучение.
    private static string StateSha256(LlmModel model)
    {
        using var buffer = new MemoryStream();
        // Состояние выписывается тем же каноническим способом, что и снимок на диск.
        model.SaveCheckpointToStream(buffer);
        // Поток перематывается в начало, иначе отпечаток считается от пустого остатка.
        buffer.Position = 0;
        return Convert.ToHexString(SHA256.HashData(buffer)).ToLowerInvariant();
    }

    // Подъём от папки сборки до корня хранилища по файлу решения.
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            // Корень опознаётся по файлу решения, как в соседних проверках.
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "SUPERVISED_DONOR_READOUT_REPOSITORY_ROOT_NOT_FOUND");
    }
}
