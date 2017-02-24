//-----------------------------------------------------------------------
// <copyright file="CslaNewModelBinderTest.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: http://www.lhotka.net/cslanet/
// </copyright>
// <summary>no summary</summary>
//-----------------------------------------------------------------------
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Globalization;

namespace Csla.Web.Mvc.Test.ModelBinderTest
{
    [TestClass]
    public class CslaModelBinderTest
    {
        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            ModelBinders.Binders.DefaultBinder = new CslaNewModelBinder();
        }

        [TestMethod]
        public void CanBind_To_SingleRoot()
        {
            var model = SingleRoot.GetSingleRoot(1);
            var values = new NameValueCollection();
            values.Add("name", "Gimli");
            values.Add("dob", "1/1/2000");

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(SingleRoot)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retModel = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retModel, "Return should be the same as original");
            Assert.AreEqual("Gimli", model.Name);
            Assert.AreEqual(new SmartDate(new DateTime(2000, 1, 1)), model.DOB);
        }

        [TestMethod]
        public void CanBind_WithPrefix_To_SingleRoot()
        {
            var model = SingleRoot.GetSingleRoot(2);
            var values = new NameValueCollection();
            values.Add("pfx.name", "Gimli");

            var bindingContext = new ModelBindingContext
            {
                ModelName = "pfx",
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(SingleRoot)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retModel = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retModel, "Return should be the same as original");
            Assert.AreEqual("Gimli", model.Name);
        }

        [TestMethod]
        public void Bind_InvalidValues_ModelState_Returns_Errors()
        {
            var model = RootWithValidation.NewRootWithValidation();
            var values = new NameValueCollection();
            values.Add("Max5Chars", "More Than 5 Characters");
            values.Add("Between2And10", "100");
            values.Add("AlwaysInvalid", "bla..");

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootWithValidation)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            binder.BindModel(new ControllerContext(), bindingContext);

            Assert.IsFalse(model.IsValid, "Object should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValid, "Model State should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValidField("Max5Chars"), "Max5Chars property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["Max5Chars"].Errors.Count);
            Assert.IsFalse(bindingContext.ModelState.IsValidField("Between2And10"), "Between2And10 property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["Between2And10"].Errors.Count);
            Assert.IsFalse(bindingContext.ModelState.IsValidField("AlwaysInvalid"), "AlwaysInvalid property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["AlwaysInvalid"].Errors.Count);
        }

        [TestMethod]
        public void Bind_InvalidValues_WithPrefix_ModelState_Returns_Errors()
        {
            var model = RootWithValidation.NewRootWithValidation();
            var values = new NameValueCollection();
            values.Add("pfx.Max5Chars", "More Than 5 Characters");
            values.Add("pfx.Between2And10", "100");
            values.Add("pfx.AlwaysInvalid", "bla..");

            var bindingContext = new ModelBindingContext
            {
                ModelName = "pfx",
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootWithValidation)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            binder.BindModel(new ControllerContext(), bindingContext);

            Assert.IsFalse(model.IsValid, "Object should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValid, "Model State should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValidField("pfx.Max5Chars"), "Max5Chars property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["pfx.Max5Chars"].Errors.Count);
            Assert.IsFalse(bindingContext.ModelState.IsValidField("pfx.Between2And10"), "Between2And10 property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["pfx.Between2And10"].Errors.Count);
            Assert.IsFalse(bindingContext.ModelState.IsValidField("pfx.AlwaysInvalid"), "AlwaysInvalid property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["pfx.AlwaysInvalid"].Errors.Count);
        }

        [TestMethod]
        public void CanBind_To_RootList()
        {
            var model = RootList.Get(3);
            var values = new NameValueCollection();
            values.Add("[0].Id", "1");
            values.Add("[0].ChildName", "Alkid");
            values.Add("[0].ChildAge", "37");
            values.Add("[1].Id", "2");
            values.Add("[1].ChildName", "Ditmar");
            values.Add("[1].ChildAge", "35");
            values.Add("[2].Id", "2");
            values.Add("[2].ChildName", "Julian");
            values.Add("[2].ChildAge", "45");
            // add new child to root list
            values.Add("[3].Id", "6");
            values.Add("[3].ChildName", "Comodorus");
            values.Add("[3].ChildAge", "100");

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootList)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retModel = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retModel, "Return should be the same as original");
            Assert.AreEqual(6, model[3].Id);
            Assert.AreEqual("Comodorus", model[3].ChildName);
            Assert.AreEqual(100, model[3].ChildAge);

        }

        [TestMethod]
        public void CanBind_WithPrefix_To_RootList()
        {
            var model = RootList.Get(2);
            var values = new NameValueCollection();
            values.Add("pfx[0].AnyString", "Any-0");
            values.Add("pfx[0].Max5Chars", "Max5-0");
            values.Add("pfx[1].AnyString", "Any-1");
            values.Add("pfx[1].Max5Chars", "Max5-1");

            var bindingContext = new ModelBindingContext
            {
                ModelName = "pfx",
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootList)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retModel = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retModel, "Return should be the same as original");
            //Assert.AreEqual("Any-0", model[0].AnyString);
            //Assert.AreEqual("Max5-0", model[0].Max5Chars);
            //Assert.AreEqual("Any-1", model[1].AnyString);
            //Assert.AreEqual("Max5-1", model[1].Max5Chars);
        }

        [TestMethod]
        public void Bind_RootList_WithInvalidValues_ModelState_ReturnsErrors()
        {

            var model = RootList.Get(2);
            var values = new NameValueCollection();
            values.Add("[0].Id", "1");
            values.Add("[0].ChildName", "Alkid");
            values.Add("[0].ChildAge", "37");
            values.Add("[0].grandchildren[0].Id", "1");
            values.Add("[0].grandchildren[0].GrandChildrenName", "Valter");
            values.Add("[0].grandchildren[0].GrandChildrenAge", "2");
            values.Add("[1].Id", "2");
            values.Add("[1].ChildName", "Ditmar Ditmar longer than 20 characters");
            values.Add("[1].ChildAge", "35");


            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootList)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            binder.BindModel(new ControllerContext(), bindingContext);

            Assert.IsFalse(model.IsValid, "Object should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValid, "Model State should be invalid");
            Assert.IsFalse(bindingContext.ModelState.IsValidField("[1].ChildName"), "ChildAge property should be invalid");
            Assert.AreEqual(1, bindingContext.ModelState["[1].ChildName"].Errors.Count);
        }

        [TestMethod]
        public void CanBind_To_RootWithChildren()
        {
            var model = RootWithChildren.Get(1);
            var values = new NameValueCollection();
            values.Add("id", "1");
            values.Add("name", "Vullnet");
            values.Add("children[0].Id", "1");
            values.Add("children[0].ChildName", "Alkid");
            // modify the age for an existing child
            values.Add("children[0].ChildAge", "38");
            //modify the name of the existing grandchildren
            values.Add("children[0].grandchildren[0].Id", "1");
            values.Add("children[0].grandchildren[0].GrandChildrenName", "Valterush");
            values.Add("children[0].grandchildren[0].GrandChildrenAge", "2");
            //add new grandchildren
            values.Add("children[0].grandchildren[1].Id", "5");
            values.Add("children[0].grandchildren[1].GrandChildrenName", "Anna");
            values.Add("children[0].grandchildren[1].GandChildrenAge", "1");
            values.Add("children[1].Id", "2");
            values.Add("children[1].ChildName", "Ditmar");
            values.Add("children[1].ChildAge", "35");
            //add new child
            values.Add("children[2].Id", "6");
            values.Add("children[2].ChildName", "Katerina");
            values.Add("children[2].ChildAge", "29");
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootWithChildren)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retObject = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retObject, "Return should be the same as original");
            var retModel = (RootWithChildren)retObject;
            Assert.AreEqual("Vullnet", retModel.Name);
            Assert.AreEqual("Katerina", retModel.Children[2].ChildName);
            Assert.AreEqual(29, retModel.Children[2].ChildAge);
            Assert.AreEqual(6, retModel.Children[2].Id);
            Assert.AreEqual("Anna", retModel.Children[0].GrandChildren[1].GrandChildrenName);
            Assert.AreEqual(1, retModel.Children[0].GrandChildren[1].GandChildrenAge);
            Assert.AreEqual(5, retModel.Children[0].GrandChildren[1].Id);
            Assert.AreEqual(model, retModel);
            //Assert.AreEqual("Max5-0", model.Children[0].Max5Chars);
            //Assert.AreEqual("Any-1", model.Children[1].AnyString);
            //Assert.AreEqual("Max5-1", model.Children[1].Max5Chars);
        }

        [TestMethod]
        public void CanBind_WithPrefix_To_RootWithChildren()
        {
            var model = RootWithChildren.Get(2);
            var values = new NameValueCollection();
            values.Add("pfx.id", "20");
            values.Add("pfx.name", "Gimli");
            //values.Add("pfx.children[0].AnyString", "Any-0");
            //values.Add("pfx.children[0].Max5Chars", "Max5-0");
            //values.Add("pfx.children[1].AnyString", "Any-1");
            //values.Add("pfx.children[1].Max5Chars", "Max5-1");

            var bindingContext = new ModelBindingContext
            {
                ModelName = "pfx",
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(RootWithChildren)),
                ValueProvider = new NameValueCollectionValueProvider(values, CultureInfo.CurrentCulture)
            };
            var binder = new CslaNewModelBinder();
            object retModel = binder.BindModel(new ControllerContext(), bindingContext);

            Assert.AreSame(model, retModel, "Return should be the same as original");
            Assert.AreEqual("Gimli", model.Name);
            //Assert.AreEqual("Any-0", model.Children[0].AnyString);
            //Assert.AreEqual("Max5-0", model.Children[0].Max5Chars);
            //Assert.AreEqual("Any-1", model.Children[1].AnyString);
            //Assert.AreEqual("Max5-1", model.Children[1].Max5Chars);
        }
    }
}