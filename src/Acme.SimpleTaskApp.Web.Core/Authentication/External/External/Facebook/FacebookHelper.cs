using Newtonsoft.Json.Linq;
using System;

namespace Acme.SimpleTaskApp.Authentication.External.External.Facebook
{
    public static class FacebookHelper
    {
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("id");
        }

        public static string GetAgeRangeMin(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return TryGetValue(user, "age_range", "min");
        }

        public static string GetAgeRangeMax(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return TryGetValue(user, "age_range", "max");
        }

        public static string GetBirthday(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("birthday");
        }

        public static string GetEmail(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("email");
        }

        public static string GetFirstName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("first_name");
        }

        public static string GetGender(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("gender");
        }

        public static string GetLastName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("last_name");
        }

        public static string GetLink(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("link");
        }

        public static string GetLocation(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return TryGetValue(user, "location", "name");
        }

        public static string GetLocale(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("locale");
        }

        public static string GetMiddleName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("middle_name");
        }

        public static string GetName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("name");
        }

        public static string GetTimeZone(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("timezone");
        }

        private static string TryGetValue(JObject user, string propertyName, string subProperty)
        {
            if (user.TryGetValue(propertyName, out var value) && (JObject.Parse(value.ToString())?.TryGetValue(subProperty, out value) ?? false))
            {
                return value.ToString();
            }

            return null;
        }
    }
}
