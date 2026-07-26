// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MatrixAlgorithms;

// Int8Quantizer compresses a weight matrix by mapping each floating-point weight
// to the nearest signed 8-bit integer (range -128 to 127).
// This reduces storage from 8 bytes per weight (double) to 1 byte — an 8× saving.
// The mapping is affine (a linear mapping):
//   quantized = round(weight / Scale) + ZeroPoint
//   weight ≈ (quantized - ZeroPoint) * Scale
// Scale and ZeroPoint are chosen so that the full float range [min, max] maps to [-128, 127].
// The small accuracy loss is acceptable for many neural network inference tasks.
public static class Int8Quantizer
{
    // Quantise an array of floating-point weights to 8-bit integers.
    // The returned QuantizedWeightMatrix includes the scale and zero-point
    // needed to convert back to approximate floating-point values.
    public static QuantizedWeightMatrix Quantize(double[] weights, int rows, int columns)
    {
        if (weights.Length < rows * columns)
            throw new ArgumentException(
                $"Weights array length {weights.Length} is too short for {rows}×{columns}.");

        int count = rows * columns;

        // Find the minimum and maximum weight values.
        // These define the float range that must be covered by the 8-bit range [-128, 127].
        double minWeight = weights[0];
        double maxWeight = weights[0];
        for (int i = 1; i < count; i++)
        {
            if (weights[i] < minWeight) minWeight = weights[i];
            if (weights[i] > maxWeight) maxWeight = weights[i];
        }

        // Handle the degenerate case where all weights are the same.
        if (maxWeight - minWeight < 1e-15)
        {
            // All weights equal — scale is 1, all quantized values are 0.
            var trivial = new sbyte[count];
            return new QuantizedWeightMatrix(trivial, 1.0, 0, rows, columns, 0.0);
        }

        // Scale maps the float range [minWeight, maxWeight] to the 8-bit range 0..255,
        // then we shift by 128 to centre it at zero so the signed range is -128..127.
        // scale = (maxWeight - minWeight) / 255  (one 8-bit unit = scale floating-point units)
        double scale = (maxWeight - minWeight) / 255.0;

        // ZeroPoint is the quantized integer that corresponds to floating-point 0.
        // zeroPoint = round(0 - minWeight) / scale = round(-minWeight / scale)
        int zeroPoint = (int)Math.Round(-minWeight / scale);
        // Clamp zeroPoint to the valid 8-bit signed integer range to avoid overflow.
        zeroPoint = Math.Clamp(zeroPoint, -128, 127);

        var quantized     = new sbyte[count];
        double maxAbsError = 0.0;

        for (int i = 0; i < count; i++)
        {
            // Map weight to an integer: q = round(weight / scale) + zeroPoint
            int rawQ = (int)Math.Round(weights[i] / scale) + zeroPoint;

            // Clamp to the signed 8-bit range so overflow cannot occur.
            rawQ = Math.Clamp(rawQ, -128, 127);
            quantized[i] = (sbyte)rawQ;

            // Track the maximum absolute reconstruction error for the report.
            double reconstructed = (rawQ - zeroPoint) * scale;
            double absError      = Math.Abs(weights[i] - reconstructed);
            if (absError > maxAbsError) maxAbsError = absError;
        }

        return new QuantizedWeightMatrix(quantized, scale, zeroPoint, rows, columns, maxAbsError);
    }

    // Convert a QuantizedWeightMatrix back to approximate floating-point weights.
    // Each weight is reconstructed as: (quantized[i] - ZeroPoint) × Scale
    public static double[] Dequantize(QuantizedWeightMatrix quantized)
    {
        int count  = quantized.Rows * quantized.Columns;
        var result = new double[count];

        for (int i = 0; i < count; i++)
            result[i] = (quantized.QuantizedValues[i] - quantized.ZeroPoint) * quantized.Scale;

        return result;
    }
}
