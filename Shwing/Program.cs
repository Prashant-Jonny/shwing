using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp
{
    public class ShwingBuilder
    {
        public static ShwingNode OracleNode { get; set; }
        
        public ShwingBuilder(string path)
        {
            var filePath = path;
            var fileContents = File.ReadAllText(filePath);

            if (fileContents.Length <= 0)
                return;

            var tree = CSharpSyntaxTree.ParseText(fileContents);

            if (OracleNode == null)
                OracleNode = new ShwingNode(tree.GetRoot(), -1, 0, 1);
            else
                OracleNode.Add(tree.GetRoot(), -1, 0, 1);
        }


        
    }

  

    public class ShwingNode
    {
        static int S_UID = 0;
        public SyntaxKind Kind { get; private set; }
        public int Counter { get; set; } = 0;

        TextSpan _filename;

        public List<List<ShwingNode>> Children { get; private set; } = new List<List<ShwingNode>>();
        // int is a counter
        
        private ShwingNode() { } // NO.
        public int Id { get; private set; }
        private HashSet<int> _parentIds = new HashSet<int>();
        public ShwingNode(SyntaxNodeOrToken node, int precedingId, int currentDepth, int maxDepth)
        {
            Id = S_UID++;

            Kind            = node.Kind();
            _filename       = node.GetLocation().SourceSpan;

            Add(node, precedingId, currentDepth, maxDepth);
        }

        public void Add(SyntaxNodeOrToken newNode, int precedingId, int currentDepth, int maxDepth = int.MaxValue)
        {
            Counter += 1;
            _parentIds.Add(precedingId);

            if (currentDepth >= maxDepth)
                return;
            // TODO: Throw, or error check or something.
            Debug.Assert(newNode.Kind() == Kind, "Uh oh. Kinds must be the same at ShwingNodes.");

            var newChildren = newNode.ChildNodesAndTokens();
            ShwingNode lastTouchedNode = null;
            for (int i = 0; i < newChildren.Count; i++)
            {
                var newKid = newChildren[i];

                if (i == Children.Count)
                    Children.Add(new List<ShwingNode>());

                var j = Children[i].FindIndex(x => x.Kind == newKid.Kind());

                var lastId = lastTouchedNode?.Id ?? -2;

                if (j != -1)
                {
                    Children[i][j].Add(newKid, lastId, currentDepth + 1, maxDepth);
                    lastTouchedNode = Children[i][j];
                }
                else
                {
                    lastTouchedNode = new ShwingNode(newKid, lastId, currentDepth + 1, maxDepth);
                    Children[i].Add(lastTouchedNode);
                }
            }
        }

        
        public void Dump(int maxDepth, int indent = 0)
        {
            const int INDENT_MULTIPLIER = 4;

            if (indent > maxDepth)
                return;
            
            var pretext = new string(' ', indent * INDENT_MULTIPLIER);

            Console.Write($"[{string.Join(",", _parentIds)}]->{Id}:{Kind}({Counter})");

            for (int i = 0; i < Children.Count; i++)
            {
                for(int j = 0; j < Children[i].Count; j++)
                {
                    Children[i][j].Dump(maxDepth, indent + 1);
                }
                Console.WriteLine();
            }

        }

    }

    public class Shwing
    {
        int[] _data = null;

        public int[] RawData
        {
            get
            {
                return _data;
            }

        }

        private Shwing() {  } // NO.
        public Shwing(int[] data)
        {
            _data = data;
        }

        static void Main(string[] args)
        {
            var scanPath = string.Join("", args);
            foreach (var sourceFile in Directory.EnumerateFiles(scanPath, "*.cs", SearchOption.AllDirectories))
            {
                ShwingBuilder builder = new ShwingBuilder(sourceFile);
            }

            ShwingBuilder.OracleNode.Dump(1);
        }
    }
}
