using System;
using Newtonsoft.Json.Linq;

namespace Acme.SimpleTaskApp.Authentication.External.External.Google
{
    public static class GoogleHelper
    {
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("id");
        }

        public static string GetName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("name");
        }

        public static string GetGivenName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("given_name");
        }

        public static string GetFamilyName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("family_name");
        }

        public static string GetProfile(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("link");
        }

        public static string GetEmail(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Value<string>("email");
        }

        private static string TryGetValue(JObject user, string propertyName, string subProperty)
        {
            if (user.TryGetValue(propertyName, out var value) && (JObject.Parse(value.ToString())?.TryGetValue(subProperty, out value) ?? false))
            {
                return value.ToString();
            }

            return null;
        }

        private static string TryGetFirstValue(JObject user, string propertyName, string subProperty)
        {
            if (user.TryGetValue(propertyName, out var value))
            {
                JArray jArray = JArray.Parse(value.ToString());
                if (jArray != null && jArray.Count > 0)
                {
                    JObject jObject = JObject.Parse(jArray.First!.ToString());
                    if (jObject != null && jObject.TryGetValue(subProperty, out value))
                    {
                        return value.ToString();
                    }
                }
            }

            return null;
        }
    }
}
