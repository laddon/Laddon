using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System.ComponentModel.Composition;
//using DafnyServiceProvider;
using Microsoft.Dafny;
using EnvDTE;

namespace MTA.Laddon
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidLaddonPkgString)]
	[ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionExists)]
	public sealed class LaddonPackage : Package
	{
		//private AstExposer m_dafnyExposer = null;

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public LaddonPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (mcs != null)
			{
				CommandID laddonMenuCommandID = new CommandID(GuidList.guidLaddonCmdSet, (int)PkgCmdIDList.cmdidRenameVariable);

				// CURRENTLY DISABLED, AS IS NOT PROPERLY IMPLEMENTED
				// Create the command for the Rename Variable item.
				//OleMenuCommand laddonRenameVarItem = new OleMenuCommand(LaddonRenameVariableCallback, laddonMenuCommandID);
				//laddonRenameVarItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
				//mcs.AddCommand(laddonRenameVarItem)

				// Create the command for the Extract Method item.
				laddonMenuCommandID = new CommandID(GuidList.guidLaddonCmdSet, (int)PkgCmdIDList.cmdidExtractMethod);
				OleMenuCommand laddonExtractMethodItem = new OleMenuCommand(LaddonExtractMethodCallback, laddonMenuCommandID);
				laddonExtractMethodItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
				mcs.AddCommand(laddonExtractMethodItem);

				laddonMenuCommandID = new CommandID(GuidList.guidLaddonCmdSet, (int)PkgCmdIDList.cmdidExtractLemma);
				OleMenuCommand laddonExtractLemmaItem = new OleMenuCommand(LaddonExtractLemmaCallback, laddonMenuCommandID);
				laddonExtractLemmaItem.BeforeQueryStatus += menuItem_BeforeQueryStatus;
				mcs.AddCommand(laddonExtractLemmaItem);
			}

			// *********** Yoni 08.05.2015 - This will not be used for now. ***********
			// ********* The program code will be reparsed by Laddon instead **********
			//// Initialize Dafny service and exposer.
			//IDafnyService service = GetService(typeof(SDafnyService)) as IDafnyService;
			//if (null == service)
			//{
			//	// If the service is not available we can exit now.
			//	Trace.WriteLine("Can not get the global service.");
			//	return;
			//}

			//m_dafnyExposer = new AstExposer(service);
		}

		private void menuItem_BeforeQueryStatus(object sender, EventArgs e)
		{
			// get the menu that fired the event
			var menuCommand = sender as OleMenuCommand;
			if (menuCommand != null)
			{
				IWpfTextView view = GetActiveTextView();
				bool isDafnyFile = IsDafnyFile(view);

				menuCommand.Visible = isDafnyFile;
				menuCommand.Enabled = isDafnyFile;
			}
		}

		private bool IsDafnyFile(IWpfTextView view)
		{
			if (view == null)
			{
				return false;
			}

			ITextDocument doc;
			var isDoc = view.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out doc);
			if (isDoc)
			{
				return doc.FilePath.EndsWith(".dfy");
			}
			return false;
		}

	
		private void LaddonRenameVariableCallback(object sender, EventArgs e)
		{

		}

		private void LaddonExtractMethodCallback(object sender, EventArgs e)
		{
			SaveActiveFile();

			var window = new ExtractMethodWindow(GetActiveTextView());
			if (window.IsValid)
			{
				window.ShowDialog();
			}
		}

		private void LaddonExtractLemmaCallback(object sender, EventArgs e)
		{
			SaveActiveFile();
			var window = new ExtractLemmaWindow(GetActiveTextView());
			if (window.IsValid)
			{
				window.ShowDialog();
				//SaveActiveFile();
			}
		}

		// get the active WpfTextView, if there is one.
		private IWpfTextView GetActiveTextView()
		{
			IWpfTextView view = null;
			IVsTextView vTextView = null;

			IVsTextManager txtMgr =
				(IVsTextManager)GetService(typeof(SVsTextManager));
			int mustHaveFocus = 1;
			txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);

			IVsUserData userData = vTextView as IVsUserData;
			if (null != userData)
			{
				IWpfTextViewHost viewHost;
				object holder;
				Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
				userData.GetData(ref guidViewHost, out holder);
				viewHost = (IWpfTextViewHost)holder;
				view = viewHost.TextView;
			}

			return view;
		}

		private bool SaveActiveFile()
		{
			DTE saveManager = (DTE)GetService(typeof(DTE));
			if (!saveManager.ActiveDocument.Saved)
			{
				try
				{
					saveManager.ActiveDocument.Save(saveManager.ActiveDocument.FullName); // Overwrite existing code file.
				}
				catch (Exception ex)
				{
					// Save operation failed... Is the file Read-Only?
					Console.WriteLine("Error while saving file: " + ex.Message);
					return false;
				}
			}

			return true;
		}
	}
}
