namespace Compression;

// Burrows-Wheeler Transform — rearranges bytes so identical bytes cluster together.
// After BWT, a simple entropy coder can achieve much better compression.
// Reference: bzip2 blocksort.c (BSD license)
public static class BurrowsWheelerTransform
{
    // Apply the forward BWT to a block of bytes.
    // Returns the transformed bytes and the row index of the original string.
    // The row index is required for the inverse transform.
    public static (byte[] Transformed, int OriginalRow) Forward(byte[] input)
    {
        int n = input.Length;

        // Build and sort all rotation start indices by their rotation's lexicographic order.
        var sortedRotations = Enumerable.Range(0, n).ToArray();
        Array.Sort(sortedRotations, (a, b) => CompareRotations(input, a, b, n));

        var transformed = new byte[n];
        int originalRow = -1;

        for (int rank = 0; rank < n; rank++)
        {
            // The last character of the rotation starting at sortedRotations[rank].
            int lastPos      = (sortedRotations[rank] + n - 1) % n;
            transformed[rank] = input[lastPos];

            // The rotation that starts at position 0 is the original string.
            if (sortedRotations[rank] == 0)
                originalRow = rank;
        }

        return (transformed, originalRow);
    }

    // Reverse the BWT to reconstruct the original bytes.
    // bwtOutput and originalRow must come from a previous call to Forward().
    public static byte[] Inverse(byte[] bwtOutput, int originalRow)
    {
        int n = bwtOutput.Length;

        // Sort the BWT output to get the first column of the rotation table.
        var firstColumn = (byte[])bwtOutput.Clone();
        Array.Sort(firstColumn);

        // Build the 'next' array: next[i] gives the F-column row reached by following
        // the BWT link from L-column row i.  next[i] = C[L[i]] + rank(L[i], i)
        // where C[c] = number of bytes in L strictly less than c.
        var next   = new int[n];
        var counts = new int[256];
        foreach (byte b in bwtOutput) counts[b]++;

        // Cumulative sum: firstColumnPos[c] = position in F where byte c starts.
        var firstColumnPos = new int[256];
        int cumSum = 0;
        for (int sym = 0; sym < 256; sym++)
        {
            firstColumnPos[sym] = cumSum;
            cumSum += counts[sym];
        }

        // next[i] = F-position of the i-th L entry.
        var nextCounter = (int[])firstColumnPos.Clone();
        for (int rank = 0; rank < n; rank++)
        {
            byte b     = bwtOutput[rank];
            next[rank] = nextCounter[b]++;
        }

        // Walk the 'next' array starting from originalRow to recover the original.
        var original = new byte[n];
        int cur = originalRow;
        for (int i = n - 1; i >= 0; i--)
        {
            original[i] = bwtOutput[cur];
            cur          = next[cur];
        }

        return original;
    }

    // Compare two rotations (starting at startA and startB) lexicographically.
    private static int CompareRotations(byte[] input, int startA, int startB, int length)
    {
        for (int offset = 0; offset < length; offset++)
        {
            byte a = input[(startA + offset) % length];
            byte b = input[(startB + offset) % length];
            if (a != b) return a.CompareTo(b);
        }
        return 0;
    }

}
