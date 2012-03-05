using System;
using System.Web.UI;

namespace SharpBrake.WebFormsApp
{
    public partial class Default : Page
    {
        protected Default()
        {
            Load += PageLoad;
        }


        private static void PageLoad(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Sample error");
        }
    }
}