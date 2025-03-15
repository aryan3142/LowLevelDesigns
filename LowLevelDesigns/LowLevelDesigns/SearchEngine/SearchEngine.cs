using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS
{
    #region Levenshtein Distance
    // Edit distance minimum number of moves to convert string1 into string2
    // Levenstein distance is the minimum number of single-character edits (insertions, deletions or substitutions) required to change one word into the other.
    // Can be used in situations where you want to find the similarity between two strings and fuzzy matching.
    public class Levenshtein
    {
        public static int Compute(string s1, string s2)
        {
            int len1 = s1.Length, len2 = s2.Length;
            int[,] dp = new int[len1 + 1, len2 + 1];

            for(int i = 0; i <= len1; i++)
            {
                dp[i, 0] = i;
            }

            for(int j = 0; j <= len2; j++)
            {
                dp[0, j] = j;
            }

            for(int i = 1; i<= len1; i++)
            {
                for(int j = 1; j <= len2; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                    {
                        dp[i,j] = dp[i - 1,j - 1];
                    }
                    else
                    {
                        dp[i, j] = 1 + Math.Min(Math.Min(dp[i - 1, j], dp[i, j - 1]), dp[i - 1, j - 1]);
                    }
                    
                }
            }

            return dp[len1, len2];
        }
    }
    #endregion

    #region trie for fuzzy matching
    public class TrieeNode
    {
        public Dictionary<char, TrieeNode> Children = new Dictionary<char, TrieeNode>();
        public bool IsEndOfWord;
    }

    public class Triee
    {
        private TrieeNode root = new TrieeNode();

        public void Insert(string word)
        {
            var node = root;
            foreach(char c in word)
            {
                if (!node.Children.ContainsKey(c))
                {
                    node.Children[c] = new TrieeNode();
                }
                node = node.Children[c];
            }
            node.IsEndOfWord = true;
        }

        public List<string> SearchWithEditDistance(string word,int maxEdits)
        {
                List<string> results = new List<string>();
                DFS(root, "", word, maxEdits, results);
                return results;
        }

        private void DFS(TrieeNode node, string prefix,string target, int maxEdits, List<string> results)
        {
            if(Levenshtein.Compute(prefix,target) <= maxEdits && node.IsEndOfWord)
            {
                results.Add(prefix);
            }

            foreach(var ch in node.Children)
            {
                DFS(node, prefix + ch.Key, target, maxEdits, results);
            }
        }
    }

    #endregion

    #region BM25 Ranking
    public class BM25
    {
        private const double k1 = 1.5;
        private const double b = 0.75;
        private int totalDocs;
        private double avgDocLength;
        private Dictionary<int, int> docLengths;

        public BM25(int totalDocs, Dictionary<int, int> docLengths)
        {
            this.totalDocs = totalDocs;
            this.docLengths = docLengths;
            this.avgDocLength = docLengths.Values.Average();
        }

        public double Score(int docId, int termFrequency, int docFrequency)
        {
            double idf = Math.Log((totalDocs - docFrequency + 0.5) / (docFrequency + 0.5) + 1);
            double tf = (termFrequency * (k1 + 1)) / (termFrequency + k1 * (1 - b + b * (docLengths[docId] / avgDocLength)));
            return idf * tf;
        }
    }
    #endregion

    public class InvertedIndex
    {
        private ConcurrentDictionary<string, ConcurrentDictionary<int, int>> index = new ConcurrentDictionary<string, ConcurrentDictionary<int, int>>();
        private Triee trie = new Triee();
        // A readerwriterLock is used to synchronize access. It allows multiple readers or a sigle writer to access the resource
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        private Dictionary<int, int> docLengths = new Dictionary<int, int>();

        public void IndexDocument(int docId, string text)
        {
            string[] words = text.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            rwLock.EnterWriteLock();
            try
            {
                docLengths[docId] = words.Length;
                foreach(string word in words)
                {
                    if (!index.ContainsKey(word))
                    {
                        index[word] = new ConcurrentDictionary<int, int>();
                        trie.Insert(word);
                    }
                    index[word].AddOrUpdate(docId, 1, (key, oldValue) => oldValue + 1);
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public Dictionary<int,double> Search(string query, int maxEdits = 2)
        {
            rwLock.EnterReadLock();
            try
            {
                var matches = trie.SearchWithEditDistance(query, maxEdits);
                var results = new Dictionary<int, double>();
                BM25 bm25 = new BM25(docLengths.Count, docLengths);

                foreach(var match in matches)
                {
                    if (index.ContainsKey(match))
                    {
                        foreach(var kvp in index[match])
                        {
                            double score = bm25.Score(kvp.Key, kvp.Value, index[match].Count);
                            results[kvp.Key] = results.ContainsKey(kvp.Key) ? results[kvp.Key] + score : score;
                        }
                    }
                }

                return results.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            }
            finally
            {
                rwLock.ExitReadLock();
            }

        }
    }
    public class SearchEngine
    {
        private InvertedIndex index = new InvertedIndex();

        public void IndexDocument(int docId, string text)
        {
            index.IndexDocument(docId, text);
        }

        public Dictionary<int, double> Search(string query, int maxEdits = 2)
        {
            return index.Search(query, maxEdits);
        }
    }

    #region Program Entry Point
    public class Program9
    {
        public static void Main()
        {
            SearchEngine engine = new SearchEngine();
            engine.IndexDocument(1, "The quick brown fox jumps over the lazy dog");
            engine.IndexDocument(2, "The fast brown fox leaps over the lazy hound");

            Console.WriteLine("Search results for 'quik':");
            var results = engine.Search("quik", 1);
            foreach (var result in results)
            {
                Console.WriteLine($"DocID: {result.Key}, Score: {result.Value}");
            }
        }
    }
    #endregion
}
