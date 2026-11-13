// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO;

/// <summary>
/// Internal proof that a DPO mutation capability was issued by a trusted
/// admission boundary. Public callers cannot implement this marker.
/// </summary>
internal interface IDpoMutationAdmissionIssuer
{
    IDisposable AcquireDpoTrainerMutationPermit();
}

/// <summary>
/// Opaque, revocable authority required by the low-level DPO trainer.
/// </summary>
public sealed class DpoMutationAdmissionCapability
{
    public const string RequiredErrorCode =
        "DPO_TRAINER_MUTATION_ADMISSION_CAPABILITY_REQUIRED";

    private readonly IDpoMutationAdmissionIssuer _issuer;

    // _declaredOwnerId — владелец, объявленный ТОЙ СТОРОНОЙ, которая выдаёт возможность.
    //
    // Зачем он здесь, а не у вызывающего. Объявление владельца должно приходить от доверенной
    // стороны: вызывающий возможность не создаёт, а получает, поэтому подменить объявление
    // своим он не может. Пустое значение означает прежний путь без объявленного владельца, и
    // он намеренно сохранён: ни один нынешний вызывающий от этой правки не ломается.
    private readonly string? _declaredOwnerId;

    // _ownerLedger — учёт пар «владелец ↔ фактически изменяемый набор весов». Он принадлежит
    // выданной возможности, то есть выдавшей стороне, и создаётся вместе с ней.
    private readonly DpoMutationOwnerLedger _ownerLedger = new();

    private DpoMutationAdmissionCapability(
        IDpoMutationAdmissionIssuer issuer,
        string? declaredOwnerId)
    {
        _issuer = issuer;
        _declaredOwnerId = declaredOwnerId; // Объявление владельца фиксируется при выдаче.
    }

    // Issue выдаёт возможность. Довод с объявленным владельцем необязательный и стоит
    // последним, поэтому прежние выдачи работают без правок.
    internal static DpoMutationAdmissionCapability Issue(
        IDpoMutationAdmissionIssuer issuer,
        string? declaredOwnerId = null)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        return new DpoMutationAdmissionCapability(issuer, declaredOwnerId);
    }

    // DeclaredOwnerId — объявленный владелец этой возможности, или пустое значение, когда
    // владелец не объявлялся. Нужен обучателю, чтобы отличить прежний путь от сверяемого.
    internal string? DeclaredOwnerId => _declaredOwnerId;

    // RequireOwnerMatchesWeights сверяет объявленного при выдаче владельца с ФАКТИЧЕСКИ
    // изменяемым набором весов. Отпечаток набора считает обучатель из привязок получателя: это
    // тождество ссылок настоящих объектов, а не заявление вызывающего о себе.
    //
    // Отказ выражается тем же исключением допуска, что и сегодня: поведение границы не
    // меняется, добавляется названная причина отказа. Когда владелец не объявлялся, сверка не
    // выполняется — это прежний путь, и он назван прямо.
    internal void RequireOwnerMatchesWeights(string actualWeightsIdentity)
    {
        if (string.IsNullOrWhiteSpace(_declaredOwnerId))
            return; // Прежний путь без объявленного владельца сохранён целиком.

        var verdict = _ownerLedger.RequireOwnerMatchesWeights(_declaredOwnerId!, actualWeightsIdentity);
        if (!verdict.Admitted)
            throw new DpoMutationAdmissionException(verdict.Human, verdict.ErrorCode); // Отказ названным кодом.
    }

    internal IDisposable AcquireMutationPermit()
        => _issuer.AcquireDpoTrainerMutationPermit()
           ?? throw new DpoMutationAdmissionException(
               "A live DPO trainer mutation permit is required.",
               RequiredErrorCode);
}

/// <summary>
/// Stable fail-closed exception emitted when low-level DPO mutation authority
/// is absent, revoked, or otherwise unavailable.
/// </summary>
public sealed class DpoMutationAdmissionException : InvalidOperationException
{
    internal DpoMutationAdmissionException(
        string message,
        string errorCode)
        : base($"{errorCode}: {message}")
    {
        ErrorCode = string.IsNullOrWhiteSpace(errorCode)
            ? DpoMutationAdmissionCapability.RequiredErrorCode
            : errorCode;
    }

    public string ErrorCode { get; }
}
