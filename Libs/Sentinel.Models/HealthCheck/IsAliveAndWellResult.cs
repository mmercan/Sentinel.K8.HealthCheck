using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sentinel.Models.HealthCheck
{
    public class IsAliveAndWellResult
    {

        [Key]
        public string Id { get; set; } = default!;
        public string Result { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsSuccessStatusCode { get; set; } = default!;
        public string CheckedUrl { get; set; } = default!;
        public string Exception { get; set; } = default!;


        public DateTime CheckedAt { get; set; } = default!;
    }

    public class IsAliveAndWellResultTimeSerie
    {

        [BsonId]
        public ObjectId Id { get; set; } = default!;
        public IsAliveAndWellResultTimeSerieMetadata Metadata { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsSuccessStatusCode { get; set; } = default!;
        public string ResultDetailId { get; set; } = default!;
        public BsonDateTime CheckedAt { get; set; } = default!;
    }


    public class IsAliveAndWellResultTimeSerieMetadata
    {
        public string Namespace { get; set; } = default!;
        public string ServiceName { get; set; } = default!;
        public string CheckedUrl { get; set; } = default!;

    }
}