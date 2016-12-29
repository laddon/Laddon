using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTA.Laddon
{
    class Formatter
    {
        private IWpfTextView _view;
        private bool _isChangingText;

        public Formatter(IWpfTextView view)
        {
			//_view = view;
			//_view.TextBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(TextBuffer_Changed);
			//_view.TextBuffer.PostChanged += new EventHandler(TextBuffer_PostChanged);
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            _isChangingText = false;
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            if (!_isChangingText)
            {
                _isChangingText = true;
                FormatCode(e);
            }
        }

        private void FormatCode(TextContentChangedEventArgs e)
        {
            if (e.Changes != null)
            {
                for (int i = 0; i < e.Changes.Count; i++)
                {
                    HandleChange(e.Changes[0].NewText);
                }
            }
        }

        private void HandleChange(string newText)
        {
			//ITextEdit edit = _view.TextBuffer.CreateEdit();
			//edit.Insert(0, "Hello");
			//edit.Apply();
        }
    }
}
