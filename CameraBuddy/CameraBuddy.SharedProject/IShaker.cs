using System;
using System.Collections.Generic;
using System.Text;

namespace CameraBuddy
{
    public interface IShaker
    {
        void AddShake(float length, float delta, float amount);
    }
}
