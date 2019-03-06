//
// WeatherServer.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2011 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using MapKit;
using SQLite;

namespace WeatherMap
{
    public class WeatherServer : IDisposable
	{
        SQLiteConnection store;

		public WeatherServer ()
		{
			// Create our SQL connection context
			store = new SQLiteConnection ("WeatherMap.sqlite");

			// Create a table mapping for WeatherItem (if it doesn't already exist)
			store.CreateTable<WeatherForecast> ();

           //If items are already in database then add Fayetteville
            if (store.Table<WeatherForecast>().Count() < 7)
            {
                store.Insert(new WeatherForecast
                {
                    Place = "Fayetteville",
                    Latitude = 36.0822,
                    Longitude = 94.1719,
                    High = 49,
                    Low = 27,
                    Windchill = 20,
                    Condition = WeatherConditions.Snow
                });
                store.Commit();
            }
            // Check to see if we've already got some items in the database
            else if (store.Table<WeatherForecast>().Count() == 0) 
            {
                // Populate our database with a list of WeatherItems
                store.Insert(new WeatherForecast
                {
                    Place = "S.F.",
                    Latitude = 37.779941,
                    Longitude = -122.417908,
                    High = 80,
                    Low = 50,
                    Windchill = 2,
                    Condition = WeatherConditions.Sunny
                });

                store.Insert(new WeatherForecast
                {
                    Place = "Denver",
                    Latitude = 39.752601,
                    Longitude = -104.982605,
                    High = 40,
                    Low = 30,
                    Windchill = 21,
                    Condition = WeatherConditions.Snow
                });

                store.Insert(new WeatherForecast
                {
                    Place = "Chicago",
                    Latitude = 41.863425,
                    Longitude = -87.652359,
                    High = 39,
                    Low = 29,
                    Windchill = 19,
                    Condition = WeatherConditions.Cloudy
                });

                store.Insert(new WeatherForecast
                {
                    Place = "Seattle",
                    Latitude = 47.615884,
                    Longitude = -122.332764,
                    High = 75,
                    Low = 45,
                    Windchill = 12,
                    Condition = WeatherConditions.Showers
                });

                store.Insert(new WeatherForecast
                {
                    Place = "Boston",
                    Latitude = 42.350425,
                    Longitude = -71.070557,
                    High = 75,
                    Low = 45,
                    Windchill = 19,
                    Condition = WeatherConditions.PartlyCloudy
                });

                store.Insert(new WeatherForecast
                {
                    Place = "Miami",
                    Latitude = 25.780107,
                    Longitude = -80.244141,
                    High = 90,
                    Low = 75,
                    Windchill = 8,
                    Condition = WeatherConditions.Thunderstorms
                });

                //Creating a WeatherForecast Node (Object) for Fayetteville
                store.Insert(new WeatherForecast
                {
                    Place = "Fayetteville",
                    Latitude = 36.0822,
                    Longitude = 94.1719,
                    High = 49,
                    Low = 27,
                    Windchill = 20,
                    Condition = WeatherConditions.Snow
                });

                store.Commit();

            }
        }

		public WeatherForecastAnnotation[] GetForecastAnnotations (MKCoordinateRegion region, int maxCount)
		{
            double longMin = region.Center.Longitude - region.Span.LongitudeDelta / 2;
            double longMax = region.Center.Longitude + region.Span.LongitudeDelta / 2;
            double latMin = region.Center.Latitude - region.Span.LatitudeDelta / 2;
            double latMax = region.Center.Latitude + region.Span.LatitudeDelta / 2;

			// Query for WeatherForecasts within our specified region
			var results = from item in store.Table<WeatherForecast> ()
				//where (item.Latitude > latMin && item.Latitude < latMax && item.Longitude > longMin && item.Longitude < longMax)
					orderby item.Latitude
					orderby item.Longitude
					select item;

			// Iterate over the results and add them to a list
			var list = new List<WeatherForecastAnnotation> ();
			foreach (WeatherForecast forecast in results)
            {
                list.Add(new WeatherForecastAnnotation(forecast));
            }

            if (list.Count <= maxCount) {
				// We got fewer results than the max, so just return what we found
				return list.ToArray ();
			}

			// Calculate a stride so we can get an evenly distributed sampling of the results
			double index = 0.0, stride = (double) (list.Count) / (double) maxCount;
			var annotations = new WeatherForecastAnnotation [maxCount];

			for (int i = 0; i < maxCount && (int) index < list.Count; i++, index += stride)
				annotations[i] = list[(int) index];

			return annotations;
		}

		public void Dispose ()
		{
			if (store != null) {
				store.Commit ();
				store.Dispose ();
				store = null;
			}
		}
	}
}

