/// Data structure. A group of string parameters prepared to be written to Kuju data files. 
/// Each DataBlock stands to one pair of parenthesys and can contain zero to many DataBlocks inside.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeData.Kuju_shape
{
    class DataBlock
    {
        private const string newLine = "\r\n";
        private const string tab = "	";

        public string Name { get; }
        public List<string> Values { get; }
        public List<DataBlock> Blocks { get; }
        public List<string> FinalValues { get; }

        public DataBlock( 
            string name,
            List<string> values,
            List<DataBlock> blocks = null,
            List<string> finalValues = null
            )
        {
            Name = name;

            Values = values;
            if (values == null)
                Values = new List<string>();

            Blocks = blocks;  
            if (blocks == null)
                Blocks = new List<DataBlock>();

            FinalValues = finalValues;  
            if (finalValues == null)
                FinalValues = new List<string>();
        }

        public void AddBlock(DataBlock block) => Blocks.Add(block);

        public void PrintBlock(StringBuilder sb, int tab = 0)
        {
            sb.Append(Tabs(tab) + Name + " (");

            if (Values != null & Values.Any())
                foreach (var v in Values)
                    sb.Append(' ' + v);

            if (Blocks != null && Blocks.Any())
            {
                sb.Append(newLine);

                foreach (var b in Blocks)
                    b.PrintBlock(sb, tab + 1);

                sb.Append(Tabs(tab) + ')');
            }
            else
                sb.Append(" )");


            if (FinalValues != null & FinalValues.Any())
                foreach (var fv in FinalValues)
                    sb.Append(' ' + fv);

            sb.Append(newLine);
        }

        private static string Tabs(int count)
        {
            var s = "";
            for (int i = 0; i < count; i++)
                s += tab;
            return s;
        }
    }
}
