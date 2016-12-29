using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MTA.Laddon
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        private IWpfTextView view;

        public RenameWindow(IWpfTextView view)
        {
            this.view = view;

            Loaded += RenameWindow_Loaded;

            InitializeComponent();
        }

        private void RenameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string selectedText = view.Selection.SelectedSpans[0].GetText();
            txtOldName.Text = selectedText;
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            // TODO: validate
            var oldText = txtOldName.Text;
            var newText = txtNewName.Text;

            IVariableFinder finder = new DummyVariableFinder();
            var spans = finder.GetVariableLocations(view, oldText);

            ITextEdit edit = view.TextBuffer.CreateEdit();
            foreach (var span in spans)
            {
                edit.Replace(span, newText);
            }
            edit.Apply();

            this.Close();
        }
    }
}
