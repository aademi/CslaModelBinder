//-----------------------------------------------------------------------
// <copyright file="Child.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: http://www.lhotka.net/cslanet/
// </copyright>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Csla.Web.Mvc.Test.ModelBinderTest
{
    [Serializable()]
    public class GrandChild : BusinessBase<GrandChild>
    {
        public GrandChild()
        {

        }

        public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(c => c.Id);
        public int Id
        {
            get { return GetProperty(IdProperty); }
            set { SetProperty(IdProperty, value); }
        }
        public static readonly PropertyInfo<string> GrandChildrenNameProperty = RegisterProperty<string>(c => c.GrandChildrenName);
        public string GrandChildrenName
        {
            get { return GetProperty(GrandChildrenNameProperty); }
            set { SetProperty(GrandChildrenNameProperty, value); }
        }

        public static readonly PropertyInfo<int> GandChildrenAgeProperty = RegisterProperty<int>(c => c.GandChildrenAge);
        public int GandChildrenAge
        {
            get { return GetProperty(GandChildrenAgeProperty); }
            set { SetProperty(GandChildrenAgeProperty, value); }
        }

        private void Child_Fetch(int idx)
        {
            using (BypassPropertyChecks)
            {
                Id = idx;
                if (idx == 1)
                {
                    GrandChildrenName = "Valter";
                    GandChildrenAge = 2;
                }
                Id = idx;
                if (idx == 2)
                {
                    GrandChildrenName = "Joan";
                    GandChildrenAge = 5;
                }
                Id = idx;
                if (idx == 3)
                {
                    GrandChildrenName = "Sofi";
                    GandChildrenAge = 2;
                }

            }
        }
    }
}
