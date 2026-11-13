// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

using SemanticCompilerAdapter;
using Domain = SemanticCompiler29.Domain;

// Compatibility surface for historical experiment fixtures.
// The classification source of truth is SemanticCompilerAdapter.DomainClassifierUnit.
public sealed class DomainClassifier
{
    public const int CODE_QUESTION = 0;
    public const int BUG_FIX = 1;
    public const int CODE_CHANGE = 2;
    public const int ORCHESTRATION = 3;
    public const int ARCHITECTURE = 4;

    public static readonly string[] DomainNames =
        ["CODE_QUESTION", "BUG_FIX", "CODE_CHANGE", "ORCHESTRATION", "ARCHITECTURE"];

    private readonly DomainClassifierUnit _classifier;
    private bool _trained;

    public DomainClassifier(int k = 5)
    {
        _classifier = new DomainClassifierUnit(new SemanticCompiler.RouteClassifier(k));
    }

    public void Train(IReadOnlyList<(int[] Predicates, int Domain)> examples)
    {
        _classifier.Train(examples
            .Select(example => (example.Predicates, Domain: ToCanonicalDomain(example.Domain)))
            .ToList());
        _trained = examples.Count > 0;
    }

    public int Predict(int[] predicates)
    {
        if (!_trained)
            return CODE_QUESTION;

        var classification = _classifier.Classify(new DomainClassifierRequest(
            Prompt: string.Empty,
            Predicates: predicates));
        return FromCanonicalDomain(classification.Label);
    }

    public static int PredictByKeywords(string text)
    {
        var classification = new DomainClassifierUnit().Classify(new DomainClassifierRequest(text ?? string.Empty));
        return FromCanonicalDomain(classification.Label);
    }

    public static int AutoLabel(string prompt) => PredictByKeywords(prompt);

    private static Domain ToCanonicalDomain(int domain)
    {
        return domain switch
        {
            BUG_FIX => Domain.BUG,
            CODE_CHANGE => Domain.REFACTOR,
            ORCHESTRATION => Domain.PLAN,
            ARCHITECTURE => Domain.ARCH,
            _ => Domain.CODE_Q
        };
    }

    private static int FromCanonicalDomain(Domain domain)
    {
        return domain switch
        {
            Domain.BUG => BUG_FIX,
            Domain.REFACTOR => CODE_CHANGE,
            Domain.PLAN => ORCHESTRATION,
            Domain.ARCH => ARCHITECTURE,
            _ => CODE_QUESTION
        };
    }
}
