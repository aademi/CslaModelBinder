//-----------------------------------------------------------------------
// <copyright file="RootWithChildren.cs" company="Marimer LLC">
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
    [Serializable]
  public class RootWithChildren:Csla.BusinessBase<RootWithChildren>
  {
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(p => p.ID);
    public int ID
    {
      get { return GetProperty<int>(IdProperty); }
      private set { SetProperty<int>(IdProperty, value); }
    }
    [StringLength(20)]
    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(p => p.Name);
    public string Name
    {
      get { return GetProperty<string>(NameProperty); }
      set { SetProperty<string>(NameProperty, value); }
    }


        public static readonly PropertyInfo<int> AgeProperty = RegisterProperty<int>(c => c.Age);
        public int Age
        {
            get { return GetProperty(AgeProperty); }
            set { SetProperty(AgeProperty, value); }
        }
        public static readonly PropertyInfo<ChildList> ChildrenProperty = RegisterProperty<ChildList>(c => c.Children, RelationshipTypes.Child | RelationshipTypes.LazyLoad);
        public ChildList Children
        {
            get
            {
                if (!FieldManager.FieldExists(ChildrenProperty))
                {
                    Children = ChildList.Get(ID);
                }
                return GetProperty(ChildrenProperty);
            }
            private set
            {
                LoadProperty(ChildrenProperty, value);
                OnPropertyChanged(ChildrenProperty);
            }
        }

    public static RootWithChildren Get(int childCount)
    {
      return DataPortal.Fetch<RootWithChildren>(new SingleCriteria<RootList, int>(childCount));
    }
    private void DataPortal_Fetch(SingleCriteria<RootList, int> criteria)
    {
      using (BypassPropertyChecks)
      {
                ID = criteria.Value;
                if (criteria.Value==1)
                {
                    Name = "Vullnet";
                    Age = 73;
                }
                if (criteria.Value==2)
                {
                    Name = "Grigor";
                    Age = 79;
                }
                LoadProperty(ChildrenProperty, ChildList.Get(ID));
            }
 
    }

  }
}