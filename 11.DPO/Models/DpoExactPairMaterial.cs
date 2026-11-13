// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using TensorCore.Core;
using Tokenizer;

namespace DPO.Models;

/// <summary>
/// Detached, exact assessed pair material. This commitment is descriptive data,
/// not teacher, licence, mutation or publication authority.
/// </summary>
public sealed class DpoExactPairMaterial
{
    public const string LegacyVersion = "legacy-full-sequence-mean-v1";
    public const string Version = "segmented-byte-BPE-v2";
    public const string FramingVersion = "prompt-response-segments-no-separator-v2";
    public const string ObjectiveVersion = "response-sum-softplus-signal-weighted-v3";
    public const string InvalidErrorCode = "DPO_EXACT_MATERIAL_INVALID";
    public const string CapacityErrorCode = "DPO_EXACT_MATERIAL_CAPACITY_EXCEEDED";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly float[] _chosenIds, _rejectedIds;
    private readonly float[] _chosenMask, _rejectedMask;
    private readonly float[] _chosenResponseMask, _rejectedResponseMask;

    private DpoExactPairMaterial(string prompt, string chosen, string rejected,
        DpoTrainerPairAdmission admission, string tokenizerSha256, int tokenizerVocabSize,
        int[] promptIds, int[] chosenResponse, int[] rejectedResponse)
    {
        Prompt = prompt; Chosen = chosen; Rejected = rejected;
        AdmissionFingerprintSha256 = admission.ContentFingerprintSha256;
        TokenizerSha256 = tokenizerSha256; TokenizerVocabSize = tokenizerVocabSize;
        PromptLength = promptIds.Length;
        ChosenLength = checked(PromptLength + chosenResponse.Length);
        RejectedLength = checked(PromptLength + rejectedResponse.Length);
        SequenceLength = Math.Max(ChosenLength, RejectedLength);
        _chosenIds = Tokens(promptIds, chosenResponse, SequenceLength);
        _rejectedIds = Tokens(promptIds, rejectedResponse, SequenceLength);
        _chosenMask = Mask(0, ChosenLength, SequenceLength);
        _rejectedMask = Mask(0, RejectedLength, SequenceLength);
        _chosenResponseMask = Mask(PromptLength, ChosenLength, SequenceLength);
        _rejectedResponseMask = Mask(PromptLength, RejectedLength, SequenceLength);
        DigestSha256 = ComputeDigest();
    }

    public string Prompt { get; }
    public string Chosen { get; }
    public string Rejected { get; }
    public string AdmissionFingerprintSha256 { get; }
    public string TokenizerSha256 { get; }
    public int TokenizerVocabSize { get; }
    public int PromptLength { get; }
    public int ChosenLength { get; }
    public int RejectedLength { get; }
    public int SequenceLength { get; }
    public string DigestSha256 { get; }

    // Only the existing trusted producer can create this carrier. The tokenizer
    // is frozen before encoding; no callback can rewrite its later segments.
    internal static DpoBatch CreateBatch(BpeTokenizer tokenizer, string prompt,
        string chosen, string rejected, DpoTrainerPairAdmission admission)
    {
        CheckText(prompt); CheckText(chosen); CheckText(rejected);
        if (StringComparer.Ordinal.Equals(chosen, rejected)) Invalid();
        using var state = new MemoryStream();
        tokenizer.Save(state); state.Position = 0;
        var capturedTokenizer = BpeTokenizer.Load(state);
        var promptIds = EncodeSegment(capturedTokenizer, prompt);
        var chosenIds = EncodeSegment(capturedTokenizer, chosen);
        var rejectedIds = EncodeSegment(capturedTokenizer, rejected);
        if (chosenIds.SequenceEqual(rejectedIds)) Invalid();
        var material = new DpoExactPairMaterial(prompt, chosen, rejected, admission,
            capturedTokenizer.ComputeStateSha256(), capturedTokenizer.VocabSize,
            promptIds, chosenIds, rejectedIds);
        var batch = new DpoBatch
        {
            ChosenIds = material.Tensor(material._chosenIds),
            RejectedIds = material.Tensor(material._rejectedIds),
            ChosenMask = material.Tensor(material._chosenMask),
            RejectedMask = material.Tensor(material._rejectedMask),
            ChosenResponseMask = material.Tensor(material._chosenResponseMask),
            RejectedResponseMask = material.Tensor(material._rejectedResponseMask),
            Admission = admission, MaterialVersion = Version, ExactMaterial = material
        };
        material.Validate(batch, int.MaxValue, int.MaxValue);
        return batch;
    }

