﻿using HNG12_Task1.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace HNG12_Task1.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumberClassificationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly ConcurrentDictionary<int, string> _funFactCache = new();

        public NumberClassificationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("classify-number")]
        public async Task<IActionResult> ClassifyNumber()
        {
            var numberQuery = HttpContext.Request.Query["number"].ToString();

            // ✅ Custom validation: Empty number parameter
            if (string.IsNullOrWhiteSpace(numberQuery))
            {
                return BadRequest(new { error = true, number = "" });
            }

            if (!int.TryParse(numberQuery, out int parsedNumber))
            {
                return BadRequest(new { error = true, number = numberQuery });
            }

            // Asynchronously fetch properties and fun fact
            var isPrime = IsPrime(parsedNumber);
            var isPerfect = IsPerfect(parsedNumber);
            var isArmstrong = IsArmstrong(parsedNumber);
            var digitSum = GetDigitSum(parsedNumber);
            var funFact = await GetFunFact(parsedNumber);

            // Determine the number properties
            var properties = new List<string> { parsedNumber % 2 == 0 ? "even" : "odd" };
            if (isArmstrong) properties.Add("armstrong");

            // Build and return the response
            return Ok(new NumberClassificationResponse
            {
                Number = parsedNumber,
                is_prime = isPrime,
                is_perfect = isPerfect,
                Properties = properties,
                digit_sum = digitSum,
                fun_fact = funFact
            });
        }

        // Determine if the number is prime
        private bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int limit = (int)Math.Sqrt(number);
            for (int i = 3; i <= limit; i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        // Determine if the number is a perfect number
        private bool IsPerfect(int number)
        {
            if (number < 1) return false;

            int sum = 1, sqrt = (int)Math.Sqrt(number);
            for (int i = 2; i <= sqrt; i++)
            {
                if (number % i == 0)
                {
                    sum += i + (i != number / i ? number / i : 0);
                }
            }
            return sum == number && number != 1;
        }

        // Determine if the number is an Armstrong number
        private bool IsArmstrong(int number)
        {
            int positiveNumber = Math.Abs(number); // Ensure the number is positive
            int sum = 0, temp = positiveNumber, digits = positiveNumber.ToString().Length;
            while (temp != 0)
            {
                int digit = temp % 10;
                sum += (int)Math.Pow(digit, digits);
                temp /= 10;
            }
            return sum == positiveNumber;
        }


        // Calculate the sum of digits of the number
        private int GetDigitSum(int number) => Math.Abs(number).ToString().Sum(c => c - '0');

        // Get a fun fact about the number from an external API or cache
        private async Task<string> GetFunFact(int number)
        {
            if (_funFactCache.TryGetValue(number, out string cachedFact))
            {
                return cachedFact;
            }

            var client = _httpClientFactory.CreateClient();
            string fact = await client.GetStringAsync($"http://numbersapi.com/{number}/math");

            _funFactCache[number] = fact; // Cache the fact for future use
            return fact;
        }
    }
}
