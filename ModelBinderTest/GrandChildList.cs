using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csla.Web.Mvc.Test.ModelBinderTest
{
    [Serializable]
    public class GrandChildList : Csla.BusinessListBase<GrandChildList, GrandChild>
    {
        public GrandChildList()
        { }



        public static GrandChildList Get(int ChildId)
        {
            return DataPortal.Fetch<GrandChildList>(ChildId);
        }
        private void DataPortal_Fetch(int ChildId)
        {
            int i;
            if (ChildId == 1)
            {
                i = 1;
                this.Add(DataPortal.FetchChild<GrandChild>(i));

            }

            if (ChildId == 5)
            {
                i = 2;
                this.Add(DataPortal.FetchChild<GrandChild>(i));
                i = 3;
                this.Add(DataPortal.FetchChild<GrandChild>(i));


            }

        }
    }
}
