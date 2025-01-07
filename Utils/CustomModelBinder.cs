using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace MV_test;

public class CustomDateTimeModelBinder : IModelBinder
{
    private readonly string _dateFormat;

    public CustomDateTimeModelBinder(string dateFormat = "dd.MM.yyyy")
    {
        _dateFormat = dateFormat;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        if (DateTime.TryParseExact(value, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedDate);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Invalid date format. Expected format: {_dateFormat}");
        }

        return Task.CompletedTask;
    }
}
