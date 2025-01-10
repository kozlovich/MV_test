using MV_test.Models;

namespace MV_test.Services;

public interface ISearchService
{
    List<Offer> Search(SearchParameters searchParameters);
}

public class SearchService : ISearchService
{
    public readonly List<Accomodation> _accomodations;
    public SearchService(ISeedService seedService)
    {
        _accomodations = seedService.GetSeededData();
    }

    public List<Offer> Search(SearchParameters searchParameters)
    {
        var totalGuests = searchParameters.GuestAges.Count();

        var offers = new List<Offer>();
        foreach (var accomodation in _accomodations)
        {
            var facilities = accomodation.Facilities.Where(f => f.NumberOfBeds >= totalGuests);
            foreach (var facility in facilities)
            {
                if (!CheckAvailability(facility.PricesPerPerson, searchParameters))
                    continue;

                Dictionary<DateTime, float> dateAndPricePerPersonPerDayPairs = MapDatesAndPrices(facility.PricesPerPerson, searchParameters);
                Dictionary<DateTime, float[]> dateAndBedDiscountPairs = MapDatesAndBedDiscounts(facility.BedDiscounts, searchParameters);
                Dictionary<DateTime, float> dateAndPromotionDiscountPairs = MapDatesAndPromotionDiscounts(facility.Promotions, searchParameters);

                // CALCULATE DISCOUNTED PRICE
                var totalPrice = CalculateTotalPrice(dateAndPricePerPersonPerDayPairs, dateAndBedDiscountPairs, dateAndPromotionDiscountPairs);

                // CALCULATE PRICE WITHOUT ANY DISCOUNTS
                var totalPriceWithoutDiscounts = CalculateTotalPriceWithoutDiscounts(dateAndPricePerPersonPerDayPairs, searchParameters.GuestAges.Count());

                // CREATE OFFER
                Offer offer = new Offer
                {
                    AccomodationId = accomodation.Id,
                    FacilityId = facility.Id,
                    Price = (float)Math.Round(totalPrice, 2, MidpointRounding.AwayFromZero),
                    PriceWithoutDiscounts = totalPriceWithoutDiscounts
                };

                offers.Add(offer);
            }
        }

        return offers;
    }
                
    /// <summary>
    /// CHECK IF BOOKING POSSIBLE
    /// Assume that booking is possible if all dates are available in range between Arrival and Departure
    /// Ex. requested dates 11.01-13.01, than prices should exists: 11.01, 12.01, 13.01
    /// Ex. requested dates 11.01-15.01 and exists 11.01, 12.01, 13.01 => cannot book a facility
    /// </summary>
    private bool CheckAvailability(List<PricePerPerson> pricesPerPersonPerDay, SearchParameters searchParameters)
    {
        DateTime currentDate = searchParameters.Arrival;
        while (currentDate <= searchParameters.Departure)
        {
            if (!pricesPerPersonPerDay.Any(p => p.ValidFrom <= currentDate && currentDate <= p.ValidTo))
                return false;

            currentDate = currentDate.AddDays(1);
        }

        return true;
    }

    /// <summary>
    /// Create map with prices
    /// </summary>
    private Dictionary<DateTime, float> MapDatesAndPrices(List<PricePerPerson> pricesPerPersonPerDay, SearchParameters searchParameters)
    {
        Dictionary<DateTime, float> pricePerPersonPerDayPairs = new Dictionary<DateTime, float>();
        int totalDays = (int)searchParameters.Departure.Subtract(searchParameters.Arrival).TotalDays;
        for (int i = 0; i <= totalDays; i++)
        {
            var currentDate = searchParameters.Arrival.AddDays(i);
            var pricePerPersonPerDay = pricesPerPersonPerDay.Where(x => x.ValidFrom <= currentDate && currentDate <= x.ValidTo).OrderByDescending(x => x.Created).FirstOrDefault();
            if (pricePerPersonPerDay != null)
                pricePerPersonPerDayPairs.Add(currentDate, pricePerPersonPerDay.Price);
        }
        
        return pricePerPersonPerDayPairs;
    }

