using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entitys
{
    public interface ISaveable
    {
        byte[] OnSave();
        void OnLoad(byte[] bytes);
    }
}
