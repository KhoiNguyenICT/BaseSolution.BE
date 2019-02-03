namespace BaseSolution.Core.Commons.Constants
{
    public class DefineValueList
    {
        public const int DefaultQuerySkip = 0;

        public const int DefaultQueryTake = 10;

        public const int FormOptionsMultipartBodyLengthLimit = 104857600; // 100 * 1024 * 1024 = 10485760 = 100M // MultipartReader.DefaultMultipartBodyLengthLimit;
    }
}