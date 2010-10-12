using System;
using System.Web.UI;

namespace SampleWeb
{
    public partial class Default : Page
    {
        public Default()
        {
            Load += PageLoad;
        }

        protected void PageLoad(object sender, EventArgs e)
        {
            throw new InvalidOperationException("Sample error");
        }
    }
}