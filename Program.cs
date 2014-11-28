using System;
using System.Text;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.CoreTypeAttributes;
using ElasticsearchCRUD.Tracing;

namespace ConsoleElasticsearchTypeMappings
{	
	class Program
	{
		private static readonly IElasticsearchMappingResolver ElasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private const string ConnectionString = "http://localhost:9200";

		static void Main(string[] args)
		{
			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(ElasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.IndexCreate<AmazingThisMapping>();
			}

			Console.ReadLine();

			var data = new AmazingThisMapping
			{
				Cost = 3.4,
				Id = 1,
				Description = "Description",
				Name = "Name",
				Timestamp = DateTime.UtcNow,
				TimestampWithOffset = DateTime.Now,
				SmallAmount = 3,
				Data = "",
				DescriptionBothAnayzedAndNotAnalyzed = "This is a nice black cat",
				NumberOf = 67
			};

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(ElasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(data, data.Id);
				context.SaveChanges();
			}

			Console.ReadLine();

			using (var context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(ElasticsearchMappingResolver)))
			{
				context.TraceProvider = new ConsoleTraceProvider();

				// We expect a result here because we copied the value from Name to data
				var doc = context.Search<AmazingThisMapping>(BuildSearch("Name"));

				// We expect no result here because we copied no valuefrom Description to data
				var doc2 = context.Search<AmazingThisMapping>(BuildSearch("Description"));
				Console.WriteLine("Should be 1 Found:" + doc.PayloadResult.Count);
				Console.WriteLine("Should be 0 Found:" + doc2.PayloadResult.Count);
			}

			Console.ReadLine();
		}

		private static string BuildSearch(string dataText)
		{
			var sb = new StringBuilder();
			sb.Append("{ \"query\": {");
			sb.Append("\"bool\": {");
			sb.Append("\"must\": [");
			sb.Append("{");
			sb.Append("\"match\" : {");
			sb.Append(" \"data\" : \"" + dataText + "\"");
			sb.Append("}}]} }}");
			sb.Append("");
			return sb.ToString();
		}
	}

	public class AmazingThisMapping
	{
		public int Id { get; set; }

		[ElasticsearchInteger(Coerce=true)]
		public int NumberOf { get; set; }

		[ElasticsearchString(CopyTo = "data")]
		public string Name { get; set; }

		public string Description { get; set; }

		public string Data { get; set; }

		[ElasticsearchInteger]
		public short SmallAmount { get; set; }

		[ElasticsearchString(Boost = 1.4, Fields = typeof(FieldDataDefNotAnalyzed), Index = StringIndex.analyzed)]
		public string DescriptionBothAnayzedAndNotAnalyzed { get; set; }

		[ElasticsearchDouble(Boost = 2.0,Store=true)]
		public double Cost { get; set; }

		[ElasticsearchDate]
		public DateTime Timestamp { get; set; }

		[ElasticsearchDate]
		public DateTimeOffset TimestampWithOffset { get; set; }

	}

	public class FieldDataDefNotAnalyzed
	{
		[ElasticsearchString(Index = StringIndex.not_analyzed)]
		public string Raw { get; set; }
	}
}
