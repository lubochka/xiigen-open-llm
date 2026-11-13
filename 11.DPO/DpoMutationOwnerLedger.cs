// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// Учёт пар «объявленный владелец ↔ фактически изменяемый набор весов» на границе изменения.
//
// Назначение: находка опыта сто двадцать один (запись 117 в едином реестре переносов,
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md).
//
// ЧТО СЛОМАНО РОВНО. На границе изменения весов не было объявленного владельца вовсе. Кто
// получатель — решал тот, кто передал модель: попросив обучить один проект и передав модель
// другого, вызывающий получал обновление весов другого проекта без единого возражения. Опыт
// измерил это прямо: правильный адрес получателя 0 из 1 до правки и 1 из 1 после; отпечаток
// весов в обоих мирах одинаковый, расхождение продуктового измерителя ровно ноль — отличался
// ТОЛЬКО владелец, и поймал это проверяющий по отпечаткам, а не продукт.
//
// ЧТО ДЕЛАЕТ ЭТОТ УЧЁТ. Запоминает пару «владелец ↔ отпечаток набора весов» при первом
// объявлении и отказывает, когда та же пара приходит противоречиво: один владелец над другим
// набором весов, или один набор весов под другим владельцем. Обе половины лжи об адресе
// называются своими кодами.
//
// НА ЧЁМ ДЕРЖИТСЯ ЛИЧНОСТЬ, И КТО ЕЁ МОЖЕТ ПОДДЕЛАТЬ — отвечаю прямо, потому что это главный
// вопрос этой починки. Личность держится на тождестве ССЫЛОК фактических объектов величин:
// отпечаток приходит сюда посчитанным из ключей привязок получателя и личности замороженного
// эталона, то есть из того, что вызывающий о себе ЗАЯВИТЬ не может — он либо передал те же
// объекты, либо другие. Поэтому подделать соответствие заявлением нельзя.
//
// Чего этот учёт НЕ закрывает, и я не выдаю это за закрытое. Внутренний доступ к сборке
// обучения выдаётся по ИМЕНИ сборки, поэтому кто угодно под разрешённым именем может сам выдать
// себе возможность изменения и объявить любого владельца. Этот учёт ловит ПРОТИВОРЕЧИЕ между
// объявлениями над одним набором весов; он не делает имя сборки проверяемой личностью.
// Переименованием ту дыру не чинят: её закрытие — продуктовое решение владельца проекта
// (проверяемая личность сборки либо узкий публичный способ получить возможность), и оно названо
// в записи 117 реестра.
namespace DPO;

/// <summary>Исход сверки владельца с фактически изменяемым набором весов.</summary>
internal sealed record DpoOwnerLedgerVerdict(
    bool Admitted,      // Сверка пройдена или отказана.
    string ErrorCode,   // Названный код отказа; у прохода пустая строка.
    string Human);      // Человеческое объяснение исхода.

/// <summary>
/// Учёт владельцев изменяемых наборов весов. Живёт ВНУТРИ выданной возможности, то есть
/// принадлежит выдавшему её, а не вызывающему: подменить учёт своим вызывающий не может,
/// потому что он возможность не создаёт, а получает.
/// </summary>
internal sealed class DpoMutationOwnerLedger
{
    /// <summary>Объявленный владелец пришёл над ДРУГИМ набором весов, чем прежде.</summary>
    internal const string OwnerWeightsChangedErrorCode =
        "DPO_TRAINER_DECLARED_OWNER_OVER_FOREIGN_WEIGHTS";

    /// <summary>Тот же набор весов пришёл под ДРУГИМ объявленным владельцем.</summary>
    internal const string WeightsOwnerChangedErrorCode =
        "DPO_TRAINER_WEIGHTS_CLAIMED_BY_ANOTHER_OWNER";

    /// <summary>Отпечаток набора весов пуст: сверять нечего, и молча проходить нельзя.</summary>
    internal const string EmptyWeightsIdentityErrorCode =
        "DPO_TRAINER_EMPTY_WEIGHTS_IDENTITY";

    // Какой набор весов закреплён за объявленным владельцем.
    private readonly Dictionary<string, string> _weightsByOwner = new(StringComparer.Ordinal);

    // Какой владелец закреплён за набором весов. Второе отображение нужно именно затем, чтобы
    // ловить обратную половину лжи об адресе: тот же набор под другим именем владельца.
    private readonly Dictionary<string, string> _ownerByWeights = new(StringComparer.Ordinal);

    // RequireOwnerMatchesWeights сверяет объявленного владельца с фактически изменяемым набором.
    //
    // Первое объявление закрепляет пару и проходит: до него сравнивать не с чем. Дальше любое
    // противоречие отказывается названным кодом. Такой порядок выбран потому, что продукт не
    // имеет сегодня доверенного списка «проект → его модель»: пока такого списка нет, честно
    // доказуемо именно ПРОТИВОРЕЧИЕ между объявлениями, а не подлинность имени.
    internal DpoOwnerLedgerVerdict RequireOwnerMatchesWeights(
        string declaredOwnerId,
        string actualWeightsIdentity)
    {
        if (string.IsNullOrWhiteSpace(actualWeightsIdentity))
            // Пустой отпечаток не является совпадением: сверять нечего.
            return new DpoOwnerLedgerVerdict(
                false,
                EmptyWeightsIdentityErrorCode,
                "отпечаток фактически изменяемого набора весов пуст: сверка владельца невозможна");

        var owner = declaredOwnerId ?? string.Empty; // Объявленный владелец одной величиной.

        if (_weightsByOwner.TryGetValue(owner, out var rememberedWeights) &&
            !string.Equals(rememberedWeights, actualWeightsIdentity, StringComparison.Ordinal))
            // Тот же владелец пришёл над другим набором весов: урок уходит чужим весам.
            return new DpoOwnerLedgerVerdict(
                false,
                OwnerWeightsChangedErrorCode,
                "владелец «" + owner + "» уже закреплён за другим набором весов: объявленный " +
                "владелец не совпадает с тем, чьи веса меняются");

        if (_ownerByWeights.TryGetValue(actualWeightsIdentity, out var rememberedOwner) &&
            !string.Equals(rememberedOwner, owner, StringComparison.Ordinal))
            // Тот же набор весов пришёл под другим владельцем: обратная половина той же лжи.
            return new DpoOwnerLedgerVerdict(
                false,
                WeightsOwnerChangedErrorCode,
                "этот набор весов уже закреплён за владельцем «" + rememberedOwner +
                "»: второй владелец на тех же весах не допускается");

        _weightsByOwner[owner] = actualWeightsIdentity; // Пара закрепляется при первом объявлении.
        _ownerByWeights[actualWeightsIdentity] = owner; // И в обратную сторону — для второй половины.

        // Объявленный владелец и фактически изменяемый набор весов не противоречат учёту.
        return new DpoOwnerLedgerVerdict(true, string.Empty,
            "объявленный владелец совпадает с набором весов, которые фактически меняются");
    }
}
