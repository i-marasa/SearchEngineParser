using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


    class ControlInviker
    {
        delegate void UniversalVoidDelegate();

        /// <summary>
        /// Call form controll action from different thread
        /// </summary>
        public static void ControlInvike(Control control, Action function)
        {
            if (control.IsDisposed || control.Disposing)
                return;

            if (control.InvokeRequired)
            {
                control.Invoke(new UniversalVoidDelegate(() => ControlInvike(control, function)));
                return;
            }
            function();
        }
    }

