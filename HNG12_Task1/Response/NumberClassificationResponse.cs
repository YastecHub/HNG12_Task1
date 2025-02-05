namespace HNG12_Task1.Response
{
    public class NumberClassificationResponse
    {
        public int Number { get; set; }
        public bool is_prime { get; set; }
        public bool is_perfect { get; set; }
        public List<string> Properties { get; set; }
        public int digit_sum { get; set; }
        public string fun_fact { get; set; }
    }
}