    /// <summary>
    /// Map Bed Discounts and Prices
    /// </summary>
    private Dictionary<DateTime, float[]> MapDatesAndBedDiscounts(List<BedDiscount> bedDiscounts, SearchParameters searchParameters)
    {
        Dictionary<DateTime, float[]> dateAndBedDiscountPairs = new Dictionary<DateTime, float[]>();
        int totalDays = (int)searchParameters.Departure.Subtract(searchParameters.Arrival).TotalDays;
        for (int i = 0; i <= totalDays; i++)
        {
            var currentDate = searchParameters.Arrival.AddDays(i);
            
            float[] multipliers = new float[searchParameters.GuestAges.Count()];

            for (int j = 0; j < searchParameters.GuestAges.Count(); j++)
            {
                float discountMultiplier = 1.0f;
                int guestAge = searchParameters.GuestAges[j];
                int bed = j + 1;
                var bedDiscount = bedDiscounts.Where(x => x.Bed == bed && x.GuestAgeFrom <= guestAge && guestAge <= x.GuestAgeTo).OrderByDescending(o => o.Created).FirstOrDefault();
                if (bedDiscount != null)
                {
                    discountMultiplier = (100 - bedDiscount.DiscountInPercents) / 100f;
                }
                multipliers[j] = discountMultiplier;
            }
            
            dateAndBedDiscountPairs.Add(currentDate, multipliers);
        }
        
        return dateAndBedDiscountPairs;
    }

    private Dictionary<DateTime, float> MapDatesAndPromotionDiscounts(List<Promotion> promotions, SearchParameters searchParameters)
    {
        Dictionary<DateTime, float> promotionPairs = new Dictionary<DateTime, float>();
        int totalDays = (int)searchParameters.Departure.Subtract(searchParameters.Arrival).TotalDays;
        for (int i = 0; i <= totalDays; i++)
        {
            var discountMultiplier = 1.0f;
            var currentDate = searchParameters.Arrival.AddDays(i);
            var promotionPerDay = promotions.Where(x => x.ValidFrom <= currentDate && currentDate <= x.ValidTo).OrderByDescending(x => x.Created).FirstOrDefault();
            if (promotionPerDay != null)
                discountMultiplier = (100 - promotionPerDay.DiscountInPercents) / 100f;
            
            promotionPairs.Add(currentDate, discountMultiplier);
        }
        
        return promotionPairs;
    }

    private float CalculateTotalPriceWithoutDiscounts(Dictionary<DateTime, float> dateAndPricePerPersonPerDayPairs, int totalGuests)
    {
        return dateAndPricePerPersonPerDayPairs.Sum(x => x.Value) * totalGuests;
    }

    private float CalculateTotalPrice(Dictionary<DateTime, float> dateAndPricePerPersonPerDayPairs, Dictionary<DateTime, float[]> dateAndBedDiscountPairs, Dictionary<DateTime, float> dateAndPromotionDiscountPairs)
    {
        float totalPrice = 0f;
        DateTime currentDate = dateAndPricePerPersonPerDayPairs.First().Key;
        DateTime departureDate = dateAndPricePerPersonPerDayPairs.Last().Key;
        while (currentDate <= departureDate)
        {
            // GET price per person for current date
            float currentDatePrice = dateAndPricePerPersonPerDayPairs[currentDate];
            // APPLY Bed Discount
            float currentDatePriceWithBedDiscounts = dateAndBedDiscountPairs[currentDate].Sum(x => currentDatePrice * x);
            // APPLY Promotion Discount
            float promotionMultiplier = dateAndPromotionDiscountPairs[currentDate];
            totalPrice += currentDatePriceWithBedDiscounts * promotionMultiplier;

            currentDate = currentDate.AddDays(1);
        }
        return totalPrice;
    }
}