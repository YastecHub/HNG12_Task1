using HNG12_Task1.Response;
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
        public IActionResult ClassifyNumber([FromQuery] string number)
        {

            // ✅ Validate input early and return immediately
            if (string.IsNullOrWhiteSpace(number) || !int.TryParse(number, out int parsedNumber))
            {
                return BadRequest(new
                {
                    error = true,
                    number = number
                });
            }
            return Ok(ClassifyValidNumber(parsedNumber));
        }

        private NumberClassificationResponse ClassifyValidNumber(int number)
        {
            var isPrime = IsPrime(number);
            var isPerfect = IsPerfect(number);
            var isArmstrong = IsArmstrong(number);
            var digitSum = GetDigitSum(number);
            var properties = new List<string> { number % 2 == 0 ? "even" : "odd" };
            if (isArmstrong) properties.Add("armstrong");
            properties.Sort();

            return new NumberClassificationResponse
            {
                Number = number,
                is_prime = isPrime,
                is_perfect = isPerfect,
                Properties = properties,
                digit_sum = digitSum,
                fun_fact = GetFunFact(number).Result
            };
        }


        // Determine if the number is prime
        private bool IsPrime(int number)
        {
            if (number < 2) 
                return false;
            if (number == 2) 
                return true;
            if (number % 2 == 0) 
                return false;

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
            if (number < 1) 
                return false;

            int sum = 1;
            int sqrt = (int)Math.Sqrt(number);
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
            int positiveNumber = Math.Abs(number);
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

            _funFactCache[number] = fact;
            return fact;
        }
    }
}