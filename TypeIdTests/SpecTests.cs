namespace TypeIdTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            var validitiesByFilePath = new Dictionary<string, bool>
            {
                { "SpecTests/invalid.yml", false },
                { "SpecTests/valid.yml", true }
            };

            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            foreach (var validityByFilePath in validitiesByFilePath)
            {
                var fileContents = File.ReadAllText(validityByFilePath.Key);
                var isValid = validityByFilePath.Value;

                // store the data in a list of test cases
                foreach (var testCase in deserializer.Deserialize<IEnumerable<Models.TestCase>>(fileContents))
                {
                    validationTestPairs.Add(testCase, isValid);
                }
            }
        }

        [TestMethod]
        public void SpecTestInvalidDecoding()
        {
            foreach (var testCase in validationTestPairs.Where(tc => !tc.Value).Select(t => t.Key))
            {
                var isValid = TypeId.TryParse(testCase.typeid, out var typeId);
                Assert.AreEqual(false, isValid, testCase.description ?? $"{testCase.typeid} should be valid");
            }
        }

        [TestMethod]
        public void SpecTestInvalidEncoding()
        {
            foreach (var testCase in validationTestPairs.Where(tc => !tc.Value).Select(t => t.Key))
            {
                var isValid = TypeId.TryParse(testCase.typeid, out var typeId);
                Assert.AreEqual(false, isValid, testCase.description ?? $"{testCase.typeid} should not be valid");
            }
        }

        [TestMethod]
        public void SpecTestValidDecoding()
        {
            foreach (var testCase in validationTestPairs.Where(tc => tc.Value).Select(t => t.Key))
            {
                var isValid = TypeId.TryParse(testCase.typeid, out var typeId);
                Assert.AreEqual(true, isValid, testCase.description ?? $"{testCase.typeid} should be valid");
                Assert.AreEqual(testCase.typeid, typeId.ToString(), testCase.description ?? $"{nameof(testCase.typeid)} should be {testCase.typeid}");
                Assert.AreEqual(testCase.uuid, typeId.GetUuid(), testCase.description ?? $"{nameof(testCase.uuid)} should be {testCase.uuid}");
            }
        }

        [TestMethod]
        public void SpecTestValidEncoding()
        {
            foreach (var testCase in validationTestPairs.Where(tc => tc.Value).Select(t => t.Key))
            {
                var result = new TypeId(testCase.prefix, testCase.uuid);
                Assert.AreEqual(testCase.typeid, result.ToString(), testCase.description ?? $"Should be {testCase.typeid}");
            }
        }
    }
}