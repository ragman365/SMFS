using System;
using System.Data;
using System.Windows.Forms;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public class PanelScroll : System.Windows.Forms.Panel
    {
        /***********************************************************************************************/
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
        public class PanelNoScroll : System.Windows.Forms.Panel
        {
            protected override System.Drawing.Point ScrollToControl(Control activeControl)
            {
                return DisplayRectangle.Location;
            }
        }
    }
    /***********************************************************************************************/
}
