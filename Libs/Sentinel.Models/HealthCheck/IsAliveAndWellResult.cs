using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sentinel.Models.K8sDTOs;

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

    public class IsAliveAndWellResultListWithHealthCheck
    {
        public IsAliveAndWellResultListWithHealthCheck()
        {
            // IsAliveAndWellResults = new List<IsAliveAndWellResult>();
        }
        public List<IsAliveAndWellResult> IsAliveAndWellResults { get; set; } = default!;
        public HealthCheckResourceV1 HealthCheck { get; set; } = default!;
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


        public static IsAliveAndWellResultTimeSerie FromIsAliveAndWellResult(HealthCheckResourceV1 healthcheck, IsAliveAndWellResult result)
        {
            IsAliveAndWellResultTimeSerie timeSerie = new IsAliveAndWellResultTimeSerie();
            timeSerie.Id = ObjectId.GenerateNewId();
            timeSerie.Metadata = new IsAliveAndWellResultTimeSerieMetadata();
            timeSerie.Metadata.Namespace = healthcheck.RelatedService?.Namespace;
            timeSerie.Metadata.ServiceName = healthcheck.RelatedService?.Name;
            timeSerie.Metadata.CheckedUrl = result.CheckedUrl;

            // timeSerie.ResultDetailId = result.ResultDetailId;
            timeSerie.Status = result.Status;
            timeSerie.IsSuccessStatusCode = result.IsSuccessStatusCode;
            timeSerie.CheckedAt = result.CheckedAt;

            if (result.CheckedAt == DateTime.MinValue)
            {
                timeSerie.CheckedAt = DateTime.UtcNow;
            }
            else
            {
                timeSerie.CheckedAt = result.CheckedAt.ToUniversalTime();
            }
            return timeSerie;

        }
    }


    public class IsAliveAndWellResultTimeSerieMetadata
    {
        public string Namespace { get; set; } = default!;
        public string ServiceName { get; set; } = default!;
        public string CheckedUrl { get; set; } = default!;

    }
}