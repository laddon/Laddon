using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTA.Laddon
{
	/// <summary>
	/// Listener class to get notified about text editor instance creations
	/// </summary>
	internal sealed class TextViewCreationListener : IWpfTextViewCreationListener
	{
		[ContentType("code")]
		[Export(typeof(IWpfTextViewCreationListener))]
		[TextViewRole(PredefinedTextViewRoles.Editable)]
		public void TextViewCreated(IWpfTextView textView)
		{
			new Formatter(textView);
		}
	}
}