    /// <summary>Capture all mutable tensors before any permission callback.</summary>
    /// <param name="shiftedInput">
    /// FIX-SC. Ложь — прежний договор: модель получает всю последовательность, и
    /// в окно обязаны влезть все её T знаков. Истина — назначенный сдвинутый
    /// профиль: модель получает префикс без последнего знака, поэтому в окно
    /// обязаны влезть T−1 знаков, то есть допустимо T = C + 1. Значение по
    /// умолчанию оставляет путь без профиля побайтово прежним.
    /// </param>
    internal static DpoBatch CaptureAndValidate(DpoBatch batch, int vocabSize,
        int contextLength, string? expectedTokenizerSha256 = null,
        string? expectedVersion = null, bool shiftedInput = false)
    {
        ArgumentNullException.ThrowIfNull(batch);
        var snapshot = batch with
        {
            ChosenIds = Copy(batch.ChosenIds), RejectedIds = Copy(batch.RejectedIds),
            ChosenMask = Copy(batch.ChosenMask), RejectedMask = Copy(batch.RejectedMask),
            ChosenResponseMask = batch.ChosenResponseMask is null ? null : Copy(batch.ChosenResponseMask),
            RejectedResponseMask = batch.RejectedResponseMask is null ? null : Copy(batch.RejectedResponseMask)
        };
        if (expectedVersion is not null && snapshot.MaterialVersion != expectedVersion) Invalid();
        if (snapshot.MaterialVersion == LegacyVersion)
        {
            if (snapshot.ExactMaterial is not null || snapshot.ChosenResponseMask is not null ||
                snapshot.RejectedResponseMask is not null) Invalid();
            return snapshot;
        }
        if (snapshot.MaterialVersion != Version || snapshot.ExactMaterial is null) Invalid();
        // FIX-SC: назначенный профиль входа передаётся дальше без изменения: ровно
        // он решает, сколько знаков обязано влезть в окно модели.
        snapshot.ExactMaterial!.Validate(snapshot, vocabSize, contextLength, shiftedInput);
        if (expectedTokenizerSha256 is not null &&
            snapshot.ExactMaterial.TokenizerSha256 != expectedTokenizerSha256) Invalid();
        return snapshot;
    }

    private void Validate(DpoBatch batch, int vocabSize, int contextLength, bool shiftedInput = false)
    {
        if (batch.Admission is null || PromptLength < 1 ||
            ChosenLength <= PromptLength || RejectedLength <= PromptLength ||
            batch.Admission.ContentFingerprintSha256 != AdmissionFingerprintSha256) Invalid();
        var recomputedAdmission = DpoTrainerPairCustoms.CreateAdmitted(Prompt, Chosen, Rejected,
            batch.Admission!.ChosenAuthor, batch.Admission.RejectedAuthor, batch.Admission.IssuerName).Admission;
        if (recomputedAdmission.ContentFingerprintSha256 != AdmissionFingerprintSha256 ||
            ComputeDigest() != DigestSha256) Invalid();
        // FIX-SC: сколько знаков входа модель действительно обязана вместить.
        // Без сдвинутого профиля это по-прежнему вся длина последовательности.
        var requiredInputLength = SequenceLength;
        if (shiftedInput)
        {
            // Сдвинутый профиль подаёт модели префикс без последнего знака, поэтому
            // ему нужно на одну позицию меньше. Проверка T >= 2 стоит здесь явно:
            // у префикса длины ноль нет ни одной строки предсказаний, и такой
            // материал нельзя пропускать молча, даже если он уже отклонён выше.
            if (SequenceLength < 2) Invalid();
            requiredInputLength = SequenceLength - 1;
        }
        // Цели не обрезаются и вместимость входа не подменяется числом оцениваемых
        // знаков: T = C + 1 принимается, T = C + 2 отклоняется тем же кодом отказа.
        if (requiredInputLength > contextLength)
            throw new DpoMutationAdmissionException("Exact DPO material exceeds model context capacity.", CapacityErrorCode);
        Match(batch.ChosenIds, _chosenIds); Match(batch.RejectedIds, _rejectedIds);
        Match(batch.ChosenMask, _chosenMask); Match(batch.RejectedMask, _rejectedMask);
        Match(batch.ChosenResponseMask, _chosenResponseMask); Match(batch.RejectedResponseMask, _rejectedResponseMask);
        foreach (var ids in new[] { _chosenIds, _rejectedIds })
            foreach (var id in ids)
                if (!float.IsFinite(id) || id < 0 || id >= vocabSize || id >= TokenizerVocabSize ||
                    id != MathF.Truncate(id) || (float)(int)id != id) Invalid();
    }

