namespace Datatag.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var result = Parser.Decode("test.dt");
            var resultString = Parser.Encode(result, EncodeSettings.Pretty);
            Console.WriteLine(resultString);

            result = Parser.DecodeString(resultString);
            resultString = Parser.Encode(result, EncodeSettings.Pretty);
            Console.WriteLine(resultString);
        }
    }
}