namespace TypeIdTests
{
    using System.Collections.Generic;
    using TypeId;

    [TestClass]
    public class SpecTests
    {
        private readonly Dictionary<Models.TestCase, bool> validationTestPairs = new();

        [TestInitialize]
        public void Initialize()
        {
            /*
             * Last updated: 2023-06-29
             */

            // load the file

            var validityByFilePath = new Dictionary<string, bool>
            {
                { "SpecTests/invalid.yml", false },
                { "SpecTests/valid.yml", true }
            };

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            foreach (var filePath in validityByFilePath.Keys)
            {
                var fileContents = File.ReadAllText(filePath);
                var isValid = validityByFilePath[filePath];

                // parse the file
                var testCases = deserializer.Deserialize<List<Models.TestCase>>(fileContents);

                // store the data in a list of test cases
                foreach (var testCase in testCases)
                {
                    validationTestPairs.Add(testCase, isValid);
                }
            }
        }

        [Ignore]
        [TestMethod]
        public void SpecTest()
        {
            foreach (var testCase in validationTestPairs.Keys)
            {
                var isValid = validationTestPairs[testCase];
                //TODO: implement
            }
        }
    }
}