using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DafnyVariable
	{
		#region Ctor

		public DafnyVariable(string name, Microsoft.Dafny.Type type, bool isGhost)
		{
			Name = name;
			Type = type;
			IsDeclaredInSelectedStatements = false;
			IsUsedAfterSelectedStatements = false;
			IsOnHeap = false;
			IsGhost = isGhost;
		}

		#endregion

		#region Public Properties

		public string Name { get; private set; }

		public Microsoft.Dafny.Type Type { get; private set; }

		public bool IsUsedAfterSelectedStatements { get; set; }

		public bool IsDeclaredInSelectedStatements { get; set; }

		public bool IsOnHeap { get; set; }

		public bool IsGhost { get; set; }

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return this.Name + ": " + this.Type.ToString();
		}

		#endregion
	}
}
