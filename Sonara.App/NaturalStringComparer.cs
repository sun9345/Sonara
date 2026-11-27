namespace Sonara.App
{
    public sealed class NaturalStringComparer : IComparer<string?>
    {
        public static NaturalStringComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0; 
            if (x is null) return -1;            
            if (y is null) return 1;

            int i = 0, j = 0; 

            while (i < x.Length && j < y.Length) 
            {
                if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j])) 
                {
                    int startI = i, startJ = j; 
                    while (i < x.Length && char.IsAsciiDigit(x[i])) i++; 
                    while (j < y.Length && char.IsAsciiDigit(y[j])) j++; 

                    var numX = x.AsSpan(startI, i - startI).TrimStart('0');
                    var numY = y.AsSpan(startJ, j - startJ).TrimStart('0');

                    if (numX.Length != numY.Length)
                        return numX.Length.CompareTo(numY.Length);

                    int numCmp = numX.CompareTo(numY, StringComparison.Ordinal);
                    if (numCmp != 0) return numCmp;
                }
                else
                {
                    int charCmp = char.ToLowerInvariant(x[i]).CompareTo(char.ToLowerInvariant(y[j]));
                    if (charCmp != 0) return charCmp;
                    i++; 
                    j++;
                }
            }

            return (x.Length - i).CompareTo(y.Length - j);
        }
    }
}