namespace TypeIdTests
{
    using System.Diagnostics;
    using TypeId;

    [TestClass]
    public class TypeIdGeneralTests
    {
        [TestMethod]
        public void Equal()
        {
            var typeId = TypeId.NewTypeId("prefix");
            var typeId2 = TypeId.Parse(typeId.ToString());
            Assert.AreEqual(typeId, typeId2, "Parsing");

            typeId2 = new TypeId();
            typeId2.SetId(typeId.Id);
            typeId2.SetType(typeId.Type);
            Assert.AreEqual(typeId, typeId2, "Setters");
        }

        [TestMethod]
        public void NotEqual()
        {
            var typeId = TypeId.NewTypeId("prefix");
            var typeId2 = TypeId.NewTypeId("prefix");
            Assert.AreNotEqual(typeId, typeId2);
        }

        [TestMethod]
        public void PerformanceTest()
        {
            // run a thousand times to make sure we don't get an equal sign and use Stopwatch to
            // measure the time
            const int iterations = int.MaxValue / 1_000;
            var hashSetOfIds = new HashSet<TypeId>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < iterations; i++)
            {
                hashSetOfIds.Add(TypeId.NewTypeId("prefix"));
            }
            stopwatch.Stop();

            const decimal timePerIterationMilliseconds = (decimal)0.003;

            var elapsedMilliseconds = (decimal)stopwatch.ElapsedMilliseconds;

            // assert it takes less than timePerIterationMilliseconds per iteration
            Assert.IsTrue(elapsedMilliseconds < iterations * timePerIterationMilliseconds, $"Time per iteration: {elapsedMilliseconds / iterations} ms");
            Console.WriteLine($"Time to run {iterations} iterations: {elapsedMilliseconds} ms, {elapsedMilliseconds / iterations} ms per iteration");

            // assert we have the expected number of unique ids
            Assert.AreEqual(iterations, hashSetOfIds.Count, "Unique ids");
        }

        [TestMethod]
        public void SortingTest()
        {
            // create a list of type ids
            var typeIds = new List<TypeId>();
            for (var i = 0; i < 10; i++)
            {
                var typeId = TypeId.NewTypeId("prefix");
                typeIds.Add(typeId);
                Console.WriteLine($"-> {typeId}");
            }

            // typeIds are sortable so the list should be sorted without any extra work
            var orderedSequence = typeIds.OrderBy(x => x).ToList();

            // assert the lists are sorted
            Assert.IsTrue(typeIds.SequenceEqual(orderedSequence), "Sorting");
        }

        [TestMethod]
        public void TryParse()
        {
            var typeId = TypeId.NewTypeId("prefix");
            Assert.IsTrue(TypeId.TryParse(typeId.ToString(), out var typeId2));
            Assert.AreEqual(typeId, typeId2, "Parsing");
        }

        [TestMethod]
        public void TryParseFail()
        {
            // Missing prefix
            Assert.IsFalse(TypeId.TryParse(string.Empty, out var typeId2));
            Assert.AreEqual(default, typeId2, "Missing prefix");

            // Missing suffix
            Assert.IsFalse(TypeId.TryParse("prefix1", out typeId2));
            Assert.AreEqual(default, typeId2, "Missing suffix");

            // Invalid prefix
            Assert.IsFalse(TypeId.TryParse("prefix1_", out typeId2));
            Assert.AreEqual(default, typeId2, "Invalid prefix");

            // Invalid suffix
            Assert.IsFalse(TypeId.TryParse("prefix1_1234567890123456789012345", out typeId2));
            Assert.AreEqual(default, typeId2, "Invalid suffix");

            // Invalid prefix and suffix
            Assert.IsFalse(TypeId.TryParse("prefix__xW2NU3TAO4KE7AR7CETWTJAUGI", out typeId2), "Invalid prefix and suffix");
            Assert.AreEqual(default, typeId2, "Invalid prefix and suffix");
        }

        [TestMethod]
        public void TryParseFromString()
        {
            const string typeIdExample = "prefix_DW2NU3TAO4KE7AR7CETWTJAUGI";
            Assert.IsTrue(TypeId.TryParse(typeIdExample, out var typeId2), "Parsing example");
            Assert.IsTrue(TypeId.IsValidPrefix(typeId2.Type), "Validate prefix");
            Assert.IsTrue(TypeId.IsValidSuffix(typeId2.Id), "Validate suffix");
        }
    }
}