using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using QC_Management.Models;
using System.Collections;
using System.Collections.Generic;

namespace QC_Management.Converters
{
    public class CorrectiveActionsToSummaryConverter : IValueConverter
    {
        // value: IEnumerable<CorrectiveAction>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            IEnumerable<CorrectiveAction> actions = value switch
            {
                IEnumerable<CorrectiveAction> a => a,
                IEnumerable ie => ie.Cast<object>().OfType<CorrectiveAction>(),
                _ => Enumerable.Empty<CorrectiveAction>()
            };

            var lines = actions
                .OrderBy(a => a.CreatedAt)
                .Select(a =>
                {
                    var time = a.CreatedAt != default ? a.CreatedAt.ToString("dd/MM/yyyy HH:mm") : string.Empty;
                    var ownerPart = string.IsNullOrWhiteSpace(a.ActionOwner) ? string.Empty : $" (by {a.ActionOwner})";
                    var outcomePart = string.IsNullOrWhiteSpace(a.Outcome) ? string.Empty : $" - {a.Outcome}";
                    var desc = a.ActionDescription ?? string.Empty;
                    return string.Concat(
                        string.IsNullOrEmpty(time) ? string.Empty : time + " - ",
                        desc,
                        ownerPart,
                        outcomePart
                    );
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            return lines.Count == 0 ? string.Empty : string.Join(Environment.NewLine, lines);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}