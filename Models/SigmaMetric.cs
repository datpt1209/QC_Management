using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QC_Management.Models
{
    public class SigmaMetric
    {
        public int Id { get; set; }
        public int IdDevice { get; set; }
        public int IdTest { get; set; }
        public int? IdLevel { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double TEaPercent { get; set; }
        public double BiasPercent { get; set; }   // |mean bias| các đợt ngoại kiểm
        public int BiasN { get; set; }
        public double CvPercent { get; set; }
        public int CvN { get; set; }
        public double Sigma { get; set; }
        public double? Qgi { get; set; }
        public string? Grade { get; set; }
        public DateTime ComputedAt { get; set; }
        public string? ComputedBy { get; set; }
    }
}
