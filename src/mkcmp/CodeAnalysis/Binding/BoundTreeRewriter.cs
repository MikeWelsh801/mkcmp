namespace Mkcmp.CodeAnalysis.Binding;

internal abstract class BoundTreeRewriter
{
    public virtual BoundStatement RewriteStatement(BoundStatement s)
    {
        throw new NotImplementedException();
    }

    public virtual BoundExpression RewriteExpression(BoundExpression e)
    {
        throw new NotImplementedException();
    }
}

