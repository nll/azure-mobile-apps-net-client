﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTests.Shared.Helpers
{
    public static class Utilities
    {
        public static void CamelCaseProps(JObject itemToUpdate)
        {
            List<string> keys = new List<string>();
            foreach (var x in itemToUpdate)
            {
                keys.Add(x.Key);
            }

            foreach (var key in keys)
            {
                if (char.IsUpper(key[0]))
                {
                    StringBuilder camel = new StringBuilder(key);
                    camel[0] = Char.ToLowerInvariant(key[0]);
                    itemToUpdate[camel.ToString()] = itemToUpdate[key];
                    itemToUpdate.Remove(key);
                }
            }
        }

        public static bool CompareArrays<T>(T[] array1, T[] array2)
        {
            return CompareArrays<T>(array1, array2, null);
        }

        public static bool CompareArrays<T>(T[] array1, T[] array2, List<string> errors)
        {
            if (array1 == null)
            {
                if (array2 != null && errors != null)
                {
                    errors.Add("First array is null, second is not");
                }

                return array2 == null;
            }

            if (array2 == null)
            {
                if (errors != null)
                {
                    errors.Add("First array is not null, second is null");
                }

                return false;
            }

            if (array1.Length != array2.Length)
            {
                if (errors != null)
                {
                    errors.Add(string.Format(CultureInfo.InvariantCulture, "Size of first array ({0}) is different than second ({1})", array1.Length, array2.Length));
                }

                return false;
            }

            for (int i = 0; i < array2.Length; i++)
            {
                object item1 = array1.GetValue(i);
                object item2 = array2.GetValue(i);
                if ((item1 == null) != (item2 == null))
                {
                    if (errors != null)
                    {
                        errors.Add(string.Format(CultureInfo.InvariantCulture, "Difference in item {0}: first {1} null, second {2} null",
                            i, item1 == null ? "is" : "is not", item2 == null ? "is" : "is not"));
                    }

                    return false;
                }

                if (item1 != null && !item1.Equals(item2))
                {
                    if (errors != null)
                    {
                        errors.Add(string.Format(CultureInfo.InvariantCulture, "Difference in item {0}: first = {1}; second = {2}", i, item1, item2));
                    }

                    return false;
                }
            }

            return true;
        }

        public static string CreateSimpleRandomString(Random rndGen, int size)
        {
            return new string(Enumerable.Range(0, size).Select(_ => (char)rndGen.Next(' ', '~' + 1)).ToArray());
        }

        public static string ArrayToString<T>(T[] array)
        {
            if (array == null)
            {
                return "<<NULL>>";
            }
            else
            {
                return "[" + string.Join(", ", array.Select(i => i == null ? "<NULL>" : i.ToString())) + "]";
            }
        }

        public static bool CompareJson(JToken expected, JToken actual, List<string> errors)
        {
            if (expected == null)
            {
                return actual == null;
            }

            if (actual == null)
            {
                return false;
            }

            if (expected.Type != actual.Type)
            {
                errors.Add(string.Format("Expected value type {0} != actual {1}", expected.Type, actual.Type));
                return false;
            }

            switch (expected.Type)
            {
                case JTokenType.Boolean:
                    return expected.Value<bool>() == actual.Value<bool>();
                case JTokenType.Null:
                    return true;
                case JTokenType.String:
                case JTokenType.Date:
                    return expected.Value<string>() == actual.Value<string>();
                case JTokenType.Float:
                case JTokenType.Integer:
                    double expectedNumber = expected.Value<double>();
                    double actualNumber = actual.Value<double>();
                    double delta = 1 - expectedNumber / actualNumber;
                    double acceptableEpsilon = 0.000001;
                    if (Math.Abs(delta) > acceptableEpsilon)
                    {
                        errors.Add(string.Format("Numbers differ more than the allowed difference: {0} - {1}",
                            expected, actual));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case JTokenType.Array:
                    JArray expectedArray = (JArray)expected;
                    JArray actualArray = (JArray)actual;
                    if (expectedArray.Count != actualArray.Count)
                    {
                        errors.Add(string.Format("Size of arrays are different: expected {0} != actual {1}", expectedArray.Count, actualArray.Count));
                        return false;
                    }

                    for (int i = 0; i < expectedArray.Count; i++)
                    {
                        if (!CompareJson(expectedArray[i], actualArray[i], errors))
                        {
                            errors.Add("Difference in array item at index " + i);
                            return false;
                        }
                    }

                    return true;
                case JTokenType.Object:
                    JObject expectedObject = (JObject)expected;
                    Dictionary<string, string> expectedKeyMap = new Dictionary<string, string>();
                    foreach (var child in expectedObject)
                    {
                        expectedKeyMap.Add(child.Key.ToLowerInvariant(), child.Key);
                    }

                    JObject actualObject = (JObject)actual;
                    Dictionary<string, string> actualKeyMap = new Dictionary<string, string>();
                    foreach (var child in actualObject)
                    {
                        actualKeyMap.Add(child.Key.ToLowerInvariant(), child.Key);
                    }

                    foreach (var child in expectedObject)
                    {
                        var key = child.Key.ToLowerInvariant();
                        if (key == "id") continue; // set by server, ignored at comparison

                        if (!actualKeyMap.ContainsKey(key) || actualObject[actualKeyMap[key]] == null)
                        {
                            // Still might be OK, if the missing value is default.
                            var expectedObjectValue = expectedObject[expectedKeyMap[key]];

                            if (expectedObjectValue.Type == JTokenType.Null ||
                                (expectedObjectValue.Type == JTokenType.Integer && expectedObjectValue.Value<long>() == 0) ||
                                (expectedObjectValue.Type == JTokenType.Float && expectedObjectValue.Value<double>() == 0.0))
                            {
                                // No problem.
                            }
                            else
                            {
                                errors.Add(string.Format("Expected object contains a pair with key {0}, actual does not.", child.Key));
                                return false;
                            }
                        }
                        else
                        {
                            if (!CompareJson(expectedObject[expectedKeyMap[key]], actualObject[actualKeyMap[key]], errors))
                            {
                                errors.Add("Difference in object member with key " + key);
                                return false;
                            }
                        }
                    }

                    return true;
                default:
                    throw new ArgumentException("Don't know how to compare JToken of type " + expected.Type);
            }
        }

        public static int GetArrayHashCode<T>(T[] array)
        {
            int result = 0;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object item = array.GetValue(i);
                    if (item != null)
                    {
                        result ^= item.GetHashCode();
                    }
                }
            }

            return result;
        }

        public static async Task<MobileServiceUser> GetDummyUser(MobileServiceClient mobileServiceClient)
        {
            var dummyUser = await mobileServiceClient.InvokeApiAsync("JwtTokenGenerator", HttpMethod.Get, null);

            MobileServiceUser user = new MobileServiceUser((string)dummyUser["userId"])
            {
                MobileServiceAuthenticationToken = (string)dummyUser["authenticationToken"]
            };
            return user;
        }
    }
}
