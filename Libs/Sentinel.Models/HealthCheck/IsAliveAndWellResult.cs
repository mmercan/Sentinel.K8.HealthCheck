using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

    public class IsAliveAndWellResultTimeSeries
    {

        [Key]
        public string Id { get; set; } = default!;
        public IsAliveAndWellResultTimeSeriesMetadata Metadata { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsSuccessStatusCode { get; set; } = default!;

        public DateTime CheckedAt { get; set; } = default!;
    }


    public class IsAliveAndWellResultTimeSeriesMetadata
    {
        public string Namespace { get; set; } = default!;
        public string Service { get; set; } = default!;
        public string CheckedUrl { get; set; } = default!;

    }
}