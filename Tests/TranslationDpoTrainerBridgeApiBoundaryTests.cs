namespace LlmFromScratch.Tests;

using System.IO;
using System.Reflection;
using System.Text;
using ContinuousLearning;
using DPO;
using TranslationDomain;

public sealed class TranslationDpoTrainerBridgeApiBoundaryTests
{
    [Fact]
    public void TrainWithStepMany_HasNoUndocumentedPublicDirectTrainerMutationOverload()
    {
        var bridgeType = typeof(TranslationDpoTrainerBridge);
        var directTrainerOverloads = bridgeType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method =>
                method.Name == nameof(TranslationDpoTrainerBridge.TrainWithStepMany) &&
                method.GetParameters().Any(parameter => parameter.ParameterType == typeof(DpoTrainer)))
            .OrderBy(method => string.Join(
                "|",
                method.GetParameters().Select(parameter => parameter.ParameterType.FullName)))
            .ToArray();

        Assert.Equal(2, directTrainerOverloads.Length);
        Assert.Contains(
            directTrainerOverloads,
            method => HasParameterTypes(
                method,
                typeof(TranslationDpoTrainingHandoff),
                typeof(DpoTrainer),
                typeof(int),
                typeof(TranslationDpoTokenizationSettings)));
        var snapshotPublicOverload = Assert.Single(
            directTrainerOverloads,
            method => HasParameterTypes(
                method,
                typeof(TranslationDpoTrainingHandoff),
                typeof(TranslationDpoMutationBatchSnapshot),
                typeof(DpoTrainer),
                typeof(int)));
        var admittedCore = Assert.Single(
            bridgeType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            method => method.Name == "TrainAdmittedWithStepMany");
        Assert.True(admittedCore.IsStatic);
        Assert.False(admittedCore.IsPublic);
        Assert.Null(admittedCore.GetCustomAttribute<ObsoleteAttribute>());
        Assert.Equal(snapshotPublicOverload.ReturnType, admittedCore.ReturnType);
        Assert.True(
            HasParameterTypes(
                admittedCore,
                typeof(TranslationDpoTrainingHandoff),
                typeof(TranslationDpoMutationBatchSnapshot),
                typeof(DpoTrainer),
                typeof(int)));

        var boundaryMessageField = Assert.Single(
            bridgeType.GetFields(BindingFlags.NonPublic | BindingFlags.Static),
            field => field.Name == "DirectTrainerTrainWithStepManyBoundaryMessage");
        Assert.True(boundaryMessageField.IsAssembly);
        Assert.True(boundaryMessageField.IsLiteral);
        var boundaryMessage = Assert.IsType<string>(boundaryMessageField.GetRawConstantValue());

        var obsoleteAttributes = directTrainerOverloads
            .Select(method => Assert.Single(method.GetCustomAttributes<ObsoleteAttribute>()))
            .ToArray();

        Assert.All(obsoleteAttributes, obsolete => Assert.False(obsolete.IsError));
        var message = Assert.Single(
            obsoleteAttributes
                .Select(obsolete => obsolete.Message)
                .Distinct(StringComparer.Ordinal));
        Assert.Equal(boundaryMessage, message);
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.Contains("compatibility/testing surface", message, StringComparison.Ordinal);
        Assert.Contains(nameof(TranslationDpoTrainEvaluateRunner), message, StringComparison.Ordinal);
        Assert.Contains(nameof(IServerOwnedDpoTrainingLeaseBroker), message, StringComparison.Ordinal);
        Assert.Contains("DpoTrainerMutationAdmissionCapability", message, StringComparison.Ordinal);
        Assert.Contains("final fail-closed mutation gate", message, StringComparison.Ordinal);

        Assert.Null(bridgeType.GetCustomAttribute<ObsoleteAttribute>());
        Assert.All(
            bridgeType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => !directTrainerOverloads.Contains(method)),
            method => Assert.Empty(method.GetCustomAttributes<ObsoleteAttribute>()));

        var runnerSource = File.ReadAllText(FindRepositoryFile(
            "27.TranslationDomain",
            "TranslationDpoTrainEvaluateRunner.cs"));
        // Смысловой договор вместо прежнего счётчика вызовов (аудит переноса 9 ноября 2026,
        // пункт о повторяющихся baseline-падениях: «проверить границу допуска в каждом пути,
        // затем заменить хрупкий счётчик смысловым контрактом; простой переход 2→3
        // недостаточен»). Бегунок вправе входить во внутренний допущенный шов столько раз,
        // сколько требует продукт: обычное обучение, ветвь восстановления незавершённой
        // публикации и продолжение обучения в жизненном цикле контрольной точки. Охраняется
        // не число входов, а то, что каждый вход несёт снимок изменений из области
        // обучателя, выданной серверным арендодателем, — то есть именно тот снимок, чей хеш
        // был допущен к изменению весов.
        const string admittedSeamCall =
            "TranslationDpoTrainerBridge.TrainAdmittedWithStepMany(";
        var admittedSeamCallSites = FindOccurrences(runnerSource, admittedSeamCall);
        Assert.NotEmpty(admittedSeamCallSites);
        Assert.All(
            admittedSeamCallSites,
            callSiteIndex =>
            {
                var arguments = ExtractCallArguments(
                    runnerSource,
                    callSiteIndex + admittedSeamCall.Length);
                Assert.Equal(4, arguments.Length);
                Assert.Equal("trainerScope.MutationSnapshot", arguments[1]);
            });
        Assert.Equal(
            0,
            CountOccurrences(
                runnerSource,
                "TranslationDpoTrainerBridge.TrainWithStepMany("));
    }

    private static bool HasParameterTypes(MethodInfo method, params Type[] parameterTypes)
    {
        return method.GetParameters()
            .Select(parameter => parameter.ParameterType)
            .SequenceEqual(parameterTypes);
    }

    private static string FindRepositoryFile(params string[] pathSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathSegments).ToArray());
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            "Could not locate repository file.",
            Path.Combine(pathSegments));
    }

    private static int[] FindOccurrences(string source, string value)
    {
        var positions = new List<int>();
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            positions.Add(index);
            index += value.Length;
        }

        return positions.ToArray();
    }

    private static string[] ExtractCallArguments(
        string source,
        int indexAfterOpeningParenthesis)
    {
        var arguments = new List<string>();
        var current = new StringBuilder();
        var depth = 1;
        var index = indexAfterOpeningParenthesis;
        while (index < source.Length && depth > 0)
        {
            var character = source[index];
            if (character == '(')
            {
                depth++;
            }
            else if (character == ')')
            {
                depth--;
                if (depth == 0)
                    break;
            }

            if (character == ',' && depth == 1)
            {
                arguments.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }

            index++;
        }

        var last = current.ToString().Trim();
        if (last.Length != 0)
            arguments.Add(last);

        return arguments.ToArray();
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
