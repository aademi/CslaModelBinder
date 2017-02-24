using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csla.Web.Mvc.Test.ModelBinderTest
{
    [Serializable]
    public class ChildList : Csla.BusinessListBase<ChildList, Child>
    {
        public ChildList()
        { }

        public static ChildList Get(int RootId)
        {
            return DataPortal.Fetch<ChildList>(RootId);
        }
        private void DataPortal_Fetch(int RootId)
        {
            int i;
            if (RootId==1)
            {
                i = 1;
                this.Add(DataPortal.FetchChild<Child>(i));
                i = 2;
                this.Add(DataPortal.FetchChild<Child>(i));

            }

            if (RootId==2)
            {
                i = 3;
                this.Add(DataPortal.FetchChild<Child>(i));
                i = 4;
                this.Add(DataPortal.FetchChild<Child>(i));
                i = 5;
                this.Add(DataPortal.FetchChild<Child>(i));

            }

        }
    }
}
