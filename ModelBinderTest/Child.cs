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
using Csla;

namespace Csla.Web.Mvc.Test.ModelBinderTest
{
  [Serializable()]
  public class Child : Csla.BusinessBase<Child>
  {

        public Child()
        {

        }

        public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(c => c.Id);
        public int Id
        {
            get { return GetProperty(IdProperty); }
            set { SetProperty(IdProperty, value); }
        }


        public static readonly PropertyInfo<string> ChildNameProperty = RegisterProperty<string>(c => c.ChildName);
        [StringLength(20)]
        public string ChildName
        {
            get { return GetProperty(ChildNameProperty); }
            set { SetProperty(ChildNameProperty, value); }
        }


        public static readonly PropertyInfo<int> ChildAgeProperty = RegisterProperty<int>(c => c.ChildAge);
        public int ChildAge
        {
            get { return GetProperty(ChildAgeProperty); }
            set { SetProperty(ChildAgeProperty, value); }
        }


        public static readonly PropertyInfo<GrandChildList> GrandChildrenProperty = RegisterProperty<GrandChildList>(c => c.GrandChildren, RelationshipTypes.Child);
        public GrandChildList GrandChildren
        {
            get { return GetProperty(GrandChildrenProperty); }
            private set { SetProperty(GrandChildrenProperty, value); }
        }
    private void Child_Fetch(int idx)
    {
      using (BypassPropertyChecks)
      {
                Id = idx;
                if (idx == 1)
                {
                    ChildName = "Alkid";
                    ChildAge = 37;
                }
                Id = idx;
                if (idx == 2)
                {
                    ChildName = "Ditmar";
                    ChildAge = 35;
                }
                Id = idx;
                if (idx == 3)
                {
                    ChildName = "Julian";
                    ChildAge = 45;
                }
                Id = idx;
                if (idx == 4)
                {
                    ChildName = "Isuela";
                    ChildAge = 40;
                }
                if (idx == 5)
                {
                    ChildName = "Gjergji";
                    ChildAge = 37;
                }

                LoadProperty(GrandChildrenProperty, GrandChildList.Get(Id));
            }
        }

  }
}