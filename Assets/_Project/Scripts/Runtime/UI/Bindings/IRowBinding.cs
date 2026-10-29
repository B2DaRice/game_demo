namespace AstroIdle.UI.Bindings
{
    /// <summary>
    /// What a <see cref="RowView"/> displays and does. Properties are re-read on every refresh, so
    /// implementations should compute live values (costs, levels) rather than caching them.
    /// To show a new kind of row, implement this interface — RowView itself never changes.
    /// </summary>
    public interface IRowBinding
    {
        string Title { get; }
        string Detail { get; }
        bool HasButton { get; }
        string ButtonLabel { get; }
        bool ButtonEnabled { get; }
        RowStyle Style { get; }

        void OnClick();
    }
}
