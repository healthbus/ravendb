﻿namespace Raven.Studio.Database
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows;
	using Caliburn.Micro;
	using Framework;
	using Newtonsoft.Json.Linq;
	using Raven.Database;

	public class DocumentViewModel : PropertyChangedBase
	{
		public const int SummaryLength = 150;

		readonly IDictionary<string, JToken> data;
		readonly IDictionary<string, JToken> metadata;

		public DocumentViewModel(JsonDocument document, DocumentTemplateProvider templateProvider)
		{
			data = new Dictionary<string, JToken>();
			metadata = new Dictionary<string, JToken>();

			JsonData = PrepareRawJsonString(document.DataAsJson);
			//JsonMetadata = PrepareRawJsonString(document.Metadata);

			Id = document.Key;
			//data = ParseJsonToDictionary(document.DataAsJson);
			metadata = ParseJsonToDictionary(document.Metadata);

			LastModified = metadata.IfPresent<DateTime>("Last-Modified");
			CollectionType = DetermineCollectionType();
			ClrType = metadata.IfPresent<string>("Raven-Clr-Type");

			templateProvider
				.GetTemplateFor(CollectionType ?? "default")
				.ContinueOnSuccess(x=>
				                   	{
				                   		DataTemplate = x.Result;
				                   		NotifyOfPropertyChange(() => DataTemplate);
				                   	});
		}

		string DetermineCollectionType()
		{
			return metadata.IfPresent<string>("Raven-Entity-Name") ?? (Id.StartsWith("Raven/") ? "System" : null) ?? "document";
		}

		public DataTemplate DataTemplate {get;private set;}
		public string ClrType {get; private set;}
		public string CollectionType {get; private set;}
		public DateTime LastModified {get; private set;}

		public string Id { get; private set; }
		public string DisplayId
		{
			get
			{
				var id = Id
					.Replace(CollectionType + "/",string.Empty)
					.Replace("Raven/",string.Empty);

				Guid guid;
				if(Guid.TryParse(id,out guid))
				{
					id = id.Substring(0,8);
				}
				return id;
			}
		}
		public string JsonData { get; private set; }

		public string JsonMetadata { get; private set; }

		public string Summary
		{
			get
			{
				return (JsonData.Length > SummaryLength
				       	? JsonData.Substring(0, SummaryLength) + "..."
				       	: JsonData)
						.Replace("\r", "").Replace("\n", " ");
			}
		}

		public IDictionary<string, JToken> Data
		{
			get { return data; }
		}

		public IDictionary<string, JToken> Metadata
		{
			get { return metadata; }
		}
		
		static IDictionary<string, JToken> ParseJsonToDictionary(JObject dataAsJson)
		{
			IDictionary<string, JToken> result = new Dictionary<string, JToken>();

			foreach (var d in dataAsJson)
			{
				result.Add(d.Key, d.Value);
			}

			return result;
		}

		static string PrepareRawJsonString(IEnumerable<KeyValuePair<string, JToken>> data)
		{
			var result = new StringBuilder("{\n");

			foreach (var item in data)
			{
				result.AppendFormat("\"{0}\" : {1},\n", item.Key, item.Value);
			}
			result.Append("}");

			return result.ToString();
		}

	}
}