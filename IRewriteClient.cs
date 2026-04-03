namespace AIHotkey;

internal interface IRewriteClient
{
    Task<string> RewriteAsync(string selectedText, CancellationToken cancellationToken);

    Task<string> TestConnectionAsync(CancellationToken cancellationToken);
}
