namespace Compression;

// One node in the Huffman tree — either a leaf (has a symbol) or an internal junction.
public sealed class HuffmanNode
{
    public int         Symbol    { get; init; }  // valid only for leaf nodes
    public long        Frequency { get; init; }
    public bool        IsLeaf    { get; init; }
    public HuffmanNode? Left     { get; init; }
    public HuffmanNode? Right    { get; init; }
}

// Builds the optimal Huffman code tree for a set of symbol frequencies.
// After Build(), each symbol has a bit code (Codes[sym]) and bit length (CodeLengths[sym]).
public sealed class HuffmanTree
{
    // Bit pattern assigned to each symbol (indexed by byte value 0-255).
    public uint[] Codes       { get; private set; } = new uint[256];

    // Number of bits in each symbol's code.
    public int[]  CodeLengths { get; private set; } = new int[256];

    // Build the optimal Huffman code tree from symbol frequency counts.
    public void Build(long[] frequencies)
    {
        // Collect leaf nodes for every symbol that appears at least once.
        var nodes = new List<HuffmanNode>();
        for (int sym = 0; sym < 256; sym++)
        {
            if (frequencies[sym] > 0)
                nodes.Add(new HuffmanNode { Symbol = sym, Frequency = frequencies[sym], IsLeaf = true });
        }

        if (nodes.Count == 0) return;

        // Single symbol — assign a 1-bit code.
        if (nodes.Count == 1)
        {
            Codes[nodes[0].Symbol]       = 0;
            CodeLengths[nodes[0].Symbol] = 1;
            return;
        }

        // Build the tree bottom-up using a sorted list as a priority queue.
        while (nodes.Count > 1)
        {
            // Sort ascending by frequency — lowest frequency first.
            nodes.Sort((a, b) => a.Frequency.CompareTo(b.Frequency));

            // Merge the two least-frequent nodes into one internal node.
            var left  = nodes[0];
            var right = nodes[1];
            nodes.RemoveRange(0, 2);

            nodes.Add(new HuffmanNode
            {
                Frequency = left.Frequency + right.Frequency,
                IsLeaf    = false,
                Left      = left,
                Right     = right
            });
        }

        // Assign codes by walking from the root: left = 0, right = 1.
        AssignCodes(nodes[0], 0u, 0);
    }

    // Walk the tree recursively, accumulating the bit pattern for each leaf.
    private void AssignCodes(HuffmanNode node, uint code, int depth)
    {
        if (node.IsLeaf)
        {
            Codes[node.Symbol]       = code;
            CodeLengths[node.Symbol] = depth == 0 ? 1 : depth;
            return;
        }

        if (node.Left  is not null) AssignCodes(node.Left,  code << 1,        depth + 1);
        if (node.Right is not null) AssignCodes(node.Right, (code << 1) | 1u, depth + 1);
    }
}
