using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyCodeGen
{
    public abstract class CodeGeneratorBase<T>
    {
		public abstract string Generate(T dafnyObject);
    }
}
