using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programming_Language
{
    class EnumeratorReader<EnumeratorType>
    {
        EnumeratorType enumerator;
        public int index { get; private set; }
        public EnumeratorReader (EnumeratorType enumerator)
        {
            this.enumerator = enumerator;
        }
        public void MoveNext ()
        {
            index++;
        }
        public object Current
        {
            get
            {
                return ((IList<object>)enumerator)[index];
            }
            set
            {
                ((IList<object>)enumerator)[index] = value;
            }
        }
        public void Reset ()
        {

        }
    }
}
