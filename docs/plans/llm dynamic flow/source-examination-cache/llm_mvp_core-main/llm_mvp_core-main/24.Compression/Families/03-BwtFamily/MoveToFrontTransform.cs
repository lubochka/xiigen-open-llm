namespace Compression;

// Move-To-Front transform — converts BWT output (runs of repeated bytes)
// into a stream of small numbers (mostly zeros) that compress well.
// After BWT, each repeated byte in a run outputs 0 repeatedly.
// Reference: bzip2 compress.c (BSD license)
public static class MoveToFrontTransform
{
    // Apply Move-To-Front encoding.
    // Input: typically BWT-transformed bytes.
    // Output: integers in 0-255 range; small values indicate recently-seen bytes.
    public static int[] Forward(byte[] input)
    {
        // Initialize the symbol list in canonical order: [0, 1, 2, ..., 255].
        var symbolList = Enumerable.Range(0, 256).ToArray();
        var output     = new int[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            int b        = input[i];
            int position = Array.IndexOf(symbolList, b);

            output[i] = position; // small = recently seen

            // Move the symbol to the front, shifting others right by one.
            for (int j = position; j > 0; j--)
                symbolList[j] = symbolList[j - 1];
            symbolList[0] = b;
        }

        return output;
    }

    // Reverse the Move-To-Front transform.
    public static byte[] Inverse(int[] encoded)
    {
        var symbolList = Enumerable.Range(0, 256).ToArray();
        var output     = new byte[encoded.Length];

        for (int i = 0; i < encoded.Length; i++)
        {
            int position = encoded[i];
            int b        = symbolList[position];
            output[i]    = (byte)b;

            for (int j = position; j > 0; j--)
                symbolList[j] = symbolList[j - 1];
            symbolList[0] = b;
        }

        return output;
    }
}
