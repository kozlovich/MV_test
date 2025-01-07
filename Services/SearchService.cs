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
                var prices = facility.PricesPerPerson;

                // CHECK IF BOOKING POSSIBLE
                // Assume that booking is possible if all dates are available in range between Arrival and Departure
                // Ex. requested dates 11.01-13.01, than prices should exists: 11.01, 12.01, 13.01
                // Ex. requested dates 11.01-15.01 and exists 11.01, 12.01, 13.01 => cannot book a facility
                if (!CheckAvailability(prices, searchParameters.Arrival, searchParameters.Departure))
                    continue;

                // CREATE A PRICE MAP
                Dictionary<DateTime, float> pricePerPersonPairs = new Dictionary<DateTime, float>();
                int totalDays = (int)searchParameters.Departure.Subtract(searchParameters.Arrival).TotalDays + 1;
                for (int i = 0; i < totalDays; i++)
                {
                    var day = searchParameters.Arrival.AddDays(i);
                    var price = prices.Where(x => x.ValidFrom <= day && day <= x.ValidTo).OrderByDescending(x => x.Created).FirstOrDefault();
                    pricePerPersonPairs.Add(day, price.Price);
                }

                // CALCULATE BED DISCOUNTS
                var bedDiscounts = facility.BedDiscounts.Where(x => x.Beds >= totalGuests);
                List<KeyValuePair<int, float>> discountByGuests = new List<KeyValuePair<int, float>>(); // represents <age, bed discount multiplier>
                foreach (var guestAge in searchParameters.GuestAges)
                {
                    var bedDiscountPriceMultiplier = 1.0f;
                    var bedDiscount = bedDiscounts.Where(x => x.GuestAgeFrom <= guestAge && guestAge <= x.GuestAgeTo).OrderByDescending(x => x.Created).FirstOrDefault();
                    if (bedDiscount != null)
                        bedDiscountPriceMultiplier = (100 - bedDiscount.DiscountInPercents) / 100f;
                    
                    discountByGuests.Add(new KeyValuePair<int, float>(guestAge, bedDiscountPriceMultiplier));
                }

                var promotionPriceMultiplier = 1.0f;
                var promotion = facility.Promotions.Where(x => x.ValidFrom <= searchParameters.Arrival && searchParameters.Departure <= x.ValidTo).OrderByDescending(x => x.Created).FirstOrDefault();
                if (promotion != null)
                {
                    promotionPriceMultiplier = (100 - promotion.DiscountInPercents) / 100f;
                }

                // CALCULATE DISCOUNTED PRICE
                var totalPrice = discountByGuests.Sum(g => g.Value * pricePerPersonPairs.Sum(x => x.Value)) * promotionPriceMultiplier;

                // CALCULATE PRICE WITHOUT ANY DISCOUNTS
                var totalPriceWithoutDiscounts = pricePerPersonPairs.Sum(x => x.Value) * totalGuests;

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

    private bool CheckAvailability(List<PricePerPerson> prices, DateTime arrivalDate, DateTime departureDate)
    {
        DateTime currentDate = arrivalDate;
        while (currentDate <= departureDate)
        {
            if (!prices.Any(p => p.ValidFrom <= currentDate && currentDate <= p.ValidTo))
                return false;

            currentDate = currentDate.AddDays(1);
        }

        return true;
    }

}