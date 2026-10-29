using System;

namespace AstroIdle.UI.Bindings
{
    /// <summary>General-purpose row wired up with delegates (used for info rows and dev tools).</summary>
    public class ActionRowBinding : IRowBinding
    {
        private readonly Func<string> title;
        private readonly Func<string> detail;
        private readonly Func<string> buttonLabel;
        private readonly Func<bool> buttonEnabled;
        private readonly Action onClick;

        /// <param name="buttonLabel">Null for a row without a button.</param>
        public ActionRowBinding(Func<string> title, Func<string> detail, Func<string> buttonLabel = null, Action onClick = null, Func<bool> buttonEnabled = null, RowStyle style = RowStyle.Normal)
        {
            this.title = title;
            this.detail = detail;
            this.buttonLabel = buttonLabel;
            this.onClick = onClick;
            this.buttonEnabled = buttonEnabled;
            Style = style;
        }

        public string Title => title();
        public string Detail => detail();
        public bool HasButton => buttonLabel != null;
        public string ButtonLabel => buttonLabel != null ? buttonLabel() : string.Empty;
        public bool ButtonEnabled => buttonEnabled == null || buttonEnabled();
        public RowStyle Style { get; }

        public void OnClick()
        {
            onClick?.Invoke();
        }
    }
}
