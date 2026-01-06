using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.States
{
    public interface IResizableState
    {
        void OnResize(int newWidth, int newHeight);
    }
}
