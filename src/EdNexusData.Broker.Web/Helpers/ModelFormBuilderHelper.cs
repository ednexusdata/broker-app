using IConfiguration = EdNexusData.Broker.Connector.Configuration.IConfiguration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EdNexusData.Broker.Web.Helpers;

public class ModelFormBuilderHelper
{
    public static string HtmlForModel(IConfiguration model)
    {
        var formHTML = $"""
<form method="post" action="/Settings/Configuration/{model.GetType().FullName}">
<input type="hidden" name="ConnectorConfigurationType" value="{model.GetType().AssemblyQualifiedName}">
"""; // start form html output

        // Get type of incoming model
        var modelType = model.GetType();
        // Get the properties of the model
        var modelTypeProps = modelType.GetProperties();
        // For each, determine input html needed
        foreach(var modelTypeProp in modelTypeProps)
        {
            // include existing value if set in model

            // Write HTML
            var modelTypePropAttrsDataType = (DataTypeAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DataTypeAttribute)).FirstOrDefault()!;
            var modelTypePropAttrsDisplayName = (DisplayNameAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
            var modelTypePropAttrsDescription = (DescriptionAttribute)modelTypeProp.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DescriptionAttribute)).FirstOrDefault()!;

            var displayNameToUse = modelTypeProp.Name;
            if (modelTypePropAttrsDisplayName is not null)
            {
              displayNameToUse = modelTypePropAttrsDisplayName.DisplayName;
            }

            if (modelTypePropAttrsDataType.DataType == DataType.Url)
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

            if (modelTypePropAttrsDataType.DataType == DataType.Text)
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

            if (modelTypePropAttrsDataType.DataType == DataType.Password)
            {
                formHTML += $"""
                <div class="sm:col-span-4 my-4">
              <label for="{modelTypeProp.Name}" class="block text-sm font-medium leading-6 text-gray-900">{displayNameToUse}</label>
              <div class="mt-2">
                <div class="flex rounded-md shadow-sm ring-1 ring-inset ring-gray-300 focus-within:ring-2 focus-within:ring-inset focus-within:ring-tertiary-700 sm:max-w-md">
                  <input type="password" autocomplete="off" name="{modelTypeProp.Name}" id="{modelTypeProp.Name}" class="block w-full rounded-md border-0 p-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-tertiary-700 sm:text-sm sm:leading-6">
                </div>
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
