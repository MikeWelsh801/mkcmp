using Mkcmp.CodeAnalysis.Symbols;

namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class Conversion
{
    public static readonly Conversion None = new(exists: false, isIdentity: false, isImplicit: false);
    public static readonly Conversion Identity = new(exists: true, isIdentity: true, isImplicit: true);
    public static readonly Conversion Explicit = new(exists: true, isIdentity: false, isImplicit: false);
    public static readonly Conversion Implicit = new(exists: true, isIdentity: false, isImplicit: true);

    private Conversion(bool exists, bool isIdentity, bool isImplicit)
    {
        Exists = exists;
        IsIdentity = isIdentity;
        IsImplicit = isImplicit;
    }

    public bool Exists { get; }
    public bool IsIdentity { get; }
    public bool IsImplicit { get; }
    public bool IsExplicit => Exists && !IsImplicit;

    public static Conversion Classify(TypeSymbol from, TypeSymbol to)
    {
        if (from == to)
            return Conversion.Identity;

        if (from == TypeSymbol.Bool || from == TypeSymbol.Int)
        {
            if (to == TypeSymbol.String)
                return Conversion.Explicit;
        }

        return Conversion.None;
    }
}

