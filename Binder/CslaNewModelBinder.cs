//-----------------------------------------------------------------------
// <copyright file="CslaModelBinder.cs" company="Marimer LLC">
//     Copyright (c) Marimer LLC. All rights reserved.
//     Website: http://www.lhotka.net/cslanet/
// </copyright>
// <summary>Model binder for use with CSLA .NET editable business objects.</summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Csla.Web.Mvc
{
    /// <summary>
    /// Model binder for use with CSLA .NET editable business
    /// objects.
    /// </summary>
    public class CslaNewModelBinder : DefaultModelBinder
    {
        private class ObjectManager : Csla.Server.ObjectFactory
        {
            public new void MarkAsChild(object obj)
            {
                base.MarkAsChild(obj);
            }

        }
        private bool _checkRulesOnModelUpdated;

        /// <summary>
        /// Creates an instance of the model binder.
        /// </summary>
        /// <param name="CheckRulesOnModelUpdated">Value indicating if business rules will be checked after the model is updated.</param>
        public CslaNewModelBinder(bool CheckRulesOnModelUpdated = true)
        {
            _checkRulesOnModelUpdated = CheckRulesOnModelUpdated;
        }

        /// <summary>
        /// Binds the model by using the specified controller context and binding context.
        /// </summary>
        /// <param name="controllerContext">Controller Context</param>
        /// <param name="bindingContext">Binding Context</param>
        /// <returns>Bound object</returns>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (typeof(Csla.Core.IEditableCollection).IsAssignableFrom((bindingContext.ModelType)))
                return BindCslaCollection(controllerContext, bindingContext);

            var suppress = bindingContext.Model as Csla.Core.ICheckRules;
            if (suppress != null)
                suppress.SuppressRuleChecking();
            var result = base.BindModel(controllerContext, bindingContext);
            return result;
        }

        /// <summary>
        /// Bind CSLA Collection object using specified controller context and binding context
        /// </summary>
        /// <param name="controllerContext">Controller Context</param>
        /// <param name="bindingContext">Binding Context</param>
        /// <returns>Bound CSLA collection object</returns>
        private object BindCslaCollection(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.Model == null)
                bindingContext.ModelMetadata.Model = CreateModel(controllerContext, bindingContext, bindingContext.ModelType);
            ObjectManager objectManager = new ObjectManager();
            var collection = (IList)bindingContext.Model;
            for (int currIdx = 0; currIdx < collection.Count; currIdx++)
            {
                string subIndexKey = CreateSubIndexName(bindingContext.ModelName, currIdx);
                if (!bindingContext.ValueProvider.ContainsPrefix(subIndexKey))
                {
                    collection.RemoveAt(currIdx); // no data submitted for this child, delete from the collection
                    continue;
                }

                var elementModel = collection[currIdx];
                var suppress = elementModel as Csla.Core.ICheckRules;
                if (suppress != null)
                    suppress.SuppressRuleChecking();
                var elementContext = new ModelBindingContext()
                {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => elementModel, elementModel.GetType()),
                    ModelName = subIndexKey,
                    ModelState = bindingContext.ModelState,
                    PropertyFilter = bindingContext.PropertyFilter,
                    ValueProvider = bindingContext.ValueProvider
                };

                if (OnModelUpdating(controllerContext, elementContext))
                {
                    //update element's properties
                    foreach (PropertyDescriptor property in GetFilteredModelProperties(controllerContext, elementContext))
                    {
                        BindProperty(controllerContext, elementContext, property);
                    }
                    OnModelUpdated(controllerContext, elementContext);
                }
            }

            // process the values that should be added to the collection

            bool stopOnIndexNotFound;
            IEnumerable<string> indexes;
            GetIndexes(bindingContext, out stopOnIndexNotFound, out indexes);

            foreach (string currentIndex in indexes)
            {
                if (collection.Count  <= Convert.ToInt32(currentIndex))
                {
                    string subIndexKey = CreateSubIndexName(bindingContext.ModelName, currentIndex);
                    if (!bindingContext.ValueProvider.ContainsPrefix(subIndexKey))
                    {
                        if (stopOnIndexNotFound)
                        {
                            // we ran out of elements to pull
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    var collectionItemType = collection.AsQueryable().ElementType;

                    var elementModel = Activator.CreateInstance(collectionItemType, true);
                    objectManager.MarkAsChild(elementModel);
                    var suppress = elementModel as Csla.Core.ICheckRules;
                    if (suppress != null)
                        suppress.SuppressRuleChecking();
                    var elementContext = new ModelBindingContext()
                    {
                        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => elementModel, elementModel.GetType()),
                        ModelName = subIndexKey,
                        ModelState = bindingContext.ModelState,
                        PropertyFilter = bindingContext.PropertyFilter,
                        ValueProvider = bindingContext.ValueProvider
                    };

                    if (OnModelUpdating(controllerContext, elementContext))
                    {
                        //update element's properties
                        foreach (PropertyDescriptor property in GetFilteredModelProperties(controllerContext, elementContext))
                        {
                            BindProperty(controllerContext, elementContext, property);
                        }
                        OnModelUpdated(controllerContext, elementContext);
                    }

                    // we need to merge model errors up
                    //AddValueRequiredMessageToModelState(controllerContext, bindingContext.ModelState, subIndexKey, elementType, thisElement);
                    collection.Add(elementModel);
                }
            }

            return bindingContext.Model;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.Mvc.ValueProviderResult.ConvertTo(System.Type)", Justification = "ValueProviderResult already handles culture conversion appropriately.")]
        private static void GetIndexes(ModelBindingContext bindingContext, out bool stopOnIndexNotFound, out IEnumerable<string> indexes)
        {
            string indexKey = CreateSubPropertyName(bindingContext.ModelName, "index");
            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(indexKey);

            if (valueProviderResult != null)
            {
                string[] indexesArray = valueProviderResult.ConvertTo(typeof(string[])) as string[];
                if (indexesArray != null)
                {
                    stopOnIndexNotFound = false;
                    indexes = indexesArray;
                    return;
                }
            }

            // just use a simple zero-based system
            stopOnIndexNotFound = true;
            indexes = GetZeroBasedIndexes();
        }
        private static IEnumerable<string> GetZeroBasedIndexes()
        {
            int i = 0;
            while (true)
            {
                yield return i.ToString(CultureInfo.InvariantCulture);
                i++;
            }
        }
        /// <summary>
        /// Creates an instance of the model if the controller implements
        /// IModelCreator.
        /// </summary>
        /// <param name="controllerContext">Controller context</param>
        /// <param name="bindingContext">Binding context</param>
        /// <param name="modelType">Type of model object</param>
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            var controller = controllerContext.Controller as IModelCreator;
            if (controller != null)
                return controller.CreateModel(modelType);
            else
                return base.CreateModel(controllerContext, bindingContext, modelType);
        }

        /// <summary>
        /// Checks the validation rules for properties
        /// after the Model has been updated.
        /// </summary>
        /// <param name="controllerContext">Controller context</param>
        /// <param name="bindingContext">Binding context</param>
        protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var obj = bindingContext.Model as Csla.Core.BusinessBase;
            if (obj != null)
            {
                if (this._checkRulesOnModelUpdated)
                {
                    var suppress = obj as Csla.Core.ICheckRules;
                    if (suppress != null)
                    {
                        suppress.ResumeRuleChecking();
                        suppress.CheckRules();
                    }
                }
                var errors = from r in obj.BrokenRulesCollection
                             where r.Severity == Csla.Rules.RuleSeverity.Error
                             select r;
                foreach (var item in errors)
                {
                    ModelState state;
                    string mskey = CreateSubPropertyName(bindingContext.ModelName, item.Property ?? string.Empty);
                    if (bindingContext.ModelState.TryGetValue(mskey, out state))
                    {
                        if (state.Errors.Where(e => e.ErrorMessage == item.Description).Any())
                            continue;
                        else
                            bindingContext.ModelState.AddModelError(mskey, item.Description);
                    }
                    else if (mskey == string.Empty)
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, item.Description);
                }
            }
            else
              if (!(bindingContext.Model is IViewModel))
                base.OnModelUpdated(controllerContext, bindingContext);
        }

        /// <summary>
        /// Prevents IDataErrorInfo validation from
        /// operating against editable objects.
        /// </summary>
        /// <param name="controllerContext">Controller context</param>
        /// <param name="bindingContext">Binding context</param>
        /// <param name="propertyDescriptor">Property descriptor</param>
        /// <param name="value">Value</param>
        protected override void OnPropertyValidated(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
        {
            if (!(bindingContext.Model is Csla.Core.BusinessBase))
                base.OnPropertyValidated(controllerContext, bindingContext, propertyDescriptor, value);
        }


    }
}