    private static int[] EncodeSegment(BpeTokenizer tokenizer, string text)
    {
        var ids = tokenizer.Encode(text);
        if (ids.Count == 0 || !StringComparer.Ordinal.Equals(tokenizer.Decode(ids), text)) Invalid();
        foreach (var id in ids)
            if (id < 0 || id >= tokenizer.VocabSize || (int)(float)id != id) Invalid();
        return ids.ToArray();
    }

    private static void CheckText(string text)
    {
        if (string.IsNullOrEmpty(text)) Invalid();
        try
        {
            if (StrictUtf8.GetString(StrictUtf8.GetBytes(text)) != text) Invalid();
        }
        catch (EncoderFallbackException) { Invalid(); }
        catch (DecoderFallbackException) { Invalid(); }
    }

    private Tensor Tensor(float[] data) => new(data.ToArray(), new TensorShape(1, SequenceLength));
    private static Tensor Copy(Tensor tensor)
    {
        if (tensor is null) Invalid();
        return new Tensor(tensor!.Data.ToArray(), new TensorShape(tensor.Shape.Dimensions.ToArray()));
    }
    private void Match(Tensor? actual, float[] expected)
    {
        if (actual is null || actual.Shape.Rank != 2 || actual.Shape.Dimensions[0] != 1 ||
            actual.Shape.Dimensions[1] != SequenceLength || !actual.Data.SequenceEqual(expected)) Invalid();
    }
    private static float[] Tokens(int[] prompt, int[] response, int length)
    {
        var ids = new float[length];
        for (var i = 0; i < prompt.Length; i++) ids[i] = prompt[i];
        for (var i = 0; i < response.Length; i++) ids[prompt.Length + i] = response[i];
        return ids;
    }
    private static float[] Mask(int start, int end, int length)
    {
        var data = new float[length];
        Array.Fill(data, 1f, start, end - start); return data;
    }
    private string ComputeDigest()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, StrictUtf8, leaveOpen: true))
        {
            foreach (var text in new[] { Version, FramingVersion, ObjectiveVersion,
                Prompt, Chosen, Rejected, AdmissionFingerprintSha256, TokenizerSha256 }) writer.Write(text);
            writer.Write(TokenizerVocabSize); writer.Write(PromptLength);
            writer.Write(ChosenLength); writer.Write(RejectedLength); writer.Write(SequenceLength);
            foreach (var data in new[] { _chosenIds, _rejectedIds, _chosenMask, _rejectedMask,
                _chosenResponseMask, _rejectedResponseMask })
            {
                writer.Write(data.Length); foreach (var value in data) writer.Write(value);
            }
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
    private static void Invalid() => throw new DpoMutationAdmissionException(
        "Exact DPO material, version, tokenizer, tokens or assessed pair binding is invalid.", InvalidErrorCode);
}
