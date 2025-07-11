using IConfiguration = EdNexusData.Broker.Common.Configuration.IConfiguration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using EdNexusData.Broker.Common.Lookup;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Core.Lookup;

namespace EdNexusData.Broker.Web.Helpers;

public class ModelFormBuilderHelper
{
    private readonly MappingLookupService mappingLookupService;

    public ModelFormBuilderHelper(MappingLookupService mappingLookupService)
    {
        this.mappingLookupService = mappingLookupService;
    }

    public async Task<string> HtmlForModel(IConfiguration model, string assemblyName)
    {
        var formHTML = $"""
<form method="post" action="/Settings/Configuration/{assemblyName}">
<input type="hidden" name="ConnectorConfigurationType" value="{model.GetType().FullName}">
<input type="hidden" name="ConnectorConfigurationTypeAssembly" value="{model.GetType().AssemblyQualifiedName}">
"""; // start form html output

        // Get type of incoming model
        var modelType = model.GetType();
        // Get the properties of the model
        var modelTypeProps = modelType.GetProperties();
        // For each, determine input html needed
        foreach (var modelTypeProp in modelTypeProps)
        {
            // include existing value if set in model

            // Write HTML
            var modelTypePropAttrsDataType = (DataTypeAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DataTypeAttribute)).FirstOrDefault()!;
            var modelTypePropAttrsDisplayName = (DisplayNameAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
            var modelTypePropAttrsDescription = (DescriptionAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DescriptionAttribute)).FirstOrDefault()!;
            var modelTypePropAttrsLookup = (LookupAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(LookupAttribute)).FirstOrDefault()!;

            var displayNameToUse = modelTypeProp.Name;
            if (modelTypePropAttrsDisplayName is not null)
            {
                displayNameToUse = modelTypePropAttrsDisplayName.DisplayName;
            }

            if (modelTypePropAttrsDataType is not null && modelTypePropAttrsDataType.DataType == DataType.Url)
            {
                formHTML += $"""
                <div class="sm:col-span-4 my-4">
              <label for="{modelTypeProp.Name}" class="block text-sm font-medium leading-6 text-gray-900">{displayNameToUse}</label>
              <div class="mt-2">
                <div class="flex rounded-md shadow-sm ring-1 ring-inset ring-gray-300 focus-within:ring-2 focus-within:ring-inset focus-within:ring-tertiary-700 sm:max-w-md">
                  <input type="text" autocomplete="off" name="{modelTypeProp.Name}" id="{modelTypeProp.Name}" value="{modelTypeProp.GetValue(model)}" class="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-tertiary-700 sm:text-sm sm:leading-6">
                </div>
                {modelTypePropAttrsDescription?.Description}
              </div>
              </div>
              """;
            }

            if (modelTypePropAttrsDataType is not null && modelTypePropAttrsDataType.DataType == DataType.Text)
            {
                formHTML += $"""
                <div class="sm:col-span-4 my-4">
              <label for="{modelTypeProp.Name}" class="block text-sm font-medium leading-6 text-gray-900">{displayNameToUse}</label>
              <div class="mt-2">
                <div class="flex rounded-md shadow-sm ring-1 ring-inset ring-gray-300 focus-within:ring-2 focus-within:ring-inset focus-within:ring-tertiary-700 sm:max-w-md">
                  <input type="text" autocomplete="off" name="{modelTypeProp.Name}" id="{modelTypeProp.Name}" value="{modelTypeProp.GetValue(model)}" class="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-tertiary-700 sm:text-sm sm:leading-6">
                </div>
                {modelTypePropAttrsDescription?.Description}
              </div>
              </div>
              """;
            }

            if (modelTypePropAttrsDataType is not null && modelTypePropAttrsDataType.DataType == DataType.Password)
            {
                var passwordPlaceholder = (!string.IsNullOrEmpty(modelTypeProp.GetValue(model)?.ToString())) ? "Password is set" : "";
                var passwordValue = (!string.IsNullOrEmpty(modelTypeProp.GetValue(model)?.ToString())) ? "ValueSet" : "";
                formHTML += $"""
                <div class="sm:col-span-4 my-4">
              <label for="{modelTypeProp.Name}" class="block text-sm font-medium leading-6 text-gray-900">{displayNameToUse}</label>
              <div class="mt-2">
                <div class="flex rounded-md shadow-sm ring-1 ring-inset ring-gray-300 focus-within:ring-2 focus-within:ring-inset focus-within:ring-tertiary-700 sm:max-w-md">
                  <input type="password" autocomplete="off" name="{modelTypeProp.Name}" id="{modelTypeProp.Name}" value="{passwordValue}" placeholder="{passwordPlaceholder}" class="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-tertiary-700 sm:text-sm sm:leading-6">
                </div>
                {modelTypePropAttrsDescription?.Description}
              </div>
              </div>
              """;
            }

            if (modelTypePropAttrsLookup is not null)
            {
                // Get the lookup type
                var lookupType = modelTypePropAttrsLookup.LookupType;

                // Get the lookup items
                var lookupItems = await mappingLookupService.SelectAsync(modelTypePropAttrsLookup, modelTypeProp.GetValue(model)?.ToString());
                formHTML += $"""
               <div class="sm:col-span-4 my-4">
              <label for="{modelTypeProp.Name}" class="block text-sm font-medium leading-6 text-gray-900">{displayNameToUse}</label>
              <div class="mt-2">
                <select name="{modelTypeProp.Name}" id="{modelTypeProp.Name}" class="block w-fit rounded-md border-0 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 focus:ring-2 focus:ring-inset focus:ring-tertiary-700 text-sm">
                  <option value="">Select {displayNameToUse}</option>
            """;
                foreach (var item in lookupItems)
                {
                    formHTML += $"""
                    <option value="{item.Value}" {(item.Selected ? "selected" : "")}>{item.Text}</option>
                    """;
                }
                formHTML += $"""
                </select>
                {modelTypePropAttrsDescription?.Description}
              </div>
              </div>
              """;
            }
        }
        formHTML += """
<div class="mt-6 flex items-center justify-end gap-x-6">
    <button type="button" class="text-sm font-semibold leading-6 text-gray-900">Cancel</button>
    <button type="submit" class="rounded-md bg-tertiary-700 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-tertiary-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-tertiary-700">Save</button>
</div>
</form>
""";
        return formHTML; // output form html
    }
}
