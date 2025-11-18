using System;

namespace Acme.SimpleTaskApp.Products
{
	public static class ProductExtensions
	{
		public static decimal GetDiscountedPrice(this ProductVariant variant, decimal discountPercentage)
		{
			if (discountPercentage <= 0)
				return variant.Price;

			var discountAmount = variant.Price * (discountPercentage / 100);
			return variant.Price - discountAmount;
		}

		public static decimal GetDiscountAmount(this ProductVariant variant, decimal discountPercentage)
		{
			if (discountPercentage <= 0)
				return 0;

			return variant.Price * (discountPercentage / 100);
		}

		public static bool HasDiscount(this ProductVariant variant, decimal discountPercentage)
		{
			return discountPercentage > 0;
		}
	}
}
