// FitNesse.NET
// Copyright � 2007,2008 Syterra Software Inc. This program is free software;
// you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System;
using System.Web;
using fitSharp.Fit.Model;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fit.Operators {
    public class CompareNumeric : CompareOperator<Cell> {
        private static readonly Type[] numericTypes = {
            typeof(byte), typeof(sbyte), typeof(decimal), typeof(double),
            typeof(float), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort)
        };

        private static readonly Comparison[] comparisons = {
            new Comparison(">=", 0, 1), new Comparison("<=", -1, 0), new Comparison("< ", -1, -1),
            new Comparison(">", 1, 1), new Comparison("<", -1, -1)
        };

        public bool TryCompare(Processor<Cell> processor, TypedValue instance, Tree<Cell> parameters, ref bool result) {
            if (parameters.Value.Text.StartsWith("<<")
                || FindComparison(parameters.Value.Text) == null
                || Array.IndexOf(numericTypes, instance.Type) < 0) return false;

            object actual = instance.Value;
            Comparison comparison = FindComparison(parameters.Value.Text);

            Parse cell = (Parse)parameters.Value;
            var rest = new CellSubstring(cell, comparison.Operator);
            object expected = processor.Parse(instance.Type, rest).Value;
            parameters.Value.SetBody(actual + comparison.Operator + rest.Body);

            int compare = actual is float || actual is double
                              ? (Convert.ToDouble(actual) < Convert.ToDouble(expected)
                                     ? -1
                                     : (Convert.ToDouble(actual) > Convert.ToDouble(expected) ? 1 : 0))
                              : decimal.Compare(Convert.ToDecimal(actual), Convert.ToDecimal(expected));
            result = comparison.MinCompare <= compare && compare <= comparison.MaxCompare;
            return true;
        }

        private static Comparison FindComparison(string theExpectedValue) {
            foreach (Comparison comparison in comparisons) {
                if (theExpectedValue.StartsWith(comparison.Operator)) return comparison;
            }
            return null;
        }

        private class Comparison {
            public Comparison(string theOperator, int theMinCompare, int theMaxCompare) {
                Operator = theOperator;
                MinCompare = theMinCompare;
                MaxCompare = theMaxCompare;
            }

            public readonly string Operator;
            public readonly int MinCompare;
            public readonly int MaxCompare;
        }
    }

    public class CellSubstring: Cell {
        private readonly Parse baseCell;
        private readonly int bodyStart;
        private readonly int textStart;

        public CellSubstring(Parse baseCell, string prefix) {
            this.baseCell = baseCell;
            textStart = prefix.Length;
            bodyStart = baseCell.Body.StartsWith(prefix) ? prefix.Length : HttpUtility.HtmlEncode(prefix).Length;
        }

        public void SetBody(string body) { baseCell.SetBody(baseCell.Body.Substring(0, bodyStart) + body); }
        public string Body { get { return baseCell.Body.Substring(bodyStart); } }
        public string Text { get { return baseCell.Text.Substring(textStart); } }
        public Parse ParseCell { get { return baseCell; } }
    }
}