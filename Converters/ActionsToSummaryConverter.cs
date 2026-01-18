using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using QC_Management.Models;
using System.Collections;
using System.Collections.Generic;

namespace QC_Management.Converters
{
    public class ActionsToSummaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<CorrectiveAction> actions)
            {
                var lines = actions
                    .OrderBy(a => a.CreatedAt)
                    .Select(a =>
                    {
                        var time = a.CreatedAt != default ? a.CreatedAt.ToString("dd/MM/yyyy HH:mm") : string.Empty;
                        var ownerPart = string.IsNullOrWhiteSpace(a.ActionOwner) ? string.Empty : $" (by {a.ActionOwner})";
                        var outcomePart = string.IsNullOrWhiteSpace(a.Outcome) ? string.Empty : $" - {a.Outcome}";
                        var desc = a.ActionDescription ?? string.Empty;
                        return string.Concat(time, string.IsNullOrEmpty(time) ? string.Empty : " - ", desc, ownerPart, outcomePart);
                    });

                return string.Join(Environment.NewLine, lines);
            }

            // also handle non-generic collections
            if (value is IEnumerable nonGeneric)
            {
                var list = new List<CorrectiveAction>();
                foreach (var o in nonGeneric)
                    if (o is CorrectiveAction ca) list.Add(ca);

                return Convert(list, targetType, parameter, culture);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}