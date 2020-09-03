using System;
using System.Collections.Generic;
using System.Text;

namespace smokeDetector
{
    public interface INotification
    {
        void CreateNotification(String title, String message);
    }
}
