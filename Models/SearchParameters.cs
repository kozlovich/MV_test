using Microsoft.AspNetCore.Mvc;

namespace MV_test.Models;

public class SearchParameters
{
    // public DateTime? Date { get;set; }

    public int[] GuestAges { get; set; }

    [ModelBinder(BinderType = typeof(CustomDateTimeModelBinder))]
    public DateTime Arrival { get; set; }

    [ModelBinder(BinderType = typeof(CustomDateTimeModelBinder))]
    public DateTime Departure { get; set; }
}